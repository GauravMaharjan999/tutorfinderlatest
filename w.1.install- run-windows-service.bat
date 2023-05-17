sc create LiveEncodingService binpath=%~dp0windowsServiceArtifacts\KachuwaSchedularService.exe
Net Start LiveEncodingService 
sc config LiveEncodingService start=auto
pause

