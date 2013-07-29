using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    public static class DSGeometryExtension
    {
        internal static string DoublePrintFormat = "F6";

        public static bool LessThanOrEquals(double x, double y, double tolerance = 0.000001)
        {
            if (Equals(x, y, tolerance))
            {
                return true;
            }
            else if (x < y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Equals(double x, double y, double tolerance = 0.000001)
        {
            return Math.Abs(x - y) < tolerance;
        }

        public static bool EqualsTo(this Double thisValue, Double value)
        {
            return DSGeometryExtension.Equals(thisValue, value);
        }

        public static bool LessThanOrEqualTo(this Double thisValue, Double value)
        {
            return thisValue < value || DSGeometryExtension.Equals(thisValue, value);
        }

        [Browsable(false)]
        internal static bool EqualsTo(this IPointEntity thisValue, IPointEntity value)
        {
            if (Object.ReferenceEquals(thisValue, value))
                return true;
            if (null == thisValue || null == value)
                return false;

            return DSGeometryExtension.Equals(thisValue.X, value.X) && DSGeometryExtension.Equals(thisValue.Y, value.Y) &&
                    DSGeometryExtension.Equals(thisValue.Z, value.Z);
        }

        [Browsable(false)]
        internal static bool ArePointsColinear(this IPointEntity[] pts)
        {
            if (null == pts)
                return false;
            if (pts.Length < 3)
                return true;

            int nCount = pts.Length - 2;
            for (int i = 0; i < nCount; ++i)
            {
                var vec12 = pts[i].GetVectorTo(pts[i + 1]);
                var vec23 = pts[i + 1].GetVectorTo(pts[i + 2]);
                if (!vec12.IsZeroVector() && !vec23.IsZeroVector() && !vec12.IsParallel(vec23))
                    return false;
            }

            return true;
        }

        [Browsable(false)]
        public static bool AreCoincident(this IPointEntity[] pts)
        {
            if (null == pts)
                return true;
            int nPts = pts.Length;
            for (int i = 0; i < nPts - 1; ++i)
            {
                if (!pts[i].EqualsTo(pts[i + 1]))
                    return false;
            }

            return true;
        }

        public static bool ArePointsColinear(DSPoint one, DSPoint two, DSPoint three)
        {
            var onePos = one.PointEntity;
            var twoPos = two.PointEntity;
            var thrPos = three.PointEntity;

            var vec12 = onePos.GetVectorTo(twoPos);
            var vec23 = twoPos.GetVectorTo(thrPos);

            return vec12.IsZeroVector() || vec23.IsZeroVector() || vec12.IsParallel(vec23);
        }

        [Browsable(false)]
        private static ILineEntity GetPerpBisector(DSPoint startPt, DSPoint endPt,
                                            IPlaneEntity planeOfCircle)
        {
            DSVector dir = startPt.DirectionTo(endPt);
            dir = dir.Normalize();

            IPointEntity midPt = HostFactory.Factory.CreatePoint((startPt.X + endPt.X) / 2, (startPt.Y + endPt.Y) / 2, (startPt.Z + endPt.Z) / 2);

            //  get the perpendicular plane to plane of circle at mid point of segment[startPt, endPt]
            //
            IPlaneEntity perpPlane = HostFactory.Factory.PlaneByOriginNormal(midPt, dir.IVector);

            //  this intersection results in line perp to segment[startPt, endPt] & plane normal
            //  and happens to be the perpBisector of mentioned segment in planeOfCircle
            //
            ILineEntity perpBisector = planeOfCircle.IntersectWith(perpPlane);
            return perpBisector;
        }

        private static DSPoint GetCircumCenter(DSPoint firstPt, DSPoint secondPt, DSPoint thirdPt, DSVector normal)
        {
            var one = firstPt.PointEntity;
            var two = secondPt.PointEntity;
            var thr = thirdPt.PointEntity;

            using (IPlaneEntity planeOfCircle = HostFactory.Factory.PlaneByOriginNormal(one, normal.IVector))
            {
                using (ILineEntity perpBisector1 = GetPerpBisector(firstPt, secondPt, planeOfCircle))
                {
                    using (ILineEntity perpBisector2 = GetPerpBisector(secondPt, thirdPt, planeOfCircle))
                    {
                        IPointEntity[] circumCntr = perpBisector1.IntersectWith(perpBisector2);
                        if (circumCntr == null || circumCntr.Length < 1)
                        {
                            return null;
                        }
                        return circumCntr[0].ToPoint(false, null);
                    }
                }
            }
        }

        internal static DSPoint GetCircumCenter(DSPoint one, DSPoint two, DSPoint three, out DSVector normal)
        {
            normal = null;
            if (one == null || two == null || three == null)
            {
                return null;
            }
            else if (ArePointsColinear(one, two, three))
            {
                return null;
            }

            var dir1 = one.PointEntity.GetVectorTo(two.PointEntity);
            var dir2 = two.PointEntity.GetVectorTo(three.PointEntity);
            var normDir = dir1.Cross(dir2);
            if (normDir.IsZeroVector())
            {
                return null;
            }
            normal = normDir.Normalize();
            return GetCircumCenter(one, two, three, normal);
        }

        public static readonly double PI = 3.14159265358979323846;

        public static double DegreesToRadians(double deg)
        {
            return deg * PI / 180.0;
        }

        public static double RadiansToDegrees(double rad)
        {
            return rad * 180.0 / PI;
        }

        public static double AngleBetweenUnitVectors(DSVector one, DSVector two)
        {
            var dp = one.Dot(two);
            return RadiansToDegrees(Math.Acos(dp));
        }

        [Browsable(false)]
        public static DSVector GetVectorTo(this IPointEntity fromPoint, IPointEntity toPoint)
        {
            return DSVector.ByCoordinates(toPoint.X - fromPoint.X, toPoint.Y - fromPoint.Y, toPoint.Z - fromPoint.Z);
        }

        [Browsable(false)]
        public static IPointEntity Add(this IPointEntity point, DSVector direction)
        {
            return HostFactory.Factory.CreatePoint(point.X + direction.X, point.Y + direction.Y, point.Z + direction.Z);
        }

        [Browsable(false)]
        public static double DistanceTo(this IPointEntity fromPoint, IPointEntity toPoint)
        {
            return fromPoint.GetVectorTo(toPoint).GetLength();
        }

        [Browsable(false)]
        public static IPointEntity[][] ToPointEntityArray(this DSPoint[][] points, bool checkRectangular = true)
        {
            if (null == points)
                return null;

            List<IPointEntity[]> hostPoints = new List<IPointEntity[]>();
            int length = -1;
            foreach (var item in points)
            {
                IPointEntity[] hosts = item.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
                if (null == hosts || hosts.Length == 0)
                    continue;

                if (checkRectangular)
                {
                    if (length == -1)
                        length = hosts.Length;
                    if (length != hosts.Length)
                        return null;
                }
                hostPoints.Add(hosts);
            }
            if (hostPoints.Count == 0)
                return null;

            return hostPoints.ToArray();
        }

        [Browsable(false)]
        public static DSPoint[][] ToPointArray(this IPointEntity[][] points, bool checkRectangular = true)
        {
            List<DSPoint[]> hostPoints = new List<DSPoint[]>();
            int length = -1;
            foreach (var item in points)
            {
                DSPoint[] pts = item.ConvertAll((IPointEntity p) => p.ToPoint(false, null));
                if (null == pts)
                    continue;

                if (checkRectangular)
                {
                    if (length == -1)
                        length = pts.Length;
                    if (length != pts.Length)
                        return null;
                }

                hostPoints.Add(pts);
            }
            return hostPoints.ToArray();
        }

        private static GeomClass ToGeometry<GeomClass, GeomEntity>(this GeomEntity host, bool persist = false, DSGeometry context = null)
            where GeomClass : DSGeometry
            where GeomEntity : IGeometryEntity
        {
            return DSGeometry.ToGeometry(host, persist, context) as GeomClass;
        }

        [Browsable(false)]
        internal static GEOM[] ToArray<GEOM, ENTITY>(this ENTITY[] hosts, bool persist = true, DSGeometry context = null)
            where GEOM : DSGeometry
            where ENTITY : IGeometryEntity
        {
            if (null == hosts)
                return null;

            List<GEOM> objects = new List<GEOM>();
            foreach (var item in hosts)
            {
                if (null == item)
                    continue;

                GEOM obj = DSGeometry.ToGeometry(item, persist, context) as GEOM;
                if (obj == null)
                    continue;

                objects.Add(obj);
            }

            return objects.ToArray();
        }

        [Browsable(false)]
        public static IDesignScriptEntity ToEntity(this DesignScriptEntity data)
        {
            return data.ToEntity<DesignScriptEntity, IDesignScriptEntity>();
        }

        [Browsable(false)]
        internal static ENTITY ToEntity<GEOMETRY, ENTITY>(this GEOMETRY data)
            where GEOMETRY : DesignScriptEntity
            where ENTITY : IDesignScriptEntity
        {
            if (null == data)
                return default(ENTITY);

            return (ENTITY)data.HostImpl;
        }

        [Browsable(false)]
        public static ICurveEntity GetCurveEntity(DSCurve curve, bool checkClosed)
        {
            bool isClosed = curve.IsClosed;
            if ((checkClosed && isClosed) || (!checkClosed && !isClosed))
                return curve.CurveEntity;

            return null;
        }

        internal static bool IsDisposed<T>(this T obj) where T : DesignScriptEntity
        {
            return null == obj || obj.IsDisposed;
        }

        internal static bool IsDisposed<T>(this T[] obj) where T : DesignScriptEntity
        {
            if (null == obj)
                return true;
            foreach (var item in obj)
            {
                if (item.IsDisposed())
                    return true;
            }
            return false;
        }

        internal static bool IsDisposed<T>(this T[][] obj) where T : DesignScriptEntity
        {
            if (null == obj)
                return true;
            foreach (var item in obj)
            {
                if (item.IsDisposed())
                    return true;
            }
            return false;
        }

        internal static void DisposeObject(this IDisposable obj)
        {
            if (null != obj)
                obj.Dispose();
        }

        internal static void DisposeObject<T>(ref T obj) where T : DesignScriptEntity
        {
            if (null != obj)
            {
                obj.ReleaseEntity();
                if (obj.IsDisposed)
                    obj = null;
            }
        }

        internal static void DisposeObject<T>(ref T[] obj) where T : DesignScriptEntity
        {
            obj.ForEach((T item) => DSGeometryExtension.DisposeObject(ref item));
            if (obj.IsDisposed())
                obj = null;
        }

        internal static void DisposeObject<T>(ref T[][] obj) where T : DesignScriptEntity
        {
            obj.ForEach((T[] item) => DSGeometryExtension.DisposeObject(ref item));
            if (obj.IsDisposed())
                obj = null;
        }

        internal static void DisposeObject(this IDisposable[] obj)
        {
            obj.ForEach(DSGeometryExtension.DisposeObject);
        }

        internal static void DisposeObject(this IDisposable[][] obj)
        {
            obj.ForEach<IDisposable>(DSGeometryExtension.DisposeObject);
        }

        internal static void AssignTo<T>(this T right, ref T left) where T : DesignScriptEntity
        {
            DSGeometryExtension.DisposeObject(ref left);
            left = right.Retain();
        }

        internal static void AssignTo<T>(this T[] right, ref T[] left) where T : DesignScriptEntity
        {
            DSGeometryExtension.DisposeObject(ref left);
            left = right.Retain();
        }

        internal static void AssignTo<T>(this T[][] right, ref T[][] left) where T : DesignScriptEntity
        {
            DSGeometryExtension.DisposeObject(ref left);
            left = right.Retain();
        }

        internal static DesignScriptEntity Retain(this DesignScriptEntity entity)
        {
            if (null != entity)
                entity.RetainEntity();
            return entity;
        }

        internal static T Retain<T>(this T entity) where T : DesignScriptEntity
        {
            if (null != entity)
                entity.RetainEntity();
            return entity;
        }

        internal static T[] Retain<T>(this T[] entities) where T : DesignScriptEntity
        {
            if (null == entities)
                return entities;

            foreach (var item in entities)
                item.Retain();

            return entities;
        }

        internal static T[][] Retain<T>(this T[][] entities) where T : DesignScriptEntity
        {
            if (null == entities)
                return entities;

            foreach (var item in entities)
                item.Retain();

            return entities;
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (null == array)
                return;
            foreach (var item in array)
                action(item);
        }

        public static void ForEach<T>(this T[][] array, Action<T> action)
        {
            if (null == array)
                return;
            foreach (var item in array)
                item.ForEach(action);
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Converter<TInput, TOutput> converter)
        {
            if (null == array)
                return null;

            List<TOutput> retArray = new List<TOutput>();
            foreach (var input in array)
            {
                if (null == input)
                    continue;

                TOutput val = converter(input);
                if (null == val)
                    continue;

                retArray.Add(val);
            }
            if (retArray.Count == 0)
                return null;
            return retArray.ToArray();
        }

        internal static DSLine[] CreateEdges(DSPoint[] points)
        {
            int nCount = points.Length;
            DSLine[] edges = new DSLine[nCount];
            for (int i = 0; i < nCount - 1; ++i)
                edges[i] = new DSLine(points[i], points[i + 1], false);

            edges[nCount - 1] = new DSLine(points[nCount - 1], points[0], false);
            return edges;
        }

        internal static DSPoint[] CreatePoints(this IPointEntity[] points)
        {
            return points.ToArray<DSPoint, IPointEntity>(false);
        }

        internal static DSPoint ToPoint(this IPointEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSPoint, IPointEntity>(persist, context);
        }

        internal static DSCurve ToCurve(this ICurveEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSCurve, ICurveEntity>(persist, context);
        }

        internal static DSLine ToLine(this ILineEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSLine, ILineEntity>(persist, context);
        }

        internal static DSSurface ToSurf(this ISurfaceEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSSurface, ISurfaceEntity>(persist, context);
        }

        internal static DSBSplineSurface ToBSurf(this IBSplineSurfaceEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSBSplineSurface, IBSplineSurfaceEntity>(persist, context);
        }

        internal static DSPlane ToPlane(this IPlaneEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSPlane, IPlaneEntity>(persist, context);
        }

        internal static DSSolid ToSolid(this ISolidEntity host, bool persist, DSGeometry context)
        {
            return host.ToGeometry<DSSolid, ISolidEntity>(persist, context);
        }

        /// <summary>
        /// Internal utility method
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="distance"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        internal static IBSplineSurfaceEntity[] ExtrudeAsBSplineSurfaces(this ICurveEntity profile, double distance, DSVector direction)
        {
            if (null == profile || direction.IsZeroVector())
                return null;

            using (IPointEntity startPt = profile.PointAtParameter(0.5))
            {
                DSVector offset = direction.Normalize().Scale(distance);
                using (IPointEntity endPt = startPt.CopyAndTranslate(offset.IVector) as IPointEntity)
                {
                    using (ILineEntity path = HostFactory.Factory.LineByStartPointEndPoint(startPt, endPt))
                    {
                        using (ISurfaceEntity surf = HostFactory.Factory.SurfaceBySweep(profile, path))
                        {
                            return surf.ConvertToBSplineSurface();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Clips parameter between 0 and 1
        /// </summary>
        /// <param name="param">Input parameter</param>
        /// <returns>Boolean to indicate if parameter was clipped</returns>
        internal static bool ClipParamRange(ref double param)
        {
            if (param < 0.0)
            {
                param = 0.0;
                return true;
            }
            else if (param > 1.0)
            {
                param = 1.0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Locates given file from pre-defined search path set to this system
        /// </summary>
        /// <param name="fileName">File name to locate.</param>
        /// <returns>Full path for the given file.</returns>
        public static string LocateFile(string fileName)
        {
            IExecutionSession session = DSApplication.Instance.Session;
            if (null != session)
            {
                string filePath = session.SearchFile(fileName);
                if (null != filePath)
                    return filePath;
            }
            else
            {
                //In the case the session is null
                //But we still need to search the application directory.
                //Here, search the application directory.
                string fullPathName;
                System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                Uri codeBaseUri = new Uri(executingAssembly.CodeBase);
                fullPathName = Path.Combine(Path.GetDirectoryName(codeBaseUri.LocalPath), fileName);
                if (null != fullPathName && File.Exists(fullPathName))
                    return fullPathName;
            }

            //Finally also look at PATH environment variable.
            string[] paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';');
            foreach (string test in paths)
            {
                string path = test.Trim();
                if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, fileName)))
                    return Path.GetFullPath(path);
            }

            return fileName;
        }

    }
}
