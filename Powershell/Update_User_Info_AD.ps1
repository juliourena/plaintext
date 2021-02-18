# Powershell Script to automate user information on Active Directory from a CSV file. 

$users=import-csv c:\users.csv
foreach($user in $users)
{
    # Clear variables
    $sam = $null
    $managerSAM = $null
    $userselected = $null
    $manager = $null

    $sam = $user.SamAccountName
    # Write-Host "User: "$sam 

    $managerSAM = $user.Manager
    # Write-Host "Manager: " $managerSAM

	try {
		$userselected = Get-ADUser -Filter {SamAccountName -eq $sam}
        	# Write-Host "userselected: " $userselected

		$userselected.EmployeeID = $user.EmployeeID 
		$userselected.EmployeeNumber = $user.EmployeeNumber
		$userselected.GivenName = $user.GivenName
		$userselected.Surname = $user.Surname
		$userselected.Description = $user.Description
		$userselected.Title = $user.Title
		$userselected.Department = $user.Department
		$userselected.EmailAddress = $user.EmailAddress
		$userselected.OfficePhone = $user.OfficePhone
		$userselected.MobilePhone = $user.MobilePhone
		$userselected.Office = $user.Office
		$userselected.Company = $user.Company
        
        # Update User information
        Set-ADUser -Instance $userselected    

        # Check If there is any manager define
        if ($managerSAM)
        { 
            $manager = Get-ADUser -Filter {SamAccountName -eq $managerSAM}
            # Write-Host "Manager Identity: " $manager

            $managerSAN = $manager.SamAccountName
            Set-ADUser -Identity $userselected.SamAccountName -Manager $managerSAN
        }

        Write-Host "[+] User Updated Successfully " $userselected.Name
	}
	catch {
		Write-Host "[-] User not found: " $user.GivenName $user.Surname
        	Write-Host $_
	}
}
