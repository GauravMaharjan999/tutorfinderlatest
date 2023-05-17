
SET builddir=%~dp0
SET EX="C:\Program Files\IIS Express\iisexpress.exe"
CALL %EX% /path:%builddir%home /port:8880
pause
