using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Route.DataInteroperability;

namespace Route
{
    public partial class QuickImportUI : Form
    {
        private Thread QIThread;
        private QuickImport QI;

        public QuickImportUI()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBoxEx1.Text = openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxEx2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            ControlBox = false;
            QIThread = new Thread(new ThreadStart(Execute));
            QIThread.Start();
        }

        private void Execute()
        {
            progressBar1.Invoke(new Action(progressBar1.Show));
            progressBar1.Invoke(new Action(progressBar1.Start));
            if (QI == null) 
            {
                QI = new QuickImport();
                QI.ExecuteCompleted += new EventHandler(QI_ExecuteCompleted);
            }
            QI.Source = textBoxEx1.Text;
            QI.Destination = textBoxEx2.Text + "\\" + Path.GetFileNameWithoutExtension(textBoxEx1.Text) + ".gdb";
            try
            {
                QI.Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
            finally
            {
                Invoke(new Action<bool>(enabled =>
                {
                    button1.Enabled = enabled;
                }), true);
                Invoke(new Action<bool>(enabled =>
                {
                    ControlBox = enabled;
                }), true);
                progressBar1.Invoke(new Action(progressBar1.Stop));
                progressBar1.Invoke(new Action(progressBar1.Hide));
            }
        }

        private void QI_ExecuteCompleted(object sender, EventArgs e)
        {
            MessageBox.Show("转换完成");
        }
    }
}
