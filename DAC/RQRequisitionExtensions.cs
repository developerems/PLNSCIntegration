using PX.Data;
using System;
using PX.Objects.CM;
using PX.Objects.SO;
using PLNSC;

namespace PX.Objects.RQ
{
    public class RQRequisitionExt : PXCacheExtension<PX.Objects.RQ.RQRequisition>
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
        #region UsrClarificationDate
        public abstract class usrClarificationDate : IBqlField
        {
        }
        private DateTime? _UsrClarificationDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Clarification Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? UsrClarificationDate
        {
            get
            {
                return this._UsrClarificationDate;
            }
            set
            {
                this._UsrClarificationDate = value;
            }
        }
        #endregion
        #region UsrPreDONbr
        public abstract class usrPreDoNbr : IBqlField
        {
        }
        protected String _UsrPreDoNbr;
        [PXDBString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region UsrRKSNbr
        public abstract class usrRKSNbr : IBqlField
        {
        }
        protected String _UsrRKSNbr;
        [PXDBString(30)]
        [PXUIField(DisplayName = "RKS Nbr")]

        public virtual string UsrRKSNbr
        {
            get
            {
                return this._UsrRKSNbr;
            }
            set
            {
                this._UsrRKSNbr = value;
            }
        }
        #endregion

        #region UsrROK
        public abstract class usrROK : IBqlField
        {
        }
        protected Decimal? _UsrROK;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "R.O.K Rate, %")]

        public virtual Decimal? UsrROK
        {
            get
            {
                return this._UsrROK;
            }
            set
            {
                this._UsrROK = value;
            }
        }
        #endregion
        #region UsrGoodsShipRate
        public abstract class usrGoodsShipRate : IBqlField
        {
        }
        protected Decimal? _UsrGoodsShipRate;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Goods Shipment Rate, %")]

        public virtual Decimal? UsrGoodsShipRate
        {
            get
            {
                return this._UsrGoodsShipRate;
            }
            set
            {
                this._UsrGoodsShipRate = value;
            }
        }
        #endregion
        #region UsrServShipRate
        public abstract class usrServShipRate : IBqlField
        {
        }
        protected Decimal? _UsrServShipRate;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Service Shipment Rate, %")]

        public virtual Decimal? UsrServShipRate
        {
            get
            {
                return this._UsrServShipRate;
            }
            set
            {
                this._UsrServShipRate = value;
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
        [PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrInspectorCost))]
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
        [PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrTotalItem))]
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
        [PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrTotalShipCost))]
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
        [PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrTotalROK))]
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

        #region UsrGrandTotal
        public abstract class usrGrandTotal : IBqlField
        {
        }
        protected Decimal? _UsrGrandTotal;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]

        public virtual Decimal? UsrGrandTotal
        {
            get
            {
                return this._UsrGrandTotal;
            }
            set
            {
                this._UsrGrandTotal = value;
            }
        }
        #endregion
        #region UsrCuryGrandTotal
        public abstract class usrCuryGrandTotal : IBqlField
        {
        }
        protected Decimal? _UsrCuryGrandTotal;
        [PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrGrandTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Grand Total", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual Decimal? UsrCuryGrandTotal
        {
            get
            {
                return this._UsrCuryGrandTotal;
            }
            set
            {
                this._UsrCuryGrandTotal = value;
            }
        }
        #endregion

        #region UsrDONbr
        public abstract class usrDONbr : IBqlField
        {
        }
        protected String _UsrDONbr;
        [PXDBString(15, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "DO Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<SOOrderTypeConstants.salesOrder>>>), typeof(SOOrder.orderNbr), typeof(SOOrder.orderDesc),
            DescriptionField = typeof(SOOrder.orderDesc))]
        public virtual string UsrDONbr
        {
            get
            {
                return this._UsrDONbr;
            }
            set
            {
                this._UsrDONbr = value;
            }
        }
        #endregion

        #region UsrCustOrderNbr
        [PXDBString(30)]
        [PXUIField(DisplayName = "Customer Order Nbr.")]

        public virtual string UsrCustOrderNbr { get; set; }
        public abstract class usrCustOrderNbr : IBqlField { }
        #endregion

        #region UsrPurchMethod
        public abstract class usrPurchMethod : IBqlField
        {
        }
        protected int? _UsrPurchMethod;
        [PXDBInt]
        [PXUIField(DisplayName = "Purchasing Method", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(1)]
        [PXIntList(new int[] { 0, 1, 2 },
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

    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class RQ_RQRequisition_ExistingColumn : PXCacheExtension<PX.Objects.RQ.RQRequisition>
    {
        #region ReqNbr  
        [PXMergeAttributes(Method = MergeMethod.Append)]

        [PXCustomizeSelectorColumns(
        typeof(PX.Objects.RQ.RQRequisition.reqNbr),
        typeof(PX.Objects.RQ.RQRequisition.description),
        typeof(PX.Objects.RQ.RQRequisition.status),
        typeof(PX.Objects.RQ.RQRequisition.employeeID),
        typeof(PX.Objects.RQ.RQRequisition.vendorID))]
        public string ReqNbr { get; set; }
        #endregion
    }

}