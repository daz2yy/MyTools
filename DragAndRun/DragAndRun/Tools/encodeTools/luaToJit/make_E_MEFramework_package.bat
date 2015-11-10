@echo off
set DIR=%~dp0
call %DIR%compile_scripts.bat -i ../../../MEFramework -o MEFramework.zip -m zip -p MEFramework

echo.
echo ### UPDATING ###
echo.
echo updating all MEFramework.zip
echo.

rem dir /s/b "../../../Resource/" | find "MEFramework.zip" > ___tmp___

rem for /f %%f in (___tmp___) do (
rem     echo %%f
rem     copy MEFramework.zip %%f > NUL
rem )

rem del ___tmp___

echo.
echo DONE
echo.