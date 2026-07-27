# خطوات رفع المشروع على السيرفر (Windows Server 2022 + IIS + SQL Server 2017)

المشروع عبارة عن تطبيق **ASP.NET Core 8 (.NET 8)** واحد يخدم الـ API وملفات الواجهة الأمامية من مجلد `wwwroot`، ويحتوي بالفعل على `web.config` جاهز للعمل تحت IIS عبر ASP.NET Core Module V2. لا يوجد مشروع Angular/Node.js منفصل داخل الريبو الحالي — كل شيء مبني ومخدوم من نفس التطبيق.

> ملاحظة: يوجد ملفا `MIGRATION_DEPLOYMENT_GUIDE.md` و`PRODUCTION_OPERATIONS_AND_NFR_GUIDE.md` في جذر المشروع فيهما تفاصيل تكميلية (Migration خطوة بخطوة وتشغيل Production). هذا الملف يركز على خطوات التنصيب الفعلي على السيرفر.

---

## 1. المتطلبات على السيرفر

1. **Windows Server 2022** (متوفر).
2. **IIS** مع تفعيل الميزات التالية من "Server Manager → Add Roles and Features → Web Server (IIS)":
   - Web Server → Common HTTP Features (الأساسيات)
   - Web Server → Application Development (لا حاجة لـ ASP.NET التقليدي)
3. **.NET 8 Hosting Bundle** (وليس مجرد الـ Runtime):
   - نزّل من الموقع الرسمي لمايكروسوفت: "ASP.NET Core Runtime 8.x - Windows Hosting Bundle".
   - هذا الحزمة تثبّت: .NET Runtime + ASP.NET Core Module V2 (ANCM) الذي يربط IIS بتطبيق dotnet.
   - بعد التثبيت: `iisreset` لإعادة تشغيل IIS حتى يتعرف على الموديول الجديد.
4. **SQL Server 2017** (متوفر لديك بالفعل).
5. صلاحية RDP على السيرفر لتنفيذ الخطوات.
6. **LibreOffice** (إلزامي — ليس مكتبة NuGet بل برنامج يُثبَّت على السيرفر نفسه، انظر القسم 2.5 بالأسفل).

للتأكد من أن الـ Hosting Bundle مثبّت بشكل صحيح:
```powershell
dotnet --info
```
ويجب أن تظهر قائمة "ASP.NET Core Shared Framework" ضمن Runtimes.

---

## 2. تجهيز قاعدة البيانات (SQL Server)

1. افتح **SQL Server Management Studio (SSMS)** على السيرفر واتصل بالـ instance.
2. أنشئ قاعدة بيانات جديدة، مثلاً:
   ```sql
   CREATE DATABASE StudentRegistryDb;
   ```
3. نفّذ ملف السكيما الموجود في الريبو:
   - المسار: [database/schema.sql](database/schema.sql)
   - افتحه داخل SSMS وشغّله على قاعدة `StudentRegistryDb` لإنشاء الجداول.
4. أنشئ مستخدم SQL Login مخصص للتطبيق (لا تستخدم `sa` في الإنتاج):
   ```sql
   CREATE LOGIN StudentRegistryAppUser WITH PASSWORD = 'كلمة_مرور_قوية_هنا';
   USE StudentRegistryDb;
   CREATE USER StudentRegistryAppUser FOR LOGIN StudentRegistryAppUser;
   ALTER ROLE db_datareader ADD MEMBER StudentRegistryAppUser;
   ALTER ROLE db_datawriter ADD MEMBER StudentRegistryAppUser;
   ALTER ROLE db_ddladmin  ADD MEMBER StudentRegistryAppUser; -- إذا كان التطبيق يشغّل Migrations تلقائيًا
   ```
5. فعّل **SQL Server + Windows Authentication mode** (Mixed Mode) إن كنت ستستخدم SQL Login، وأعد تشغيل خدمة SQL Server بعدها.
6. تأكد أن **TCP/IP protocol** مفعّل في "SQL Server Configuration Manager" وأن الخدمة تستمع على البورت 1433 (أو البورت الذي تريده)، وافتح البورت في الجدار الناري (Windows Defender Firewall) إذا كان الاتصال من سيرفر آخر.

---

## 2.5. تثبيت LibreOffice (مطلوب لتصدير PDF) — هام جدًا

لاحظت أن ميزة تصدير بيانات الطالب PDF (`GET /api/students/{id}/export/pdf`) لا تستخدم مكتبة PDF عبر NuGet، بل تعمل عن طريق تشغيل **LibreOffice في وضع headless** كعملية خارجية لتحويل ملف Excel المولّد إلى PDF (الكود في [StudentExcelExportService.cs:99-176](backend/StudentRegistry.Infrastructure/Export/StudentExcelExportService.cs)). هذا يعني:

