using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsFirePlatform
{
    //波段数
    public enum BandNum { Band_1, Band_2, Band_7, Band_21, Band_22, Band_31, Band_32, Band_5 };

    public class HdfImage
    {
        public static HdfImage m_hdf ;
        //原始数据矩阵
        private UInt16[, ,] m_HdfImagedata;
        //经度矩阵，实际数据为原始数据的1/5下抽样
        private float[,] m_Longitude;
        //纬度矩阵
        private float[,] m_Latitude;
        //图像高度
        private int m_height;
        //图像宽度
        private int m_width;
        
        //拍摄时间(时和分)
        private string m_Date;
        //拍摄日期（年月日）
        private string m_DateYMD;
        //海洋掩膜
        private byte[,] m_SeaMask;
        //太阳天顶角
        private short[,] m_SolarZenith;
        //太阳方位角
        private short[,] m_SolarAzimuth;
        //视角
        private short[,] m_ViewAngle;

        //火掩膜,像素值为7、8、9表示火点
        private byte[,] m_Firemask;
        //火点二值矩阵
        private byte[,] m_BinaryFiremask;
        private List<Point> m_BinaryFiremaskList;
        //火迹点矩阵
        private byte[,] m_FireArea;
        //火迹线链表
        private List<Point> m_FireLineList;

        //火点扫描线对应坐标
        private short[] m_FPline;
        //火点抽样坐标
        private short[] m_FPsample;
        //火点纬度
        private float[] m_FPlat;
        //火点经度
        private float[] m_FPlong;
        //检测得到的火点标号，7、8、9为火点
        private byte[,] m_DetectFireplot;

        //不可用数据点标记
        public byte[,] m_UselessPoint;

        //火点保存路径，tiff图像
        public static string m_firmaskpath = Application.StartupPath.ToString() + "\\TestData";
        public string m_originalData = Application.StartupPath.ToString() + "\\TestData\\OriginalData";
        public string m_googleData = Application.StartupPath.ToString() + "\\TestData\\GoogleData";

        

        public HdfImage() { }

        public HdfImage(int a, int b, int c = (int)BandNum.Band_5 + 1)
        {
            Height = a;
            Width = b;
            HdfImageData = new UInt16[a, b, c];
            m_DetectFireplot = new byte[a,b];
            m_BinaryFiremask = new byte[a,b];
            m_ViewAngle = new short[a,b];
            m_BinaryFiremaskList = new List<Point>();
        }
        /// <summary>
        /// 属性，读取原始数据，注意为static
        /// </summary>
        public UInt16[, ,] HdfImageData
        {
            get { return m_HdfImagedata; }
            set { m_HdfImagedata = value; }
        }

        public float[,] HdfLongitude
        {
            get { return m_Longitude; }
            set { m_Longitude = value; }
        }

        public float[,] HdfLatitude
        {
            get { return m_Latitude; }
            set { m_Latitude = value; }
        }

        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        public string Date
        {
            get { return m_Date; }
            set { m_Date = value; } 
        }

        public string DateYMD
        {
            get { return m_DateYMD; }
            set { m_DateYMD = value; }
        }

        public bool DayOrNight
        {
            get 
            {
                int takedate = int.Parse(Date);
                return (takedate / 100 + 8) % 24 < 18 & (takedate / 100 + 8) % 24 > 6;//加8小时换算为北京时间，介于6~18点为白天
            }
        }

        public byte[,] SeaMask
        { 
            get { return m_SeaMask; }
            set { m_SeaMask = value; } 
        }

        public short[,] SolarZenith
        {
            get { return m_SolarZenith; } 
            set {m_SolarZenith = value;}
        }

        public short[,] SolarAzimuth
        {
            get { return m_SolarAzimuth; }
            set { m_SolarAzimuth = value; }
        }

        public short[,] ViewAngle
        {
            get { return m_ViewAngle; }
            set { m_ViewAngle = value; }
        }

        public byte[,] Firemask
        {
            get { return m_Firemask; }
            set { m_Firemask = value; }
        }

        public byte[,] BinaryFiremask
        {
            get 
            {
                for(int m = 0;m < Height;++m)
                    for (int n = 0; n < Width; ++n)
                    {
                        if (DetectFirePlot[m, n] >= 7)
                            m_BinaryFiremask[m, n] = 1;
                        else
                            m_BinaryFiremask[m, n] = 0;

                    }
                return m_BinaryFiremask;
            }
        }

        public List<Point> BinaryFiremaskList
        {
            get 
            {
                if (m_BinaryFiremaskList.Count() > 0)
                    m_BinaryFiremaskList.Clear();
                for(int m = 0;m < Height;++m)
                    for (int n = 0; n < Width; ++n)
                    {
                        if (DetectFirePlot[m, n] >= 7)
                            m_BinaryFiremaskList.Add(new Point(m,n));
                    }
                return m_BinaryFiremaskList;
            }
        }

        public byte[,] FireArea
        {
            get { return m_FireArea; }
            set { m_FireArea = value; }
        }

        public List<Point> FireLineList
        {
            get { return m_FireLineList; }
            set { m_FireLineList = value; }
        }

        public short[] FP_line
        {
            get { return m_FPline; }
            set { m_FPline = value; }
        }
        public short[] FP_sample
        {
            get { return m_FPsample; }
            set { m_FPsample = value; }
        }
        public float[] FP_long
        {
            get { return m_FPlong; }
            set { m_FPlong = value; }
        }
        public float[] FP_lat
        {
            get { return m_FPlat; }
            set { m_FPlat = value; }
        }

        public byte[,] DetectFirePlot
        {
            get { return m_DetectFireplot; }
            set { m_DetectFireplot = value; }
        }


        /// <summary>
        /// 保存有效数据到对象m_hdf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oridata"></param>
        /// <param name="srcdata"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public void CopyData<T>(T[,] oridata, UInt16[, ,] srcdata, int height, int width)
        {
            if (oridata.GetLength(0) != srcdata.GetLength(0) | oridata.GetLength(1) != srcdata.GetLength(1))
            {
                MessageBox.Show("数据不匹配，无法保存！", "Error");
                return;
            }
            UInt16 tem = 0;
            for (int i = 0; i < height; ++i)
                for (int j = 0; j < width; ++j)
                {
                    UInt16.TryParse(oridata[i, j].ToString(), out tem);
                    srcdata[i, j, 1] = tem;
                }
        }

        public void CopyData<T>(T[,] oridata, UInt16[, ,] srcdata, int height, int width, int band)
        {
            if (oridata.GetLength(1) != srcdata.GetLength(0) | oridata.GetLength(0) != srcdata.GetLength(1))
            {
                MessageBox.Show("数据不匹配，无法保存！", "Error");
                return;
            }
            UInt16 tem = 0;

            int m = 0, n = 0;
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    UInt16.TryParse(oridata[m, n].ToString(), out tem);
                    srcdata[j, i, band] = tem;
                    m += 1;
                    if (m == oridata.GetLength(0))
                    {
                        m = 0;
                        n += 1;
                    }
                }
        }

        public void CopyData<T>(T[, ,] oridata, UInt16[, ,] srcdata, int height, int width, int[] dims, int[] odim)
        {
            if (oridata.GetLength(0) != srcdata.GetLength(0) | oridata.GetLength(1) != srcdata.GetLength(1) | dims.Length != odim.Length)
            {
                MessageBox.Show("数据不匹配，无法保存！", "Error");
                return;
            }
            UInt16 tem = 0;

            for (int i = 0; i < height; ++i)
                for (int j = 0; j < width; ++j)
                    for (int od = 0; od < dims.Length; ++od)
                    {
                        int orgdim = odim[od];
                        int srcdim = dims[od];
                        UInt16.TryParse(oridata[i, j, orgdim].ToString(), out tem);
                        srcdata[i, j, srcdim] = tem;
                    }
        }

        public void CopyData<T>(T[,] oridata, T[,] srcdata, int height, int width)
        {
            if (oridata.GetLength(1) != srcdata.GetLength(0) | oridata.GetLength(0) != srcdata.GetLength(1))
            {
                MessageBox.Show("数据不匹配，无法保存！", "Error");
                return;
            }
            int m = 0, n = 0;
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    srcdata[j, i] = oridata[m, n];
                    m += 1;
                    if (m == oridata.GetLength(0))
                    {
                        m = 0;
                        n += 1;
                    }
                }
        }

        public void CopyData<T>(T[] oridata, T[] srcdata, int height)
        {
            if (oridata.GetLength(0) != srcdata.GetLength(0) | oridata.GetLength(0) != height)
            {
                MessageBox.Show("数据不匹配，无法保存！", "Error");
                return;
            }
            for (int i = 0; i < height; ++i)
            {
                srcdata[i] = oridata[i];
            }
        }

        /// <summary>
        /// 将一个字节数组转换为8bit灰度位图
        /// </summary>
        /// <param name="rawValues">显示字节数组</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns>位图</returns>
        public Bitmap ToGrayBitmap(byte[,] rawValues, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// 获取图像参数
            int stride = bmpData.Stride;  // 扫描线的宽度
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置
            int scanBytes = stride * height;   // 用stride宽度，表示这是内存区域的大小

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组
            int posScan = 0, posReal = 0;   // 分别设置两个位置指针，指向源数组和目标数组
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存
            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描
                for (int y = 0; y < width; y++)
                {
                    if (rawValues[x, y] >= 7)
                        pixelValues[posScan++] = 255;
                    else
                        pixelValues[posScan++] = 0;
                }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域

            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bmp.Palette = tempPalette;

            //// 算法到此结束，返回结果
            return bmp;
        }

        /// <summary>
        /// 将3个字节数组转换为8bitRGB位图
        /// </summary>
        /// <param name="index">在原始数组中的通道索引</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns>位图</returns>
        public Bitmap ToRGBBitmap(int[] index, int width, int height)
        {
            if (index.Length != 3)
                return null;

            ////获取原始16bit数据
            byte[, ,] rawValues = new byte[height, width, 3];
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                    for (int d = 0; d < index.Length; ++d)
                    {
                        rawValues[i, j, d] = (byte)(m_hdf.HdfImageData[i, j, index[d]] / 256);
                    }
            }
            // 申请目标位图的变量，并将其内存区域锁定
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            //// 获取图像参数
            int stride = bmpData.Stride;  // 扫描线的宽度
            int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置
            int scanBytes = stride * height;   // 用stride宽度，表示这是内存区域的大小

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组
            int posScan = 0;   // 分别设置两个位置指针，指向源数组和目标数组
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存
            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描
                for (int y = 0; y < width; y++)
                    for (int d = 0; d < index.Length; ++d)
                    {
                        pixelValues[posScan++] = rawValues[x, y, d];
                    }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域


            //// 算法到此结束，返回结果
            return bmp;
        }

        public Bitmap RGBToGrayBitmap(int[] index, int width, int height)
        {
            if (index.Length != 3)
                return null;
            //获取原始16bit数据
            byte[,] rawValues = new byte[height, width];
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    double temp = m_hdf.HdfImageData[i, j, index[0]] * 0.11 + m_hdf.HdfImageData[i, j, index[1]]
                        * 0.59 + (m_hdf.HdfImageData[i, j, index[2]] / 5);
                    rawValues[i, j] = (byte)(temp / 256);
                }
            }

            //// 申请目标位图的变量，并将其内存区域锁定
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// 获取图像参数
            int stride = bmpData.Stride;  // 扫描线的宽度
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置
            int scanBytes = stride * height;   // 用stride宽度，表示这是内存区域的大小

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组
            int posScan = 0;   // 分别设置两个位置指针，指向源数组和目标数组
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存
            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描
                for (int y = 0; y < width; y++)
                {
                    pixelValues[posScan++] = rawValues[x, y];
                }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域

            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bmp.Palette = tempPalette;

            //// 算法到此结束，返回结果
            return bmp;
        }

        /// <summary>
        /// 反射率/辐射率和亮温值计算
        /// </summary>
        /// <param name="reflect">输出参数：反射率</param>
        /// <param name="brightTotempre">输出参数：亮温值</param>
        public void TransformReflect(double[,,] reflect,double[,,] brightTotempre)
       {
           try
           {
               if (Height == 0 | Width == 0)
               {
                   MessageBox.Show("HDF数据为空", "Error");
                   return;
               }
               m_UselessPoint = new byte[Height, Width];
               double h = 6.6256 * Math.Pow(10.0, -34.0);
               double c = 3.0 * Math.Pow(10.0, 8);
               double k = 1.38 * Math.Pow(10.0, -23);
               double[] scale = new double[] { 5.1327 * Math.Pow(10.0,-5.0),3.2367 * Math.Pow(10.0,-5.0),2.7114 * Math.Pow(10.0,-5.0),
                                0.0031495,6.9216 * Math.Pow(10.0,-5.0),0.00084002,0.0007297};
               double[] offset = new double[] { 0, 0, 0, 2730.5835, 2730.5835, 1577.3397, 1658.2213 };
               double[] w = new double[] { 0.65, 0.86, 2.1, 4, 4, 11, 12 };//各波段中心波长um
               for (int m = 0; m < reflect.GetLength(0); ++m)
                   for (int n = 0; n < reflect.GetLength(1); ++n)
                       for (int j = 0; j <= (int)BandNum.Band_32; ++j)
                       {
                           reflect[m, n, j] = scale[j] * (HdfImageData[m, n, j] - offset[j]);
                           if (reflect[m, n, j] < 0)
                           {
                               reflect[m, n, j] = 0;
                               m_UselessPoint[m,n] = 1;//标记不可用点
                           }
                           if (j >= (int)BandNum.Band_21)
                           {
                               int i = j - (int)BandNum.Band_21;//亮温值只保存波段21、22、31、32共4个
                               double te1 = Math.Pow(10.0, 24) * 2 * h * Math.Pow(c, 2.0);
                               double te2 = reflect[m, n, j] * Math.Pow(w[j], 5.0);
                               double te3 = Math.Log(te1 / te2 + 1);
                               brightTotempre[m, n, i] = Math.Pow(10.0, 6.0) * h * c / (k * w[j] * te3);
                               if (j == (int)BandNum.Band_22)
                               {
                                   if (brightTotempre[m, n, i] >= 331)
                                       brightTotempre[m, n, i] = brightTotempre[m, n, i - 1];
                               }
                           }
                       }
           }
           catch (Exception ex)
           {
               MessageBox.Show(ex.Message, "Error");
           }
       }

        /// <summary>
        /// 潜在火点检测，标记为10
        /// </summary>
        /// <param name="reflect">反射率/辐射率</param>
        /// <param name="brightTotempre">亮温值</param>
        public void PotentialFirePlots(double[, ,] reflect, double[, ,] brightTotempre)
        {
            if(Height == 0 | Width ==0)
            {
                MessageBox.Show("HDF数据为空", "Error");
                return;
            }
            //foreach (Point pt in m_UselessPoint)
            //    m_DetectFireplot[pt.X, pt.Y] = 1;//异常值
            for(int m = 0;m < Height;++m)
                for (int n = 0; n < Width; ++n)
                {
                    if (m_UselessPoint[m, n] == 1)
                    {
                        m_DetectFireplot[m, n] = 1;
                        continue;
                    }
                    if (DayOrNight == true)//白天
                    {
                        if (reflect[m, n,0] + reflect[m, n, 1] > 0.9 | brightTotempre[m, n, 3] < 265 | (brightTotempre[m, n, 3] < 285 &
                             reflect[m, n,0] + reflect[m, n,1] > 0.7))
                            m_DetectFireplot[m, n] = 4;//云和水体掩膜
                        else if (brightTotempre[m, n, 1] > 310 & (brightTotempre[m, n, 1] - brightTotempre[m, n, 2]) > 10
                            & reflect[m, n, 1] < 0.3)
                            m_DetectFireplot[m, n] = 10;//潜在火点识别,否则为非火点
                    }
                    else //晚上
                    {
                        if (brightTotempre[m, n, 3] < 265)
                            m_DetectFireplot[m, n] = 4;
                        else if(brightTotempre[m,n,1] > 305 & (brightTotempre[m,n,1] - brightTotempre[m,n,2]) > 10)
                            m_DetectFireplot[m,n] = 10;
                    }
                }
        }
        
        /// <summary>
        ///  检测火点
        /// </summary>
        /// <param name="reflect">反射率/辐射率</param>
        /// <param name="brightTotempre">亮温值</param>
        public void DetectFirePoints(double[, ,] reflect, double[, ,] brightTotempre)
        {
            if (Height == 0 | Width == 0)
            {
                MessageBox.Show("HDF数据为空","Error");
                return;
            }
            double[,] s = new double[Height,Width];//SolarZenith
            double[,] fai = new double[Height,Width];//SolarAzimuth
            double[,] v = new double[Height,Width];//ViewAngle
            for(int m = 0;m < Height; ++m)
                for(int n = 0;n < Width; ++n)
                {
                    s[m,n] = (double)SolarZenith[m,n] / 100 * Math.PI;
                    fai[m,n] = (double)SolarAzimuth[m,n] / 100 * Math.PI;
                    v[m,n] = (double)ViewAngle[m,n] / 360 * Math.PI;
                }
            for(int m = 0;m < Height;++m)
                for(int n = 0;n < Width;++n)
                {
                    byte naw = 0;//8邻域像素近邻水像素数
                    byte nac = 0;//8邻域像素近邻云像素数
                    bool background = false;//背景判断标志，用于跳出窗口循环
                    short np = 0;//窗口像素数,潜在火点不算在内
                    short nf = 0;//背景火像素计数
                    short nc = 0;//背景云像素计数
                    short nw = 0;//背景水像素计数
                    short nv = 0;//有效背景像素计数
                    double T4j = 0;//4um通道有效像素亮温值之和
                    double T11j = 0;//11um通道有效像素亮温值之和
                    double T4_11j = 0;//4um、11um通道有效像素亮温差之和
                    List<double> yx4 = new List<double>();//存储有效像素4um通道亮温值
                    List<double> yx11 = new List<double>();//存储有效像素11um通道亮温值
                    List<double> yx4_11 = new List<double>();//两者之差
                    double T4pj = 0;//背景火像素4um亮温值之和
                    List<double> ck4 = new List<double>();//存储像素该通道亮温值
                    double delta4 = 0;//窗口有效像素4um亮温_平均绝对偏差
                    double delta11 = 0;//窗口有效像素11um亮温_平均绝对偏差
                    double delta4_11 = 0 ;//窗口有效像素亮温差_平均绝对偏差
                    double z4 = 0;//
                    double z_deltaT = 0;//
                    double delta4p = 0;//

                    if(DetectFirePlot[m,n] == 10)//定位潜在火点像素
                    {
                        for(int i = m - 1; i <= m + 1;++i)
                            for(int j = n - 1;j <= n + 1; ++j)
                            {
                                if(i > -1 & j > -1 & i < Height & j < Width & i != m & j != n )
                                {
                                    if (DetectFirePlot[i, j] != 1)
                                    {
                                        if (DetectFirePlot[i, j] == 3)//8邻域像素判断
                                            ++naw;
                                        else if (DetectFirePlot[i, j] == 4)
                                            ++nac;
                                    }
                                }
                            }

                        //3*3窗口到21*21窗口循环判断
                        for(int p = 1;p <= 10; ++p)
                        {
                            //参数清零
                            np = 0;//窗口像素数,潜在火点不算在内
                            nf = 0;//背景火像素计数
                            nc = 0;//背景云像素计数
                            nw = 0;//背景水像素计数
                            nv = 0;//有效背景像素计数
                            T4j = 0;//4um通道有效像素亮温值之和
                            T11j = 0;//11um通道有效像素亮温值之和
                            T4_11j = 0;//4um、11um通道有效像素亮温差之和
                            yx4 = new List<double>();//存储有效像素4um通道亮温值
                            yx11 = new List<double>();//存储有效像素11um通道亮温值
                            yx4_11 = new List<double>();//两者之差
                            T4pj = 0;//背景火像素4um亮温值之和
                            ck4 = new List<double>();//存储像素该通道亮温值
                            delta4 = 0;//窗口有效像素4um亮温_平均绝对偏差
                            delta11 = 0;//窗口有效像素11um亮温_平均绝对偏差
                            delta4_11 = 0;//窗口有效像素亮温差_平均绝对偏差
                            z4 = 0;//
                            z_deltaT = 0;//
                            delta4p = 0;//

                            //当前窗口内像素遍历
                            for(int i = m - p;i <= m + p;++i)
                                for(int j = n - p; j <= n + p; ++j)
                                {
                                    if(i > -1 & j > -1 & i < Height & j < Width & i != m & j != n )
                                    {
                                        if (DetectFirePlot[i, j] != 1)
                                        {
                                            ++np;//窗口像素数,潜在火点不算在内
                                            if ((DayOrNight == true & brightTotempre[i, j, 1] > 325 & (brightTotempre[i, j, 1] - brightTotempre[i, j, 2]) > 20) |
                                                DayOrNight == false & brightTotempre[i, j, 1] > 310 & (brightTotempre[i, j, 1] - brightTotempre[i, j, 2]) > 10)
                                            {
                                                ++nf;//背景火像素计数
                                                T4pj += brightTotempre[i, j, 1];
                                                ck4.Add(brightTotempre[i, j, 1]);
                                            }
                                            else if (DetectFirePlot[i, j] == 4)
                                                ++nc;//背景云像素计数
                                            else if (DetectFirePlot[i, j] == 3)
                                                ++nw;//背景水像素计数
                                            else if (DetectFirePlot[i, j] != 3 & DetectFirePlot[i, j] != 4 & DetectFirePlot[i, j] != 12)
                                            {
                                                ++nv;//有效背景像素计数
                                                T4j += brightTotempre[i, j, 1];//4um、11um通道有效像素亮温值之和
                                                T11j += brightTotempre[i, j, 2];
                                                T4_11j += brightTotempre[i, j, 1] - brightTotempre[i, j, 2];//有效像素4um、11um通道亮温差之和
                                                yx4.Add(brightTotempre[i, j, 1]);//存储有效像素4um、11um通道亮温值及两者之差
                                                yx11.Add(brightTotempre[i, j, 2]);
                                                yx4_11.Add(brightTotempre[i, j, 1] - brightTotempre[i, j, 2]);
                                            }
                                        }
                                    }
                                }

                            if(nv >= 8 & nv >= (2 * p + 1) * (2 * p + 1) / 4 )
                            {
                                T4j /= nv;//4um、11um通道有效像素亮温值集亮温值之差_均值
                                T11j /= nv;
                                T4_11j /= nv;
                                T4pj /= nf;

                                for(int i = 0;i < nv;++i)
                                {
                                    delta4 += Math.Abs(yx4[i] - T4j);
                                    delta11 += Math.Abs(yx11[i] - T11j);
                                    delta4_11 += Math.Abs(yx4_11[i] - T4_11j);
                                }

                                delta4 /= nv;//4um、11um通道有效像素亮温值及亮温值之差_平均绝对偏差
                                delta11 /= nv;
                                delta4_11 /= nv;
                                z4 = (brightTotempre[m,n,1] - T4j) / delta4;
                                z_deltaT = (brightTotempre[m,n,1] - brightTotempre[m,n,2] - T4_11j) / delta4_11;

                                for(int i = 0;i < nf; ++i)
                                    delta4p += Math.Abs(ck4[i] - T4pj);
                                delta4p /= nf;
                                background = true;
                                break;//有效窗口，跳出循环
                            }
                        }

                        //背景特性判别完成,以下为第6步，火点识别 
                        if(background & (brightTotempre[m,n,1] - brightTotempre[m,n,2]) > (T4_11j + 3.5 * delta4_11) 
                            & (brightTotempre[m,n,1] - brightTotempre[m,n,2]) > (T4_11j + 6) &
                            brightTotempre[m,n,1] > (T4j + 3 * delta4) & brightTotempre[m,n,2] > (T11j + delta11 -4 )
                            | delta4p > 5)
                            DetectFirePlot[m,n] = 13;//通过火点识别，设其值为13
                        else if ((DayOrNight == true & brightTotempre[m,n,1] > 360 ) | (DayOrNight == false & brightTotempre[m,n,1] > 320))
                            DetectFirePlot[m,n] = 15;
                        else if (background == false & !((DayOrNight & brightTotempre[m,n,1] > 360) | (DayOrNight == false & brightTotempre[m,n,1] > 320)))
                            DetectFirePlot[m,n] = 6;// 背景判别失败，绝对火点判别失败，标记为未知像素
                    }

                    //白天数据，去除阳光、沙漠边界、沿海虚警干扰;夜晚数据，到此为止
                    double ndvi = 0;//NDVI指数
                    double g = 0;//生成反余弦角度值
                    if(DayOrNight)
                    {
                        if(DetectFirePlot[m,n] == 13 | DetectFirePlot[m,n] == 15)
                        {
                            ndvi = (reflect[m,n,0] - reflect[m,n,1]) / (reflect[m,n,0] + reflect[m,n,1]);
                            g = Math.Acos(Math.Cos(v[m,n]) * Math.Cos(s[m,n]) - Math.Sin(v[m,n]) * Math.Sin(s[m,n]) * Math.Cos(fai[m,n]));
                            if(g < 2 | (g < 8 & reflect[m,n,0] > 0.1 & reflect[m,n,1] > 0.2 & reflect[m,n,2] > 0.12) |
                                (g < 12 & nw + naw > 0))
                                DetectFirePlot[m,n] = 16;//阳光干扰,设其值为16
                            else if (nf > 0.1 * nv & nv >= 4 & reflect[m,n,1] > 0.15 & T4pj < 345 & delta4p < 3
                                & brightTotempre[m,n,1] < (T4pj + 6 * delta4p))
                                DetectFirePlot[m,n] = 18;//沙漠边界干扰，设其值为18
                            else if (reflect[m,n,2] < 0.05 & reflect[m,n,1] < 0.15 & ndvi < 0)
                                DetectFirePlot[m,n] = 19;//沿海虚警,设其值为19
                            else 
                                DetectFirePlot[m,n] = 20;//最终确定的火点
                        }

                        //白天判定完成，进行火点信任计算
                        double c2 = SlopeFunction(z4,2.5,6);
                        double c3 = SlopeFunction(z_deltaT,3,6);
                        double c4 = 1 - SlopeFunction(nac,0,6);
                        double c5 = 1 - SlopeFunction(naw,0,6);
                        double c = 0;

                        if(DayOrNight & DetectFirePlot[m,n] == 20)
                        {
                            double c1 = SlopeFunction(brightTotempre[m,n,1],310,340);
                            c = Math.Pow(c1 * c2 * c3 * c4 * c5,1/5);
                        }
                        else if(DayOrNight == false & (DetectFirePlot[m,n] == 13 | DetectFirePlot[m,n] == 15))
                        {
                            double c1 = SlopeFunction(brightTotempre[m,n,1],305,320);
                            c = Math.Pow(c1 * c2 *c3 , 1/3);
                        }

                        if(c > 0 & c <= 0.3)
                            DetectFirePlot[m,n] = 7;//火点概率低
                        else if(c <= 0.8 & c > 0.3)
                            DetectFirePlot[m,n] = 8;//火点概率中
                        else if(c > 0.8 & c <= 1)
                            DetectFirePlot[m,n] = 9;//火点概率高
                    }
                }
        }

        /// <summary>
        /// 斜坡函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>斜坡系数</returns>
        private double SlopeFunction(double x,double a,double b)
        {
            double s = 0;
            if(x <= a)
                s = 0;
            else if(x > a & x < b)
                s = (x - a) / (b - a);
            else if(x >= b)
                s = 1;
            return s;
        }

        /// <summary>
        /// 计算GEMI统计值
        /// </summary>
        /// <param name="M_s">均值平均值</param>
        /// <param name="SD_s">方差平均值</param>
        private void ComputeGEMI_B(ref double M_s, ref double SD_s)
        {
            int band5 = (int)BandNum.Band_5;
            int band7 = (int)BandNum.Band_7;
            List<double> ExceptionList = new List<double>();
            List<double> VarList = new List<double>();
            //计算每个种子点的3*3窗口内的均值和方差
            foreach (Point po in m_BinaryFiremaskList)
            {
                int x = po.X;
                int y = po.Y;
                const int p = 1;//3*3窗口半径
                //窗口边界坐标
                int bxl = x - p;
                int bxr = x + p;
                int byl = y - p;
                int byr = y + p;
                //防止越界
                if (bxl < 0)
                    bxl = 0;
                if (bxr > Height - 1)
                    bxr = Height - 1;
                if (byl < 0)
                    byl = 0;
                if (byr > Width - 1)
                    byr = Width - 1;
                List<ushort> r5 = new List<ushort>();
                List<ushort> r7 = new List<ushort>();
                //窗口均值和标准差
                for(int i = bxl;i <= bxr;++i)
                    for (int j = byl; j <= byr; ++j)
                    {
                        r5.Add(HdfImageData[i, j, band5]);
                        r7.Add(HdfImageData[i, j, band7]);
                    }
                double exp = 0;
                double vari = 0;
                ComputeGEMIinWindow(r5, r7, ref exp, ref vari);
                ExceptionList.Add(exp);// 均值
                VarList.Add(vari);//标准差
            }
            //计算所有窗口GEMI的均值平均值和标准差平均值
            double exptotal = 0;
            double vartotal = 0;
            foreach (double ex in ExceptionList)
                exptotal += ex;
            M_s = exptotal / ExceptionList.Count;
            foreach (double tmpvar in VarList)
                vartotal += tmpvar;
            SD_s = vartotal / VarList.Count;
        }

        /// <summary>
        /// 计算小窗口的GEMI均值和标准差
        /// </summary>
        /// <param name="r5">r5通道数据</param>
        /// <param name="r7">r7通道数据</param>
        /// <param name="exp">均值</param>
        /// <param name="vari">标准差</param>
        private void ComputeGEMIinWindow(List<ushort> r5, List<ushort> r7, ref double exp, ref double vari)
        {
            double scale5 = 3.6166 * Math.Pow(10, -5);
            double scale7 = 2.7114 * Math.Pow(10, -5);
            double total = 0.0;
            List<double> GEMIofPoint = new List<double>();
            for (int i = 0; i < r5.Count;++i )
            {
                double R5 = r5[i] * scale5;
                double R7 = r7[i] * scale7;
                double elta = (2 * (R7 * R7 - R5 * R5) + 1.5 * R7) / (R7 + R5 + 0.5);
                double temGEMI = elta * (1 - elta) - (R5 - 0.125) / (1 - R5);
                total += temGEMI;
                GEMIofPoint.Add(temGEMI);
            }
            exp = total / r5.Count;
            vari = 0;
            foreach (double db in GEMIofPoint)
                vari += (db - exp) * (db - exp);
            vari /= r5.Count;
            vari = Math.Pow(vari, 1 / 2);
        }

        /// <summary>
        /// 检测火迹地
        /// </summary>
        public void DetectFireArea()
        {
            m_FireArea = new byte[Height,Width];
            if (m_BinaryFiremaskList.Count == 0)
            {
                MessageBox.Show("火点为空，请先检测火点！","Error");
                return;
            }
            foreach (Point po in m_BinaryFiremaskList)
                m_FireArea[po.X, po.Y] = 10;
            double M_s = 0.0;
            double SD_s = 0.0;
            const double N1 = 0.4;
            const double N2 = 1;
            ComputeGEMI_B(ref M_s, ref SD_s);//统计特征
            List<Point> SeedPoint = new List<Point>(m_BinaryFiremaskList);//上一次计算得到的种子点
            int band5 = (int)BandNum.Band_5;//计算GEMI_B的参数
            int band7 = (int)BandNum.Band_7;
            double scale5 = 3.6166 * Math.Pow(10, -5);
            double scale7 = 2.7114 * Math.Pow(10, -5);
            int count = 0;
            while (SeedPoint.Count != 0 & count < 200)
            {
                List<Point> SeedPointBefore = new List<Point>(SeedPoint);//保存上一次的种子点
                SeedPoint.Clear();//清空以前的种子点
                Byte[,] seed = new byte[Height, Width];
                count++;
                foreach (Point po in SeedPointBefore)
                {
                    int x = po.X;
                    int y = po.Y;
                    const int p = 1;//3*3窗口半径
                    //窗口边界坐标
                    int bxl = x - p;
                    int bxr = x + p;
                    int byl = y - p;
                    int byr = y + p;
                    //防止越界
                    if (bxl < 0)
                        bxl = 0;
                    if (bxr > Height - 1)
                        bxr = Height - 1;
                    if (byl < 0)
                        byl = 0;
                    if (byr > Width - 1)
                        byr = Width - 1;
                    List<ushort> r5 = new List<ushort>();
                    List<ushort> r7 = new List<ushort>();
                    //窗口均值和标准差
                    for (int i = bxl; i <= bxr; ++i)
                        for (int j = byl; j <= byr; ++j)
                        {
                            r5.Add(HdfImageData[i, j, band5]);
                            r7.Add(HdfImageData[i, j, band7]);
                        }
                    double M_w = 0;
                    double SD_w = 0;
                    ComputeGEMIinWindow(r5, r7, ref M_w, ref SD_w);
                    if (SD_w < N1 * SD_s && M_w >= (M_s - N2 * SD_s) && M_w <= (M_s + N2 * SD_s))
                    {
                        for (int i = bxl; i <= bxr; ++i)
                            for (int j = byl; j <= byr; ++j)
                            {
                                if(m_FireArea[i,j] == 0)
                                    seed[i, j] = 1;
                            }
                    }
                    else
                    {
                        for (int i = bxl; i <= bxr; ++i)
                            for (int j = byl; j <= byr; ++j)
                            {
                                double R5 = HdfImageData[i, j, band5] * scale5;
                                double R7 = HdfImageData[i, j, band7] * scale7;
                                double elta = (2 * (R7 * R7 - R5 * R5) + 1.5 * R7) / (R7 + R5 + 0.5);
                                double temGEMI = elta * (1 - elta) - (R5 - 0.125) / (1 - R5);
                                if (temGEMI > M_w - SD_w && temGEMI < M_w + SD_w & m_FireArea[i,j] == 0)
                                    seed[i, j] = 1;
                            }
                    }
                }
                //保存当前计算得到的火点
                for (int i = 0; i < Height; ++i)
                    for (int j = 0; j < Width; ++j)
                    {
                        if (seed[i, j] == 1)
                        {
                            SeedPoint.Add(new Point(i, j));
                            m_FireArea[i, j] = 10;
                        }
                    }
            }
        }

        /// <summary>
        /// 在过火面积上计算火迹线
        /// </summary>
        /// <param name="line">火迹线</param>
        public void DetectFireLine()
        {
            if(FireArea.Length == 0)
            {
                MessageBox.Show("过火面积尚未检测！", "Error");
                return;
            }
            m_FireLineList = new List<Point>();
            for(int m = 0; m < Height;++m)
                for (int n = 0; n < Width; ++n)
                {
                    if (m_FireArea[m, n] > 0)
                    {
                        int p = 2;
                        //窗口边界坐标
                        int bxl = m - p;
                        int bxr = m + p;
                        int byl = n - p;
                        int byr = n + p;
                        //防止越界
                        if (bxl < 0)
                            bxl = 0;
                        if (bxr > Height - 1)
                            bxr = Height - 1;
                        if (byl < 0)
                            byl = 0;
                        if (byr > Width - 1)
                            byr = Width - 1;
                        int count = 0;//总数
                        int firecount = 0;//火点数
                        for (int i = bxl; i <= bxr; ++i)
                            for (int j = byl; j <= byr; ++j)
                            {
                                count++;
                                if (m_FireArea[i, j] > 0)
                                    firecount++;
                            }
                        double rate = (double)firecount / (double)count;
                        if (rate> 0.3 & rate < 0.7)
                        {
                            m_FireLineList.Add(new Point(m, n));
                        }
                    }
                }
        }

    }

}
