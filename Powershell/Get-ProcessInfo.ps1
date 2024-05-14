function Get-ProcessInfo {
    [CmdletBinding()]
    Param (
        [Parameter(Mandatory=$true)]
        [int]$Port
    )

    # Get the TCP connection associated with the specified port
    $connectionInfo = Get-NetTCPConnection | Where-Object { $_.LocalPort -eq $Port }

    # Get process details using the process ID from the connection info
    $processInfo = $connectionInfo | ForEach-Object {
        $process = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
        $service = Get-WmiObject Win32_Service | Where-Object { $_.ProcessId -eq $process.Id }

        # Output object with details
        [PSCustomObject]@{
            LocalAddress = $_.LocalAddress
            LocalPort = $_.LocalPort
            ProcessName = $process.Name
            ProcessId = $process.Id
			ServiceDisplayName = $service.DisplayName
            ServiceName = $service.Name			
            ServiceDescription = $service.Description
        }
    }

    # Display the results
    $processInfo | Format-List

    # Check if any results were found
    if ($processInfo) {
        Write-Output "Process details found for port $Port."
    } else {
        Write-Output "No process found using port $Port. Check if the port number is correct or if the process is active."
    }
}