- **يجب تثبيت LibreOffice كاملاً على السيرفر نفسه** (وليس فقط على جهازك)، وإلا فستفشل كل عمليات تصدير PDF بخطأ "تعذر تشغيل LibreOffice لتحويل الملف إلى PDF".
- الكود يبحث تلقائيًا عن الملف التنفيذي في أحد هذين المسارين الافتراضيين على ويندوز:
  ```
  C:\Program Files\LibreOffice\program\soffice.exe
  C:\Program Files (x86)\LibreOffice\program\soffice.exe
  ```
- خطوات التثبيت:
  1. نزّل **LibreOffice** (نسخة ويندوز الكاملة، وليس Portable) من الموقع الرسمي وثبّته على السيرفر بالمسار الافتراضي.
  2. لا حاجة لأي إعداد إضافي إذا كان التثبيت في أحد المسارين أعلاه.
  3. إن ثبّته في مسار مختلف، أضف المفتاح التالي في `appsettings.Production.json` (أو Environment Variable):
     ```json
     "LibreOffice": {
       "ExecutablePath": "D:\\Apps\\LibreOffice\\program\\soffice.exe"
     }
     ```
- **صلاحيات مهمة**: التطبيق يشغّل `soffice.exe` من تحت هوية Application Pool Identity (`IIS AppPool\StudentRegistryPool`)، ويحتاج كتابة ملفات مؤقتة داخل `Path.GetTempPath()` الخاص بنفس الهوية. تأكد أن هذا الحساب لديه صلاحية Read/Write/Execute على:
  - مجلد LibreOffice نفسه (قراءة/تشغيل فقط)
  - مجلد Temp الخاص بحساب Application Pool (عادة `C:\Windows\Temp` أو `C:\Windows\ServiceProfiles\...\AppData\Local\Temp`)
- بعد التثبيت، جرّب تشغيل يدوي تأكيدي من موجه أوامر بصلاحيات مشابهة لحساب IIS:
  ```powershell
  & "C:\Program Files\LibreOffice\program\soffice.exe" --headless --convert-to pdf --outdir C:\Temp C:\Temp\test.xlsx
  ```
  إذا نجح هذا الأمر يدويًا، فسيعمل تصدير PDF من التطبيق أيضًا.
- المهلة الزمنية للتحويل محددة بـ 60 ثانية داخل الكود (`WaitForExit(60_000)`)؛ لو السيرفر بطيء جدًا قد تحتاج لمراجعة هذا الرقم مستقبلاً، لكن لا داعي لتغييره حاليًا.

---

## 3. بناء (Publish) المشروع

على جهازك (أو على السيرفر نفسه إذا كان فيه SDK):

```bash
cd backend
dotnet publish StudentRegistry.API/StudentRegistry.API.csproj -c Release -o ./publish
```

هذا الأمر سينتج مجلد `publish` يحتوي على:
- `StudentRegistry.API.dll` والملفات المرفقة
- `web.config` (يُنسخ تلقائيًا لأنه موجود بالمشروع)
- `wwwroot` (ملفات الواجهة الأمامية الثابتة إن وجدت)

انسخ محتوى مجلد `publish` بالكامل إلى السيرفر، مثلاً إلى:
```
C:\inetpub\wwwroot\StudentRegistry
```

---

## 4. ضبط إعدادات الاتصال بقاعدة البيانات (appsettings.Production.json)

الملف [backend/StudentRegistry.API/appsettings.Production.json](backend/StudentRegistry.API/appsettings.Production.json) يحتوي حاليًا على بيانات اتصال placeholder (سيرفر ومستخدم `sa` وكلمة مرور ظاهرة). **يجب تعديله قبل الرفع** ليطابق بيانات سيرفرك الفعلي:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=اسم_السيرفر_أو_IP,1433;Database=StudentRegistryDb;User Id=StudentRegistryAppUser;Password=كلمة_المرور_الحقيقية;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

⚠️ **مهم جدًا (أمان):**
- لا ترفع كلمة المرور الحقيقية داخل الملف الموجود في Git. الأفضل تعديل النسخة الموجودة فعليًا على السيرفر فقط بعد نسخها من `publish`، أو استخدام **Environment Variables** بدلاً من الملف:
  ```powershell
  [System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=...;...", "Machine")
  ```
  ثم إعادة تشغيل Application Pool. هذا يمنع تسريب كلمة المرور في ملفات الكود.

---

## 5. إنشاء الموقع في IIS

