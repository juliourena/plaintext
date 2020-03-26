(new-object System.Net.WebClient).DownloadFile('https://gist.githubusercontent.com/darkoperator/e229781fe44edac94bb8bcb027d786f7/raw/e3f478f8a9427421c137fd8d13d1c5546bd5429e/accounting.csv', "$env:temp\Accounting.csv")
(new-object System.Net.WebClient).DownloadFile('https://gist.githubusercontent.com/darkoperator/986058fbfe9fa18b537120b42762c82b/raw/d384504ee2d91ee28ef4834bbbc5fb9c3416d0e0/ops_acc.csv', "$env:temp\Ops.csv")
(new-object System.Net.WebClient).DownloadFile('https://gist.githubusercontent.com/darkoperator/1e7eff7a1aeb082ba241037cfa02bc2f/raw/3ec7f4a7d40fb96d44d6c832f7360c0f674afe7c/hr_acc.csv', "$env:temp\HR.csv")
(new-object System.Net.WebClient).DownloadFile('https://raw.githubusercontent.com/juliourena/plaintext/master/Labs/IntoTheBreach/it_acc.csv', "$env:temp\IT.csv")
(new-object System.Net.WebClient).DownloadFile('https://gist.githubusercontent.com/darkoperator/a3e31fa0d75550a820da6bea5d83c62e/raw/6e6b0b366151e0be5a8eeeec567e64e3c38a3171/marketing.csv', "$env:temp\Marketing.csv")
(new-object System.Net.WebClient).DownloadFile('https://gist.githubusercontent.com/darkoperator/caa3414620c80bbab62435593358c774/raw/a8da36006066f161f19f2fa82b675e9fb7bcd4f5/sales.csv', "$env:temp\Sales.csv")
(new-object System.Net.WebClient).DownloadFile('https://gist.githubusercontent.com/darkoperator/9f4b29a2e14542d593b24893ffeacbce/raw/da5c01f0ceba8ccd29917b63997f33d4f3e01371/support.csv', "$env:temp\Support.csv")

# Script created by Carlos Perez (darkoperator) modified by Julio Ureña

$DomainDN = ([adsi]'').distinguishedName
<#
.SYNOPSIS
    Imports a CSV from Fake Name Generator to create test AD User accounts.
.DESCRIPTION
    Imports a CSV from Fake Name Generator to create test AD User accounts. 
    It will create OUs per country under the OU specified. Bulk
   generated accounts from fakenamegenerator.com must have as fields:
   * GivenName
   * Surname
   * StreetAddress
   * City
   * Title
   * Username
   * Password
   * Country
   * TelephoneNumber
   * Occupation
.EXAMPLE
    C:\PS> Import-LabADUser -Path .\unique.csv -OU DemoUsers
#>
function Import-LabADUser
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true,
                   Position=0,
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   HelpMessage="Path to one or more locations.")]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty()]
        [string[]]
        $Path,

        [Parameter(Mandatory=$true,
                   position=1,
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   HelpMessage="Organizational Unit to save users.")]
        [String]
        [Alias('OU')]
        $OrganizationalUnit
    )
    
    begin {
        if (-not (Get-Module -Name 'ActiveDirectory')) {
            Write-Error -Message 'ActiveDirectory module is not present'
            return
        }
    }
    
    process {
       
        $data = 
        Import-Csv -Path $Path | select  @{Name="Name";Expression={$_.Surname + ", " + $_.GivenName}},
                @{Name="SamAccountName"; Expression={$_.Username}},
                @{Name="UserPrincipalName"; Expression={$_.Username +"@" + $forest}},
                @{Name="GivenName"; Expression={$_.GivenName}},
                @{Name="Surname"; Expression={$_.Surname}},
                @{Name="DisplayName"; Expression={$_.Surname + ", " + $_.GivenName}},
                @{Name="City"; Expression={$_.City}},
                @{Name="StreetAddress"; Expression={$_.StreetAddress}},
                @{Name="State"; Expression={$_.State}},
                @{Name="Country"; Expression={$_.Country}},
                @{Name="PostalCode"; Expression={$_.ZipCode}},
                @{Name="EmailAddress"; Expression={$_.Username +"@" + $forest}},
                @{Name="AccountPassword"; Expression={ (Convertto-SecureString -Force -AsPlainText $_.password)}},
                @{Name="OfficePhone"; Expression={$_.TelephoneNumber}},
                @{Name="Title"; Expression={$_.Occupation}},
                @{Name="Path"; Expression={$OrganizationalUnit}},
                @{Name="Enabled"; Expression={$true}},
                @{Name="PasswordNeverExpires"; Expression={$true}} | ForEach-Object -Process {
                    $_ | New-ADUser  
                }
    }    
    end {}
}

