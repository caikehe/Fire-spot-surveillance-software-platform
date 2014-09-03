using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WindowsFormsFirePlatform
{
    public  class FileManage
    {
        public static FileManage m_FileManage = new FileManage();
        //卫星表
        public List<string> m_satellite;
        //日期表
        public List<MyDate> m_dateList;
        //每天的卫星名表
        public List<List<string> > m_sateInDate;
        //当前时间
        public MyDate m_currentDate;

        public FileManage()
        {
            m_satellite = new List<string> { "MODIS" };
            m_dateList = new List<MyDate>();
            m_sateInDate = new List<List<string> >();
        }

        /// <summary>
        /// 扫描指定文件夹，更新日期表、卫星名表、每天的卫星名表
        /// </summary>
        /// <param name="filepath">指定文件夹路径</param>
        public void ScanFiles(string filepath)
        {
            if (!Directory.Exists(filepath))
                return;
            //清空当前所有链表
            m_sateInDate.Clear();
            m_dateList.Clear();
            m_satellite.Clear();

            DirectoryInfo df = new DirectoryInfo(filepath);
            DirectoryInfo[] files1 = df.GetDirectories();//日期文件夹
            foreach (DirectoryInfo fi in files1)
            {
                string tempstr = fi.Name;
                string[] datestr = tempstr.Split(new char[] { ' ', '_', '.','/','\\' },3);//获得文件夹的日期
                MyDate tempdate = new MyDate(int.Parse(datestr[0]),int.Parse(datestr[1]),int.Parse(datestr[2]));
                if (!m_dateList.Contains(tempdate))
                    m_dateList.Add(tempdate);//添加当前文件夹日期
                DirectoryInfo df2 = new DirectoryInfo(filepath + "\\" + tempstr);
                DirectoryInfo[] files2 = df2.GetDirectories();
                List<string> sateindate = new List<string>();//每个日期下的卫星名
                foreach (DirectoryInfo fi2 in files2)
                {
                    string tempstr2 = fi2.Name;//卫星文件夹
                    if (!m_satellite.Contains(tempstr2))//更新卫星名
                        m_satellite.Add(tempstr2);
                    DirectoryInfo hdfDI = new DirectoryInfo(filepath + "\\" + tempstr + "\\" + tempstr2);
                    List<string> hdfFI = new List<string>();
                    ListFiles(hdfDI,"hdf",hdfFI);
                    //FileInfo[] hdfFI = hdfDI.GetFiles("*.hdf");//扫描hdf文件
                    if (hdfFI.Count() < 2)
                    {
                        continue;// 当前文件夹中的hdf文件不足或不存在
                    }
                    bool mod02 = false;//MOD02存在标识
                    bool mod14 = false;//MOD14存在标识
                    switch (tempstr2)
                    {
                        case "MODIS":
                            foreach (string hdfname in hdfFI)
                            {
                                if (hdfname.Contains("MOD021KM") | hdfname.Contains("MYD021KM"))
                                    mod02 = true;
                                if (hdfname.Contains("MOD14") | hdfname.Contains("MYD14"))
                                    mod14 = true;
                            }
                            break;
                        default:
                            if (hdfFI.Count() >= 2)
                            {
                                mod02 = true;
                                mod14 = true;
                            }
                            break;
                    }
                    if (mod02 && mod14)
                        sateindate.Add(tempstr2);
                }
                m_sateInDate.Add(sateindate);//添加到每天的卫星名链表
            }
        }

        ///<summary>
        /// 递归搜索某一目录下的所有指定扩展名的文件，调用实例：ListFiles(new DirectoryInfo("C:\\"), "txt", namelist);
        /// </summary>
        /// <param name="info">目录路径</param>
        /// <param name="Ext">指定扩展名</param>
        /// <param name="obj">输出表</param>
        public void ListFiles(FileSystemInfo info, string Ext,  List<string> filelist)
        {
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录 
            if (dir == null) return;
            try
            {

                FileSystemInfo[] files = dir.GetFileSystemInfos();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i] as FileInfo;
                    //是文件
                    if (file != null && file.Extension.ToUpper() == "." + Ext.ToUpper())
                    {
                        filelist.Add(file.Name);
                    }
                    //对于子目录，进行递归调用 
                    else
                        ListFiles(files[i], Ext,  filelist);

                }
            }
            catch (UnauthorizedAccessException ex)
            {
                
            } 

        }

    }
}
