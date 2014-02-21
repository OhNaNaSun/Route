using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Route
{
    /// <summary>
    /// 表示Windows窗体进度条
    /// </summary>
    public class ProgressBar : Control
    {
        private Timer timer = new Timer();
        private Rectangle rect;
        private Brush brush;
        private int barLength;
        private int speed;
        private int count, maxCount;

        public ProgressBar()
        {
            barLength = 100;
            speed = 10;
            timer.Interval = 20;
            timer.Tick += new EventHandler(timer_Tick);
            brush = new SolidBrush(ForeColor);
        }

        /// <summary>
        /// 获取或设置进度条中方块的长度
        /// </summary>
        [Browsable(true), Category("Layout"), DefaultValue(100), Description("进度条中方块的长度")]
        public int BarLength
        {
            get
            {
                return barLength;
            }
            set
            {
                barLength = value;
            }
        }

        /// <summary>
        /// 获取或设置进度条滚动的速度
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(10), Description("进度条滚动的速度")]
        public int Speed
        { 
            get
            {
                return speed;   
            }
            set
            {
                if (value == 0)
                    throw new ArgumentOutOfRangeException();
                speed = value;
            }
        }

        /// <summary>
        /// 开始滚动
        /// </summary>
        public void Start()
        {
            count = 0;
            maxCount = (Width + barLength) / speed;
            rect = new Rectangle(-barLength, 0, barLength, Height);
            timer.Start();
        }

        /// <summary>
        /// 停止滚动
        /// </summary>
        public void Stop()
        {
            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            count++;
            rect.X += speed;
            Refresh();
            if (count >= maxCount)
            {
                rect = new Rectangle(-barLength, 0, barLength, Height);
                count = 0;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(brush, rect);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            brush = new SolidBrush(ForeColor);
        }
    }
}
