@echo off
@chcp 65001 1> NUL 2> NUL

SET gitHookDir=.\.git\hooks
SET preMergeCommit=%gitHookDir%\pre-merge-commit

echo Pre-Merge-Commit Path : %preMergeCommit%

if exist %preMergeCommit% (
    del %preMergeCommit%
)

(
    echo #!/bin/sh
    echo # https://thomasvilhena.com/2021/11/prevent-merge-from-specific-branch-git-hook
    echo CURRENT_BRANCH="$(git rev-parse --abbrev-ref HEAD)"
    echo ALLOWED_BRANCH="dev"
    echo ART_BRANCH_PREFIX="art/"
    echo if [[ $GIT_REFLOG_ACTION == *merge* ]]; then
    echo 	if [[ $GIT_REFLOG_ACTION != *$ALLOWED_BRANCH* ]]; then
    echo        if [[ $CURRENT_BRANCH == *$ART_BRANCH_PREFIX* ]]; then
    echo                 echo
    echo                 echo \# !!! 머지를 진행할 수 없습니다 !!!
    echo                 echo \#
    echo                 echo \# 현재 브랜치 : \"$CURRENT_BRANCH\"
    echo                 echo \#
    echo                 echo \# 머지는 다음 브랜치에서만 허용됩니다 : \"$ALLOWED_BRANCH\"
    echo                 echo \#
    echo                 echo \# [커스텀 액션 - Git 리셋]으로 머지를 초기화 해 주세요.
    echo                 echo \#
    echo                 echo \# 또는 다음 명령으로 머지를 초기화. "(git reset --merge)"
    echo                 echo
    echo                 exit 1
    echo         fi
    echo 	fi
    echo fi
) > %preMergeCommit%