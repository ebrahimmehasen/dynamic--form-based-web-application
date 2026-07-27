# ربط السيرفر بـ GitHub عبر Actions — نشر تلقائي عند كل Push على main

الهدف: كل ما تعمل push/merge على main، السيرفر يعمل تلقائيًا:
1. Build + Publish للكود الجديد.
2. تطبيق أي Migrations جديدة على قاعدة البيانات (إضافة/تعديل بدون حذف البيانات الحالية، مع بوابة أمان توقف أي عملية حذف مدمّرة).
3. تحديث الموقع على IIS بدون توقف طويل.

الطريقة: **GitHub Actions Self-hosted Runner** مثبّت على نفس Windows Server. هو الأنسب هنا لأن السيرفر داخلي (on-prem) وميعرفش يفتح للإنترنت بسهولة، فبدل ما GitHub يتصل بالسيرفر، **السيرفر هو اللي بيتصل بـ GitHub** ويسأل "فيه شغل جديد؟" (Pull model)، وده أأمن بكتير من فتح منفذ SSH/WinRM من بره.

---

## الجزء 1: تجهيز السيرفر (مرة واحدة فقط)

### 1.1 تثبيت الأدوات الأساسية

على السيرفر (Windows Server 2022)، تأكد إن دي موجودة (لو عملت الخطوات في الملف السابق فهي غالبًا موجودة):

- **.NET 8 SDK** (مش بس Hosting Bundle — الـ Runner محتاج SDK كامل عشان يعمل `dotnet publish` و`dotnet ef` على السيرفر نفسه):
  نزّل من الموقع الرسمي: "Download .NET 8 SDK" → نسخة x64 Windows installer.
  تأكد بعد التثبيت:
  ```powershell
  dotnet --version
  ```
- **dotnet-ef tool** (عشان تطبيق الـ Migrations):
  ```powershell
  dotnet tool install --global dotnet-ef
  ```
- **Git for Windows** (الـ Runner محتاجه عشان يعمل `git pull`):
  نزّله من git-scm.com وثبّته بالإعدادات الافتراضية.
- **LibreOffice** (لو لسه ما ثبتّهوش — مطلوب لتصدير PDF كما شرحنا قبل كده).

### 1.2 إنشاء مجلد ثابت للنشر ومجلد للـ Runner

```powershell
New-Item -ItemType Directory -Force -Path "C:\Deploy\StudentRegistry"       # هنا هيكون الموقع الفعلي اللي IIS بيقرأ منه
New-Item -ItemType Directory -Force -Path "C:\actions-runner"              # هنا هيتثبت الـ Runner
```

تأكد إن IIS Site (اللي عملته في الملف السابق) الـ Physical Path بتاعه هو نفسه `C:\Deploy\StudentRegistry`.

---

## الجزء 2: تسجيل Self-hosted Runner على الريبو

### 2.1 من صفحة الريبو على GitHub

1. روح لصفحة الريبو على github.com.
2. **Settings → Actions → Runners → New self-hosted runner**.
3. اختر **Windows** و **x64**.
4. GitHub هيديك أوامر تنزيل وتسجيل (فيها Token خاص بالريبو، بيتغير كل مرة — منفّذها زي ما هي).

### 2.2 على السيرفر (PowerShell كـ Administrator)

نفّذ الأوامر اللي GitHub وريهالك، وهتكون بالشكل ده تقريبًا (استبدل بالقيم الحقيقية اللي هيديهولك GitHub):

```powershell
cd C:\actions-runner
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/vX.X.X/actions-runner-win-x64-X.X.X.zip -OutFile actions-runner.zip
Expand-Archive -Path actions-runner.zip -DestinationPath .
.\config.cmd --url https://github.com/اسم_المستخدم/اسم_الريبو --token TOKEN_من_GitHub
```

أثناء `config.cmd` هيسألك أسئلة:
- Runner group: Enter (Default).
- Runner name: مثلاً `student-registry-prod-server`.
- Labels إضافية: اكتب `production` (هنستخدمها لاحقًا في الـ workflow عشان نتأكد إن الشغل بيتنفذ على السيرفر ده بالذات، مش على أي Runner تاني).
- Work folder: Enter (افتراضي).

### 2.3 تثبيت الـ Runner كخدمة Windows دائمة

مهم جدًا — عشان يشتغل تلقائيًا حتى لو حد عمل logout أو السيرفر اتعمله Restart:

