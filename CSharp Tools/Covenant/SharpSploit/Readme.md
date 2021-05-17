### Clone repository and assign a name
cd /opt
sudo git clone--recurse-submodules https://github.com/cobbr/Covenant

### SharpSploit 
#### We'll use ShareSploit pth branch to include lateral movement for WMI & SMB using hashes. In order to keep SharpSploit version up to date we need to replace/add part of this code to some locations.

cd /opt/Covenant/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/LateralMovement/

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PassTheHash.cs

cd /opt/Covenant/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/Execution/

sudo mkdir PassTheHash && cd PassTheHash

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PassTheHash/WMIExec.cs

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/PassTheHash/SMBExec.cs

cd /opt/Covenant/Covenant/Data/ReferenceSourceLibraries/SharpSploit/SharpSploit/Misc/

sudo wget https://raw.githubusercontent.com/juliourena/plaintext/master/CSharp%20Tools/Covenant/SharpSploit/Utilities.cs

cd /opt/Covenant/Covenant/ 

sudo dotnet build 

sudo dotnet run
