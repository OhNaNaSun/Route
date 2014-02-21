using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Route.DataInteroperability;

namespace Route
{
    public partial class QuickExportUI : Form
    {
        private Thread QEThread;
        private QuickExport QE;

        public QuickExportUI()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxEx1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxEx2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            button3.Enabled = false;
            ControlBox = false;
            QEThread = new Thread(new ThreadStart(Execute));
            QEThread.Start();
        }

        private void Execute()
        {
            progressBar1.Invoke(new Action(progressBar1.Show));
            progressBar1.Invoke(new Action(progressBar1.Start));
            if (QE == null)
            {
                QE = new QuickExport();
                QE.ExecuteCompleted += new EventHandler(QE_ExecuteCompleted);
            }
            QE.Source = textBoxEx1.Text;
            QE.Destination = "MIF," + textBoxEx2.Text;
            try
            {
                QE.Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Invoke(new Action<bool>(enabled =>
                {
                    button3.Enabled = enabled;
                }), true);
                Invoke(new Action<bool>(enabled =>
                {
                    ControlBox = enabled;
                }), true);
                progressBar1.Invoke(new Action(progressBar1.Stop));
                progressBar1.Invoke(new Action(progressBar1.Hide));
            }
        }

        private void QE_ExecuteCompleted(object sender, EventArgs e)
        {
            MessageBox.Show("转换完成");
        }
    }
}
