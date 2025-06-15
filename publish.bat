@echo off
setlocal
set DOTNET_FLAGS=-c Release

dotnet publish Snaffler\Snaffler.csproj %DOTNET_FLAGS% -f net451 -p:PublishProfile=net451

dotnet publish Snaffler\Snaffler.csproj %DOTNET_FLAGS% -f net9.0 -p:PublishProfile=net9
endlocal

