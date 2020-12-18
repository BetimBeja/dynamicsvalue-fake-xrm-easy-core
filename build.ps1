param (
    [string]$targetFramework = "netcoreapp3.1"
 )

$localPackagesFolder = '../local-packages'
Write-Host "Checking if local packages folder '$($localPackagesFolder)' exists..."

$packagesFolderExists = Test-Path $localPackagesFolder -PathType Container

if(!($packagesFolderExists)) 
{
    New-Item $localPackagesFolder -ItemType Directory
}

dotnet restore -p:TargetFrameworks=$targetFramework
if(!($LASTEXITCODE -eq 0)) {
    throw "Error restoring packages"
}

dotnet build --configuration Debug --no-restore --framework $targetFramework
if(!($LASTEXITCODE -eq 0)) {
    throw "Error during build step"
}

dotnet test --configuration Debug --no-restore --framework $targetFramework --verbosity normal --collect:"XPlat code coverage" --settings tests/.runsettings --results-directory ./coverage
if(!($LASTEXITCODE -eq 0)) {
    throw "Error during test step"
}

Write-Host  "*** Succeeded :)  **** " -ForegroundColor Green
