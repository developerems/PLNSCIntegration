using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.RQ;
using PX.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using PLNSC;

namespace PX.Objects.RQ
{
    public class RQBiddingVendorExt : PXCacheExtension<PX.Objects.RQ.RQBiddingVendor>
    {
        #region UsrIncoterm
        public abstract class usrIncoterm : IBqlField
        {
        }
        private String _UsrIncoterm;
        [PXDBString(15, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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

        #region UsrLeadTime
        public abstract class usrLeadTime : IBqlField
        {
        }
        protected Int32? _UsrLeadTime;
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Lead Time (Days)")]

        public virtual Int32? UsrLeadTime
        {
            get
            {
                return this._UsrLeadTime;
            }
            set
            {
                this._UsrLeadTime = value;
            }
        }
        #endregion

        #region UsrLeadFrom
        public abstract class usrLeadFrom : IBqlField
        {
        }
        protected Int32? _UsrLeadFrom;
        [PXDBInt]
        [PXDefault(TypeCode.Int32, "2", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Due From")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 },
            new string[] { "Contract Signed Date", "Open LC Date", "Working Permit Start Date", "Prepayment Paid Date", "Drawing Approval Date"})]

        public virtual Int32? UsrLeadFrom
        {
            get
            {
                return this._UsrLeadFrom;
            }
            set
            {
                this._UsrLeadFrom = value;
            }
        }
        #endregion

        #region UsrRefDueDate
        public abstract class usrRefDueDate : IBqlField
        {
        }
        protected DateTime? _UsrRefDueDate;
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)] 
        [PXDBDate]
        [PXUIField(DisplayName = "Ref. Due Date")]

        public virtual DateTime? UsrRefDueDate
        {
            get
            {
                return this._UsrRefDueDate;
            }
            set
            {
                this._UsrRefDueDate = value;
            }
        }
        #endregion

        #region UsrShipFrom
        [PXDBString(50)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ship From")]

        public virtual string UsrShipFrom { get; set; }
        public abstract class usrShipFrom : IBqlField { }
        #endregion

        #region UsrShipTo
        [PXDBString(50)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ship To")]

        public virtual string UsrShipTo { get; set; }
        public abstract class usrShipTo : IBqlField { }
        #endregion

        #region UsrShipVia
        public abstract class usrShipVia : IBqlField
        {
        }
        protected Int32? _UsrShipVia;
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ship Via")]
        [PXIntList(new int[] { 1, 2, 3 },
            new string[] { "Sea", "Air", "Land" })]

        public virtual Int32? UsrShipVia
        {
            get
            {
                return this._UsrShipVia;
            }
            set
            {
                this._UsrShipVia = value;
            }
        }
        #endregion

        #region UsrPaymentTerms
        [PXDBString(10)]
        [PXDefault(typeof(Search<Vendor.termsID, Where2<FeatureInstalled<FeaturesSet.vendorRelations>,
                And<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>,
              Or2<Not<FeatureInstalled<FeaturesSet.vendorRelations>>,
                And<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>>>>),
        PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.vendor>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
        [PXUIField(DisplayName = "Payment Terms", Visibility = PXUIVisibility.Visible)]
        public virtual string UsrPaymentTerms { get; set; }
        public abstract class usrPaymentTerms : IBqlField { }
        #endregion
    }
}