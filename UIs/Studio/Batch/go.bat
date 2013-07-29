@echo off 

if "%1" == "" exit

if "%1" == "bin" cd /d "%FINAL_BIN%"

if "%1" == "run" (
    cd /d "%FINAL_BIN%"
    start "DesignScript Studio" "%FINAL_BIN%\DesignScriptStudio.App.exe"
)

if "%1" == "ide" (
    cd /d "%FINAL_BIN%"
    start "DesignScript Studio" "%FINAL_BIN%\DesignScript.App.exe"
)
