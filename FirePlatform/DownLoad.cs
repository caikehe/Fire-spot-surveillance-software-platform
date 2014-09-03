using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using System.Net;
using ESRI.ArcGIS.Controls;

namespace WindowsFormsFirePlatform
{
    public partial class DownLoad : DevExpress.XtraEditors.XtraForm
    {
        private WebClient webDownLoad = new WebClient();

        private string  dDirectory = HdfImage.m_firmaskpath + @"\\GoogleData\\" + FileManage.m_FileManage.m_currentDate.year
                        + "." + FileManage.m_FileManage.m_currentDate.month + "." + FileManage.m_FileManage.m_currentDate.day;
        
        public DownLoad()
        {
            InitializeComponent();
        }

        private void DownLoad_Load(object sender, EventArgs e)
        {
            urlTxt.Text = @"http:/ladsweb.nascom.nasa.gov/data/search.html";
            foreach (string sate in FileManage.m_FileManage.m_satellite)
            {
                SatecomboBox.Properties.Items.Add(sate);
            }
            SatecomboBox.Text = FileManage.m_FileManage.m_satellite.Last();

            this.localPathTxt.Text = dDirectory + "\\" +SatecomboBox.Text;
            this.msgLab.Text = "准备就绪";
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (this.urlTxt.Text == string.Empty)
            {
                return;
            }

            if (!Directory.Exists(localPathTxt.Text))
                Directory.CreateDirectory(localPathTxt.Text);
            this.progressBar1.EditValue = 0;
            this.butOK.Enabled = false;

            string filePath = this.localPathTxt.Text + "\\" + this.fileNameTxt.Text;

            if (filePath.Contains(@"\\") == true)
            {
                filePath.Replace(@"\\", @"\");
            }

            try
            {
                webDownLoad.DownloadFileCompleted += new
                    AsyncCompletedEventHandler(webDownLoad_DownloadFileCompleted);
                webDownLoad.DownloadProgressChanged += new
                    DownloadProgressChangedEventHandler(webDownLoad_DownloadProgressChanged);
                webDownLoad.DownloadFileAsync(new Uri(this.urlTxt.Text.Trim()), filePath);
            }
            catch (Exception o)
            {
                this.butOK.Enabled = true; 
                this.msgLab.Text = "下载出错"; 
                this.progressBar1.EditValue = 0;                 
                //string FilePath = this.localPathTxt.Text + "\\" + this.fileNameTxt.Text; 
                //System.IO.File.Delete(FilePath);    
              
                MessageBox.Show(o.Message, "出错", MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private void butOpenDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true; 
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK) 
            { 
                this.localPathTxt.Text = fbd.SelectedPath;
            } 
        }

        private void urlTxt_TextChanged(object sender, EventArgs e)
        {
            this.progressBar1.EditValue = 0;
            this.butOK.Enabled = true; 
            this.msgLab.Text = "准备就绪"; 
            string url = this.urlTxt.Text.Trim();
            if (url != string.Empty)
            { 
                int index = url.LastIndexOf('/');
                this.fileNameTxt.Text = url.Substring(index + 1, url.Length - index - 1); 
            }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            webDownLoad.CancelAsync();
            this.msgLab.Text = "下载取消";
            this.butOK.Enabled = true; 
            this.progressBar1.EditValue = 0;         
            //string filePath = this.localPathTxt.Text + "\\" + this.fileNameTxt.Text;   
            //System.IO.File.Delete(filePath);
        }

        private void webDownLoad_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) 
        { 
            msgLab.Text = string.Format("{0}%", e.ProgressPercentage.ToString()); 
            this.progressBar1.EditValue = e.ProgressPercentage;
        }     
 
        private void webDownLoad_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) 
            { 
                this.msgLab.Text = "下载取消"; 
            }
            else 
            { 
                this.msgLab.Text = "下载完成";
            } 
        }

        private void msgLab_TextChanged(object sender, EventArgs e)
        {
            if (this.msgLab.Text.CompareTo("下载完成") == 0)
            { 
                string FileFolder = this.localPathTxt.Text.Trim();
                System.Diagnostics.Process.Start("explorer.exe", FileFolder);
            }
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SelectSate_Click(object sender, EventArgs e)
        {
            this.localPathTxt.Text = dDirectory + "\\" + SatecomboBox.Text;
        }

        
       
    }
}
