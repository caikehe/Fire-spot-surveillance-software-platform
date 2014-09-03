using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars.Helpers;
using System.IO;
using System.Linq;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessing;


namespace WindowsFormsFirePlatform
{
    public partial class Form1 : RibbonForm
    {
        #region 声明类的成员变量;
        //TOCControl控件变量
        private ITOCControl2 m_tocControl = null;
        //TOCControl中Map菜单
        private IToolbarMenu m_menuMap = null;
        //TOCControl中图层菜单
        private IToolbarMenu m_menuLayer = null;

        //保存当前皮肤名
        private ControlsSynchronizer m_controlsSynchronizer = null;
        private ESRI.ArcGIS.Controls.IMapControl3 m_mapControl = null;
        private ESRI.ArcGIS.Controls.IPageLayoutControl2 m_pageLayoutControl = null;

        // private IMapDocument pMapDocument;
        private string sMapUnits = " ";
        private int flag = 0;

        double formWidth;//窗体原始宽度
        double formHeight;//窗体原始高度
        double scaleX;//水平缩放比例
        double scaleY;//垂直缩放比例
        Dictionary<string, string> controlInfo = new Dictionary<string, string>();//控件中心Left,Top,控件Width,控件Height,控件字体Size
        #endregion  
        
        #region 界面初始化及鹰眼的实现
        public Form1()
        {
            InitializeComponent();
            InitSkinGallery();
            GetAllInitInfo(this.Controls[1]);
        }

        void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins1, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string str = Application.StartupPath.ToString();
            str = Directory.GetParent(str).ToString();
            str = Directory.GetParent(str).ToString();//两级父目录找到icon路径
            this.Icon = new System.Drawing.Icon(str + @"/Images/Earth.ico");

            HdfImage.m_hdf = new HdfImage();

            sMapUnits = "Unknown";

            m_tocControl = (ITOCControl2)this.axTOCControl1.Object;

            m_menuMap = new ToolbarMenuClass();
            m_menuLayer = new ToolbarMenuClass();

            // 取得 MapControl 和 PageLayoutControl 的引用
            m_mapControl = (IMapControl3)this.axMapControl1.Object;
            m_pageLayoutControl = (IPageLayoutControl2)this.axPageLayoutControl1.Object;

            //初始化controls synchronization calss
            m_controlsSynchronizer = new ControlsSynchronizer(m_mapControl, m_pageLayoutControl);

            //把MapControl和PageLayoutControl绑定起来(两个都指向同一个Map),然后设置MapControl为活动的Control
            m_controlsSynchronizer.BindControls(true);

            ///为了在切换MapControl和PageLayoutControl视图同步，要添加Framework Control
            m_controlsSynchronizer.AddFrameworkControl(axToolbarControl1.Object);
            m_controlsSynchronizer.AddFrameworkControl(this.axTOCControl1.Object);

            //添加打开命令按钮到工具条
            OpenNewMapDocument openMapDoc = new OpenNewMapDocument(m_controlsSynchronizer);
            axToolbarControl1.AddItem(openMapDoc, -1, 0, false, -1, esriCommandStyles.esriCommandStyleIconOnly);

            {
                //添加自定义菜单项到TOCCOntrol的Map菜单中
                //打开文档菜单
                m_menuMap.AddItem(new OpenNewMapDocument(m_controlsSynchronizer), -1, 0, false, esriCommandStyles.esriCommandStyleIconAndText);
                //添加数据菜单
                m_menuMap.AddItem(new ControlsAddDataCommandClass(), -1, 1, false, esriCommandStyles.esriCommandStyleIconAndText);
                //打开全部图层菜单
                m_menuMap.AddItem(new LayerVisibility(), 1, 2, false, esriCommandStyles.esriCommandStyleTextOnly);
                //关闭全部图层菜单
                m_menuMap.AddItem(new LayerVisibility(), 2, 3, false, esriCommandStyles.esriCommandStyleTextOnly);

                //以二级菜单的形式添加内置的“选择”菜单
                m_menuMap.AddSubMenu("esriControls.ControlsFeatureSelectionMenu", 4, true);
                //以二级菜单的形式添加内置的“地图浏览”菜单
                m_menuMap.AddSubMenu("esriControls.ControlsMapViewMenu", 5, true);

                //添加自定义菜单项到TOCCOntrol的图层菜单中
                m_menuLayer = new ToolbarMenuClass();
                //添加“移除图层”菜单项
                m_menuLayer.AddItem(new RemoveLayer(), -1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
                //添加“放大到整个图层”菜单项
                m_menuLayer.AddItem(new ZoomToLayer(), -1, 1, true, esriCommandStyles.esriCommandStyleTextOnly);

                //设置菜单的Hook
                m_menuLayer.SetHook(m_mapControl);
                m_menuMap.SetHook(m_mapControl);

                platdate.DateTime = DateTime.Now;
            }
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl2.SelectedTabPageIndex == 0)
            {
                //激活MapControl
                m_controlsSynchronizer.ActivateMap();
            }
            else
            {
                //激活PageLayoutControl
                m_controlsSynchronizer.ActivatePageLayout();
            }
        }

