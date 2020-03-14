REM @echo off

IF EXIST .\compile.cmd cd ..

CALL scripts\environnement.cmd

scripts\nuget restore

REM --------------------------------------------------------
REM  MSBuild
REM --------------------------------------------------------
"%MSBUILD%" /t:Build /p:Configuration=Debug;Platform="Any CPU" "Strev.WebClient.sln" || exit /b -1
"%MSBUILD%" /t:Build /p:Configuration=Release;Platform="Any CPU" "Strev.WebClient.sln" || exit /b -1
