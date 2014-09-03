using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Skins;

namespace WindowsFormsFirePlatform
{
    public partial class SkinForm : DevExpress.XtraEditors.XtraForm
    {
        public string m_ActiveSkinName;
        public DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;

        public SkinForm()
        {
            InitializeComponent();
            defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            foreach (SkinContainer skin in SkinManager.Default.Skins)
            {
                listBoxControl1.Items.Add(skin.SkinName);
            }

            listBoxControl1.SelectedItem = DevExpress.LookAndFeel.UserLookAndFeel.Default.ActiveSkinName;
        }

        private void listBoxControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            defaultLookAndFeel1.LookAndFeel.UseWindowsXPTheme = false;
            defaultLookAndFeel1.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Skin;
            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.LookAndFeel.LookAndFeelHelper.ForceDefaultLookAndFeelChanged();
            string skinname = listBoxControl1.SelectedItem.ToString();
            defaultLookAndFeel1.LookAndFeel.SkinName = skinname;
            m_ActiveSkinName = skinname;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}