1. افتح **IIS Manager** (`inetmgr`).
2. **Application Pools → Add Application Pool**:
   - الاسم: `StudentRegistryPool`
   - .NET CLR version: **No Managed Code** (لأن ASP.NET Core لا يستخدم CLR الخاص بـ IIS، بل يعمل كعملية مستقلة عبر ANCM).
   - Managed pipeline mode: Integrated
3. **Sites → Add Website**:
   - Site name: `StudentRegistry`
   - Application pool: اختر `StudentRegistryPool`
   - Physical path: `C:\inetpub\wwwroot\StudentRegistry` (المكان الذي نسخت فيه ملفات publish)
   - Binding: مثلاً `http` على البورت 80، أو `https` على 443 إذا لديك شهادة SSL (موصى به).
4. تأكد أن **Identity** الخاص بـ Application Pool (افتراضيًا `ApplicationPoolIdentity`) لديه صلاحية **قراءة/كتابة** على مجلد الموقع (خصوصًا مجلد `logs` و`wwwroot` لو فيه رفع ملفات/صور):
   - كليك يمين على المجلد → Properties → Security → Edit → Add → اكتب `IIS AppPool\StudentRegistryPool` → امنحه Modify.
5. إذا كنت ستتصل بقاعدة بيانات على سيرفر آخر بـ SQL Login فلا داعي لصلاحيات Windows إضافية؛ أما لو ستستخدم Windows Authentication (`Trusted_Connection=True` كما في appsettings.json الافتراضي)، فيجب منح حساب Application Pool Identity صلاحية دخول على SQL Server، وهذا أعقد على IIS الافتراضي — لذلك يُفضّل استخدام **SQL Login** كما بالخطوة 2 بدلاً من Windows Authentication.

---

## 6. تفعيل السجلات (Logs) للتشخيص عند الحاجة

في `web.config` الموجود بالمشروع، السطر:
```xml
<aspNetCore processPath="dotnet" arguments=".\StudentRegistry.API.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
```
لو واجهت خطأ 500.30 أو التطبيق لا يعمل، غيّر `stdoutLogEnabled="false"` إلى `true` مؤقتًا، وأنشئ مجلد `logs` داخل مجلد الموقع، ثم راجع الملف الناتج لمعرفة سبب الخطأ. أعده إلى `false` بعد الانتهاء من التشخيص (لتفادي امتلاء القرص).

---

## 7. اختبار التشغيل

1. من متصفح على نفس السيرفر: `http://localhost` أو البورت الذي حددته.
2. من جهاز آخر بالشبكة: `http://اسم_أو_IP_السيرفر`.
3. تحقق من أن الصفحات تُحمّل، وأن أي عملية تتفاعل مع قاعدة البيانات (تسجيل دخول، حفظ بيانات طالب...) تعمل بدون أخطاء 500.
4. راجع Event Viewer (Windows Logs → Application) لو ظهرت مشاكل غير واضحة من السجلات.

---

## 8. HTTPS (موصى به بشدة للإنتاج)

- احصل على شهادة SSL (من جهة موثوقة أو Let's Encrypt عبر أداة مثل `win-acme` المتوافقة مع IIS).
- في IIS Manager: Site → Bindings → Add → Type: https → اختر الشهادة.
- فعّل إعادة التوجيه من HTTP إلى HTTPS إذا لزم عبر URL Rewrite module.

---

## 9. ملخص الخطوات السريعة (Checklist)

- [ ] تثبيت .NET 8 Hosting Bundle على السيرفر + `iisreset`
- [ ] تثبيت LibreOffice على السيرفر (لتصدير PDF) + التأكد من صلاحيات Application Pool Identity على مجلد Temp
- [ ] إنشاء قاعدة البيانات وتنفيذ [database/schema.sql](database/schema.sql)
- [ ] إنشاء SQL Login مخصص للتطبيق (وليس sa)
- [ ] `dotnet publish -c Release -o ./publish`
- [ ] نسخ محتوى `publish` إلى مجلد على السيرفر (مثلاً `C:\inetpub\wwwroot\StudentRegistry`)
- [ ] تعديل `appsettings.Production.json` ببيانات الاتصال الحقيقية (أو استخدام Environment Variable)
- [ ] إنشاء Application Pool بوضع "No Managed Code"
- [ ] إنشاء Site في IIS يشير لمجلد النشر ويستخدم الـ Pool
- [ ] صلاحيات NTFS لحساب Application Pool على مجلد الموقع
- [ ] اختبار الوصول عبر المتصفح + مراجعة السجلات عند وجود مشاكل
- [ ] (اختياري لكن موصى به) تفعيل HTTPS