```powershell
.\svc.exe install
.\svc.exe start
```

تأكد إنه شغال:
```powershell
Get-Service actions.runner.*
```
لازم تكون الحالة `Running`.

بعد كده هتلاقي في GitHub → Settings → Actions → Runners إن الـ Runner ظاهر باللون الأخضر "Idle".

---

## الجزء 3: تخزين الأسرار (Secrets) بأمان

**لا تحط كلمة مرور قاعدة البيانات أبدًا داخل ملف الـ workflow نفسه.** استخدم GitHub Secrets:

1. Settings → Secrets and variables → Actions → **New repository secret**.
2. أضف:
   - `PROD_DB_CONNECTION_STRING` = كونكشن سترينج الإنتاج الكامل (نفس اللي في appsettings.Production.json بس بكلمة المرور الحقيقية).
3. هذه القيمة هتتحط كـ Environment Variable وقت التشغيل بس، ومش هتتخزن في الكود ولا تظهر في اللوجات.

---

## الجزء 4: ملف الـ Workflow (`.github/workflows/deploy.yml`)

أنشئ الملف ده في الريبو على مسار `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production Server

on:
  push:
    branches: [main]

concurrency:
  group: production-deploy
  cancel-in-progress: false   # ميلغيش نشر شغال عشان أي push جديد، ينتظر يخلص الأول

jobs:
  deploy:
    runs-on: [self-hosted, production]
    timeout-minutes: 20

    steps:
      - name: Checkout latest main
        uses: actions/checkout@v4

      - name: Restore & Build
        run: dotnet build backend/StudentRegistry.sln -c Release
        shell: pwsh

      - name: Check for destructive schema changes (safety gate)
        id: safety_gate
        shell: pwsh
        run: |
          $pending = dotnet ef migrations script --idempotent --project backend/StudentRegistry.Data --startup-project backend/StudentRegistry.API
          $pending | Out-File pending_migration.sql -Encoding utf8
          $destructive = Select-String -Path pending_migration.sql -Pattern "DROP TABLE|DROP COLUMN" -SimpleMatch:$false
          if ($destructive) {
            Write-Host "::error::تم اكتشاف عملية حذف جدول أو عمود في الـ Migration الجديدة. النشر متوقف لمراجعة يدوية."
            $destructive | ForEach-Object { Write-Host $_.Line }
            exit 1
          }
          Write-Host "لا توجد عمليات حذف مدمّرة — آمن للمتابعة."

      - name: Apply database migrations (safe, additive)
        if: success()
        shell: pwsh
        env:
          ConnectionStrings__DefaultConnection: ${{ secrets.PROD_DB_CONNECTION_STRING }}
        run: |
          dotnet ef database update --project backend/StudentRegistry.Data --startup-project backend/StudentRegistry.API

      - name: Publish application
        run: dotnet publish backend/StudentRegistry.API/StudentRegistry.API.csproj -c Release -o C:\Deploy\_publish_temp
        shell: pwsh

      - name: Stop IIS Application Pool
        shell: pwsh
        run: |
          Import-Module WebAdministration
          Stop-WebAppPool -Name "StudentRegistryPool"
          Start-Sleep -Seconds 5

      - name: Sync published files to live folder
        shell: pwsh
        run: |
          robocopy "C:\Deploy\_publish_temp" "C:\Deploy\StudentRegistry" /MIR /XD "wwwroot\uploads" /XF "appsettings.Production.json"
          if ($LASTEXITCODE -ge 8) { throw "robocopy failed with code $LASTEXITCODE" }

      - name: Start IIS Application Pool
        shell: pwsh
        run: |
          Import-Module WebAdministration
          Start-WebAppPool -Name "StudentRegistryPool"

      - name: Health check
        shell: pwsh
        run: |
          Start-Sleep -Seconds 5
          $response = Invoke-WebRequest -Uri "http://localhost/health" -UseBasicParsing -TimeoutSec 30
          if ($response.StatusCode -ne 200) { throw "Health check failed: $($response.StatusCode)" }
```

### ملاحظات مهمة على الملف ده:

