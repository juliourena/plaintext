### Clone repository and assign a name
```
cd /opt
sudo git clone --recurse-submodules https://github.com/cobbr/Covenant --brach dev Covenant-dev
```
### SharpSploit 
#### We'll use SharpSploit pth branch to include lateral movement for WMI & SMB using hashes. In order to keep SharpSploit version up to date we need to replace/add part of this code to some locations.

#### SharpSploit's lastest Mimikatz update has some random errors, so I used the old classes and create a Task that consume the old method to run Mimikatz. (MimikatzOld.cs / PEOld.cs)

```
cd /opt/Covenant-dev/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/LateralMovement/

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PassTheHash.cs

cd /opt/Covenant-dev/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/Execution/

sudo mkdir PassTheHash && cd PassTheHash

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PassTheHash/WMIExec.cs

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PassTheHash/SMBExec.cs

cd /opt/Covenant-dev/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/Misc/

sudo rm Utilities.cs

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/Utilities.cs

cd /opt/Covenant-dev/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/Execution/

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PEOld.cs

cd /opt/Covenant-dev/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/Credentials/

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/MimikatzOld.cs

cd /opt/Covenant-dev/Covenant/ 

sudo dotnet build 

sudo dotnet run
```
