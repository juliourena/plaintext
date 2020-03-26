$UserName = 'zerotrustws\sqlservice'
$Service = 'SNMPTRAP' 
$ServerN = 'SRV-WEB-01'
$Password = 'Supersecure!'

$svcD=gwmi win32_service -computername $ServerN -filter "name='$service'"
$svcD | Invoke-WmiMethod -Name ChangeStartMode -ArgumentList "Automatic"
$svcD.StopService() 
$svcD.change($null,$null,$null,$null,$null,$null,$UserName,$Password,$null,$null,$null) 
$svcD.StartService()