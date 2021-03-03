using System;
using PX.Data;

namespace PLNSC
{
  [Serializable]
  public class SCLoCRequest : IBqlTable
  {
        #region LoCRequestID
        public abstract class loCRequestID : IBqlField
        {
        }
        protected String _LoCRequestID;
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Lo CRequest ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String LoCRequestID
        {
            get
            {
                return this._LoCRequestID;
            }
            set
            {
                this._LoCRequestID = value;
            }
        }
        #endregion
        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Order Type")]
        public virtual string OrderType { get; set; }
        public abstract class orderType : IBqlField { }
        #endregion
        #region OrderNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Order Nbr")]
        public virtual string OrderNbr { get; set; }
        public abstract class orderNbr : IBqlField { }
        #endregion
        #region AddendumNbr
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Addendum Nbr")]
        public virtual string AddendumNbr { get; set; }
        public abstract class addendumNbr : IBqlField { }
        #endregion
        #region AddendumDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Addendum Date")]
        public virtual DateTime? AddendumDate { get; set; }
        public abstract class addendumDate : IBqlField { }
        #endregion
        #region AdvisingBankID
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Advising Bank ID")]
        public virtual string AdvisingBankID { get; set; }
        public abstract class advisingBankID : IBqlField { }
        #endregion
        #region EstOpenLCDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Est Open LCDate")]
        public virtual DateTime? EstOpenLCDate { get; set; }
        public abstract class estOpenLCDate : IBqlField { }
        #endregion
        #region ActOpenLCDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Act Open LCDate")]
        public virtual DateTime? ActOpenLCDate { get; set; }
        public abstract class actOpenLCDate : IBqlField { }
        #endregion
        #region LoCType
        [PXDBInt()]
        [PXUIField(DisplayName = "Lo CType")]
        public virtual int? LoCType { get; set; }
        public abstract class loCType : IBqlField { }
        #endregion
        #region Locnbr
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Locnbr")]
        public virtual string Locnbr { get; set; }
        public abstract class locnbr : IBqlField { }
        #endregion
        #region LoCCreationDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Lo CCreation Date")]
        public virtual DateTime? LoCCreationDate { get; set; }
        public abstract class loCCreationDate : IBqlField { }
        #endregion
        #region LeadTime
        [PXDBInt()]
        [PXUIField(DisplayName = "Lead Time")]
        public virtual int? LeadTime { get; set; }
        public abstract class leadTime : IBqlField { }
        #endregion
        #region LoCExpiredDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Lo CExpired Date")]
        public virtual DateTime? LoCExpiredDate { get; set; }
        public abstract class loCExpiredDate : IBqlField { }
        #endregion
        #region DocPresentDur
        [PXDBInt()]
        [PXUIField(DisplayName = "Doc Present Dur")]
        public virtual int? DocPresentDur { get; set; }
        public abstract class docPresentDur : IBqlField { }
        #endregion
        #region ItemCondition
        [PXDBInt()]
        [PXUIField(DisplayName = "Item Condition")]
        public virtual int? ItemCondition { get; set; }
        public abstract class itemCondition : IBqlField { }
        #endregion
        #region IncoTerm
        [PXDBInt()]
        [PXUIField(DisplayName = "Inco Term")]
        public virtual int? IncoTerm { get; set; }
        public abstract class incoTerm : IBqlField { }
        #endregion
        #region PartialShipment
        [PXDBInt()]
        [PXUIField(DisplayName = "Partial Shipment")]
        public virtual int? PartialShipment { get; set; }
        public abstract class partialShipment : IBqlField { }
        #endregion
        #region Transhipment
        [PXDBInt()]
        [PXUIField(DisplayName = "Transhipment")]
        public virtual int? Transhipment { get; set; }
        public abstract class transhipment : IBqlField { }
        #endregion
        #region EstShipmentDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Est Shipment Date")]
        public virtual DateTime? EstShipmentDate { get; set; }
        public abstract class estShipmentDate : IBqlField { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : IBqlField { }
        #endregion
        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : IBqlField { }
        #endregion
        #region CreatedDateTime
        [PXDBDate()]
        [PXUIField(DisplayName = "Created Date Time")]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : IBqlField { }
        #endregion
        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : IBqlField { }
        #endregion
        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : IBqlField { }
        #endregion
        #region LastModifiedDateTime
        [PXDBDate()]
        [PXUIField(DisplayName = "Last Modified Date Time")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : IBqlField { }
        #endregion
        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : IBqlField { }
        #endregion
    }
}