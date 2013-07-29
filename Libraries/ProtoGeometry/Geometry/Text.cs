using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public enum Orientation
    {
        XYPlane = 0,
        XZPlane = 1,
        YZPlane = 2,
    }

    public class Text : Geometry
    {
        internal ITextEntity TextEntity { get { return HostImpl as ITextEntity; } }

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            TextString = TextEntity.GetString();
            FontSize = TextEntity.GetFontSize();
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected Text(CoordinateSystem contextCoordinateSystem, Orientation orientation, string textString, double fontSize,bool persist)
            : base(ByCoordinateSystemCore(contextCoordinateSystem, orientation, textString, fontSize),persist)
        {
            InitializeGuaranteedProperties();
            ContextCoordinateSystem = contextCoordinateSystem;
            Orientation = orientation;
        }

        #endregion

        #region PRIVATE_MEMBERS
        private Text(ITextEntity entity, bool persist)
            : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        static void InitType()
        {
            RegisterHostType(typeof(ITextEntity), (IGeometryEntity host, bool persist) => { return new Text(host as ITextEntity, persist); });
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTOR

        /// <summary>
        /// Constructs the Text starting from the origin of ParentCoordinateSystem. 
        /// </summary>
        /// <param name="contextCoordinateSystem">The origin of the 
        /// ParentCoordinateSystem will serve as the starting point of the newly 
        /// constructed text</param>
        /// <param name="orientation">
        /// The orientation of the text object constructed</param>
        /// <param name="textString">
        /// The text string content of the text object constructed</param>
        /// <param name="fontSize">
        /// The font of the text string content of the text object constructed</param>
        /// <returns></returns>
        public static Text ByCoordinateSystem(CoordinateSystem contextCoordinateSystem, Orientation orientation, string textString, double fontSize)
        {
            return new Text(contextCoordinateSystem, orientation, textString, fontSize, true);
        }

        #endregion

        #region CORE_METHODS

        private static ITextEntity ByCoordinateSystemCore(CoordinateSystem contextCoordinateSystem, Orientation orientation, string textString, double fontSize)
        {
            if (fontSize < 0.0001)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, fontSize, "Text fontSize"), "fontSize");
            if (null == contextCoordinateSystem)
                throw new System.ArgumentNullException("contextCoordinateSystem");

            AssertUniScaledOrtho(contextCoordinateSystem.CSEntity);

            ITextEntity entity = HostFactory.Factory.TextByCoordinateSystem(contextCoordinateSystem.CSEntity, (int)orientation, textString, fontSize);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Text.ByCoordinateSystem"));
            return entity;
        }

        internal override Geometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            AssertUniScaledOrtho(csEntity);

            Text text = base.TransformBy(csEntity) as Text;
            text.Orientation = Orientation;
            return text;
        }

        private static void AssertUniScaledOrtho(ICoordinateSystemEntity csEntity)
        {
            if (csEntity.IsUniscaledOrtho())
                return;

            if (csEntity.IsScaledOrtho())
                throw new System.ArgumentException(string.Format(Properties.Resources.NotSupported, "Non Uniform Scaling", "Text"));
            else
                throw new System.ArgumentException(string.Format(Properties.Resources.NotSupported, "Shear Transform", "Text"));
        }
        #endregion

        #region PROPERTIES

        public Orientation Orientation { get; private set; }

        public string TextString { get; private set; }

        public double FontSize { get; private set; }

        #endregion

        #region FROM_OBJECT

        public override string ToString()
        {
            var f6 = GeometryExtension.DoublePrintFormat;
            return string.Format("Text(\"{0}\", Orientation : {1}, Size : {2})", TextString, Orientation, FontSize.ToString(f6));
        }

        #endregion
    }
}
