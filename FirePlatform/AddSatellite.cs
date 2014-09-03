using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace WindowsFormsFirePlatform
{
    public partial class AddSatellite : DevExpress.XtraEditors.XtraForm
    {
        public string m_newSate;
        public AddSatellite()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (satelliteEdit.Text == "")
            {
                MessageBox.Show("请输入新卫星名！", "Error Message");
                return;
            }
            m_newSate = satelliteEdit.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        
    }
}