@echo off
setlocal

set VERSION=0.1.0
set APP=onester
set OUTDIR=publish\%APP%-v%VERSION%
set ZIP=%APP%-v%VERSION%-win-x64.zip

echo Building %APP% v%VERSION%...

dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:DebugType=None /p:DebugSymbols=false -o "%OUTDIR%"
if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

if exist "%ZIP%" del "%ZIP%"

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%OUTDIR%\*' -DestinationPath '%ZIP%' -Force"
if errorlevel 1 (
    echo ZIP creation failed.
    exit /b 1
)

echo Done: %ZIP%
endlocal
