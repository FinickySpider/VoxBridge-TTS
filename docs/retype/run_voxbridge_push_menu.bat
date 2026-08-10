@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "PY_SCRIPT=%SCRIPT_DIR%push_folder_to_voxbridge.py"
set "PYTHON_CMD="
set "AUTO_OPTION=%~1"
set "AUTO_SOURCE=%~2"

if not exist "%PY_SCRIPT%" (
    echo error: Could not find push_folder_to_voxbridge.py next to this .bat file.
    echo Expected: "%PY_SCRIPT%"
    pause
    exit /b 1
)

where python >nul 2>nul
if "%ERRORLEVEL%"=="0" set "PYTHON_CMD=python"

if "%PYTHON_CMD%"=="" (
    where py >nul 2>nul
    if "%ERRORLEVEL%"=="0" set "PYTHON_CMD=py"
)

if "%PYTHON_CMD%"=="" (
    echo error: Could not find Python. Install Python or add it to PATH.
    pause
    exit /b 1
)

if not "%AUTO_OPTION%"=="" (
    set "choice=%AUTO_OPTION%"
    goto auto_dispatch
)

:menu
cls
echo ==========================================
echo   VoxBridge-TTS Folder Push Menu
echo ==========================================
echo.
echo Target repo:
echo   https://github.com/FinickySpider/VoxBridge-TTS
echo.
echo 1. Dry run - preview changes only
echo 2. Push folder contents
echo 3. Push folder contents with custom commit message
echo 4. Prune push - mirror folder to repo root
echo 5. Prune push with custom commit message
echo 6. Show Python script help
echo 7. Exit
echo.
set /p "choice=Choose an option [1-7]: "

:auto_dispatch
if "%choice%"=="1" goto dry_run
if "%choice%"=="2" goto push_default
if "%choice%"=="3" goto push_custom_message
if "%choice%"=="4" goto prune_push
if "%choice%"=="5" goto prune_custom_message
if "%choice%"=="6" goto show_help
if "%choice%"=="7" goto done

echo.
echo Invalid choice. Try again.
if not "%AUTO_OPTION%"=="" exit /b 1
pause
goto menu

:get_source
if not "%AUTO_SOURCE%"=="" (
    set "SOURCE_FOLDER=%AUTO_SOURCE%"
) else (
    set "SOURCE_FOLDER=%SCRIPT_DIR%.retype"
)

if not exist "%SOURCE_FOLDER%\" (
    echo error: Folder does not exist: "%SOURCE_FOLDER%"
    if not "%AUTO_OPTION%"=="" exit /b 1
    pause
    exit /b 1
)

exit /b 0

:dry_run
call :get_source
if errorlevel 1 goto menu
echo.
%PYTHON_CMD% "%PY_SCRIPT%" --dry-run "%SOURCE_FOLDER%"
set "RESULT=%ERRORLEVEL%"
echo.
if not "%AUTO_OPTION%"=="" exit /b %RESULT%
pause
goto menu

:push_default
call :get_source
if errorlevel 1 goto menu
echo.
%PYTHON_CMD% "%PY_SCRIPT%" "%SOURCE_FOLDER%"
set "RESULT=%ERRORLEVEL%"
echo.
if not "%AUTO_OPTION%"=="" exit /b %RESULT%
pause
goto menu

:push_custom_message
call :get_source
if errorlevel 1 goto menu
echo.
set "COMMIT_MESSAGE=%~3"
if "%COMMIT_MESSAGE%"=="" set /p "COMMIT_MESSAGE=Enter commit message: "
if "%COMMIT_MESSAGE%"=="" (
    echo error: Commit message is required.
    if not "%AUTO_OPTION%"=="" exit /b 1
    pause
    goto menu
)
echo.
%PYTHON_CMD% "%PY_SCRIPT%" -m "%COMMIT_MESSAGE%" "%SOURCE_FOLDER%"
set "RESULT=%ERRORLEVEL%"
echo.
if not "%AUTO_OPTION%"=="" exit /b %RESULT%
pause
goto menu

:prune_push
call :get_source
if errorlevel 1 goto menu
if "%AUTO_OPTION%"=="" (
    call :confirm_prune
    if errorlevel 1 goto menu
)
echo.
%PYTHON_CMD% "%PY_SCRIPT%" --prune "%SOURCE_FOLDER%"
set "RESULT=%ERRORLEVEL%"
echo.
if not "%AUTO_OPTION%"=="" exit /b %RESULT%
pause
goto menu

:prune_custom_message
call :get_source
if errorlevel 1 goto menu
if "%AUTO_OPTION%"=="" (
    call :confirm_prune
    if errorlevel 1 goto menu
)
echo.
set "COMMIT_MESSAGE=%~3"
if "%COMMIT_MESSAGE%"=="" set /p "COMMIT_MESSAGE=Enter commit message: "
if "%COMMIT_MESSAGE%"=="" (
    echo error: Commit message is required.
    if not "%AUTO_OPTION%"=="" exit /b 1
    pause
    goto menu
)
echo.
%PYTHON_CMD% "%PY_SCRIPT%" --prune -m "%COMMIT_MESSAGE%" "%SOURCE_FOLDER%"
set "RESULT=%ERRORLEVEL%"
echo.
if not "%AUTO_OPTION%"=="" exit /b %RESULT%
pause
goto menu

:confirm_prune
echo.
echo WARNING: Prune mode deletes files from the repo root if they are not in the source folder.
set "CONFIRM="
set /p "CONFIRM=Type YES to continue: "
if /i not "%CONFIRM%"=="YES" (
    echo Prune push cancelled.
    pause
    exit /b 1
)
exit /b 0

:show_help
echo.
%PYTHON_CMD% "%PY_SCRIPT%" --help
set "RESULT=%ERRORLEVEL%"
echo.
if not "%AUTO_OPTION%"=="" exit /b %RESULT%
pause
goto menu

:done
endlocal
exit /b 0