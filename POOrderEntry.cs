using PX.Data;
using System;
using PLNSC;
using PLNSC.ScreenService;
using PLNSC.ProjectService;
using EllipseWebServicesClient;
using System.Net;
using System.Collections.Generic;
using System.Collections;

using PX.Objects.SO;
using PX.Objects.RQ;

namespace PX.Objects.PO
{
    public class POOrderEntry_Extension : PXGraphExtension<POOrderEntry>
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
        [PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void POOrder_OrderDesc_CacheAttached(PXCache cache)
        {
        }

        protected void POOrder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var row = (POOrder)e.Row;

            string dateTime = DateTime.Now.ToString();
            string createddate = Convert.ToDateTime(dateTime).ToString("yyyy-MM-dd");
            DateTime dt = DateTime.ParseExact(createddate, "yyyy-MM-dd", null);

            if (row.ExpectedDate < dt)
            {
                cache.RaiseExceptionHandling<POOrder.expectedDate>(e.Row, row.ExpectedDate, new PXSetPropertyException(CustomMessage.DateGreaterToday));
            }
        }

        protected void POOrder_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            POOrder pOOrder = (POOrder)e.Row;
            if (pOOrder.Status == POOrderStatus.Open)
            {
                string poNbr = pOOrder.OrderNbr;
                bool isPOExists = reviewPO(pOOrder);

                if (isPOExists)
                {
                    foreach (POLine pOLine in PXSelect<POLine, Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>>>.Select(Base, pOOrder.OrderNbr))
                    {
                        modifyPO(pOOrder, pOLine);
                    }
                }
                else
                {
                    foreach (POLine pOLine in PXSelect<POLine, Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>>>.Select(Base, pOOrder.OrderNbr))
                    {
                        int? pOLineNbr = pOLine.POLineNbr;
                        int? lineNbr = pOLine.LineNbr;
                        createPO(pOOrder, pOLine);
                    }
                }
            }
        }
        #endregion

        bool reviewPO(POOrder pOOrder)
        {
            bool result = false;
            bool loggedIn = false;
            string screenName = "";
            string errorMessage = "";

            PLNSC.ScreenService.OperationContext screenContext = new PLNSC.ScreenService.OperationContext()
            {
                district = districtCode,
                position = positionID,
                maxInstances = 1
            };

            ScreenService screenService = new ScreenService()
            {
                Timeout = sessionTimeout,
                Url = $"{urlPrefix(dbName)}ScreenService"
            };
            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                ClientConversation.authenticate(userName, password);
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
                    ClientConversation.authenticate(userName, password);
                    screenReply = screenService.executeScreen(screenContext, "MSO220");
                    screenName = screenReply.mapName;

                    if (screenName != "MSM220A")
                    {
                        throw new PXException(CustomMessage.NotMSM221A);
                    }

                    ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                    fields[0].fieldName = "OPTION1I";
                    fields[0].value = "1";
                    fields[1].fieldName = "PO_NO1I";
                    fields[1].value = pOOrder.OrderNbr.Trim();

                    submitRequest.screenFields = fields;

                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenContext, submitRequest);
                    screenName = screenReply.mapName;
                    errorMessage = screenReply.message;

                    if (errorMessage.Trim().Contains("PURCHASE ORDER DOES NOT EXIST"))
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                    screenService.positionToMenu(screenContext);
                }
                catch (Exception ex)
                {
                    result = false;
                    throw new PXException(ex.Message);
                }
            }
            return result;
        }

        static class SSContextClass
        {
            private static PLNSC.ScreenService.OperationContext c = new PLNSC.ScreenService.OperationContext();
            public static PLNSC.ScreenService.OperationContext context
            {
                get { return c; }
                set { c = value; }
            }
        }

        public virtual IEnumerable createPO(POOrder pOOrder, POLine pOLine)
        {
            RQRequisitionOrder rQRequisitionOrder = PXSelect<RQRequisitionOrder, 
                Where<RQRequisitionOrder.orderNbr, Equal<Required<RQRequisitionOrder.orderNbr>>,
                And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.po>>>>.Select(Base, pOOrder.OrderNbr);

            RQRequisition rQRequisition = PXSelect<RQRequisition, Where<RQRequisition.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>>>.Select(Base, rQRequisitionOrder.ReqNbr);
            RQRequisitionExt rQRequisitionExt = rQRequisition.GetExtension<RQRequisitionExt>();

            int purchaseMethod = rQRequisition != null ? rQRequisitionExt.UsrPurchMethod ?? 2 : 2;
            string nonCoreDO = rQRequisition != null ? rQRequisitionExt.UsrDONbr != null ? rQRequisitionExt.UsrDONbr.Trim() : string.Empty : string.Empty;
            string modifyPRResult = string.Empty;

            if (purchaseMethod != 2)
            {
                modifyPRResult = modifyPOCostAlloc(pOOrder, pOLine, purchaseMethod, nonCoreDO);
                if (!modifyPRResult.Trim().Contains("OK")) return modifyPRResult.Trim();
            }

            PLNSC.ScreenService.OperationContext screenContext = new PLNSC.ScreenService.OperationContext()
            {
                district = "SC01",
                position = "INTPO",
                maxInstances = 1
            };

            ScreenService screenService = new ScreenService()
            {
                Timeout = sessionTimeout,
                Url = $"{urlPrefix(dbName)}ScreenService"
            };
            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            bool loggedIn = false;
            bool modifyFlag = false;
            string screenName = "";
            string errMess = "";
            string currentCursor = "";

            try
            {
                ClientConversation.authenticate(userName, password);
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
                    screenReply = screenService.executeScreen(screenContext, "MSO230");
                    screenName = screenReply.mapName;
                    if (screenName != "MSM230A")
                    {
                        throw new PXException(CustomMessage.NotMSO230);
                    }
                    ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                    fields[0].fieldName = "OPTION1I";
                    fields[0].value = "3";
                    fields[1].fieldName = "PREQ_NO1I";
                    fields[1].value = pOLine.RQReqNbr;
                    fields[2].fieldName = "PREQ_ITEM_NO1I";
                    fields[2].value = pOLine.RQReqLineNbr.ToString();

                    submitRequest.screenFields = fields;

                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenContext, submitRequest);
                    screenName = screenReply.mapName;
                    errMess = screenReply.message;

                    if (errMess.Trim() != "")
                    {
                        throw new PXException(errMess.Trim());
                    }

                    if (screenName == "MSM23EA")
                    {
                        ScreenFieldDTO[] screenFieldDTOs = screenReply.screenFields;
                        foreach (ScreenFieldDTO screenFieldDTO in screenFieldDTOs)
                        {
                            String fieldName = screenFieldDTO.fieldName;
                            String preqItemNo = "";
                            if (fieldName == "PREQ_ITEM_NO1I")
                            {
                                preqItemNo = screenFieldDTO.value;
                            }
                        }

                        ScreenNameValueDTO[] field23E = new ScreenNameValueDTO[5];
                        field23E[0] = new ScreenNameValueDTO();
                        field23E[0].fieldName = "PO_NO1I";
                        field23E[0].value = pOOrder.OrderNbr.Trim();

                        field23E[1] = new ScreenNameValueDTO();
                        field23E[1].fieldName = "PO_ITEM1I";
                        field23E[1].value = pOLine.LineNbr.ToString();

                        field23E[2] = new ScreenNameValueDTO();
                        field23E[2].fieldName = "PROCESS_ITEM1I";
                        field23E[2].value = "L";

                        field23E[3] = new ScreenNameValueDTO();
                        field23E[3].fieldName = "ORDER_DATE1I";
                        field23E[3].value = DateTime.Now.ToString("yyyyMMdd");

                        field23E[4] = new ScreenNameValueDTO();
                        field23E[4].fieldName = "PURCH_OFFICER1I";
                        field23E[4].value = "ADMIN";

                        submitRequest.screenFields = field23E;

                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenContext, submitRequest);

                        if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenContext, submitRequest);
                        }

                        screenName = screenReply.mapName;
                        errMess = screenReply.message;
                        currentCursor = screenReply.currentCursorFieldName;

                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }
                }
                catch(Exception e)
                {
                    throw new PXException(e.Message);
                }
            }

            return null;
        }

        public virtual IEnumerable modifyPO(POOrder pOOrder, POLine pOLine) {
            PLNSC.ScreenService.OperationContext screenContext = new PLNSC.ScreenService.OperationContext()
            {
                district = "SC01",
                position = "INTPO",
                maxInstances = 1
            };

            ScreenService screenService = new ScreenService()
            {
                Timeout = sessionTimeout,
                Url = $"{urlPrefix(dbName)}ScreenService"
            };
            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            bool loggedIn = false;
            bool modifyFlag = false;
            string screenName = "";
            string errMess = "";
            string currentCursor = "";

            try
            {
                ClientConversation.authenticate(userName, password);
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
                    screenReply = screenService.executeScreen(screenContext, "MSO220");
                    screenName = screenReply.mapName;
                    if (screenName != "MSM220A")
                    {
                        throw new PXException(CustomMessage.NotMSM220A);
                    }
                    ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                    fields[0].fieldName = "OPTION1I";
                    fields[0].value = "1";
                    fields[1].fieldName = "PO_NO1I";
                    fields[1].value = pOOrder.OrderNbr;
                    fields[2].fieldName = "PO_ITEM_NO1I";
                    fields[2].value = pOLine.LineNbr.ToString();

                    submitRequest.screenFields = fields;

                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenContext, submitRequest);
                    screenName = screenReply.mapName;
                    errMess = screenReply.message;

                    if (errMess.Trim() != "")
                    {
                        throw new PXException(errMess.Trim());
                    }

                    if (screenName == "MSM22CA")
                    {
                        ScreenFieldDTO[] screenFieldDTOs = screenReply.screenFields;

                        ScreenNameValueDTO[] modifyPOFields = new ScreenNameValueDTO[7];
                        modifyPOFields[0] = new ScreenNameValueDTO();
                        modifyPOFields[0].fieldName = "GROSS_PR_UOP1I";
                        if (pOLine.LineType == POLineType.GoodsForDropShip || pOLine.LineType == POLineType.GoodsForInventory || pOLine.LineType == POLineType.GoodsForSalesOrder ||
                            pOLine.LineType == POLineType.GoodsForManufacturing || pOLine.LineType == POLineType.GoodsForReplenishment )
                        {
                            modifyPOFields[0].value = Math.Round(pOLine.CuryUnitCost ?? 0, 2).ToString();
                        } else
                        {
                            modifyPOFields[0].value = Math.Round((pOLine.CuryUnitCost ?? 0) * (pOLine.OrderQty ?? 0), 2).ToString();
                        }

                        modifyPOFields[1] = new ScreenNameValueDTO();
                        modifyPOFields[1].fieldName = "UNIT_OF_ISSUE1I";
                        modifyPOFields[1].value = pOLine.UOM.Trim();

                        modifyPOFields[2] = new ScreenNameValueDTO();
                        modifyPOFields[2].fieldName = "CURR_QTY_P1I";
                        if (pOLine.LineType == POLineType.GoodsForDropShip || pOLine.LineType == POLineType.GoodsForInventory || pOLine.LineType == POLineType.GoodsForSalesOrder ||
                            pOLine.LineType == POLineType.GoodsForManufacturing || pOLine.LineType == POLineType.GoodsForReplenishment)
                        {
                            modifyPOFields[2].value = Math.Round(pOLine.OrderQty ?? 0, 2).ToString();
                        }
                        else
                        {
                            modifyPOFields[2].value = string.Empty;
                        }
                        
                        modifyPOFields[3] = new ScreenNameValueDTO();
                        modifyPOFields[3].fieldName = "UNIT_OF_PURCH1I";
                        modifyPOFields[3].value = pOLine.UOM.Trim();

                        modifyPOFields[4] = new ScreenNameValueDTO();
                        modifyPOFields[4].fieldName = "PRICE_CODE1I";
                        modifyPOFields[4].value = pOOrder.TermsID.Trim();

                        modifyPOFields[5] = new ScreenNameValueDTO();
                        modifyPOFields[5].fieldName = "DUE_DATE1I";
                        modifyPOFields[5].value = (pOLine.PromisedDate ?? DateTime.Now).ToString("yyyyMMdd");

                        modifyPOFields[6] = new ScreenNameValueDTO();
                        modifyPOFields[6].fieldName = "DUE_SITE1I";
                        modifyPOFields[6].value = (pOLine.PromisedDate ?? DateTime.Now).ToString("yyyyMMdd");

                        submitRequest.screenFields = modifyPOFields;

                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenContext, submitRequest);
                        screenName = screenReply.mapName;
                        errMess = screenReply.message;
                        currentCursor = screenReply.currentCursorFieldName;

                        if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenContext, submitRequest);
                        }

                        screenName = screenReply.mapName;
                        errMess = screenReply.message;
                        currentCursor = screenReply.currentCursorFieldName;

                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            throw new PXException(errMess.Trim());
                        }
                    }
                    screenService.positionToMenu(screenContext);
                }
                catch (Exception e)
                {
                    screenService.positionToMenu(screenContext);
                    throw new PXException(e.Message);
                }
            }

            return null;
        }

        string modifyPOCostAlloc(POOrder pOOrder, POLine pOLine, int purchaseMethod, string nonCoreDO)
        {
            string result = "";
            string projectNo = "";

            if (nonCoreDO != string.Empty)
            {

                projectNo = nonCoreDO;
            }
            else
            {
                SOLineSplit sOLineSplit = PXSelect<SOLineSplit, Where<SOLineSplit.pONbr, Equal<Required<SOLineSplit.pONbr>>>>.Select(Base, pOOrder.OrderNbr);

                if (sOLineSplit != null)
                {
                    projectNo = sOLineSplit.OrderNbr.Trim();
                }
                else
                {
                    RQRequisitionOrder rQRequisitionOrder = PXSelect<RQRequisitionOrder, 
                        Where<RQRequisitionOrder.orderNbr, Equal<Required<RQRequisitionOrder.orderNbr>>,
                        And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.po>>>>.Select(Base, pOOrder.OrderNbr);

                    if (rQRequisitionOrder != null)
                    {
                        string reqNbr = rQRequisitionOrder.ReqNbr;
                        RQRequisitionOrder poReq = PXSelect<RQRequisitionOrder,
                        Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
                        And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>,
                        And<RQRequisitionOrder.orderType, Equal<SOOrderTypeConstants.salesOrder>>>>>.Select(Base, reqNbr);

                        if (poReq != null)
                        {
                            projectNo = poReq.OrderNbr.Trim();
                        }
                    }
                }
            }
           

            PLNSC.ScreenService.OperationContext screenContext = new PLNSC.ScreenService.OperationContext()
            {
                district = "SC01",
                position = "INTPO",
                maxInstances = 1
            };

            ScreenService screenService = new ScreenService()
            {
                Timeout = sessionTimeout,
                Url = $"{urlPrefix(dbName)}ScreenService"
            };
            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            bool loggedIn = false;
            bool modifyFlag = false;
            string screenName = "";
            string errMess = "";
            string currentCursor = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                loggedIn = true;
            }
            catch (Exception ex)
            {
                loggedIn = false;
                result = ex.Message.Trim();
                throw new PXException(ex.Message);
            }

            if (loggedIn)
            {
                try
                {
                    screenReply = screenService.executeScreen(screenContext, "MSO230");
                    screenName = screenReply.mapName;
                    if (screenName != "MSM230A")
                    {
                        result = CustomMessage.NotMSM220A;
                        throw new PXException(CustomMessage.NotMSM220A);
                    }
                    ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                    fields[0].fieldName = "OPTION1I";
                    fields[0].value = "5";
                    fields[1].fieldName = "PREQ_NO1I";
                    fields[1].value = pOLine.RQReqNbr;

                    submitRequest.screenFields = fields;

                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(screenContext, submitRequest);
                    screenName = screenReply.mapName;
                    errMess = screenReply.message;

                    if (errMess.Trim() != "")
                    {
                        result = errMess.Trim();
                        throw new PXException(errMess.Trim());
                    }

                    if (screenName == "MSM232A")
                    {
                        ScreenFieldDTO[] screenReplyFields = screenReply.screenFields;
                        foreach (ScreenFieldDTO screenFieldDto in screenReplyFields)
                        {
                            if (screenFieldDto.fieldName == "WO_PROJECT1I1")
                            {
                                if (screenFieldDto.value != null)
                                {
                                    if (screenFieldDto.value.Trim() != "")
                                    {
                                        return "OK";
                                    }
                                }
                            }
                        }

                        ScreenNameValueDTO[] fields232A = { new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                        fields232A[0].fieldName = "WO_PROJECT1I1";
                        fields232A[0].value = projectNo;
                        fields232A[1].fieldName = "PROJECT_IND1I1";
                        fields232A[1].value = "P";

                        submitRequest.screenFields = fields232A;

                        submitRequest.screenKey = "1"; // OK
                        screenReply = screenService.submit(screenContext, submitRequest);
                        screenName = screenReply.mapName;
                        errMess = screenReply.message;

                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            result = errMess.Trim();
                            throw new PXException(errMess.Trim());
                        }

                        if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenContext, submitRequest);
                        }

                        screenName = screenReply.mapName;
                        errMess = screenReply.message;

                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                        {
                            result = errMess.Trim();
                            throw new PXException(errMess.Trim());
                        }
                        else
                        {
                            result = "OK";
                            screenService.positionToMenu(screenContext);
                        }
                    }
                }
                catch (Exception ex)
                {
                    screenService.positionToMenu(screenContext);
                    throw new PXException(ex.Message);
                }
            }

            return result;
        }
    }
}