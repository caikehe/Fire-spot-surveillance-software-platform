using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsFirePlatform
{
    public partial class ProgressForm : DevExpress.XtraEditors.XtraForm
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public int MAX 
        {
            get { return progressBarControl1.Properties.Maximum; }
            set { progressBarControl1.Properties.Maximum = value; }
        }

        public int MIN
        {
            get { return progressBarControl1.Properties.Minimum; }
            set { progressBarControl1.Properties.Minimum = value; }
        }

        public void setpos(int value)
        {
            if (value <= progressBarControl1.Properties.Maximum)//如果值有效
            {
                progressBarControl1.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
                progressBarControl1.EditValue = value;//设置进度值
                labelControl1.Text = "已执行" + (value * 100 / progressBarControl1.Properties.Maximum).ToString() + "%，请等待！";//显示百分比
                if(value >= progressBarControl1.Properties.Maximum / 2)
                    System.Threading.Thread.Sleep(500);
            }
            Application.DoEvents();//重点，必须加上，否则父子窗体都假死
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.Owner.Enabled = false;//设置父窗体不可用
        }

        private void ProgressForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Owner.Enabled = true;//回复父窗体为可用
        }
    }
}
