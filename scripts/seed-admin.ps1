param(
    [string]$ProjectPath = "$PSScriptRoot\..\backend\src\PpvRecon.Api\PpvRecon.Api.csproj"
)

$ErrorActionPreference = "Stop"

dotnet run --project $ProjectPath -- seed-admin
