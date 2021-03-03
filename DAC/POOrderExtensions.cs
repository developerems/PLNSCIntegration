using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects;
using PX.SM;
using PX.TM;
using System.Collections.Generic;
using System;
using PLNSC;

namespace PX.Objects.PO
{
    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class PO_POOrder_ExistingColumn : PXCacheExtension<PX.Objects.PO.POOrder>
    {
        #region OrderNbr  
        [PXMergeAttributes(Method = MergeMethod.Append)]

        [PXCustomizeSelectorColumns(
        typeof(PX.Objects.PO.POOrder.orderType),
        typeof(PX.Objects.PO.POOrder.orderNbr),
        typeof(PX.Objects.PO.POOrder.orderDesc),
        typeof(PX.Objects.PO.POOrder.vendorRefNbr),
        typeof(PX.Objects.PO.POOrder.orderDate),
        typeof(PX.Objects.PO.POOrder.status),
        typeof(PX.Objects.PO.POOrder.vendorID),
        typeof(PX.Objects.PO.POOrder.vendorID_Vendor_acctName),
        typeof(PX.Objects.PO.POOrder.vendorLocationID),
        typeof(PX.Objects.PO.POOrder.curyID),
        typeof(PX.Objects.PO.POOrder.curyOrderTotal),
        typeof(PX.Objects.PO.POOrder.sOOrderType),
        typeof(PX.Objects.PO.POOrder.sOOrderNbr))]
        public string OrderNbr { get; set; }
        #endregion
    }

    public class POOrderExt : PXCacheExtension<PX.Objects.PO.POOrder>
    {
        #region UsrRevisionNbr
        public abstract class usrRevisionNbr : IBqlField
        {
        }
        protected String _UsrRevisionNbr;
        [PXDBString(30)]
        [PXUIField(DisplayName = "Addendum / Novasi")]
        public virtual string UsrRevisionNbr
        {
            get { return this._UsrRevisionNbr; }
            set { this._UsrRevisionNbr = value; }
        }
        #endregion
        #region UsrRevisionDate
        public abstract class usrRevisionDate : IBqlField
        {
        }
        protected DateTime? _UsrRevisionDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Addendum / Novasi Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? UsrRevisionDate
        {
            get { return this._UsrRevisionDate; }
            set { this._UsrRevisionDate = value; }
        }
        #endregion

        #region UsrAdvisingBank
        [PXDBString(30)]
        [PXUIField(DisplayName = "Advising Bank")]

        public virtual string UsrAdvisingBank { get; set; }
        public abstract class usrAdvisingBank : IBqlField { }
        #endregion
        #region UsrAdvBankBranch
        [PXDBString(30)]
        [PXUIField(DisplayName = "Branch Code")]

        public virtual string UsrAdvBankBranch { get; set; }
        public abstract class usrAdvBankBranch : IBqlField { }
        #endregion
        #region UsrAdvBankAddress
        [PXDBString(255)]
        [PXUIField(DisplayName = "Bank Address")]

        public virtual string UsrAdvBankAddress { get; set; }
        public abstract class usrAdvBankAddress : IBqlField { }
        #endregion
        #region UsrAdvSWIFT
        [PXDBString(11)]
        [PXUIField(DisplayName = "SWIFT Code")]

        public virtual string UsrAdvSWIFT { get; set; }
        public abstract class usrAdvSWIFT : IBqlField { }
        #endregion
        #region UsrAdvAccountNbr
        [PXDBString(30)]
        [PXUIField(DisplayName = "Bank Account Nbr")]

        public virtual string UsrAdvAccountNbr { get; set; }
        public abstract class usrAdvAccountNbr : IBqlField { }
        #endregion

