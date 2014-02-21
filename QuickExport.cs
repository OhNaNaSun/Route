using System;
using ESRI.ArcGIS.DataInteroperabilityTools;
using ESRI.ArcGIS.Geoprocessor;

namespace Route.DataInteroperability
{
    public class QuickExport
    {
        public event EventHandler ExecuteCompleted;

        private Geoprocessor gp;
        private ESRI.ArcGIS.DataInteroperabilityTools.QuickExport qe;

        public string Source { get; set; }

        public string Destination { get; set; }

        public QuickExport()
        {
            gp = new Geoprocessor();
            qe = new ESRI.ArcGIS.DataInteroperabilityTools.QuickExport();
        }

        public void Execute()
        {
            qe.Input = Source;
            qe.Output = Destination;
            try
            {
                gp.Execute(qe, null);
            }
            catch (Exception)
            {
                throw;
            }
            OnExecuteCompleted(EventArgs.Empty);
        }

        private void OnExecuteCompleted(EventArgs e)
        {
            if (ExecuteCompleted != null)
            {
                ExecuteCompleted(this, e);
            }
        }
    }
}
