using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Route
{
    public partial class Splash : Form
    {
        private const string titleFontName = "Century Gothic";
        private const int titleFontSize = 50;
        private const int buttonOffset = 10;

        private Font titleFont;
        private Brush titleBrush;
        private Size titleSize;
        private Point titleLocation;

        private Form mainForm;
        private Thread mainFormThread;

        public Splash()
        {
            InitializeComponent();

            // 启动滚动条
            progressBar1.Start();

            // 设置标题的字体，颜色，位置
            titleFont = new Font(titleFontName, titleFontSize);
            titleBrush = new SolidBrush(ForeColor);
            titleSize = TextRenderer.MeasureText(Application.ProductName, titleFont);
            titleLocation = new Point(Width / 2 - titleSize.Width / 2, Height / 2 - titleSize.Height / 2);
            titleLocation.Offset(5, 0);
            
            // 创建主窗体线程
            mainFormThread = new Thread(new ThreadStart(createMainForm));
            mainFormThread.SetApartmentState(ApartmentState.STA);
            mainFormThread.Start();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // 隐藏Splash窗体
            Hide();
            // 关闭主窗体
            mainFormThread.Abort();
            // 关闭Splash窗体
            Close();
        }

        private void createMainForm()
        {
            // 实例化主窗体
            mainForm = new MainForm();
            mainForm.FormClosed += new FormClosedEventHandler(mainForm_FormClosed);
            // 隐藏Splash窗体
            Invoke(new Action(Hide));
            // 显示主窗体
            mainForm.ShowDialog();
        }

        private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 关闭窗体
            Invoke(new Action(Close));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 显示应用程序的标题
            e.Graphics.DrawString(Application.ProductName, titleFont, titleBrush, titleLocation);
        }
    }
}
