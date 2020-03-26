
$labName = 'IntoTheBreach'
$domain = 'zerotrustws.xyz'
$domainName = 'ZeroTrust'
$adminAcct = 'admin'
$adminPass = 'SecurePassword!'
$labsources = "C:\tools\LabSetup"

$DC='DC-01'
$SRVWEB='SRV-WEB-01'
$WS1='WS-01'
$WS2='WS-02'

New-LabDefinition -Name $labName -DefaultVirtualizationEngine Azure

# Set username and password 
Add-LabDomainDefinition -Name $domain -AdminUser $adminAcct -AdminPassword $adminPass
Set-LabInstallationCredential -Username $adminAcct -Password $adminPass


# Deploying to West Europe for faster speed
$azureDefaultLocation = 'East US'
Add-LabAzureSubscription -DefaultLocationName $azureDefaultLocation

#Role Definition
$role = Get-LabMachineRoleDefinition -Role RootDC @{
    ForestFunctionalLevel = 'WinThreshold'
    DomainFunctionalLevel = 'WinThreshold'
    SiteName = $domainName
	SiteSubnet = '192.168.2.0/24'
}

# Deploying DC and Client machine. (Server has less cost and less services by default)

Add-LabMachineDefinition -Name $DC -Memory 4gb -OperatingSystem 'Windows Server 2019 Datacenter (Desktop Experience)' -IpAddress '192.168.2.5' -Roles $role -DomainName $domain

Add-LabMachineDefinition -Name $SRVWEB -Memory 2gb -OperatingSystem 'Windows Server 2019 Datacenter (Desktop Experience)' -IpAddress '192.168.2.10' -DnsServer1 '192.168.2.5' -Roles WebServer -DomainName $domain -Network 'IntoTheBreach'

Add-LabMachineDefinition -Name $WS1 -Memory 2gb -OperatingSystem 'Windows 10 Enterprise' -IpAddress '192.168.2.101' -DnsServer1 '192.168.2.5' -DomainName $domain -Network 'IntoTheBreach'

Add-LabMachineDefinition -Name $WS2 -Memory 2gb -OperatingSystem 'Windows 10 Enterprise' -IpAddress '192.168.2.102' -DnsServer1 '192.168.2.5' -DomainName $domain -Network 'IntoTheBreach'

Install-Lab | Add-Content -Path ".\IntoTheBreach.log"

# Copy GPO
Copy-LabFileItem -Path C:\tools\LabSetup\GPO -ComputerName $DC -DestinationFolderPath C:\

# Configure fake accounts, OU, groups and GPOs
Invoke-LabCommand -FilePath C:\tools\LabSetup\LabSetup.ps1 -ComputerName $DC 

# Onboarding MDATP
# Send Onboarding package to machines 
Copy-LabFileItem -Path C:\tools\LabSetup\server-WindowsDefenderATPLocalOnboardingScript.cmd -ComputerName $DC -DestinationFolderPath C:\
Copy-LabFileItem -Path C:\tools\LabSetup\server-WindowsDefenderATPLocalOnboardingScript.cmd -ComputerName (Get-LabVM -ComputerName $SRVWEB) -DestinationFolderPath C:\
Copy-LabFileItem -Path C:\tools\LabSetup\WindowsDefenderATPLocalOnboardingScript.cmd -ComputerName (Get-LabVM -ComputerName $WS1) -DestinationFolderPath C:\
Copy-LabFileItem -Path C:\tools\LabSetup\WindowsDefenderATPLocalOnboardingScript.cmd -ComputerName $WS2 -DestinationFolderPath C:\

# Run Onboarding package 
Invoke-LabCommand -ScriptBlock { C:\server-WindowsDefenderATPLocalOnboardingScript.cmd } -ComputerName $DC
Invoke-LabCommand -ScriptBlock { C:\server-WindowsDefenderATPLocalOnboardingScript.cmd } -ComputerName $SRVWEB
Invoke-LabCommand -ScriptBlock { C:\WindowsDefenderATPLocalOnboardingScript.cmd } -ComputerName $WS1
Invoke-LabCommand -ScriptBlock { C:\WindowsDefenderATPLocalOnboardingScript.cmd } -ComputerName $WS2

# Local Admins
Invoke-LabCommand -ScriptBlock { Add-LocalGroupMember -Group 'Administrators' -Member ('SrvAdmins','kkreps') } -ComputerName $SRVWEB
Invoke-LabCommand -ScriptBlock { Add-LocalGroupMember -Group 'Administrators' -Member ('DesktopAdmins','lewen') } -ComputerName $WS1
Invoke-LabCommand -ScriptBlock { Add-LocalGroupMember -Group 'Administrators' -Member ('DesktopAdmins', 'kkreps') } -ComputerName $WS2

# Configure sqlservice account to run the service SNMPTraps
Invoke-LabCommand -FilePath C:\tools\LabSetup\SNMPTRAP.ps1 -ComputerName $SRVWEB

Show-LabDeploymentSummary -Detailed 

# Stop VM
$DcVm = Get-AzVM -ResourceGroupName $labName -name $DC
$SRVWEBVm = Get-AzVM -ResourceGroupName $labName -name $SRVWEB
$WS1Vm = Get-AzVM -ResourceGroupName $labName -name $WS1
$WS2Vm = Get-AzVM -ResourceGroupName $labName -name $WS2
$DcVm | Stop-AzVM -ResourceGroupName $labName -force
$SRVWEBVm | Stop-AzVM -ResourceGroupName $labName -force
$WS1Vm | Stop-AzVM -ResourceGroupName $labName -force
$WS2Vm | Stop-AzVM -ResourceGroupName $labName -force

# Resize VM to a lower cost image.
#$DcVm.HardwareProfile.VmSize = "Standard_B1ms"
$SRVWEBVm.HardwareProfile.VmSize = "Standard_B1ms"
$WS1Vm.HardwareProfile.VmSize = "Standard_B1ms"
$WS2Vm.HardwareProfile.VmSize = "Standard_B1ms"

#Update-AzVM -VM $DcVm -ResourceGroupName $labName -Verbose
Update-AzVM -VM $SRVWEBVm -ResourceGroupName $labName -Verbose
Update-AzVM -VM $WS1Vm -ResourceGroupName $labName -Verbose
Update-AzVM -VM $WS2Vm -ResourceGroupName $labName -Verbose