using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

partial class ArabicInstaller : Form
{
    static readonly Color Ink = Color.FromArgb(19, 23, 24);
    static readonly Color Card = Color.FromArgb(29, 34, 35);
    static readonly Color Field = Color.FromArgb(21, 26, 27);
    static readonly Color Line = Color.FromArgb(57, 65, 64);
    static readonly Color Bone = Color.FromArgb(235, 232, 219);
    static readonly Color Muted = Color.FromArgb(159, 168, 164);
    static readonly Color Sage = Color.FromArgb(173, 190, 169);
    static readonly Color Blood = Color.FromArgb(182, 104, 95);

    readonly TextBox sourcePath = new TextBox();
    readonly TextBox destinationPath = new TextBox();
    readonly Button chooseSource = new ThemedButton();
    readonly Button chooseDestination = new ThemedButton();
    readonly Button buildButton = new ThemedButton();
    readonly Button cancelButton = new ThemedButton();
    readonly Button folderButton = new ThemedButton();
    readonly Button discordButton = new ThemedButton();
    readonly Label statusLabel = new Label();
    readonly Label progressLabel = new Label();
    readonly Label socialFeedback = new Label();
    readonly ProgressTrack progressTrack = new ProgressTrack();
    readonly BackgroundWorker buildWorker = new BackgroundWorker();
    readonly Timer feedbackTimer = new Timer();
    string completedOutput;
    string automaticOutput;

