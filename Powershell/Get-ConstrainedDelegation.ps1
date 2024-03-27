function Get-ConstrainedDelegation {
    [CmdletBinding()]
    param (
        [string]$ComputerName,
        [switch]$CheckOrphaned
    )

    process {

    # Check if PowerView module is loaded
    if (-not (Get-Command Get-DomainComputer -ErrorAction SilentlyContinue)) {
        Write-Host "PowerView module is not loaded. Please load the module and try again." -ForegroundColor Red
        return
    }

    try {
        $outputCollection = @()
        $computers = if ($ComputerName) {
            @(Get-DomainComputer -Identity $ComputerName)
        } else {
            Get-DomainComputer -TrustedToAuth
        }

        foreach ($computer in $computers) {
            $delegationSPNs = $computer.'msds-allowedtodelegateto'
            if ($delegationSPNs) {
                $orphanedSPNs = @()
                foreach ($spn in $delegationSPNs) {
                    $target = $spn.Split('/')[1].Split('.')[0]
                    if ($CheckOrphaned) {
                        if (-not (Test-ComputerExistence -ComputerName $target)) {
                            $orphanedSPNs += $spn
                        }
                    } else {
                        $outputCollection += [PSCustomObject]@{
                            ComputerName = $computer.Name
                            TargetServer = $target
                            SPN = $spn
                        }
                    }
                }

                if ($CheckOrphaned -and $orphanedSPNs) {
                    foreach ($spn in $orphanedSPNs) {
                        $outputCollection += [PSCustomObject]@{
                            ComputerName = $computer.Name
                            TargetServer = ($spn.Split('/')[1].Split('.')[0])
                            SPN = $spn
                        }
                    }
                }
            }
        }

        $outputCollection | Format-Table -AutoSize
        } catch {
            Write-Error "An error occurred: $_"
        }
    }
}

function Test-ComputerExistence {
    param (
        [string]$ComputerName
    )
    # Replace the Get-DomainComputer cmdlet with the correct method to check computer existence in your environment
    $computer = Get-DomainComputer -Identity $ComputerName -ErrorAction SilentlyContinue
    return $null -ne $computer
}
