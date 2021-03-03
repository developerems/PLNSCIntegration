using CRLocation = PX.Objects.CR.Standalone.Location;
using POReceipt = PX.Objects.PO.POReceipt;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.SO
{
    public class SOShipmentExt : PXCacheExtension<PX.Objects.SO.SOShipment>
    {
        #region UsrOnstReceiptDate
        public abstract class usrOnstReceiptDate : IBqlField { }
        protected DateTime? _UsrOnstReceiptDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Onsite Receipt Date")]

        public virtual DateTime? UsrOnstReceiptDate
        {
            get
            {
                return this._UsrOnstReceiptDate;
            }
            set
            {
                this._UsrOnstReceiptDate = value;
            }
        }
        #endregion

        #region UsrBastNbr
        public abstract class usrBastNbr : IBqlField { }
        protected String _UsrBastNbr;
        [PXDBString(40)]
        [PXUIField(DisplayName = "BAST Nbr.")]

        public virtual string UsrBastNbr
        {
            get
            {
                return this._UsrBastNbr;
            }
            set
            {
                this._UsrBastNbr = value;
            }
        }
        #endregion

        #region UsrBastDate
        public abstract class usrBastDate : IBqlField { }
        protected DateTime? _UsrBastDate;
        [PXDBDate]
        [PXUIField(DisplayName = "BAST Date")]

        public virtual DateTime? UsrBastDate
        {
            get
            {
                return this._UsrBastDate;
            }
            set
            {
                this._UsrBastDate = value;
            }
        }
        #endregion
    }
}