- **`/XD "wwwroot\uploads"`**: بيستثني مجلد رفع صور الطلاب من الحذف/الاستبدال عشان الصور المرفوعة سابقًا متتمسحش عند كل نشر.
- **`/XF "appsettings.Production.json"`**: بيستثني ملف الإعدادات من الاستبدال، لأنه فيه بيانات اتصال حقيقية بتتظبط يدويًا مرة واحدة على السيرفر ومينفعش يترفع من الريبو (ولازم أصلاً الملف ده يكون في `.gitignore` — راجع الجزء 5).
- **بوابة الأمان (`safety_gate`)**: أي Migration فيها `DROP TABLE` أو `DROP COLUMN` بتوقف الـ pipeline كله فورًا قبل ما تلمس السيرفر، وتوريك بالظبط السطر اللي فيه المشكلة. لازم تراجعها يدويًا (تتأكد إنها مقصودة) وتشغّلها بنفسك على السيرفر لو فعلاً محتاجها، بدل ما تسيبها تتنفذ أوتوماتيك.
- **Health check**: لازم يكون فيه endpoint اسمه `/health` في الـ API (لو مش موجود، ممكن نضيفه بسطرين في Program.cs باستخدام `app.MapHealthChecks("/health")`)، عشان نتأكد إن الموقع فعلاً رجع شغال بعد النشر مش بس إن الملفات اتنسخت.
- **`cancel-in-progress: false`**: لو حصل push تاني وفيه نشر شغال بالفعل، الجديد هينتظر الدور بدل ما يقاطع نشر نص شغال ويسيب السيرفر في حالة غير متسقة.

---

## الجزء 5: تأمين appsettings.Production.json (مهم قبل ما تفعّل الـ Workflow)

دلوقتي الملف ده فيه بيانات اتصال حقيقية للسيرفر (Server name, User, Password) وموجود جوا الريبو. لازم:

1. أضفه لملف `.gitignore` عشان مايترفعش بتحديثات مستقبلية:
   ```
   backend/StudentRegistry.API/appsettings.Production.json
   ```
2. احذفه من تتبع Git (بدون ما يتحذف من السيرفر):
   ```bash
   git rm --cached backend/StudentRegistry.API/appsettings.Production.json
   ```
3. اعمل نسخة `appsettings.Production.json.example` بقيم وهمية (placeholders) ترفعها بدل الأصلي، عشان أي حد يفتح الريبو يعرف الشكل المطلوب من غير ما يشوف الباسورد الحقيقي.
4. الملف الحقيقي هيفضل موجود يدويًا على السيرفر فقط داخل `C:\Deploy\StudentRegistry`، ولإن الـ workflow بيستثنيه من `robocopy` (`/XF`)، مش هيتلمس أبدًا عند أي نشر جديد.

---

## الجزء 6: اختبار الفلو كامل قبل الاعتماد عليه

1. اعمل تعديل بسيط غير مؤثر (زي تعديل نص في صفحة)، اعمل commit وادفعه لـ main.
2. روح GitHub → Actions، هتلاقي الـ workflow اشتغل تلقائيًا على الـ Runner بتاعك.
3. تابع كل خطوة، وتأكد إنها خضراء بالكامل.
4. افتح الموقع من المتصفح وتأكد إن التعديل ظهر.
5. جرّب سيناريو Migration حقيقي (إضافة عمود بسيط تجريبي) وتأكد إنه اتطبق على قاعدة البيانات من غير ما يمسح أي بيانات موجودة.
6. (اختياري لكن موصى به) جرّب سيناريو "حذف عمود" قصدًا مرة واحدة على بيئة تجريبية، وتأكد إن بوابة الأمان (`safety_gate`) فعلاً بتوقف النشر وتمنعه من التنفيذ التلقائي.

---

## ملخص الفكرة (Checklist)

- [ ] تثبيت .NET 8 SDK + dotnet-ef + Git for Windows على السيرفر
- [ ] تسجيل GitHub Self-hosted Runner على الريبو وتثبيته كخدمة Windows (`svc.exe install/start`)
- [ ] إضافة label `production` للـ Runner
- [ ] إضافة `PROD_DB_CONNECTION_STRING` في GitHub Secrets
- [ ] إنشاء `.github/workflows/deploy.yml` بالمحتوى أعلاه
- [ ] استثناء `appsettings.Production.json` من Git وربطه بـ `.gitignore`
- [ ] استثناء مجلد الصور المرفوعة والإعدادات من `robocopy /MIR`
- [ ] التأكد من وجود `/health` endpoint للـ Health check
- [ ] اختبار push بسيط، ثم اختبار Migration إضافي، ثم اختبار Migration حذف (للتأكد من بوابة الأمان)
