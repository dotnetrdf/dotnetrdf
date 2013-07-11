@echo off

set TEMP=temp.txt
set FOUND=found.txt

:: Delete working files if they exist
if exist %TEMP% rem %TEMP%
if exist %FOUND% rem %FOUND%

:: Determine if there are unversioned files
hg status > %TEMP%
findstr /B /L ? %TEMP% > %FOUND%
echo %~dp0
set SIZE=%%~zFOUND

:: Clean up working files
rem %TEMP%

:: Exit as appropriate
if %SIZE% gtr 0 (
  echo %~dp0FOUND
  echo %SIZE%
  echo Unversioned files present while attempting commit
  type %FOUND%
  exit /b 1
) else (
  echo Commit has no unversioned files, OK to proceed
  exit /b 0
)