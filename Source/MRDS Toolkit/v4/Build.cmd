@echo off
setlocal

rem *** Syntax
if "%~1" == "" (
	echo Configuration name parameter is required.
	exit /b 1
)
set ConfigurationName=%~1

echo %ConfigurationName% Build
echo.
echo Performs a %ConfigurationName% build then copies the output
echo into a solution local "%ConfigurationName%" subdirectory.
echo.

echo.
echo Delete old files...
if exist "%~dp0%ConfigurationName%" (
	rmdir "%~dp0%ConfigurationName%" /s /q
	if %errorlevel% gtr 1 goto error
)

echo.
echo Compiling %ConfigurationName% build...
msbuild "%~dp0MRDS Toolkit v4.sln" /p:Configuration=%ConfigurationName%
if %errorlevel% neq 0 goto error
if not "%ConfigurationName%" == "Release" (
	msbuild "%~dp0MrdsToolkit.Windows.Setup.X64\MrdsToolkit.Windows.Setup.X64.wixproj" /p:SolutionDir=%~dp0;Configuration=%ConfigurationName%
	if %errorlevel% neq 0 goto error
	msbuild "%~dp0MrdsToolkit.Windows.Setup.X86\MrdsToolkit.Windows.Setup.X86.wixproj" /p:SolutionDir=%~dp0;Configuration=%ConfigurationName%
	if %errorlevel% neq 0 goto error
)

echo.
echo Copying components...
robocopy "%~dp0MrdsToolkit.Windows.Services\bin\%ConfigurationName%" "%~dp0%ConfigurationName%\Components" /xf *CodeAnalysis* /xf *.pdb
if %errorlevel% gtr 7 goto error
robocopy "%~dp0Dependencies\Microsoft\Robotics\v4.0" "%~dp0%ConfigurationName%\Components" /s
if %errorlevel% gtr 7 goto error

echo.
echo Copying setup...
robocopy "%~dp0MrdsToolkit.Windows.Setup.X64\bin\%ConfigurationName%\en-US" "%~dp0%ConfigurationName%\Setup" *.msi
if %errorlevel% gtr 7 goto error
robocopy "%~dp0MrdsToolkit.Windows.Setup.X86\bin\%ConfigurationName%\en-US" "%~dp0%ConfigurationName%\Setup" *.msi
if %errorlevel% gtr 7 goto error

echo.
echo Copying documentation...
robocopy "%~dp0Documentation" "%~dp0%ConfigurationName%\Documentation"
if %errorlevel% gtr 7 goto error

echo.
echo Copying version references...
md "%~dp0%ConfigurationName%\Version"
if %errorlevel% neq 0 goto error
copy "%~dp0Version.txt" "%~dp0%ConfigurationName%\Version\MrdsToolkit.Version.txt" /y
if %errorlevel% neq 0 goto error

echo.
echo %ConfigurationName% build successful.
exit /b 0

:error
set result=%errorlevel%
echo.
echo Error %result%
exit /b %result%
