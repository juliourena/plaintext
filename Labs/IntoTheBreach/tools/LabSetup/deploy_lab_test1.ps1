$labName = 'IntoTheBreach'
$domain = 'zerotrustws.xyz'
$domainName = 'ZeroTrust'
$adminAcct = 'Administrator'
$adminPass = 'SecurePassword!'
$labsources = "C:\tools\LabSetup"

$DC='DC-01'
$SRVWEB='SRV-WEB-01'
$WS1='WS-01'
$WS2='WS-02'

New-LabDefinition -Name $labName -DefaultVirtualizationEngine Azure

# Deploying to West Europe for faster speed
$azureDefaultLocation = 'East US'
Add-LabAzureSubscription -DefaultLocationName $azureDefaultLocation

#Role Definition
$role = Get-LabMachineRoleDefinition -Role RootDC @{
    ForestFunctionalLevel = 'WinThreshold '
    DomainFunctionalLevel = 'WinThreshold '
    SiteName = $domainName
}
