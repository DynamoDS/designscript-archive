using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Text : Geometry
    {
        internal ITextEntity TextEntity { get { return HostImpl as ITextEntity; } }

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            TextString = TextEntity.Text;
            FontSize = TextEntity.Height;
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected Text(CoordinateSystem contextCoordinateSystem, string textString, double fontSize, bool persist)
            : base(ByCoordinateSystemCore(contextCoordinateSystem, textString, fontSize), persist)
        {
            InitializeGuaranteedProperties();
            ContextCoordinateSystem = contextCoordinateSystem;
        }

        #endregion

        #region PRIVATE_MEMBERS
        private Text(ITextEntity entity, bool persist)
            : base (entity, persist )
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
        public static Text ByCoordinateSystem(CoordinateSystem contextCoordinateSystem, string textString, double fontSize)
        {
            return new Text(contextCoordinateSystem, textString, fontSize, true);
        }

        #endregion

        #region CORE_METHODS

        // PB: Do we need orientation here?  Isn't that implied by the orientation of the coordinate system?
        private static ITextEntity ByCoordinateSystemCore(CoordinateSystem contextCoordinateSystem, string textString, double fontSize)
        {
            if (fontSize < 0.0001)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, fontSize, "Text fontSize"), "fontSize");
            if (null == contextCoordinateSystem)
                throw new System.ArgumentNullException("contextCoordinateSystem");

            AssertUniScaledOrtho(contextCoordinateSystem.CSEntity);

            var entity = HostFactory.Factory.TextByCoordinateSystem(contextCoordinateSystem.CSEntity, textString, fontSize);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Text.ByCoordinateSystem"));
            return entity;
        }

        internal override Geometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            AssertUniScaledOrtho(csEntity);

            var text = base.TransformBy(csEntity) as Text;
            return text;
        }

        private static void AssertUniScaledOrtho(ICoordinateSystemEntity csEntity)
        {
            if (csEntity.IsUniscaledOrtho)
                return;

            if (csEntity.IsScaledOrtho)
                throw new System.ArgumentException(string.Format(Properties.Resources.NotSupported, "Non Uniform Scaling", "Text"));
            else
                throw new System.ArgumentException(string.Format(Properties.Resources.NotSupported, "Shear Transform", "Text"));
        }
        #endregion

        #region PROPERTIES

        public string TextString { get; private set; }

        public double FontSize { get; private set; }

        #endregion

        #region FROM_OBJECT

        public override string ToString()
        {
            var f6 = GeometryExtension.DoublePrintFormat;
            return string.Format("Text(\"{0}\", Size : {1})", TextString, FontSize.ToString(f6));
        }

        #endregion
    }
}
