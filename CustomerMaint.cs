using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CR.MassProcess;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Data.ReferentialIntegrity.Attributes;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.Repositories;
using PX.Objects.Common;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using CashAccountAttribute = PX.Objects.GL.CashAccountAttribute;
using PX.Objects;
using PX.Objects.AR;
using PLNSC;
using PLNSC.CustomerRef;
using EllipseWebServicesClient;
using PLNSC.ScreenService;

namespace PX.Objects.AR
{
  public class CustomerMaint_Extension : PXGraphExtension<CustomerMaint>
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

        #region Event Handlers


        [PXDBString(32, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
        protected virtual void Customer_AcctName_CacheAttached(PXCache cache) { }

        [PXDBEmail]
        [PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMassMergableField]
        [PXDefault()]
        protected virtual void Contact_EMail_CacheAttached(PXCache cache) { }

        [PXDBString(16, IsUnicode = true)]
        [PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
        [PhoneValidation()]
        [PXDefault()]
        [PXMassMergableField]
        protected virtual void Contact_Phone1_CacheAttached(PXCache cache)
        { }

        [PXDBString(32, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXMassMergableField]
        protected virtual void Address_AddressLine1_CacheAttached(PXCache cache)
        { }

        [PXDBString(32, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 2", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMassMergableField]
        protected virtual void Address_AddressLine2_CacheAttached(PXCache cache)
        { }

        [PXDBString(32, IsUnicode = true)]
        [PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXMassMergableField]
        protected virtual void Address_City_CacheAttached(PXCache cache)
        { }

        [PXDBString(10)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(Address.countryID))]
        [PXDefault()]
        [PXDynamicMask(typeof(Search<Country.zipCodeMask, Where<Country.countryID, Equal<Current<Address.countryID>>>>))]
        [PXMassMergableField]
        protected virtual void Address_PostalCode_CacheAttached(PXCache cache) { }

        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
        [PXDefault(typeof(Search<CustomerClass.termsID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
        [PXUIField(DisplayName = "Terms")]
        protected virtual void Customer_TermsID_CacheAttached(PXCache cache) { }

        [PXDBString(5, IsUnicode = true)]
        [PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
        [PXUIField(DisplayName = "Currency ID")]
        [PXDefault(typeof(Search<CustomerClass.curyID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
        protected virtual void Customer_CuryID_CacheAttached(PXCache cache) { }

        [PXDBString(6, IsUnicode = true)]
        [PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
        [PXDefault(typeof(Search<CustomerClass.curyRateTypeID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
        [PXUIField(DisplayName = "Curr. Rate Type ")]
        protected virtual void Customer_CuryRateTypeID_CacheAttached(PXCache cache) { }

        #endregion

        protected void Customer_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            Customer row = (Customer)e.Row;

            if (row == null) return;

            var str = row.AcctCD.ToString().Trim();
            int value = str.Length;

            if (System.Text.RegularExpressions.Regex.IsMatch(str, @"^[a-zA-Z0-9]+$"))
            {
                if (value != 6)
                {
                    sender.RaiseExceptionHandling<Customer.acctCD>(e.Row, row.AcctCD, new PXSetPropertyException(CustomMessage.CustomerIDInvalid));
                }
            }
            else
            {
                sender.RaiseExceptionHandling<Customer.acctCD>(e.Row, row.AcctCD, new PXSetPropertyException(CustomMessage.BAccountCharInvalid));
            }
            maintainCustomer(sender, row);
        }

        public virtual IEnumerable maintainCustomer(PXCache cache, Customer customer)
        {
            string screenName = "";
            string errMess = "";
            string customerAcctCD;

            if (customer.AcctCD == null)
            {
                return null;
            }
            else
            {
                customerAcctCD = customer.AcctCD.Trim();
            }

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
                PLNSC.ScreenService.OperationContext screenContext = new PLNSC.ScreenService.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = 1,
                    returnWarnings = false,
                    trace = false
                    //transaction = transID
                };

                string currencyType = customer.CuryID != null ? customer.CuryID.Trim() : "IDR";

                screenReply = screenService.executeScreen(screenContext, "MSO500");
                screenName = screenReply.mapName;

                if (screenName != "MSM500A")
                {
                    screenService.positionToMenu(screenContext);
                    screenReply = screenService.executeScreen(screenContext, "MSO500");
                    screenName = screenReply.mapName;
                    if (screenName != "MSM500A")
                    {
                        throw new PXException(CustomMessage.NotMSM500A);
                    }
                }

                ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                fields[0].fieldName = "OPTION1I";
                fields[0].value = "1";
                fields[1].fieldName = "CUST_NO1I";
                fields[1].value = customerAcctCD;

                submitRequest.screenFields = fields;

                submitRequest.screenKey = "1"; // OK
                screenReply = screenService.submit(screenContext, submitRequest);

                screenName = screenReply.mapName;
                errMess = screenReply.message;

                if (errMess.Trim() != "" && !errMess.Contains("CUSTOMER CODE ALREADY EXISTS"))
                {
                    throw new PXException(errMess.Trim());
                }
                else
                {
                    if (errMess.Contains("CUSTOMER CODE ALREADY EXISTS"))
                    {
                        ScreenNameValueDTO[] fieldsMod = { new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                        fieldsMod[0].fieldName = "OPTION1I";
                        fieldsMod[0].value = "2";
                        fieldsMod[1].fieldName = "CUST_NO1I";
                        fieldsMod[1].value = customerAcctCD;

                        submitRequest.screenFields = fieldsMod;

                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenContext, submitRequest);

                        screenName = screenReply.mapName;
                        errMess = screenReply.message;

                        if (errMess.Trim() != "" && !errMess.Contains("CUSTOMER CODE ALREADY EXISTS"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }

                    if (screenName == "MSM500B")
                    {
                        BAccount acctCustomer = PXSelect<BAccount,
                        Where<BAccount.bAccountID,
                        Equal<Required<BAccount.bAccountID>>>>.Select(Base, customer.BAccountID);

                        Address customerAddress = PXSelect<Address,
                            Where<Address.bAccountID, Equal<Required<Address.bAccountID>>>>.Select(Base, customer.BAccountID);

                        Contact contact = PXSelect<Contact,
                            Where<Contact.bAccountID, Equal<Required<Contact.bAccountID>>>>.Select(Base, customer.BAccountID);

                        String customerCountry = "";
                        String addressLine1 = "";
                        String addressLine2 = "";

                        String attn = "";
                        String email = "";
                        String phone = "";

                        if (customerAddress != null)
                        {
                            customerCountry = customerAddress.CountryID.Trim();
                            addressLine1 = customerAddress.AddressLine1.Trim();
                            addressLine2 = customerAddress.AddressLine2 != null ? customerAddress.AddressLine2.Trim() : " ";
                        }

                        if (contact != null)
                        {
                            attn = contact.Salutation != null ? contact.Salutation.Trim() : " ";
                            email = contact.EMail != null ? contact.EMail.Trim() : " ";
                            phone = contact.Phone1 != null ? contact.Phone1.Trim() : " ";
                        }

                        if (screenName == "MSM500B")
                        {
                            ScreenNameValueDTO[] fields500B = { new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO(),
                                new ScreenNameValueDTO()
                            };

                            string customerAcctName = customer.AcctName != null ? customer.AcctName.Trim() : " ";

                            fields500B[0].fieldName = "CUST_NAME2I";
                            fields500B[0].value = customerAcctName;
                            fields500B[1].fieldName = "CURRENCY_TYPE2I";
                            fields500B[1].value = currencyType;
                            fields500B[2].fieldName = "COUNTRY_CODE2I";
                            fields500B[2].value = customerCountry;
                            fields500B[3].fieldName = "INV_ADDR_12I";
                            fields500B[3].value = addressLine1;
                            fields500B[4].fieldName = "INV_ADDR_22I";
                            fields500B[4].value = addressLine2;
                            fields500B[5].fieldName = "INV_CONTACT2I";
                            fields500B[5].value = attn;
                            fields500B[6].fieldName = "INV_PHONE2I";
                            fields500B[6].value = phone;

                            submitRequest.screenFields = fields500B;

                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenContext, submitRequest);

                            screenName = screenReply.mapName.Trim();
                            errMess = screenReply.message;

                            if (errMess.Trim() != "" && !errMess.Contains("WARNING: CHECK PRICING CODE CURRENCY IN DISTRICTS"))
                            {
                                throw new PXException(errMess.Trim());
                            }
                            else
                            {
                                if (errMess.Contains("WARNING: CHECK PRICING CODE CURRENCY IN DISTRICTS"))
                                {
                                    submitRequest.screenKey = "1"; // OK
                                    screenReply = screenService.submit(screenContext, submitRequest);
                                    screenName = screenReply.mapName.Trim();
                                    errMess = screenReply.message;
                                }
                            }

                            if (screenName == "MSM500D")
                            {
                                ScreenNameValueDTO[] fields500D = { new ScreenNameValueDTO() };
                                fields500D[0].fieldName = "EMAIL_ADDRESS4I1";
                                fields500D[0].value = email;

                                submitRequest.screenFields = fields500D;

                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenContext, submitRequest);

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    throw new PXException(errMess.Trim());
                                }

                                if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                                {
                                    submitRequest.screenKey = "1"; // OK
                                    screenReply = screenService.submit(screenContext, submitRequest);
                                }

                                screenName = screenReply.mapName.Trim();
                                errMess = screenReply.message;

                                if (errMess.Trim() != "")
                                {
                                    throw new PXException(errMess.Trim());
                                }

                                if (screenName == "MSM500C")
                                {
                                    ScreenNameValueDTO[] fields500C = new ScreenNameValueDTO[9];

                                    fields500C[0] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "PMT_METH_IND3I",
                                        value = "C"
                                    };

                                    fields500C[1] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "CR_TERMS3I",
                                        value = "U"
                                    };

                                    fields500C[2] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "SECURED3I",
                                        value = "N"
                                    };

                                    fields500C[3] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "STMT_REQ_SW3I",
                                        value = "Y"
                                    };

                                    fields500C[4] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "INV_MEDIUM_IND3I",
                                        value = "P"
                                    };

                                    fields500C[5] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "DAYS_GRACE3I",
                                        value = "5"
                                    };

                                    fields500C[6] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "DAYS_TO_PAY3I",
                                        value = "30"
                                    };

                                    string customerClass = customer.CustomerClassID;
                                    string acctGrpCode = "";

                                    if (customerClass != null)
                                    {
                                        customerClass = customerClass.Trim();
                                        switch (customerClass)
                                        {
                                            case "PLN":
                                                acctGrpCode = "0001";
                                                break;

                                            case "IP":
                                                acctGrpCode = "0002";
                                                break;

                                            case "PJB":
                                                acctGrpCode = "0003";
                                                break;

                                            case "OTHERS":
                                                acctGrpCode = "0000";
                                                break;

                                            default:
                                                acctGrpCode = "0000";
                                                break;
                                        }
                                    }

                                    fields500C[7] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "ACCT_GRP_CODE3I",
                                        value = acctGrpCode
                                    };

                                    fields500C[8] = new ScreenNameValueDTO()
                                    {
                                        fieldName = "STATEMENT_TYPE3I",
                                        value = "B"
                                    };

                                    submitRequest.screenFields = fields500C;

                                    submitRequest.screenKey = "1"; // OK
                                    screenReply = screenService.submit(screenContext, submitRequest);

                                    screenName = screenReply.mapName.Trim();
                                    errMess = screenReply.message;

                                    if (errMess.Trim() != "" && !errMess.Contains("ACCT GROUP CODE ONLY UPDATED IF VALID FOR DISTRICT"))
                                    {
                                        throw new PXException(errMess.Trim());
                                    }

                                    if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                                    {
                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenContext, submitRequest);
                                        screenName = screenReply.mapName.Trim();
                                        errMess = screenReply.message;

                                        if (errMess.Trim() != "")
                                        {
                                            throw new PXException(errMess.Trim());
                                        }
                                        else
                                        {
                                            screenService.positionToMenu(screenContext);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }
            return null;
        }
    }
}