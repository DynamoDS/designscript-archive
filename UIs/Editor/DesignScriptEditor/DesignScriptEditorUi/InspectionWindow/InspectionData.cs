using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace DesignScript.Editor
{
    public class InspectionData
    {
        #region Private Properties

        private string displayText = string.Empty;
        private string expression = string.Empty;
        private string runtimeValue = string.Empty;
        private string runtimeType = string.Empty;

        private int arrayIndexer = -1;
        private int expansionLimit = 0;
        private bool isExpanded = false;
        private InspectionData parentData = null;
        private ObservableCollection<InspectionData> derivations = null;
        private ProtoCore.DSASM.AddressType opType = ProtoCore.DSASM.AddressType.Invalid;

        #endregion

        #region Public Operational Class Methods

        // This is only to be called from AddDerivation.
        private InspectionData(InspectionData parentData, string expression, int indexer)
        {
            this.arrayIndexer = indexer;
            this.parentData = parentData;
            SetInitialExpression(expression);
        }

        public InspectionData(string expression)
        {
            SetInitialExpression(expression);
        }

        public InspectionData(string expression, string value, string type)
        {
            SetInitialExpression(expression);
            runtimeValue = value;
            runtimeType = type;
            IsExpanded = false;
        }

        internal InspectionData AddDerivation(string childExpression, int childIndexer)
        {
            if (null == derivations)
                derivations = new ObservableCollection<InspectionData>();

            InspectionData data = new InspectionData(this, childExpression, childIndexer);
            derivations.Add(data);
            return data;
        }

        internal InspectionData GetDrivationAtIndex(int index)
        {
            if (null == derivations)
                return null;
            if (index < 0 || (index >= derivations.Count))
                return null;

            return derivations[index];
        }

        internal void ClearDerivations()
        {
            this.isExpanded = false;
            if (null == derivations)
                return;

            derivations.Clear();
            derivations = null;
        }

        internal string GetQualifiedName()
        {
            if (null == parentData)
                return this.expression;

            string parentQualifiedName = parentData.GetQualifiedName();
            if (this.arrayIndexer == -1) // This is a regular data member.
                return string.Format("{0}.{1}", parentQualifiedName, this.expression);

            // This guy has got a valid index, so it's an array indexer.
            return string.Format("{0}[{1}]", parentQualifiedName, arrayIndexer);
        }

        #endregion

        #region Public Properties

        public string Expression
        {
            get { return expression; }
            set { SetInitialExpression(value); }
        }

        public string DisplayText
        {
            get
            {
                if (null == parentData)
                    return displayText;
                if (-1 == arrayIndexer)
                    return expression;

                // This is an element of a parent array, show it in display text.
                return string.Format("{0}[{1}]", parentData.DisplayText, arrayIndexer);
            }
        }

        public string Value
        {
            get { return runtimeValue; }
            set { runtimeValue = value; }
        }

        public string Type
        {
            get { return runtimeType; }
            set { runtimeType = value; }
        }

        internal ProtoCore.DSASM.AddressType OpType
        {
            get { return opType; }
            set
            {
                if (opType != value)
                {
                    this.ClearDerivations();
                    opType = value;
                }
            }
        }

        internal int ExpansionLimit
        {
            get { return expansionLimit; }
        }

        internal bool IsEmptyData
        {
            get
            {
                return (string.IsNullOrEmpty(expression) &&
                  string.IsNullOrEmpty(runtimeType) &&
                  string.IsNullOrEmpty(runtimeValue));
            }
        }

        /// <summary>
        /// Collection of sub-variables
        /// @TODO(Ben): Make this read-only property.
        /// </summary>
        public ObservableCollection<InspectionData> Derivations
        {
            get { return derivations; }
            set { derivations = value; }
        }

        /// <summary>
        /// Whether the variable has sub-variables or not
        /// </summary>
        public bool HasItems
        {
            get { return (null != derivations && (derivations.Count > 0)); }
        }

        /// <summary>
        /// If the variable has been expanded by the user, this flag is set
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { isExpanded = value; }
        }

        internal InspectionData ParentData { get { return this.parentData; } }

        #endregion

        #region Private Class Helper Methods

        private void SetInitialExpression(string expression)
        {
            expression = expression.Trim();

            expansionLimit = 0;
            this.expression = expression;
            this.displayText = expression;

            int commaIndex = expression.LastIndexOf(',');
            if (-1 != commaIndex)
            {
                string limitString = expression.Substring(commaIndex + 1);
                if (!string.IsNullOrEmpty(limitString))
                {
                    if (!int.TryParse(limitString, out expansionLimit))
                        expansionLimit = 0;
                }

                this.expression = expression.Substring(0, commaIndex);
            }
        }

        #endregion
    }
}
