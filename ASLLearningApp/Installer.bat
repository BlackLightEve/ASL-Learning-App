@echo off
SETLOCAL

:: Check if Python 3.10 is available
py -3.10 --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Python 3.10 is not installed or not added to PATH.
    echo Please install Python 3.10 and add it to PATH.
    GOTO :EOF
)

:: Install the required libraries
py -3.10 -m pip install --upgrade pip
py -3.10 -m pip install mediapipe opencv-python pywin32

echo Installation complete.
ENDLOCAL