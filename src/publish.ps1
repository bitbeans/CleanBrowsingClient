Remove-Item "E:\x64\*" -recurse
Remove-Item "E:\x86\*" -recurse
dotnet publish -c release -r win-x64 /p:Platform=x64 /p:PublishSingleFile=true /p:PublishTrimmed=true /p:DebugType=None -o "E:\x64"
dotnet publish -c release -r win-x86 /p:Platform=x86 /p:PublishSingleFile=true /p:PublishTrimmed=true /p:DebugType=None -o "E:\x86"