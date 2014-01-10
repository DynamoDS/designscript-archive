using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using System.Collections;

//[assembly: ContextDataProvider(typeof(Autodesk.DesignScript.Geometry.GeometryDataProvider))]

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    class GeometryDataProvider : IContextDataProvider
    {
        private static readonly string mName = "DesignScript.Geometry"; 
        public string Name
        {
            get { return mName; }
        }

        public IContextData[] ImportData(Dictionary<string, object> connectionParameters)
        {
            List<Geometry> geometryList = new List<Geometry>();
            Geometry[] geometry = null;
            foreach (var param in connectionParameters)
            {
                switch (param.Key)
                {
                    case "OBJ":
                        geometry = Mesh.ImportFromOBJ(Convert.ToString(param.Value));
                        break;
                    case "SAT":
                        geometry = Geometry.ImportFromSAT(Convert.ToString(param.Value));
                        break;
                    default:
                        if (param.Value.GetType().IsArray)
                        {
                            Array data = param.Value as Array;
                            geometry = GeometryDataSerializer.CreateGeometryFromData(param.Key, data);
                        }
                        break;
                }
                if (null != geometry && geometry.Length > 0)
                    geometryList.AddRange(geometry);
            }

            int nItems = geometryList.Count;
            if (nItems == 0)
                return null;

            IContextData[] contextData = new IContextData[nItems];
            for (int i = 0; i < nItems; ++i)
            {
                contextData[i] = new GeometryData("ImportData", geometryList[i], this);
            }
            return contextData;
        }

        Dictionary<string, object> IContextDataProvider.ExportData(IContextData[] data, string filePath)
        {
            throw new NotImplementedException();
        }

        Dictionary<string, object> IContextDataProvider.CaptureData()
        {
            throw new NotImplementedException();
        }

        public string GetExpression(Dictionary<string, object> parameters, string variable)
        {
            object obj = parameters["Handle"];
            Array handles = null;
            if (obj.GetType().IsArray)
                handles = obj as Array;
            else
                handles = new Object[] { obj };

            List<IGeometryEntity> geometry = new List<IGeometryEntity>();
            foreach (var item in handles)
            {
                IPersistentObject persitentObject = HostFactory.PersistenceManager.GetPersistentObjectFromHandle(item);
                if (null != persitentObject)
                    geometry.Add(persitentObject.Geometry);
            }

            return GeometryExpressionBuilder.GetExpression(geometry.ToArray(), variable);
        }
    }

    class GeometryData : IContextData
    {
        private IContextDataProvider mContextDataProvider = null;

        public GeometryData(string name, Object data, IContextDataProvider provider)
        {
            Name = name;
            Data = data;
            mContextDataProvider = provider;

            INotifyPropertyChanged notify = data as INotifyPropertyChanged;
            if (notify != null)
                notify.PropertyChanged += NotifyPropertyChanged;
        }

        void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (null != DataChanged)
                DataChanged(this, new EventArgs());
        }

        public string Name
        {
            get;
            private set;
        }

        public object Data
        {
            get;
            private set;
        }

        public IContextDataProvider ContextProvider
        {
            get { return mContextDataProvider; }
        }

        public event EventHandler DataChanged;
    }
}
