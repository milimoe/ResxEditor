using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResxEditor
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClearEng.Click += ClearEng_Click;
            this.ClearSC.Click += ClearSC_Click;
            this.ClearTC.Click += ClearTC_Click;
            this.LoadEng.Click += LoadEng_Click;
            this.LoadSC.Click += LoadSC_Click;
            this.LoadTC.Click += LoadTC_Click;
            this.Shown += ShowEvent;
        }

        #region L和C按钮

        private void SelectFileAndSetFileName(TextBox T)
        {
            // 选择文件并设置文件路径
            OpenFileDialog objDialog = new()
            {
                Title = "Load File",
                Filter = "XML Files(*.resx)|*.resx"
            };
            if (objDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                T.Text = Convert.ToString(objDialog.FileName);
            }
        }

        private void ClearSC_Click(object sender, EventArgs e)
        {
            this.TextSC.Text = "";
        }

        private void ClearTC_Click(object sender, EventArgs e)
        {
            this.TextTC.Text = "";
        }

        private void ClearEng_Click(object sender, EventArgs e)
        {
            this.TextEng.Text = "";
        }

        private void LoadSC_Click(object sender, EventArgs e)
        {
            SelectFileAndSetFileName(TextSC);
        }

        private void LoadTC_Click(object sender, EventArgs e)
        {
            SelectFileAndSetFileName(TextTC);
        }

        private void LoadEng_Click(object sender, EventArgs e)
        {
            SelectFileAndSetFileName(TextEng);
        }

        #endregion

        private void ShowEvent(object sender, EventArgs e)
        {
            TextEng.Text = ResxEditor.ResxEng;
            TextSC.Text = ResxEditor.ResxSC;
            TextTC.Text = ResxEditor.ResxTC;
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            ResxEditor.ResxEng = TextEng.Text;
            ResxEditor.ResxSC = TextSC.Text;
            ResxEditor.ResxTC = TextTC.Text;
            ResxEditor.WriteINI("Resource", "English", ResxEditor.ResxEng);
            ResxEditor.WriteINI("Resource", "SChinese", ResxEditor.ResxSC);
            ResxEditor.WriteINI("Resource", "TChinese", ResxEditor.ResxTC);
            MessageBox.Show("写入完成！");
            this.Hide();
        }
    }
}
