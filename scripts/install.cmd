#@echo off
setlocal

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -property installationPath`) do set VS_PATH=%%i

if "%VS_PATH%"=="" (
    echo ERROR: Could not locate Visual Studio installation.
    exit /b 1
)

set VSIX_INSTALLER="%VS_PATH%\Common7\IDE\VSIXInstaller.exe"
set VSIX_FILE="%~dp0..\src\FilePathOnDocument\bin\Release\net48\FilePathOnDocument.vsix"

if not exist %VSIX_FILE% (
    echo ERROR: VSIX not found. Run a Release build first.
    exit /b 1
)

echo Installing FilePathOnDocument...
%VSIX_INSTALLER% /quiet %VSIX_FILE%

if %ERRORLEVEL% == 0 (
    echo Install successful.
) else (
    echo Install failed with exit code %ERRORLEVEL%.
)

exit /b %ERRORLEVEL%
