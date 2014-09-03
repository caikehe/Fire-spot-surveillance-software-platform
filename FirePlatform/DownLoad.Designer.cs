namespace WindowsFormsFirePlatform
{
    partial class DownLoad
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelDownLoadLink = new System.Windows.Forms.Label();
            this.labelFileName = new System.Windows.Forms.Label();
            this.labelSavePath = new System.Windows.Forms.Label();
            this.CurrentProgress = new System.Windows.Forms.Label();
            this.msgLab = new System.Windows.Forms.Label();
            this.fileNameTxt = new System.Windows.Forms.TextBox();
            this.urlTxt = new System.Windows.Forms.TextBox();
            this.localPathTxt = new System.Windows.Forms.TextBox();
            this.butOpenDirectory = new DevExpress.XtraEditors.SimpleButton();
            this.butOK = new DevExpress.XtraEditors.SimpleButton();
            this.butCancel = new DevExpress.XtraEditors.SimpleButton();
            this.butClose = new DevExpress.XtraEditors.SimpleButton();
            this.progressBar1 = new DevExpress.XtraEditors.ProgressBarControl();
            this.SatecomboBox = new DevExpress.XtraEditors.ComboBoxEdit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SatecomboBox.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDownLoadLink
            // 
            this.labelDownLoadLink.AutoSize = true;
            this.labelDownLoadLink.Location = new System.Drawing.Point(12, 50);
            this.labelDownLoadLink.Name = "labelDownLoadLink";
            this.labelDownLoadLink.Size = new System.Drawing.Size(55, 14);
            this.labelDownLoadLink.TabIndex = 0;
            this.labelDownLoadLink.Text = "下载链接";
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(12, 108);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(55, 14);
            this.labelFileName.TabIndex = 1;
            this.labelFileName.Text = "文件名称";
            // 
            // labelSavePath
            // 
            this.labelSavePath.AutoSize = true;
            this.labelSavePath.Location = new System.Drawing.Point(12, 165);
            this.labelSavePath.Name = "labelSavePath";
            this.labelSavePath.Size = new System.Drawing.Size(55, 14);
            this.labelSavePath.TabIndex = 2;
            this.labelSavePath.Text = "保存路径";
            // 
            // CurrentProgress
            // 
            this.CurrentProgress.AutoSize = true;
            this.CurrentProgress.Location = new System.Drawing.Point(12, 224);
            this.CurrentProgress.Name = "CurrentProgress";
            this.CurrentProgress.Size = new System.Drawing.Size(55, 14);
            this.CurrentProgress.TabIndex = 3;
            this.CurrentProgress.Text = "当前进度";
            // 
            // msgLab
            // 
            this.msgLab.AutoSize = true;
            this.msgLab.Location = new System.Drawing.Point(555, 224);
            this.msgLab.Name = "msgLab";
            this.msgLab.Size = new System.Drawing.Size(55, 14);
            this.msgLab.TabIndex = 5;
            this.msgLab.Text = "准备就绪";
            this.msgLab.TextChanged += new System.EventHandler(this.msgLab_TextChanged);
            // 
            // fileNameTxt
            // 
            this.fileNameTxt.Location = new System.Drawing.Point(84, 104);
            this.fileNameTxt.Name = "fileNameTxt";
            this.fileNameTxt.Size = new System.Drawing.Size(542, 22);
            this.fileNameTxt.TabIndex = 9;
            // 
            // urlTxt
            // 
            this.urlTxt.Location = new System.Drawing.Point(84, 48);
            this.urlTxt.Name = "urlTxt";
            this.urlTxt.Size = new System.Drawing.Size(542, 22);
            this.urlTxt.TabIndex = 10;
            this.urlTxt.TextChanged += new System.EventHandler(this.urlTxt_TextChanged);
            // 
            // localPathTxt
            // 
            this.localPathTxt.Location = new System.Drawing.Point(84, 163);
            this.localPathTxt.Name = "localPathTxt";
            this.localPathTxt.Size = new System.Drawing.Size(378, 22);
            this.localPathTxt.TabIndex = 11;
            // 
            // butOpenDirectory
            // 
            this.butOpenDirectory.Location = new System.Drawing.Point(551, 157);
            this.butOpenDirectory.Name = "butOpenDirectory";
            this.butOpenDirectory.Size = new System.Drawing.Size(78, 28);
            this.butOpenDirectory.TabIndex = 13;
            this.butOpenDirectory.Text = "浏览";
            this.butOpenDirectory.Click += new System.EventHandler(this.butOpenDirectory_Click);
            // 
            // butOK
            // 
            this.butOK.Location = new System.Drawing.Point(127, 294);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(86, 34);
            this.butOK.TabIndex = 14;
            this.butOK.Text = "确定";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(265, 294);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(87, 34);
            this.butCancel.TabIndex = 15;
            this.butCancel.Text = "取消";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butClose
            // 
            this.butClose.Location = new System.Drawing.Point(413, 294);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(87, 34);
            this.butClose.TabIndex = 16;
            this.butClose.Text = "关闭";
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(84, 219);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(465, 29);
            this.progressBar1.TabIndex = 17;
            // 
            // SatecomboBox
            // 
            this.SatecomboBox.Location = new System.Drawing.Point(468, 163);
            this.SatecomboBox.Name = "SatecomboBox";
            this.SatecomboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.SatecomboBox.Size = new System.Drawing.Size(77, 20);
            this.SatecomboBox.TabIndex = 18;
            this.SatecomboBox.EditValueChanged += new System.EventHandler(this.SelectSate_Click);
            // 
            // DownLoad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 398);
            this.Controls.Add(this.SatecomboBox);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.butClose);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.butOpenDirectory);
            this.Controls.Add(this.localPathTxt);
            this.Controls.Add(this.urlTxt);
            this.Controls.Add(this.fileNameTxt);
            this.Controls.Add(this.msgLab);
            this.Controls.Add(this.CurrentProgress);
            this.Controls.Add(this.labelSavePath);
            this.Controls.Add(this.labelFileName);
            this.Controls.Add(this.labelDownLoadLink);
            this.Name = "DownLoad";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "文件下载器";
            this.Load += new System.EventHandler(this.DownLoad_Load);
            ((System.ComponentModel.ISupportInitialize)(this.progressBar1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SatecomboBox.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDownLoadLink;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.Label labelSavePath;
        private System.Windows.Forms.Label CurrentProgress;
        private System.Windows.Forms.Label msgLab;
        private System.Windows.Forms.TextBox fileNameTxt;
        private System.Windows.Forms.TextBox urlTxt;
        private System.Windows.Forms.TextBox localPathTxt;
        private DevExpress.XtraEditors.SimpleButton butOpenDirectory;
        private DevExpress.XtraEditors.SimpleButton butOK;
        private DevExpress.XtraEditors.SimpleButton butCancel;
        private DevExpress.XtraEditors.SimpleButton butClose;
        private DevExpress.XtraEditors.ProgressBarControl progressBar1;
        private DevExpress.XtraEditors.ComboBoxEdit SatecomboBox;
    }
}