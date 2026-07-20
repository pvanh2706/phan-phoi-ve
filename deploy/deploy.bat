@echo off
setlocal enabledelayedexpansion

REM Cho phep chay tu bat cu dau: chuyen ve thu muc goc repo (thu muc cha cua deploy\)
pushd "%~dp0.."
set REPO_ROOT=%CD%

if not exist "%~dp0config.bat" (
    echo [LOI] Khong tim thay deploy\config.bat
    echo       Hay copy deploy\config.example.bat thanh deploy\config.bat va dien thong tin server that.
    popd
    exit /b 1
)
call "%~dp0config.bat"

set BRANCH=%1
if "%BRANCH%"=="" set BRANCH=%DEFAULT_BRANCH%

echo ============================================================
echo   Deploy nhanh "%BRANCH%" ^(remote %GIT_REMOTE%^) len %SERVER_HOST%
echo ============================================================

echo.
echo [1/6] Cap nhat code tu %GIT_REMOTE%/%BRANCH% ...
call git fetch %GIT_REMOTE%
if errorlevel 1 goto :error
call git checkout %BRANCH%
if errorlevel 1 goto :error
call git pull %GIT_REMOTE% %BRANCH%
if errorlevel 1 goto :error

echo.
echo [2/6] Build frontend ...
pushd frontend
call npm install
if errorlevel 1 (popd & goto :error)
call npm run build
if errorlevel 1 (popd & goto :error)
popd

echo.
echo [3/6] Build backend ^(.NET self-contained linux-x64^) ...
set PUBLISH_DIR=%TEMP%\ppv-publish
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
call dotnet publish backend\src\PpvRecon.Api\PpvRecon.Api.csproj -c Release -r linux-x64 --self-contained true -o "%PUBLISH_DIR%"
if errorlevel 1 goto :error

echo.
echo [4/6] Gop frontend vao wwwroot cua backend ...
if exist "%PUBLISH_DIR%\wwwroot" rmdir /s /q "%PUBLISH_DIR%\wwwroot"
mkdir "%PUBLISH_DIR%\wwwroot"
xcopy /e /i /y /q frontend\dist\* "%PUBLISH_DIR%\wwwroot\" >nul
if errorlevel 1 goto :error

echo.
echo [5/6] Dong goi ...
set TARBALL=%TEMP%\ppv-release.tar.gz
if exist "%TARBALL%" del /f /q "%TARBALL%"
pushd "%PUBLISH_DIR%"
call tar -czf "%TARBALL%" .
if errorlevel 1 (popd & goto :error)
popd

echo.
echo [6/6] Upload va trien khai len server ...
if not exist "%~dp0node_modules\ssh2" (
    echo   Cai dat thu vien ssh2 cho script deploy ^(chi lam 1 lan^) ...
    pushd "%~dp0"
    call npm install
    if errorlevel 1 (popd & goto :error)
    popd
)
node "%~dp0deploy-ssh.js"
if errorlevel 1 goto :error

echo.
echo ============================================================
echo   DEPLOY THANH CONG - http://%SERVER_HOST%/
echo ============================================================
popd
exit /b 0

:error
echo.
echo ============================================================
echo   DEPLOY THAT BAI - xem loi ben tren
echo ============================================================
popd
exit /b 1
