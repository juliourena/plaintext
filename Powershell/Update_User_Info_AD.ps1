# Powershell Script to automate user information on Active Directory from a CSV file. 

$users=import-csv c:\users.csv
foreach($user in $users)
{
    # Clear variables
    $email = $null
    $managerEmail = $null
    $userselected = $null
    $manager = $null

    $email = $user.EmailAddress
    # Write-Host "User: "$email 

    $managerEmail = $user.Manager
    # Write-Host "Manager: " $managerEmail

	try {
		$userselected = Get-ADUser -Filter {EmailAddress -eq $email}
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
        if ($managerEmail)
        { 
            $manager = Get-ADUser -Filter {EmailAddress -eq $managerEmail}
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
