using EllipseWebServicesClient;
using Oracle.ManagedDataAccess.Client;
using PLNSC;
using PLNSC.ScreenService;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.RQ;
using System;
using System.Linq;
using System.Web;

namespace PX.Objects.SO
{
    public class SOShipmentEntry_Extension : PXGraphExtension<SOShipmentEntry>
    {
        public static string dbName = Data.Update.PXInstanceHelper.DatabaseName;

        #region Event Handlers
        [PXOverride]
        public virtual void CorrectShipment(SOOrderEntry docgraph, SOShipment shiporder, Action<SOOrderEntry, SOShipment> baseCorrectShipment)
        {
            string DBName = Data.Update.PXInstanceHelper.DatabaseName;
            var shipLines = PXSelect<SOShipLine,
                Where<SOShipLine.shipmentNbr, Equal<Required<SOShipLine.shipmentNbr>>,
                And<SOShipLine.shipmentType, Equal<Required<SOShipLine.shipmentType>>>>>.Select(Base, shiporder.ShipmentNbr, shiporder.ShipmentType).ToList().Select(a => a.GetItem<SOShipLine>());

            foreach (SOShipLine shipLine in shipLines)
            {
                string sONumber = shipLine.OrigOrderNbr != null ? shipLine.OrigOrderNbr.Trim() : string.Empty;
                int sOLineNumber = shipLine.OrigLineNbr ?? 0;
                int inventoryID = shipLine.InventoryID ?? 0;
                decimal shippedQty = shipLine.ShippedQty ?? 0;

                string shipmentDate = (shiporder.ShipDate ?? DateTime.Now).ToString("yyyyMMdd");
                string glCurrentPeriod = getCurrentPeriod(DBName);

                string purchaseMethod = getPurchaseMethod(sONumber, sOLineNumber, shippedQty,
                    out decimal sOCuryUnitPrice, out decimal sOUnitPrice, out decimal sOShipCuryExtAmt, out decimal sOShipExtAmt,
                    out string strSOShipExtAmt, out string strSOShipCuryExtAmt, out string soLineSort, out string requisitionNbr, out string sOLineCuryExtPrice, out string sOLineExtPrice,
                    out int customerCode, out int customerLocID);

                string inventoryCode = getInventoryInfo(inventoryID, out string invtCOGSAcct, out string invtCOGSSub, out string invtSalesAcct, out string invtSalesSub);
                string pOLineNumber = getPOInfo(requisitionNbr, out string pONumber, out int supplierCode, out int supplierLocID);
                string vendorCD = getSupplierCode(supplierCode, supplierLocID);
                string customercd = getCustomerAccount(customerCode, customerLocID, out string custSalesAcctCD, out string custDiscAcctCD, out string custSalesSubCD);
                string receivedValue = getReceiptValue(pONumber, pOLineNumber, DBName, out string memoAmount, out string currencyType, out string receiptAccount);

                string curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                string foreignInd = curyID == "IDR" ? string.Empty : "Y";

                string reverseJournalResult = string.Empty;
                if (purchaseMethod == "ROK")
                {
                    reverseJournalResult = reverseROKJournal(shiporder, shipLine, DBName,
                        soLineSort, glCurrentPeriod, sONumber, invtCOGSAcct, invtCOGSSub, invtSalesAcct, invtSalesSub, purchaseMethod, vendorCD, pONumber,
                        shipmentDate, custSalesSubCD, receivedValue, receiptAccount, custDiscAcctCD, strSOShipExtAmt, memoAmount, strSOShipCuryExtAmt, sOLineNumber, shippedQty,
                        sOCuryUnitPrice, sOUnitPrice, sOShipCuryExtAmt, sOShipExtAmt);
                }
                else
                {
                    reverseJournalResult = reverseB2BJournal(shiporder, shipLine, DBName,
                        soLineSort, glCurrentPeriod, sONumber, invtCOGSAcct, invtCOGSSub, invtSalesAcct, invtSalesSub, purchaseMethod, vendorCD, pONumber,
                        shipmentDate, custSalesSubCD, receivedValue, receiptAccount, custDiscAcctCD, strSOShipExtAmt, memoAmount, strSOShipCuryExtAmt, sOLineNumber, shippedQty,
                        sOCuryUnitPrice, sOUnitPrice, sOShipCuryExtAmt, sOShipExtAmt);
                }
                //correctShipment(shiporder, shipLine, DBName);
            }
            baseCorrectShipment(docgraph, shiporder);
        }

        [PXOverride]
        public virtual void ConfirmShipment(SOOrderEntry docgraph, SOShipment shiporder, Action<SOOrderEntry, SOShipment> baseConfirmShipment)
        {
            string DBName = Data.Update.PXInstanceHelper.DatabaseName;
            var shipLines = PXSelect<SOShipLine,
                Where<SOShipLine.shipmentNbr, Equal<Required<SOShipLine.shipmentNbr>>,
                And<SOShipLine.shipmentType, Equal<Required<SOShipLine.shipmentType>>>>>.Select(Base, shiporder.ShipmentNbr, shiporder.ShipmentType).ToList().Select(a => a.GetItem<SOShipLine>());

            foreach (SOShipLine shipLine in shipLines)
            {
                string sONumber = shipLine.OrigOrderNbr != null ? shipLine.OrigOrderNbr.Trim() : string.Empty;
                int sOLineNumber = shipLine.OrigLineNbr ?? 0;
                int inventoryID = shipLine.InventoryID ?? 0;
                decimal shippedQty = shipLine.ShippedQty ?? 0;

                string shipmentDate = (shiporder.ShipDate ?? DateTime.Now).ToString("yyyyMMdd");
                string glCurrentPeriod = getCurrentPeriod(DBName);

                string purchaseMethod = getPurchaseMethod(sONumber, sOLineNumber, shippedQty, 
                    out decimal sOCuryUnitPrice, out decimal sOUnitPrice, out decimal sOShipCuryExtAmt, out decimal sOShipExtAmt,
                    out string strSOShipExtAmt, out string strSOShipCuryExtAmt, out string soLineSort, out string requisitionNbr, out string sOLineCuryExtPrice, out string sOLineExtPrice, 
                    out int customerCode, out int customerLocID);

                string inventoryCode = getInventoryInfo(inventoryID, out string invtCOGSAcct, out string invtCOGSSub, out string invtSalesAcct, out string invtSalesSub);
                string pOLineNumber = getPOInfo(requisitionNbr, out string pONumber, out int supplierCode, out int supplierLocID);
                string vendorCD = getSupplierCode(supplierCode, supplierLocID);
                string customercd = getCustomerAccount(customerCode, customerLocID, out string custSalesAcctCD, out string custDiscAcctCD, out string custSalesSubCD);
                string receivedValue = getReceiptValue(pONumber, pOLineNumber, DBName, out string memoAmount, out string currencyType, out string receiptAccount);

                string curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                string foreignInd = curyID == "IDR" ? string.Empty : "Y";

                string createJournalResult = string.Empty;
                if (purchaseMethod == "ROK")
                {
                    createJournalResult = createROKJournal(shiporder, shipLine, DBName,
                        soLineSort, glCurrentPeriod, sONumber, invtCOGSAcct, invtCOGSSub, invtSalesAcct, invtSalesSub, purchaseMethod, vendorCD, pONumber, 
                        shipmentDate, custSalesSubCD, receivedValue, receiptAccount, custDiscAcctCD, strSOShipExtAmt, memoAmount, strSOShipCuryExtAmt, sOLineNumber, shippedQty, 
                        sOCuryUnitPrice, sOUnitPrice, sOShipCuryExtAmt, sOShipExtAmt);
                }
                else
                {
                    createJournalResult = createB2BJournal(shiporder, shipLine, DBName,
                        soLineSort, glCurrentPeriod, sONumber, invtCOGSAcct, invtCOGSSub, invtSalesAcct, invtSalesSub, purchaseMethod, vendorCD, pONumber,
                        shipmentDate, custSalesSubCD, receivedValue, receiptAccount, custDiscAcctCD, strSOShipExtAmt, memoAmount, strSOShipCuryExtAmt, sOLineNumber, shippedQty,
                        sOCuryUnitPrice, sOUnitPrice, sOShipCuryExtAmt, sOShipExtAmt);
                }
            }
            baseConfirmShipment(docgraph, shiporder);
        }
        #endregion

