@echo off
cd /d "%~dp0"
dotnet build .\LiveWallpaperWin32.slnx -p:PlatformTarget=x86 -v:q
dotnet build .\LiveWallpaperWin32.slnx -p:PlatformTarget=x64 -v:q
dotnet build .\LiveWallpaperWin32.slnx -p:PlatformTarget=arm64 -v:q

if not exist bin\output (
  md bin\output || exit /b 1
)

for /f "delims=" %%f in ('dir /s /b /a-d bin') do @copy /y "%%f" bin\output
