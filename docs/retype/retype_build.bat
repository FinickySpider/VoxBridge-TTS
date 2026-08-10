@echo off
setlocal enabledelayedexpansion

cd /d "%~dp0"

echo Cleaning...
call retype clean
if errorlevel 1 (
    echo.
    echo Error: retype clean failed
    pause
    exit /b 1
)

echo.
echo Building...
call retype build
if errorlevel 1 (
    echo.
    echo Error: retype build failed
    pause
    exit /b 1
)


echo.
echo Pushing to GitHub Pages...
call run_voxbridge_push_menu.bat 2
if errorlevel 1 (
    echo.
    echo Error: push to GitHub Pages failed
    pause
    exit /b 1
)



echo.
echo Build complete.
pause



