using System;
using System.Threading;
using ESRI.ArcGIS.DataInteroperabilityTools;
using ESRI.ArcGIS.Geoprocessor;

namespace Route.DataInteroperability
{
    /// <summary>
    /// 封装快速导入工具的类
    /// </summary>
    public sealed class QuickImport
    {
        private string source;
        private string destination;
        private Geoprocessor gp;
        private ESRI.ArcGIS.DataInteroperabilityTools.QuickImport qi;

        public QuickImport()
        {
            Init();
        }

        public QuickImport(string source, string destination)
        {
            this.source = source;
            this.destination = destination;
            Init();
        }

        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }

        public string Destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination = value;
            }
        }

        private void Init()
        {
            // 创建处理器
            gp = new Geoprocessor();
            // 创建工具
            qi = new ESRI.ArcGIS.DataInteroperabilityTools.QuickImport();
        }

        /// <summary>
        /// 执行工具
        /// </summary>
        public void Execute()
        {
            // 存入参数
            qi.Input = source;
            qi.Output = destination;
            // 执行
            try
            {
                gp.Execute(qi, null);
            }
            catch (Exception)
            {
                throw;
            }
            // 执行完毕，触发事件
            OnExecuteCompleted(EventArgs.Empty);
        }

        private void OnExecuteCompleted(EventArgs e)
        {
            if (ExecuteCompleted != null)
                ExecuteCompleted(this, e);
        }

        /// <summary>
        /// 执行完毕时被触发
        /// </summary>
        public event EventHandler ExecuteCompleted;
    }
}
