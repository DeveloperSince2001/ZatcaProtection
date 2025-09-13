@echo off
echo ==============================
echo رفع مشروع C# على GitHub
echo ==============================

:: اطلب من المستخدم إدخال مسار المشروع
set /p PROJECT_PATH=➡️ ادخل المسار الكامل للمجلد اللي فيه المشروع (.sln): 

:: تحقق إن المسار موجود
if not exist "%PROJECT_PATH%" (
    echo ❌ المسار غير موجود: %PROJECT_PATH%
    echo برجاء التأكد من كتابته صحيح
    pause
    exit /b
)

:: اذهب إلى مجلد المشروع
cd /d "%PROJECT_PATH%"
echo ✅ انتقلنا إلى: %cd%

:: اطلب من المستخدم إدخال رابط الريبو
set /p REPO_URL=➡️ ادخل رابط الريبو على GitHub (https://github.com/User/Repo.git): 

echo --- Initializing local git repo ---
git init

echo --- Adding remote origin ---
git remote add origin %REPO_URL%

echo --- Creating .gitignore for Visual Studio ---
dotnet new gitignore

echo --- Adding all files ---
git add .

echo --- Commiting files ---
git commit -m "Initial commit - Added C# solution [ZatcaProtection]"

echo --- Renaming branch to main ---
git branch -M main

echo --- Pushing to GitHub ---
git push -u origin main

echo ==============================
echo ✅ Done! Your project is now on GitHub.
echo ==============================
pause
