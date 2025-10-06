@echo off
REM Rigol DHO914S Oscilloscope Tools - Windows Batch Script
REM This script provides easy access to common oscilloscope operations

echo.
echo ========================================
echo  Rigol DHO914S Oscilloscope Tools
echo  https://github.com/hectorMiranda/Rigol_DHO914S
echo ========================================
echo.

REM Check if Python is available
python --version >nul 2>&1
if errorlevel 1 (
    echo Error: Python is not installed or not in PATH
    echo Please install Python 3.7+ and try again
    pause
    exit /b 1
)

REM Check if required packages are installed
python -c "import pyvisa" >nul 2>&1
if errorlevel 1 (
    echo Installing required packages...
    pip install -r requirements.txt
    if errorlevel 1 (
        echo Error: Failed to install required packages
        pause
        exit /b 1
    )
)

:menu
echo.
echo Please select an option:
echo.
echo 1. Take a screenshot
echo 2. Get device information
echo 3. Export waveform data
echo 4. Run basic usage examples
echo 5. Run automated test suite
echo 6. Install as package (development mode)
echo 7. Exit
echo.
set /p choice="Enter your choice (1-7): "

if "%choice%"=="1" goto screenshot
if "%choice%"=="2" goto info
if "%choice%"=="3" goto export
if "%choice%"=="4" goto examples
if "%choice%"=="5" goto test
if "%choice%"=="6" goto install
if "%choice%"=="7" goto exit
echo Invalid choice. Please try again.
goto menu

:screenshot
echo.
echo Taking screenshot...
python scripts\screenshot.py --timestamp --format PNG
if errorlevel 1 (
    echo Screenshot failed. Check connection and try again.
) else (
    echo Screenshot completed successfully!
)
pause
goto menu

:info
echo.
echo Getting device information...
python scripts\scope_info.py --verbose
if errorlevel 1 (
    echo Failed to get device info. Check connection and try again.
) else (
    echo Device information retrieved successfully!
)
pause
goto menu

:export
echo.
echo Exporting waveform data from all channels...
if not exist "data" mkdir data
python scripts\waveform_export.py --all-channels --format csv,plot --output data --verbose
if errorlevel 1 (
    echo Waveform export failed. Check connection and try again.
) else (
    echo Waveform export completed successfully!
    echo Data saved to 'data' directory
)
pause
goto menu

:examples
echo.
echo Running basic usage examples...
python src\examples\basic_usage.py
if errorlevel 1 (
    echo Examples failed. Check connection and try again.
) else (
    echo Examples completed successfully!
)
pause
goto menu

:test
echo.
echo Running automated test suite...
if not exist "test_results" mkdir test_results
python src\examples\automated_test.py --output test_results --verbose
if errorlevel 1 (
    echo Test suite failed. Check connection and try again.
) else (
    echo Test suite completed successfully!
    echo Results saved to 'test_results' directory
)
pause
goto menu

:install
echo.
echo Installing package in development mode...
pip install -e .
if errorlevel 1 (
    echo Installation failed.
) else (
    echo Package installed successfully!
    echo You can now use 'rigol-screenshot', 'rigol-info', and 'rigol-export' commands
)
pause
goto menu

:exit
echo.
echo Thank you for using Rigol DHO914S Tools!
echo Visit: https://github.com/hectorMiranda/Rigol_DHO914S
echo.
pause
exit /b 0
