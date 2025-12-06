@echo off
echo ========================================
echo Unity 6 + DOTS 1.4 Clean Reimport Script
echo ========================================
echo.
echo This script will:
echo 1. Close Unity (you need to do this manually first!)
echo 2. Delete Library folder to force DOTS codegen regeneration
echo 3. Delete Temp folder
echo 4. You then reopen Unity 6
echo.
echo IMPORTANT: Close Unity before running this script!
echo.
pause

echo.
echo Deleting Library folder...
if exist "Library" (
    rmdir /s /q "Library"
    echo Library folder deleted successfully!
) else (
    echo Library folder not found (already deleted?)
)

echo.
echo Deleting Temp folder...
if exist "Temp" (
    rmdir /s /q "Temp"
    echo Temp folder deleted successfully!
) else (
    echo Temp folder not found (already deleted?)
)

echo.
echo ========================================
echo Done!
echo ========================================
echo.
echo Now:
echo 1. Reopen Unity 6
echo 2. Wait for DOTS codegen regeneration (5-10 minutes)
echo 3. Wait for full compilation
echo 4. Check for errors in Console
echo 5. Report any errors for fixing
echo.
echo This clean reimport ensures all DOTS .gen.cs files
echo recompile against Unity 6's property system.
echo.
pause
