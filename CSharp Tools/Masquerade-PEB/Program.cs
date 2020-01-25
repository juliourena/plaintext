using System;
using System.Diagnostics;
using static modifypeb.structs;
using System.Runtime.InteropServices;

/*
This code is based on @FuzzSec https://twitter.com/FuzzySec work Masquerade-PEB.ps1
https://github.com/FuzzySecurity/PowerShell-Suite/blob/master/Masquerade-PEB.ps1
*/ 

namespace modifypeb
{
    class Program
    {
        public static void Emit_UNICODE_STRING(IntPtr hProcess, IntPtr lpBaseAddress, UInt32 dwSize, string data)
        {
            // Set access protections -> PAGE_EXEcUTE_READWRITE
            UInt32 lpfOldProtect = 0;
            bool CallResult = false;
            CallResult = Kernel32.VirtualProtectEx(hProcess, lpBaseAddress, dwSize, 0x40, ref lpfOldProtect);

            //Create replacement struct
            UNICODE_STRING UnicodeObject = new UNICODE_STRING();
            string UnicodeObject_Buffer = data;
            UnicodeObject.Length = (ushort)(UnicodeObject_Buffer.Length * 2);
            UnicodeObject.MaximumLength = (ushort)(UnicodeObject.Length + 1);
            UnicodeObject.Buffer = Marshal.StringToHGlobalUni(UnicodeObject_Buffer);
            IntPtr InMemoryStruct = new IntPtr();
            InMemoryStruct = Marshal.AllocHGlobal((int)dwSize);
            Marshal.StructureToPtr(UnicodeObject, InMemoryStruct, true);

            //Console.WriteLine("[>] Overwriting Current Process Information");

            //Overwrite PEB UNICODE_STRING struct
            UInt32 lpNumberOfBytesWritten = 0;
            CallResult = Kernel32.WriteProcessMemory(hProcess, lpBaseAddress, InMemoryStruct, dwSize, ref lpNumberOfBytesWritten);

            //Free InMemoryStruct
            Marshal.FreeHGlobal(InMemoryStruct);
        }