        private void axToolbarControl1_OnMouseDown(object sender, IToolbarControlEvents_OnMouseDownEvent e)
        {
            // 取得鼠标所在工具的索引号
            int index = axToolbarControl1.HitTest(e.x, e.y, false);

            if (index != -1)
            {
                // 取得鼠标所在工具的 ToolbarItem
                IToolbarItem toolbarItem = axToolbarControl1.GetItem(index);

                // 设置状态栏信息
                MessageLabel.Text = toolbarItem.Command.Message;
            }
            else
            {
                MessageLabel.Text = " 就绪 ";
            }
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            // 显示当前比例尺
            ScaleLabel.Text = " 比例尺 1:" + ((long)this.axMapControl1.MapScale).ToString();

            // 显示当前坐标
            CoordinateLabel.Text = String.Format(" 当前坐标 X = {0} Y = {1} {2}", (float)e.mapX, (float)e.mapY, this.axMapControl1.MapUnits.ToString().Substring(4));

            if (e.button == 2)
            {
                //弹出右键菜单
                m_menuMap.PopupMenu(e.x, e.y, m_mapControl.hWnd);
            }
            this.axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

            //下面代码段用于捕捉鼠标的点击状况，用于手工测距
            if (flag == 1)
            {
                IPoint startPoint = new PointClass();
                IPoint endPoint = new PointClass();
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                IGeometry geometry = null;
                geometry = axMapControl1.TrackLine();
                axMapControl1.Map.SelectByShape(geometry, null, false);
                axMapControl1.Refresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                //startPoint和endPoint分别表示经纬度坐标下的起始和终点时的鼠标坐标
                startPoint = geometry.Envelope.LowerLeft;
                endPoint = geometry.Envelope.UpperRight;
                TifCoordinate tif = new TifCoordinate();
                int x1 = 0 , y1 = 0 , x2 = 0, y2 = 0;
                tif.ShowCoordinate2XY(startPoint.X, startPoint.Y,ref x1, ref y1);
                tif.ShowCoordinate2XY(endPoint.X, endPoint.Y , ref x2 ,ref y2);
                double dis = Math.Sqrt(Math.Pow(x1 - x2 , 2) + Math.Pow(y1 - y2 ,2));
                labelControl3.Text += (float)dis + "千米";
            }
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            flag = 0;
        }

        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            esriUnits mapUnits = axMapControl1.MapUnits;
            switch (mapUnits)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "Centimeters";
                    break;
                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "Decimal Degrees";
                    break;
                case esriUnits.esriDecimeters:
                    sMapUnits = "Decimeters";
                    break;
                case esriUnits.esriFeet:
                    sMapUnits = "Feet";
                    break;
                case esriUnits.esriInches:
                    sMapUnits = "Inches";
                    break;
                case esriUnits.esriKilometers:
                    sMapUnits = "Kilometers";
                    break;
                case esriUnits.esriMeters:
                    sMapUnits = "Meters";
                    break;
                case esriUnits.esriMiles:
                    sMapUnits = "Miles";
                    break;
                case esriUnits.esriMillimeters:
                    sMapUnits = "Millimeters";
                    break;
                case esriUnits.esriNauticalMiles:
                    sMapUnits = "NauticalMiles";
                    break;
                case esriUnits.esriPoints:
                    sMapUnits = "Points";
                    break;
                case esriUnits.esriUnknownUnits:
                    sMapUnits = "Unknown";
                    break;
                case esriUnits.esriYards:
                    sMapUnits = "Yards";
                    break;
            }

            // 当主地图显示控件的地图更换时，鹰眼中的地图也跟随更换
            this.axMapControl2.Map = new MapClass();

            // 添加主地图控件中的所有图层到鹰眼控件中
            for (int i = 1; i <= this.axMapControl1.LayerCount; i++)
            {
                this.axMapControl2.AddLayer(this.axMapControl1.get_Layer(this.axMapControl1.LayerCount - i));
            }

            // 设置 MapControl 显示范围至数据的全局范围
            this.axMapControl2.Extent = this.axMapControl1.FullExtent;

            // 刷新鹰眼控件地图
            this.axMapControl2.Refresh();
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            // 得到新范围
            IEnvelope pEnv = (IEnvelope)e.newEnvelope;
            IGraphicsContainer pGra = axMapControl2.Map as IGraphicsContainer;
            IActiveView pAv = pGra as IActiveView;

            // 在绘制前，清除 axMapControl2 中的任何图形元素
            pGra.DeleteAllElements();
            IRectangleElement pRectangleEle = new RectangleElementClass();
            IElement pEle = pRectangleEle as IElement;
            pEle.Geometry = pEnv;

            // 设置鹰眼图中的红线框
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 255;

            // 产生一个线符号对象
            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = pColor;

            // 设置颜色属性
            pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 0;

            // 设置填充符号的属性
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutline;
            IFillShapeElement pFillShapeEle = pEle as IFillShapeElement;
            pFillShapeEle.Symbol = pFillSymbol;
            pGra.AddElement((IElement)pFillShapeEle, 0);

            // 刷新
            pAv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (this.axMapControl2.Map.LayerCount != 0)
            {
                // 按下鼠标左键移动矩形框
                if (e.button == 1)
                {
                    IPoint pPoint = new PointClass();
                    pPoint.PutCoords(e.mapX, e.mapY);
                    IEnvelope pEnvelope = this.axMapControl1.Extent;
                    pEnvelope.CenterAt(pPoint);
                    this.axMapControl1.Extent = pEnvelope;
                    this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
            }
            // 按下鼠标右键绘制矩形框
            else if (e.button == 2)
            {
                IEnvelope pEnvelop = this.axMapControl2.TrackRectangle();
                this.axMapControl1.Extent = pEnvelop;
                this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        private void axMapControl2_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            // 如果不是左键按下就直接返回
            if (e.button != 1) return;
            IPoint pPoint = new PointClass();
            pPoint.PutCoords(e.mapX, e.mapY);
            this.axMapControl1.CenterAt(pPoint);
            this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            //如果不是右键按下直接返回
            if (e.button != 2)
                return;

            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            ILayer layer = null;
            object other = null;
            object index = null;

            //判断所选菜单的类型
            m_tocControl.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);

            //确定选定的菜单类型，Map或是图层菜单
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_tocControl.SelectItem(map, null);
            else
                m_tocControl.SelectItem(layer, null);

            //设置CustomProperty为layer (用于自定义的Layer命令)
            m_mapControl.CustomProperty = layer;

            //弹出右键菜单
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_menuMap.PopupMenu(e.x, e.y, m_tocControl.hWnd);
            if (item == esriTOCControlItem.esriTOCControlItemLayer)
                m_menuLayer.PopupMenu(e.x, e.y, m_tocControl.hWnd);
        }

        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            esriTOCControlItem toccItem = esriTOCControlItem.esriTOCControlItemNone;
            ILayer iLayer = null;
            IBasicMap iBasicMap = null;
            object unk = null;
            object data = null;
            if (e.button == 1)
            {
                axTOCControl1.HitTest(e.x, e.y, ref toccItem, ref iBasicMap, ref iLayer, ref unk, ref data);
                System.Drawing.Point pos = new System.Drawing.Point(e.x, e.y);
                if (toccItem == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    ESRI.ArcGIS.Carto.ILegendClass pLC = new LegendClassClass();
                    ESRI.ArcGIS.Carto.ILegendGroup pLG = new LegendGroupClass();
                    if (unk is ILegendGroup)
                    {
                        pLG = (ILegendGroup)unk;
                    }
                    pLC = pLG.get_Class((int)data);
                    ISymbol pSym;
                    pSym = pLC.Symbol;
                    ESRI.ArcGIS.DisplayUI.ISymbolSelector pSS =
                        new ESRI.ArcGIS.DisplayUI.SymbolSelector();
                    bool bOK = false;
                    pSS.AddSymbol(pSym);
                    bOK = pSS.SelectSymbol(0);
                    if (bOK)
                    {
                        pLC.Symbol = pSS.GetSymbolAt(0);
                    }
                    this.axMapControl1.ActiveView.Refresh();
                    this.axTOCControl1.Refresh();
                }
            }

        }
        #endregion

        #region 菜单栏-文件
        private void New_ItemClick(object sender, ItemClickEventArgs e)
        {
            //询问是否保存当前地图
            DialogResult res = MessageBox.Show("是否保存当前地图?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                //如果要保存，调用另存为对话框
                ICommand command = new ControlsSaveAsDocCommand();
                if (m_mapControl != null)
                    command.OnCreate(m_controlsSynchronizer.MapControl.Object);
                else
                    command.OnCreate(m_controlsSynchronizer.PageLayoutControl.Object);
                command.OnClick();
            }
            //创建新的地图实例
            IMap map = new Map();
            map.Name = "Map";
            m_controlsSynchronizer.MapControl.DocumentFilename = string.Empty;
            //更新新建地图实例的共享地图文档
            m_controlsSynchronizer.ReplaceMap(map);
        }

        private void Open_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                DialogResult result = MessageBox.Show("是否保存当前地图？", "警告",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.Yes)
                    this.Save_Click(null, null);
            }
            OpenNewMapDocument openMapDoc = new OpenNewMapDocument(m_controlsSynchronizer);
            openMapDoc.OnCreate(m_controlsSynchronizer.MapControl.Object);
            openMapDoc.OnClick();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            // 首先确认当前地图文档是否有效
            if (null != m_pageLayoutControl.DocumentFilename && m_mapControl.CheckMxFile(
                m_pageLayoutControl.DocumentFilename))
            {
                // 创建一个新的地图文档实例
                IMapDocument mapDoc = new MapDocumentClass();
                // 打开当前地图文档
                mapDoc.Open(m_pageLayoutControl.DocumentFilename, string.Empty);
                // 用 PageLayout 中的文档替换当前文档中的 PageLayout 部分
                mapDoc.ReplaceContents((IMxdContents)m_pageLayoutControl.PageLayout);
                // 保存地图文档
                mapDoc.Save(mapDoc.UsesRelativePaths, false);
                mapDoc.Close();
            }
        }

        private void AddData_ItemClick(object sender, ItemClickEventArgs e)
        {
            int currentLayerCount = this.axMapControl1.LayerCount;
            ICommand pCommand = new ControlsAddDataCommand();
            pCommand.OnCreate(this.axMapControl1.Object);
            pCommand.OnClick();

            IMap pMap = this.axMapControl1.Map;
            this.m_controlsSynchronizer.ReplaceMap(pMap);
        }

        private void Save_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 首先确认当前地图文档是否有效
            if (null != m_pageLayoutControl.DocumentFilename && m_mapControl.CheckMxFile(
                m_pageLayoutControl.DocumentFilename))
            {
                // 创建一个新的地图文档实例
                IMapDocument mapDoc = new MapDocumentClass();
                // 打开当前地图文档
                mapDoc.Open(m_pageLayoutControl.DocumentFilename, string.Empty);
                // 用 PageLayout 中的文档替换当前文档中的 PageLayout 部分
                mapDoc.ReplaceContents((IMxdContents)m_pageLayoutControl.PageLayout);
                // 保存地图文档
                mapDoc.Save(mapDoc.UsesRelativePaths, false);
                mapDoc.Close();
            }
        }

        private void SaveAs_ItemClick(object sender, ItemClickEventArgs e)
        {
            //如果当前视图为MapControl时，提示用户另存为操作将丢失PageLayoutControl中的设置
            if (m_controlsSynchronizer.ActiveControl is IMapControl3)
            {
                if (MessageBox.Show("另存为地图文档将丢失制版视图的设置\r\n您要继续吗?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            //调用另存为命令
            ICommand command = new ControlsSaveAsDocCommand();
            command.OnCreate(m_controlsSynchronizer.ActiveControl);
            command.OnClick();
        }

        private void Exit_ItemClick(object sender, ItemClickEventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region 菜单栏-HDF数据处理
        private void butOriginalData_ItemClick(object sender, ItemClickEventArgs e)
        {
            //提示在加载MODIS数据前加载省界图层
            if (this.axMapControl1.LayerCount == 0)
            {
                MessageBox.Show("请先加载省界线!");
                return;
            }

            MessageBox.Show("请打开 MODIS_02_1KM 分辨率的HDF文件！", "Open HDF file");
            OpenFileDialog filedlg = new OpenFileDialog();
            filedlg.Filter = "Hierarchical Data Format(*.hdf)|*.hdf";
            filedlg.Multiselect = false;
            filedlg.Title = "Open HDF（MODIS_02）file";
            if (filedlg.ShowDialog() == DialogResult.OK)
            {
                string filepath = filedlg.FileName;
                FileInfo filepathInfo = new FileInfo(filepath);
                string tempfilepath = filepathInfo.Name;
                string[] filenamelist = tempfilepath.Split('.');
                string save = HdfImage.m_firmaskpath;
                if (!Directory.Exists(save))
                    Directory.CreateDirectory(save);
                ProgressForm prog = new ProgressForm();//调出进度条
                int pronum = 0;
                prog.MAX = 35;
                prog.MIN = 0;
                prog.Show(this);
                prog.setpos(pronum++);
                COM_IDL_connectLib.COM_IDL_connect oCOM_IDL1 = new COM_IDL_connectLib.COM_IDL_connect();
                oCOM_IDL1.CreateObject(0, 0, 0);
                oCOM_IDL1.SetIDLVariable("dir", filepath);
                oCOM_IDL1.ExecuteString(".compile '" + Application.StartupPath.ToString() + "\\ReadMOD021KM_hdf_1.pro'");
                prog.setpos(pronum++);
                oCOM_IDL1.ExecuteString("ReadMOD021KM_hdf_1,dir,Longitude_data=Longitude_data,Band_1=Band_1,Band_2=Band_2,Band_5=Band_5,Band_7=Band_7");
                prog.setpos(pronum++);
                COM_IDL_connectLib.COM_IDL_connect oCOM_IDL2 = new COM_IDL_connectLib.COM_IDL_connect();
                oCOM_IDL2.CreateObject(0, 0, 0);
                oCOM_IDL2.SetIDLVariable("dir", filepath);
                oCOM_IDL2.SetIDLVariable("savepath", save);
                oCOM_IDL2.ExecuteString(".compile '" + Application.StartupPath.ToString() + "\\ReadMOD021KM_hdf_2.pro'");
                prog.setpos(pronum++);
                oCOM_IDL2.ExecuteString("ReadMOD021KM_hdf_2,dir,savepath,Latitude_data=Latitude_data,Band_21=Band_21,Band_22=Band_22,Band_31=Band_31,Band_32=Band_32");
                prog.setpos(pronum++);
                object band_1 = oCOM_IDL1.GetIDLVariable("Band_1");//通道1
                prog.setpos(pronum++);
                object band_2 = oCOM_IDL1.GetIDLVariable("Band_2");//通道2
                prog.setpos(pronum++);
                object band_5 = oCOM_IDL1.GetIDLVariable("Band_5");//通道5
                prog.setpos(pronum++);
                object band_7 = oCOM_IDL1.GetIDLVariable("Band_7");//通道7
                prog.setpos(pronum++);
                object band_21 = oCOM_IDL2.GetIDLVariable("Band_21");//通道21
                prog.setpos(pronum++);
                object band_22 = oCOM_IDL2.GetIDLVariable("Band_22");//通道22
                prog.setpos(pronum++);
                object band_31 = oCOM_IDL2.GetIDLVariable("Band_31");//通道31
                prog.setpos(pronum++);
                object band_32 = oCOM_IDL2.GetIDLVariable("Band_32");//通道32
                prog.setpos(pronum++);
                object objlat = oCOM_IDL1.GetIDLVariable("Latitude_data");//纬度
                prog.setpos(pronum++);
                object objlong = oCOM_IDL1.GetIDLVariable("Longitude_data");//经度
                prog.setpos(pronum++);//14

                try
                {
                    //波段1
                    if (band_1.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_1 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf = new HdfImage(height, width);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_1);
                    }
                    else if (band_1.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_1 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf = new HdfImage(height, width);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_1);
                    }
                    else if (band_1.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_1 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf = new HdfImage(height, width);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_1);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    //对日期赋值
                    HdfImage.m_hdf.Date = filenamelist[2];
                    int year = Int16.Parse(filenamelist[1].Substring(1,4));
                    int days = Int16.Parse(filenamelist[1].Substring(5));
                    DateTime tbase = Convert.ToDateTime(string.Format("{0}-1-1", year));
                    TimeSpan ts = new TimeSpan(days,0,0,0);
                    tbase += ts;
                    HdfImage.m_hdf.DateYMD = string.Format(".{0}.{1}.{2}.{3}.{4}", tbase.Year, tbase.Month, tbase.Day, 
                                               Int16.Parse(HdfImage.m_hdf.Date.Substring(0, 2)), Int16.Parse(HdfImage.m_hdf.Date.Substring(2, 2)));

                    prog.setpos(pronum++);

                    //波段2
                    if (band_2.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_2 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_2);
                    }
                    else if (band_2.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_2 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_2);
                    }
                    else if (band_2.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_2 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_2);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    //波段5
                    if (band_5.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_5 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_5);
                    }
                    else if (band_5.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_5 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_5);
                    }
                    else if (band_5.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_5 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_5);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    //波段7
                    if (band_7.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_7 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_7);
                    }
                    else if (band_7.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_7 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_7);
                    }
                    else if (band_7.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_7 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_7);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    //波段21
                    if (band_21.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_21 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_21);
                    }
                    else if (band_21.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_21 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_21);
                    }
                    else if (band_21.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_21 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_21);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    //波段22
                    if (band_22.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_22 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_22);
                    }
                    else if (band_22.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_22 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_22);
                    }
                    else if (band_22.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_22 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_22);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    //波段31
                    if (band_31.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_31 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_31);
                    }
                    else if (band_31.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_31 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_31);
                    }
                    else if (band_31.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_31 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_31);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    //波段32
                    if (band_32.GetType() == typeof(byte[,]))//二维byte数据
                    {
                        byte[,] im = band_32 as byte[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_32);
                    }
                    else if (band_32.GetType() == typeof(int[,]))//二维int数据
                    {
                        int[,] im = band_32 as int[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_32);
                    }
                    else if (band_32.GetType() == typeof(short[,]))//二维uint数据
                    {
                        short[,] im = band_32 as short[,];
                        int height = im.GetLength(1);
                        int width = im.GetLength(0);
                        HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, (int)BandNum.Band_32);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);

                    if (objlat.GetType() == typeof(float[]) && objlong.GetType() == typeof(float[]))
                    {
                        float[] lat = objlat as float[];
                        int height = lat.Length;
                        float[] longi = objlong as float[];
                        int width = longi.Length;
                        if (height != width)
                        {
                            MessageBox.Show("经纬度长度不一致，请输入有效数据！", "Error");
                            return;
                        }
                        HdfImage.m_hdf.HdfLatitude = new float[1, height];
                        for (int i = 0; i < height; ++i)
                            HdfImage.m_hdf.HdfLatitude[0, i] = lat[i];
                        HdfImage.m_hdf.HdfLongitude = new float[1, width];
                        for (int i = 0; i < width; ++i)
                            HdfImage.m_hdf.HdfLongitude[0, i] = longi[i];
                    }
                    else if (objlat.GetType() == typeof(float[,]) && objlong.GetType() == typeof(float[,]))
                    {
                        float[,] lat = objlat as float[,];
                        float[,] longi = objlong as float[,];
                        if (lat.GetLength(0) != longi.GetLength(0) | lat.GetLength(1) != longi.GetLength(1))
                        {
                            MessageBox.Show("经纬度长度不一致，请输入有效数据！", "Error");
                            return;
                        }
                        int height = lat.GetLength(1);
                        int width = lat.GetLength(0);
                        HdfImage.m_hdf.HdfLatitude = new float[height, width];
                        HdfImage.m_hdf.HdfLongitude = new float[height, width];
                        HdfImage.m_hdf.CopyData(lat, HdfImage.m_hdf.HdfLatitude, height, width);
                        HdfImage.m_hdf.CopyData(longi, HdfImage.m_hdf.HdfLongitude, height, width);
                    }
                    else
                    {
                        MessageBox.Show("请选择正确的数据格式！", "Error");
                        return;
                    }
                    prog.setpos(pronum++);
                    #region 3 dims array
                    //取得band_1和band_2
                    //if (objband_250.GetType() == typeof(byte[, ,]))//三维byte数据
                    //{
                    //    byte[, ,] im = objband_250 as byte[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 0, 1 };
                    //    HdfImage.m_hdf = new HdfImage(height, width);
                    //    int[] dims = new int[] { (int)BandNum.Band_1, (int)BandNum.Band_2 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else if (objband_250.GetType() == typeof(int[, ,]))//三维int数据
                    //{
                    //    int[, ,] im = objband_250 as int[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 0, 1 };
                    //    HdfImage.m_hdf = new HdfImage(height, width);
                    //    int[] dims = new int[] { (int)BandNum.Band_1, (int)BandNum.Band_2 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else if (objband_250.GetType() == typeof(ushort[, ,]))//三维uint数据
                    //{
                    //    ushort[, ,] im = objband_250 as ushort[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 0, 1 };
                    //    HdfImage.m_hdf = new HdfImage(height, width);
                    //    int[] dims = new int[] { (int)BandNum.Band_1, (int)BandNum.Band_2 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else
                    //{
                    //    throw new FormatException("请选择正确的数据格式！");
                    //}

                    //取得band_7
                    //if (objband_500.GetType() == typeof(byte[, ,]))
                    //{
                    //    byte[, ,] im = objband_500 as byte[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 4 };
                    //    int[] dims = new int[] { (int)BandNum.Band_7 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else if (objband_500.GetType() == typeof(int[, ,]))//三维int数据
                    //{
                    //    int[, ,] im = objband_500 as int[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 4 };
                    //    int[] dims = new int[] { (int)BandNum.Band_7 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else if (objband_500.GetType() == typeof(ushort[, ,]))//三维uint数据
                    //{
                    //    ushort[, ,] im = objband_500 as ushort[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 4 };
                    //    int[] dims = new int[] { (int)BandNum.Band_7 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}

                    //取得band_21、22、31、32
                    //if (objband_1000.GetType() == typeof(byte[, ,]))//三维byte数据
                    //{
                    //    byte[, ,] im = objband_1000 as byte[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 1, 2, 10, 11 };
                    //    int[] dims = new int[] { (int)BandNum.Band_21, (int)BandNum.Band_22, (int)BandNum.Band_31, (int)BandNum.Band_32 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else if (objband_1000.GetType() == typeof(int[, ,]))//三维int数据
                    //{
                    //    int[, ,] im = objband_1000 as int[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 1, 2, 10, 11 };
                    //    int[] dims = new int[] { (int)BandNum.Band_21, (int)BandNum.Band_22, (int)BandNum.Band_31, (int)BandNum.Band_32 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}
                    //else if (objband_1000.GetType() == typeof(ushort[, ,]))//三维uint数据
                    //{
                    //    ushort[, ,] im = objband_1000 as ushort[, ,];
                    //    int height = im.GetLength(0);
                    //    int width = im.GetLength(1);
                    //    int[] odim = new int[] { 1, 2, 10, 11 };
                    //    int[] dims = new int[] { (int)BandNum.Band_21, (int)BandNum.Band_22, (int)BandNum.Band_31, (int)BandNum.Band_32 };
                    //    HdfImage.m_hdf.CopyData(im, HdfImage.m_hdf.HdfImageData, height, width, dims, odim);
                    //}


                    # endregion

                    int[] channel = new int[] { (int)BandNum.Band_21, (int)BandNum.Band_31, (int)BandNum.Band_32 };
                    Bitmap rgbbmp = HdfImage.m_hdf.ToRGBBitmap(channel, HdfImage.m_hdf.Width, HdfImage.m_hdf.Height);
                    prog.setpos(pronum++);
                    Bitmap graybmp = HdfImage.m_hdf.RGBToGrayBitmap(channel, HdfImage.m_hdf.Width, HdfImage.m_hdf.Height);
                    if (!Directory.Exists(HdfImage.m_hdf.m_originalData))
                        Directory.CreateDirectory(HdfImage.m_hdf.m_originalData);
                    
                    rgbbmp.Save(HdfImage.m_hdf.m_originalData + @"\原始数据层_rgb" + HdfImage.m_hdf.DateYMD + ".tif", System.Drawing.Imaging.ImageFormat.Tiff);
                    prog.setpos(pronum++);
                    graybmp.Save(HdfImage.m_hdf.m_originalData + @"\原始数据层_gray" + HdfImage.m_hdf.DateYMD + ".tif", System.Drawing.Imaging.ImageFormat.Tiff);
                    prog.setpos(pronum++);
                    TifCoordinate tifcor = new TifCoordinate();
                    tifcor.creatImg(HdfImage.m_hdf.m_originalData + @"\原始数据层_rgb" + HdfImage.m_hdf.DateYMD + ".tif");
                    prog.setpos(pronum++);//26

                }
                catch (FormatException ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }

                //获取同目录下的MOD03级数据
                if (filenamelist[0].Contains("MOD"))
                    filenamelist[0] = "MOD03";
                else if (filenamelist[0].Contains("MYD"))
                    filenamelist[0] = "MYD03";
                else
                {
                    MessageBox.Show("读入的卫星数据不为MODIS数据，请重新输入！", "Error");
                    return;
                }
                string filepath03 = filenamelist[0] + '.' + filenamelist[1] + '.' + filenamelist[2];
                List<string> hdffile = new List<string>();
                FileManage fm = new FileManage();
                fm.ListFiles(filepathInfo.Directory, "hdf", hdffile);
                string get03path = "";//MOD03级数据路径
                foreach (string pa in hdffile)
                {
                    if (pa.Contains(filepath03))
                    {
                        get03path = String.Format("{0}\\{1}", filepathInfo.Directory, pa);
                        break;
                    }
                }
                prog.setpos(pronum++);
                if (get03path.Count() != 0)
                {
                    COM_IDL_connectLib.COM_IDL_connect oCOM_IDL3 = new COM_IDL_connectLib.COM_IDL_connect();
                    oCOM_IDL3.CreateObject(0, 0, 0);
                    oCOM_IDL3.SetIDLVariable("dir", get03path);
                    oCOM_IDL3.ExecuteString(".compile '" + Application.StartupPath.ToString() + "\\ReadMOD03_hdf.pro'");
                    prog.setpos(pronum++);
                    oCOM_IDL2.ExecuteString("ReadMOD03_hdf,dir,seamask=seamask,SolarZenith=SolarZenith,SolarAzimuth=SolarAzimuth");
                    prog.setpos(pronum++);
                    object objsea = oCOM_IDL1.GetIDLVariable("seamask");//seamask
                    prog.setpos(pronum++);
                    object objsz = oCOM_IDL1.GetIDLVariable("SolarZenith");//seamask
                    prog.setpos(pronum++);
                    object objsa = oCOM_IDL1.GetIDLVariable("SolarAzimuth");//seamask
                    prog.setpos(pronum++);

                    //seamask
                    if (objsea.GetType() == typeof(byte[,]))
                    {
                        byte[,] sea = objsea as byte[,];
                        int height = sea.GetLength(1);
                        int width = sea.GetLength(0);
                        HdfImage.m_hdf.SeaMask = new byte[height, width];
                        HdfImage.m_hdf.CopyData(sea, HdfImage.m_hdf.SeaMask, height, width);
                    }
                    prog.setpos(pronum++);

                    //SolarZenith
                    if (objsz.GetType() == typeof(short[,]))
                    {
                        short[,] sz = objsz as short[,];
                        int height = sz.GetLength(1);
                        int width = sz.GetLength(0);
                        HdfImage.m_hdf.SolarZenith = new short[height, width];
                        HdfImage.m_hdf.CopyData(sz, HdfImage.m_hdf.SolarZenith, height, width);
                    }
                    prog.setpos(pronum++);

                    //SolarAzimuth
                    if (objsa.GetType() == typeof(short[,]))
                    {
                        short[,] sa = objsa as short[,];
                        int height = sa.GetLength(1);
                        int width = sa.GetLength(0);
                        HdfImage.m_hdf.SolarAzimuth = new short[height, width];
                        HdfImage.m_hdf.CopyData(sa, HdfImage.m_hdf.SolarAzimuth, height, width);
                    }
                    prog.setpos(pronum++);//35
                }
                prog.Close();
            }
            AutoLoadRaster(HdfImage.m_hdf.m_originalData, @"\原始数据层_rgb" + HdfImage.m_hdf.DateYMD + ".tif");
        }

        /// <summary>
        /// 自动加载栅格图像
        /// </summary>
        /// <param name="inPath">路径</param>
        /// <param name="inName">文件名</param>
        private void AutoLoadRaster(string inPath, string inName)
        {
            IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactory();
            IWorkspace workspace;
            workspace = workspaceFactory.OpenFromFile(inPath, 0); //inPath栅格数据存储路径
            if (workspace == null)
            {
                Console.WriteLine("Could not open the workspace.");
                return;
            }
            IRasterWorkspace rastWork = (IRasterWorkspace)workspace;
            IRasterDataset rastDataset;
            rastDataset = rastWork.OpenRasterDataset(inName);//inName栅格文件名
            if (rastDataset == null)
            {
                Console.WriteLine("Could not open the raster dataset.");
                return;
            }
            //影像金字塔的判断与创建
            IRasterPyramid pRasPyrmid;
            pRasPyrmid = rastDataset as IRasterPyramid;
            if (pRasPyrmid != null)
            {
                if (!(pRasPyrmid.Present))
                {
                    pRasPyrmid.Create();
                }
            }
            IRaster pRaster;
            pRaster = rastDataset.CreateDefaultRaster();
            IRasterLayer pRasterLayer;
            pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromRaster(pRaster);
            ILayer pLayer = pRasterLayer as ILayer;
            axMapControl1.AddLayer(pLayer, 1);
            //下面代码实现鹰眼与原图的同步
            IMap pMap = this.axMapControl1.Map;
            this.m_controlsSynchronizer.ReplaceMap(pMap);
        }

        private void butHDFFireSpot_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageBox.Show("请打开 MODIS_14 分辨率的HDF文件！", "Open HDF file");
            OpenFileDialog filedlg = new OpenFileDialog();
            filedlg.Filter = "Hierarchical Data Format(*.hdf)|*.hdf";
            filedlg.Multiselect = false;
            filedlg.Title = "Open HDF（MODIS_14）file";
            if (filedlg.ShowDialog() == DialogResult.OK)
            {
                string filepath = filedlg.FileName;
                ProgressForm prog = new ProgressForm();//调出进度条
                prog.MAX = 10;
                prog.MIN = 0;
                prog.Show(this);
                prog.setpos(0);
                COM_IDL_connectLib.COM_IDL_connect oCOM_IDL = new COM_IDL_connectLib.COM_IDL_connect();
                oCOM_IDL.CreateObject(0, 0, 0);
                oCOM_IDL.SetIDLVariable("dir", filepath);
                oCOM_IDL.ExecuteString(".compile '" + Application.StartupPath.ToString() + "\\ReadMOD14_hdf.pro'");
                prog.setpos(1);
                oCOM_IDL.ExecuteString("ReadMOD14_hdf,dir,Latitude_data=Latitude_data,Longitude_data=Longitude_data,"
                        + "line_data=line_data,sample_data=sample_data,Firemask_data=Firemask_data");
                prog.setpos(2);
                object objfiremask = oCOM_IDL.GetIDLVariable("Firemask_data");//Firemask
                object objline = oCOM_IDL.GetIDLVariable("line_data");//line
                object objsample = oCOM_IDL.GetIDLVariable("sample_data");//sample
                prog.setpos(4);
                object objlat = oCOM_IDL.GetIDLVariable("Latitude_data");//纬度
                object objlong = oCOM_IDL.GetIDLVariable("Longitude_data");//经度

                if (objfiremask.GetType() == typeof(byte[,]))
                {
                    byte[,] fm = objfiremask as byte[,];
                    int height = fm.GetLength(1);
                    int width = fm.GetLength(0);
                    HdfImage.m_hdf.Firemask = new byte[height, width];
                    HdfImage.m_hdf.CopyData(fm, HdfImage.m_hdf.Firemask, height, width);
                }
                prog.setpos(5);

                if (objline.GetType() == typeof(short[]))
                {
                    short[] line = objline as short[];
                    int height = line.GetLength(0);
                    HdfImage.m_hdf.FP_line = new short[height];
                    HdfImage.m_hdf.CopyData(line, HdfImage.m_hdf.FP_line, height);
                }
                prog.setpos(6);

                if (objsample.GetType() == typeof(short[]))
                {
                    short[] sample = objsample as short[];
                    int height = sample.GetLength(0);
                    HdfImage.m_hdf.FP_sample = new short[height];
                    HdfImage.m_hdf.CopyData(sample, HdfImage.m_hdf.FP_sample, height);
                }
                prog.setpos(7);

                if (objlat.GetType() == typeof(float[]))
                {
                    float[] lat = objlat as float[];
                    int height = lat.GetLength(0);
                    HdfImage.m_hdf.FP_lat = new float[height];
                    HdfImage.m_hdf.CopyData(lat, HdfImage.m_hdf.FP_lat, height);
                }
                prog.setpos(8);

                if (objlong.GetType() == typeof(float[]))
                {
                    float[] lon = objlong as float[];
                    int height = lon.GetLength(0);
                    HdfImage.m_hdf.FP_long = new float[height];
                    HdfImage.m_hdf.CopyData(lon, HdfImage.m_hdf.FP_long, height);
                }
                prog.setpos(9);

                Bitmap bmp = HdfImage.m_hdf.ToGrayBitmap(HdfImage.m_hdf.Firemask, HdfImage.m_hdf.Firemask.GetLength(1), HdfImage.m_hdf.Firemask.GetLength(0));
                if (!Directory.Exists(HdfImage.m_hdf.m_originalData))
                    Directory.CreateDirectory(HdfImage.m_hdf.m_originalData);
                bmp.Save(HdfImage.m_hdf.m_originalData + "\\火点层.Tiff", System.Drawing.Imaging.ImageFormat.Tiff);
                prog.setpos(10);
                prog.Close();
            }
        }

        private void HDFFireSpotTOFeature_ItemClick(object sender, ItemClickEventArgs e)
        {
            //string str = System.IO.Directory.GetCurrentDirectory();
            //string filePath=HdfImage.m_hdf.m_firmaskpath;
            string filePath = HdfImage.m_hdf.m_originalData;
            const string rasterFileName = @"火点层.Tiff";
            const string outFeatureClassName = @"FeatureFireSpot.shp";
            DeleteSignedFile(filePath, outFeatureClassName);
            ConvertRaterToLineFeature(filePath, rasterFileName, outFeatureClassName);
        }

        // 此函数用于删除给定路径下的指定文件 
        private static void DeleteSignedFile(string path, string fileName)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            //遍历该路径下的所有文件 
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name == (fileName).ToLower())
                {
                    File.Delete(path + "\\" + fi.Name);//删除当前文件
                }
            }
        }

        public void ConvertRaterToLineFeature(string pRasterWs, string pRasterDatasetName, string pShapeFileName)
        {
            //Convert the raster to a line feature class.
            //Get the input raster
            IRasterDataset pRasterDataset = GetRasterWorkspace(pRasterWs).OpenRasterDataset(pRasterDatasetName);
            //Create RasterConverionOp.
            IConversionOp pConversionOp = new RasterConversionOp() as IConversionOp;
            IRasterAnalysisEnvironment pEnv = (IRasterAnalysisEnvironment)pConversionOp;
            IWorkspaceFactory pWorkspaceFactory = new RasterWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pRasterWs, 0);
            pEnv.OutWorkspace = pWorkspace;
            //Create an output shapefile workspace.
            IWorkspaceFactory pShapeFactory = new ShapefileWorkspaceFactory();
            IWorkspace pShapeWS = pShapeFactory.OpenFromFile(pRasterWs, 0);
            //Execute conversion. 
            System.Object pDangle = (System.Object)1.0;
            IGeoDataset pFeatClassOutput = pConversionOp.RasterDataToLineFeatureData((IGeoDataset)pRasterDataset,
            pShapeWS, pShapeFileName, true, false, ref pDangle);
        }

        public void ConvertRaterToPointFeature(string pRasterWs, string pRasterDatasetName, string pShapeFileName)
        {
            IRasterDataset pRasterDataset = GetRasterWorkspace(pRasterWs).OpenRasterDataset(pRasterDatasetName);
            IConversionOp pConversionOp = new RasterConversionOp() as IConversionOp;
            IRasterAnalysisEnvironment pEnv = (IRasterAnalysisEnvironment)pConversionOp;
            IWorkspaceFactory pWorkspaceFactory = new RasterWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(pRasterWs, 0);
            pEnv.OutWorkspace = pWorkspace;
            IWorkspaceFactory pShapeFactory = new ShapefileWorkspaceFactory();
            IWorkspace pShapeWS = pShapeFactory.OpenFromFile(pRasterWs, 0);
            IGeoDataset pFeatClassOutput = pConversionOp.RasterDataToPointFeatureData((IGeoDataset)pRasterDataset,
            pShapeWS, pShapeFileName);
        }

        public IRasterWorkspace GetRasterWorkspace(string pWsName)
        {
            try
            {
                IWorkspaceFactory pWorkFact = new RasterWorkspaceFactory();
                return pWorkFact.OpenFromFile(pWsName, 0) as IRasterWorkspace;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 菜单栏-图层加载
        private void butAddProvince_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                DialogResult result = MessageBox.Show("是否保存当前地图？", "警告",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.Yes)
                    this.Save_Click(null, null);
            }
            OpenNewMapDocument openMapDoc = new OpenNewMapDocument(m_controlsSynchronizer);
            openMapDoc.OnCreate(m_controlsSynchronizer.MapControl.Object);
            openMapDoc.OnClick();
            //OpenShapeFile(0);
        }

        private void butRemoteData_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "遥感图像 (*.tif;*.tiff)|*.tif;*.tiff;*.jpg;*.bmp";
            dlg.Multiselect = false;
            dlg.InitialDirectory = HdfImage.m_firmaskpath + "\\CreatedData";
            dlg.Title = "打开遥感图像";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string strFullPath = dlg.FileName;
                if (strFullPath == "") return;
                int Index = strFullPath.LastIndexOf("\\");
                string inPath = strFullPath.Substring(0, Index);
                string inName = strFullPath.Substring(Index + 1);
                IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactory();
                IWorkspace workspace;
                workspace = workspaceFactory.OpenFromFile(inPath, 0); //inPath栅格数据存储路径
                if (workspace == null)
                {
                    Console.WriteLine("Could not open the workspace.");
                    return;
                }
                IRasterWorkspace rastWork = (IRasterWorkspace)workspace;
                IRasterDataset rastDataset;
                rastDataset = rastWork.OpenRasterDataset(inName);//inName栅格文件名
                if (rastDataset == null)
                {
                    Console.WriteLine("Could not open the raster dataset.");
                    return;
                }
                //影像金字塔的判断与创建
                IRasterPyramid pRasPyrmid;
                pRasPyrmid = rastDataset as IRasterPyramid;
                if (pRasPyrmid != null)
                {
                    if (!(pRasPyrmid.Present))
                    {
                        pRasPyrmid.Create();
                    }
                }
                IRaster pRaster;
                pRaster = rastDataset.CreateDefaultRaster();
                IRasterLayer pRasterLayer;
                pRasterLayer = new RasterLayerClass();
                pRasterLayer.CreateFromRaster(pRaster);
                ILayer pLayer = pRasterLayer as ILayer;
                axMapControl1.AddLayer(pLayer, 1);
                //下面代码实现鹰眼与原图的同步
                IMap pMap = this.axMapControl1.Map;
                this.m_controlsSynchronizer.ReplaceMap(pMap);
            }  
        }

        private void butAddFireLine_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenShapeFile(2);
        }

        private void butAddFireSpot_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenShapeFile(3);
        }

        //此函数用于以对话框形式打开矢量文件
        private void OpenShapeFile(int toIndex)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            //获取当前路径和文件名
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Shape(*.shp)|*.shp|All Files(*.*)|*.*";
            dlg.Title = "打开矢量文件";
            dlg.InitialDirectory = HdfImage.m_firmaskpath + "\\OriginalData";
            dlg.ShowDialog();
            string strFullPath = dlg.FileName;
            if (strFullPath == "") return;
            int Index = strFullPath.LastIndexOf("\\");
            string filePath = strFullPath.Substring(0, Index);
            string fileName = strFullPath.Substring(Index + 1);

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactory();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filePath, 0);
            pFeatureLayer = new FeatureLayerClass();

            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(fileName);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
            ILayer pLayer = pFeatureLayer as ILayer;
            axMapControl1.AddLayer(pLayer);
            axMapControl1.ActiveView.Refresh();

            //下面代码实现鹰眼与原图的同步
            IMap pMap = this.axMapControl1.Map;
            this.m_controlsSynchronizer.ReplaceMap(pMap);
        }
        #endregion

        #region 菜单栏-数据处理模块
        private void butFirePoint_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (HdfImage.m_hdf.Height == 0 | HdfImage.m_hdf.Width == 0)
            {
                MessageBox.Show("HDF数据为空，请先输入原始数据！", "Error");
                return;
            }
            ProgressForm prog = new ProgressForm();//调出进度条
            prog.MAX = 5;
            prog.MIN = 0;
            prog.Show(this);
            prog.setpos(0);
            double[, ,] Reflect = new double[HdfImage.m_hdf.Height, HdfImage.m_hdf.Width, (int)BandNum.Band_32 + 1];
            double[, ,] BrightTotemp = new double[HdfImage.m_hdf.Height, HdfImage.m_hdf.Width, 4];
            prog.setpos(1);
            HdfImage.m_hdf.TransformReflect(Reflect, BrightTotemp);
            prog.setpos(2);
            HdfImage.m_hdf.PotentialFirePlots(Reflect, BrightTotemp);
            prog.setpos(3);
            //WriteTxt(HdfImage.m_hdf.DetectFirePlot, "E:\\label.txt");
            HdfImage.m_hdf.DetectFirePoints(Reflect, BrightTotemp);
            prog.setpos(4);
            Bitmap bmp = HdfImage.m_hdf.ToGrayBitmap(HdfImage.m_hdf.DetectFirePlot, HdfImage.m_hdf.DetectFirePlot.GetLength(1), HdfImage.m_hdf.DetectFirePlot.GetLength(0));
            bmp.Save(HdfImage.m_hdf.m_originalData + @"\检测火点层" + HdfImage.m_hdf.DateYMD + ".Tif", System.Drawing.Imaging.ImageFormat.Tiff);
            prog.setpos(5);
            prog.Close();

            CreateFirePointsAsMultiPoints();
        }

        private void CreateFirePointsAsMultiPoints()
        {
            List<System.Drawing.Point> fireplot = HdfImage.m_hdf.BinaryFiremaskList;
            //下面代码段用于统计0,1矩阵中1的个数
            int featureCounts = fireplot.Count;

            //下面代码段用于创建多点集合，即将多个点作为整体来操作
            IPointCollection4 pointCollection = new MultipointClass();
            IGeometryBridge geometryBridge = new GeometryEnvironmentClass();//线程
            IPoint[] points = new PointClass[featureCounts];
            IMultipoint multipoint;
            object missing = Type.Missing;
            TifCoordinate objTif = new TifCoordinate();
            int featureCount = 0;
            //下面代码段用于将火点的像素坐标转换为经纬度坐标
            foreach (System.Drawing.Point po in fireplot)
            {
                float showTo_X = 0.0F, showTo_Y = 0.0F;

                //将图像中的像素坐标转换成经纬度坐标,注意j,i的顺序
                objTif.ShowCoordinate(po.Y, po.X, ref showTo_X, ref showTo_Y);

                IPoint DisPoint = new PointClass();
                DisPoint.PutCoords(showTo_X, showTo_Y);
                points[featureCount++] = DisPoint;
            }

            geometryBridge.SetPoints(pointCollection, ref points);
            multipoint = pointCollection as IMultipoint;

            string strShapeFolder = HdfImage.m_firmaskpath + "\\OriginalData";
            string strShapeFile = "FireSpotMask" + HdfImage.m_hdf.DateYMD + ".shp";

            CreateShpFromMultiPoints(multipoint, strShapeFolder, strShapeFile);
        }

        private void CreateShpFromMultiPoints(IMultipoint multiPoint, string strShapeFolder, string strShapeFile)
        {
            //删除shp文件
            int index = strShapeFile.LastIndexOf('.');
            string str = strShapeFile.Substring(0, index);

            if (File.Exists(String.Format("{0}\\{1}", strShapeFolder, strShapeFile)))
                File.Delete(String.Format("{0}\\{1}", strShapeFolder, strShapeFile));
            if (File.Exists(String.Format("{0}\\{1}.dbf", strShapeFolder, str)))
                File.Delete(String.Format("{0}\\{1}.dbf", strShapeFolder, str));
            if (File.Exists(String.Format("{0}\\{1}.shx", strShapeFolder, str)))
                File.Delete(String.Format("{0}\\{1}.shx", strShapeFolder, str));

            ISpatialReference pSpatialReference = this.axMapControl1.ActiveView.FocusMap.SpatialReference;
            string shapeFileFullName = strShapeFolder + strShapeFile;
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(strShapeFolder, 0);
            IFeatureClass pFeatureClass;
            if (File.Exists(shapeFileFullName))
            {
                pFeatureClass = pFeatureWorkspace.OpenFeatureClass(strShapeFile);
                IDataset pDataset = (IDataset)pFeatureClass;
                pDataset.Delete();
            }
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            IGeometryDefEdit pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryMultipoint;
            pGeoDefEdit.SpatialReference_2 = pSpatialReference;
            //new UnknownCoordinateSystemClass();
            pFieldEdit.GeometryDef_2 = pGeoDef;
            pFieldsEdit.AddField(pField);
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "ID";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            pFieldsEdit.AddField(pField);

            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Pixels";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            pFieldsEdit.AddField(pField);
            pFeatureClass = pFeatureWorkspace.CreateFeatureClass(strShapeFile, pFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            IMultipoint pMultipoint = new MultipointClass();
            pMultipoint = multiPoint;
            IFeature pFeature = pFeatureClass.CreateFeature();
            pFeature.Shape = pMultipoint;
            pFeature.set_Value(pFeature.Fields.FindField("ID"), "D-1");
            pFeature.set_Value(pFeature.Fields.FindField("Pixels"), 0);
            pFeature.Store();

            AutoLoadShp(strShapeFolder, strShapeFile);
        }

        /// <summary>
        /// 自动加载shp文件
        /// </summary>
        /// <param name="strShapeFolder">路径</param>
        /// <param name="strShapeFile">文件名</param>
        private void AutoLoadShp(string strShapeFolder, string strShapeFile)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactory();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(strShapeFolder, 0);
            pFeatureLayer = new FeatureLayerClass();

            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(strShapeFile);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
            ILayer pLayer = pFeatureLayer as ILayer;
            axMapControl1.AddLayer(pLayer);
            axMapControl1.ActiveView.Refresh();

            //下面代码实现鹰眼与原图的同步
            IMap pMap = this.axMapControl1.Map;
            this.m_controlsSynchronizer.ReplaceMap(pMap);
        }

        private void butFireArea_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (HdfImage.m_hdf.Height == 0 | HdfImage.m_hdf.Width == 0)
            {
                MessageBox.Show("HDF数据为空，请先输入原始数据！", "Error");
                return;
            }
            if (HdfImage.m_hdf.BinaryFiremaskList.Count == 0)
            {
                MessageBox.Show("火点为空，请先检测火点！", "Error");
                return;
            }
            ProgressForm prog = new ProgressForm();//调出进度条
            prog.MAX = 5;
            prog.MIN = 0;
            prog.Show(this);
            prog.setpos(0);
            HdfImage.m_hdf.DetectFireArea();
            prog.setpos(1);
            HdfImage.m_hdf.DetectFireLine();
            prog.setpos(2);

            //输出过火面积
            double total = 0;
            for (int m = 0; m < HdfImage.m_hdf.Height; ++m)
                for (int n = 0; n < HdfImage.m_hdf.Width; ++n)
                {
                    if (HdfImage.m_hdf.FireArea[m, n] > 0)
                        total++;
                }
            total /= 10000;
            InfoListBox.Items.Clear();
            InfoListBox.Items.Add(string.Format("该区域的过火面积为{0}万平方千米", total));
            prog.setpos(3);

            //直接调用火迹线检测
            butFireLineDetect_Click();
            prog.setpos(5);
            prog.Close();
        }

        //此函数用于进行图像的火迹线检测处理
        private void butFireLineDetect_Click()
        {
            if (HdfImage.m_hdf.FireLineList.Count == 0)
            {
                MessageBox.Show("火线数据尚未检测，请先检测火线！", "Error");
                return;
            }
            List<System.Drawing.Point> fireline = HdfImage.m_hdf.FireLineList;
            //下面代码段用于创建多点集合，即将多个点作为整体来操作
            IPointCollection4 pointCollection = new MultipointClass();
            IGeometryBridge geometryBridge = new GeometryEnvironmentClass();//线程
            IPoint[] points = new PointClass[fireline.Count];
            IMultipoint multipoint;
            int rasterCount = 0;
            TifCoordinate objTif = new TifCoordinate();

            //下面代码段用于将火点的像素坐标转换为经纬度坐标
            foreach (System.Drawing.Point po in fireline)
            {
                float showTo_X = 0.0F, showTo_Y = 0.0F;

                //将图像中的像素坐标转换成经纬度坐标,注意j,i的顺序
                objTif.ShowCoordinate(po.Y, po.X, ref showTo_X, ref showTo_Y);

                IPoint DisPoint = new PointClass();
                DisPoint.PutCoords(showTo_X, showTo_Y);
                points[rasterCount++] = DisPoint;
            }
            geometryBridge.SetPoints(pointCollection, ref points);
            multipoint = pointCollection as IMultipoint;

            string strShapeFolder = HdfImage.m_firmaskpath + "\\OriginalData";
            string strShapeFile = "FireLineMask" + HdfImage.m_hdf.DateYMD +".shp";

            CreateShpFromMultiPoints(multipoint, strShapeFolder, strShapeFile);
        }

        private void butDistance_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (HdfImage.m_hdf.Height == 0 | HdfImage.m_hdf.Width == 0)
            {
                MessageBox.Show("HDF数据为空，请先输入原始数据！", "Error");
                return;
            }
            if (HdfImage.m_hdf.BinaryFiremaskList.Count == 0)
            {
                MessageBox.Show("火点为空，请先检测火点！", "Error");
                return;
            }
            if (HdfImage.m_hdf.FireLineList == null)
            {
                MessageBox.Show("火线为空，请先检测火线！", "Error");
                return;
            }
            List<System.Drawing.Point> FireSpot = HdfImage.m_hdf.BinaryFiremaskList;
            List<System.Drawing.Point> FireLine = HdfImage.m_hdf.FireLineList;
            CSingleList<double> MinPoint = new CSingleList<double>();

            for (int i = 0; i < FireSpot.Count; ++i)
                for (int j = 0; j < FireLine.Count; ++j)
                {
                    double dis = Math.Sqrt(Math.Pow(FireSpot[i].X - FireLine[j].X, 2) +
                        Math.Pow(FireSpot[i].Y - FireLine[j].Y, 2));
                    if (MinPoint.Empty)
                        MinPoint.Head.Next = new CNode<double>(dis, FireSpot[i], FireLine[j]);
                    else
                    {
                        CNode<double> cnode = new CNode<double>(dis, FireSpot[i], FireLine[j]);
                        MinPoint.AscendInsert(cnode);
                        if (MinPoint.Count > 10)
                            MinPoint.DeleteLast();
                    }
                }

            int n = 8;//用于指定输出的前n个火点
            PrintAndFlashFireSpot(n, MinPoint);  
        }


        //此函数用于输出前指定个数的火点距输电走廊的距离，并闪烁前指定个火点
        private void PrintAndFlashFireSpot<T>(int n, CSingleList<T> slist) where T : IComparable
        {
            //在相关信息输出栏输出距离信息
            InfoListBox.Items.Clear();
            InfoListBox.Items.Add(String.Format(@"距离输电走廊最近的{0}个火点的距离和经纬度坐标分别为:", n));
            TifCoordinate objTif = new TifCoordinate();
            IPoint[] pPoints = new PointClass[n];
            for (int i = 0; i < n; i++)
            {
                float showTo_X = 0;
                float showTo_Y = 0;
                //x对应行号，y对应列号
                int x = slist[i].StartPoint.X;
                int y = slist[i].StartPoint.Y;
                objTif.ShowCoordinate(y, x, ref showTo_X, ref showTo_Y);
                InfoListBox.Items.Add(String.Format("{0}Km   经度为：{1}  ,  纬度为：{2}", slist[i].KeyValue, showTo_X, showTo_Y));
            }
            for (int i = 0; i < n; i++)
            {
                float showTo_X = 0;
                float showTo_Y = 0;
                //x对应行号，y对应列号
                int x = slist[i].StartPoint.X;
                int y = slist[i].StartPoint.Y;
                objTif.ShowCoordinate(y, x, ref showTo_X, ref showTo_Y);
                IPoint pPoint = new PointClass();
                pPoint.PutCoords(showTo_X, showTo_Y);
                pPoints[i] = pPoint;

            }
            IPointCollection4 pointCollection = new MultipointClass();
            IMultipoint multiPoint;
            IGeometryBridge geometryBridge = new GeometryEnvironmentClass();
            geometryBridge.SetPoints(pointCollection, ref pPoints);
            multiPoint = pointCollection as IMultipoint;

            IActiveView activeView = this.axMapControl1.ActiveView;
            activeView.ScreenDisplay.StartDrawing(activeView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
            FlashPoint(this.axMapControl1, activeView.ScreenDisplay, multiPoint as IGeometry);
            activeView.ScreenDisplay.FinishDrawing();
        }

        //此函数用于闪烁火点
        private static void FlashPoint(AxMapControl mapControl, IScreenDisplay iScreenDisplay, IGeometry iGeometry)
        {
            ISimpleMarkerSymbol iMarkerSymbol;
            ISymbol iSymbol;
            IRgbColor iRgbColor;
            iMarkerSymbol = new SimpleMarkerSymbol();
            iMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            iRgbColor = new RgbColor();
            iRgbColor.RGB = System.Drawing.Color.FromArgb(0, 0, 0).ToArgb();
            iMarkerSymbol.Color = iRgbColor;
            iSymbol = (ISymbol)iMarkerSymbol;
            iSymbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            mapControl.FlashShape(iGeometry, 10, 2000, iSymbol);
        }

        //获取颜色对象
        public IRgbColor getRGB(int r, int g, int b)
        {
            IRgbColor pColor;
            pColor = new RgbColorClass();
            pColor.Red = r;
            pColor.Green = g;
            pColor.Blue = b;
            return pColor;
        }
        #endregion

        #region 卫星数据接收状态显示
        private void butDownLoad_Click(object sender, EventArgs e)
        {
            DownLoad downLoad = new DownLoad();
            downLoad.ShowDialog();
        }

        private void butSearchDirectory_Click()
        {
            string filepath = HdfImage.m_hdf.m_googleData;
            if (!Directory.Exists(filepath))
                Directory.CreateDirectory(filepath);
            FileManage.m_FileManage.ScanFiles(filepath);

            //界面上卫星种类更新
            //satelliteBox.Items.Clear();
            //satelliteBox.Items.Add(@"现有卫星种类包括：");
            //foreach (string sate in FileManage.m_FileManage.m_satellite)
            //    satelliteBox.Items.Add(sate);
        }

        private void AddSatellite_Click(object sender, EventArgs e)
        {
            AddSatellite addsate = new AddSatellite();
            if (addsate.ShowDialog() == DialogResult.OK)
            {
                FileManage.m_FileManage.m_satellite.Add(addsate.m_newSate);
                //satelliteBox.Items.Add(addsate.m_newSate);
                DateChanged(sender, e);
            }
        }

        private void DateChanged(object sender, EventArgs e)
        {
            bool dataexist = true;
            //日期更新扫描文件夹
            if (FileManage.m_FileManage.m_dateList.Count() == 0 && FileManage.m_FileManage.m_sateInDate.Count() == 0)
                butSearchDirectory_Click();
            DataState.Items.Clear();
            MyDate currentdate = new MyDate(platdate.DateTime.Year, platdate.DateTime.Month, platdate.DateTime.Day);
            FileManage.m_FileManage.m_currentDate = currentdate;
            int index = -1;
            for (int i = 0; i < FileManage.m_FileManage.m_dateList.Count(); ++i)
            {
                if (FileManage.m_FileManage.m_dateList[i] == currentdate)
                {
                    index = i;
                    break;
                }
            }
            //DataState.Items.Add(platdate.DateTime.Year + "年" + platdate.DateTime.Month + "月" +
            //    platdate.DateTime.Day + "日 卫星数据状态为：");

            
            if (index == -1)
            {
                foreach (string sate in FileManage.m_FileManage.m_satellite)
                {
                    DataState.Items.Add(string.Format("{0,-10}", sate + ":") + string.Format("{0,5}", "不存在"));
                    dataexist &= false;
                }
            }
            else
            {
                foreach (string sate in FileManage.m_FileManage.m_satellite)
                {
                    if (FileManage.m_FileManage.m_sateInDate[index].Contains(sate))
                    {
                        DataState.Items.Add(string.Format("{0,-10}", sate + ":") + string.Format("{0,5}", "存在"));
                        dataexist &= true;
                    }
                    else
                    {
                        DataState.Items.Add(string.Format("{0,-10}", sate + ":") + string.Format("{0,5}", "不存在"));
                        dataexist &= false;
                    }
                }
            }

            if (dataexist)
                butDownLoad.Enabled = false;
            else
                butDownLoad.Enabled = true;
        }

        private void DetectedDate_Click(object sender, EventArgs e)
        {
            if (HdfImage.m_hdf.Date == null)
                return;
            DetectedDate.Items.Clear();//清空原始数据
            DetectedDate.Items.Add(@"已检测火点的日期：");
            string strShapeFolder = HdfImage.m_firmaskpath + "\\OriginalData";
            DirectoryInfo di = new DirectoryInfo(strShapeFolder);
            List<string> firepolt = new List<string>();
            FileManage.m_FileManage.ListFiles(di,"shp",firepolt);
            List<DateTime> detecteddate = new List<DateTime>();
            foreach (string str in firepolt)
            {
                string[] telist = str.Split('.');
                if (telist[0] == @"FireSpotMask")
                {
                    DateTime dt = new DateTime(Int16.Parse(telist[1]), Int16.Parse(telist[2]), Int16.Parse(telist[3]), Int16.Parse(telist[4]), Int16.Parse(telist[5]),0);
                    if (!detecteddate.Contains(dt))
                        detecteddate.Add(dt);
                }
            }
            foreach (DateTime dt in detecteddate)
                DetectedDate.Items.Add(dt);
        }
        #endregion

        #region 扩展面板1
        private void butAccident_Click(object sender, EventArgs e)
        {
            if (HdfImage.m_hdf.Height == 0)
            {
                MessageBox.Show("HDF数据为空，请先输入原始数据！", "Error");
                return;
            }
            //下面代码段用于获取输电走廊线Multipoint矢量文件
            string filePath = HdfImage.m_firmaskpath + "\\OriginalData";
            string fileName = "FireLineMask.shp";
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;
            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactory();
            pFeatureLayer = new FeatureLayerClass();
            //注意此处的路径是不能带文件名的
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filePath, 0);
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(fileName);
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IFeature pFeature = pFeatureClass.GetFeature(0);
            IMultipoint pMultiPoint = pFeature.Shape as IMultipoint;
            int featureCounts = pFeatureClass.FeatureCount(null);

            IActiveView activeView = this.axMapControl1.ActiveView;
            activeView.ScreenDisplay.StartDrawing(activeView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
            FlashPoint(this.axMapControl1, activeView.ScreenDisplay, pMultiPoint as IGeometry);
            activeView.ScreenDisplay.FinishDrawing();
        }

        private void butDistanceByHand_Click(object sender, EventArgs e)
        {
            if (this.axMapControl1.LayerCount == 0)
            {
                return;
            }

            //此处用于手工测距，运用flag将操作转移到axMapControl1_OnMouseDown函数
            MessageBox.Show("起点单击鼠标左键，终点双击鼠标左键！");
            flag = 1;
        }

        /// <summary>
        /// 获取所有原始数据
        /// </summary>
        protected void GetAllInitInfo(Control CrlContainer)
        {
            if (CrlContainer.Parent == this)
            {
                formWidth = Convert.ToDouble(CrlContainer.Width);
                formHeight = Convert.ToDouble(CrlContainer.Height);
            }
            foreach (Control item in CrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                    controlInfo.Add(item.Name, (item.Left + item.Width / 2) + "," + (item.Top + item.Height / 2) + "," + item.Width + "," + item.Height + "," + item.Font.Size);
                if ((item as UserControl) == null && item.Controls.Count > 0)
                    GetAllInitInfo(item);
            }
        }
        private void ControlsChangeInit(Control CrlContainer)
        {
            scaleX = (Convert.ToDouble(CrlContainer.Width) / formWidth);
            scaleY = (Convert.ToDouble(CrlContainer.Height) / formHeight);
        }
        private void ControlsChange(Control CrlContainer)
        {
            double[] pos = new double[5];//pos数组保存当前控件中心Left,Top,控件Width,控件Height,控件字体Size
            foreach (Control item in CrlContainer.Controls)
            {
                if (item.Name.Trim() != "")
                {
                    if ((item as UserControl) == null && item.Controls.Count > 0)
                        ControlsChange(item);
                    string[] strs = controlInfo[item.Name].Split(',');
                    for (int j = 0; j < 5; j++)
                    {
                        pos[j] = Convert.ToDouble(strs[j]);
                    }
                    double itemWidth = pos[2] * scaleX;
                    double itemHeight = pos[3] * scaleY;
                    item.Left = Convert.ToInt32(pos[0] * scaleX - itemWidth / 2);
                    item.Top = Convert.ToInt32(pos[1] * scaleY - itemHeight / 2);
                    item.Width = Convert.ToInt32(itemWidth);
                    item.Height = Convert.ToInt32(itemHeight);
                    item.Font = new Font(item.Font.Name, float.Parse((pos[4] * Math.Min(scaleX, scaleY)).ToString()));
                }
            }
        } 
        private void Form1_OnSizeChanged(object sender, EventArgs e)
        {
            if (controlInfo.Count > 0)
            {
                ControlsChangeInit(this.Controls[1]);
                ControlsChange(this.Controls[1]);
            }
        }
        #endregion
        

    }
}