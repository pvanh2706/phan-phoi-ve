@echo off
REM Copy file nay thanh "config.bat" (cung thu muc) va dien thong tin that.
REM config.bat KHONG duoc commit len git (da nam trong .gitignore).

set SERVER_HOST=10.19.193.129
set SERVER_USER=anhbt@ezcloud.ez
set SERVER_PASS=CHANGE_ME

REM Remote/branch mac dinh khi khong truyen tham so cho deploy.bat
set GIT_REMOTE=bitbucket
set DEFAULT_BRANCH=master
