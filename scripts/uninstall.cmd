@echo off
setlocal

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -property installationPath`) do set VS_PATH=%%i

if "%VS_PATH%"=="" (
    echo ERROR: Could not locate Visual Studio installation.
    exit /b 1
)

set VSIX_INSTALLER="%VS_PATH%\Common7\IDE\VSIXInstaller.exe"
set EXTENSION_ID=FilePathOnDocument.63fa84b8-9f97-49d3-bcda-f5cb92f829d0

echo Uninstalling FilePathOnDocument...
%VSIX_INSTALLER% /quiet /uninstall:%EXTENSION_ID%

if %ERRORLEVEL% == 0 (
    echo Uninstall successful.
) else (
    echo Uninstall failed with exit code %ERRORLEVEL%.
)

exit /b %ERRORLEVEL%
