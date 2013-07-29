@echo off

set OPT_BUILD_OGLGEOM=
set OPT_BUILD_REDIST=
set OPT_BUILD_STUDIO=
set OPT_REBUILD=
set OPT_BUILD_MASTER=

IF "%OPT_CONFIGURATION%"=="" GOTO DISPLAYUSAGE

:STARTLOOP
IF "%1"=="" GOTO ENDLOOP
    IF /I "%1"=="OGL" (
        SET OPT_BUILD_OGLGEOM=YES
        SHIFT
    ) ELSE ( IF /I "%1"=="MASTER" (
        SET OPT_BUILD_MASTER=YES
        SHIFT
    ) ELSE ( IF /I "%1"=="REDIST" (
        SET OPT_BUILD_REDIST=YES
        SHIFT
    ) ELSE ( IF /I "%1"=="STUDIO" (
        SET OPT_BUILD_STUDIO=YES
        SHIFT
    ) ELSE ( IF /I "%1"=="REBUILD" (
        SET OPT_REBUILD=YES
        SHIFT
    ) ELSE ( IF /I "%1"=="ENV" (
        CALL setupenv.bat
        SHIFT
    ) ELSE (SHIFT) )))))

GOTO STARTLOOP
:ENDLOOP

IF "%OPT_BUILD_OGLGEOM%"=="YES" (
set OPT_BUILD_REDIST=
call build-ogl.bat
)

IF "%OPT_BUILD_REDIST%"=="YES" (
msbuild %REDIST_SLN_PATH% /t:Rebuild /p:platform=x64 /p:configuration=%OPT_CONFIGURATION%
)

IF "%OPT_BUILD_STUDIO%"=="YES" (
msbuild %STUDIO_SLN% /t:Rebuild /p:platform=x64 /p:configuration=%OPT_CONFIGURATION%
)

IF "%OPT_BUILD_MASTER%"=="YES" (
msbuild %MASTER_SLN_PATH% /t:Rebuild /p:platform=x64 /p:configuration=%OPT_CONFIGURATION%
copy /Y "%THIRD_PARTIES%\*.*" "%FINAL_BIN%"
)

GOTO DONE

:DISPLAYUSAGE
@echo.
@echo ERROR 'setupenv.bat' not called!
@echo.

:DONE
set OPT_BUILD_OGLGEOM=
set OPT_BUILD_REDIST=
set OPT_BUILD_STUDIO=
set OPT_REBUILD=
set OPT_BUILD_MASTER=
