#Example: Invoke-InternetTest -Duration 100 -Targets plaintext.do,google.com,amazon.com

function Invoke-InternetTest
{
<#
.SYNOPSIS
To messure the internet connection.

Only tested on Windows 10
Author: Julio Ureña (@JulioUrena)
License: 
Required Dependencies: None
Optional Dependencies: None

.PARAMETER Command
 
.EXAMPLE
Invoke-InternetTest -Duration 100 -Targets plaintext.do,google.com,amazon.com
#>

	param
	(
		[string[]] $Targets, 
        [int] $Duration = 10 
	)

	$len = 1

	while($len -le $Duration) 
	{	

        foreach ($target in $Targets)
        {
		    $date = Get-Date
		    try
		    {
			    $connectionTest = Test-Connection -ComputerName $target -Count 1 -ErrorAction Stop
			    $rp = $connectionTest.ResponseTime
                Start-Sleep -m 500
		    }
		    Catch
		    {
			    $rp = "connection timeout"
                Start-Sleep -m 500
		    }
		
		    #Print the response
		    "$target     $date     $rp"
            $len++
			Start-Sleep -m 300
        }
	} 
}

