Silent Hill Origins Arabic V2.0 — Installer source
Hassan RONEX / حسن رونكس
https://linktr.ee/ronex1780

C# Windows Forms source files:
ArabicInstallerV20.Core.cs — patch validation and application
ArabicInstallerV20.UI.cs — interface and entry point
ArabicInstallerV20.Assembly.cs — version and author metadata
ArabicInstallerV20.manifest — Windows application manifest

Compile from this folder in PowerShell on Windows with .NET Framework:

& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:anycpu /optimize+ /codepage:65001 /out:SHO_Arabic_Patcher_V2.0.exe /win32manifest:ArabicInstallerV20.manifest /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll ArabicInstallerV20.Core.cs ArabicInstallerV20.UI.cs ArabicInstallerV20.Assembly.cs

This builds the small patcher. To apply the translation, extract the V2.0
Patch release beside the EXE, preserving its Patches directory structure.
Patch payloads and game files are not included in this source archive.

For a standalone installer, add these compiler arguments before the source files:
/resource:Patches\Full\Arabic_V2.0_Full.shopatch,FullPatch
/resource:Patches\Update_from_V1.1\Arabic_V1.1_to_V2.0.shopatch,UpdatePatch
and change /out to SHO_Arabic_Setup_V2.0.exe.

Preview without applying a patch:
.\SHO_Arabic_Patcher_V2.0.exe --preview preview.png

هذا سورس الأنستولر فقط. بيانات التعريب ليست ضمن الأرشيف.
للتشغيل استخدم ملفات باتش V2.0 مع بنية مجلد Patches الأصلية.