    ArabicInstaller()
    {
        Text = "Silent Hill Origins | التعريب العربي V2.0 — حسن رونكس";
        ClientSize = new Size(940, 700);
        MinimumSize = new Size(930, 730);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Font = UiFont(10F, FontStyle.Regular);
        BackColor = Ink;
        ForeColor = Bone;
        DoubleBuffered = true;

        TableLayoutPanel page = BuildLayout(1, 4);
        page.Padding = new Padding(28, 24, 28, 16);
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        Controls.Add(page);
        page.Controls.Add(CreateHeader(), 0, 0);

        TableLayoutPanel body = BuildLayout(3, 1);
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 242));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.Controls.Add(body, 0, 2);
        body.Controls.Add(CreateSidebar(), 0, 0);

        TableLayoutPanel steps = BuildLayout(1, 5);
        steps.RowStyles.Add(new RowStyle(SizeType.Absolute, 138));
        steps.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        steps.RowStyles.Add(new RowStyle(SizeType.Absolute, 138));
        steps.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        steps.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(steps, 2, 0);
        steps.Controls.Add(CreatePathStep("01", "اختر نسخة اللعبة", "USA / SLUS-21731 الأصلية أو التعريب الرسمي V1.1", sourcePath, chooseSource, "اختيار الملف"), 0, 0);
        steps.Controls.Add(CreatePathStep("02", "حدّد مكان النسخة الجديدة", "ملف ISO مستقل؛ لن يُستبدل أي ملف موجود.", destinationPath, chooseDestination, "مكان الحفظ"), 0, 2);
        steps.Controls.Add(CreateBuildStep(), 0, 4);

        Label footer = TextLabel("تعريب جماهيري غير رسمي • Silent Hill Origins • PlayStation 2", 8.5F, Muted, ContentAlignment.MiddleCenter, true);
        page.Controls.Add(footer, 0, 3);
        WireEvents();
    }

    static Font UiFont(float size, FontStyle style)
    {
        return new Font("Segoe UI", size, style, GraphicsUnit.Point);
    }

    static TableLayoutPanel BuildLayout(int columns, int rows)
    {
        TableLayoutPanel panel = new TableLayoutPanel();
        panel.ColumnCount = columns;
        panel.RowCount = rows;
        panel.Dock = DockStyle.Fill;
        panel.Margin = Padding.Empty;
        panel.Padding = Padding.Empty;
        panel.BackColor = Color.Transparent;
        panel.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        if (columns == 1) panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        if (rows == 1) panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return panel;
    }

    static Label TextLabel(string text, float size, Color color, ContentAlignment alignment, bool rtl)
    {
        Label label = new Label();
        label.Text = text;
        label.Font = UiFont(size, FontStyle.Regular);
        label.ForeColor = color;
        label.BackColor = Color.Transparent;
        label.Dock = DockStyle.Fill;
        label.Margin = Padding.Empty;
        label.TextAlign = rtl ? MirrorAlignment(alignment) : alignment;
        label.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
        label.UseCompatibleTextRendering = false;
        return label;
    }

    static ContentAlignment MirrorAlignment(ContentAlignment alignment)
    {
        switch (alignment)
        {
            case ContentAlignment.TopLeft: return ContentAlignment.TopRight;
            case ContentAlignment.TopRight: return ContentAlignment.TopLeft;
            case ContentAlignment.MiddleLeft: return ContentAlignment.MiddleRight;
            case ContentAlignment.MiddleRight: return ContentAlignment.MiddleLeft;
            case ContentAlignment.BottomLeft: return ContentAlignment.BottomRight;
            case ContentAlignment.BottomRight: return ContentAlignment.BottomLeft;
            default: return alignment;
        }
    }

    static void StyleButton(Button button, string text, bool primary)
    {
        button.Text = text;
        button.Font = UiFont(9.5F, primary ? FontStyle.Bold : FontStyle.Regular);
        button.Dock = DockStyle.Fill;
        button.Margin = Padding.Empty;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Line;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(191, 207, 185) : Color.FromArgb(44, 52, 51);
        button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(151, 172, 148) : Color.FromArgb(50, 60, 57);
        button.BackColor = primary ? Sage : Card;
        button.ForeColor = primary ? Ink : Bone;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        button.RightToLeft = RightToLeft.Yes;
        ThemedButton themed = button as ThemedButton;
        if (themed != null) themed.IsPrimary = primary;
    }

    Control CreateHeader()
    {
        TableLayoutPanel header = BuildLayout(3, 1);
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));

        TableLayoutPanel brand = BuildLayout(1, 3);
        brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 19));
        brand.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        brand.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        brand.Controls.Add(TextLabel("R O N E X   /   A R A B I C   P R O J E C T", 8F, Sage, ContentAlignment.MiddleLeft, false), 0, 0);
        Label title = TextLabel("SILENT HILL", 30F, Bone, ContentAlignment.MiddleLeft, false);
        title.Font = new Font("Georgia", 30F, FontStyle.Regular, GraphicsUnit.Point);
        brand.Controls.Add(title, 0, 1);
        brand.Controls.Add(TextLabel("O R I G I N S", 13F, Muted, ContentAlignment.TopLeft, false), 0, 2);
        header.Controls.Add(brand, 0, 0);

        TableLayoutPanel author = BuildLayout(1, 3);
        author.Padding = new Padding(0, 6, 0, 6);
        author.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
        author.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
        author.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        author.Controls.Add(TextLabel("التعريب والتطوير والخرائط", 9F, Muted, ContentAlignment.MiddleRight, true), 0, 0);
        Label arabicName = TextLabel("حسن رونكس", 22F, Bone, ContentAlignment.MiddleRight, true);
        arabicName.Font = UiFont(22F, FontStyle.Bold);
        author.Controls.Add(arabicName, 0, 1);
        author.Controls.Add(TextLabel("Hassan RONEX", 10F, Sage, ContentAlignment.MiddleRight, false), 0, 2);
        header.Controls.Add(author, 2, 0);
        return header;
    }

    Control CreateSidebar()
    {
        RoundedSurface surface = new RoundedSurface(Card, Line);
        surface.Padding = new Padding(18, 15, 18, 12);
        TableLayoutPanel content = BuildLayout(1, 10);
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        surface.Controls.Add(content);

        TableLayoutPanel edition = BuildLayout(2, 1);
        edition.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        edition.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Label version = TextLabel("2.0", 32F, Sage, ContentAlignment.TopLeft, false);
        version.Font = UiFont(32F, FontStyle.Regular);
        edition.Controls.Add(version, 0, 0);
        edition.Controls.Add(TextLabel("الإصدار العربي\r\nARABIC EDITION", 8.5F, Bone, ContentAlignment.MiddleRight, true), 1, 0);
        content.Controls.Add(edition, 0, 0);
        content.Controls.Add(CreateNotice("نسختك الأصلية محفوظة", "يُفحص ملف المصدر، ثم تُبنى نسخة جديدة ويُتحقق من سلامتها.", Sage), 0, 1);
        content.Controls.Add(CreateNotice("قبل أن تبدأ اللعب", "احتفظ بنسخة من بطاقة الذاكرة. استخدم حفظ اللعبة؛ تجنّب الحفظ السريع القديم.", Blood), 0, 2);

        Label contactTitle = TextLabel("تواصل مع صاحب التعريب", 9F, Muted, ContentAlignment.BottomRight, true);
        contactTitle.Padding = new Padding(0, 0, 0, 12);
        content.Controls.Add(contactTitle, 0, 3);
        Button linktree = new ThemedButton();
        StyleButton(linktree, "Linktree  /  ronex1780   ↗", false);
        linktree.RightToLeft = RightToLeft.No;
        linktree.Font = UiFont(9F, FontStyle.Regular);
        linktree.Click += delegate { OpenExternal("https://linktr.ee/ronex1780"); };
        content.Controls.Add(linktree, 0, 4);
        Button instagram = new ThemedButton();
        StyleButton(instagram, "Instagram  /  2kb_20   ↗", false);
        instagram.RightToLeft = RightToLeft.No;
        instagram.Font = UiFont(9F, FontStyle.Regular);
        instagram.Click += delegate { OpenExternal("https://www.instagram.com/2kb_20/"); };
        content.Controls.Add(instagram, 0, 6);
        StyleButton(discordButton, "Discord  /  f.r5   ·   نسخ", false);
        discordButton.RightToLeft = RightToLeft.No;
        discordButton.Font = UiFont(9F, FontStyle.Regular);
        content.Controls.Add(discordButton, 0, 8);
        socialFeedback.Dock = DockStyle.Fill;
        socialFeedback.Font = UiFont(8F, FontStyle.Regular);
        socialFeedback.ForeColor = Sage;
        socialFeedback.TextAlign = ContentAlignment.MiddleCenter;
        socialFeedback.RightToLeft = RightToLeft.Yes;
        socialFeedback.Margin = Padding.Empty;
        content.Controls.Add(socialFeedback, 0, 9);
        return surface;
    }

    Control CreateNotice(string title, string text, Color accent)
    {
        TableLayoutPanel notice = BuildLayout(1, 2);
        notice.Padding = new Padding(0, 7, 0, 7);
        notice.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
        notice.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Label heading = TextLabel(title, 10F, accent, ContentAlignment.MiddleRight, true);
        heading.Font = UiFont(10F, FontStyle.Bold);
        notice.Controls.Add(heading, 0, 0);
        notice.Controls.Add(TextLabel(text, 9F, Muted, ContentAlignment.TopRight, true), 0, 1);
        return notice;
    }

    static Control StepHeading(string number, string title)
    {
        TableLayoutPanel heading = BuildLayout(2, 1);
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 43));
        Label text = TextLabel(title, 13F, Bone, ContentAlignment.MiddleRight, true);
        text.Font = UiFont(13F, FontStyle.Bold);
        heading.Controls.Add(text, 0, 0);
        Label numeral = TextLabel(number, 14F, Sage, ContentAlignment.MiddleRight, false);
        heading.Controls.Add(numeral, 1, 0);
        return heading;
    }

    Control CreatePathStep(string number, string title, string hint, TextBox path, Button choose, string chooseText)
    {
        RoundedSurface surface = new RoundedSurface(Card, Line);
        surface.Padding = new Padding(16, 11, 16, 15);
        TableLayoutPanel content = BuildLayout(1, 3);
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        surface.Controls.Add(content);
        content.Controls.Add(StepHeading(number, title), 0, 0);
        content.Controls.Add(TextLabel(hint, 9.2F, Muted, ContentAlignment.TopRight, true), 0, 1);

        TableLayoutPanel files = BuildLayout(3, 1);
        files.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
        files.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 9));
        files.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        StyleButton(choose, chooseText, false);
        files.Controls.Add(choose, 0, 0);
        RoundedSurface field = new RoundedSurface(Field, Line);
        field.Radius = 5;
        field.Padding = new Padding(9, 9, 9, 5);
        path.BorderStyle = BorderStyle.None;
        path.ReadOnly = true;
        path.BackColor = Field;
        path.ForeColor = Bone;
        path.Font = UiFont(9F, FontStyle.Regular);
        path.Dock = DockStyle.Fill;
        path.RightToLeft = RightToLeft.No;
        path.ShortcutsEnabled = true;
        path.TabStop = true;
        field.Controls.Add(path);
        files.Controls.Add(field, 2, 0);
        content.Controls.Add(files, 0, 2);
        return surface;
    }

    Control CreateBuildStep()
    {
        RoundedSurface surface = new RoundedSurface(Card, Line);
        surface.Padding = new Padding(16, 11, 16, 14);
        TableLayoutPanel content = BuildLayout(1, 5);
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 39));
        surface.Controls.Add(content);
        content.Controls.Add(StepHeading("03", "ابنِ النسخة وتحقّق منها"), 0, 0);

        TableLayoutPanel state = BuildLayout(2, 1);
        state.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 51));
        state.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        progressLabel.Text = "0%";
        progressLabel.Font = UiFont(12F, FontStyle.Regular);
        progressLabel.ForeColor = Sage;
        progressLabel.Dock = DockStyle.Fill;
        progressLabel.Margin = Padding.Empty;
        progressLabel.TextAlign = ContentAlignment.MiddleLeft;
        state.Controls.Add(progressLabel, 0, 0);
        statusLabel.Text = "اختر المصدر ومكان الحفظ للبدء.";
        statusLabel.ForeColor = Muted;
        statusLabel.Font = UiFont(9.5F, FontStyle.Regular);
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Margin = Padding.Empty;
        statusLabel.RightToLeft = RightToLeft.Yes;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        state.Controls.Add(statusLabel, 1, 0);
        content.Controls.Add(state, 0, 1);
        progressTrack.Dock = DockStyle.Fill;
        progressTrack.Margin = Padding.Empty;
        content.Controls.Add(progressTrack, 0, 2);

        TableLayoutPanel actions = BuildLayout(5, 1);
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 121));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 9));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        StyleButton(folderButton, "فتح مجلد الناتج", false);
        folderButton.Enabled = false;
        StyleButton(cancelButton, "إلغاء", false);
        cancelButton.Enabled = false;
        StyleButton(buildButton, "إنشاء النسخة العربية", true);
        actions.Controls.Add(folderButton, 0, 0);
        actions.Controls.Add(cancelButton, 2, 0);
        actions.Controls.Add(buildButton, 4, 0);
        content.Controls.Add(actions, 0, 4);
        return surface;
    }

    void WireEvents()
    {
        chooseSource.Click += delegate
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "PlayStation 2 ISO (*.iso)|*.iso";
                dialog.Title = "اختر نسخة USA الأصلية أو التعريب الرسمي V1.1";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                sourcePath.Text = dialog.FileName;
                if (String.IsNullOrEmpty(destinationPath.Text) || String.Equals(destinationPath.Text, automaticOutput, StringComparison.OrdinalIgnoreCase))
                {
                    automaticOutput = SuggestedOutput(dialog.FileName);
                    destinationPath.Text = automaticOutput;
                }
                ResetReadyState();
            }
        };
        chooseDestination.Click += delegate
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PlayStation 2 ISO (*.iso)|*.iso";
                dialog.Title = "اختر اسمًا جديدًا للنسخة العربية";
                dialog.DefaultExt = "iso";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = false;
                dialog.CheckPathExists = true;
                dialog.FileName = String.IsNullOrEmpty(destinationPath.Text) ? "Silent Hill Origins Arabic V2.0.iso" : Path.GetFileName(destinationPath.Text);
                if (!String.IsNullOrEmpty(destinationPath.Text)) dialog.InitialDirectory = Path.GetDirectoryName(destinationPath.Text);
                else if (!String.IsNullOrEmpty(sourcePath.Text)) dialog.InitialDirectory = Path.GetDirectoryName(sourcePath.Text);
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (File.Exists(dialog.FileName) || Directory.Exists(dialog.FileName))
                {
                    ShowProblem("هذا الاسم مستخدم بالفعل. اختر اسمًا آخر؛ لا يمكن استبدال الملفات الموجودة.");
                    return;
                }
                destinationPath.Text = dialog.FileName;
                automaticOutput = null;
                ResetReadyState();
            }
        };
        buildButton.Click += delegate { BeginBuild(); };
        cancelButton.Click += delegate { RequestCancel(); };
        folderButton.Click += delegate
        {
            if (!String.IsNullOrEmpty(completedOutput)) OpenExternal(Path.GetDirectoryName(completedOutput));
        };
        discordButton.Click += delegate
        {
            try { Clipboard.SetText("f.r5"); SocialMessage("تم نسخ f.r5 إلى الحافظة"); }
            catch (Exception) { SocialMessage("تعذّر النسخ. المعرّف: f.r5"); }
        };
        feedbackTimer.Interval = 4500;
        feedbackTimer.Tick += delegate { feedbackTimer.Stop(); socialFeedback.Text = ""; };

        buildWorker.WorkerReportsProgress = true;
        buildWorker.WorkerSupportsCancellation = true;
        buildWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
        {
            string[] paths = (string[])e.Argument;
            int lastProgress = -1;
            string lastStatus = null;
            try
            {
                Apply(paths[0], paths[1], delegate(int percentage, string message)
                {
                    percentage = Math.Max(0, Math.Min(100, percentage));
                    if (percentage != lastProgress || !String.Equals(message, lastStatus, StringComparison.Ordinal))
                    {
                        lastProgress = percentage;
                        lastStatus = message;
                        buildWorker.ReportProgress(percentage, message);
                    }
                }, delegate { return buildWorker.CancellationPending; });
                e.Result = paths[1];
            }
            catch (OperationCanceledException) { e.Cancel = true; }
        };
        buildWorker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
        {
            progressTrack.Value = e.ProgressPercentage;
            progressLabel.Text = e.ProgressPercentage.ToString() + "%";
            if (!buildWorker.CancellationPending) statusLabel.Text = Convert.ToString(e.UserState);
        };
        buildWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
        {
            SetBusy(false);
            if (e.Cancelled)
            {
                statusLabel.ForeColor = Muted;
                statusLabel.Text = "تم الإلغاء بأمان؛ ملف المصدر محفوظ.";
                progressTrack.Value = 0;
                progressLabel.Text = "—";
            }
            else if (e.Error != null)
            {
                statusLabel.ForeColor = Blood;
                statusLabel.Text = "تعذّر إنشاء النسخة. ملف المصدر محفوظ.";
                progressLabel.Text = "—";
                ShowProblem(e.Error.Message);
            }
            else
            {
                completedOutput = (string)e.Result;
                progressTrack.Value = 100;
                progressLabel.Text = "100%";
                statusLabel.ForeColor = Sage;
                statusLabel.Text = "النسخة العربية جاهزة. اكتمل الإنشاء والتحقّق بنجاح.";
                folderButton.Enabled = true;
                buildButton.Text = "إنشاء نسخة أخرى";
            }
        };
        FormClosing += delegate(object sender, FormClosingEventArgs e)
        {
            if (!buildWorker.IsBusy) return;
            e.Cancel = true;
            RequestCancel();
        };
        FormClosed += delegate { feedbackTimer.Dispose(); buildWorker.Dispose(); };
    }

    static string SuggestedOutput(string input)
    {
        string directory = Path.GetDirectoryName(Path.GetFullPath(input));
        string name = Path.Combine(directory, "Silent Hill Origins Arabic V2.0.iso");
        int suffix = 2;
        while (File.Exists(name) || Directory.Exists(name))
        {
            name = Path.Combine(directory, "Silent Hill Origins Arabic V2.0 (" + suffix.ToString() + ").iso");
            suffix++;
        }
        return name;
    }

    void ResetReadyState()
    {
        completedOutput = null;
        folderButton.Enabled = false;
        buildButton.Text = "إنشاء النسخة العربية";
        progressTrack.Value = 0;
        progressLabel.Text = "0%";
        statusLabel.ForeColor = Muted;
        statusLabel.Text = "جاهز للبدء. سيُفحص المصدر قبل إنشاء النسخة.";
    }

    void BeginBuild()
    {
        if (buildWorker.IsBusy) return;
        try
        {
            if (String.IsNullOrWhiteSpace(sourcePath.Text) || !File.Exists(sourcePath.Text))
                throw new IOException("اختر ملف ISO للعبة أولًا.");
            if (String.IsNullOrWhiteSpace(destinationPath.Text))
                throw new IOException("حدّد مكان حفظ النسخة العربية أولًا.");
            string input = Path.GetFullPath(sourcePath.Text);
            string output = Path.GetFullPath(destinationPath.Text);
            if (String.Equals(input, output, StringComparison.OrdinalIgnoreCase))
                throw new IOException("يجب أن يختلف ملف الناتج عن المصدر. اختر اسمًا جديدًا.");
            if (File.Exists(output) || Directory.Exists(output))
                throw new IOException("ملف الناتج موجود بالفعل. اختر اسمًا جديدًا لحفظ النسخة.");
            if (!Directory.Exists(Path.GetDirectoryName(output)))
                throw new IOException("مجلد الحفظ غير موجود. اختر مجلدًا متاحًا.");
            completedOutput = null;
            progressTrack.Value = 0;
            progressLabel.Text = "0%";
            statusLabel.ForeColor = Muted;
            statusLabel.Text = "جارٍ فحص نسخة اللعبة…";
            SetBusy(true);
            buildWorker.RunWorkerAsync(new string[] { input, output });
        }
        catch (Exception ex) { SetBusy(false); ShowProblem(ex.Message); }
    }

    void SetBusy(bool busy)
    {
        chooseSource.Enabled = !busy;
        chooseDestination.Enabled = !busy;
        sourcePath.Enabled = !busy;
        destinationPath.Enabled = !busy;
        buildButton.Enabled = !busy;
        cancelButton.Enabled = busy;
        folderButton.Enabled = !busy && !String.IsNullOrEmpty(completedOutput);
        buildButton.Text = busy ? "جارٍ إنشاء النسخة…" : "إنشاء النسخة العربية";
        UseWaitCursor = false;
    }

    void RequestCancel()
    {
        if (!buildWorker.IsBusy) return;
        buildWorker.CancelAsync();
        cancelButton.Enabled = false;
        statusLabel.ForeColor = Muted;
        statusLabel.Text = "جارٍ الإلغاء وتنظيف الملف المؤقت… يرجى الانتظار.";
    }

    void ShowProblem(string message)
    {
        MessageBox.Show(this, message, "التعريب العربي V2.0", MessageBoxButtons.OK, MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }

    void OpenExternal(string target)
    {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
        catch (Exception) { ShowProblem("تعذّر فتح الوجهة تلقائيًا. يمكنك فتحها يدويًا:\r\n" + target); }
    }

    void SocialMessage(string message)
    {
        socialFeedback.Text = message;
        feedbackTimer.Stop();
        feedbackTimer.Start();
    }

    static void WriteTestReport(string path, string text)
    {
        try { File.WriteAllText(path, text, new UTF8Encoding(false)); }
        catch (Exception ex) { try { Console.Error.WriteLine("Cannot write report: " + ex.Message); } catch (Exception) { } }
    }

    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length == 3 && (args[0] == "--test" || args[0] == "--test-cancel"))
        {
            bool cancellationTest = args[0] == "--test-cancel";
            bool cancellationRequested = false;
            int greatestProgress = 0;
            string lastMessage = "";
            string[] previousPartialFiles = new string[0];
            long originalSize = -1;
            DateTime originalWriteTime = DateTime.MinValue;
            try
            {
                if (cancellationTest)
                {
                    FileInfo original = new FileInfo(args[1]);
                    originalSize = original.Length;
                    originalWriteTime = original.LastWriteTimeUtc;
                    previousPartialFiles = MatchingPartialFiles(args[2]);
                }
                Apply(args[1], args[2], delegate(int n, string message)
                {
                    greatestProgress = Math.Max(greatestProgress, n);
                    lastMessage = message;
                    if (cancellationTest && n >= 35) cancellationRequested = true;
                }, delegate { return cancellationRequested; });
                if (cancellationTest)
                    throw new InvalidOperationException("Cancellation test failed: Apply completed without acknowledging cancellation.");
                WriteTestReport(args[2] + ".verification.txt", "PASS: output SHA-256 verified by Apply.\r\nOutput: " + Path.GetFullPath(args[2]) + "\r\nProgress: " + greatestProgress + "\r\nStatus: " + lastMessage + "\r\n");
                return 0;
            }
            catch (OperationCanceledException ex)
            {
                if (cancellationTest && cancellationRequested && !File.Exists(args[2]))
                {
                    try
                    {
                        foreach (string partial in MatchingPartialFiles(args[2]))
                            if (Array.IndexOf(previousPartialFiles, partial) < 0)
                                throw new IOException("Cancellation cleanup failed: a new partial output remains: " + partial);
                        FileInfo original = new FileInfo(args[1]);
                        if (!original.Exists || original.Length != originalSize || original.LastWriteTimeUtc != originalWriteTime)
                            throw new IOException("Cancellation test failed: source metadata changed.");
                        WriteTestReport(args[2] + ".cancellation.txt", "PASS: cancellation acknowledged; final output does not exist; no new partial files remain.\r\nSource size and modification time unchanged.\r\nCancellation requested at progress >=35.\r\nMaximum progress: " + greatestProgress + "\r\nStatus: " + lastMessage + "\r\n");
                        return 0;
                    }
                    catch (Exception cleanupError)
                    {
                        WriteTestReport(args[2] + ".error.txt", cleanupError.ToString());
                        return 1;
                    }
                }
                WriteTestReport(args[2] + ".error.txt", ex.ToString());
                return 1;
            }
            catch (Exception ex)
            {
                WriteTestReport(args[2] + ".error.txt", ex.ToString());
                return 1;
            }
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if ((args.Length == 2 || args.Length == 3) && args[0] == "--preview")
        {
            try
            {
                using (ArabicInstaller preview = new ArabicInstaller())
                {
                    preview.FormBorderStyle = FormBorderStyle.None;
                    preview.MinimumSize = Size.Empty;
                    preview.ClientSize = new Size(940, 700);
                    preview.StartPosition = FormStartPosition.Manual;
                    preview.ShowInTaskbar = false;
                    preview.Location = new Point(-32000, -32000);
                    string state = args.Length == 3 ? args[2].ToLowerInvariant() : "default";
                    preview.PreparePreviewState(state);
                    preview.Show();
                    Application.DoEvents();
                    CreatePreviewControls(preview);
                    preview.PerformLayout();
                    preview.Refresh();
                    Application.DoEvents();
                    using (Bitmap bitmap = new Bitmap(preview.Width, preview.Height))
                    {
                        preview.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        bitmap.Save(args[1], ImageFormat.Png);
                    }
                    preview.Close();
                }
                return 0;
            }
            catch (Exception ex) { WriteTestReport(args[1] + ".error.txt", ex.ToString()); return 1; }
        }
        if (args.Length != 0) return 2;
        Application.Run(new ArabicInstaller());
        return 0;
    }

    static void CreatePreviewControls(Control control)
    {
        control.CreateControl();
        foreach (Control child in control.Controls) CreatePreviewControls(child);
        control.PerformLayout();
    }

    void PreparePreviewState(string state)
    {
        if (state != "default" && state != "progress" && state != "success" && state != "150")
            throw new ArgumentException("Preview state must be default, progress, success or 150.");
        if (state == "default") return;
        sourcePath.Text = @"D:\Games\Silent Hill Origins USA.iso";
        destinationPath.Text = @"D:\Games\Silent Hill Origins Arabic V2.0.iso";
        if (state == "progress")
        {
            SetBusy(true);
            progressTrack.Value = 62;
            progressLabel.Text = "62%";
            statusLabel.Text = "جارٍ إنشاء نسخة التعريب V2.0…";
        }
        else if (state == "success")
        {
            completedOutput = destinationPath.Text;
            folderButton.Enabled = true;
            progressTrack.Value = 100;
            progressLabel.Text = "100%";
            statusLabel.ForeColor = Sage;
            statusLabel.Text = "النسخة العربية جاهزة. اكتمل الإنشاء والتحقّق بنجاح.";
            buildButton.Text = "إنشاء نسخة أخرى";
        }
        else
        {
            statusLabel.Text = "جاهز للبدء. سيُفحص المصدر قبل إنشاء النسخة.";
            AutoScaleMode = AutoScaleMode.None;
            SuspendLayout();
            Scale(new SizeF(1.5F, 1.5F));
            ScalePreviewFonts(this, 1.5F);
            ClientSize = new Size(1410, 1050);
            ResumeLayout(true);
        }
    }

    static void ScalePreviewFonts(Control control, float factor)
    {
        // Save each explicit font before changing its parent's inherited font.
        foreach (Control child in control.Controls) ScalePreviewFonts(child, factor);
        Font current = control.Font;
        control.Font = new Font(current.FontFamily, current.Size * factor, current.Style, current.Unit);
    }

    static string[] MatchingPartialFiles(string destination)
    {
        string absolute = Path.GetFullPath(destination);
        return Directory.GetFiles(Path.GetDirectoryName(absolute), Path.GetFileName(absolute) + ".*.partial", SearchOption.TopDirectoryOnly);
    }

    sealed class RoundedSurface : Panel
    {
        readonly Color fill;
        readonly Color border;
        public int Radius = 10;

        public RoundedSurface(Color background, Color borderColor)
        {
            fill = background;
            border = borderColor;
            Dock = DockStyle.Fill;
            Margin = Padding.Empty;
            BackColor = background;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Control backdrop = Parent;
            while (backdrop != null && backdrop.BackColor == Color.Transparent) backdrop = backdrop.Parent;
            e.Graphics.Clear(backdrop == null ? Ink : backdrop.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF bounds = new RectangleF(0.5F, 0.5F, Math.Max(1, Width - 1F), Math.Max(1, Height - 1F));
            using (GraphicsPath shape = RoundRectangle(bounds, Radius))
            using (Brush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(border))
            {
                e.Graphics.FillPath(brush, shape);
                e.Graphics.DrawPath(pen, shape);
            }
        }
    }

    sealed class ThemedButton : Button
    {
        public bool IsPrimary;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled)
            {
                base.OnPaint(e);
                return;
            }
            Color background = IsPrimary ? Color.FromArgb(61, 74, 64) : Card;
            Color foreground = IsPrimary ? Color.FromArgb(187, 199, 181) : Color.FromArgb(117, 129, 122);
            e.Graphics.Clear(background);
            if (FlatAppearance.BorderSize > 0)
                using (Pen border = new Pen(Line)) e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            if (RightToLeft == RightToLeft.Yes) flags |= TextFormatFlags.RightToLeft;
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, foreground, background, flags);
        }
    }

    static GraphicsPath RoundRectangle(RectangleF bounds, float radius)
    {
        GraphicsPath path = new GraphicsPath();
        float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    sealed class ProgressTrack : Control
    {
        int progressValue;
        public int Value
        {
            get { return progressValue; }
            set { progressValue = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        public ProgressTrack()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AccessibleName = "تقدم إنشاء النسخة العربية";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Card);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width < 2 || Height < 2) return;
            using (GraphicsPath rail = RoundRectangle(new RectangleF(0, 0, Width - 1, Height - 1), 3))
            using (Brush background = new SolidBrush(Line)) e.Graphics.FillPath(background, rail);
            float filled = (Width - 1) * progressValue / 100F;
            if (filled < 1) return;
            using (GraphicsPath bar = RoundRectangle(new RectangleF(Width - 1 - filled, 0, filled, Height - 1), 3))
            using (Brush foreground = new SolidBrush(Sage)) e.Graphics.FillPath(foreground, bar);
        }
    }
}
