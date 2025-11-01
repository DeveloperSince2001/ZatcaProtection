@echo off
:: تفعيل UTF-8
chcp 65001 >nul
echo ==============================
echo رفع التعديلات الجديدة على GitHub
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

:: اطلب رسالة الكوميت من المستخدم
set /p COMMIT_MSG=➡️ ادخل رسالة الكوميت: 

echo --- Adding all changes ---
git add .

echo --- Committing changes ---
git commit -m "%COMMIT_MSG%"

echo --- Pushing to GitHub ---
git push

echo ==============================
echo ✅ Done! التعديلات اترفعت على GitHub
echo ==============================
pause
