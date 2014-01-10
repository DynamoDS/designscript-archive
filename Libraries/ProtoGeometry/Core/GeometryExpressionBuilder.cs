using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    class GeometryExpressionBuilder
    {
        private static readonly string kImportMethodPrefix = "__import_";
        private StringBuilder mExpression = new StringBuilder();
        private int id = 0;
        private static Dictionary<Type, Func<GeometryExpressionBuilder, IDesignScriptEntity, string, string>> mWriters
            = new Dictionary<Type, Func<GeometryExpressionBuilder, IDesignScriptEntity, string, string>>();

        static GeometryExpressionBuilder()
        {
            mWriters.Add(typeof(ICoordinateSystemEntity), WriteEntity<ICoordinateSystemEntity>);
            mWriters.Add(typeof(IPointEntity), WriteEntity<IPointEntity>);
            mWriters.Add(typeof(ILineEntity), WriteEntity<ILineEntity>);
            mWriters.Add(typeof(ICircleEntity), WriteEntity<ICircleEntity>);
            mWriters.Add(typeof(IArcEntity), WriteEntity<IArcEntity>);
            mWriters.Add(typeof(INurbsCurveEntity), WriteEntity<INurbsCurveEntity>);
            mWriters.Add(typeof(ISurfaceEntity), WriteEntity<ISurfaceEntity>);
            mWriters.Add(typeof(ISolidEntity), WriteEntity<ISolidEntity>);
        }

        public static string WriteEntity<T>(GeometryExpressionBuilder eb, IDesignScriptEntity entity, string paramName) where T : IDesignScriptEntity
        {
            return eb.WriteEntity((T)entity, paramName);
        }

        private string WriteEntity(object p, string paramName)
        {
            throw new NotImplementedException();
        }

        public string WriteEntity(IPointEntity point, string paramName = null)
        {
            if(string.IsNullOrEmpty(paramName))
                paramName = string.Format("__point_{0}", ++id);

            mExpression.AppendFormat("{0} = Point.ByCoordinates({1}, {2}, {3});", paramName, point.X, point.Y, point.Z);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(IVectorEntity vector, string paramName = null)
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = string.Format("__vec_{0}", ++id);

            mExpression.AppendFormat("{0} = Vector.ByCoordinates({1}, {2}, {3});", paramName, vector.X, vector.Y, vector.Z);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(ICoordinateSystemEntity cs, string paramName = null)
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = string.Format("__cs_{0}", ++id);

            string origin = WriteEntity(cs.Origin);
            string xaxis = WriteEntity(cs.XAxis);
            string yaxis = WriteEntity(cs.YAxis);
            string zaxis = WriteEntity(cs.ZAxis);
            mExpression.AppendFormat("{0} = CoordinateSystem.ByOriginVectors({1}, {2}, {3}, {4});", paramName, origin, xaxis, yaxis, zaxis);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(ILineEntity line, string paramName = null)
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = string.Format("__line_{0}", ++id);

            string start = WriteEntity(line.StartPoint);
            string end = WriteEntity(line.EndPoint);
            mExpression.AppendFormat("{0} = Line.ByStartPointEndPoint({1}, {2});", paramName, start, end);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(ICircleEntity circle, string paramName = null)
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = string.Format("__circle_{0}", ++id);

            string center = WriteEntity(circle.CenterPoint);
            string normal = WriteEntity(circle.Normal);
            mExpression.AppendFormat("{0} = Circle.ByCenterPointRadius({1}, {2}, {3});", paramName, center, circle.Radius, normal);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(IArcEntity arc, string paramName = null)
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = string.Format("__arc_{0}", ++id);

            string center = WriteEntity(arc.CenterPoint);
            string normal = WriteEntity(arc.Normal);
            mExpression.AppendFormat("{0} = Arc.ByCenterPointRadiusAngle({1}, {2}, {3}, {4}, {5});", paramName, center,
                arc.Radius, GeometryExtension.RadiansToDegrees(arc.StartAngle), GeometryExtension.RadiansToDegrees(arc.SweepAngle), normal);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(INurbsCurveEntity bspline, string paramName = null)
        {
            if (string.IsNullOrEmpty(paramName))
                paramName = string.Format("__bspline_{0}", ++id);

            IPointEntity[] points = bspline.GetControlVertices();
            string cp = WriteEntities(points);

            mExpression.AppendFormat("{0} = BSplineCurve.ByControlVertices({1}, {2}, {3});", paramName, cp, bspline.Degree, bspline.IsPeriodic);
            mExpression.AppendLine();
            return paramName;
        }

        public string WriteEntity(ISolidEntity solid, string paramName)
        {
            WriteSATImport(new IGeometryEntity[] { solid }, paramName);
            return paramName;
        }

        public string WriteEntity(ISurfaceEntity surface, string paramName)
        {
            WriteSATImport(new IGeometryEntity[] { surface }, paramName);
            return paramName;
        }

        private string WriteSATImport(IGeometryEntity[] entities, string paramName)
        {
            string fileName = string.Format("{0}_{1}.sat", paramName, DateTime.Now);
            //solid and surface entities are exports as sat file
            string foldername = Path.GetDirectoryName(GeometrySettings.RootModulePath);
            string filePath = Path.Combine(foldername, fileName);
            if (!HostFactory.Factory.SaveSat(filePath, entities))
                return String.Empty;

            mExpression.AppendFormat(@"{0} = Geometry.ImportFromSAT(""{1}"");", paramName, filePath);
            mExpression.AppendLine();
            return fileName;
        }

        private string WriteDSEntity(IDesignScriptEntity entity, string paramName = null)
        {
            Type type = entity.GetType();
            Type[] interfaces = type.GetInterfaces();

            //Find first available writer for this entity and the serialize.
            for (int i = interfaces.Length - 1; i >= 0; --i)
            {
                Func<GeometryExpressionBuilder, IDesignScriptEntity, string, string> writer;
                if (mWriters.TryGetValue(interfaces[i], out writer))
                {
                    return writer(this, entity, paramName);
                }
            }
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return mExpression.ToString();
        }

        public string WriteEntities(IGeometryEntity[] geometry)
        {
            StringBuilder sb = new StringBuilder("{");
            foreach (var item in geometry)
            {
                try
                {
                    string variable = WriteDSEntity(item, null);
                    sb.AppendFormat("{0}, ", variable);
                }
                catch (NotImplementedException)
                {
                    continue;
                }
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("}");

            return sb.ToString();
        }

        private void WriteString(string value)
        {
            mExpression.AppendLine(value);
        }

        public static string GetExpression(IGeometryEntity[] entities, string paramName)
        {
            GeometryExpressionBuilder eb = new GeometryExpressionBuilder();
            string methodName = kImportMethodPrefix + paramName;
            eb.WriteString(String.Format(@"def {0}() {{", methodName));

            if (entities.Length == 1)
                eb.WriteDSEntity(entities[0], "return");
            else
            {
                List<IGeometryEntity> primitives = new List<IGeometryEntity>();
                List<IGeometryEntity> asm = new List<IGeometryEntity>();

                foreach (var item in entities)
                {
                    if (item is ISurfaceEntity || item is ISolidEntity)
                        asm.Add(item);
                    else
                        primitives.Add(item);
                }
                string sat = "__sat";
                if (asm.Count > 0)
                    eb.WriteSATImport(asm.ToArray(), sat);

                if (primitives.Count > 0)
                {
                    string collection = eb.WriteEntities(primitives.ToArray());
                    if(asm.Count > 0)
                        eb.WriteString(String.Format(@"return = Flatten({{{1}, {0}}});", sat, collection));
                    else
                        eb.WriteString(String.Format("return = {0};", collection));
                }
            }

            eb.WriteString("}");
            eb.WriteString(string.Format("{0} = {1}();", paramName, methodName));

            return eb.ToString();
        }
    }
}
