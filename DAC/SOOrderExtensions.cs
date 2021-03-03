using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using PLNSC;

namespace PX.Objects.SO
{
    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class SO_SOOrder_ExistingColumn : PXCacheExtension<PX.Objects.SO.SOOrder>
    {
        #region OrderNbr  
        [PXMergeAttributes(Method = MergeMethod.Append)]

        [PXCustomizeSelectorColumns(
        typeof(PX.Objects.SO.SOOrder.orderNbr),
        typeof(PX.Objects.SO.SOOrder.orderDesc),
        typeof(PX.Objects.SO.SOOrder.customerOrderNbr),
        typeof(PX.Objects.SO.SOOrder.orderDate),
        typeof(PX.Objects.SO.SOOrder.customerID),
        typeof(PX.Objects.SO.SOOrder.customerID_Customer_acctName),
        typeof(PX.Objects.SO.SOOrder.customerLocationID),
        typeof(PX.Objects.SO.SOOrder.curyID),
        typeof(PX.Objects.SO.SOOrder.curyOrderTotal),
        typeof(PX.Objects.SO.SOOrder.status),
        typeof(PX.Objects.SO.SOOrder.invoiceNbr))]
        public string OrderNbr { get; set; }
        #endregion
    }

    public class SOOrderExt : PXCacheExtension<PX.Objects.SO.SOOrder>
    {
        #region UsrIncoterm
        public abstract class usrIncoterm : IBqlField
        {
        }
        private String _UsrIncoterm;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Incoterm", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<SCIncoterm.incotermID>), DescriptionField = typeof(SCIncoterm.description), CacheGlobal = true)]
        public virtual string UsrIncoterm
        {
            get
            {
                return this._UsrIncoterm;
            }
            set
            {
                this._UsrIncoterm = value;
            }
        }
        #endregion
        #region UsrPreDONbr
        public abstract class usrPreDoNbr : IBqlField
        {
        }
        protected String _UsrPreDoNbr;
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Pre DO Nbr")]
        public virtual string UsrPreDoNbr
        {
            get { return this._UsrPreDoNbr; }
            set { this._UsrPreDoNbr = value; }
        }
        #endregion
        #region UsrPreDODate
        public abstract class usrPreDoDate : IBqlField
        {
        }
        protected DateTime? _UsrPreDoDate;
        [PXDBDate()]
        //[PXDefault(typeof(AccessInfo.businessDate))]
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

        #region UsrInspectorCost
        public abstract class usrInspectorCost : IBqlField
        {
        }
        protected Decimal? _UsrInspectorCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]

        public virtual Decimal? UsrInspectorCost
        {
            get
            {
                return this._UsrInspectorCost;
            }
            set
            {
                this._UsrInspectorCost = value;
            }
        }
        #endregion
        #region UsrCuryInspectorCost
        public abstract class usrCuryInspectorCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryInspectorCost;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrderExt.usrInspectorCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Inspector Cost", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual Decimal? UsrCuryInspectorCost
        {
            get
            {
                return this._UsrCuryInspectorCost;
            }
            set
            {
                this._UsrCuryInspectorCost = value;
            }
        }
        #endregion

        #region UsrTotalItem
        public abstract class usrTotalItem : IBqlField
        {
        }
        protected Decimal? _UsrTotalItem;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]

        public virtual Decimal? UsrTotalItem
        {
            get
            {
                return this._UsrTotalItem;
            }
            set
            {
                this._UsrTotalItem = value;
            }
        }
        #endregion
        #region UsrCuryTotalItem
        public abstract class usrCuryTotalItem : IBqlField
        {
        }
        protected Decimal? _UsrCuryTotalItem;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrderExt.usrTotalItem))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Item Ext. Cost", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual Decimal? UsrCuryTotalItem
        {
            get
            {
                return this._UsrCuryTotalItem;
            }
            set
            {
                this._UsrCuryTotalItem = value;
            }
        }
        #endregion

        #region UsrTotalShipCost
        public abstract class usrTotalShipCost : IBqlField
        {
        }
        protected Decimal? _UsrTotalShipCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]

        public virtual Decimal? UsrTotalShipCost
        {
            get
            {
                return this._UsrTotalShipCost;
            }
            set
            {
                this._UsrTotalShipCost = value;
            }
        }
        #endregion
        #region UsrCuryTotalShipCost
        public abstract class usrCuryTotalShipCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryTotalShipCost;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrderExt.usrTotalShipCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Shipment Cost", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual Decimal? UsrCuryTotalShipCost
        {
            get
            {
                return this._UsrCuryTotalShipCost;
            }
            set
            {
                this._UsrCuryTotalShipCost = value;
            }
        }
        #endregion

        #region UsrTotalROK
        public abstract class usrTotalROK : IBqlField
        {
        }
        protected Decimal? _UsrTotalROK;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]

        public virtual Decimal? UsrTotalROK
        {
            get
            {
                return this._UsrTotalROK;
            }
            set
            {
                this._UsrTotalROK = value;
            }
        }
        #endregion
        #region UsrCuryTotalROK
        public abstract class usrCuryTotalROK : IBqlField
        {
        }
        protected Decimal? _UsrCuryTotalROK;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrderExt.usrTotalROK))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total R.O.K Cost", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual Decimal? UsrCuryTotalROK
        {
            get
            {
                return this._UsrCuryTotalROK;
            }
            set
            {
                this._UsrCuryTotalROK = value;
            }
        }
        #endregion

        #region UsrPurchMethod
        public abstract class usrPurchMethod : IBqlField
        {
        }
        protected int? _UsrPurchMethod;
        [PXDBInt]
        [PXUIField(DisplayName = "Purchasing Method", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(1)]
        [PXIntList(new int[] { 0, 1, 3 },
            new string[] { "B2B", "ROK", "GA" })]
        public virtual int? UsrPurchMethod
        {
            get
            {
                return this._UsrPurchMethod;
            }
            set
            {
                this._UsrPurchMethod = value;
            }
        }
        #endregion
    }
}