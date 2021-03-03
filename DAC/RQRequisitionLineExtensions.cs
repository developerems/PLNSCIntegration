using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.RQ;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;
using PLNSC;

namespace PX.Objects.RQ
{
    public class RQRequisitionLineExt : PXCacheExtension<PX.Objects.RQ.RQRequisitionLine>
    {
        #region UsrCuryInitialCost
        public abstract class usrCuryInitialCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _UsrCuryInitialCost;
        [PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrInitialCost))]
        [PXUIField(DisplayName = "Initial Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrCuryInitialCost
        {
            get
            {
                return this._UsrCuryInitialCost;
            }
            set
            {
                this._UsrCuryInitialCost = value;
            }
        }
        #endregion
        #region UsrInitialCost
        public abstract class usrInitialCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _UsrInitialCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Initial Unit Cost")]
        public virtual Decimal? UsrInitialCost
        {
            get
            {
                return this._UsrInitialCost;
            }
            set
            {
                this._UsrInitialCost = value;
            }
        }
        #endregion
        #region UsrInitialExtCost
        public abstract class usrInitialExtCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _UsrInitialExtCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Initial Ext. Cost")]
        public virtual Decimal? UsrInitialExtCost
        {
            get
            {
                return this._UsrInitialExtCost;
            }
            set
            {
                this._UsrInitialExtCost = value;
            }
        }
        #endregion
        #region UsrCuryInitialExtCost
        public abstract class usrCuryInitialExtCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _UsrCuryInitialExtCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrInitialExtCost))]
        [PXUIField(DisplayName = "Initial Ext. Cost", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Mult<RQRequisitionLineExt.usrCuryInitialCost, RQRequisitionLine.orderQty>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrCuryInitialExtCost
        {
            get
            {
                return this._UsrCuryInitialExtCost;
            }
            set
            {
                this._UsrCuryInitialExtCost = value;
            }
        }
        #endregion

        #region UsrPatternCost
        public abstract class usrPatternCost : IBqlField
        {
        }
        protected Decimal? _UsrPatternCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrPatternCost
        {
            get
            {
                return this._UsrPatternCost;
            }
            set
            {
                this._UsrPatternCost = value;
            }
        }
        #endregion
        #region UsrCuryPatternCost
        public abstract class usrCuryPatternCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryPatternCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrPatternCost))]
        [PXUIField(DisplayName = "Pattern Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrCuryPatternCost
        {
            get
            {
                return this._UsrCuryPatternCost;
            }
            set
            {
                this._UsrCuryPatternCost = value;
            }
        }
        #endregion

        #region UsrAdditionalCost
        public abstract class usrAdditionalCost : IBqlField
        {
        }
        protected Decimal? _UsrAdditionalCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrAdditionalCost
        {
            get
            {
                return this._UsrAdditionalCost;
            }
            set
            {
                this._UsrAdditionalCost = value;
            }
        }
        #endregion
        #region UsrCuryAdditionalCost
        public abstract class usrCuryAdditionalCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryAdditionalCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrAdditionalCost))]
        [PXUIField(DisplayName = "Additional Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrCuryAdditionalCost
        {
            get
            {
                return this._UsrCuryAdditionalCost;
            }
            set
            {
                this._UsrCuryAdditionalCost = value;
            }
        }
        #endregion

        #region UsrIntermediateCost
        public abstract class usrIntermediateCost : IBqlField
        {
        }
        protected Decimal? _UsrIntermediateCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrIntermediateCost
        {
            get
            {
                return this._UsrIntermediateCost;
            }
            set
            {
                this._UsrIntermediateCost = value;
            }
        }
        #endregion
        #region UsrCuryIntermediateCost
        public abstract class usrCuryIntermediateCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryIntermediateCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrIntermediateCost))]
        [PXUIField(DisplayName = "Intermediate Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Mult<RQRequisitionLine.orderQty, RQRequisitionLine.curyEstUnitCost>),
            typeof(SumCalc<RQRequisitionExt.usrCuryTotalItem>))]
        public virtual Decimal? UsrCuryIntermediateCost
        {
            get
            {
                return this._UsrCuryIntermediateCost;
            }
            set
            {
                this._UsrCuryIntermediateCost = value;
            }
        }
        #endregion

        #region UsrEstShipCost
        public abstract class usrEstShipCost : IBqlField
        {
        }
        protected Decimal? _UsrEstShipCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrEstShipCost
        {
            get
            {
                return this._UsrEstShipCost;
            }
            set
            {
                this._UsrEstShipCost = value;
            }
        }
        #endregion
        #region UsrCuryEstShipCost
        public abstract class usrCuryEstShipCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryEstShipCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrEstShipCost))]
        [PXUIField(DisplayName = "Est. Shipment Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<RQRequisitionLineExt.usrEstShipRate, RQRequisitionLineExt.usrCuryIntermediateCost>, decimal100>),
            typeof(SumCalc<RQRequisitionExt.usrCuryTotalShipCost>))]
        public virtual Decimal? UsrCuryEstShipCost
        {
            get
            {
                return this._UsrCuryEstShipCost;
            }
            set
            {
                this._UsrCuryEstShipCost = value;
            }
        }
        #endregion

        #region UsrROKCost
        public abstract class usrROKCost : IBqlField
        {
        }
        protected Decimal? _UsrROKCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrROKCost
        {
            get
            {
                return this._UsrROKCost;
            }
            set
            {
                this._UsrROKCost = value;
            }
        }
        #endregion
        #region UsrCuryROKCost
        public abstract class usrCuryROKCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryROKCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrROKCost))]
        [PXUIField(DisplayName = "R.O.K Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<RQRequisitionLineExt.usrROK, Add<RQRequisitionLineExt.usrCuryIntermediateCost, Add<RQRequisitionLineExt.usrCuryAdditionalCost, RQRequisitionLineExt.usrCuryEstShipCost>>>, decimal100>),
            typeof(SumCalc<RQRequisitionExt.usrCuryTotalROK>))]
        public virtual Decimal? UsrCuryROKCost
        {
            get
            {
                return this._UsrCuryROKCost;
            }
            set
            {
                this._UsrCuryROKCost = value;
            }
        }
        #endregion

        #region UsrTotalAll
        public abstract class usrTotalAll : IBqlField
        {
        }
        protected Decimal? _UsrTotalAll;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrTotalAll
        {
            get
            {
                return this._UsrTotalAll;
            }
            set
            {
                this._UsrTotalAll = value;
            }
        }
        #endregion
        #region UsrCuryTotalAll
        public abstract class usrCuryTotalAll : IBqlField
        {
        }
        protected Decimal? _UsrCuryTotalAll;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrTotalAll))]
        [PXUIField(DisplayName = "Line Grand Total", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Add<RQRequisitionLineExt.usrCuryIntermediateCost, Add<RQRequisitionLineExt.usrCuryEstShipCost, Add<RQRequisitionLineExt.usrAdditionalCost, RQRequisitionLineExt.usrCuryROKCost>>>),
            typeof(SumCalc<RQRequisitionExt.usrCuryGrandTotal>))]
        public virtual Decimal? UsrCuryTotalAll
        {
            get
            {
                return this._UsrCuryTotalAll;
            }
            set
            {
                this._UsrCuryTotalAll = value;
            }
        }
        #endregion

        #region CuryEstExtCost
        public abstract class curyEstExtCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryEstExtCost;
        [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLine.estExtCost))]
        [PXUIField(DisplayName = "Est. Ext. Cost", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Add<RQRequisitionLineExt.usrCuryIntermediateCost, Add<RQRequisitionLineExt.usrCuryEstShipCost, Add<RQRequisitionLineExt.usrCuryAdditionalCost, RQRequisitionLineExt.usrCuryROKCost>>>),
            typeof(SumCalc<RQRequisition.curyEstExtCostTotal>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryEstExtCost
        {
            get
            {
                return this._CuryEstExtCost;
            }
            set
            {
                this._CuryEstExtCost = value;
            }
        }
        #endregion

        #region UsrEstShipRate
        public abstract class usrEstShipRate : IBqlField
        {
        }
        protected Decimal? _UsrEstShipRate;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Est. Shipping Rate, %")]
        public virtual Decimal? UsrEstShipRate
        {
            get
            {
                return this._UsrEstShipRate;
            }
            set
            {
                this._UsrEstShipRate = value;
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
        [PXUIField(DisplayName = "R.O.K Rate , %", IsReadOnly = true, Enabled = true)]

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

        #region UsrReqLineLeadTime
        [PXDBInt]
        [PXUIField(DisplayName = "Lead Time (Days)")]

        public virtual int? UsrReqLineLeadTime { get; set; }
        public abstract class usrReqLineLeadTime : IBqlField { }
        #endregion

        #region PreventUpdate
        public abstract class preventUpdate : PX.Data.IBqlField
        {
        }
        [PXBool]
        public virtual Boolean? PreventUpdate { get; set; }
        #endregion

        #region UsrIntegrated
        [PXDBBool]
        [PXUIField(DisplayName = "Integrated")]

        public virtual bool? UsrIntegrated { get; set; }
        public abstract class usrIntegrated : IBqlField { }
        #endregion

        #region UsrEllMessage
        [PXDBString(40)]
        [PXUIField(DisplayName = "Ellipse Message")]

        public virtual string UsrEllMessage { get; set; }
        public abstract class usrEllMessage : IBqlField { }
        #endregion
    }

    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class RQ_RQRequisitionLine_ExistingColumn : PXCacheExtension<PX.Objects.RQ.RQRequisitionLine>
    {
        #region InventoryID  
        [PXMergeAttributes(Method = MergeMethod.Append)]

        [PXCustomizeSelectorColumns(
        typeof(PX.Objects.IN.InventoryItem.inventoryCD),
        typeof(PX.Objects.IN.InventoryItem.descr),
        typeof(PX.Objects.IN.InventoryItem.itemClassID),
        typeof(PX.Objects.IN.InventoryItem.itemStatus),
        typeof(PX.Objects.IN.InventoryItem.itemType),
        typeof(PX.Objects.IN.InventoryItem.baseUnit),
        typeof(PX.Objects.IN.InventoryItem.salesUnit),
        typeof(PX.Objects.IN.InventoryItem.purchaseUnit),
        typeof(PX.Objects.IN.InventoryItem.basePrice),
        typeof(PX.Objects.IN.InventoryItemExt.usrAlternateIDs))]
        public int? InventoryID { get; set; }
        #endregion
    }
}