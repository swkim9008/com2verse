@echo off

if exist "*.csproj" del "*.csproj"
if exist "*.sln" del "*.sln"
if exist "*.log" del "*.log"
if exist ".idea\.idea.c2vclient\.idea\shelf" rmdir /s /q ".idea\.idea.c2vclient\.idea\shelf"
if exist "AddressableContents" rmdir /s /q "AddressableContents"
if exist "Build" rmdir /s /q "Build"
if exist "Builds" rmdir /s /q "Builds"
if exist "Library" rmdir /s /q "Library"
if exist "Logs" rmdir /s /q "Logs"
if exist "obj" rmdir /s /q "obj"

set temp_dir=%Temp%\Com2Verse
if exist "%temp_dir%" rmdir /s /q "%temp_dir%"

set appdata_dir=%AppData%\..\LocalLow\Com2Verse
if exist "%appdata_dir%" rmdir /s /q "%appdata_dir%"

set editor_dir=%AppData%\..\LocalLow\Unity\Com2Verse_Com2Verse
if exist editor_dir=%" rmdir /s /q editor_dir=%"

echo Clean up finished!

pause