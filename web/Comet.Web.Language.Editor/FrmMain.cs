using System;
using System.Collections;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace Comet.Web.Language.Editor
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void BtnSelectSourceFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

            TxtSourceFilePath.Text = openFileDialog.FileName;
            label1.Text = $"Source File: {Path.GetFileName(openFileDialog.FileName)}";

            //using ResXResourceWriter resxFile = new ResXResourceWriter(openFileDialog.FileName);
            using ResXResourceReader resxFile = new ResXResourceReader(openFileDialog.FileName);
            foreach (DictionaryEntry resx in resxFile)
            {
                string key = resx.Key?.ToString() ?? "";
                string value = resx.Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                DataGridSource.Rows.Add(new string[] { key, value });
            }
        }

        private void BtnSelectTargetFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

            TxtTargetFilePath.Text = openFileDialog.FileName;
            label2.Text = $"Target File: {Path.GetFileName(openFileDialog.FileName)}";

            using ResXResourceReader resxFile = new ResXResourceReader(openFileDialog.FileName);
            foreach (DictionaryEntry resx in resxFile)
            {
                string key = resx.Key?.ToString() ?? "";
                string value = resx.Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                DataGridTarget.Rows.Add(new string[] { key, value });
            }
        }
    }
}
