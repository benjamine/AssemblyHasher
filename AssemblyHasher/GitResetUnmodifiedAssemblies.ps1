Param(
    [string]$path = ".",
    [string]$filter = "*.dll"
)
$script:startTime = get-date

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

if (!(Test-Path -PathType Container $path)) {
    Throw "Path $path not found"
}

if (Test-Path "$scriptPath\bin\Release\AssemblyHasher.exe") {
    $assemblyHasherPath = (Resolve-Path "$scriptPath\bin\Release\AssemblyHasher.exe").Path
} elseif (Test-Path "$scriptPath\AssemblyHasher.exe") {
    $assemblyHasherPath = (Resolve-Path "$scriptPath\AssemblyHasher.exe").Path
}
Get-Childitem -recurse -path $path -filter $filter | % {
    $filePath = $_.FullName
    $fileRelativePath = Resolve-Path -Relative $filePath
    Write-Host -NoNewline "$fileRelativePath -> "
    $hash = Invoke-Expression "$assemblyHasherPath --ignore-versions $filePath"
    "$hash"
    $hashFilePath = $filePath + ".hash"
    if (Test-Path $hashFilePath) {
        $currentHash = Get-Content $hashFilePath
        if ($currentHash -eq $hash) {
            try {
                if (git diff --name-only $fileRelativePath) {
                    "unchanged, resetting file"
                    git checkout -- $fileRelativePath
                } else {
                    "unchanged"
                }
            } catch {
                "error tying to reset $filePath"
            }
            try {
                $filePdbRelativePath = $fileRelativePath.Substring(0, $fileRelativePath.Length-4) + ".pdb"
                if (Test-Path $filePdbRelativePath) {
                    if (git diff --name-only $filePdbRelativePath) {
                        git checkout -- $filePdbRelativePath
                    }
                }
            } catch {
                "error tying to reset $filePdbRelativePath"
            }
        } else {
            "changed, updating hash"
            "$hash" > $hashFilePath
        }
    } else {
        "new, saving hash"
        "$hash" > $hashFilePath
    }
}

""
$elapsedTime = $(get-date) - $script:StartTime
"Hashing Complete."
"Total Elapsed Time: $elapsedTime"
""