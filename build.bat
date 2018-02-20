# shell
msbuild  /t:Rebuild UltimateMp3TaggerShell\UltimateMp3TaggerShell.csproj  /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None /p:Platform="Any CPU" /p:OutputPath=
# ui (only windows)
msbuild  /t:Rebuild ModernAudioTagger\ModernAudioTagger.csproj  /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None /p:Platform="Any CPU" /p:OutputPath=