using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.RQ;
using PX.Objects.SM;
using PX.Objects;
using PX.SM;
using PX.TM;
using System.Collections.Generic;
using System;
using PLNSC;

namespace PX.Objects.RQ
{
    public class RQRequestExt : PXCacheExtension<PX.Objects.RQ.RQRequest>
    {
        #region UsrIncoTerms
        public abstract class usrIncoTerms : IBqlField { }
        private String _UsrIncoTerms;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName="Incoterm", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<SCIncoterm.incotermID>), DescriptionField = typeof(SCIncoterm.description), CacheGlobal = true)]
        public virtual string UsrIncoTerms
        {
            get
            {
                return this._UsrIncoTerms;
            }
            set
            {
                this._UsrIncoTerms = value;
            }
        }
        #endregion
        #region UsrPreDoNbr
        public abstract class usrPreDoNbr : IBqlField
        {
        }
        protected String _UsrPreDoNbr;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Pre DO Nbr")]
        public virtual string UsrPreDoNbr
        {
            get { return this._UsrPreDoNbr; }
            set { this._UsrPreDoNbr = value; }
        }
        #endregion
        #region UsrPreDoDate
        public abstract class usrPreDoDate : IBqlField
        {
        }
        protected DateTime? _UsrPreDoDate;
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Pre DO Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrPreDoDate
        {
            get
            {
                return this._UsrPreDoDate;
            }
            set
            {
                this._UsrPreDoDate = value;
            }
        }
        #endregion
    }

    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class RQ_RQRequest_ExistingColumn : PXCacheExtension<PX.Objects.RQ.RQRequest>
    {
        #region OrderNbr  
        [PXMergeAttributes(Method = MergeMethod.Append)]

        [PXCustomizeSelectorColumns(
        typeof(PX.Objects.RQ.RQRequest.orderNbr),
        typeof(PX.Objects.RQ.RQRequest.description),
        typeof(PX.Objects.RQ.RQRequest.orderDate),
        typeof(PX.Objects.RQ.RQRequest.status),
        typeof(PX.Objects.RQ.RQRequest.employeeID),
        typeof(PX.Objects.RQ.RQRequest.departmentID))]
        public string OrderNbr { get; set; }
        #endregion
  }
}