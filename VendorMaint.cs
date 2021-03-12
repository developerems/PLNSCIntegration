using PLNSC;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using System.Collections;
using System;
using PLNSC.ScreenService;
using EllipseWebServicesClient;

namespace PX.Objects.AP
{
    public class VendorMaint_Extension : PXGraphExtension<VendorMaint>
    {
        public static string dbName = Data.Update.PXInstanceHelper.DatabaseName;
        public string urlPrefix(string dbName)
        {
            string result = string.Empty;

            if (dbName.Trim().Contains("DEV"))
            {
                result = "http://ews-elldev.ellipse.plnsc.co.id/ews/services/";
            }
            else if (dbName.Trim().Contains("TRN"))
            {
                result = "http://ews-elltrn.ellipse.plnsc.co.id/ews/services/";
            }
            else if (dbName.Trim().Contains("PRD"))
            {
                result = "http://ews-ellprd.ellipse.plnsc.co.id/ews/services/";
            }
            else
            {
                result = "http://ews-elldev.ellipse.plnsc.co.id/ews/services/";
            }

            return result;
        }

        int sessionTimeout = 3600000;
        int maxInstance = 1;

        public static string districtCode = "SC01";
        public static string positionID = "INTPO";
        public static string userName = "ADMIN";
        public static string password = "P@ssw0rd";

