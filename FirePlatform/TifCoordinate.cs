using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsFirePlatform
{
    public class TifCoordinate
    {
        //X方向上每像素的分辨率
        public double m_ScaleX;
        //Y方向上每像素的分辨率,负数
        public double m_ScaleY;
        //旋转系统
        public int m_ShiftD;
        //旋转系统
        public int m_ShiftB;
        //左上角像素X坐标
        public float m_CoorX;
        //右上角像素Y坐标
        public float m_CoorY;
        //所有像素的经纬度
        public float[,] m_FullPixelLat;//Y坐标
        public float[,] m_FullPixelLong;//X坐标

        /// <summary>
        /// 在构造函数中对所有参数初始化
        /// </summary>
        public TifCoordinate()
        {
            if (HdfImage.m_hdf.HdfLatitude.Length == 0 | HdfImage.m_hdf.HdfLongitude.Length == 0)
            {
                MessageBox.Show("原始图像经纬度坐标未读取！", "Error");
                return;
            }
            m_FullPixelLat = new float[HdfImage.m_hdf.Height, HdfImage.m_hdf.Width];
            m_FullPixelLong = new float[HdfImage.m_hdf.Height, HdfImage.m_hdf.Width];
            Interpolation(HdfImage.m_hdf.HdfLatitude,m_FullPixelLat);
            Interpolation(HdfImage.m_hdf.HdfLongitude,m_FullPixelLong);
            //m_ScaleX = (m_FullPixelLong[0, HdfImage.m_hdf.Width - 1] - m_FullPixelLong[0, 0]) / HdfImage.m_hdf.Width;
            //m_ScaleY = (m_FullPixelLat[HdfImage.m_hdf.Height - 1, 0] - m_FullPixelLat[0, 0]) / HdfImage.m_hdf.Height;
            m_ShiftB = 0;
            m_ShiftD = 0;
            //m_CoorX = m_FullPixelLong[0, 0];
            //m_CoorY = m_FullPixelLat[0, 0];
            m_ScaleX = 0.00145996;
            m_ScaleY = -0.001651884;
            m_CoorX = 111.4044F;
            m_CoorY = 39.06221F;
        }

        /// <summary>
        /// 将HDF中的经纬度插值到原始图像大小
        /// </summary>
        /// <param name="src">原矩阵</param>
        /// <param name="dst">目标矩阵</param>
        private static void Interpolation(float[,] src,float[,] dst)
        {
            const int interval = 5;
            const int offset = 2;
            //for(int i = 0;i < src.GetLength(0); ++i)
            //    for (int j = 0; j < src.GetLength(1); ++j)
            //    {
            //        dst[i * interval + offset, j * interval + offset] = src[i, j];
            //    }
            //计算列
            for (int j = 0; j < src.GetLength(1); ++j)
            {
                int dj = j * interval + offset;
                for (int i = 0; i < src.GetLength(0) - 1; ++i)
                {
                    float gap = (src[i + 1, j] - src[i, j]) / 5;
                    int di = i * interval + offset;
                    for (int m = 0; m < interval; ++m)
                    {
                        dst[di + m, dj] = src[i, j] + gap * m;
                    }
                    //计算前面几行
                    if (i == 0)
                    {
                        int temp = offset;
                        while (temp > 0)
                        {
                            dst[di - temp, dj] = dst[di, dj] - gap * temp;
                            --temp;
                        }
                    }
                    //计算最后几行
                    if (i == src.GetLength(0) - 2)
                    {
                        di = (i + 1) * interval + offset;//取原始矩阵最后一行，更新di及以后
                        for (int temp = 0; temp < dst.GetLength(0) - di; ++temp)
                        {
                            dst[temp + di, dj] = dst[di, dj] + gap * temp;
                        }
                    }
                }
            }
            //每五列计算中间一列的所有值，然后插值出中间的其他列
            for (int i = 0; i < dst.GetLength(0); ++i)
            {
                for (int j = 0; j < src.GetLength(1) - 1; ++j)
                {
                    int dj1 = j * interval + offset;
                    int dj2 = (j + 1) * interval + offset;
                    float gap = (dst[i, dj2] - dst[i, dj1]) / 5;
                    for (int m = 1; m < interval; ++m)
                    {
                        dst[i, dj1 + m] = dst[i, dj1] + gap * m;
                    }
                    //计算前几列
                    if (j == 0)
                    {
                        int temp = offset;
                        while (temp > 0 )
                        {
                            dst[i, dj1 - temp] = dst[i, dj1] - gap * temp;
                            --temp; 
                        }
                    }
                    //计算最后几列
                    if (j == src.GetLength(1) - 2)
                    {
                        for(int temp = 1; temp < dst.GetLength(1) - dj2; ++temp)
                            dst[i,dj2 + temp] = dst[i,dj2] + gap * temp;
                    }
                }
            }
        }

        /// <summary>
        /// 为tif、bmp、jpg图像存储经纬度坐标
        /// </summary>
        /// <param name="filName">图像的全路径</param>
        public void creatImg(string filName)   　　//传入要保存的文件名，必须与图片名字一模一样！
        {
            string ext = Path.GetExtension(filName);
            switch (ext)
            {
                case ".tif":
                    ext = ".tfw";
                    break;
                case ".jpg":
                    ext = ".jgw";
                    break;
                case ".bmp":
                    ext = ".bpw";
                    break;
                default:                    
                    MessageBox.Show("请输入有效图像:bmp、tif和jpg!", "Error");
                    return;
            }
            string tfwpath = filName.Substring(0,filName.Length - 4);
            if (File.Exists(tfwpath + ext)) 　　//判断是否已有
            {
                File.Delete(tfwpath + ext);　　//如果有则删除
            }
            using (StreamWriter sw = File.AppendText(tfwpath + ext))//新建空文本，后缀为tfw
            {
                sw.WriteLine(m_ScaleX);

                //往文本中写入
                sw.WriteLine(m_ShiftD);
                sw.WriteLine(m_ShiftB);
                sw.WriteLine(m_ScaleY);
                sw.WriteLine(m_CoorX);
                sw.WriteLine(m_CoorY);
                sw.Close(); //关闭文本
            }　　
        }

        /// <summary>
        /// 将矩阵中的平面坐标转换为显示的经纬度坐标
        /// </summary>
        /// <param name="x">矩阵中的x坐标</param>
        /// <param name="y">矩阵中的y坐标</param>
        /// <param name="show_x">显示的经度坐标</param>
        /// <param name="show_y">显示的纬度坐标</param>
        public void ShowCoordinate(int x,int y,ref float show_x,ref float show_y)
        {
            show_x = m_CoorX + (float)(x * m_ScaleX);
            show_y = m_CoorY + (float)(y * m_ScaleY);
        }

        /// <summary>
        /// 将矩阵中的经纬度坐标转换为平面坐标
        /// </summary>
        /// <param name="x">矩阵中的x坐标</param>
        /// <param name="y">矩阵中的y坐标</param>
        /// <param name="show_x">显示的经度坐标</param>
        /// <param name="show_y">显示的纬度坐标</param>
        public void ShowCoordinate2XY(double show_x, double show_y, ref int x, ref int y)
        {
            x = (int)((show_x - m_CoorX) / m_ScaleX);
            y = (int)((show_y - m_CoorY) / m_ScaleY);
        }
    }
}
