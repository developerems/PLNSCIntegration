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
    public class RQRequisitionLineBiddingExt : PXCacheExtension<PX.Objects.RQ.RQRequisitionLineBidding>
    {
        #region UsrInitialCost
        public abstract class usrInitialCost : IBqlField
        {
        }
        protected Decimal? _UsrInitialCost;
        [PXBaseCury()]
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
        [PXCurrency(typeof(RQRequisitionLineBidding.curyInfoID), typeof(RQRequisitionLineBiddingExt.usrInitialCost))]
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
        public abstract class usrInitialExtCost : PX.Data.IBqlField
        {
        }
        protected Decimal? _UsrInitialExtCost;
        [PXBaseCury()]
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
        [PXCurrency(typeof(RQRequisitionLineBidding.curyInfoID), typeof(RQRequisitionLineBiddingExt.usrInitialExtCost))]
        [PXUIField(DisplayName = "Initial Ext. Cost", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Mult<RQRequisitionLineBiddingExt.usrCuryInitialCost, RQRequisitionLineBidding.orderQty>))]
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
    }
}