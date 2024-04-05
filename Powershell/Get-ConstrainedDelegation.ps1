function Get-ConstrainedDelegation {
<#
.SYNOPSIS
User PowerView to check for Orphaned SPNs.

Author: Julio Ureña (@JulioUrena)
License: 
Required Dependencies: Powerview.ps1
Optional Dependencies: None

.PARAMETER Command
 
.EXAMPLE
Get-ConstrainedDelegation -CheckOrphaned
#>
    [CmdletBinding()]
    param (
        [switch]$CheckOrphaned
    )

    process {
        $global:outputCollection = @()

        # Buscar computadoras y usuarios con delegación restringida
        $computerDelegations = Get-DomainComputer -TrustedToAuth
        $userDelegations = Get-DomainUser -TrustedToAuth

        # Procesar computadoras
        foreach ($computer in $computerDelegations) {
            ProcessDelegation -Object $computer -Type 'Computer'
        }

        # Procesar usuarios
        foreach ($user in $userDelegations) {
            ProcessDelegation -Object $user -Type 'User'
        }

        # Mostrar resultados
        if ($outputCollection.Count -gt 0) {
            $outputCollection | Format-Table -AutoSize
        } else {
            Write-Host "No se encontraron objetos con delegación restringida." -ForegroundColor Green
        }
    }
}

function ProcessDelegation {
    param (
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Type
    )

    $delegationSPNs = $Object.'msds-allowedtodelegateto'
    if ($delegationSPNs) {
        foreach ($spn in $delegationSPNs) {
            $target = $spn.Split('/')[1].Split('.')[0]

            if ($CheckOrphaned -and -not (Test-ObjectExistence -Name $target)) {
                $global:outputCollection += [PSCustomObject]@{
                    Type = $Type
                    Name = $Object.Name
                    Target = $target
                    SPN = $spn
                }
            } 

            if (!$CheckOrphaned){
                $global:outputCollection += [PSCustomObject]@{
                    Type = $Type
                    Name = $Object.Name
                    Target = $target
                    SPN = $spn
                }
            }

            
        }
    }
}

function Test-ObjectExistence {
    param ([string]$Name)
    $exists = Get-DomainObject -Identity $Name -ErrorAction SilentlyContinue
    return $null -ne $exists
}
