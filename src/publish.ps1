Remove-Item "F:\x64\*" -recurse
Remove-Item "F:\x86\*" -recurse
dotnet publish -c release -r win-x64 /p:Platform=x64 /p:PublishSingleFile=true /p:PublishTrimmed=true /p:DebugType=None -o "F:\x64"
dotnet publish -c release -r win-x86 /p:Platform=x86 /p:PublishSingleFile=true /p:PublishTrimmed=true /p:DebugType=None -o "F:\x86"