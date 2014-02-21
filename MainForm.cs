using System;
using System.Windows.Forms;
using ESRI.ArcGIS;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using Route.Commands;

namespace Route
{
    public partial class MainForm : Form
    {
        // 保存工具栏的内容
        private const string PersistedItems = "PersistedItems.txt";
        private const bool LoadPersistedItems = false;

        // ToolbarControl使用的自定义对话框，及对应的事件
        private ICustomizeDialog mCustomizeDialog;
        private ICustomizeDialogEvents_OnStartDialogEventHandler startDialogE;
        private ICustomizeDialogEvents_OnCloseDialogEventHandler closeDialogE;
   
        // TOCControl的快捷菜单
        private IToolbarMenu mMapMenu, mLayerMenu;

        // 快速导入/快速导出窗体
        private QuickImportUI quickImportUI;
        private QuickExportUI quickExportUI;

        // 工具栏上网络分析工具的索引
        private int networkAnalystCommandsIndex;

        public MainForm()
        {
            // 绑定到ArcGIS Runtime
            if (!RuntimeManager.Bind(ProductCode.EngineOrDesktop))
            {
                MessageBox.Show("无法绑定到ArcGIS运行时，程序将退出");
                Close();
            } 

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 设置标签编辑模式为手动
            axTOCControl1.LabelEdit = ESRI.ArcGIS.Controls.esriTOCControlEdit.esriTOCControlManual;

            if (System.IO.File.Exists(PersistedItems) && LoadPersistedItems)
                LoadToolbarControlItems(PersistedItems);
            else
            {
                // 添加常规命令
                axToolbarControl1.AddItem("esriControls.ControlsOpenDocCommand", -1, -1,
                    false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsAddDataCommand", -1, -1,
                    false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsSelectTool", -1, -1,
                    true, 0, esriCommandStyles.esriCommandStyleIconOnly);
                // 添加Map视图导航命令
                axToolbarControl1.AddItem("esriControls.ControlsMapZoomInTool", -1, -1, true,
                    0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsMapZoomOutTool", -1, -1,
                    false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsMapPanTool", -1, -1, false,
                    0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsMapFullExtentCommand", -1, -
                    1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsMapZoomToLastExtentBackCommand",
                    -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem(
                    "esriControls.ControlsMapZoomToLastExtentForwardCommand", -1, -1, false,
                    0, esriCommandStyles.esriCommandStyleIconOnly);
                // 添加Map视图查询命令
                axToolbarControl1.AddItem("esriControls.ControlsMapIdentifyTool", -1, -1,
                    true, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsMapFindCommand", -1, -1,
                    false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsMapMeasureTool", -1, -1,
                    false, 0, esriCommandStyles.esriCommandStyleIconOnly);
            }

            // 设置工具栏自定义对话框
            CreateCustomizeDialog();

            // 创建快捷菜单
            mMapMenu = new ToolbarMenuClass();
            mMapMenu.AddItem(new LayerVisibility(), 1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
            mMapMenu.AddItem(new LayerVisibility(), 2, 1, false, esriCommandStyles.esriCommandStyleTextOnly);
            mMapMenu.AddSubMenu("esriControls.ControlsFeatureSelectionMenu", 2, true);
            mMapMenu.SetHook(axMapControl1);
            mLayerMenu = new ToolbarMenuClass();
            mLayerMenu.AddItem(new RemoveLayer(), -1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
            //mLayerMenu.AddItem(new ScaleThresholds(), 1, 1, true, esriCommandStyles.esriCommandStyleTextOnly);
            //mLayerMenu.AddItem(new ScaleThresholds(), 2, 2, false, esriCommandStyles.esriCommandStyleTextOnly);
            //mLayerMenu.AddItem(new ScaleThresholds(), 3, 3, false, esriCommandStyles.esriCommandStyleTextOnly);
            //mLayerMenu.AddItem(new LayerSelectable(), 1, 4, true, esriCommandStyles.esriCommandStyleTextOnly);
            //mLayerMenu.AddItem(new LayerSelectable(), 2, 5, false, esriCommandStyles.esriCommandStyleTextOnly);
            mLayerMenu.AddItem(new ZoomToLayer(), -1, 1, true, esriCommandStyles.esriCommandStyleTextOnly);
            mLayerMenu.AddItem(new OpenAttributeTable(), -1, 2, true, esriCommandStyles.esriCommandStyleTextOnly);
            mLayerMenu.SetHook(axMapControl1);

            // 设定协同控件
            axToolbarControl1.SetBuddyControl(axMapControl1);
            axTOCControl1.SetBuddyControl(axMapControl1);

            // 允许MapControl拦截方向键
            axMapControl1.KeyIntercept = (int)esriKeyIntercept.esriKeyInterceptArrowKeys;
            axMapControl1.AutoKeyboardScrolling = true;
            axMapControl1.AutoMouseWheel = true;

            // 允许图层拖放
            axTOCControl1.EnableLayerDragDrop = true;
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            // 缓存数据，显示位图
            axMapControl1.SuppressResizeDrawing(true, 0);
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            // 停止显示位图，绘制数据
            axMapControl1.SuppressResizeDrawing(false, 0);
        }

        private void axTOCControl1_OnBeginLabelEdit(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnBeginLabelEditEvent e)
        {
            IBasicMap map = null;
            ILayer layer = null;
            object other = null;
            object index = null;
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            // 判断被点击的东西是什么类型
            axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
            // 只有layer类型的标签才能被修改
            if (item != esriTOCControlItem.esriTOCControlItemLayer)
                e.canEdit = false;
        }

        private void axTOCControl1_OnEndLabelEdit(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnEndLabelEditEvent e)
        {
            // 如果字符串为空，阻止修改
            if (e.newLabel.Trim() == "")
                e.canEdit = false;
        }

        private void CreateCustomizeDialog()
        {
            // 创建一个自定义对话框
            mCustomizeDialog = new CustomizeDialogClass();
            // 设置标题
            mCustomizeDialog.DialogTitle = "自定义工具栏";
            // 显示Add from File按钮
            mCustomizeDialog.ShowAddFromFile = true;
            // 设置新工具添加到的地方
            mCustomizeDialog.SetDoubleClickDestination(axToolbarControl1);

            // 设置自定义对话框的事件
            startDialogE = new ICustomizeDialogEvents_OnStartDialogEventHandler(OnStartDialog);
            closeDialogE = new ICustomizeDialogEvents_OnCloseDialogEventHandler(OnCloseDialog);
            ((ICustomizeDialogEvents_Event)mCustomizeDialog).OnStartDialog += startDialogE;
            ((ICustomizeDialogEvents_Event)mCustomizeDialog).OnCloseDialog += closeDialogE;
        }

        private void OnStartDialog() 
        {
            axToolbarControl1.Customize = true;
        }

        private void OnCloseDialog()
        {
            axToolbarControl1.Customize = false;
            toolbarCustomizeToolStripMenuItem.Checked = false;
        }

        private void toolbarCustomizeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (toolbarCustomizeToolStripMenuItem.Checked == false)
                mCustomizeDialog.CloseDialog();
            else
                mCustomizeDialog.StartDialog(axToolbarControl1.hWnd);
        }

        private void SaveToolbarControlItems(string filePath)
        {
            IBlobStream blobStream = new MemoryBlobStreamClass();
            IStream stream = blobStream;
            axToolbarControl1.SaveItems(stream);
            blobStream.SaveToFile(filePath);
        }

        private void LoadToolbarControlItems(string filePath)
        {
            IBlobStream blobStream = new MemoryBlobStreamClass();
            IStream stream = blobStream;
            blobStream.LoadFromFile(filePath);
            axToolbarControl1.LoadItems(stream);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存工具栏的内容
            SaveToolbarControlItems(PersistedItems);
        }

        private void networkAnalystToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (networkAnalystToolStripMenuItem.Checked == true)
            {
                //axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystToolbar",
                //    -1, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);
                //axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystSolverMenu",
                //    -1, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);
                networkAnalystCommandsIndex = axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystRouteCommand",
                    -1, -1, true, 0, esriCommandStyles.esriCommandStyleTextOnly);
                axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystWindowCommand",
                    -1, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystCreateLocationTool",
                    -1, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystSelectLocationTool",
                    -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystSolveCommand",
                    -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
                axToolbarControl1.AddItem("esriControls.ControlsNetworkAnalystLayerToolControl",
                    -1, -1, true, 0, esriCommandStyles.esriCommandStyleIconOnly);
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    axToolbarControl1.Remove(networkAnalystCommandsIndex);
                }
            }
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button != 2)
                return;

            // 弹出快捷菜单
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null; ILayer layer = null;
            object other = null; object index = null;

            // 确定被点击的是什么
            axTOCControl1.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);

            // 确保已被选中
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                axTOCControl1.SelectItem(map, null);
            else
                axTOCControl1.SelectItem(layer, null);

            // 把CustomProperty设置成对应图层 (this is used by the custom layer commands)			
            axMapControl1.CustomProperty = layer;

            // 
            if (item == esriTOCControlItem.esriTOCControlItemMap) mMapMenu.PopupMenu(e.x, e.y, axTOCControl1.hWnd);
            if (item == esriTOCControlItem.esriTOCControlItemLayer) mLayerMenu.PopupMenu(e.x, e.y, axTOCControl1.hWnd);

        }

        private void quickImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (quickImportUI == null)
                quickImportUI = new QuickImportUI();
            quickImportUI.Show();
        }

        private void quickExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (quickExportUI == null)
                quickExportUI = new QuickExportUI();
            quickExportUI.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
