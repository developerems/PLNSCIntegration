using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.RQ;
using PX.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace PX.Objects.RQ
{
    public class RQBiddingExt : PXCacheExtension<PX.Objects.RQ.RQBidding>
    {
        #region UsrPatternCost
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrPatternCost { get; set; }
        public abstract class usrPatternCost : IBqlField { }
        #endregion

        #region UsrCuryPatternCost
        [PXUIField(DisplayName = "Pattern Cost")]
        [PXDBCurrency(typeof(RQBidding.curyInfoID), typeof(RQBiddingExt.usrPatternCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrCuryPatternCost { get; set; }
        public abstract class usrCuryPatternCost : IBqlField { }
        #endregion

        #region CuryQuoteExtCost
        public abstract class curyQuoteExtCost : PX.Data.IBqlField
        {
        }
        [PXDBCurrency(typeof(RQBidding.curyInfoID), typeof(RQBidding.quoteExtCost))]
        [PXUIField(DisplayName = "Bid Extended Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        //[PXFormula(typeof(Mult<RQBidding.quoteQty, Add<RQBidding.curyQuoteUnitCost, RQBiddingExt.usrCuryPatternCost>>))]
        [PXFormula(typeof(Mult<RQBidding.quoteQty, RQBidding.curyQuoteUnitCost>), typeof(SumCalc<RQBiddingVendor.curyTotalQuoteExtCost>))]
        //[PXFormula(null, typeof(SumCalc<RQBiddingVendor.curyTotalQuoteExtCost>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryQuoteExtCost { get; set; }
        #endregion

        #region UsrInitialCost
        public abstract class usrInitialCost : IBqlField
        {
        }
        protected Decimal? _UsrInitialCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
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

        #region UsrCuryInitialCost
        public abstract class usrCuryInitialCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryInitialCost;
        [PXDBCurrency(typeof(RQBidding.curyInfoID), typeof(RQBiddingExt.usrInitialCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Initial Cost")]
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

        #region UsrInitialExtCost
        public abstract class usrInitialExtCost : IBqlField
        {
        }
        protected Decimal? _UsrInitialExtCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrInitialExtCost
        {
            get { return this._UsrInitialExtCost; }
            set { this._UsrInitialExtCost = value; }
        }
        #endregion

        #region UsrCuryInitialExtCost
        public abstract class usrCuryInitialExtCost : IBqlField
        {
        }
        protected Decimal? _UsrCuryInitialExtCost;
        [PXDBCurrency(typeof(RQBidding.curyInfoID), typeof(RQBiddingExt.usrInitialExtCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Initial Ext. Cost")]

        public virtual Decimal? UsrCuryInitialExtCost
        {
            get { return this._UsrCuryInitialExtCost; }
            set { this._UsrCuryInitialExtCost = value; }
        }
        #endregion

    }
}