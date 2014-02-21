using System;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;

namespace Route.Commands
{
    public class OpenAttributeTable : BaseCommand, IDisposable
    {
        private bool disposed = false;
        private IMapControl3 m_mapControl;
        private AttributeTable attributeTable;

        public OpenAttributeTable()
        {
            base.m_caption = "打开属性表";
        }

        public override void OnClick()
        {
            ILayer layer = (ILayer)m_mapControl.CustomProperty;
            if (attributeTable == null)
            {
                attributeTable = new AttributeTable();
                attributeTable.FormClosing += new System.Windows.Forms.FormClosingEventHandler(attributeTable_FormClosing);
            }
            attributeTable.Show();
            attributeTable.Layer = layer;
        }

        void attributeTable_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            attributeTable.Dispose();
            attributeTable = null;
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                attributeTable.Dispose();
                attributeTable = null;
            }
            disposed = true;
        }
    }
}
