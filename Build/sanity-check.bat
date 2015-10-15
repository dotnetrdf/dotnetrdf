@echo off

set TEMP=temp.txt
set FOUND=found.txt

:: Delete working files if they exist
if exist %TEMP% del %TEMP%
if exist %FOUND% del %FOUND%

:: Determine if there are unversioned/missing files
hg status > %TEMP%
findstr /B /L ? %TEMP% > %FOUND%
findstr /B /L ! %TEMP% >> %FOUND%
FOR /F "usebackq" %%A IN ('%FOUND%') DO set SIZE=%%~zA

:: Clean up working files
del %TEMP%

:: Determine exit code to return
set EXIT=0
if %SIZE% gtr 0 (
  echo Unversioned/missing files present while attempting commit:
  type %FOUND%
  set EXIT=1
) else (
  echo Commit has no unversioned/missing files, OK to proceed
)

:: Clean up working files
del %FOUND%

:: Try a quick build
cd Build\nant\
nant compile-libs
if ERRORLEVEL 1 (
  echo Comilation errors occurred
  set EXIT=1
) else (
  echo Code compiled successfully, OK to proceed
)
cd ..\..\

:: Exit
exit /b %EXIT%