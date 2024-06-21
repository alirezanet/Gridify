@echo off
setlocal enabledelayedexpansion

REM Check if an argument is passed to the script
if "%~1"=="" (
    set /p new_version=Enter the new version number:
) else (
    set new_version=%~1
)

dotnet husky run -n update-version -a %new_version%

