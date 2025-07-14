@echo off

:: Set output directory path
set "OUTDIR=C:\FXServer\txData\FiveMServer.base\resources\[local]\vehicle_menu"

:: Build Client
pushd Client
dotnet build -c Release
popd

:: Build Server
pushd Server
dotnet build -c Release
popd

:: Reset destination directory
rmdir /s /q "%OUTDIR%"
mkdir "%OUTDIR%"

:: Copy manifest
copy /y fxmanifest.lua "%OUTDIR%"


:: Copy compiled binaries
xcopy /y /e Client\bin\Release\net452\publish "%OUTDIR%\Client\"
xcopy /y /e Server\bin\Release\netstandard2.0\publish "%OUTDIR%\Server\"

:: ✅ Copy NUI HTML assets
xcopy /y /e Client\html "%OUTDIR%\html\"
