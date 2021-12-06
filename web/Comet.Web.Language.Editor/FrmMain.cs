using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Comet.Web.Language.Editor
{
    public partial class FrmMain : Form
    {
        private Dictionary<string, string> mKvpSource = new Dictionary<string, string>();
        private Dictionary<string, string> mKvpTarget = new Dictionary<string, string>();

        public FrmMain()
        {
            InitializeComponent();
        }

        private void CountDifferences()
        {
            int count = 0;
            int rows = 0;
            foreach (DataGridViewRow row in DataGridSource.Rows)
            {
                string key = row.Cells[0].Value.ToString();
                string sourceValue = row.Cells[1].Value.ToString();
                string targetKey = DataGridTarget.Rows[row.Index].Cells[0].Value.ToString();
                string targetValue = DataGridTarget.Rows[row.Index].Cells[1].Value.ToString();

                if (string.IsNullOrEmpty(key)) continue;
                if (string.IsNullOrEmpty(sourceValue)) continue;
                if (string.IsNullOrEmpty(targetKey)) continue;

                if (!key.Equals(targetKey))
                    continue;

                rows++;
                if (!sourceValue.Equals(targetValue))
                {
                    count++;
                }
            }

            lblDifferences.Text = $"Strings to update {rows - count} of {rows}";
        }

        private void AutoResizeRow(int index)
        {
            DataGridSource.AutoResizeRow(index);
            DataGridTarget.AutoResizeRow(index);

            int sourceSize = DataGridSource.Rows[index].Height;
            int targetSize = DataGridTarget.Rows[index].Height;

            if (sourceSize > targetSize)
            {
                DataGridTarget.Rows[index].Height = sourceSize;
            }
            else
            {
                DataGridSource.Rows[index].Height = targetSize;
            }
        }

        private void AutoResizeRows()
        {
            DataGridSource.AutoResizeRows();
            DataGridTarget.AutoResizeRows();

            foreach (DataGridViewRow row in DataGridSource.Rows)
            {
                int sourceSize = row.Height;
                int targetSize = DataGridTarget.Rows[row.Index].Height;

                if (sourceSize > targetSize)
                {
                    DataGridTarget.Rows[row.Index].Height = sourceSize;
                }
                else
                {
                    row.Height = targetSize;
                }
            }
        }

        public Task SyncGridsAsync(bool clear)
        {
            if (clear)
            {
                DataGridSource.Rows.Clear();
                DataGridTarget.Rows.Clear();
            }

            if (clear)
            {
                foreach (var key in mKvpTarget.Keys)
                {
                    string sourceValue = mKvpSource[key];
                    string targetValue;
                    Color color = Color.FromArgb(0xFF, 0xFF, 0xFF);

                    if (mKvpSource.ContainsKey(key))
                    {
                        targetValue = mKvpTarget[key];
                        if (targetValue.Equals(sourceValue))
                        {
                            color = Color.FromArgb(0x87, 0xCE, 0xFA);//"#1e90ff";
                        }
                    }
                    else
                    {
                        color = Color.FromArgb(0xA6, 0x44, 0x52); //"#A64452";
                        targetValue = string.Empty;
                    }

                    DataGridSource.Rows.Add(new string[]
                    {
                        key,
                        sourceValue
                    });

                    DataGridTarget.Rows.Add(new string[]
                    {
                        key,
                        targetValue
                    });
                    DataGridTarget.Rows[DataGridTarget.Rows.Count - 1].DefaultCellStyle.BackColor = color;
                }
            }
            else
            {
                foreach (DataGridViewRow row in DataGridSource.Rows)
                {
                    string key = row.Cells[0].Value.ToString();
                    string sourceValue = row.Cells[1].Value.ToString();
                    string targetKey = DataGridTarget.Rows[row.Index].Cells[0].Value.ToString();
                    string targetValue = DataGridTarget.Rows[row.Index].Cells[1].Value.ToString();

                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(sourceValue)) continue;
                    if (string.IsNullOrEmpty(targetKey)) continue;
                    if (string.IsNullOrEmpty(targetValue)) continue;

                    if (!key.Equals(targetKey))
                        continue;

                    if (sourceValue.Equals(targetValue))
                    {
                        DataGridTarget.Rows[row.Index].DefaultCellStyle.BackColor = Color.FromArgb(0x87, 0xCE, 0xFA);
                    }
                    else
                    {
                        DataGridTarget.Rows[row.Index].DefaultCellStyle.BackColor = Color.FromArgb(0xFF, 0xFF, 0xFF);
                    }
                }
            }
            AutoResizeRows();
            CountDifferences();
            return Task.CompletedTask;
        }

        private async void BtnSelectSourceFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

            mKvpSource.Clear();

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

                mKvpSource.Add(key, value);
                //DataGridSource.Rows.Add(new string[] { key, value });
            }

            if (!string.IsNullOrEmpty(TxtTargetFilePath.Text))
            {
                await SyncGridsAsync(true);
            }
        }

        private async void BtnSelectTargetFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

            mKvpTarget.Clear();

            TxtTargetFilePath.Text = openFileDialog.FileName;
            label2.Text = $"Target File: {Path.GetFileName(openFileDialog.FileName)}";

            using ResXResourceReader resxFile = new ResXResourceReader(openFileDialog.FileName);
            foreach (DictionaryEntry resx in resxFile)
            {
                string key = resx.Key?.ToString() ?? "";
                string value = resx.Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                mKvpTarget.Add(key, value);
                //DataGridTarget.Rows.Add(new string[] { key, value });
            }

            if (!string.IsNullOrEmpty(TxtSourceFilePath.Text))
            {
                await SyncGridsAsync(true);
            }
        }

        private void DataGridTarget_CellLeave(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DataGridTarget_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= DataGridSource.RowCount || e.RowIndex >= DataGridTarget.RowCount)
                return;

            string key = DataGridTarget.Rows[e.RowIndex].Cells[0].Value.ToString();
            string sourceValue = DataGridSource.Rows[e.RowIndex].Cells[1].Value.ToString();
            string targetValue = DataGridTarget.Rows[e.RowIndex].Cells[1].Value.ToString();

            if (sourceValue.Equals(targetValue))
            {
                DataGridTarget.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0x87, 0xCE, 0xFA);
            }
            else
            {
                DataGridTarget.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0xFF, 0xFF, 0xFF);
            }
            CountDifferences();
            AutoResizeRow(e.RowIndex);
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            using ResXResourceWriter resxFile = new ResXResourceWriter(TxtTargetFilePath.Text);
            foreach (DataGridViewRow row in DataGridTarget.Rows)
            {
                string key = row.Cells[0].Value.ToString();
                string value = row.Cells[1].Value.ToString();

                resxFile.AddResource(key, value);
            }

            await SyncGridsAsync(false);
        }

        private void DataGridSource_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (DataGridSource.FirstDisplayedScrollingRowIndex < 0 || DataGridSource.FirstDisplayedScrollingRowIndex + 1 >= DataGridTarget.RowCount)
                return;

            DataGridTarget.FirstDisplayedScrollingRowIndex = DataGridSource.FirstDisplayedScrollingRowIndex;
        }

        private void DataGridTarget_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (DataGridTarget.FirstDisplayedScrollingRowIndex < 0 || DataGridTarget.FirstDisplayedScrollingRowIndex + 1 >= DataGridSource.RowCount)
                return;

            DataGridSource.FirstDisplayedScrollingRowIndex = DataGridTarget.FirstDisplayedScrollingRowIndex;
        }
    }
}
