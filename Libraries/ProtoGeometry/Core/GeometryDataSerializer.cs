using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using System.Collections;

namespace Autodesk.DesignScript.Geometry
{
    interface IGeometryDataCollection : IEnumerable
    {
        void AddData(string parameter, object data);

        Array ToArray();

        object GetData(string parameter);

        bool DoneReading();
    }

    class ParameterizedCollection : IGeometryDataCollection
    {
        List<object> mData = new List<object>();
        Dictionary<string, int> mParameters = new Dictionary<string, int>();
        int mPosition = -1;

        public ParameterizedCollection()
        {
        }

        /// <summary>
        /// Constructor that expects input data as collection of name and value
        /// pairs.
        /// </summary>
        /// <param name="data">List of name, value pair data.</param>
        public ParameterizedCollection(Array data)
        {
            if(data.Length % 2 != 0)
                throw new DataMisalignedException(string.Format("Given data : \"{0}\" is not an array of name value pairs", data));

            int nCount = data.Length / 2;
            for (int i = 0; i < nCount; ++i)
            {
                Object name = data.GetValue(2 * i); //param name
                if (name.GetType() != typeof(string))
                    throw new DataMisalignedException(string.Format("Given data : \"{0}\" is not an array of name value pairs", data));
                mParameters.Add(Convert.ToString(name), i);
                mData.Add(data.GetValue(2 * i + 1));
            }
        }

        public ParameterizedCollection(Dictionary<string, object> data)
        {
            int i = -1;
            foreach (var item in data)
            {
                mParameters.Add(item.Key, ++i);
                mData.Add(item.Value);
            }
        }

        public void AddData(string parameter, object data)
        {
            int index = mData.Count;
            mParameters.Add(parameter, index);
            mData.Add(data);
        }

        public object GetData(string parameter)
        {
            int index = ++mPosition;
            if(null != mParameters || mParameters.Count > 0)
                index = mParameters[parameter];

            return mData[index];
        }

        public IEnumerator GetEnumerator()
        {
            return mData.GetEnumerator();
        }

        public Array ToArray()
        {
            return mData.ToArray();
        }

        public Array ToParameterizedArray()
        {
            List<object> data = new List<object>();
            foreach (var item in mParameters)
            {
                data.Add(item.Key);
                data.Add(mData[item.Value]);
            }
            return data.ToArray();
        }

        public Dictionary<string, object> GetCollection()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>();
            foreach (var item in mParameters)
            {
                collection.Add(item.Key, mData[item.Value]);
            }

            return collection;
        }

