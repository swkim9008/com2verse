@echo off
@chcp 65001 1> NUL 2> NUL
rem set TEST_FLAG=true

set FILE_TO_SIGNING=%1
if not defined FILE_TO_SIGNING (
    echo "ERROR: Invalid fileName (%FILE_TO_SIGNING%)" 1>&2
    if defined TEST_FLAG (
        pause
    )
    exit 1
)

setlocal enableextensions enabledelayedexpansion

echo .
set WINSDK_BIN="C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64"
echo * WINSDK_BIN : %WINSDK_BIN%
echo.
set SIGN_TOOL=%WINSDK_BIN%\signtool
echo * SIGN_TOOL : %SIGN_TOOL%
echo.
set NAME="Com2Verse Corporation"
set TIMESTAMP_URL=http://timestamp.digicert.com
set SHA_1_PARAM=/a /s my /n %NAME% /t %TIMESTAMP_URL% /v %FILE_TO_SIGNING%
set SHA_2_PARAM=/a /s my /n %NAME% /tr %TIMESTAMP_URL% /td sha256 /fd sha256 /v %FILE_TO_SIGNING%
set SIGN_SHA2=%SIGN_TOOL% sign %SHA_2_PARAM%
echo * SIGN_SHA2 : %SIGN_SHA2%
echo.

:: run
%SIGN_SHA2%

:: error
IF %ERRORLEVEL% NEQ 0 (
    echo "error level : %ERRORLEVEL%"
    exit %ERRORLEVEL%
)

if defined TEST_FLAG (
    pause
)
setlocal disableextensions disabledelayedexpansion
exit 0