$Groups = @(
    'Sales',
    'Support',
    'IT',
    'Marketing',
    'Accounting',
    'Ops',
    'HR'
)

foreach($Group in $Groups) {
    Write-Host -Object "Creating $($Group) OU" -ForegroundColor Cyan
    New-ADOrganizationalUnit -Name $Group -Path "$DomainDN" -ProtectedFromAccidentalDeletion $false
    Write-Host -Object "Creating Computers OU for $($Group)" -ForegroundColor Cyan 
    New-ADOrganizationalUnit -Name 'Computers' -Path "OU=$($Group),$($DomainDN)" -ProtectedFromAccidentalDeletion $false
    Write-Host -Object "Creating Users OU for $($Group)" -ForegroundColor Cyan 
    New-ADOrganizationalUnit -Name 'Users' -Path "OU=$($Group),$($DomainDN)" -ProtectedFromAccidentalDeletion $false
    Import-LabADUser -Path "$($env:temp)\$($Group).csv" -OrganizationalUnit "OU=Users,OU=$($Group),$($DomainDN)"
    $NewGroup = New-ADGroup -Name $Group -GroupScope Global -GroupCategory Security -PassThru
    Get-ADUser -SearchBase "OU=Users,OU=$($Group),$($DomainDN)" -Filter * | foreach {Add-ADGroupMember $NewGroup -Members $_ }
}

Write-Host -Object "Creating Admin accounts for enumeration." 
$DsktpAdmins = New-ADGroup -Name 'DesktopAdmins' -SamAccountName 'DesktopAdmins' -GroupCategory Security -Description 'Desktop support personel' -GroupScope Global -PassThru
$SrvAdmis = New-ADGroup -Name 'SrvAdmins' -SamAccountName 'SrvAdmins' -GroupCategory Security -Description 'Server Administrators' -GroupScope Global -PassThru
$DaGroup = Get-ADGroup -Identity "Domain Admins"
Get-ADGroupMember it | Select -First 3 |foreach {Add-ADGroupMember $DaGroup -Members $_}
Get-ADGroupMember it | Select -Index 2,3,4 |foreach {Add-ADGroupMember $SrvAdmis -Members $_}
Get-ADGroupMember it | Select -Index 5,6,7,8 |foreach {Add-ADGroupMember $DsktpAdmins -Members $_}

$Admin2Remove = Get-ADGroupMember it | Select -Index 2
Remove-ADGroupMember -Members $Admin2Remove -Identity $DaGroup -Confirm:$false


# Lab GPO add Disable Defender Realtime Monitoring
Write-Host -Object "Creating Windows Defender Excusion GPO" -ForegroundColor Cyan
$Path = "C:\GPO"
$ID = Get-ChildItem -Path $Path
$GPO = New-GPO -Name 'Disable Defender Realtime Monitoring' -Comment 'Disable Defender Realtime Monitoring'
Import-GPO -TargetName $GPO.DisplayName -Path $Path -BackupId $ID.Name
Write-Host -Object "GPO with GUID $($GPO.Id) created." -ForegroundColor Green
Write-Host -Object "Linking Windows Defender GPO to Domain" -ForegroundColor Cyan
New-GPLink -Guid $GPO.Id -Target "$($DomainDN)" -LinkEnabled Yes