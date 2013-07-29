@echo off

REM This script is used to gather code coverage information of our NUnit runs.

set SubjectDll=..\bin\x86\Debug\DesignScriptStudio.Graph.Core.dll
set TestCaseDll=..\bin\x86\Debug\DesignScriptStudio.Tests.dll

vsinstr.exe -coverage "%SubjectDll%"

start vsperfmon -coverage -output:results.coverage

"C:\Program Files (x86)\NUnit 2.6.2\bin\nunit-console-x86.exe" "%TestCaseDll%"

vsperfcmd /shutdown