        string createROKJournal(SOShipment shiporder, SOShipLine shipLine, string DBName, 
            string sOLineSort, string glCurrentPeriod, string sONumber, string invtCOGSAcct, string invtCOGSSub, string invtSalesAcct, string invtSalesSub, string purchaseMethod,
            string vendorCD, string pONumber, string shipmentDate, string custSalesSubCD, string receivedValue, string receiptAccount, string custDiscAcctCD, string strSOShipExtAmt,
            string memoAmount, string strSOShipCuryExtAmt, int sOLineNumber,
            decimal shippedQty, decimal sOCuryUnitPrice, decimal sOUnitPrice, decimal sOShipCuryExtAmt, decimal sOShipExtAmt)
        {
            string result = "";

            bool loggedIn = false;
            string screenName = string.Empty;
            string errMess = string.Empty;
            string functionKey = string.Empty;

            DateTime shipmentDate1 = DateTime.ParseExact(shipmentDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime todaysDate = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            if (shipmentDate1 > todaysDate)
            {
                shipmentDate = DateTime.Now.ToString("yyyyMMdd");
            }

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                ClientConversation.authenticate("ADMIN", "");
                loggedIn = true;
            }
            catch (Exception ex)
            {
                loggedIn = false;
                throw new PXException(ex.Message);
            }

            if (loggedIn)
            {
                try
                {
                    ClientConversation.authenticate("ADMIN", "");
                    ScreenService screenService = new ScreenService()
                    {
                        Timeout = 3600000
                    };
                    OperationContext screenOperationContext = new OperationContext()
                    {
                        district = "SC01",
                        position = "INTPO",
                        maxInstances = 1
                    };
                    screenReply = screenService.executeScreen(screenOperationContext, "MSO905");
                    screenName = screenReply.mapName;

                    if (screenName != "MSM905A")
                    {
                        throw new PXException(CustomMessage.NotMSM905A);
                    }

                    string curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                    string foreignInd = curyID == "IDR" ? string.Empty : "Y";
                    string dOLine = Right($"000{sOLineSort}", 3);

                    string manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                    string currentManjnlVchr = getExistingJournal(manJnlNbr);

                    bool proceed = false;
                    int a = 0;
                    if (currentManjnlVchr != string.Empty)
                    {
                        do
                        {
                            a++;
                            manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}-{a.ToString()}";
                            currentManjnlVchr = getExistingJournal(manJnlNbr);
                            if (currentManjnlVchr == string.Empty)
                            {
                                proceed = true;
                            }
                        } while (proceed == false);
                    }

                    ScreenNameValueDTO[] fieldsMSM905A = new ScreenNameValueDTO[5];
                    fieldsMSM905A[0] = new ScreenNameValueDTO();
                    fieldsMSM905A[0].fieldName = "OPTION1I";
                    fieldsMSM905A[0].value = "3";

                    fieldsMSM905A[1] = new ScreenNameValueDTO();
                    fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                    fieldsMSM905A[1].value = manJnlNbr;

                    fieldsMSM905A[2] = new ScreenNameValueDTO();
                    fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                    fieldsMSM905A[2].value = glCurrentPeriod;

                    fieldsMSM905A[3] = new ScreenNameValueDTO();
                    fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                    fieldsMSM905A[3].value = "N";

                    fieldsMSM905A[4] = new ScreenNameValueDTO();
                    fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                    fieldsMSM905A[4].value = string.Empty;

                    submitRequest.screenFields = fieldsMSM905A;
                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenOperationContext, submitRequest);

                    screenName = screenReply.mapName.Trim();
                    errMess = screenReply.message.Trim();
                    functionKey = screenReply.functionKeys.Trim();

                    if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                    {
                        throw new PXException(errMess.Trim());
                    }

                    if (errMess.Trim().Contains("JOURNAL ALREADY EXISTS"))
                    {
                        manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                        curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                        foreignInd = curyID == "IDR" ? string.Empty : "Y";

                        fieldsMSM905A = new ScreenNameValueDTO[5];
                        fieldsMSM905A[0] = new ScreenNameValueDTO();
                        fieldsMSM905A[0].fieldName = "OPTION1I";
                        fieldsMSM905A[0].value = "3";

                        fieldsMSM905A[1] = new ScreenNameValueDTO();
                        fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                        fieldsMSM905A[1].value = manJnlNbr;

                        fieldsMSM905A[2] = new ScreenNameValueDTO();
                        fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                        fieldsMSM905A[2].value = glCurrentPeriod;

                        fieldsMSM905A[3] = new ScreenNameValueDTO();
                        fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                        fieldsMSM905A[3].value = "N";

                        fieldsMSM905A[4] = new ScreenNameValueDTO();
                        fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                        fieldsMSM905A[4].value = string.Empty;

                        submitRequest.screenFields = fieldsMSM905A;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }

                    if (screenName == "MSM906A")
                    {
                        ScreenNameValueDTO[] fieldsDetail = new ScreenNameValueDTO[screenReply.screenFields.Length];

                        for (int i = 0 ; i < screenReply.screenFields.Length ; i++)
                        {
                            fieldsDetail[i] = new ScreenNameValueDTO();
                            fieldsDetail[i].fieldName = screenReply.screenFields[i].fieldName;
                            fieldsDetail[i].value = screenReply.screenFields[i].value;

                            if (screenReply.screenFields[i].fieldName == "JOURNAL_DESC1I")
                            {
                                fieldsDetail[i].value = $"Shipment DO-{sONumber} item ke-{sOLineNumber}";
                            }

                            if (screenReply.screenFields[i].fieldName == "ACCOUNTANT1I")
                            {
                                fieldsDetail[i].value = "ADMIN";
                            }

                            if (screenReply.screenFields[i].fieldName == "JNL_IND1I")
                            {
                                fieldsDetail[i].value = string.Empty;
                            }

                            if (screenReply.screenFields[i].fieldName == "TRANS_DATE1I")
                            {
                                fieldsDetail[i].value = shipmentDate;
                            }

                            if (screenReply.screenFields[i].fieldName == "HDR_AUTH_BY1I")
                            {
                                fieldsDetail[i].value = "8208018JA";
                            }

                            if (screenReply.screenFields[i].fieldName == "JNL_DESC1I1")
                            {
                                fieldsDetail[i].value = $"HPP {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                            }

                            if (screenReply.screenFields[i].fieldName == "DSTRCT_CODE1I1")
                            {
                                fieldsDetail[i].value = "SC01";
                            }

                            if (screenReply.screenFields[i].fieldName == "ACCOUNT_CODE1I1")
                            {
                                fieldsDetail[i].value = custSalesSubCD.Trim().Substring(0, custSalesSubCD.Trim().Length - 4) + invtCOGSSub.Trim().Substring(11) + invtCOGSAcct.Trim();
                            }

                            if (screenReply.screenFields[i].fieldName == "TRAN_AMOUNT1I1")
                            {
                                fieldsDetail[i].value = receivedValue;
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ1I1")
                            {
                                fieldsDetail[i].value = sONumber;
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ_IND1I1")
                            {
                                fieldsDetail[i].value = "P";
                            }

                            if (screenReply.screenFields[i].fieldName == "JNL_DESC1I2")
                            {
                                fieldsDetail[i].value = $"PRSD {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                            }

                            if (screenReply.screenFields[i].fieldName == "DSTRCT_CODE1I2")
                            {
                                fieldsDetail[i].value = "SC01";
                            }

                            if (screenReply.screenFields[i].fieldName == "ACCOUNT_CODE1I2")
                            {
                                fieldsDetail[i].value = receiptAccount.Trim();
                            }

                            if (screenReply.screenFields[i].fieldName == "TRAN_AMOUNT1I2")
                            {
                                fieldsDetail[i].value = $"-{receivedValue}"; ;
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ1I2")
                            {
                                fieldsDetail[i].value = sONumber;
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ_IND1I2")
                            {
                                fieldsDetail[i].value = "P";
                            }
                        }

                        submitRequest.screenFields = fieldsDetail;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "")
                        {
                            throw new PXException(errMess.Trim());
                        }

                        if (functionKey.Contains("XMIT-Confirm"))
                        {
                            ScreenNameValueDTO[] fieldsDetailFirst = new ScreenNameValueDTO[screenReply.screenFields.Length];
                            for (int i = 0; i < screenReply.screenFields.Length; i++)
                            {
                                fieldsDetailFirst[i] = new ScreenNameValueDTO();
                                fieldsDetailFirst[i].fieldName = screenReply.screenFields[i].fieldName;
                                fieldsDetailFirst[i].value = screenReply.screenFields[i].value;
                            }

                            submitRequest.screenFields = fieldsDetailFirst;
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenOperationContext, submitRequest);

                            screenName = screenReply.mapName;
                            errMess = screenReply.message;
                            functionKey = screenReply.functionKeys.Trim();

                            if (errMess.Trim() != "")
                            {
                                throw new PXException(errMess.Trim());
                            }

                            if (screenName == "MSM906A" && functionKey.Contains("XMIT-Validate"))
                            {
                                ScreenNameValueDTO[] fieldsDetailSec = new ScreenNameValueDTO[screenReply.screenFields.Length];
                                for (int i = 0; i < screenReply.screenFields.Length; i++)
                                {
                                    fieldsDetailSec[i] = new ScreenNameValueDTO();
                                    fieldsDetailSec[i].fieldName = screenReply.screenFields[i].fieldName;
                                    fieldsDetailSec[i].value = screenReply.screenFields[i].value;
                                }

                                submitRequest.screenFields = fieldsDetailSec;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenOperationContext, submitRequest);

                                screenName = screenReply.mapName;
                                errMess = screenReply.message;
                                functionKey = screenReply.functionKeys.Trim();

                                if (errMess.Trim() != "")
                                {
                                    throw new PXException(errMess.Trim());
                                }
                            }
                        }
                        screenService.positionToMenu(screenOperationContext);
                    }
                }
                catch (Exception ex)
                {
                    throw new PXException(ex.Message);
                }
            }
            return result;
        }

        string createB2BJournal(SOShipment shiporder, SOShipLine shipLine, string DBName,
            string sOLineSort, string glCurrentPeriod, string sONumber, string invtCOGSAcct, string invtCOGSSub, string invtSalesAcct, string invtSalesSub, string purchaseMethod,
            string vendorCD, string pONumber, string shipmentDate, string custSalesSubCD, string receivedValue, string receiptAccount, string custDiscAcctCD, string strSOShipExtAmt,
            string memoAmount, string strSOShipCuryExtAmt, int sOLineNumber,
            decimal shippedQty, decimal sOCuryUnitPrice, decimal sOUnitPrice, decimal sOShipCuryExtAmt, decimal sOShipExtAmt)
        {
            string result = "";

            bool loggedIn = false;
            string screenName = string.Empty;
            string errMess = string.Empty;
            string functionKey = string.Empty;

            DateTime shipmentDate1 = DateTime.ParseExact(shipmentDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime todaysDate = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            if (shipmentDate1 > todaysDate)
            {
                shipmentDate = DateTime.Now.ToString("yyyyMMdd");
            }

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                ClientConversation.authenticate("ADMIN", "");
                loggedIn = true;
            }
            catch (Exception ex)
            {
                loggedIn = false;
                throw new PXException(ex.Message);
            }

            if (loggedIn)
            {
                try
                {
                    ClientConversation.authenticate("ADMIN", "");
                    ScreenService screenService = new ScreenService()
                    {
                        Timeout = 3600000
                    };
                    OperationContext screenOperationContext = new OperationContext()
                    {
                        district = "SC01",
                        position = "INTPO",
                        maxInstances = 1
                    };
                    screenReply = screenService.executeScreen(screenOperationContext, "MSO905");
                    screenName = screenReply.mapName;

                    if (screenName != "MSM905A")
                    {
                        throw new PXException(CustomMessage.NotMSM905A);
                    }


                    string curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                    string foreignInd = curyID == "IDR" ? string.Empty : "Y";
                    string dOLine = Right($"000{sOLineSort}", 3);

                    string manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                    string currentManjnlVchr = getExistingJournal(manJnlNbr);
                    
                    bool proceed = false;
                    int a = 0;
                    if (currentManjnlVchr != string.Empty)
                    {
                        do
                        {
                            a++;
                            manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}-{a.ToString()}";
                            currentManjnlVchr = getExistingJournal(manJnlNbr);
                            if (currentManjnlVchr == string.Empty)
                            {
                                proceed = true;
                            }
                        } while (proceed == false);
                    }

                    ScreenNameValueDTO[] fieldsMSM905A = new ScreenNameValueDTO[5];
                    fieldsMSM905A[0] = new ScreenNameValueDTO();
                    fieldsMSM905A[0].fieldName = "OPTION1I";
                    fieldsMSM905A[0].value = "3";

                    fieldsMSM905A[1] = new ScreenNameValueDTO();
                    fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                    fieldsMSM905A[1].value = manJnlNbr;

                    fieldsMSM905A[2] = new ScreenNameValueDTO();
                    fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                    fieldsMSM905A[2].value = glCurrentPeriod;

                    fieldsMSM905A[3] = new ScreenNameValueDTO();
                    fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                    fieldsMSM905A[3].value = "N";

                    fieldsMSM905A[4] = new ScreenNameValueDTO();
                    fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                    fieldsMSM905A[4].value = string.Empty;

                    submitRequest.screenFields = fieldsMSM905A;
                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenOperationContext, submitRequest);

                    screenName = screenReply.mapName.Trim();
                    errMess = screenReply.message.Trim();
                    functionKey = screenReply.functionKeys.Trim();

                    if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                    {
                        throw new PXException(errMess.Trim());
                    }

                    if (errMess.Trim().Contains("JOURNAL ALREADY EXISTS"))
                    {
                        manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                        curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                        foreignInd = curyID == "IDR" ? string.Empty : "Y";

                        fieldsMSM905A = new ScreenNameValueDTO[5];
                        fieldsMSM905A[0] = new ScreenNameValueDTO();
                        fieldsMSM905A[0].fieldName = "OPTION1I";
                        fieldsMSM905A[0].value = "3";

                        fieldsMSM905A[1] = new ScreenNameValueDTO();
                        fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                        fieldsMSM905A[1].value = manJnlNbr;

                        fieldsMSM905A[2] = new ScreenNameValueDTO();
                        fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                        fieldsMSM905A[2].value = glCurrentPeriod;

                        fieldsMSM905A[3] = new ScreenNameValueDTO();
                        fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                        fieldsMSM905A[3].value = "N";

                        fieldsMSM905A[4] = new ScreenNameValueDTO();
                        fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                        fieldsMSM905A[4].value = string.Empty;

                        submitRequest.screenFields = fieldsMSM905A;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }

                    if (screenName == "MSM906A")
                    {
                        ScreenNameValueDTO[] fieldsDetail = new ScreenNameValueDTO[screenReply.screenFields.Length];
                        for (int i = 0; i < screenReply.screenFields.Length; i++)
                        {
                            fieldsDetail[i] = new ScreenNameValueDTO();
                            fieldsDetail[i].fieldName = screenReply.screenFields[i].fieldName;
                            fieldsDetail[i].value = screenReply.screenFields[i].value;

                            switch (screenReply.screenFields[i].fieldName)
                            {
                                case "JOURNAL_DESC1I":
                                    fieldsDetail[i].value = $"Shipment DO-{sONumber} item ke-{sOLineNumber}";
                                    break;
                                case "ACCOUNTANT1I":
                                    fieldsDetail[i].value = "ADMIN";
                                    break;
                                case "JNL_IND1I":
                                    fieldsDetail[i].value = string.Empty;
                                    break;
                                case "TRANS_DATE1I":
                                    fieldsDetail[i].value = shipmentDate;
                                    break;
                                case "HDR_AUTH_BY1I":
                                    fieldsDetail[i].value = "8208018JA";
                                    break;
                                case "JNL_DESC1I1":
                                    fieldsDetail[i].value = $"HPP {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I1":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I1":
                                    fieldsDetail[i].value = custSalesSubCD.Trim().Substring(0, custSalesSubCD.Trim().Length - 4) + invtCOGSSub.Trim().Substring(11) + invtCOGSAcct.Trim();
                                    break;
                                case "TRAN_AMOUNT1I1":
                                    fieldsDetail[i].value = receivedValue;
                                    break;
                                case "WORK_PROJ1I1":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I1":
                                    fieldsDetail[i].value = "P";
                                    break;
                                case "JNL_DESC1I2":
                                    fieldsDetail[i].value = $"PRSD {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I2":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I2":
                                    fieldsDetail[i].value = receiptAccount.Trim();
                                    break;
                                case "TRAN_AMOUNT1I2":
                                    fieldsDetail[i].value = $"-{receivedValue}";
                                    break;
                                case "WORK_PROJ1I2":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I2":
                                    fieldsDetail[i].value = "P";
                                    break;
                                case "JNL_DESC1I3":
                                    fieldsDetail[i].value = $"ARP {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I3":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I3":
                                    fieldsDetail[i].value = custDiscAcctCD;
                                    break;
                                case "TRAN_AMOUNT1I3":
                                    fieldsDetail[i].value = strSOShipExtAmt;
                                    break;
                                case "WORK_PROJ1I3":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I3":
                                    fieldsDetail[i].value = "P";
                                    break;
                                case "JNL_DESC1I4":
                                    fieldsDetail[i].value = $"SALES {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I4":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I4":
                                    fieldsDetail[i].value = custSalesSubCD.Trim().Substring(0, custSalesSubCD.Trim().Length - 4) + invtSalesSub.Trim().Substring(11) + invtSalesAcct.Trim();
                                    break;
                                case "TRAN_AMOUNT1I4":
                                    fieldsDetail[i].value = $"-{strSOShipExtAmt}";
                                    break;
                                case "WORK_PROJ1I4":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I4":
                                    fieldsDetail[i].value = "P";
                                    break;
                            }
                        }

                        submitRequest.screenFields = fieldsDetail;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "")
                        {
                            throw new PXException(errMess.Trim());
                        }

                        if (functionKey.Contains("XMIT-Confirm"))
                        {
                            ScreenNameValueDTO[] fieldsDetailFirst = new ScreenNameValueDTO[screenReply.screenFields.Length];
                            for (int i = 0; i < screenReply.screenFields.Length; i++)
                            {
                                fieldsDetailFirst[i] = new ScreenNameValueDTO();
                                fieldsDetailFirst[i].fieldName = screenReply.screenFields[i].fieldName;
                                fieldsDetailFirst[i].value = screenReply.screenFields[i].value;
                            }

                            submitRequest.screenFields = fieldsDetailFirst;
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenOperationContext, submitRequest);

                            screenName = screenReply.mapName;
                            errMess = screenReply.message;
                            functionKey = screenReply.functionKeys.Trim();

                            if (errMess.Trim() != "")
                            {
                                throw new PXException(errMess.Trim());
                            }

                            if (screenName == "MSM906A" && functionKey.Contains("XMIT-Validate"))
                            {
                                ScreenNameValueDTO[] fieldsDetailSec = new ScreenNameValueDTO[screenReply.screenFields.Length];
                                for (int i = 0; i < screenReply.screenFields.Length; i++)
                                {
                                    fieldsDetailSec[i] = new ScreenNameValueDTO();
                                    fieldsDetailSec[i].fieldName = screenReply.screenFields[i].fieldName;
                                    fieldsDetailSec[i].value = screenReply.screenFields[i].value;
                                }

                                submitRequest.screenFields = fieldsDetailSec;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenOperationContext, submitRequest);

                                screenName = screenReply.mapName;
                                errMess = screenReply.message;
                                functionKey = screenReply.functionKeys.Trim();

                                if (errMess.Trim() != "")
                                {
                                    throw new PXException(errMess.Trim());
                                }
                            }
                        }
                        screenService.positionToMenu(screenOperationContext);
                    }
                }
                catch (Exception ex)
                {
                    throw new PXException(ex.Message);
                }
            }
            return result;
        }

        string reverseROKJournal(SOShipment shiporder, SOShipLine shipLine, string DBName,
            string sOLineSort, string glCurrentPeriod, string sONumber, string invtCOGSAcct, string invtCOGSSub, string invtSalesAcct, string invtSalesSub, string purchaseMethod,
            string vendorCD, string pONumber, string shipmentDate, string custSalesSubCD, string receivedValue, string receiptAccount, string custDiscAcctCD, string strSOShipExtAmt,
            string memoAmount, string strSOShipCuryExtAmt, int sOLineNumber,
            decimal shippedQty, decimal sOCuryUnitPrice, decimal sOUnitPrice, decimal sOShipCuryExtAmt, decimal sOShipExtAmt)
        {
            string result = "";

            bool loggedIn = false;
            string screenName = string.Empty;
            string errMess = string.Empty;
            string functionKey = string.Empty;

            DateTime shipmentDate1 = DateTime.ParseExact(shipmentDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime todaysDate = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            if (shipmentDate1 > todaysDate)
            {
                shipmentDate = DateTime.Now.ToString("yyyyMMdd");
            }

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                ClientConversation.authenticate("ADMIN", "");
                loggedIn = true;
            }
            catch (Exception ex)
            {
                loggedIn = false;
                throw new PXException(ex.Message);
            }

            if (loggedIn)
            {
                try
                {
                    ClientConversation.authenticate("ADMIN", "");
                    ScreenService screenService = new ScreenService()
                    {
                        Timeout = 3600000
                    };
                    OperationContext screenOperationContext = new OperationContext()
                    {
                        district = "SC01",
                        position = "INTPO",
                        maxInstances = 1
                    };
                    screenReply = screenService.executeScreen(screenOperationContext, "MSO905");
                    screenName = screenReply.mapName;

                    if (screenName != "MSM905A")
                    {
                        throw new PXException(CustomMessage.NotMSM905A);
                    }

                    string curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                    string foreignInd = curyID == "IDR" ? string.Empty : "Y";
                    string dOLine = Right($"000{sOLineSort}", 3);

                    string manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                    string currentManjnlVchr = getExistingJournal(manJnlNbr);

                    bool proceed = false;
                    int a = 0;
                    if (currentManjnlVchr != string.Empty)
                    {
                        do
                        {
                            a++;
                            manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}-{a.ToString()}";
                            currentManjnlVchr = getExistingJournal(manJnlNbr);
                            if (currentManjnlVchr == string.Empty)
                            {
                                proceed = true;
                            }
                        } while (proceed == false);
                    }

                    ScreenNameValueDTO[] fieldsMSM905A = new ScreenNameValueDTO[5];
                    fieldsMSM905A[0] = new ScreenNameValueDTO();
                    fieldsMSM905A[0].fieldName = "OPTION1I";
                    fieldsMSM905A[0].value = "3";

                    fieldsMSM905A[1] = new ScreenNameValueDTO();
                    fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                    fieldsMSM905A[1].value = manJnlNbr;

                    fieldsMSM905A[2] = new ScreenNameValueDTO();
                    fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                    fieldsMSM905A[2].value = glCurrentPeriod;

                    fieldsMSM905A[3] = new ScreenNameValueDTO();
                    fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                    fieldsMSM905A[3].value = "N";

                    fieldsMSM905A[4] = new ScreenNameValueDTO();
                    fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                    fieldsMSM905A[4].value = string.Empty;

                    submitRequest.screenFields = fieldsMSM905A;
                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenOperationContext, submitRequest);

                    screenName = screenReply.mapName.Trim();
                    errMess = screenReply.message.Trim();
                    functionKey = screenReply.functionKeys.Trim();

                    if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                    {
                        throw new PXException(errMess.Trim());
                    }

                    if (errMess.Trim().Contains("JOURNAL ALREADY EXISTS"))
                    {
                        manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                        curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                        foreignInd = curyID == "IDR" ? string.Empty : "Y";

                        fieldsMSM905A = new ScreenNameValueDTO[5];
                        fieldsMSM905A[0] = new ScreenNameValueDTO();
                        fieldsMSM905A[0].fieldName = "OPTION1I";
                        fieldsMSM905A[0].value = "3";

                        fieldsMSM905A[1] = new ScreenNameValueDTO();
                        fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                        fieldsMSM905A[1].value = manJnlNbr;

                        fieldsMSM905A[2] = new ScreenNameValueDTO();
                        fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                        fieldsMSM905A[2].value = glCurrentPeriod;

                        fieldsMSM905A[3] = new ScreenNameValueDTO();
                        fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                        fieldsMSM905A[3].value = "N";

                        fieldsMSM905A[4] = new ScreenNameValueDTO();
                        fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                        fieldsMSM905A[4].value = string.Empty;

                        submitRequest.screenFields = fieldsMSM905A;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }

                    if (screenName == "MSM906A")
                    {
                        ScreenNameValueDTO[] fieldsDetail = new ScreenNameValueDTO[screenReply.screenFields.Length];

                        for (int i = 0; i < screenReply.screenFields.Length; i++)
                        {
                            fieldsDetail[i] = new ScreenNameValueDTO();
                            fieldsDetail[i].fieldName = screenReply.screenFields[i].fieldName;
                            fieldsDetail[i].value = screenReply.screenFields[i].value;

                            if (screenReply.screenFields[i].fieldName == "JOURNAL_DESC1I")
                            {
                                fieldsDetail[i].value = $"Shipment DO-{sONumber} item ke-{sOLineNumber}";
                            }

                            if (screenReply.screenFields[i].fieldName == "ACCOUNTANT1I")
                            {
                                fieldsDetail[i].value = "ADMIN";
                            }

                            if (screenReply.screenFields[i].fieldName == "JNL_IND1I")
                            {
                                fieldsDetail[i].value = string.Empty;
                            }

                            if (screenReply.screenFields[i].fieldName == "TRANS_DATE1I")
                            {
                                fieldsDetail[i].value = shipmentDate;
                            }

                            if (screenReply.screenFields[i].fieldName == "HDR_AUTH_BY1I")
                            {
                                fieldsDetail[i].value = "8208018JA";
                            }

                            if (screenReply.screenFields[i].fieldName == "JNL_DESC1I1")
                            {
                                fieldsDetail[i].value = $"HPP {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                            }

                            if (screenReply.screenFields[i].fieldName == "DSTRCT_CODE1I1")
                            {
                                fieldsDetail[i].value = "SC01";
                            }

                            if (screenReply.screenFields[i].fieldName == "ACCOUNT_CODE1I1")
                            {
                                fieldsDetail[i].value = custSalesSubCD.Trim().Substring(0, custSalesSubCD.Trim().Length - 4) + invtCOGSSub.Trim().Substring(11) + invtCOGSAcct.Trim();
                            }

                            if (screenReply.screenFields[i].fieldName == "TRAN_AMOUNT1I1")
                            {
                                fieldsDetail[i].value = $"-{receivedValue}";
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ1I1")
                            {
                                fieldsDetail[i].value = sONumber;
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ_IND1I1")
                            {
                                fieldsDetail[i].value = "P";
                            }

                            if (screenReply.screenFields[i].fieldName == "JNL_DESC1I2")
                            {
                                fieldsDetail[i].value = $"PRSD {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                            }

                            if (screenReply.screenFields[i].fieldName == "DSTRCT_CODE1I2")
                            {
                                fieldsDetail[i].value = "SC01";
                            }

                            if (screenReply.screenFields[i].fieldName == "ACCOUNT_CODE1I2")
                            {
                                fieldsDetail[i].value = receiptAccount.Trim();
                            }

                            if (screenReply.screenFields[i].fieldName == "TRAN_AMOUNT1I2")
                            {
                                fieldsDetail[i].value = receivedValue; 
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ1I2")
                            {
                                fieldsDetail[i].value = sONumber;
                            }

                            if (screenReply.screenFields[i].fieldName == "WORK_PROJ_IND1I2")
                            {
                                fieldsDetail[i].value = "P";
                            }
                        }

                        submitRequest.screenFields = fieldsDetail;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "")
                        {
                            throw new PXException(errMess.Trim());
                        }

                        if (functionKey.Contains("XMIT-Confirm"))
                        {
                            ScreenNameValueDTO[] fieldsDetailFirst = new ScreenNameValueDTO[screenReply.screenFields.Length];
                            for (int i = 0; i < screenReply.screenFields.Length; i++)
                            {
                                fieldsDetailFirst[i] = new ScreenNameValueDTO();
                                fieldsDetailFirst[i].fieldName = screenReply.screenFields[i].fieldName;
                                fieldsDetailFirst[i].value = screenReply.screenFields[i].value;
                            }

                            submitRequest.screenFields = fieldsDetailFirst;
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenOperationContext, submitRequest);

                            screenName = screenReply.mapName;
                            errMess = screenReply.message;
                            functionKey = screenReply.functionKeys.Trim();

                            if (errMess.Trim() != "")
                            {
                                throw new PXException(errMess.Trim());
                            }

                            if (screenName == "MSM906A" && functionKey.Contains("XMIT-Validate"))
                            {
                                ScreenNameValueDTO[] fieldsDetailSec = new ScreenNameValueDTO[screenReply.screenFields.Length];
                                for (int i = 0; i < screenReply.screenFields.Length; i++)
                                {
                                    fieldsDetailSec[i] = new ScreenNameValueDTO();
                                    fieldsDetailSec[i].fieldName = screenReply.screenFields[i].fieldName;
                                    fieldsDetailSec[i].value = screenReply.screenFields[i].value;
                                }

                                submitRequest.screenFields = fieldsDetailSec;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenOperationContext, submitRequest);

                                screenName = screenReply.mapName;
                                errMess = screenReply.message;
                                functionKey = screenReply.functionKeys.Trim();

                                if (errMess.Trim() != "")
                                {
                                    throw new PXException(errMess.Trim());
                                }
                            }
                        }
                        screenService.positionToMenu(screenOperationContext);
                    }
                }
                catch (Exception ex)
                {
                    throw new PXException(ex.Message);
                }
            }
            return result;
        }

        string reverseB2BJournal(SOShipment shiporder, SOShipLine shipLine, string DBName,
            string sOLineSort, string glCurrentPeriod, string sONumber, string invtCOGSAcct, string invtCOGSSub, string invtSalesAcct, string invtSalesSub, string purchaseMethod,
            string vendorCD, string pONumber, string shipmentDate, string custSalesSubCD, string receivedValue, string receiptAccount, string custDiscAcctCD, string strSOShipExtAmt,
            string memoAmount, string strSOShipCuryExtAmt, int sOLineNumber,
            decimal shippedQty, decimal sOCuryUnitPrice, decimal sOUnitPrice, decimal sOShipCuryExtAmt, decimal sOShipExtAmt)
        {
            string result = "";

            bool loggedIn = false;
            string screenName = string.Empty;
            string errMess = string.Empty;
            string functionKey = string.Empty;

            DateTime shipmentDate1 = DateTime.ParseExact(shipmentDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime todaysDate = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            if (shipmentDate1 > todaysDate)
            {
                shipmentDate = DateTime.Now.ToString("yyyyMMdd");
            }

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                ClientConversation.authenticate("ADMIN", "");
                loggedIn = true;
            }
            catch (Exception ex)
            {
                loggedIn = false;
                throw new PXException(ex.Message);
            }

            if (loggedIn)
            {
                try
                {
                    ClientConversation.authenticate("ADMIN", "");
                    ScreenService screenService = new ScreenService()
                    {
                        Timeout = 3600000
                    };
                    OperationContext screenOperationContext = new OperationContext()
                    {
                        district = "SC01",
                        position = "INTPO",
                        maxInstances = 1
                    };
                    screenReply = screenService.executeScreen(screenOperationContext, "MSO905");
                    screenName = screenReply.mapName;

                    if (screenName != "MSM905A")
                    {
                        throw new PXException(CustomMessage.NotMSM905A);
                    }


                    string curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                    string foreignInd = curyID == "IDR" ? string.Empty : "Y";
                    string dOLine = Right($"000{sOLineSort}", 3);

                    string manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}R";
                    string currentManjnlVchr = getExistingJournal(manJnlNbr);
                    bool proceed = false;
                    int a = 0;
                    if (currentManjnlVchr != string.Empty)
                    {
                        do
                        {
                            a++;
                            manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}R{a.ToString()}";
                            currentManjnlVchr = getExistingJournal(manJnlNbr);
                            if (currentManjnlVchr == string.Empty)
                            {
                                proceed = true;
                            }
                        } while (proceed == false);
                    }
                    
                    ScreenNameValueDTO[] fieldsMSM905A = new ScreenNameValueDTO[5];
                    fieldsMSM905A[0] = new ScreenNameValueDTO();
                    fieldsMSM905A[0].fieldName = "OPTION1I";
                    fieldsMSM905A[0].value = "3";

                    fieldsMSM905A[1] = new ScreenNameValueDTO();
                    fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                    fieldsMSM905A[1].value = manJnlNbr;

                    fieldsMSM905A[2] = new ScreenNameValueDTO();
                    fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                    fieldsMSM905A[2].value = glCurrentPeriod;

                    fieldsMSM905A[3] = new ScreenNameValueDTO();
                    fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                    fieldsMSM905A[3].value = "N";

                    fieldsMSM905A[4] = new ScreenNameValueDTO();
                    fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                    fieldsMSM905A[4].value = string.Empty;

                    submitRequest.screenFields = fieldsMSM905A;
                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenOperationContext, submitRequest);

                    screenName = screenReply.mapName.Trim();
                    errMess = screenReply.message.Trim();
                    functionKey = screenReply.functionKeys.Trim();

                    if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                    {
                        throw new PXException(errMess.Trim());
                    }

                    if (errMess.Trim().Contains("JOURNAL ALREADY EXISTS"))
                    {
                        manJnlNbr = $"J{shipLine.ShipmentNbr.Trim()}{sOLineSort}";
                        curyID = shiporder.CuryID != null ? shiporder.CuryID.Trim() : "IDR";
                        foreignInd = curyID == "IDR" ? string.Empty : "Y";

                        fieldsMSM905A = new ScreenNameValueDTO[5];
                        fieldsMSM905A[0] = new ScreenNameValueDTO();
                        fieldsMSM905A[0].fieldName = "OPTION1I";
                        fieldsMSM905A[0].value = "3";

                        fieldsMSM905A[1] = new ScreenNameValueDTO();
                        fieldsMSM905A[1].fieldName = "MAN_JNL_NO1I";
                        fieldsMSM905A[1].value = manJnlNbr;

                        fieldsMSM905A[2] = new ScreenNameValueDTO();
                        fieldsMSM905A[2].fieldName = "ACCT_PERIOD1I";
                        fieldsMSM905A[2].value = glCurrentPeriod;

                        fieldsMSM905A[3] = new ScreenNameValueDTO();
                        fieldsMSM905A[3].fieldName = "INTERDIST_IND1I";
                        fieldsMSM905A[3].value = "N";

                        fieldsMSM905A[4] = new ScreenNameValueDTO();
                        fieldsMSM905A[4].fieldName = "FOREIGN_IND1I";
                        fieldsMSM905A[4].value = string.Empty;

                        submitRequest.screenFields = fieldsMSM905A;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "" && !errMess.Trim().Contains("JOURNAL ALREADY EXISTS") && !functionKey.Contains("XMIT-Confirm"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }

                    if (screenName == "MSM906A")
                    {
                        ScreenNameValueDTO[] fieldsDetail = new ScreenNameValueDTO[screenReply.screenFields.Length];
                        for (int i = 0; i < screenReply.screenFields.Length; i++)
                        {
                            fieldsDetail[i] = new ScreenNameValueDTO();
                            fieldsDetail[i].fieldName = screenReply.screenFields[i].fieldName;
                            fieldsDetail[i].value = screenReply.screenFields[i].value;

                            switch (screenReply.screenFields[i].fieldName)
                            {
                                case "JOURNAL_DESC1I":
                                    fieldsDetail[i].value = $"Shipment DO-{sONumber} item ke-{sOLineNumber}";
                                    break;
                                case "ACCOUNTANT1I":
                                    fieldsDetail[i].value = "ADMIN";
                                    break;
                                case "JNL_IND1I":
                                    fieldsDetail[i].value = string.Empty;
                                    break;
                                case "TRANS_DATE1I":
                                    fieldsDetail[i].value = shipmentDate;
                                    break;
                                case "HDR_AUTH_BY1I":
                                    fieldsDetail[i].value = "8208018JA";
                                    break;
                                case "JNL_DESC1I1":
                                    fieldsDetail[i].value = $"HPP {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I1":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I1":
                                    fieldsDetail[i].value = custSalesSubCD.Trim().Substring(0, custSalesSubCD.Trim().Length - 4) + invtCOGSSub.Trim().Substring(11) + invtCOGSAcct.Trim();
                                    break;
                                case "TRAN_AMOUNT1I1":
                                    fieldsDetail[i].value = $"-{receivedValue}";
                                    break;
                                case "WORK_PROJ1I1":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I1":
                                    fieldsDetail[i].value = "P";
                                    break;
                                case "JNL_DESC1I2":
                                    fieldsDetail[i].value = $"PRSD {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I2":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I2":
                                    fieldsDetail[i].value = receiptAccount.Trim();
                                    break;
                                case "TRAN_AMOUNT1I2":
                                    fieldsDetail[i].value = receivedValue;
                                    break;
                                case "WORK_PROJ1I2":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I2":
                                    fieldsDetail[i].value = "P";
                                    break;
                                case "JNL_DESC1I3":
                                    fieldsDetail[i].value = $"ARP {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I3":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I3":
                                    fieldsDetail[i].value = custDiscAcctCD;
                                    break;
                                case "TRAN_AMOUNT1I3":
                                    fieldsDetail[i].value = $"-{strSOShipExtAmt}";
                                    break;
                                case "WORK_PROJ1I3":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I3":
                                    fieldsDetail[i].value = "P";
                                    break;
                                case "JNL_DESC1I4":
                                    fieldsDetail[i].value = $"SALES {sONumber}, item {dOLine.Trim()}, {vendorCD}, {pONumber}";
                                    break;
                                case "DSTRCT_CODE1I4":
                                    fieldsDetail[i].value = "SC01";
                                    break;
                                case "ACCOUNT_CODE1I4":
                                    fieldsDetail[i].value = custSalesSubCD.Trim().Substring(0, custSalesSubCD.Trim().Length - 4) + invtSalesSub.Trim().Substring(11) + invtSalesAcct.Trim();
                                    break;
                                case "TRAN_AMOUNT1I4":
                                    fieldsDetail[i].value = strSOShipExtAmt;
                                    break;
                                case "WORK_PROJ1I4":
                                    fieldsDetail[i].value = sONumber;
                                    break;
                                case "WORK_PROJ_IND1I4":
                                    fieldsDetail[i].value = "P";
                                    break;
                            }
                        }

                        submitRequest.screenFields = fieldsDetail;
                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                        screenName = screenReply.mapName.Trim();
                        errMess = screenReply.message.Trim();
                        functionKey = screenReply.functionKeys.Trim();

                        if (errMess.Trim() != "")
                        {
                            throw new PXException(errMess.Trim());
                        }

                        if (functionKey.Contains("XMIT-Confirm"))
                        {
                            ScreenNameValueDTO[] fieldsDetailFirst = new ScreenNameValueDTO[screenReply.screenFields.Length];
                            for (int i = 0; i < screenReply.screenFields.Length; i++)
                            {
                                fieldsDetailFirst[i] = new ScreenNameValueDTO();
                                fieldsDetailFirst[i].fieldName = screenReply.screenFields[i].fieldName;
                                fieldsDetailFirst[i].value = screenReply.screenFields[i].value;
                            }

                            submitRequest.screenFields = fieldsDetailFirst;
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenOperationContext, submitRequest);

                            screenName = screenReply.mapName;
                            errMess = screenReply.message;
                            functionKey = screenReply.functionKeys.Trim();

                            if (errMess.Trim() != "")
                            {
                                throw new PXException(errMess.Trim());
                            }

                            if (screenName == "MSM906A" && functionKey.Contains("XMIT-Validate"))
                            {
                                ScreenNameValueDTO[] fieldsDetailSec = new ScreenNameValueDTO[screenReply.screenFields.Length];
                                for (int i = 0; i < screenReply.screenFields.Length; i++)
                                {
                                    fieldsDetailSec[i] = new ScreenNameValueDTO();
                                    fieldsDetailSec[i].fieldName = screenReply.screenFields[i].fieldName;
                                    fieldsDetailSec[i].value = screenReply.screenFields[i].value;
                                }

                                submitRequest.screenFields = fieldsDetailSec;
                                submitRequest.screenKey = "1"; // OK
                                screenReply = screenService.submit(screenOperationContext, submitRequest);

                                screenName = screenReply.mapName;
                                errMess = screenReply.message;
                                functionKey = screenReply.functionKeys.Trim();

                                if (errMess.Trim() != "")
                                {
                                    throw new PXException(errMess.Trim());
                                }
                            }
                        }
                        screenService.positionToMenu(screenOperationContext);
                    }
                }
                catch (Exception ex)
                {
                    throw new PXException(ex.Message);
                }
            }
            return result;
        }

        public string Right(string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }

        public string getCurrentPeriod(string DBName)
        {
            string result = string.Empty;
            string testDBNameLocale = DBName;
            string testdbNameGlobal = dbName;
            string query = "select * from msf000_cp where dstrct_code = 'SC01' and control_rec_no = '0010'";

            string oraDB = DBName.Trim().Contains("PRD") ? DBConnection.oraDBPrd : DBConnection.oraDBDev;

            OracleConnection con = new OracleConnection(oraDB);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = query;
            cmd.Connection = con;
            con.Open();
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    string periodMonth = dr["CURR_ACCT_MN"].ToString();
                    string periodYear = dr["CURR_ACCT_YR"].ToString();
                    result = $"{periodMonth.Trim()}{periodYear.Trim()}";
                }
            } else
            {
                result = "no data";
            }
            con.Close();
            return result;
        }

        public string getReceiptValue(string pONumber, string pOItem, string DBName, out string memoAmount, out string currencyType, out string receiptAccount)
        {
            string result = string.Empty;

            string testDBNameLocale = DBName;
            string testdbNameGlobal = dbName;

            string tranAmount = string.Empty;
            string tranGroupKey = string.Empty;

            string oraDB = DBName.Trim().Contains("PRD") ? DBConnection.oraDBPrd : DBConnection.oraDBDev;

            memoAmount = "0";
            currencyType = "IDR";
            receiptAccount = string.Empty;

            try
            {
                string queryTGK = $"select max(tran_group_key) tran_group_key from msf900 where dstrct_code = 'SC01' and po_no = '{pONumber}' and po_item = '{pOItem}' and rec900_type = 'P'";
                OracleConnection con1 = new OracleConnection(oraDB);
                OracleCommand cmd1 = new OracleCommand();
                cmd1.CommandText = queryTGK;
                cmd1.Connection = con1;
                con1.Open();
                OracleDataReader dr1 = cmd1.ExecuteReader();
                if (dr1.HasRows)
                {
                    while (dr1.Read())
                    {
                        tranGroupKey = dr1["TRAN_GROUP_KEY"].ToString();
                    }
                }
                con1.Close();
            } catch (Exception ex1)
            {
                result = ex1.Message.Trim();
            }

            try
            {
                string query = $"select tran_amount, memo_amount, currency_type, account_code from msf900 where dstrct_code = 'SC01' and po_no = '{pONumber}' and po_item = '{pOItem}' and rec900_type = 'P' and tran_group_key = '{tranGroupKey}'";
                OracleConnection con = new OracleConnection(oraDB);
                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query;
                cmd.Connection = con;
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        tranAmount = dr["TRAN_AMOUNT"].ToString();
                        memoAmount = dr["MEMO_AMOUNT"].ToString();
                        currencyType = dr["CURRENCY_TYPE"].ToString().Trim();
                        receiptAccount = dr["ACCOUNT_CODE"].ToString().Trim();
                        result = tranAmount;
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message.Trim();
            }
            return result;
        }

        public string RunQuery(string query, string DBName)
        {
            string result = "";

            string testDBNameLocale = DBName;
            string testdbNameGlobal = dbName;

            string oraDB = DBName.Trim().Contains("PRD") ? DBConnection.oraDBPrd : DBConnection.oraDBDev;

            OracleConnection con = new OracleConnection(oraDB);
            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = query;
            cmd.Connection = con;
            con.Open();
            OracleDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                result = "ada";
            }
            con.Close();

            return result;
        }

        public string getExistingJournal(string manjnlVchr)
        {
            string result = string.Empty;

            string oraDB = dbName.Trim().Contains("PRD") ? DBConnection.oraDBPrd : DBConnection.oraDBDev;

            try
            {
                string query = $"select distinct manjnl_vchr from msf900 where dstrct_code = 'SC01' and manjnl_vchr = '{manjnlVchr}'";
                OracleConnection con = new OracleConnection(oraDB);
                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query;
                cmd.Connection = con;
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result = dr["MANJNL_VCHR"].ToString().Trim();
                    }
                }
                else
                {
                    result = string.Empty;
                }
                con.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message.Trim();
            }
            
            return result;
        }

        public string getPurchaseMethod(string sONumber, int sOLineNumber, decimal shippedQty, 
            out decimal sOCuryUnitPrice, out decimal sOUnitPrice, out decimal sOShipCuryExtAmt, out decimal sOShipExtAmt, 
            out string strSOShipExtAmt, out string strSOShipCuryExtAmt, out string soLineSort, out string requisitionNbr, out string sOLineCuryExtPrice, out string sOLineExtPrice,
            out int customerCode, out int customerLocID)
        {
            string result = string.Empty;

            SOOrder sOOrder = PXSelect<SOOrder,
                Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>.Select(Base, sONumber);

            SOOrderExt sOOrderExt = sOOrder.GetExtension<SOOrderExt>();

            string purchaseMethod = sOOrderExt.UsrPurchMethod == 0 ? "B2B" : sOOrderExt.UsrPurchMethod == 1 ? "ROK" : "GA";
            result =purchaseMethod;

            SOLine sOLine = PXSelect<SOLine,
                Where<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>.Select(Base, sONumber, sOLineNumber);

            SOLineExt sOLineExt = sOLine.GetExtension<SOLineExt>();

            sOCuryUnitPrice = sOLine.CuryUnitPrice ?? 0;
            sOUnitPrice = sOLine.UnitPrice ?? 0;
            sOShipCuryExtAmt = Math.Round(shippedQty * sOCuryUnitPrice, 2);
            sOShipExtAmt = Math.Round(shippedQty * sOUnitPrice, 2);

            strSOShipExtAmt = sOShipExtAmt.ToString();
            strSOShipCuryExtAmt = sOShipCuryExtAmt.ToString();
            soLineSort = (sOLine.SortOrder ?? 0).ToString();
            requisitionNbr = sOLineExt != null ? sOLineExt.UsrReqNbr.Trim() : string.Empty;

            customerCode = sOOrder.CustomerID ?? 0;
            customerLocID = sOOrder.CustomerLocationID ?? 0;
            sOLineCuryExtPrice = Math.Round(sOLine.CuryExtPrice ?? 0, 2).ToString();
            sOLineExtPrice = Math.Round(sOLine.ExtPrice ?? 0, 2).ToString();

            return result;
        }

        public string getInventoryInfo(int inventoryIdentification,
            out string invtCOGSAcct, out string invtCOGSSub, out string invtSalesAcct, out string invtSalesSub)
        {
            string result = string.Empty;

            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, inventoryIdentification);

            Account invtAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, inventoryItem.COGSAcctID ?? 0);
            Sub invtSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, inventoryItem.COGSSubID ?? 0);

            Account invtSalesAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, inventoryItem.SalesAcctID ?? 0);
            Sub invtSalesSubaccount = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, inventoryItem.SalesSubID ?? 0);

            string inventoryCode = inventoryItem != null ? inventoryItem.InventoryCD.Trim() : string.Empty;
            result = inventoryCode;

            int inventoryCOGSAcct = inventoryItem.COGSAcctID ?? 0;
            int inventoryCOGSSub = inventoryItem.COGSSubID ?? 0;
            int inventorySalesAcct = inventoryItem.SalesAcctID ?? 0;
            int inventorySalesSub = inventoryItem.SalesSubID ?? 0;

            invtCOGSAcct = invtAccount != null ? invtAccount.AccountCD.Trim() : string.Empty;
            invtCOGSSub = invtSub != null ? invtSub.SubCD.Trim() : string.Empty;
            invtSalesAcct = invtSalesAccount != null ? invtSalesAccount.AccountCD.Trim() : string.Empty;
            invtSalesSub = invtSalesSubaccount != null ? invtSalesSubaccount.SubCD.Trim() : string.Empty;

            return result;
        }

        public string getPOInfo(string requisitionNbr, out string pONumber, out int supplierCode, out int supplierLocID)
        {
            string result = string.Empty;

            RQRequisitionOrder rQRequisitionOrder = PXSelect<RQRequisitionOrder,
                Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
                And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.po>>>>.Select(Base, requisitionNbr);

            pONumber = rQRequisitionOrder.OrderNbr != null ? rQRequisitionOrder.OrderNbr.Trim() : string.Empty;

            POOrder pOOrder = PXSelect<POOrder,
                Where<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>,
                And<POOrder.orderType, Equal<POOrderType.regularOrder>>>>.Select(Base, pONumber);

            POLine pOline = PXSelect<POLine,
                Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
                And<POLine.orderType, Equal<POOrderType.regularOrder>>>>.Select(Base, pOOrder.OrderNbr);

            string pOLineNbr = Right($"000{(pOline.SortOrder ?? 0).ToString()}", 3);
            result = pOLineNbr;
            supplierCode = pOOrder.VendorID ?? 0;
            supplierLocID = pOOrder.VendorLocationID ?? 0;

            return result;
        }

        public string getCustomerAccount(int customerCode, int customerLocationCode, out string custSalesAcctCD, out string custDiscAcctCD, out string custSalesSubCD)
        {
            string result = "";

            BAccount2 customer = PXSelect<BAccount2,
                    Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                    And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                    And<BAccount.type, Equal<Required<BAccount.type>>>>>>.Select(Base, customerCode, customerLocationCode, "CU");

            Location custLocation = PXSelect<Location,
                Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(Base, customerCode, customerLocationCode);

            int customerSalesAcctId = custLocation.CSalesAcctID ?? 0;
            int customerDiscAcctId = custLocation.CDiscountAcctID ?? 0;
            int customerSalesSubId = custLocation.CSalesSubID ?? 0;

            Account custSalesAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, customerSalesAcctId);
            Account custDiscAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, customerDiscAcctId);
            Sub custSalesSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, customerSalesSubId);

            custSalesAcctCD = custSalesAccount != null ? custSalesAccount.AccountCD.Trim() : string.Empty;
            custDiscAcctCD = custDiscAccount != null ? custDiscAccount.AccountCD.Trim() : string.Empty;
            custSalesSubCD = custSalesSub != null ? custSalesSub.SubCD : string.Empty;

            result = customer.AcctCD != null ? customer.AcctCD.Trim() : string.Empty;
            return result;
        }

        public string getSupplierCode(int supplierCode, int supplierLocationCode)
        {
            string result = "";

            BAccount2 vendor = PXSelect<BAccount2,
                    Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                    And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                    And<BAccount.type, Equal<Required<BAccount.type>>>>>>.Select(Base, supplierCode, supplierLocationCode, "VE");

            Location vendLocation = PXSelect<Location,
                Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(Base, supplierCode, supplierLocationCode);

            result = vendor.AcctCD != null ? vendor.AcctCD.Trim() : string.Empty;

            return result;
        }
    }
}