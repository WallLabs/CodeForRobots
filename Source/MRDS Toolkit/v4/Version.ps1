Write-Output "Version"
Write-Output "======="
Write-Output "Increments the version number stored in the Version.txt file,"
Write-Output "then applies it to all relevant source files in the solution."
Write-Output "Build is set to the UTC year and month in ""yyMM"" format."
Write-Output "Revision is set to the UTC day * 1000 plus a three digit incrementing number." 
Write-Output ""

trap
{
    Write-Error $_
    exit 1
}

function Update-Version ([Version]$Version)
{
    $date = (Get-Date).ToUniversalTime()
    $newBuild = $date.ToString("yyMM")
    $dayRevisionMin = $date.Day * 1000
    if (($Version.Build -lt $newBuild) -or ($Version.Revision -lt $dayRevisionMin)) { $newRevision = $dayRevisionMin + 1 } else { $newRevision = $Version.Revision + 1 }
    New-Object -TypeName System.Version -ArgumentList $Version.Major, $Version.Minor, $newBuild, $newRevision
}

function Get-VersionFile ([String]$File)
{
    Write-Host ("Reading version file " + $File)
    $versionString = [System.IO.File]::ReadAllText($File).Trim()
    New-Object -TypeName System.Version -ArgumentList $versionString
}

function Set-VersionFile ([String]$File, [Version]$Version)
{
    Write-Host ("Writing version file " + $File)
    [System.IO.File]::WriteAllText($File, $Version.ToString())
}

function Set-VersionInAssemblyInfo ([String]$File, [Version]$Version)
{
    Write-Host ("Setting version in assembly info file " + $File)
    $contents = [System.IO.File]::ReadAllText($File)
    $contents = [RegEx]::Replace($contents, "(AssemblyVersion\("")(?:\d+\.\d+\.\d+\.\d+)(""\))", ("`${1}" + $Version.ToString() + "`${2}"))
    $contents = [RegEx]::Replace($contents, "(AssemblyFileVersion\("")(?:\d+\.\d+\.\d+\.\d+)(""\))", ("`${1}" + $Version.ToString() + "`${2}"))
    [System.IO.File]::WriteAllText($File, $contents)
}

function Set-VersionInWixGlobal ([String]$File, [Version]$Version)
{
    Write-Host ("Setting version in WIX global file " + $File)
    $contents = [System.IO.File]::ReadAllText($File)
    $contents = [RegEx]::Replace($contents, "(\<\?define\s*ProductVersion\s*=\s*"")(?:\d+\.\d+\.\d+\.\d+)(""\s*\?\>)", ("`${1}" + $Version.ToString() + "`${2}"))
    [System.IO.File]::WriteAllText($File, $contents)
}

function Set-VersionInAssemblyReference ([String]$File, [String]$AssemblyName, [Version]$Version)
{
    Write-Host ("Setting version in assembly references of " + $File)
    $contents = [System.IO.File]::ReadAllText($File)
    $contents = [RegEx]::Replace($contents, "(["">](?:\S+,\s+){0,1}" + $AssemblyName + ",\s+Version=)(?:\d+\.\d+\.\d+\.\d+)([,""<])", ("`${1}" + $Version.ToString() + "`${2}"))
    [System.IO.File]::WriteAllText($File, $contents)
}

function Set-VersionInBindingRedirect ([String]$File, [String]$AssemblyName, [Version]$Version)
{
    Write-Host ("Setting version in binding redirects of " + $File)
    $contents = [System.IO.File]::ReadAllText($File)
    $oldVersionMax = New-Object -TypeName "System.Version" -ArgumentList $Version.Major, $Version.Minor, $Version.Build, ($Version.Revision - 1)
    $pattern = "(<dependentAssembly>[\s\S]*?<assemblyIdentity\s+name=""" + $AssemblyName + """[\s\S]+?/>[\s\S]*?<bindingRedirect\s+oldVersion=""\d+\.\d+\.\d+\.\d+-)(?:\d+\.\d+\.\d+\.\d+)(""\s+newVersion="")(?:\d+\.\d+\.\d+\.\d+)(""[\s\S]*?/>)"
    $contents = [RegEx]::Replace($contents, $pattern, ("`${1}" + $oldVersionMax.ToString() + "`${2}" + $Version.ToString() + "`${3}"))
    [System.IO.File]::WriteAllText($File, $contents)
}

$scriptDirectory =  [System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Definition)

$versionFilePath = $scriptDirectory + "\Version.txt"
$version = Get-VersionFile -File $versionFilePath
Write-Host ("Old Version: " + $version.ToString())

$newVersion = Update-Version -Version $version
Write-Host ("New Version: " + $newVersion.ToString())
Set-VersionFile -File $versionFilePath -Version $newVersion

Set-VersionInAssemblyInfo -File ($scriptDirectory + "\MrdsToolkit.Windows.ServiceHost\Properties\AssemblyInfo.cs") -Version $newVersion
Set-VersionInAssemblyInfo -File ($scriptDirectory + "\MrdsToolkit.Windows.Services\Properties\AssemblyInfo.cs") -Version $newVersion
Set-VersionInWixGlobal -File ($scriptDirectory + "\MrdsToolkit.Windows.Setup.X64\Global.wxi") -Version $newVersion

exit 0
