call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsMSBuildCmd.bat" > NUL

msbuild /t:Clean /p:Configuration=Release .\source\WEAK.sln
msbuild /t:Build /p:Configuration=Release .\source\WEAK.sln

C:\Users\Doraku\.nuget\packages\xunit.runner.console\2.2.0\tools\xunit.console.exe .\source\WEAK.Test\bin\Release\net45\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html result_weak_net45.html

@ECHO off
FOR /f %%i in ("error.txt") do SET size=%%~zi
IF %size% GTR 0 GOTO :end
@ECHO on

msbuild /t:Pack /p:Configuration=Release .\source\WEAK.sln

:end
@ECHO off
rd TestResults /s /q

@ECHO on
more "error.txt"