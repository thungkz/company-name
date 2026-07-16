using System.Drawing;
using System.Windows.Forms;

namespace ExeEncryptor;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    // ── Layout containers ────────────────────────────────────────────────────
    private TableLayoutPanel tlpMain;
    private TableLayoutPanel tlpTopRow;
    
    // ── Top-left: Text Converter ─────────────────────────────────────────────
    private GroupBox       gpTextConverter;
    private TableLayoutPanel tlpTextConverterInner;
    private Label          lblCompanyNameTab1;
    private TextBox        txtTextInput;
    private TextBox        txtTextOutput;
    private FlowLayoutPanel flpTextActions;
    private Button         btnUppercase;
    private Button         btnLowercase;
    private Button         btnCapitalize;
    private FlowLayoutPanel flpTextMode;
    private RadioButton    rbEncrypt;
    private RadioButton    rbDecrypt;
    private Button         btnConvertText;

    // ── Top-right: Title Bar Changer ─────────────────────────────────────────
    private GroupBox    gpTitleBarChanger;
    private TableLayoutPanel tlpTitleChangerInner;
    private Label       lblExePathChanger;
    private TextBox     txtExePathChanger;
    private Button      btnBrowseChanger;
    private Label       lblCompanyCodeTitle;
    private TextBox     txtCompanyCodeTitle;
    private Label       lblCompanyNameTitle;
    private TextBox     txtCompanyNameTitle;
    private Label       lblNewTitle;
    private TextBox     txtNewTitle;
    private Button      btnRenameTitle;
    private Label       lblStatusTitle;

    // ── Bottom: EXE Version Editor ───────────────────────────────────────────
    private GroupBox    gpVersionEditorParent;
    private TableLayoutPanel tlpVersionEditorInner;
    private GroupBox    gpTargetExe;
    private TextBox     txtFilePathVer;
    private Button      btnImportVer;
    private Button      btnLoadVer;

    private GroupBox    gpVersionInfo;
    private Label       lblProductName;
    private TextBox     txtProductName;
    private Button      btnSaveProductName;
    private Label       lblFileDescription;
    private TextBox     txtFileDescription;
    private Button      btnSaveFileDescription;
    private Label       lblFileVersion;
    private TextBox     txtFileVersion;
    private Button      btnSaveFileVersion;
    private Label       lblProductVersion;
    private TextBox     txtProductVersion;
    private Button      btnSaveProductVersion;
    private Label       lblCompanyName;
    private TextBox     txtCompanyName;
    private Button      btnSaveCompanyName;
    private Label       lblComments;
    private TextBox     txtComments;
    private Button      btnSaveComments;
    private Label       lblTitleBar;
    private TextBox     txtTitleBar;
    private Button      btnSaveTitleBar;
    private Label       lblPhysicalFilename;
    private TextBox     txtPhysicalFilename;
    private Button      btnRenamePhysical;
    private Button      btnSaveChanges;
    private Label       lblStatusVer;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        // 1. Containers
        tlpMain = new TableLayoutPanel();
        tlpTopRow = new TableLayoutPanel();
        gpTextConverter = new GroupBox();
        tlpTextConverterInner = new TableLayoutPanel();
        flpTextActions = new FlowLayoutPanel();
        flpTextMode = new FlowLayoutPanel();
        
        gpTitleBarChanger = new GroupBox();
        tlpTitleChangerInner = new TableLayoutPanel();
        
        gpVersionEditorParent = new GroupBox();
        tlpVersionEditorInner = new TableLayoutPanel();
        gpTargetExe = new GroupBox();
        gpVersionInfo = new GroupBox();

        // 2. Controls
        lblCompanyNameTab1 = new Label();
        txtTextInput = new TextBox();
        txtTextOutput = new TextBox();
        btnUppercase = new Button();
        btnLowercase = new Button();
        btnCapitalize = new Button();
        rbEncrypt = new RadioButton();
        rbDecrypt = new RadioButton();
        btnConvertText = new Button();

        lblExePathChanger = new Label();
        txtExePathChanger = new TextBox();
        btnBrowseChanger = new Button();
        lblCompanyCodeTitle = new Label();
        txtCompanyCodeTitle = new TextBox();
        lblCompanyNameTitle = new Label();
        txtCompanyNameTitle = new TextBox();
        lblNewTitle = new Label();
        txtNewTitle = new TextBox();
        btnRenameTitle = new Button();
        lblStatusTitle = new Label();

        txtFilePathVer = new TextBox();
        btnImportVer = new Button();
        btnLoadVer = new Button();
        
        lblProductName = new Label();
        txtProductName = new TextBox();
        btnSaveProductName = new Button();
        lblFileDescription = new Label();
        txtFileDescription = new TextBox();
        btnSaveFileDescription = new Button();
        lblFileVersion = new Label();
        txtFileVersion = new TextBox();
        btnSaveFileVersion = new Button();
        lblProductVersion = new Label();
        txtProductVersion = new TextBox();
        btnSaveProductVersion = new Button();
        lblCompanyName = new Label();
        txtCompanyName = new TextBox();
        btnSaveCompanyName = new Button();
        lblComments = new Label();
        txtComments = new TextBox();
        btnSaveComments = new Button();
        lblTitleBar = new Label();
        txtTitleBar = new TextBox();
        btnSaveTitleBar = new Button();
        lblPhysicalFilename = new Label();
        txtPhysicalFilename = new TextBox();
        btnRenamePhysical = new Button();
        btnSaveChanges = new Button();
        lblStatusVer = new Label();

        tlpMain.SuspendLayout();
        tlpTopRow.SuspendLayout();
        gpTextConverter.SuspendLayout();
        tlpTextConverterInner.SuspendLayout();
        flpTextActions.SuspendLayout();
        flpTextMode.SuspendLayout();
        gpTitleBarChanger.SuspendLayout();
        tlpTitleChangerInner.SuspendLayout();
        gpVersionEditorParent.SuspendLayout();
        tlpVersionEditorInner.SuspendLayout();
        gpTargetExe.SuspendLayout();
        gpVersionInfo.SuspendLayout();
        SuspendLayout();

        // ════════════════════════════════════════════════════════════════════
        //  tlpMain (Root Layout: Upper 50%, Lower 50%)
        // ════════════════════════════════════════════════════════════════════
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 1;
        tlpMain.RowCount = 2;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // Upper part (Encryption & Title Changer)
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 50F)); // Lower part (Version Editor)
        tlpMain.Padding = new Padding(12);

        // Add upper row & lower row
        tlpMain.Controls.Add(tlpTopRow, 0, 0);
        tlpMain.Controls.Add(gpVersionEditorParent, 0, 1);

        // ════════════════════════════════════════════════════════════════════
        //  tlpTopRow (Upper Layout: Left 50% | Right 50%)
        // ════════════════════════════════════════════════════════════════════
        tlpTopRow.Dock = DockStyle.Fill;
        tlpTopRow.ColumnCount = 2;
        tlpTopRow.RowCount = 1;
        tlpTopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpTopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpTopRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpTopRow.Margin = new Padding(0);

        tlpTopRow.Controls.Add(gpTextConverter, 0, 0);
        tlpTopRow.Controls.Add(gpTitleBarChanger, 1, 0);

        // ════════════════════════════════════════════════════════════════════
        //  gpTextConverter (Left Side)
        // ════════════════════════════════════════════════════════════════════
        gpTextConverter.Dock = DockStyle.Fill;
        gpTextConverter.Margin = new Padding(0, 0, 6, 0);
        gpTextConverter.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        gpTextConverter.Text = "Text Encryption & Conversion Tool";
        gpTextConverter.Controls.Add(tlpTextConverterInner);

        tlpTextConverterInner.Dock = DockStyle.Fill;
        tlpTextConverterInner.Padding = new Padding(8);
        tlpTextConverterInner.ColumnCount = 2;
        tlpTextConverterInner.RowCount = 4;
        tlpTextConverterInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Label column
        tlpTextConverterInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Input column
        tlpTextConverterInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));  // Mode (Encrypt/Decrypt)
        tlpTextConverterInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));  // Input String
        tlpTextConverterInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));  // Action Buttons
        tlpTextConverterInner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Output box (grows)

        // Mode Row
        flpTextMode.Dock = DockStyle.Fill;
        flpTextMode.Margin = new Padding(0);
        flpTextMode.Controls.Add(rbEncrypt);
        flpTextMode.Controls.Add(rbDecrypt);
        tlpTextConverterInner.Controls.Add(flpTextMode, 0, 0);
        tlpTextConverterInner.SetColumnSpan(flpTextMode, 2);

        rbEncrypt.Text = "Encrypt";
        rbEncrypt.Checked = true;
        rbEncrypt.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        rbEncrypt.Size = new Size(100, 30);

        rbDecrypt.Text = "Decrypt";
        rbDecrypt.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        rbDecrypt.Size = new Size(100, 30);

        // Input String Row
        lblCompanyNameTab1.Text = "Input Text:";
        lblCompanyNameTab1.Dock = DockStyle.Fill;
        lblCompanyNameTab1.TextAlign = ContentAlignment.MiddleLeft;
        lblCompanyNameTab1.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        tlpTextConverterInner.Controls.Add(lblCompanyNameTab1, 0, 1);

        txtTextInput.Dock = DockStyle.Fill;
        txtTextInput.Font = new Font("Segoe UI", 10F);
        txtTextInput.AllowDrop = true;
        txtTextInput.DragEnter += TxtTextInput_DragEnter;
        txtTextInput.DragDrop += TxtTextInput_DragDrop;
        tlpTextConverterInner.Controls.Add(txtTextInput, 1, 1);

        // Action Buttons Row
        flpTextActions.Dock = DockStyle.Fill;
        flpTextActions.Margin = new Padding(0);
        flpTextActions.Controls.Add(btnUppercase);
        flpTextActions.Controls.Add(btnLowercase);
        flpTextActions.Controls.Add(btnCapitalize);
        flpTextActions.Controls.Add(btnConvertText);
        tlpTextConverterInner.Controls.Add(flpTextActions, 0, 2);
        tlpTextConverterInner.SetColumnSpan(flpTextActions, 2);

        btnUppercase.Text = "UPPERCASE";
        btnUppercase.Size = new Size(110, 32);
        btnUppercase.FlatStyle = FlatStyle.Flat;
        btnUppercase.BackColor = Color.FromArgb(108, 117, 125);
        btnUppercase.ForeColor = Color.White;
        btnUppercase.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnUppercase.Click += BtnUppercase_Click;

        btnLowercase.Text = "lowercase";
        btnLowercase.Size = new Size(110, 32);
        btnLowercase.FlatStyle = FlatStyle.Flat;
        btnLowercase.BackColor = Color.FromArgb(108, 117, 125);
        btnLowercase.ForeColor = Color.White;
        btnLowercase.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnLowercase.Click += BtnLowercase_Click;

        btnCapitalize.Text = "Title Case";
        btnCapitalize.Size = new Size(110, 32);
        btnCapitalize.FlatStyle = FlatStyle.Flat;
        btnCapitalize.BackColor = Color.FromArgb(108, 117, 125);
        btnCapitalize.ForeColor = Color.White;
        btnCapitalize.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnCapitalize.Click += BtnCapitalize_Click;

        btnConvertText.Text = "Convert Text";
        btnConvertText.Size = new Size(140, 32);
        btnConvertText.BackColor = Color.FromArgb(22, 160, 133);
        btnConvertText.ForeColor = Color.White;
        btnConvertText.FlatStyle = FlatStyle.Flat;
        btnConvertText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnConvertText.Click += BtnConvertText_Click;

        // Output Box Row (Takes remaining vertical space)
        txtTextOutput.Dock = DockStyle.Fill;
        txtTextOutput.Multiline = true;
        txtTextOutput.ScrollBars = ScrollBars.Vertical;
        txtTextOutput.Font = new Font("Segoe UI", 10F);
        tlpTextConverterInner.Controls.Add(txtTextOutput, 0, 3);
        tlpTextConverterInner.SetColumnSpan(txtTextOutput, 2);

        // ════════════════════════════════════════════════════════════════════
        //  gpTitleBarChanger (Right Side)
        // ════════════════════════════════════════════════════════════════════
        gpTitleBarChanger.Dock = DockStyle.Fill;
        gpTitleBarChanger.Margin = new Padding(6, 0, 0, 0);
        gpTitleBarChanger.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        gpTitleBarChanger.ForeColor = Color.FromArgb(142, 68, 173);
        gpTitleBarChanger.Text = "Title Bar Changer";
        gpTitleBarChanger.Controls.Add(tlpTitleChangerInner);

        tlpTitleChangerInner.Dock = DockStyle.Fill;
        tlpTitleChangerInner.Padding = new Padding(8);
        tlpTitleChangerInner.ColumnCount = 3;
        tlpTitleChangerInner.RowCount = 5;
        tlpTitleChangerInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Label
        tlpTitleChangerInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // TextBox/Info
        tlpTitleChangerInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); // Action button

        tlpTitleChangerInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));  // EXE Path
        tlpTitleChangerInner.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));   // Decoded Names
        tlpTitleChangerInner.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));   // Encoded Codes
        tlpTitleChangerInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));  // New Title & Button
        tlpTitleChangerInner.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));   // Status label

        // EXE Path Row
        lblExePathChanger.Text = "Target EXE:";
        lblExePathChanger.Dock = DockStyle.Fill;
        lblExePathChanger.TextAlign = ContentAlignment.MiddleLeft;
        lblExePathChanger.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblExePathChanger.ForeColor = Color.FromArgb(52, 73, 94);
        tlpTitleChangerInner.Controls.Add(lblExePathChanger, 0, 0);

        txtExePathChanger.Dock = DockStyle.Fill;
        txtExePathChanger.ReadOnly = true;
        txtExePathChanger.Font = new Font("Segoe UI", 9.5F);
        txtExePathChanger.AllowDrop = true;
        txtExePathChanger.DragEnter += TxtExePathChanger_DragEnter;
        txtExePathChanger.DragDrop += TxtExePathChanger_DragDrop;
        tlpTitleChangerInner.Controls.Add(txtExePathChanger, 1, 0);

        btnBrowseChanger.Text = "Browse...";
        btnBrowseChanger.Dock = DockStyle.Fill;
        btnBrowseChanger.BackColor = Color.FromArgb(22, 160, 133);
        btnBrowseChanger.ForeColor = Color.White;
        btnBrowseChanger.FlatStyle = FlatStyle.Flat;
        btnBrowseChanger.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnBrowseChanger.Click += BtnBrowseChanger_Click;
        tlpTitleChangerInner.Controls.Add(btnBrowseChanger, 2, 0);

        // Decoded Names Row
        lblCompanyNameTitle.Text = "Decoded Company Name(s):";
        lblCompanyNameTitle.Dock = DockStyle.Fill;
        lblCompanyNameTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblCompanyNameTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblCompanyNameTitle.ForeColor = Color.FromArgb(52, 73, 94);
        tlpTitleChangerInner.Controls.Add(lblCompanyNameTitle, 0, 1);

        txtCompanyNameTitle.Dock = DockStyle.Fill;
        txtCompanyNameTitle.ReadOnly = true;
        txtCompanyNameTitle.Multiline = true;
        txtCompanyNameTitle.ScrollBars = ScrollBars.Vertical;
        txtCompanyNameTitle.Font = new Font("Segoe UI", 9.5F);
        tlpTitleChangerInner.Controls.Add(txtCompanyNameTitle, 1, 1);
        tlpTitleChangerInner.SetColumnSpan(txtCompanyNameTitle, 2);

        // Encoded Codes Row
        lblCompanyCodeTitle.Text = "Encoded Company Code(s):";
        lblCompanyCodeTitle.Dock = DockStyle.Fill;
        lblCompanyCodeTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblCompanyCodeTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblCompanyCodeTitle.ForeColor = Color.FromArgb(52, 73, 94);
        tlpTitleChangerInner.Controls.Add(lblCompanyCodeTitle, 0, 2);

        txtCompanyCodeTitle.Dock = DockStyle.Fill;
        txtCompanyCodeTitle.ReadOnly = true;
        txtCompanyCodeTitle.Multiline = true;
        txtCompanyCodeTitle.ScrollBars = ScrollBars.Vertical;
        txtCompanyCodeTitle.Font = new Font("Segoe UI", 9.5F);
        tlpTitleChangerInner.Controls.Add(txtCompanyCodeTitle, 1, 2);
        tlpTitleChangerInner.SetColumnSpan(txtCompanyCodeTitle, 2);

        // New Title Row
        lblNewTitle.Text = "New Title:";
        lblNewTitle.Dock = DockStyle.Fill;
        lblNewTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblNewTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblNewTitle.ForeColor = Color.FromArgb(52, 73, 94);
        tlpTitleChangerInner.Controls.Add(lblNewTitle, 0, 3);

        txtNewTitle.Dock = DockStyle.Fill;
        txtNewTitle.Font = new Font("Segoe UI", 10.5F);
        tlpTitleChangerInner.Controls.Add(txtNewTitle, 1, 3);

        btnRenameTitle.Text = "Rename Title";
        btnRenameTitle.Dock = DockStyle.Fill;
        btnRenameTitle.BackColor = Color.FromArgb(142, 68, 173);
        btnRenameTitle.ForeColor = Color.White;
        btnRenameTitle.FlatStyle = FlatStyle.Flat;
        btnRenameTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnRenameTitle.Click += BtnRenameTitle_Click;
        tlpTitleChangerInner.Controls.Add(btnRenameTitle, 2, 3);

        // Status Row
        lblStatusTitle.Dock = DockStyle.Fill;
        lblStatusTitle.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblStatusTitle.ForeColor = Color.DimGray;
        lblStatusTitle.TextAlign = ContentAlignment.TopLeft;
        lblStatusTitle.Text = "Select an EXE file. If the name is longer than original, it auto-compiles from source.";
        tlpTitleChangerInner.Controls.Add(lblStatusTitle, 0, 4);
        tlpTitleChangerInner.SetColumnSpan(lblStatusTitle, 3);

        // ════════════════════════════════════════════════════════════════════
        //  gpVersionEditorParent (Lower Side Layout)
        // ════════════════════════════════════════════════════════════════════
        gpVersionEditorParent.Dock = DockStyle.Fill;
        gpVersionEditorParent.Margin = new Padding(0, 6, 0, 0);
        gpVersionEditorParent.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        gpVersionEditorParent.ForeColor = Color.FromArgb(41, 128, 185);
        gpVersionEditorParent.Text = "EXE Version Editor";
        gpVersionEditorParent.Controls.Add(tlpVersionEditorInner);

        tlpVersionEditorInner.Dock = DockStyle.Fill;
        tlpVersionEditorInner.Padding = new Padding(8);
        tlpVersionEditorInner.ColumnCount = 1;
        tlpVersionEditorInner.RowCount = 2;
        tlpVersionEditorInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));  // Target EXE box
        tlpVersionEditorInner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Version Info panel

        tlpVersionEditorInner.Controls.Add(gpTargetExe, 0, 0);
        tlpVersionEditorInner.Controls.Add(gpVersionInfo, 0, 1);

        // ── Target EXE Subgroup ──
        gpTargetExe.Dock = DockStyle.Fill;
        gpTargetExe.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        gpTargetExe.ForeColor = Color.FromArgb(22, 160, 133);
        gpTargetExe.Text = "Target EXE File";

        // Layout target EXE elements
        var tlpTargetExeInner = new TableLayoutPanel();
        tlpTargetExeInner.Dock = DockStyle.Fill;
        tlpTargetExeInner.ColumnCount = 3;
        tlpTargetExeInner.RowCount = 1;
        tlpTargetExeInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpTargetExeInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        tlpTargetExeInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        gpTargetExe.Controls.Add(tlpTargetExeInner);

        txtFilePathVer.Dock = DockStyle.Fill;
        txtFilePathVer.ReadOnly = true;
        txtFilePathVer.Font = new Font("Segoe UI", 9.5F);
        txtFilePathVer.AllowDrop = true;
        txtFilePathVer.DragEnter += TxtFilePathVer_DragEnter;
        txtFilePathVer.DragDrop += TxtFilePathVer_DragDrop;
        tlpTargetExeInner.Controls.Add(txtFilePathVer, 0, 0);

        btnImportVer.Text = "Browse...";
        btnImportVer.Dock = DockStyle.Fill;
        btnImportVer.BackColor = Color.FromArgb(22, 160, 133);
        btnImportVer.ForeColor = Color.White;
        btnImportVer.FlatStyle = FlatStyle.Flat;
        btnImportVer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnImportVer.Click += BtnImportVer_Click;
        tlpTargetExeInner.Controls.Add(btnImportVer, 1, 0);

        btnLoadVer.Text = "Load Info";
        btnLoadVer.Dock = DockStyle.Fill;
        btnLoadVer.BackColor = Color.FromArgb(52, 73, 94);
        btnLoadVer.ForeColor = Color.White;
        btnLoadVer.FlatStyle = FlatStyle.Flat;
        btnLoadVer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnLoadVer.Click += BtnLoadVer_Click;
        tlpTargetExeInner.Controls.Add(btnLoadVer, 2, 0);

        // ── Version Info Subgroup ──
        gpVersionInfo.Dock = DockStyle.Fill;
        gpVersionInfo.Text = "Version Details (2 Columns)";
        gpVersionInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        gpVersionInfo.ForeColor = Color.FromArgb(41, 128, 185);

        var tlpVersionRows = new TableLayoutPanel();
        tlpVersionRows.Dock = DockStyle.Fill;
        tlpVersionRows.Padding = new Padding(6);
        tlpVersionRows.ColumnCount = 2;
        tlpVersionRows.RowCount = 5;
        tlpVersionRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpVersionRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tlpVersionRows.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
        tlpVersionRows.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
        tlpVersionRows.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
        tlpVersionRows.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
        tlpVersionRows.RowStyles.Add(new RowStyle(SizeType.Percent, 12F)); // Status line
        gpVersionInfo.Controls.Add(tlpVersionRows);

        // Row Helper using standard relative TableLayoutPanel docking
        Panel BuildRowControl(Label lbl, string labelText, TextBox tb, Button btn, string buttonText, System.EventHandler clickH)
        {
            var p = new Panel();
            p.Dock = DockStyle.Fill;
            p.Margin = new Padding(4);

            lbl.Text = labelText;
            lbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(52, 73, 94);
            lbl.Dock = DockStyle.Left;
            lbl.Width = 110;
            lbl.TextAlign = ContentAlignment.MiddleLeft;

            btn.Text = buttonText;
            btn.Width = 85;
            btn.Dock = DockStyle.Right;
            btn.BackColor = Color.FromArgb(52, 73, 94);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.Click += clickH;

            tb.Font = new Font("Segoe UI", 9.5F);
            tb.Dock = DockStyle.Fill;

            p.Controls.Add(tb);
            p.Controls.Add(lbl);
            p.Controls.Add(btn);
            return p;
        }

        tlpVersionRows.Controls.Add(BuildRowControl(lblProductName, "Product Name:", txtProductName, btnSaveProductName, "Save", BtnSaveProductName_Click), 0, 0);
        tlpVersionRows.Controls.Add(BuildRowControl(lblFileDescription, "Description:", txtFileDescription, btnSaveFileDescription, "Save", BtnSaveFileDescription_Click), 1, 0);
        
        tlpVersionRows.Controls.Add(BuildRowControl(lblFileVersion, "File Version:", txtFileVersion, btnSaveFileVersion, "Save", BtnSaveFileVersion_Click), 0, 1);
        tlpVersionRows.Controls.Add(BuildRowControl(lblProductVersion, "Product Ver:", txtProductVersion, btnSaveProductVersion, "Save", BtnSaveProductVersion_Click), 1, 1);
        
        tlpVersionRows.Controls.Add(BuildRowControl(lblCompanyName, "Company Name:", txtCompanyName, btnSaveCompanyName, "Save", BtnSaveCompanyName_Click), 0, 2);
        tlpVersionRows.Controls.Add(BuildRowControl(lblComments, "Comments:", txtComments, btnSaveComments, "Save", BtnSaveComments_Click), 1, 2);
        
        tlpVersionRows.Controls.Add(BuildRowControl(lblTitleBar, "Original Name:", txtTitleBar, btnSaveTitleBar, "Save", BtnSaveTitleBar_Click), 0, 3);
        tlpVersionRows.Controls.Add(BuildRowControl(lblPhysicalFilename, "Physical Name:", txtPhysicalFilename, btnRenamePhysical, "Rename", BtnRenamePhysical_Click), 1, 3);

        lblStatusVer.Dock = DockStyle.Fill;
        lblStatusVer.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblStatusVer.ForeColor = Color.DimGray;
        lblStatusVer.TextAlign = ContentAlignment.MiddleLeft;
        lblStatusVer.Text = "Load an EXE to start editing properties.";
        tlpVersionRows.Controls.Add(lblStatusVer, 0, 4);
        tlpVersionRows.SetColumnSpan(lblStatusVer, 2);

        // Unused properties compatibility
        btnSaveChanges = new Button();
        btnSaveChanges.Visible = false;

        // ════════════════════════════════════════════════════════════════════
        //  Form
        // ════════════════════════════════════════════════════════════════════
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1300, 850);
        MinimumSize = new Size(950, 750);
        Controls.Add(tlpMain);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.Sizable;
        Name = "Form1";
        Text = "CompanyName";
        Load += Form1_Load;

        tlpMain.ResumeLayout(false);
        tlpTopRow.ResumeLayout(false);
        gpTextConverter.ResumeLayout(false);
        tlpTextConverterInner.ResumeLayout(false);
        flpTextActions.ResumeLayout(false);
        flpTextMode.ResumeLayout(false);
        gpTitleBarChanger.ResumeLayout(false);
        tlpTitleChangerInner.ResumeLayout(false);
        gpVersionEditorParent.ResumeLayout(false);
        tlpVersionEditorInner.ResumeLayout(false);
        gpTargetExe.ResumeLayout(false);
        gpVersionInfo.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
}
