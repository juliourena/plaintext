
#Into The Breach Lab Deployment guide

This setup describe the process to automate the lab deployement.

This lab included:
	- Domain Controller (DC-01)
	- Web Server (SRV-WEB-01)
	- Workstations (2) (WS-01,WS-02)
	- Linux Ubuntu (Command & Control)

History: The idea of this lab is to simulate how an attacker from a phishing email can compromise a company using different tactics, techniques and procedures for initial access, lateral movement, credential stealling. 

# Install AutomatedLab

It's recommended to run this lab from a VM without any restrictions, if you can create a new VM may avoid some problems. Run powershell as Administrator and install the AutomatedLab package using the following commands:

```
powershell -exec bypass -nop 
Install-Module Az -AllowClobber -Force
Install-PackageProvider Nuget -Force
Install-Module AutomatedLab -AllowClobber -Force
```

Download the instalation package <link> to the directory C:\tools\LabSetup, from the powershell session run the following command:

```
Import-Module AutomatedLab
C:\tools\LabSetup\deploy_lab.ps1
```

The setups takes aproximately 1 hour, them you will need to install Office into WS-01 manually, this is to simulate the phishing comming from Outlook. 

# Tenant configuration:
From https://securitycenter.microsoft.com go to Settings > Advanced features and disable:
	- Allow or Block file
	- Automatically resolve alerts

# Command & Control Configuration with Covenant 

[Covenant](https://github.com/cobbr/Covenant) is a .NET command and control framework that aims to highlight the attack surface of .NET, make the use of offensive .NET tradecraft easier, and serve as a collaborative command and control platform for red teamers.



# Hack Them All Step by Steps

History:
	1. Initial Access - lewen get phished and the attacker get command and control over WS-01 PC.
	2. Enumeration - The attacker started enumerating and discover all workstations, users and group from the domain using PowerView.
	3. Privilege Escalation - Since lewen was local admin on his workstation the attacker used UAC Bypass technique to get a high integrity process (aka admin privileges).
	4. Credential Stealing - The attacker execute mimikatz to steal credentials from the local computer and also get the credentials from a recently logged user (rjames), with the previous enumeration identified that rjames was member of the DesktopAdmin group.
	5. Lateral Movement - He used his new gained credentials to compromise one of the discovered workstations (WS-02) using WMI.
	6. Credential Stealing - he repeated the process on the new workstation (WS-02) and get the credentials of kkreps, a developer.
	7. Lateral Movement - with kkreps credentials the attacker access a WebServer (SRV-WEB-01) using PSEXEC.
	8. Credential Stealing - within SRV-WEB-01 the attacker was able to get the hash of a sqladmin user which ended up being part of the Domain Admin group.
	9. DCSync - with the new obtained credentials the attacker execute DCSync to get all users hashes.  
