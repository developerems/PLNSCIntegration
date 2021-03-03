using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System.Collections;
using System;

namespace PX.Objects.SO
{
    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class SO_SOLine_ExistingColumn : PXCacheExtension<PX.Objects.SO.SOLine>
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

    public class SOLineExt : PXCacheExtension<PX.Objects.SO.SOLine>
    {
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
        [PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLineExt.usrAdditionalCost))]
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
        [PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLineExt.usrIntermediateCost))]
        [PXUIField(DisplayName = "Intermediate Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Mult<SOLine.orderQty, SOLine.curyUnitCost>),
            typeof(SumCalc<SOOrderExt.usrCuryTotalItem>))]
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
        [PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLineExt.usrEstShipCost))]
        [PXUIField(DisplayName = "Est. Shipment Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<SOLineExt.usrEstShipRate, SOLineExt.usrCuryIntermediateCost>, decimal100>),
            typeof(SumCalc<SOOrderExt.usrCuryTotalShipCost>))]
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
        [PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLineExt.usrROKCost))]
        [PXUIField(DisplayName = "R.O.K Cost", IsReadOnly = true, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<SOLineExt.usrROK, Add<SOLineExt.usrCuryIntermediateCost, Add<SOLineExt.usrCuryAdditionalCost, SOLineExt.usrCuryEstShipCost>>>, decimal100>),
            typeof(SumCalc<SOOrderExt.usrCuryTotalROK>))]
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

        #region UsrEstShipRate
        public abstract class usrEstShipRate : IBqlField
        {
        }
        protected Decimal? _UsrEstShipRate;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Est. Shipping Rate")]
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
        [PXUIField(DisplayName = "R.O.K Rate", IsReadOnly = true, Enabled = true)]

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

        #region UsrReqNbr
        [PXDBString(15)]
        [PXUIField(DisplayName = "Requisition Nbr")]

        public virtual string UsrReqNbr { get; set; }
        public abstract class usrReqNbr : IBqlField { }
        #endregion
        #region UsrReqLineNbr
        [PXDBInt]
        [PXUIField(DisplayName = "Requisition Line Nbr")]

        public virtual int? UsrReqLineNbr { get; set; }
        public abstract class usrReqLineNbr : IBqlField { }
        #endregion
    }
}