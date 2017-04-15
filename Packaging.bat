call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsMSBuildCmd.bat" > NUL

DEL *.nupkg

msbuild /t:Clean,Build /p:Configuration=net45 .\source\WEAK.sln
msbuild /t:Clean,Build /p:Configuration=net46 .\source\WEAK.sln

mstest /testcontainer:.\source\WEAK.Test\bin\net45\WEAK.Test.dll /noresults /detail:errormessage /category:"!Performance&!Memory" /usestderr 2> error.txt
@ECHO off
FOR /f %%i in ("error.txt") do SET size=%%~zi
IF %size% GTR 0 GOTO :end
@ECHO on

mstest /testcontainer:.\source\WEAK.Test\bin\net46\WEAK.Test.dll /noresults /detail:errormessage /category:"!Performance&!Memory" /usestderr 2> error.txt
@ECHO off
FOR /f %%i in ("error.txt") do SET size=%%~zi
IF %size% GTR 0 GOTO :end
@ECHO on

mstest /testcontainer:.\source\WEAK.Windows.Test\bin\net45\WEAK.Windows.Test.dll /noresults /detail:errormessage /category:"!Performance&!Memory" /usestderr 2> error.txt
@ECHO off
FOR /f %%i in ("error.txt") do SET size=%%~zi
IF %size% GTR 0 GOTO :end
@ECHO on
mstest /testcontainer:.\source\WEAK.Windows.Test\bin\net46\WEAK.Windows.Test.dll /noresults /detail:errormessage /category:"!Performance&!Memory" /usestderr 2> error.txt
@ECHO off
FOR /f %%i in ("error.txt") do SET size=%%~zi
IF %size% GTR 0 GOTO :end
@ECHO on

nuget pack .\source\WEAK\WEAK.csproj -properties Configuration=net46
nuget pack .\source\WEAK.Windows\WEAK.Windows.csproj -properties Configuration=net46

:end

rd TestResults /s /q

FOR /f %%i in ("error.txt") do SET size=%%~zi
IF %size% GTR 0 more "error.txt"