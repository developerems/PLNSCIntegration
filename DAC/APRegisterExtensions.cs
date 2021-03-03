using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using CRLocation = PX.Objects.CR.Standalone.Location;
using IRegister = PX.Objects.CM.IRegister;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Abstractions;
using PX.Objects.Common.MigrationMode;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects;
using PX.TM;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace PX.Objects.AP
{
    public class APRegisterExt : PXCacheExtension<PX.Objects.AP.APRegister>
    {
        #region UsrLCType
        public abstract class usrLCType : IBqlField
        {
        }
        protected int? _UsrLCType;
        [PXDBInt]
        [PXUIField(DisplayName = "LC Type")]
        [PXIntList(new int[] { 1, 2, 3 },
            new string[] { "At Site", "Usance", "UPAS" })]
        public virtual int? UsrLCType
        {
            get { return this._UsrLCType; }
            set { this._UsrLCType = value; }
        }
        #endregion
        #region UsrNCLNbr
        [PXDBString(30)]
        [PXUIField(DisplayName = "NCL Agreement Nbr.")]

        public virtual string UsrNCLNbr { get; set; }
        public abstract class usrNCLNbr : IBqlField { }
        #endregion
        #region UsrPaySource
        public abstract class usrPaySource : IBqlField
        {
        }
        protected int? _UsrPaySource;
        [PXDBInt]
        [PXUIField(DisplayName = "Payment Source")]
        [PXIntList(new int[] { 1, 2},
            new string[] { "Non-Cash Loan", "Cash Collateral"})]
        public virtual int? UsrPaySource
        {
            get { return this._UsrPaySource; }
            set { this._UsrPaySource = value; }
        }
        #endregion
        #region UsrLCNbr
        [PXDBString(30)]
        [PXUIField(DisplayName = "LC Nbr.")]

        public virtual string UsrLCNbr { get; set; }
        public abstract class usrLCNbr : IBqlField { }
        #endregion
        #region UsrLCDate
        [PXDBDate]
        [PXUIField(DisplayName = "LC Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrLCDate { get; set; }
        public abstract class usrLCDate : IBqlField { }
        #endregion
        #region UsrOpenLCDate
        [PXDBDate]
        [PXUIField(DisplayName = "Est. Open LC Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrOpenLCDate { get; set; }
        public abstract class usrOpenLCDate : IBqlField { }
        #endregion
        #region UsrLCExpiredDate
        [PXDBDate]
        [PXUIField(DisplayName = "LC Expired Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrLCExpiredDate { get; set; }
        public abstract class usrLCExpiredDate : IBqlField { }
        #endregion
    }
}