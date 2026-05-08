@echo off
echo Building Jellyfin Local Chat Plugin...

REM Clean previous build
if exist "build" rmdir /s /q "build"
mkdir "build"

REM Build the project
dotnet build JellyfinLocalChat\JellyfinLocalChat.csproj -c Release -o "build\JellyfinLocalChat"

REM Create plugin directory structure
mkdir "build\plugins"
mkdir "build\plugins\JellyfinLocalChat"

REM Copy the DLL to the plugin directory
copy /Y "build\JellyfinLocalChat\JellyfinLocalChat.dll" "build\plugins\JellyfinLocalChat\JellyfinLocalChat.dll"

REM Copy manifest.json to the plugin directory
copy /Y "manifest.json" "build\plugins\JellyfinLocalChat\manifest.json"

REM Create the ZIP file
powershell "Compress-Archive -Path 'build\plugins\JellyfinLocalChat' -DestinationPath 'JellyfinLocalChat.zip' -Force"

echo Build complete! Plugin ZIP created: JellyfinLocalChat.zip
echo.
echo To install:
echo 1. Copy JellyfinLocalChat.zip to your Jellyfin server's plugins directory
echo 2. Restart Jellyfin server
echo 3. Enable the plugin in Dashboard -> Plugins