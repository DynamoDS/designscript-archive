@echo off 

REM Start off with a clean slate
rd /S/Q "%FINAL_BIN%"

REM Build "Redistributable.sln"
msbuild %REDIST_SLN_PATH% /t:Rebuild /p:platform=x64 /p:configuration=%OPT_CONFIGURATION%

REM Build "OpenGL Solution"
if not exist %GL_GEOMETRY_ROOT%\asm mkdir %GL_GEOMETRY_ROOT%\asm
if not exist %GL_GEOMETRY_ASMSDK% mklink /D %GL_GEOMETRY_ASMSDK% %ASM%
if not exist %GL_GEOMETRY_BOOSTSDK% mklink /D %GL_GEOMETRY_BOOSTSDK% %BOOST_ROOT% 
if not exist %GL_GEOMETRY_BINRELEASE% mkdir %GL_GEOMETRY_BINRELEASE% 
copy /Y "%FINAL_BIN%\ProtoInterface.dll" "%GL_GEOMETRY_BINRELEASE%"
msbuild %GL_GEOMETRY_ROOT%\ProtoGL.sln /t:Rebuild /p:platform=x64 /p:configuration=%OPT_CONFIGURATION%

REM We no longer do the following, these are now built into FINAL_BIN.
REM copy /Y %GL_GEOMETRY_BIN%\*.dll "%FINAL_BIN%"
REM copy /Y %GL_GEOMETRY_BIN%\*.pdb "%FINAL_BIN%"

copy /Y %GL_GEOMETRY_ROOT%\..\..\installtools\Bundle\DesignScript.Bundle\Contents\Win64\*.exe.config "%FINAL_BIN%"

REM Copy all third party binaries
copy /Y "%THIRD_PARTIES%\*.*" "%FINAL_BIN%"