        static void Main(string[] args)
        {
            bool x32Architecture = false;
            string processName = Process.GetCurrentProcess().ProcessName;

            if (IntPtr.Size == 4)
            {
                x32Architecture = true;
                Console.WriteLine("[+] Current Process is 32 bits");
            }
            else
            {
                Console.WriteLine("[+] Current Process is 64 bits");
            }

            Console.WriteLine("[+] Getting Process Information");

            IntPtr ProcHandle = Process.GetCurrentProcess().Handle;
            _PROCESS_BASIC_INFORMATION PROCESS_BASIC_INFORMATION = new _PROCESS_BASIC_INFORMATION();
            int PROCESS_BASIC_INFORMATION_Size = System.Runtime.InteropServices.Marshal.SizeOf(PROCESS_BASIC_INFORMATION);
            Int32 returnLength = new Int32();

            int CallResult = Ntdll.NtQueryInformationProcess(ProcHandle, 0, ref PROCESS_BASIC_INFORMATION, PROCESS_BASIC_INFORMATION_Size, ref returnLength);

            Console.WriteLine("[+] PID " + PROCESS_BASIC_INFORMATION.UniqueProcessId.ToString());
            Console.WriteLine("[+] Started Value of GetCurrentProcess().MainModule.FileName ");
            Console.WriteLine("   [>] " + Process.GetCurrentProcess().MainModule.FileName);

            if (x32Architecture)
                Console.WriteLine("[+] PebBaseAddress: 0x{0:X8}",PROCESS_BASIC_INFORMATION.PebBaseAddress.ToInt32());
            else
                Console.WriteLine("[+] PebBaseAddress: 0x{0:X16}",PROCESS_BASIC_INFORMATION.PebBaseAddress.ToInt64());

            _PEB _PEB = new _PEB();

            long BufferOffset = PROCESS_BASIC_INFORMATION.PebBaseAddress.ToInt64();

            IntPtr newIntPtr = new IntPtr(BufferOffset);

            //check
            // found this somewhere in internet https://social.msdn.microsoft.com/Forums/sqlserver/en-US/64439444-c889-4d4b-a89f-b8f7e0e25827/problems-in-marshalptrtostructure?forum=clr
            _PEB PEBFlags = new _PEB();
            PEBFlags = (_PEB)Marshal.PtrToStructure(newIntPtr, typeof(_PEB));

            // FastPebLock http://www.debugwin.com/home/fastpeblock
            // This is recommended but not mandatory, just tested and works without the lock. 

            if (x32Architecture)
                Ntdll.RtlEnterCriticalSection(PEBFlags.FastPebLock32);
            else
                Ntdll.RtlEnterCriticalSection(PEBFlags.FastPebLock64);

            Console.WriteLine("[+] Setting Lock to work with the PEB");
            Console.WriteLine("[!] RtlEnterCriticalSection --> &Peb->FastPebLock");


            long ImagePathName = 0;
            long CommandLine = 0;
            UInt32 StructSize = 0;

            if (x32Architecture)
            {
                long PROCESS_PARAMETERS = PEBFlags.ProcessParameters32.ToInt64();
                StructSize = 8;

                ImagePathName = PROCESS_PARAMETERS + 0x38;
                CommandLine = PROCESS_PARAMETERS + 0x40;
            }
            else
            {
                long PROCESS_PARAMETERS = PEBFlags.ProcessParameters64.ToInt64();
                StructSize = 16;

                ImagePathName = PROCESS_PARAMETERS + 0x60;
                CommandLine = PROCESS_PARAMETERS + 0x70;
            }

            IntPtr ImagePathNamePtr = new IntPtr(ImagePathName);
            IntPtr CommandLinePtr = new IntPtr(CommandLine);

            if (x32Architecture)
            {
                Console.WriteLine("[+] Getting the PEB ProcessParameters.ImagePathName Address: 0x{0:X8}",ImagePathName);
                Console.WriteLine("[+] Getting the PEB ProcessParameters.CommandLine Address: 0x{0:X8}",CommandLine);
            }
            else
            {
                Console.WriteLine("[+] Getting the PEB ProcessParameters.ImagePathName Address: 0x{0:X16}", ImagePathName);
                Console.WriteLine("[+] Getting the PEB ProcessParameters.CommandLine Address: 0x{0:X16}", CommandLine);
            }

            string BinPath = @"C:\Windows\System32\notepad.exe";

            Emit_UNICODE_STRING(ProcHandle, ImagePathNamePtr, StructSize, BinPath);
            Emit_UNICODE_STRING(ProcHandle, CommandLinePtr, StructSize, BinPath);

            Console.WriteLine("[+] Printing GetCurrentProcess().MainModule.FileName value after ImagePathName & CommandLine modification ");
            Console.WriteLine("   [>] " + Process.GetCurrentProcess().MainModule.FileName);


            //&Peb->Ldr
            _PEB_LDR_DATA _PEB_LDR_DATA = new _PEB_LDR_DATA();
            var typePEB_LDR_DATA = _PEB_LDR_DATA.GetType();

            //reusing variables keep in mind
            if (x32Architecture)
                BufferOffset = PEBFlags.Ldr32.ToInt64();
            else
                BufferOffset = PEBFlags.Ldr64.ToInt64();

            //reusing variables keep in mind
            newIntPtr = new IntPtr(BufferOffset);
            _PEB_LDR_DATA LDRFlags = new _PEB_LDR_DATA();
            LDRFlags = (_PEB_LDR_DATA)Marshal.PtrToStructure(newIntPtr, typeof(_PEB_LDR_DATA));

            //&Peb->Ldr->InLoadOrderModuleList->Flink
            _LDR_DATA_TABLE_ENTRY _LDR_DATA_TABLE_ENTRY = new _LDR_DATA_TABLE_ENTRY();
            BufferOffset = LDRFlags.InLoadOrderModuleList.Flink.ToInt64();
            newIntPtr = new IntPtr(BufferOffset);

            // For next session you should read: https://www.osronline.com/article.cfm%5Earticle=499.htm 

            Console.WriteLine("[?] Traversing &Peb->Ldr->InLoadOrderModuleList doubly linked list");

            IntPtr ListIndex = new IntPtr();
            _LDR_DATA_TABLE_ENTRY LDREntry = new _LDR_DATA_TABLE_ENTRY();

            long FullDllName = 0;
            long BaseDllName = 0;

            while (ListIndex != LDRFlags.InLoadOrderModuleList.Blink)
            {
                LDREntry = (_LDR_DATA_TABLE_ENTRY)Marshal.PtrToStructure(newIntPtr, typeof(_LDR_DATA_TABLE_ENTRY));

                if (Marshal.PtrToStringUni(LDREntry.FullDllName.Buffer).Contains(processName))
                {
                    if (x32Architecture)
                    {
                        StructSize = 8;

                        FullDllName = BufferOffset + 0x24;
                        BaseDllName = BufferOffset + 0x2C;
                    }
                    else
                    {
                        StructSize = 16;

                        FullDllName = BufferOffset + 0x48;
                        BaseDllName = BufferOffset + 0x58;
                    }

                    // Overwrite _LDR_DATA_TABLE_ENTRY struct
                    // Can easily be extended to other UNICODE_STRING structs in _LDR_DATA_TABLE_ENTRY(/or in general)
                    IntPtr FullDllNamePtr = new IntPtr(FullDllName);
                    IntPtr BaseDllNamePtr = new IntPtr(BaseDllName);

                    if (x32Architecture)
                    {
                        Console.WriteLine("[>] Overwriting _LDR_DATA_TABLE_ENTRY.FullDllName: 0x{0:X8}", FullDllName);
                        Console.WriteLine("[>] Overwriting _LDR_DATA_TABLE_ENTRY.BaseDllName: 0x{0:X8}", BaseDllName);
                    }
                    else
                    {
                        Console.WriteLine("[>] Overwriting _LDR_DATA_TABLE_ENTRY.FullDllName: 0x{0:X16}", FullDllName);
                        Console.WriteLine("[>] Overwriting _LDR_DATA_TABLE_ENTRY.BaseDllName: 0x{0:X16}", BaseDllName);
                    }

                    Emit_UNICODE_STRING(ProcHandle, FullDllNamePtr, StructSize, BinPath);
                    Emit_UNICODE_STRING(ProcHandle, BaseDllNamePtr, StructSize, BinPath);
                }

                BufferOffset = LDREntry.InLoadOrderLinks.Flink.ToInt64();
                ListIndex = (IntPtr)BufferOffset;
                newIntPtr = new IntPtr(BufferOffset);
            }
            

            // Release ownership of PEB

            if (x32Architecture)
                Ntdll.RtlLeaveCriticalSection(PEBFlags.FastPebLock32);
            else
                Ntdll.RtlLeaveCriticalSection(PEBFlags.FastPebLock64);

            Console.WriteLine("[+] Releasing the PEB Lock");
            Console.WriteLine("[!] RtlLeaveCriticalSection --> &Peb->FastPebLock");

            Console.WriteLine("[+] Printing GetCurrentProcess().MainModule.FileName value after FullDllName & BaseDllName modification ");
            Console.WriteLine("   [>] " + Process.GetCurrentProcess().MainModule.FileName);

            Console.ReadLine();
        }
    }
}
