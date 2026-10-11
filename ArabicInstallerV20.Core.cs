using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

partial class ArabicInstaller : Form
{
    const string RetailHash = "b5f738336fc637a6742419c7183951f6ebb5e852b6a47126002decb18fbecff6";
    const string V11Hash = "b52bab3be17020f93101f5881d6d5fecb7493127621b7d80af84c7ccbca7fe05";
    const string TargetHash = "71faf1fcde36579756f9febcf9d948495093667be47fc24d91c567e750992ddf";
    const long IsoSize = 3074785280L;

    static string Hash(Stream stream, Action<int, string> update, int start, int span, Func<bool> stop)
    {
        stream.Position = 0;
        byte[] buffer = new byte[1048576];
        long done = 0;
        using (var hash = SHA256.Create())
        {
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (stop()) throw new OperationCanceledException();
                hash.TransformBlock(buffer, 0, count, buffer, 0);
                done += count;
                update(start + (int)(done * span / stream.Length), start == 0 ? "فحص ملف اللعبة والتعرّف على الإصدار…" : "التحقق من سلامة نسخة V2.0…");
            }
            hash.TransformFinalBlock(new byte[0], 0, 0);
            return BitConverter.ToString(hash.Hash).Replace("-", "").ToLowerInvariant();
        }
    }

    static Stream OpenPatch(bool fromV11)
    {
        string resource = fromV11 ? "UpdatePatch" : "FullPatch";
        Stream embedded = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        if (embedded != null) return embedded;
        string relative = fromV11 ? @"Patches\Update_from_V1.1\Arabic_V1.1_to_V2.0.shopatch" : @"Patches\Full\Arabic_V2.0_Full.shopatch";
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
        if (!File.Exists(path)) throw new Exception("ملف الباتش غير موجود. فك ضغط الحزمة كاملة مع مجلد Patches، أو استخدم الأنستولر المدمج.");
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    static void Apply(string src, string dst, Action<int, string> update, Func<bool> stop)
    {
        src = Path.GetFullPath(src);
        dst = Path.GetFullPath(dst);
        if (String.Equals(src, dst, StringComparison.OrdinalIgnoreCase) || File.Exists(dst) || Directory.Exists(dst))
            throw new Exception("اختر اسم ملف جديد؛ لن نستبدل الملف الأصلي أو أي ملف موجود.");
        if (!String.Equals(Path.GetExtension(dst), ".iso", StringComparison.OrdinalIgnoreCase))
            throw new Exception("احفظ النسخة الجديدة بامتداد .iso");
        if (!Directory.Exists(Path.GetDirectoryName(dst))) throw new Exception("مجلد الحفظ غير موجود. اختر مجلدًا متاحًا.");
        using (var input = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            if (input.Length != IsoSize) throw new Exception("حجم ملف اللعبة غير مطابق. المطلوب ISO أمريكية أصلية SLUS-21731 أو نسخة V1.1 الرسمية المطابقة.");
            string sourceHash = Hash(input, update, 0, 25, stop);
            if (sourceHash != RetailHash && sourceHash != V11Hash)
                throw new Exception("بصمة ملف اللعبة غير مطابقة. يدعم هذا الإصدار الأصلية الأمريكية SLUS-21731 أو V1.1 الرسمية فقط؛ لا يدعم V1.0 أو نسخ التجربة أو ISO معدلة أخرى.");
            bool fromV11 = sourceHash == V11Hash;
            update(25, fromV11 ? "تم التعرّف على V1.1 — تجهيز التحديث إلى V2.0…" : "تم التعرّف على الأصلية — تجهيز التعريب الكامل…");
            using (Stream patch = OpenPatch(fromV11))
            using (var zip = new ZipArchive(patch, ZipArchiveMode.Read))
            {
                var manifestEntry = zip.GetEntry("manifest.json");
                if (manifestEntry == null || manifestEntry.Length > 16000000) throw new Exception("فهرس الباتش مفقود أو غير صالح.");
                Dictionary<string, object> manifest;
                using (var reader = new StreamReader(manifestEntry.Open()))
                {
                    var json = new JavaScriptSerializer { MaxJsonLength = 16000000 };
                    manifest = json.Deserialize<Dictionary<string, object>>(reader.ReadToEnd());
                }
                if ((string)manifest["format"] != "SHO-AR-COPY-1" || (string)manifest["version"] != "2.0" ||
                    (string)manifest["serial"] != "SLUS-21731" || (string)manifest["source_sha256"] != sourceHash ||
                    (string)manifest["target_sha256"] != TargetHash || Convert.ToInt64(manifest["source_size"]) != IsoSize ||
                    Convert.ToInt64(manifest["target_size"]) != IsoSize)
                    throw new Exception("الباتش لا يطابق الإصدار المطلوب. استخدم حزمة V2.0 كاملة دون خلط ملفات الإصدارات.");
                if (new DriveInfo(Path.GetPathRoot(dst)).AvailableFreeSpace < IsoSize + 67108864L)
                    throw new Exception("المساحة غير كافية في قرص الحفظ. وفّر 3.2 غيغابايت على الأقل.");
                var operations = (IEnumerable)manifest["operations"];
                long total = 0;
                foreach (object item in operations)
                {
                    var op = (IList)item;
                    if (op.Count < 2) throw new Exception("عملية غير صالحة داخل الباتش.");
                    string kind = (string)op[0];
                    long n = Convert.ToInt64(op[op.Count - 1]);
                    if (n < 0 || n > IsoSize) throw new Exception("طول غير صالح داخل الباتش.");
                    if (kind == "copy" && op.Count == 3)
                    {
                        long at = Convert.ToInt64(op[1]);
                        if (at < 0 || at > IsoSize - n) throw new Exception("نطاق نسخ غير صالح.");
                    }
                    else if (kind == "data" && op.Count == 3)
                    {
                        var entry = zip.GetEntry((string)op[1]);
                        if (entry == null || entry.Length != n) throw new Exception("بيانات الباتش ناقصة أو تالفة.");
                    }
                    else if (kind != "zero" || op.Count != 2) throw new Exception("نوع عملية غير صالح.");
                    total = checked(total + n);
                }
                if (total != IsoSize) throw new Exception("الحجم النهائي للباتش غير صحيح.");
                // Exclusively own one random sibling temporary file. Never touch existing files.
                string temporary = dst + "." + Guid.NewGuid().ToString("N") + ".partial";
                bool owned = false;
                try
                {
                    using (var result = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        owned = true;
                        var buffer = new byte[1048576];
                        long written = 0;
                        foreach (object item in operations)
                        {
                            var op = (IList)item;
                            string kind = (string)op[0];
                            long left = Convert.ToInt64(op[op.Count - 1]);
                            Stream read = null;
                            if (kind == "copy") { input.Position = Convert.ToInt64(op[1]); read = input; }
                            else if (kind == "data") read = zip.GetEntry((string)op[1]).Open();
                            try
                            {
                                while (left > 0)
                                {
                                    if (stop()) throw new OperationCanceledException();
                                    int count = (int)Math.Min(buffer.Length, left);
                                    if (read == null) Array.Clear(buffer, 0, count);
                                    else
                                    {
                                        int got = 0;
                                        while (got < count)
                                        {
                                            int next = read.Read(buffer, got, count - got);
                                            if (next == 0) throw new EndOfStreamException("بيانات ملف اللعبة أو الباتش غير مكتملة.");
                                            got += next;
                                        }
                                    }
                                    result.Write(buffer, 0, count);
                                    left -= count; written += count;
                                    update(25 + (int)(written * 50 / IsoSize), fromV11 ? "تحديث V1.1 إلى V2.0 في ملف جديد…" : "إنشاء نسخة V2.0 المعربة…");
                                }
                            }
                            finally { if (kind == "data" && read != null) read.Dispose(); }
                        }
                    }
                    using (var check = File.OpenRead(temporary))
                        if (Hash(check, update, 75, 25, stop) != TargetHash) throw new Exception("فشل فحص النسخة الناتجة. لن يتم اعتماد الملف؛ أعد تنزيل الحزمة وحاول مجددًا.");
                    if (stop()) throw new OperationCanceledException();
                    File.Move(temporary, dst);
                    owned = false;
                    update(100, "اكتمل التركيب والتحقق — نسخة V2.0 جاهزة.");
                }
                finally { if (owned && File.Exists(temporary)) File.Delete(temporary); }
            }
        }
    }
}
