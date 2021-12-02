namespace Comet.Web.Language.Editor
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TxtSourceFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnSelectSourceFile = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DataGridSource = new System.Windows.Forms.DataGridView();
            this.DataGridTarget = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TxtTargetFilePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnSelectTargetFile = new System.Windows.Forms.Button();
            this.QueryString = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridTarget)).BeginInit();
            this.SuspendLayout();
            // 
            // TxtSourceFilePath
            // 
            this.TxtSourceFilePath.Location = new System.Drawing.Point(12, 30);
            this.TxtSourceFilePath.Name = "TxtSourceFilePath";
            this.TxtSourceFilePath.ReadOnly = true;
            this.TxtSourceFilePath.Size = new System.Drawing.Size(327, 23);
            this.TxtSourceFilePath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Base File";
            // 
            // BtnSelectSourceFile
            // 
            this.BtnSelectSourceFile.Location = new System.Drawing.Point(345, 29);
            this.BtnSelectSourceFile.Name = "BtnSelectSourceFile";
            this.BtnSelectSourceFile.Size = new System.Drawing.Size(75, 23);
            this.BtnSelectSourceFile.TabIndex = 3;
            this.BtnSelectSourceFile.Text = "Select File";
            this.BtnSelectSourceFile.UseVisualStyleBackColor = true;
            this.BtnSelectSourceFile.Click += new System.EventHandler(this.BtnSelectSourceFile_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer1.Location = new System.Drawing.Point(12, 59);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.DataGridSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.DataGridTarget);
            this.splitContainer1.Size = new System.Drawing.Size(861, 565);
            this.splitContainer1.SplitterDistance = 432;
            this.splitContainer1.TabIndex = 4;
            // 
            // DataGridSource
            // 
            this.DataGridSource.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridSource.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.QueryString,
            this.Message});
            this.DataGridSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridSource.Location = new System.Drawing.Point(0, 0);
            this.DataGridSource.Name = "DataGridSource";
            this.DataGridSource.RowTemplate.Height = 25;
            this.DataGridSource.Size = new System.Drawing.Size(432, 565);
            this.DataGridSource.TabIndex = 0;
            // 
            // DataGridTarget
            // 
            this.DataGridTarget.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridTarget.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            this.DataGridTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridTarget.Location = new System.Drawing.Point(0, 0);
            this.DataGridTarget.Name = "DataGridTarget";
            this.DataGridTarget.RowTemplate.Height = 25;
            this.DataGridTarget.Size = new System.Drawing.Size(425, 565);
            this.DataGridTarget.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.Frozen = true;
            this.dataGridViewTextBoxColumn1.HeaderText = "Query";
            this.dataGridViewTextBoxColumn1.MaxInputLength = 128;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Message";
            this.dataGridViewTextBoxColumn2.MaxInputLength = 65535;
            this.dataGridViewTextBoxColumn2.MinimumWidth = 500;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 500;
            // 
            // TxtTargetFilePath
            // 
            this.TxtTargetFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtTargetFilePath.Location = new System.Drawing.Point(546, 30);
            this.TxtTargetFilePath.Name = "TxtTargetFilePath";
            this.TxtTargetFilePath.ReadOnly = true;
            this.TxtTargetFilePath.Size = new System.Drawing.Size(327, 23);
            this.TxtTargetFilePath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(465, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Target File";
            // 
            // BtnSelectTargetFile
            // 
            this.BtnSelectTargetFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSelectTargetFile.Location = new System.Drawing.Point(465, 30);
            this.BtnSelectTargetFile.Name = "BtnSelectTargetFile";
            this.BtnSelectTargetFile.Size = new System.Drawing.Size(75, 23);
            this.BtnSelectTargetFile.TabIndex = 3;
            this.BtnSelectTargetFile.Text = "Select File";
            this.BtnSelectTargetFile.UseVisualStyleBackColor = true;
            this.BtnSelectTargetFile.Click += new System.EventHandler(this.BtnSelectTargetFile_Click);
            // 
            // QueryString
            // 
            this.QueryString.Frozen = true;
            this.QueryString.HeaderText = "Query";
            this.QueryString.MaxInputLength = 128;
            this.QueryString.Name = "QueryString";
            this.QueryString.ReadOnly = true;
            this.QueryString.Width = 64;
            // 
            // Message
            // 
            this.Message.HeaderText = "String";
            this.Message.MaxInputLength = 65535;
            this.Message.MinimumWidth = 500;
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Width = 500;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 636);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.BtnSelectTargetFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BtnSelectSourceFile);
            this.Controls.Add(this.TxtTargetFilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtSourceFilePath);
            this.Name = "FrmMain";
            this.Text = "Language Editor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridTarget)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtSourceFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnSelectSourceFile;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox TxtTargetFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnSelectTargetFile;
        private System.Windows.Forms.DataGridView DataGridSource;
        private System.Windows.Forms.DataGridView DataGridTarget;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn QueryString;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
    }
}
