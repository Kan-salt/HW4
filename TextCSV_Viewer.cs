/*
MIT License

Copyright (c) 2026 Sarayut Chaisuriya

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO;
using System.Windows.Forms;

namespace FileProcessing
{
    public partial class frmTextView : Form
    {
        private TextBox tbStartRow;
        private TextBox tbEndRow;
        private TextBox tbFilterType;
        private Button btReadPartialCSV;

        /// <summary>
        /// Initializes a new instance of the frmTextView class.
        /// </summary>
        public frmTextView()
        {
            InitializeComponent();
            SetupPartialControls();
        }

        private void SetupPartialControls()
        {
            // Add controls to CSV tab for HW4 requirements
            Label lblStart = new Label { Text = "Start m:", Location = new System.Drawing.Point(170, 12), AutoSize = true };
            tbStartRow = new TextBox { Location = new System.Drawing.Point(250, 8), Size = new System.Drawing.Size(80, 29), Text = "1" };

            Label lblEnd = new Label { Text = "End n:", Location = new System.Drawing.Point(340, 12), AutoSize = true };
            tbEndRow = new TextBox { Location = new System.Drawing.Point(420, 8), Size = new System.Drawing.Size(80, 29), Text = "100" };

            Label lblFilter = new Label { Text = "Filter (e.g. exe):", Location = new System.Drawing.Point(510, 12), AutoSize = true };
            tbFilterType = new TextBox { Location = new System.Drawing.Point(680, 8), Size = new System.Drawing.Size(130, 29), Text = "" };

            btReadPartialCSV = new Button
            {
                Text = "Read Partial + Filter",
                Location = new System.Drawing.Point(820, 5),
                Size = new System.Drawing.Size(200, 40)
            };
            btReadPartialCSV.Click += new System.EventHandler(this.btReadPartialCSV_Click);

            tabpCSV.Controls.Add(lblStart);
            tabpCSV.Controls.Add(tbStartRow);
            tabpCSV.Controls.Add(lblEnd);
            tabpCSV.Controls.Add(tbEndRow);
            tabpCSV.Controls.Add(lblFilter);
            tabpCSV.Controls.Add(tbFilterType);
            tabpCSV.Controls.Add(btReadPartialCSV);
        }

        private void btRead_Click(object sender, EventArgs e)
        {
            string content = File.ReadAllText(tbFileName.Text);
            rtbShow.Text = content;
        }

        private void btReadCSV_Click(object sender, EventArgs e)
        {
            dgvData.Rows.Clear();
            dgvData.Columns.Clear();

            using (StreamReader srReader = new StreamReader(tbFileName.Text))
            {
                string strLine;
                bool bHeaderRead = false;

                while ((strLine = srReader.ReadLine()) != null)
                {
                    string[] strHeaders_arr = null;
                    if (strLine.StartsWith("#"))
                    {
                        if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("#HEADER"))
                        {
                            strHeaders_arr = strLine.Substring(8).Split(',');
                        }
                        continue;
                    }

                    string[] strValues_arr = strLine.Split(',');

                    if (!bHeaderRead)
                    {
                        foreach (string strHeader in strValues_arr)
                        {
                            string colName = strHeader.Trim();
                            if (strHeaders_arr != null && dgvData.Columns.Count < strHeaders_arr.Length)
                                colName = strHeaders_arr[dgvData.Columns.Count].Trim();
                            dgvData.Columns.Add(colName, colName);
                        }
                        bHeaderRead = true;
                    }
                    else
                    {
                        dgvData.Rows.Add(strValues_arr);
                    }
                }
            }
        }

        /// <summary>
        /// HW4: Partial Loading (m-n) + Filter by file_type_guess + Combined
        /// </summary>
        private void btReadPartialCSV_Click(object sender, EventArgs e)
        {
            dgvData.Rows.Clear();
            dgvData.Columns.Clear();

            int startRow = 1;
            int endRow = int.MaxValue;
            string filter = tbFilterType.Text.Trim().ToLower();

            int.TryParse(tbStartRow.Text, out startRow);
            int.TryParse(tbEndRow.Text, out endRow);
            if (startRow < 1) startRow = 1;
            if (endRow < startRow) endRow = int.MaxValue;

            int currentRow = 0;
            int fileTypeIndex = -1;

            using (StreamReader srReader = new StreamReader(tbFileName.Text))
            {
                string strLine;
                bool bHeaderRead = false;

                while ((strLine = srReader.ReadLine()) != null)
                {
                    if (strLine.StartsWith("#"))
                    {
                        if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("#HEADER"))
                        {
                            // Header logic handled below
                        }
                        continue;
                    }

                    string[] strValues_arr = strLine.Split(',');

                    if (!bHeaderRead)
                    {
                        // Add columns and detect file_type_guess
                        for (int i = 0; i < strValues_arr.Length; i++)
                        {
                            string colName = strValues_arr[i].Trim().Replace("\"", "");
                            dgvData.Columns.Add(colName, colName);
                            if (colName.ToLower().Contains("file_type_guess"))
                                fileTypeIndex = i;
                        }
                        bHeaderRead = true;
                        continue;
                    }

                    currentRow++;

                    if (currentRow < startRow) continue;
                    if (currentRow > endRow) break;

                    bool matches = true;
                    if (!string.IsNullOrEmpty(filter) && fileTypeIndex >= 0)
                    {
                        string typeVal = strValues_arr[fileTypeIndex].Trim().ToLower().Replace("\"", "");
                        if (!typeVal.Contains(filter))
                            matches = false;
                    }

                    if (matches)
                        dgvData.Rows.Add(strValues_arr);
                }
            }

            MessageBox.Show($"Loaded {dgvData.Rows.Count} rows (range {startRow}-{endRow}, filter: '{filter}')", "Success");
        }

        private void btBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbFileName.Text = ofd.FileName;
                }
            }
        }

        private void tbFileName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}