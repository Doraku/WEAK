@ECHO off
SET PATH=%PATH%;"C:\Users\Doraku\.nuget\packages\xunit.runner.console\2.2.0\tools\";"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"

DEL *.html
DEL *.nupkg

msbuild /t:Clean /p:Configuration=Release .\source\WEAK.sln
msbuild /t:Build /p:Configuration=Release .\source\WEAK.sln


xunit.console.exe .\source\WEAK.Test\bin\Release\net45\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html weak_net45.html
IF %ERRORLEVEL% EQU 0 DEL weak_net45.html
xunit.console.exe .\source\WEAK.Test\bin\Release\net451\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html weak_net451.html
IF %ERRORLEVEL% EQU 0 DEL weak_net451.html
xunit.console.exe .\source\WEAK.Test\bin\Release\net452\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html weak_net452.html
IF %ERRORLEVEL% EQU 0 DEL weak_net452.html

xunit.console.exe .\source\WEAK.Test\bin\Release\net46\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html weak_net46.html
IF %ERRORLEVEL% EQU 0 DEL weak_net46.html
xunit.console.exe .\source\WEAK.Test\bin\Release\net461\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html weak_net461.html
IF %ERRORLEVEL% EQU 0 DEL weak_net461.html
xunit.console.exe .\source\WEAK.Test\bin\Release\net462\WEAK.Test.dll -notrait "Category=Performance" -notrait "Category=Memory" -html weak_net462.html
IF %ERRORLEVEL% EQU 0 DEL weak_net462.html

SET Count=0
FOR %%A IN (weak_*.html) DO SET /A Count += 1

IF %Count% GTR 0 GOTO :end

msbuild /t:Pack /p:Configuration=Release .\source\WEAK\WEAK.csproj
IF %ERRORLEVEL% EQU 0 MOVE .\source\WEAK\bin\Release\*.nupkg .\

:end