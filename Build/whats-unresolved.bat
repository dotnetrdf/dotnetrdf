@echo off

set TEMP=temp.txt
set FOUND=found.txt

:: Delete working files if they exist
if exist %TEMP% del %TEMP%
if exist %FOUND% del %FOUND%

:: Determine if there are unresolve merge conflicts
hg resolve --list > %TEMP%
findstr /B /L U %TEMP% > %FOUND%
FOR /F "usebackq" %%A IN ('%FOUND%') DO set SIZE=%%~zA

:: Clean up working files
del %TEMP%

:: Determine exit code to return
set EXIT=0
if %SIZE% gtr 0 (
  echo Unresolved merge conflicts remain:
  type %FOUND%
  set EXIT=1
) else (
  echo No unresolved merge conflicts
)

:: Clean up working files
del %FOUND%

:: Exit
exit /b %EXIT%