        public bool DoneReading()
        {
            return mPosition >= mData.Count - 1;
        }
    }

    /// <summary>
    /// Doesn't have parameter information, assumes that GetData() request for a 
    /// parameter happens in the sequence in which data is stored. Deals with 
    /// compact form of data. This doesn't support AddData() method.
    /// </summary>
    class SerialDataReader : IGeometryDataCollection
    {
        private Array mData = null;
        private int mPosition = -1;

        public SerialDataReader(Array data)
        {
            mData = data;
        }

        public void AddData(string parameter, object data)
        {
            throw new NotImplementedException();
        }

        public object GetData(string parameter)
        {
            return mData.GetValue(++mPosition);
        }

        public IEnumerator GetEnumerator()
        {
            return mData.GetEnumerator();
        }

        public Array ToArray()
        {
            return mData;
        }

        public bool DoneReading()
        {
            return mPosition >= mData.Length - 1;
        }
    }

    class TypeSpecificSerializers
    {
        private Dictionary<string, Func<GeometryDataSerializer, IDesignScriptEntity>> mDataReaders
            = new Dictionary<string, Func<GeometryDataSerializer, IDesignScriptEntity>>();

        private Dictionary<Type, Func<IDesignScriptEntity, IGeometryDataCollection>> mDataWriters
            = new Dictionary<Type, Func<IDesignScriptEntity, IGeometryDataCollection>>();

        private Dictionary<Type, string> mDataTypes = new Dictionary<Type, string>();

        public void RegisterSerializers<T>(string type, Func<GeometryDataSerializer, IDesignScriptEntity> reader) where T : IDesignScriptEntity
        {
            mDataTypes.Add(typeof(T), type);
            mDataReaders.Add(type, reader);
            mDataWriters.Add(typeof(T), (IDesignScriptEntity e) => GeometryDataSerializer.WriteEntity((T)e));
        }

        public Func<GeometryDataSerializer, IDesignScriptEntity> GetReader(string geomType)
        {
            Func<GeometryDataSerializer, IDesignScriptEntity> reader = null;
            mDataReaders.TryGetValue(geomType, out reader);
            return reader;
        }

        /// <summary>
        /// Finds the underlying interface implementation type and uses appropriate
        /// writer to serialize the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="typeName">output type name</param>
        /// <returns></returns>
        public IGeometryDataCollection WriteEntity(IDesignScriptEntity entity, out string typeName)
        {
            Type type = entity.GetType();
            Type[] interfaces = type.GetInterfaces();

            //Find first available writer for this entity and the serialize.
            for (int i = interfaces.Length - 1; i >= 0; --i)
            {
                Func<IDesignScriptEntity, IGeometryDataCollection> writer;
                if (mDataWriters.TryGetValue(interfaces[i], out writer))
                {
                    typeName = mDataTypes[interfaces[i]];
                    return writer(entity);
                }
            }
            throw new NotImplementedException();
        }
    }

    class GeometryDataSerializer : IGeometryDataCollection
    {
        delegate TResult Reader<in GeometryDataSerializer, out TResult>(GeometryDataSerializer stream);

        private IGeometryDataCollection mData = null;
        private bool keepInternalDataParametrized = false;

        private static TypeSpecificSerializers mSerializers = new TypeSpecificSerializers();

        static GeometryDataSerializer()
        {
            //Register data serializers
            mSerializers.RegisterSerializers<IPointEntity>("Point", (GeometryDataSerializer s) => s.ReadPoint("Point"));
            mSerializers.RegisterSerializers<ICoordinateSystemEntity>("CoordinateSystem", (GeometryDataSerializer s) => s.ReadCoordinateSystem());
            mSerializers.RegisterSerializers<ILineEntity>("Line", (GeometryDataSerializer s) => s.ReadLine());
            mSerializers.RegisterSerializers<ICircleEntity>("Circle", (GeometryDataSerializer s) => s.ReadCircle());
            mSerializers.RegisterSerializers<IArcEntity>("Arc", (GeometryDataSerializer s) => s.ReadArc());
            mSerializers.RegisterSerializers<IBSplineCurveEntity>("BSplineCurve", (GeometryDataSerializer s) => s.ReadBSplineCurve());
        }

        /// <summary>
        /// To be used for reading/de-serializating data
        /// </summary>
        /// <param name="data">Data to deserialize from</param>
        public GeometryDataSerializer(Array data)
        {
            mData = new SerialDataReader(data);
        }

        /// <summary>
        /// To be used for reading/de-serializating parameterized data
        /// </summary>
        /// <param name="data">Parameterized data to deserialize from</param>
        public GeometryDataSerializer(Dictionary<string, object> data)
        {
            mData = new ParameterizedCollection(data);
        }

        /// <summary>
        /// To be used for serialization of data
        /// </summary>
        public GeometryDataSerializer()
        {
            mData = new ParameterizedCollection();
        }

        /// <summary>
        /// Creates data reader from the given set of data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static IGeometryDataCollection CreateDataReader(object data)
        {
            Array collection = data as Array;
            if (null == collection)
                throw new InvalidCastException(string.Format("Given data : \"{0}\" is not an array", data));

            if (collection.Length % 2 != 0 || collection.GetValue(0).GetType() != typeof(string))
                return new SerialDataReader(collection);

            return new ParameterizedCollection(collection);
        }

        #region Primitive data readers
        bool ReadBoolean(string parameter)
        {
            return Convert.ToBoolean(GetData(parameter));
        }

        double ReadDouble(string parameter)
        {
            return Convert.ToDouble(GetData(parameter));
        }

        int ReadInteger(string parameter)
        {
            return Convert.ToInt32(GetData(parameter));
        }

        string ReadString(string parameter)
        {
            return Convert.ToString(GetData(parameter));
        }

        T ReadObject<T>(string parameter, Reader<GeometryDataSerializer, T> reader)
        {
            Array p = GetData(parameter) as Array;
            if (null == p)
                throw new DataMisalignedException(string.Format("The data for parameter : {0} is not in {1} format.", parameter, reader));

            GeometryDataSerializer stream = new GeometryDataSerializer(p);
            return reader(stream);
        }

        #endregion

        #region Primitive data writers
        private void WriteStream(string parameter, IGeometryDataCollection collection, bool parameterized)
        {
            Array data = null;
            if (parameterized)
            {
                ParameterizedCollection reader = collection as ParameterizedCollection;
                if (null != reader)
                    data = reader.ToParameterizedArray();
            }
            else
                data = collection.ToArray();

            WriteObject(parameter, data);
        }

        public void WriteObject(string parameter, object value)
        {
            mData.AddData(parameter, value);
        }

        public void WriteBoolean(string parameter, bool value)
        {
            WriteObject(parameter, value);
        }

        public void WriteDouble(string parameter, double value)
        {
            WriteObject(parameter, value);
        }

        public void WriteInteger(string parameter, int value)
        {
            WriteObject(parameter, value);
        }

        public void WriteString(string parameter, string value)
        {
            WriteObject(parameter, value);
        }

        void WriteEntity<T>(string parameter, T entity)
        {
            GeometryDataSerializer stream = WriteEntity(entity);
            WriteStream(parameter, stream, keepInternalDataParametrized);
        }

        void WriteEntity<T>(string parameter, T[] collection)
        {
            GeometryDataSerializer stream = new GeometryDataSerializer();
            int i = 0;
            foreach (var item in collection)
            {
                stream.WriteEntity(parameter + i, item);
                ++i;
            }
            WriteStream(parameter, stream, false); //writing as array
        }

        void WriteObject(object data)
        {
            throw new NotSupportedException(string.Format("WriteObject without parameter name is not supported for {0}", data.GetType()));
        }

        public static GeometryDataSerializer WriteEntity<T>(T entity)
        {
            GeometryDataSerializer stream = new GeometryDataSerializer();
            stream.WriteObject(entity);
            return stream;
        }

        /// <summary>
        /// Finds the underlying interface implementation type and uses appropriate
        /// writer to serialize the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="typeName">output type name</param>
        /// <returns></returns>
        public static IGeometryDataCollection WriteEntity(IDesignScriptEntity entity, out string typeName)
        {
            return mSerializers.WriteEntity(entity, out typeName);
        }
        #endregion

        /// <summary>
        /// Creates geometry from given set of data
        /// </summary>
        /// <param name="geomType">Geometry type</param>
        /// <param name="data">Collection of input data to create geometry from</param>
        /// <returns>Collection of Geometry created using the given data</returns>
        public static Geometry[] CreateGeometryFromData(string geomType, Array data)
        {
            Func<GeometryDataSerializer, IDesignScriptEntity> reader = mSerializers.GetReader(geomType);
            if (reader == null)
                return null;

            List<Geometry> geometry = new List<Geometry>();
            GeometryDataSerializer stream = new GeometryDataSerializer(data);
            while (!stream.DoneReading())
            {
                IGeometryEntity entity = reader(stream) as IGeometryEntity;
                geometry.Add(Geometry.ToGeometry(entity, true));
            }

            return geometry.ToArray();
        }

        #region Point
        IPointEntity ReadPoint(string parameter)
        {
            return ReadObject<IPointEntity>(parameter, (GeometryDataSerializer s)=>s.ReadPoint());
        }

        public IPointEntity ReadPoint()
        {
            return HostFactory.Factory.CreatePoint(ReadDouble("X"), ReadDouble("Y"), ReadDouble("Z"));
        }

        public void WriteObject(IPointEntity point)
        {
            WriteDouble("X", point.X);
            WriteDouble("Y", point.Y);
            WriteDouble("Z", point.Z);
        }

        #endregion

        #region Vector
        IVector ReadVector(string parameter)
        {
            return ReadObject<IVector>(parameter, (GeometryDataSerializer s) => s.ReadVector());
        }

        public IVector ReadVector()
        {
            return DsVector.ByCoordinates(this.ReadDouble("X"), this.ReadDouble("Y"), this.ReadDouble("Z"));
        }

        public void WriteObject(IVector vector)
        {
            WriteDouble("X", vector.X);
            WriteDouble("Y", vector.Y);
            WriteDouble("Z", vector.Z);
        }
        #endregion

        #region CoordinateSystem
        public ICoordinateSystemEntity ReadCoordinateSystem()
        {
            ICoordinateSystemEntity cs = HostFactory.Factory.CoordinateSystemByData(null);
            using (IPointEntity origin = ReadPoint("Origin"))
            {
                IVector xAxis = ReadVector("XAxis");
                IVector yAxis = ReadVector("YAxis");
                IVector zAxis = ReadVector("ZAxis");
                cs.Set(origin, xAxis, yAxis, zAxis);
                return cs;
            }
        }

        public void WriteObject(ICoordinateSystemEntity entity)
        {
            WriteEntity("Origin", entity.Origin);
            WriteEntity("XAxis", entity.XAxis);
            WriteEntity("YAxis", entity.YAxis);
            WriteEntity("ZAxis", entity.ZAxis);
        }
        #endregion

        #region Line
        public ILineEntity ReadLine()
        {
            using (IPointEntity start = ReadPoint("StartPoint"), end = ReadPoint("EndPoint"))
            {
                return HostFactory.Factory.LineByStartPointEndPoint(start, end);
            }
        }

        public void WriteObject(ILineEntity line)
        {
            WriteEntity("StartPoint", line.StartPoint);
            WriteEntity("EndPoint", line.EndPoint);
        }
        #endregion

        #region Circle
        public ICircleEntity ReadCircle()
        {
            using (IPointEntity cen = ReadPoint("CenterPoint"))
            {
                double radius = ReadDouble("Radius");
                IVector normal = ReadVector("Normal");
                return HostFactory.Factory.CircleByCenterPointRadius(cen, radius, normal);
            }
        }

        public void WriteObject(ICircleEntity circle)
        {
            WriteEntity("CenterPoint", circle.CenterPoint);
            WriteDouble("Radius", circle.Radius);
            WriteEntity("Normal", circle.Normal);
        }
        #endregion

        #region Arc
        public IArcEntity ReadArc()
        {
            using (IPointEntity cen = ReadPoint("CenterPoint"))
            {
                double radius = ReadDouble("Radius");
                double startAngle = ReadDouble("StartAngle");
                double sweepAngle = ReadDouble("SweepAngle");
                double endAngle = startAngle + sweepAngle;
                IVector normal = ReadVector("Normal");
                return HostFactory.Factory.ArcByCenterPointRadiusAngle(cen, radius, startAngle, endAngle, normal);
            }
        }

        public void WriteObject(IArcEntity entity)
        {
            WriteEntity("CenterPoint", entity.CenterPoint);
            WriteDouble("Radius", entity.Radius);
            WriteDouble("StartAngle", entity.StartAngle);
            WriteDouble("SweepAngle", entity.SweepAngle);
            WriteEntity("Normal", entity.Normal);
        }
        #endregion

        #region BSplineCurve
        public IBSplineCurveEntity ReadBSplineCurve()
        {
            Object data = GetData("ControlVertices");
            Array vertices = data as Array;
            if (null == vertices)
                throw new DataMisalignedException(string.Format("Invalid data : {0} for parameter {1}", data, "ControlVertices"));

            GeometryDataSerializer stream = new GeometryDataSerializer(vertices);
            List<IPointEntity> points = new List<IPointEntity>();
            foreach (var item in stream)
            {
                points.Add(stream.ReadPoint());
            }

            int degree = ReadInteger("Degree");
            bool periodic = ReadBoolean("IsPeriodic");

            IBSplineCurveEntity entity = HostFactory.Factory.BSplineByControlVertices(points.ToArray(), degree, periodic);
            points.ForEach(GeometryExtension.DisposeObject);
            return entity;
        }

        public void WriteObject(IBSplineCurveEntity entity)
        {
            IPointEntity[] points = entity.GetControlVertices();
            WriteEntity<IPointEntity>("ControlVertices", points);
            WriteInteger("Degree", entity.GetDegree());
            WriteBoolean("IsPeriodic", entity.GetIsPeriodic());
        }
        #endregion

        public void AddData(string parameter, object data)
        {
            mData.AddData(parameter, data);
        }

        public object GetData(string parameter)
        {
            return mData.GetData(parameter);
        }

        public Array ToArray()
        {
            return mData.ToArray();
        }

        public IEnumerator GetEnumerator()
        {
            return mData.GetEnumerator();
        }

        public override string ToString()
        {
            return ToString(mData);
        }

        public static string ToString(IEnumerable data)
        {
            StringBuilder sb = new StringBuilder("{");
            foreach (var item in data)
                sb.AppendFormat(@"{0},", ToString(item));

            sb.Remove(sb.Length - 1, 1);
            sb.Append(@"}");
            return sb.ToString();
        }

        private static string ToString(object obj)
        {
            Type type = obj.GetType();
            if (type.IsArray)
            {
                IEnumerable data = obj as IEnumerable;
                return ToString(data);
            }
            else if (type == typeof(string))
                return string.Format(@"""{0}""", obj.ToString());
            else if (type == typeof(double))
            {
                double value = Convert.ToDouble(obj);
                return value.ToString("F6");
            }
            return obj.ToString();
        }

        public bool DoneReading()
        {
            return mData.DoneReading();
        }
    }
}
