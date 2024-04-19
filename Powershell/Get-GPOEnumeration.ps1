function Get-GPOEnumeration {
<#
.SYNOPSIS
Enumerates GPOs and their applied scopes within the domain using PowerView.

.DESCRIPTION
This function uses PowerView to find all GPOs and determine where they are linked: Domain, Site, or OU.
It lists GPOs along with their scope of application based on ACLs, explicitly excluding Domain Admins and Enterprise Admins due to their extensive permissions.

.AUTHOR
Julio Ureña (@JulioUrena)

.EXAMPLE
Get-GPOEnumeration

.NOTES
This function requires PowerView to be loaded as it utilizes its cmdlets to query Active Directory objects.

#>
    [CmdletBinding()]
    Param()

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

    # Retrieve the domain object to get the gplink attribute
    $DomainObject = Get-DomainObject -SearchScope Base
    $DomainGPLink = $DomainObject.gplink

    # Get SID for Domain Admins and Enterprise Admins
    $domainAdminsSID = Get-DomainGroup "Domain Admins" -Properties objectSid | Select-Object -ExpandProperty objectSid
    $enterpriseAdminsSID = Get-DomainGroup "Enterprise Admins" -Properties objectSid | Select-Object -ExpandProperty objectSid

    # Define excluded SIDs
    $excludedSIDs = @(
        "S-1-5-18",   # Local System
        "S-1-5-9",    # Enterprise Domain Controllers
        "S-1-5-11",   # Authenticated Users
        $domainAdminsSID,  # Domain Admins
        $enterpriseAdminsSID   # Enterprise Admins
    )

    # Retrieve GPO ACLs excluding certain SIDs
    $GPOAcls = Get-DomainGPO | Get-DomainObjectAcl | Where-Object {
        $_.ActiveDirectoryRights -match "CreateChild|WriteProperty|DeleteChild|DeleteTree|WriteDacl|WriteOwner" -and
        -not ($excludedSIDs -contains $_.SecurityIdentifier) -and
        $_.SecurityIdentifier -match "S-1-5-21-\d+-\d+-\d+-\d+$"
    }

    $GPOAcls | ForEach-Object {
        $sid = $_.SecurityIdentifier
        $principal = Get-DomainObject -Identity $sid -ErrorAction SilentlyContinue
        $principalName = if ($principal) { $principal.Name } else { "Unknown or inaccessible" }
        $GPO = Get-DomainGPO -Identity $_.ObjectDN -ErrorAction SilentlyContinue

        # Determine where the GPO is linked: Domain, Site, or OU
        $appliedScopes = @()
        $allSites = Get-DomainSite
        $allOUs = Get-DomainOU

        # Check if linked at the Domain level
        if ($DomainGPLink -like "*$($GPO.name)*") {
            $appliedScopes += "Domain: $($DomainObject.name)"
        }

        # Check if linked at Site level
        foreach ($site in $allSites) {
            if ($site.gplink -like "*$($GPO.name)*") {
                $appliedScopes += "Site: $($site.Name)"
            }
        }

        # Check if linked at OU level
        foreach ($ou in $allOUs) {
            if ($ou.gplink -like "*$($GPO.name)*") {
                $appliedScopes += "OU: $($ou.Name)"
            }
        }

        # Output the findings
        [PSCustomObject]@{
            PrincipalName = $principalName
            GPOName = $GPO.DisplayName
            GPCFileSysPath = $GPO.gpcfilesyspath
            AppliedScopes = ($appliedScopes -join ', ')
            ActiveDirectoryRights = $_.ActiveDirectoryRights
            ObjectDN = $_.ObjectDN
            AceType = $_.AceType
            IsInherited = $_.IsInherited
            InheritanceFlags = $_.InheritanceFlags
            ObjectAceType = $_.ObjectAceType
        } | Select-Object PrincipalName, GPOName, GPCFileSysPath, AppliedScopes, ActiveDirectoryRights, ObjectDN, AceType, IsInherited, InheritanceFlags, ObjectAceType
    }
}
