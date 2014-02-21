using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Route
{
    public partial class TextBoxEx : UserControl
    {
        private readonly Color HintColor = Color.Gray;
        private string hint;
        private bool textBoxFocusing;

        public TextBoxEx()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取或设置显示在文本框中的提示
        /// </summary>
        [Browsable(true), Category("Appearance"), Description("显示在文本框中的提示")]
        public string Hint
        {
            get
            {
                return hint;
            }
            set
            {
                hint = value;
                if (!textBoxFocusing && (textBox1.Text == string.Empty))
                    ShowHint();
            }
        }

        /// <summary>
        /// 获取或设置文本框显示的内容
        /// </summary>
        [Browsable(false)]
        public override string Text
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                base.Text = value;
                textBox1.Text = value;
            }
        }

        /// <summary>
        /// 在文本框里显示提示
        /// </summary>
        protected void ShowHint()
        {
            textBox1.ForeColor = HintColor;
            textBox1.Text = hint;
        }

        /// <summary>
        /// 从文本框里隐藏提示
        /// </summary>
        protected void HideHint()
        {
            textBox1.ForeColor = ForeColor;
            textBox1.Text = string.Empty;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBoxFocusing)
                return;
            if (!textBox1.Text.Equals(hint))
                textBox1.ForeColor = ForeColor;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBoxFocusing = true;
            // 如果文本内容为空则隐藏提示
            if (textBox1.Text.Equals(hint))
                HideHint();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            textBoxFocusing = false;
            // 如果文本内容为空则显示提示
            if (textBox1.Text == string.Empty)
                ShowHint();
        }
    }
}