        #region UsrEstOpenLCDate
        [PXDBDate]
        [PXUIField(DisplayName = "Est. Open LC Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrEstOpenLCDate { get; set; }
        public abstract class usrEstOpenLCDate : IBqlField { }
        #endregion
        #region UsrOpenLCDate
        [PXDBDate]
        [PXUIField(DisplayName = "Open LC Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrOpenLCDate { get; set; }
        public abstract class usrOpenLCDate : IBqlField { }
        #endregion

        #region UsrLCType
        public abstract class usrLCType : IBqlField
        {
        }
        protected int? _UsrLCType;
        [PXDBInt]
        [PXUIField(DisplayName = "LC Type")]
        [PXIntList(new int[] { 1, 2, 3 },
            new string[] { "At Sight", "Usance", "UPAS" })]

        public virtual int? UsrLCType
        {
            get { return this._UsrLCType; }
            set { this._UsrLCType = value; }
        }
        #endregion

        #region UsrStartingPoint
        public abstract class usrStartingPoint : IBqlField
        {
        }
        protected int? _UsrStartingPoint;
        [PXDBInt]
        [PXUIField(DisplayName = "StartingPoint")]
        [PXIntList(new int[] { 1, 2, 3 },
            new string[] { "Bill Of Lading", "Forwarder Cargo Receipt", "Airway Bill" })]

        public virtual int? UsrStartingPoint
        {
            get { return this._UsrStartingPoint; }
            set { this._UsrStartingPoint = value; }
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
        #region UsrLeadTime
        [PXDBInt]
        [PXUIField(DisplayName = "Lead Time")]

        public virtual int? UsrLeadTime { get; set; }
        public abstract class usrLeadTime : IBqlField { }
        #endregion
        #region UsrLCExpiredDate
        [PXDBDate]
        [PXUIField(DisplayName = "LC Expired Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrLCExpiredDate { get; set; }
        public abstract class usrLCExpiredDate : IBqlField { }
        #endregion
        #region UsrDocPresentation
        [PXDBInt]
        [PXUIField(DisplayName = "Document Presentation Duration")]

        public virtual int? UsrDocPresentation { get; set; }
        public abstract class usrDocPresentation : IBqlField { }
        #endregion

        #region UsrItemCondition
        [PXDBInt]
        [PXUIField(DisplayName = "Item Condition")]
        [PXIntList(new int[] { 1, 2 },
            new string[] { "New", "Refurbished" })]

        public virtual int? UsrItemCondition { get; set; }
        public abstract class usrItemCondition : IBqlField { }
        #endregion
        #region UsrIncoterm
        [PXDBString(15)]
        [PXUIField(DisplayName = "Incoterm", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<SCIncoterm.incotermID>), DescriptionField = typeof(SCIncoterm.description), CacheGlobal = true)]

        public virtual string UsrIncoterm { get; set; }
        public abstract class usrIncoterm : IBqlField { }
        #endregion
        #region UsrPartialShipment
        [PXDBInt]
        [PXUIField(DisplayName = "Partial Shipment")]
        [PXIntList(new int[] { 0, 1 },
            new string[] { "Allowed", "Not Allowed" })]

        public virtual int? UsrPartialShipment { get; set; }
        public abstract class usrPartialShipment : IBqlField { }
        #endregion
        #region UsrTranshipment
        [PXDBInt]
        [PXUIField(DisplayName = "Transhipment")]
        [PXIntList(new int[] { 0, 1 },
            new string[] { "Allowed", "Not Allowed" })]

        public virtual int? UsrTranshipment { get; set; }
        public abstract class usrTranshipment : IBqlField { }
        #endregion
        #region UsrEstShipmentDate
        [PXDBDate]
        [PXUIField(DisplayName = "Est. Shipment Date", Visibility = PXUIVisibility.SelectorVisible)]

        public virtual DateTime? UsrEstShipmentDate { get; set; }
        public abstract class usrEstShipmentDate : IBqlField { }
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
        #region UsrLeadFrom
        public abstract class usrLeadFrom : IBqlField
        {
        }
        protected Int32? _UsrLeadFrom;
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Due From")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 },
            new string[] { "Contract Signed Date", "Open LC Date", "Working Permit Start Date", "Prepayment Paid Date", "Drawing Approval Date" })]

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

        #region UsrLastShipmentDate
        [PXDBDate]
        [PXUIField(DisplayName = "Last Shipment Date")]

        public virtual DateTime? UsrLastShipmentDate { get; set; }
        public abstract class usrLastShipmentDate : IBqlField { }
        #endregion

        #region UsrShipFrom
        [PXDBString(50)]
        [PXUIField(DisplayName = "Ship From")]

        public virtual string UsrShipFrom { get; set; }
        public abstract class usrShipFrom : IBqlField { }
        #endregion

        #region UsrShipTo
        [PXDBString(50)]
        [PXUIField(DisplayName = "Ship To")]

        public virtual string UsrShipTo { get; set; }
        public abstract class usrShipTo : IBqlField { }
        #endregion

        #region UsrCountryOfOrigin
        [PXDBString(50)]
        [PXUIField(DisplayName = "Country Of Origin")]

        public virtual string UsrCountryOfOrigin { get; set; }
        public abstract class usrCountryOfOrigin : IBqlField { }
        #endregion

        #region UsrRatePenalty
        [PXDBString(30)]
        [PXUIField(DisplayName = "Rate Penalty")]

        public virtual string UsrRatePenalty { get; set; }
        public abstract class usrRatePenalty : IBqlField { }
        #endregion

        #region UsrMaxPenalty
        [PXDBString(30)]
        [PXUIField(DisplayName = "Max Penalty")]

        public virtual string UsrMaxPenalty { get; set; }
        public abstract class usrMaxPenalty : IBqlField { }
        #endregion

        #region UsrPenaltyRule
        public abstract class usrPenaltyRule : IBqlField
        {
        }
        protected Int32? _UsrPenaltyRule;
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Penalty Rule")]
        [PXIntList(new int[] { 1, 2},
            new string[] { "Nilai Total Kontrak", "Proporsional"})]

        public virtual Int32? UsrPenaltyRule
        {
            get
            {
                return this._UsrPenaltyRule;
            }
            set
            {
                this._UsrPenaltyRule = value;
            }
        }
        #endregion
        #region UsrVAT
        [PXDBDecimal]
        [PXUIField(DisplayName = "VAT(%)")]

        public virtual Decimal? UsrVAT { get; set; }
        public abstract class usrVAT : IBqlField { }
        #endregion
        #region UsrLCPercentage
        [PXDBDecimal]
        [PXUIField(DisplayName = "LC Percentage (%)")]

        public virtual Decimal? UsrLCPercentage { get; set; }
        public abstract class usrLCPercentage : IBqlField { }
        #endregion
        #region UsrLCValue
        [PXDBDecimal]
        [PXUIField(DisplayName = "LC Value")]

        public virtual Decimal? UsrLCValue { get; set; }
        public abstract class usrLCValue : IBqlField { }
        #endregion

        #region UsrPrepaid
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid")]

        public virtual bool? UsrPrepaid { get; set; }
        public abstract class usrPrepaid : IBqlField { }
        #endregion

    }
}