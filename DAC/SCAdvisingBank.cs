using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;

namespace PLNSC
{
  [Serializable]
  public class SCAdvisingBank : IBqlTable
  {
        #region AdvisingBankID
        public abstract class advisingBankID : IBqlField
        {
        }
        protected string _AdvisingBank;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Advising Bank ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXSelector(typeof(Search<SCAdvisingBank.advisingBankID>), CacheGlobal = true)]
        public virtual string AdvisingBankID
        {
            get
            {
                return this._AdvisingBank;
            }
            set
            {
                this._AdvisingBank = value;
            }
        }
        #endregion
        #region AdvisingBankName
        public abstract class advisingBankName : IBqlField
        {
        }
        protected string _AdvisingBankName;
        [PXDBLocalizableString(60, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Advising Bank Name", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        public virtual string AdvisingBankName
        {
            get
            {
                return this._AdvisingBankName;
            }
            set
            {
                this._AdvisingBankName = value;
            }
        }
        #endregion
        #region AdvisingBankBranch
        public abstract class advisingBankBranch : IBqlField
        {
        }
        protected string _AdvisingBankBranch;
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Advising Bank Branch")]
        public virtual string AdvisingBankBranch
        {
            get
            {
                return this._AdvisingBankBranch;
            }
            set
            {
                this._AdvisingBankBranch = value;
            }
        }
        #endregion
        #region BankAcctNbr
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Bank Acct Nbr")]
        public virtual string BankAcctNbr { get; set; }
        public abstract class bankAcctNbr : IBqlField { }
        #endregion
        #region DisplayName
        public abstract class displayName : PX.Data.IBqlField
        {
        }
        [PXString]
        [PXUIField(DisplayName = "Address", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(SmartJoin<Space, SCAdvisingBank.addressLine1, SCAdvisingBank.addressLine2, SCAdvisingBank.addressLine3>))]
        public virtual String DisplayName { get; set; }
        #endregion

        #region AddressLine1
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Address Line1")]
        public virtual string AddressLine1 { get; set; }
        public abstract class addressLine1 : IBqlField { }
        #endregion
        #region AddressLine2
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Address Line2")]
        public virtual string AddressLine2 { get; set; }
        public abstract class addressLine2 : IBqlField { }
        #endregion
        #region AddressLine3
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Address Line3")]
        public virtual string AddressLine3 { get; set; }
        public abstract class addressLine3 : IBqlField { }
        #endregion
        #region City
        public abstract class city : PX.Data.IBqlField
        {
        }
        protected String _City;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMassMergableField]
        public virtual String City
        {
            get
            {
                return this._City;
            }
            set
            {
                this._City = value;
            }
        }
        #endregion
        #region State
        public abstract class state : PX.Data.IBqlField
        {
        }
        protected String _State;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "State")]
        [State(typeof(SCAdvisingBank.countryID))]
        [PXMassMergableField]
        public virtual String State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
            }
        }
        #endregion
        #region CountryID
        public abstract class countryID : PX.Data.IBqlField
        {
        }
        protected String _CountryID;
        [PXDefault(typeof(Search<PX.Objects.GL.Branch.countryID, Where<PX.Objects.GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
        [PXDBString(100)]
        [PXUIField(DisplayName = "Country")]
        [Country]
        [PXMassMergableField]
        public virtual String CountryID
        {
            get
            {
                return this._CountryID;
            }
            set
            {
                this._CountryID = value;
            }
        }
        #endregion
        #region PostalCode
        public abstract class postalCode : PX.Data.IBqlField
        {
        }
        protected String _PostalCode;
        [PXDBString(20)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(Address.countryID))]
        [PXDynamicMask(typeof(Search<Country.zipCodeMask, Where<Country.countryID, Equal<Current<Address.countryID>>>>))]
        [PXMassMergableField]
        public virtual String PostalCode
        {
            get
            {
                return this._PostalCode;
            }
            set
            {
                this._PostalCode = value;
            }
        }
        #endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.IBqlField
        {
        }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.IBqlField
        {
        }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.IBqlField
        {
        }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion

        #region Tstamp
        public abstract class Tstamp : PX.Data.IBqlField
        {
        }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.IBqlField
        {
        }
        protected Guid? _NoteID;
        [PXNote(DescriptionField = typeof(SCAdvisingBank.displayName))]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
    }
}