        [PXDBString(32, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Vendor Name", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
        protected virtual void VendorR_AcctName_CacheAttached(PXCache cache) { }

        [PXDBString(32, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMassMergableField]
        protected virtual void Address_AddressLine1_CacheAttached(PXCache cache) { }

        [PXDBString(32, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Address Line 2", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMassMergableField]
        protected virtual void Address_AddressLine2_CacheAttached(PXCache cache) { }

        [PXDBEmail]
        [PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMassMergableField]
        [PXDefault()]
        protected virtual void Contact_EMail_CacheAttached(PXCache cache) { }

        [PXDBString(16, IsUnicode = true)]
        [PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PhoneValidation()]
        [PXMassMergableField]
        protected virtual void Contact_Phone1_CacheAttached(PXCache cache) { }

        [PXDBString(32, IsUnicode = true)]
        [PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXMassMergableField]
        protected virtual void Address_City_CacheAttached(PXCache cache) { }

        [PXDBString(10)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(Address.countryID))]
        [PXDynamicMask(typeof(Search<Country.zipCodeMask, Where<Country.countryID, Equal<Current<Address.countryID>>>>))]
        [PXDefault()]
        [PXMassMergableField]
        protected virtual void Address_PostalCode_CacheAttached(PXCache cache) { }

        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.vendor>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
        [PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.termsID))]
        [PXUIField(DisplayName = "Terms")]
        //[PXForeignReference(typeof(Field<Vendor.termsID>.IsRelatedTo<Terms.termsID>))]
        protected virtual void VendorR_TermsID_CacheAttached(PXCache cache) { }

        [PXDBString(5, IsUnicode = true)]
        [PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isFinancial, Equal<True>>>), CacheGlobal = true)]
        [PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyID))]
        [PXUIField(DisplayName = "Currency ID")]
        protected virtual void VendorR_CuryID_CacheAttached(PXCache cache) { }

        [PXDBString(6, IsUnicode = true)]
        [PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
        [PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyRateTypeID))]
        [PXUIField(DisplayName = "Curr. Rate Type ")]
        protected virtual void VendorR_CuryRateTypeID_CacheAttached(PXCache cache) { }

        protected void Vendor_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {

            var row = (Vendor)e.Row;

            string vClass = row.VendorClassID;

            if(vClass == "FORWARDER")
            {
                AutoNumberAttribute.SetNumberingId<Vendor.acctCD>(cache, "FORWARDER");
            }
        }

        protected void VendorR_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            Vendor row = (Vendor)e.Row;
            var str = row.AcctCD.ToString().Trim();
            int value = str.Length;

            if (System.Text.RegularExpressions.Regex.IsMatch(str, @"^[a-zA-Z0-9]+$"))
            {
                if (value != 6)
                {
                    sender.RaiseExceptionHandling<Vendor.acctCD>(e.Row, row.AcctCD, new PXSetPropertyException(CustomMessage.CustomerIDInvalid));
                    //throw new PXRowPersistingException(typeof(VendorR.acctCD).Name, null, "Cannot Be Less Or More Than 6 Characters");
                }
            }
            else
            {
                sender.RaiseExceptionHandling<Vendor.acctCD>(e.Row, row.AcctCD, new PXSetPropertyException(CustomMessage.BAccountCharInvalid));
            }
            maintainVendor(sender, row);
        }
        public virtual IEnumerable maintainVendor(PXCache cache, Vendor vendor)
        {
            string screenName = "";
            string errMess = "";

            if (vendor.AcctCD == null) return null;
            string vendorAcctCD = vendor.AcctCD.Trim();
            string currencyType = vendor.CuryID != null ? vendor.CuryID.Trim() : "IDR";
            string vendorAcctName = vendor.AcctName != null ? vendor.AcctName.Trim() : " ";

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                ClientConversation.authenticate(userName, password);
                ScreenService screenService = new ScreenService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}ScreenService"
                };
                OperationContext screenContext = new OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = 1,
                    returnWarnings = false,
                    trace = false
                };

                screenReply = screenService.executeScreen(screenContext, "MSO200");
                screenName = screenReply.mapName;

                if (screenName != "MSM200A")
                {
                    screenService.positionToMenu(screenContext);
                    screenReply = screenService.executeScreen(screenContext, "MSO200");
                    screenName = screenReply.mapName;
                    if (screenName != "MSM200A")
                    {
                        throw new PXException(CustomMessage.NotMSM200A);
                    }
                }

                ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                fields[0].fieldName = "OPTION1I";
                fields[0].value = "1";
                fields[1].fieldName = "SUPPLIER_NO1I";
                fields[1].value = vendorAcctCD;
                fields[2].fieldName = "SUP_STATUS1I";
                fields[2].value = "N";

                submitRequest.screenFields = fields;

                submitRequest.screenKey = "1";
                screenReply = screenService.submit(screenContext, submitRequest);

                screenName = screenReply.mapName;
                errMess = screenReply.message;

                if (errMess.Trim() != "" && !errMess.Contains("SUPPLIER CODE ALREADY EXISTS"))
                {
                    screenService.positionToMenu(screenContext);
                    throw new PXException(errMess.Trim());
                }
                else
                {
                    if (errMess.Contains("SUPPLIER CODE ALREADY EXISTS"))
                    {
                        ScreenNameValueDTO[] fieldsMod = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                        fieldsMod[0].fieldName = "OPTION1I";
                        fieldsMod[0].value = "2";
                        fieldsMod[1].fieldName = "SUPPLIER_NO1I";
                        fieldsMod[1].value = vendorAcctCD;
                        fieldsMod[2].fieldName = "SUP_STATUS1I";
                        fieldsMod[2].value = string.Empty;

                        submitRequest.screenFields = fieldsMod;

                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenContext, submitRequest);

                        screenName = screenReply.mapName;
                        errMess = screenReply.message;

                        if (errMess.Trim() != "" && !errMess.Contains("SUPPLIER CODE ALREADY EXISTS"))
                        {
                            screenService.positionToMenu(screenContext);
                            throw new PXException(errMess.Trim());
                        }
                    }

                    if (screenName == "MSM200B")
                    {
                        BAccount acctVendor = PXSelect<BAccount,
                        Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(Base, vendor.BAccountID);

                        Address vendorAddress = PXSelect<Address,
                            Where<Address.bAccountID, Equal<Required<Address.bAccountID>>>>.Select(Base, vendor.BAccountID);

                        Contact contact = PXSelect<Contact,
                            Where<Contact.bAccountID, Equal<Required<Contact.bAccountID>>>>.Select(Base, vendor.BAccountID);

                        String vendorCountry = "";
                        String addressLine1 = "";
                        String addressLine2 = "";
                        String addressLine3 = "";
                        string kota = string.Empty;
                        string zipCode = string.Empty;

                        String attn = "";
                        String email = "";
                        String phone = "";

                        if (vendorAddress != null)
                        {
                            vendorCountry = vendorAddress.CountryID.Trim();
                            addressLine1 = vendorAddress.AddressLine1.Trim();
                            addressLine2 = vendorAddress.AddressLine2 != null ? vendorAddress.AddressLine2.Trim() : string.Empty;
                            addressLine3 = vendorAddress.AddressLine3 != null ? vendorAddress.AddressLine3.Trim() : string.Empty;
                            kota = vendorAddress != null ? vendorAddress.City : string.Empty;
                            zipCode = vendorAddress.PostalCode != null ? vendorAddress.PostalCode.Trim() : string.Empty;
                        }

                        if (contact != null)
                        {
                            attn = contact.Salutation != null ? contact.Salutation.Trim() : " ";
                            email = contact.EMail != null ? contact.EMail.Trim() : " ";
                            phone = contact.Phone1 != null ? contact.Phone1.Trim() : " ";
                        }

                        if (screenName == "MSM200B")
                        {
                            ScreenNameValueDTO[] fields200B = { new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO()
                            };

                            fields200B[0].fieldName = "SUPPLIER_NAME2I";
                            fields200B[0].value = vendorAcctName;
                            fields200B[1].fieldName = "COUNTRY_CODE2I";
                            fields200B[1].value = vendorCountry;
                            fields200B[2].fieldName = "ORDER_ADDR_12I";
                            fields200B[2].value = addressLine1;
                            fields200B[3].fieldName = "ORDER_ADDR_22I";
                            fields200B[3].value = addressLine2;
                            fields200B[4].fieldName = "ORDER_ADDR_32I";
                            fields200B[4].value = kota;
                            fields200B[5].fieldName = "ORDER_CONTACT2I";
                            fields200B[5].value = attn;
                            fields200B[6].fieldName = "ORDER_ZIP2I";
                            fields200B[6].value = zipCode;
                            fields200B[7].fieldName = "ORDER_PHONE2I";
                            fields200B[7].value = phone;
                            fields200B[8].fieldName = "ORDER_EMAIL_L12I";
                            fields200B[8].value = email;

                            submitRequest.screenFields = fields200B;

                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenContext, submitRequest);

                            screenName = screenReply.mapName.Trim();
                            errMess = screenReply.message;

                            if (screenName == "MSM200F")
                            {
                                ScreenNameValueDTO[] fields200F = { new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO()
                                };

                                fields200F[0].fieldName = "PAYMENT_NAME6I";
                                fields200F[0].value = vendorAcctName;
                                fields200F[1].fieldName = "PAYMENT_ADDR_16I";
                                fields200F[1].value = addressLine1;
                                fields200F[2].fieldName = "PAYMENT_ADDR_26I";
                                fields200F[2].value = addressLine2;
                                fields200F[3].fieldName = "PAYMENT_ADDR_36I";
                                fields200F[3].value = addressLine3;
                                fields200F[4].fieldName = "PAYMENT_CTACT6I";
                                fields200F[4].value = attn;
                                fields200F[5].fieldName = "PAYMENT_PHONE6I";
                                fields200F[5].value = phone;
                                fields200F[6].fieldName = "PAYMENT_EMAIL_L16I";
                                fields200F[6].value = email;

                                submitRequest.screenFields = fields200F;

                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM200D")
                            {
                                ScreenNameValueDTO[] fields200D = { new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO()
                                };

                                fields200D[0].fieldName = "RETURNS_NAME4I";
                                fields200D[0].value = vendorAcctName;
                                fields200D[1].fieldName = "RETURNS_ADDR_14I";
                                fields200D[1].value = addressLine1;
                                fields200D[2].fieldName = "RETURNS_ADDR_24I";
                                fields200D[2].value = addressLine2;
                                fields200D[3].fieldName = "RETURNS_ADDR_34I";
                                fields200D[3].value = addressLine3;
                                fields200D[4].fieldName = "RETURNS_CTACT4I";
                                fields200D[4].value = attn;
                                fields200D[5].fieldName = "RETURNS_PHONE4I";
                                fields200D[5].value = phone;
                                fields200D[6].fieldName = "RETURNS_EMAIL_L14I";
                                fields200D[6].value = email;

                                submitRequest.screenFields = fields200D;

                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM20EA")
                            {
                                ScreenNameValueDTO[] fields20EA = { new ScreenNameValueDTO() };
                                fields20EA[0].fieldName = "COY_NAME1I";
                                fields20EA[0].value = "";

                                submitRequest.screenFields = fields20EA;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM200C")
                            {
                                ScreenNameValueDTO[] fields200C = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                                fields200C[0].fieldName = "CLOSE_XMAS3I";
                                fields200C[0].value = "N";
                                fields200C[1].fieldName = "CURRENCY_TYPE3I";
                                fields200C[1].value = currencyType;
                                fields200C[2].fieldName = "NO_ITEM_ORDER3I";
                                fields200C[2].value = "M";

                                submitRequest.screenFields = fields200C;

                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM200E")
                            {
                                ScreenNameValueDTO[] fields200E = { new ScreenNameValueDTO() };

                                fields200E[0].fieldName = "LEGAL_NAME_15I";
                                fields200E[0].value = "";

                                submitRequest.screenFields = fields200E;
                            
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM20DA")
                            {
                                ScreenNameValueDTO[] fields20DA = {
                                    new ScreenNameValueDTO(),
                                    new ScreenNameValueDTO(),
                                    new ScreenNameValueDTO(),
                                    new ScreenNameValueDTO(),
                                    new ScreenNameValueDTO(),
                                    new ScreenNameValueDTO(),
                                    new ScreenNameValueDTO()
                                };

                                fields20DA[0].fieldName = "INV_STMT_IND1I";
                                fields20DA[0].value = "L";
                                fields20DA[1].fieldName = "NO_OF_DAYS_PAY1I";
                                fields20DA[1].value = "30";
                                fields20DA[2].fieldName = "ORDS_ALLOWED1I";
                                fields20DA[2].value = "Y";
                                fields20DA[3].fieldName = "MAX_ORD_ITEMS1I";
                                fields20DA[3].value = "999";
                                fields20DA[4].fieldName = "PAYMNT_ALLOWED1I";
                                fields20DA[4].value = "Y";
                                fields20DA[5].fieldName = "PAYMENT_METH1I";
                                fields20DA[5].value = "C";
                                fields20DA[6].fieldName = "RECON_REQUIRED1I";
                                fields20DA[6].value = "Y";

                                submitRequest.screenFields = fields20DA;

                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                                else
                                {
                                    if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                                    {
                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenContext, submitRequest);

                                        screenName = screenReply.mapName.Trim();
                                        errMess = screenReply.message;
                                        if (errMess.Trim() != "")
                                        {
                                            screenService.positionToMenu(screenContext);
                                            throw new PXException(errMess.Trim());
                                        }
                                    }
                                }
                            }

                            if (screenName == "MSM20DB")
                            {
                                ScreenNameValueDTO[] fields20DB = { new ScreenNameValueDTO() };
                                fields20DB[0].fieldName = "ORDER_MEDIUM2I";
                                fields20DB[0].value = "";

                                submitRequest.screenFields = fields20DB;

                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM205A")
                            {
                                ScreenNameValueDTO[] fields205A = { new ScreenNameValueDTO() };
                                fields205A[0].fieldName = "DSTRCT_CODE1I";
                                fields205A[0].value = "";

                                submitRequest.screenFields = fields205A;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM205B")
                            {
                                ScreenNameValueDTO[] fields205B = { new ScreenNameValueDTO() };
                                fields205B[0].fieldName = "DSTRCT_CODE2I";
                                fields205B[0].value = "";

                                submitRequest.screenFields = fields205B;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    screenService.positionToMenu(screenContext);
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (screenName == "MSM200A")
                            {
                                screenService.positionToMenu(screenContext);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }

            return null;
        }
    }
}