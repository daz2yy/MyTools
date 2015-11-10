@echo off
set DIR=%~dp0
call %DIR%compile_scripts.bat -i ../../../Resource -o METestAPP.zip -m zip

echo.
echo ### UPDATING ###
echo.
echo updating all METestAPP.zip
echo.

dir /s/b "../../../Resource/" | find "METestAPP.zip" > ___tmp___

for /f %%f in (___tmp___) do (
    echo %%f
    copy METestAPP.zip %%f > NUL
)

del ___tmp___

echo.
echo DONE
echo.