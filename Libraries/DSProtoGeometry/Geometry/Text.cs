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

    public class DSText : DSGeometry
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

        protected DSText(DSCoordinateSystem contextCoordinateSystem, Orientation orientation, string textString, double fontSize,bool persist)
            : base(ByCoordinateSystemCore(contextCoordinateSystem, orientation, textString, fontSize),persist)
        {
            InitializeGuaranteedProperties();
            ContextCoordinateSystem = contextCoordinateSystem;
            Orientation = orientation;
        }

        #endregion

        #region PRIVATE_MEMBERS
        private DSText(ITextEntity entity, bool persist)
            : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        static void InitType()
        {
            RegisterHostType(typeof(ITextEntity), (IGeometryEntity host, bool persist) => { return new DSText(host as ITextEntity, persist); });
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
        public static DSText ByCoordinateSystem(DSCoordinateSystem contextCoordinateSystem, Orientation orientation, string textString, double fontSize)
        {
            return new DSText(contextCoordinateSystem, orientation, textString, fontSize, true);
        }

        #endregion

        #region CORE_METHODS

        private static ITextEntity ByCoordinateSystemCore(DSCoordinateSystem contextCoordinateSystem, Orientation orientation, string textString, double fontSize)
        {
            if (fontSize < 0.0001)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, fontSize, "DSText fontSize"), "fontSize");
            if (null == contextCoordinateSystem)
                throw new System.ArgumentNullException("contextCoordinateSystem");

            AssertUniScaledOrtho(contextCoordinateSystem.CSEntity);

            ITextEntity entity = HostFactory.Factory.TextByCoordinateSystem(contextCoordinateSystem.CSEntity, (int)orientation, textString, fontSize);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSText.ByCoordinateSystem"));
            return entity;
        }

        internal override DSGeometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            AssertUniScaledOrtho(csEntity);

            DSText text = base.TransformBy(csEntity) as DSText;
            text.Orientation = Orientation;
            return text;
        }

        private static void AssertUniScaledOrtho(ICoordinateSystemEntity csEntity)
        {
            if (csEntity.IsUniscaledOrtho())
                return;

            if (csEntity.IsScaledOrtho())
                throw new System.ArgumentException(string.Format(Properties.Resources.NotSupported, "Non Uniform Scaling", "DSText"));
            else
                throw new System.ArgumentException(string.Format(Properties.Resources.NotSupported, "Shear Transform", "DSText"));
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
            var f6 = DSGeometryExtension.DoublePrintFormat;
            return string.Format("DSText(\"{0}\", Orientation : {1}, Size : {2})", TextString, Orientation, FontSize.ToString(f6));
        }

        #endregion
    }
}
