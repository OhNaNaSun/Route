using System;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcDataBinding;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Route
{
    public partial class AttributeTable : Form
    {
        private TableWrapper tableWrapper;
        private ILayer layer;
        private ITable table;

        public AttributeTable()
        {
            InitializeComponent();
        }

        public ILayer Layer
        {
            set
            {
                if (layer != value)
                {
                    layer = value;
                    OnLayerChanged(EventArgs.Empty);
                }
            }
        }

        protected void OnLayerChanged(EventArgs e)
        {
            table = ((FeatureLayer)layer).FeatureClass as ITable;
            if (table != null)
            {
                tableWrapper = new TableWrapper(table);
                bindingSource.DataSource = tableWrapper;
                dataGridView1.DataSource = bindingSource;
            }
            if (LayerChanged != null)
            {
                LayerChanged(this, e);
            }
        }

        public event EventHandler LayerChanged;
    }
}
