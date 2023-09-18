@echo off
@chcp 65001 1> NUL 2> NUL
setlocal enableextensions enabledelayedexpansion

set ERROR_MSG="unknown fail"

:: test
rem set TARGET_BRANCH=feature/MeetingRoomUI
rem set SOURCE_BRANCH=art/UI_Dev

rem git merge --no-ff art/UI_Dev -m Merge from magic_btn.bat - Merge branch '%SOURCE_BRANCH%' into '%TARGET_BRANCH%'"
rem git push origin %TARGET_BRANCH%
rem git status
rem exit

:: git fetch
echo @ git fetch
git fetch
echo.

:: argument count check
echo @ argument count check
rem echo param 01 : %1
rem echo param 02 : %2
set MATCH_ARG_COUNT=1
set ARG_COUNT=0
for %%i in (%*) do set /a ARG_COUNT+=1
echo   -^> arg count : %ARG_COUNT%

if %ARG_COUNT% neq %MATCH_ARG_COUNT% (
	set ERROR_MSG="   -> arg count not match"
	goto FAIL
)
if "%~1" equ "" (
	set ERROR_MSG="   -> arg 1 is empty"
	goto FAIL
)
rem if "%~2" equ "" (
rem 	set ERROR_MSG="   -> arg 2 is empty"
rem 	goto FAIL
rem )
echo.

:: get current branch
:: ex) "feature/MeetingRoomUI"
echo @ get current branch
for /f "tokens=* delims= " %%a in ('git branch --show-current') do set CURRNT_BRANCH=%%a
echo   -^> current branch : %CURRNT_BRANCH%
echo.
echo.

:: target (feature)
:: ex) feature/MeetingRoomUI
set TARGET_BRANCH=%CURRNT_BRANCH%
echo   -^> target (feature) branch : %TARGET_BRANCH%

:: source (art)
:: ex) art/UI_Dev
set SOURCE_BRANCH=%1
echo   -^> source (art) branch : %SOURCE_BRANCH%
echo.

:: source(art) branch name check
echo @ source(art) branch name check
set ART_BRANCH_LIST=art/UI_Dev art/Character
set CHECK_CONTAIN_ART_BRANCH=false
for %%a in (%ART_BRANCH_LIST%) do (
	if %%a==%SOURCE_BRANCH% (
		set CHECK_CONTAIN_ART_BRANCH=true
	)
)
echo   -^> source (art) branch : %SOURCE_BRANCH%
if %CHECK_CONTAIN_ART_BRANCH%==true (
	echo   -^> art branch contain
) else (
	echo   -^> able art branch name list
	for %%a in (%ART_BRANCH_LIST%) do (
		echo    + %%a
	)
	set ERROR_MSG="   -> branch name is not contain art branch"
	goto FAIL
)
echo.

:: remote branch check (source,target)
echo @ remote branch check (source,target)
set REMOTE_TARGET_BRANCH=origin/%TARGET_BRANCH%
set REMOTE_SOURCE_BRANCH=origin/%SOURCE_BRANCH%
echo remote target branch : %REMOTE_TARGET_BRANCH%
echo remote source branch : %REMOTE_SOURCE_BRANCH%

:: remote branch check (target)
echo @ remote branch check (target)
for /f "tokens=* delims= " %%a in ('git branch -r --list %REMOTE_TARGET_BRANCH%') do set TARGET_RESULT=%%a
echo   -^> TARGET_RESULT : %TARGET_RESULT%
if "%TARGET_RESULT%" equ "" (
	set ERROR_MSG="   -> (target) branch (%REMOTE_TARGET_BRANCH%) is not contain remote"
	goto FAIL
) else (
	echo "   -> (target) branch (%REMOTE_TARGET_BRANCH%) is contain remote"
)

:: remote branch check (source)
echo @ remote branch check (source)
for /f "tokens=* delims= " %%a in ('git branch -r --list %REMOTE_SOURCE_BRANCH%') do set SOURCE_RESULT=%%a
echo   -^> SOURCE_RESULT : %SOURCE_RESULT%
if "%SOURCE_RESULT%" equ "" (
	set ERROR_MSG="   -> (source) branch (%REMOTE_SOURCE_BRANCH%) is not contain remote"
	goto FAIL
) else (
	echo "   -> (source) branch (%REMOTE_SOURCE_BRANCH%) is contain remote"
)

:: stash
:: - stash 관리할 경우 어떤것을 받을지랑 msg 중복 가능성 있음. -> git status 로 체크 하는 방향으로.
rem git stash
rem git stash -m "test stash msg"

:: git status check
call:funcGitStatus

:: clean
rem echo @ clean
rem git clean -fd
rem git checkout .
rem echo.

:: update source (art)
echo @ update source (art)
git checkout %SOURCE_BRANCH%
git pull origin %SOURCE_BRANCH%
git submodule update --recursive
rem git clean -fd
echo.

:: update target (feature)
echo @ update target (feature)
git checkout %TARGET_BRANCH%
git pull origin %TARGET_BRANCH%
git submodule update --recursive
rem git clean -fd
echo.

:: time wait
call:funcTimeWait 5

:: git status check
call:funcGitStatus

:: merge log
echo @ merge log
set MERGE_LOG="Merge from magic_btn.bat - Merge branch '%SOURCE_BRANCH%' into '%TARGET_BRANCH%'"
echo merge log : %MERGE_LOG%

:: merge
echo @ merge
for /f "tokens=* delims= " %%a in ('git merge --no-ff %SOURCE_BRANCH% -m %MERGE_LOG%') do set GIT_MERGE=%%a
echo %GIT_MERGE%
echo .
if "%GIT_MERGE%" equ "Automatic merge failed; fix conflicts and then commit the result." (
	set ERROR_MSG="   -> merge conflict."
	goto FAIL
) else if "%GIT_MERGE%" equ "Already up to date." (
	set ERROR_MSG="   -> not merge file."
	goto FAIL
) else (
	echo   -^> merge Success.
)
echo .

:: time wait
call:funcTimeWait 2

:: push
echo @ push
git push origin %TARGET_BRANCH%
echo.

:: fetch
echo @ fetch
git fetch

:: git status check
call:funcGitStatus

:: SUCCESS call.
goto SUCCESS

:: last exit
setlocal disableextensions disabledelayedexpansion
exit

:: FAIL case
:FAIL
echo.
echo @ Fail
echo %ERROR_MSG%
setlocal disableextensions disabledelayedexpansion
exit 1

:: SUCCESS case
:SUCCESS
echo.
echo @ Success
setlocal disableextensions disabledelayedexpansion
exit

:: Git Status
:funcGitStatus
echo @ git status check
set GIT_STATUS=""
set /a index=0
for /f "tokens=* delims= " %%a in ('git status') do (
	set GIT_STATUS[!index!]=%%a
	set /a index=!index!+1
)
set /a lenght=!index!-1
rem echo lenght : %lenght%
for /l %%a in (0,1,!lenght!) do (
	echo !GIT_STATUS[%%a]!
)
rem echo git status (last) : !GIT_STATUS[%lenght%]!

if "!GIT_STATUS[%lenght%]!" equ "nothing to commit, working tree clean" (
	echo   -^> local repo clean
) else (
	set ERROR_MSG="   -> local repo not clean"
	goto FAIL
)
echo.

:: Time wait
:funcTimeWait
echo @ wait
ping -n %~1 127.0.0.1 > NUL
echo .