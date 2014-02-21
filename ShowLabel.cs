using System;
using System.Threading;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace Route.Commands
{
    public class ShowLabel
    {
        private ILayer mLayer;
        private string mField;
        private IMapControl3 mMapControl;

        public ShowLabel(ILayer layer, string field, IMapControl3 mapControl)
        {
            mLayer = layer;
            mField = field;
            mMapControl = mapControl;
        }

        public void Show()
        {
            IFeatureLayer featureLayer = (IFeatureLayer)mLayer;
            IDataset dataset = (IDataset)featureLayer;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)dataset.Workspace;
            IFeatureClass featureClass = CreateStandardAnnotationClass(
                featureWorkspace,
                null,
                mLayer.Name + "Annotation",
                mMapControl.SpatialReference,
                mMapControl.MapScale,
                mMapControl.MapUnits,
                null
                );
        }

        public IFeatureClass CreateStandardAnnotationClass(
            IFeatureWorkspace featureWorkspace,
            IFeatureDataset featureDataset,
            String className,
            ISpatialReference spatialReference,
            double referenceScale,
            esriUnits referenceScaleUnits,
            String configKeyword
            )
        {
            // Create an annotation class and provide it with a name.
            ILabelEngineLayerProperties labelEngineLayerProperties = new
                LabelEngineLayerPropertiesClass();
            IAnnotateLayerProperties annotateLayerProperties = (IAnnotateLayerProperties)
                labelEngineLayerProperties;
            annotateLayerProperties.Class = className;

            // Get the symbol from the annotation class. Make any changes to its properties
            // here.
            ITextSymbol annotationTextSymbol = labelEngineLayerProperties.Symbol;
            ISymbol annotationSymbol = (ISymbol)annotationTextSymbol;

            // Create a symbol collection and add the default symbol from the
            // annotation class to the collection. Assign the resulting symbol ID
            // to the annotation class.
            ISymbolCollection symbolCollection = new SymbolCollectionClass();
            ISymbolCollection2 symbolCollection2 = (ISymbolCollection2)symbolCollection;
            ISymbolIdentifier2 symbolIdentifier2 = null;
            symbolCollection2.AddSymbol(annotationSymbol, className, out symbolIdentifier2);
            labelEngineLayerProperties.SymbolID = symbolIdentifier2.ID;

            // Add the annotation class to a collection.
            IAnnotateLayerPropertiesCollection annotateLayerPropsCollection = new
                AnnotateLayerPropertiesCollectionClass();
            annotateLayerPropsCollection.Add(annotateLayerProperties);

            // Create a graphics layer scale object.
            IGraphicsLayerScale graphicsLayerScale = new GraphicsLayerScaleClass();
            graphicsLayerScale.ReferenceScale = referenceScale;
            graphicsLayerScale.Units = referenceScaleUnits;

            // Create the overposter properties for the standard label engine.
            IOverposterProperties overposterProperties = new BasicOverposterPropertiesClass()
                ;

            // Instantiate a class description object.
            //IObjectClassDescription ocDescription = new
            //    AnnotationFeatureClassDescriptionClass();
            //IFeatureClassDescription fcDescription = (IFeatureClassDescription)ocDescription;

            //// Get the shape field from the class description's required fields.
            //IFields requiredFields = ocDescription.RequiredFields;
            //int shapeFieldIndex = requiredFields.FindField(fcDescription.ShapeFieldName);
            //IField shapeField = requiredFields.get_Field(shapeFieldIndex);
            IFeatureLayer layer = (IFeatureLayer)mLayer;
            IFields fields = layer.FeatureClass.Fields;
            IField shapeField = fields.Field[fields.FindField(mField)];
            IGeometryDef geometryDef = shapeField.GeometryDef;
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.SpatialReference_2 = spatialReference;

            // Create the annotation layer factory.
            IAnnotationLayerFactory annotationLayerFactory = new
                FDOGraphicsLayerFactoryClass();

            // Create the annotation feature class and an annotation layer for it.
            IAnnotationLayer annotationLayer = annotationLayerFactory.CreateAnnotationLayer
                (featureWorkspace, featureDataset, className, geometryDef, null,
                annotateLayerPropsCollection, graphicsLayerScale, symbolCollection, false,
                false, false, true, overposterProperties, configKeyword);

            // Get the feature class from the feature layer.
            IFeatureLayer featureLayer = (IFeatureLayer)annotationLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;

            return featureClass;
        }
    }
}
