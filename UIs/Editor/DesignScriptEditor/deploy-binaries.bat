
@echo off
@echo.
@echo Deploying DesignScript binaries to build environment...

if '%rxcoreapps%' == '' goto EnvNotSetUp

@echo.
copy ..\..\Prototype\bin\x64\Debug\*.dll "%rxcoreapps%\AcDesignScript\Redist\x64\Debug"
copy ..\..\Prototype\bin\x64\Release\*.dll "%rxcoreapps%\AcDesignScript\Redist\x64\Release"

@echo.
@echo Deployment completed, do have fun.
@echo.

goto :eof

:EnvNotSetUp

@echo.
@echo Your AutoCAD build environment isn't set up, go get some rest.
@echo.
