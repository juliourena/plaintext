function Get-GPOEnumeration {
<#
.SYNOPSIS
Enumerates GPOs and their applied scopes within the domain using PowerView, with additional capabilities to modify, link, or create GPOs.

.DESCRIPTION
This function uses PowerView to find all GPOs and determine where they are linked: Domain, Site, or OU. It lists GPOs along with their scope of application based on ACLs. Additionally, it can check permissions for modifying, linking, or creating GPOs.

.PARAMETER ModifyGPOs
If specified, the function will search for users with rights to modify GPOs.

.PARAMETER LinkGPOs
If specified, the function will search for users with rights to link GPOs to Sites, Domains, or OUs.

.PARAMETER CreateGPO
If specified, the function will search for non-admin users with rights to create GPOs.

.AUTHOR
Julio Ureña (@JulioUrena)

.EXAMPLE
Get-GPOEnumeration -LinkGPOs
# Searches for users with rights to link GPOs.

.EXAMPLE
Get-GPOEnumeration -CreateGPO
# Searches for non-admin users with rights to create GPOs.

.EXAMPLE
Get-GPOEnumeration -ModifyGPOs
# Searches for non-admin users with rights to modify GPOs.

.EXAMPLE
Get-GPOEnumeration
# Displays default enumeration of GPOs and their applied scopes.

.NOTES
This function requires PowerView to be loaded as it utilizes its cmdlets to query Active Directory objects.
#>
    [CmdletBinding()]
    Param(
        [switch]$ModifyGPOs,
        [switch]$LinkGPOs,
        [switch]$CreateGPO
    )

    # Check if PowerView module is imported
    Try {
        # Attempt to get the 'Get-Domain' command and stop if not found
        $null = Get-Command Get-Domain -ErrorAction Stop
    }
    Catch {
        # Error handling if PowerView is not loaded
        Write-Warning "PowerView is not imported in the current session. Ensure it is imported before continuing."
        Return
    }

    if ($LinkGPOs) {
		### Searching for users with rights to Link GPOs
		Write-Output "Searching for users with rights to Link GPOs to Sites, Domains, and OUs..."

		#### Search for rights to link GPOs to a Site
		$siteResults = @()
		$sites = Get-DomainSite -Properties distinguishedname
		foreach ($site in $sites) {
			$siteAcls = Get-DomainObjectAcl -SearchBase $site.distinguishedname -ResolveGUIDs |
						Where-Object {
							$_.ObjectAceType -eq "GP-Link" -and
							$_.ActiveDirectoryRights -match "WriteProperty"
						}
			foreach ($acl in $siteAcls) {
				$siteResults += [PSCustomObject]@{
					Level = "Site"
					ObjectDN = $acl.ObjectDN
					ResolvedSID = ConvertFrom-SID $acl.SecurityIdentifier
				}
			}
		}

		#### Search for rights to link GPOs to a Domain
		$domainAcls = Get-DomainObjectAcl -SearchScope Base -ResolveGUIDs |
					  Where-Object {
						  $_.ObjectAceType -eq "GP-Link" -and
						  $_.ActiveDirectoryRights -match "WriteProperty"
					  }
		$domainDN = (Get-DomainObject -SearchScope Base).distinguishedname
		$domainResults = @()
		foreach ($acl in $domainAcls) {
			$domainResults += [PSCustomObject]@{
				Level = "Domain"
				ObjectDN = $domainDN
				ResolvedSID = ConvertFrom-SID $acl.SecurityIdentifier
			}
		}

		#### Search for rights to link GPOs to an OU
		$ouResults = @()
		$ous = Get-DomainOU
		foreach ($ou in $ous) {
			$ouAcls = Get-DomainObjectAcl -Identity $ou.distinguishedname -ResolveGUIDs |
					  Where-Object {
						  $_.ObjectAceType -eq "GP-Link" -and
						  $_.ActiveDirectoryRights -match "WriteProperty"
					  }
			foreach ($acl in $ouAcls) {
				$ouResults += [PSCustomObject]@{
					Level = "OU"
					ObjectDN = $ou.distinguishedname
					ResolvedSID = ConvertFrom-SID $acl.SecurityIdentifier
				}
			}
		}

		# Combine all ACLs and output together to prevent space between outputs
		$combinedAcls = $siteResults + $domainResults + $ouResults
		$combinedAcls | Format-List
	}

    if ($CreateGPO) {
        ### Searching for non-admin users with rights to create GPOs
        Write-Output "Searching for non-admin users with rights to create GPOs..."
        $identity = (Get-DomainGPO).distinguishedname -replace 'CN=\{[A-F0-9-]+\},',''
        Get-DomainObjectACL -Identity $identity -ResolveGUIDs | 
        where { $_.ActiveDirectoryRights -contains "CreateChild" -and $_.SecurityIdentifier -match '^S-1-5-.*-[1-9]\d{3,}$' } | 
        foreach { ConvertFrom-SID $_.SecurityIdentifier }
    }

    if ($ModifyGPOs) {
		### Searching for users with rights to Modify GPOs
		Write-Output "Searching for users with rights to Modify GPOs..."
		$modifyGPOResults = @()
		#$gpos = Get-DomainGPO

		$modifyGPOResults = @()
		$gpoAcls = Get-DomainGPO | Get-DomainObjectAcl -ResolveGUIDs | 
			where { $_.ActiveDirectoryRights -match "CreateChild|WriteProperty|DeleteChild|DeleteTree|WriteDacl|WriteOwner" -and $_.SecurityIdentifier -match '^S-1-5-.*-[1-9]\d{3,}$' }
		
		foreach ($acl in $gpoAcls) {
			$principalName = (ConvertFrom-SID $acl.SecurityIdentifier)
			$modifyGPOResults += [PSCustomObject]@{
				GPOName           = (Get-DomainGPO -Identity ($acl.ObjectDN -replace '.*(\{[A-F0-9-]+\}).*', '$1')).displayname
				GPOGUID           = ($acl.ObjectDN -replace '.*(\{[A-F0-9-]+\}).*', '$1')
				PrincipalName     = $principalName
				ActiveDirectoryRights = $acl.ActiveDirectoryRights
			}
		}

		$modifyGPOResults | Format-List
    }

    if (-not ($ModifyGPOs -or $LinkGPOs -or $CreateGPO)) {
        ### Default behavior: Enumerate GPOs and their applied scopes
        Write-Output "Enumerating GPOs and their applied scopes..."
        $DomainObject = Get-DomainObject -SearchScope Base
        $DomainGPLink = $DomainObject.gplink

        $GPOAcls = Get-DomainGPO | Get-DomainObjectAcl | Where-Object {
            $_.ActiveDirectoryRights -match "CreateChild|WriteProperty|DeleteChild|DeleteTree|WriteDacl|WriteOwner" -and
            $_.SecurityIdentifier -match '^S-1-5-.*-[1-9]\d{3,}$'
        }

        $GPOAcls | ForEach-Object {
            $sid = $_.SecurityIdentifier
            $principal = Get-DomainObject -Identity $sid -ErrorAction SilentlyContinue
            $principalName = if ($principal) { $principal.Name } else { "Unknown or inaccessible" }
            $GPO = Get-DomainGPO -Identity $_.ObjectDN -ErrorAction SilentlyContinue

            $appliedScopes = @()
            $allSites = Get-DomainSite
            $allOUs = Get-DomainOU

            if ($DomainGPLink -like "*$($GPO.name)*") {
                $appliedScopes += "Domain: $($DomainObject.name)"
            }

            foreach ($site in $allSites) {
                if ($site.gplink -like "*$($GPO.name)*") {
                    $appliedScopes += "Site: $($site.Name)"
                }
            }

            foreach ($ou in $allOUs) {
                if ($ou.gplink -like "*$($GPO.name)*") {
                    $appliedScopes += "OU: $($ou.Name)"
                }
            }

            [PSCustomObject]@{
                PrincipalName = $principalName
                GPOName = $GPO.DisplayName
                GPCFileSysPath = $GPO.gpcfilesyspath
                AppliedScopes = ($appliedScopes -join ', ')
                ActiveDirectoryRights = $_.ActiveDirectoryRights
                ObjectDN = $_.ObjectDN
            } | Select-Object PrincipalName, GPOName, GPCFileSysPath, AppliedScopes, ActiveDirectoryRights, ObjectDN
        }
    }
}
