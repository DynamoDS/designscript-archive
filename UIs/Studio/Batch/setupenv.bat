@echo off 

call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

@echo Setting environment for building DesignScript Studio with OpenGL geometry

set OPT_CONFIGURATION=Debug
IF /I "%1"=="Release" set OPT_CONFIGURATION=Release

REM ################################################################################
REM Begin Machine specific configurations

set SVN_ROOT=C:\DS\DesignScript
set ASM=C:\DS\asm
set BOOST_ROOT=C:\DS\boost_1_51\
set THIRD_PARTIES=C:\DS\gl_3rdparty_x64\%OPT_CONFIGURATION%

set SVN_BRANCH=%SVN_ROOT%\trunk\DesignScript
REM End Machine specific configurations
REM ################################################################################

REM Common SVN-based configurations
set PATH=%PATH%;%SVN_BRANCH%\Studio\Batch
set STUDIO_PATH=%SVN_BRANCH%\Studio
set STUDIO_SLN=%STUDIO_PATH%\DesignScriptStudio.sln
set STUDIO_BIN=%STUDIO_PATH%\bin\x64\%OPT_CONFIGURATION%
set FINAL_BIN=%SVN_BRANCH%\Prototype\bin\x64\%OPT_CONFIGURATION%
set GL_GEOMETRY_ROOT=%SVN_BRANCH%\Libraries\GLGeometry
set STUDIO_PROJECT_ROOT=%SVN_BRANCH%\Studio
set REDIST_SLN_PATH=%SVN_BRANCH%\Redistributable.sln
set MASTER_SLN_PATH=%SVN_BRANCH%\DesignScriptMaster.sln
set GLGEOM_SETUP=%SVN_BRANCH%\Libraries\GLGeometry\setupenv.bat

REM Common OpenGL geometry based configurations
set GL_GEOMETRY_BIN=%GL_GEOMETRY_ROOT%\bin\x64\%OPT_CONFIGURATION%
set GL_GEOMETRY_BINRELEASE=%GL_GEOMETRY_ROOT%\bin_release
set GL_GEOMETRY_ASMSDK=%GL_GEOMETRY_ROOT%\asm\asm_sdk
set GL_GEOMETRY_BOOSTSDK=%GL_GEOMETRY_ROOT%\boost_1_51

cd /d "%STUDIO_PATH%"
