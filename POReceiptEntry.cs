using EllipseWebServicesClient;
using PX.Data;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;
using PLNSC;
using PLNSC.ReceiptDocumentService;
using PLNSC.ScreenService;
using PLNSC.TransactionRef;
using PLNSC.POReceiptRef;

namespace PX.Objects.PO
{
    public class POReceiptEntry_Extension : PXGraphExtension<POReceiptEntry>
    {
        int sessionTimeout = 3600000;
        int maxInstance = 1;

        string userName = "ADMIN";
        string passWord = "";
        string districtCode = "SC01";
        string positionID = "INTPO";
        string errorMsg = "";
        string screenName = "";
        string integrationResult = "";

        bool loggedIn = false;

        #region Event Handlers
        protected virtual void POReceipt_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            POReceipt row = (POReceipt)e.Row;
            string dateTime = DateTime.Now.ToString();
            string createddate = Convert.ToDateTime(dateTime).ToString("yyyy-MM-dd");
            DateTime dt = DateTime.ParseExact(createddate, "yyyy-MM-dd", null);

            if (row.ReceiptDate > dt)
            {
                cache.RaiseExceptionHandling<POReceipt.receiptDate>(e.Row, row.ReceiptDate, new PXSetPropertyException(CustomMessage.DateLessToday));
            }
        }
        #endregion

        public PXAction<POReceipt> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            receiptPO(adapter);

            //Base.release.Press();

            return Base.Release(adapter);
        }

        public virtual IEnumerable receiptPO(PXAdapter adapter)
        {
            bool receiptGoods = false;
            bool receiptService = false;

            string pONbr = "";

            foreach (POReceipt poreceipt in adapter.Get<POReceipt>())
            {
                if (poreceipt.VendorID == null)
                {
                    throw new PXException(PLNSC.CustomMessage.EmptyVendor);
                }

                List<POReceiptLine> receiptLineList = new List<POReceiptLine>();
                foreach (POReceiptLine recLine in Base.transactions.Select(poreceipt.ReceiptNbr))
                {
                    if (recLine.LineType == "GI" || recLine.LineType == "GS")
                    {
                        receiptLineList.Add(recLine);
                        receiptGoods = true;
                        pONbr = recLine.PONbr;
                    }
                    else
                    {
                        if (poreceipt.ReceiptType == POReceiptType.POReceipt)
                        {
                            integrationResult = receiptPONonGoods(poreceipt, recLine);
                        }

                        if (poreceipt.ReceiptType == POReceiptType.POReturn)
                        {
                            integrationResult = cancelPONonGoods(poreceipt, recLine);
                        }
                    }
                }

                if (receiptGoods)
                {
                    int listlineCount = receiptLineList.Count;
                    if (poreceipt.ReceiptType == POReceiptType.POReceipt)
                    {
                        integrationResult = receiptPOGoods(poreceipt, pONbr, receiptLineList);
                    }

                    if (poreceipt.ReceiptType == POReceiptType.POReturn)
                    {
                        integrationResult = cancelPOGoods(poreceipt, pONbr, receiptLineList);
                    }

                    if (integrationResult.Trim() != "OK")
                    {
                        throw new PXException(integrationResult.Trim());
                    }
                }
            }
            return adapter.Get();
        }

        string receiptPOGoods(POReceipt pOReceipt, string pONbr, List<POReceiptLine> receiptLines)
        {
            string result = "";
            string transID = "";

            bool isOverSupply = false;

            try
            {
                ClientConversation.authenticate(userName, passWord);

                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout
                };

                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                ReceiptDocumentService receipt = new ReceiptDocumentService()
                {
                    Timeout = 3600000
                };

                PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemService receiptPurchaseOrderItemService = new PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemService()
                {
                    Timeout = 3600000
                };

                PLNSC.ReceiptPurchaseOrderItemRef.OperationContext receiptPOItemContext = new PLNSC.ReceiptPurchaseOrderItemRef.OperationContext()
                {
                    district = transContext.district,
                    position = transContext.position,
                    maxInstances = transContext.maxInstances,
                    returnWarnings = transContext.returnWarnings,
                    trace = transContext.trace,
                    transaction = transID
                };

                PLNSC.ReceiptDocumentService.OperationContext receiptContext = new PLNSC.ReceiptDocumentService.OperationContext()
                {
                    district = transContext.district,
                    position = transContext.position,
                    maxInstances = transContext.maxInstances,
                    returnWarnings = transContext.returnWarnings,
                    trace = transContext.trace,
                    transaction = transID
                };

                var userID = PXAccess.GetUserID();
                EPEmployee ePEmployee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(Base, PXAccess.GetUserID());
                string receivedByUser = ePEmployee.AcctCD.Trim() ?? "ADMIN";

                PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemSearchParam receiptPurchaseOrderItemSearchParam = new PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemSearchParam()
                {
                    documentDistrictCode = "SC01",
                    documentNumber = pONbr,
                    documentTypeDescription = "POP",
                    isReceiveAll = false
                };

                PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemDTO restartParam = new PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemDTO()
                {
                    documentNumber = "",
                    documentDistrictCode = "",
                    documentItem = "",
                    documentTypeDescription = "",
                    receiptDocumentType = ""
                };

                PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemServiceResult[] searchResult = receiptPurchaseOrderItemService.search(receiptPOItemContext, receiptPurchaseOrderItemSearchParam, restartParam);

                if (searchResult.Length > 0)
                {
                    PLNSC.ReceiptPurchaseOrderItemRef.Error[] searchErrors = searchResult[0].errors;
                    PLNSC.ReceiptPurchaseOrderItemRef.Message[] searchMsg = searchResult[0].informationalMessages;
                    if (searchErrors.Length > 0)
                    {
                        result = searchErrors[0].messageText;
                        transContext.transaction = transID;
                        transactionService.rollback(transContext);
                        return result;
                    }
                    
                    ReceiptPurchaseOrderItemDTO[] requestItem = new ReceiptPurchaseOrderItemDTO[searchResult.Length];
                    int arrIndex = 0;
                    foreach (PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemServiceResult receiptPurchaseOrderItemServiceResult in searchResult)
                    {
                        PLNSC.ReceiptPurchaseOrderItemRef.ReceiptPurchaseOrderItemDTO lineReceipt = receiptPurchaseOrderItemServiceResult.receiptPurchaseOrderItemDTO;

                        string testPOReceiptLine = Right("000" + lineReceipt.documentItem.Trim(), 3);
                        string receiptLine = lineReceipt.documentNumber != null ? testPOReceiptLine : "001";
                        decimal receiptQty = 0;
                        pONbr = lineReceipt.documentNumber != null ? lineReceipt.documentNumber.Trim() : " ";

                        foreach (POReceiptLine pOReceiptLine in receiptLines)
                        {
                            string poNbr = pOReceiptLine.PONbr.Trim();
                            string poLine = Right("000" + pOReceiptLine.POLineNbr.ToString(), 3);
                            if (poNbr == pONbr && poLine == receiptLine)
                            {
                                receiptQty = pOReceiptLine.ReceiptQty ?? 0;
                                decimal origOrderQty = pOReceiptLine.OrigOrderQty ?? 0;
                                decimal openOrderQty = pOReceiptLine.OpenOrderQty ?? 0;
                                decimal outstandingQty = openOrderQty != 0 ? openOrderQty : origOrderQty;
                                if (receiptQty > outstandingQty)
                                {
                                    isOverSupply = true;
                                }
                                else
                                {
                                    isOverSupply = false;
                                }
                            }
                        }

                        requestItem[arrIndex] = new ReceiptPurchaseOrderItemDTO()
                        {
                            documentNumber = lineReceipt.documentNumber.Trim() ?? " ",
                            custodianId = lineReceipt.custodianId.Trim() ?? " ",
                            documentItem = receiptLine,
                            receiptQuantity = receiptQty,
                            receiptQuantitySpecified = true,
                            documentTypeDescription = lineReceipt.documentTypeDescription.Trim() ?? " ",
                            documentType = lineReceipt.documentType.Trim() ?? " ",
                            deliveryLocation = lineReceipt.deliveryLocation.Trim() ?? " ",
                            isReceive = true,
                            isReceiveSpecified = true,
                            isCompleteItem = false,
                            isCompleteItemSpecified = true,
                            isOverSupplyItem = isOverSupply,
                            isOverSupplyItemSpecified = true,
                            isBinCodeDisabled = lineReceipt.isBinCodeDisabled,
                            isBinCodeDisabledSpecified = true,
                            isCategoryCodeDisabled = lineReceipt.isCategoryCodeDisabled,
                            isCategoryCodeDisabledSpecified = true,
                            receiptDocumentType = lineReceipt.receiptDocumentType.Trim() ?? " ",
                            receivingDistrictCode = lineReceipt.receivingDistrictCode.Trim() ?? " ",
                            receivingWarehouseId = lineReceipt.receivingWarehouseId.Trim() ?? " ",
                            requestedByEmployeeId = receivedByUser,
                            purchaseRequisition = lineReceipt.purchaseRequisition.Trim() ?? " ",
                            unitOfPurchase = lineReceipt.unitOfPurchase.Trim() ?? " ",
                            unitOfIssueOutstanding = lineReceipt.unitOfIssueOutstanding,
                            unitOfIssueOutstandingSpecified = true,
                            unitOfMeasure = lineReceipt.unitOfMeasure.Trim() ?? " ",
                            unitOfPurchaseOutstanding = lineReceipt.unitOfPurchaseOutstanding,
                            unitOfPurchaseOutstandingSpecified = true
                        };
                        arrIndex += 1;
                    }

                    ReceiptDocumentDTO request = new ReceiptDocumentDTO()
                    {
                        documentNumber = pONbr,
                        receiptReference = "G-" + pOReceipt.ReceiptNbr.Trim() + "RC",
                        receiptPurchaseOrderItemDTOs = requestItem,
                        receiptDate = DateTime.Now,
                        receiptDateSpecified = true,
                        isReceiveAll = false,
                        isReceiveAllSpecified = true,
                        receivedBy = receivedByUser,
                        receivedByPosition = "INTPO",
                        receivingDistrictCode = "SC01",
                        documentDistrictCode = "SC01",
                        documentTypeDescription = "POP"
                    };

                    ReceiptDocumentServiceResult receiptDocumentServiceResult = receipt.update(receiptContext, request);
                    PLNSC.ReceiptDocumentService.Error[] updErrors = receiptDocumentServiceResult.errors;
                    PLNSC.ReceiptDocumentService.Message[] updMessages = receiptDocumentServiceResult.informationalMessages;

                    if (updErrors.Length > 0)
                    {
                        result = updErrors[0].messageText;
                        transContext.transaction = transID;
                        transactionService.rollback(transContext);
                    }
                    else
                    {
                        result = "OK";
                        transContext.transaction = transID;
                        transactionService.commit(transContext);
                    }
                }
                else
                {
                    return "No Item To receive or All Item has already been received";
                }
                //ReceiptPurchaseOrderItemDTO[] requestItem = new ReceiptPurchaseOrderItemDTO[receiptLines.Count];
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        string receiptPONonGoods(POReceipt pOReceipt, POReceiptLine pOReceiptLine)
        {
            string result = "";

            PLNSC.ScreenService.OperationContext SSContextClass = new PLNSC.ScreenService.OperationContext()
            {
                district = "SC01",
                position = "INTPO",
                maxInstances = 1
            };

            ScreenService screenService = new ScreenService()
            {
                Timeout = 3600000
            };

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                screenService.Timeout = 3600000;

                ClientConversation.authenticate("ADMIN", "");

                screenReply = screenService.executeScreen(SSContextClass, "MSO155");
                screenName = screenReply.mapName;

                if (screenName != "MSM155A")
                {
                    throw new PXException(CustomMessage.NotMSO155);
                }

                ScreenNameValueDTO[] fields = {
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO()
                };

                fields[0].fieldName = "OPTION1I";
                fields[0].value = "2";
                fields[1].fieldName = "PO_NO1I";
                fields[1].value = pOReceiptLine.PONbr;
                fields[2].fieldName = "PO_ITEM1I";
                fields[2].value = pOReceiptLine.POLineNbr.ToString();
                fields[3].fieldName = "AUTH_BY1I";
                fields[3].value = "ADMIN";

                submitRequest.screenFields = fields;

                submitRequest.screenKey = "1"; // OK
                screenReply = screenService.submit(SSContextClass, submitRequest);

                screenName = screenReply.mapName;
                errorMsg = screenReply.message;

                if (errorMsg.Trim().Contains("X2:6552 - ITEM HAS BEEN FULLY RECEIVED"))
                {
                    return "OK";
                }

                if (errorMsg.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                {
                    throw new PXException(errorMsg.Trim());
                }

                if (screenName != "MSM156A")
                {
                    throw new PXException(CustomMessage.NotMSM156A);
                }

                ScreenNameValueDTO[] fields156 = {
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO()
                };
                string receiptRef = pOReceipt.ReceiptNbr != null ? pOReceipt.ReceiptNbr.Trim() : " ";
                fields156[0].fieldName = "VALUE_RECVD1I";
                fields156[0].value = pOReceiptLine.CuryExtCost.ToString().Trim();
                fields156[1].fieldName = "RECEIPT_REF1I";
                fields156[1].value = "S-" + receiptRef + "RC";
                fields156[2].fieldName = "RECEIPT_DATE1I";
                fields156[2].value = DateTime.Now.ToString("yyyyMMdd");

                submitRequest.screenFields = fields156;

                submitRequest.screenKey = "1"; // OK
                screenReply = screenService.submit(SSContextClass, submitRequest);

                screenName = screenReply.mapName;
                errorMsg = screenReply.message;

                if (screenName == "MSM156A")
                {
                    ScreenNameValueDTO[] fields156B = { new ScreenNameValueDTO() };
                    fields156B[0].fieldName = "ANSWER1I";
                    fields156B[0].value = "Y";

                    submitRequest.screenFields = fields156B;

                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(SSContextClass, submitRequest);

                    screenName = screenReply.mapName;
                    errorMsg = screenReply.message;
                }

                if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                {
                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(SSContextClass, submitRequest);

                    screenName = screenReply.mapName;
                    errorMsg = screenReply.message;
                }

                screenService.positionToMenu(SSContextClass);

                result = "OK";
            }
            catch (Exception ex)
            {
                result = ex.Message;
                throw new PXException(ex.Message);
            }

            return result;
        }

        string cancelPOGoods(POReceipt pOReceipt, string pONbr, List<POReceiptLine> receiptLines)
        {
            string result = "";
            string transID = "";

            try
            {
                foreach (POReceiptLine pOReceiptLine in receiptLines)
                {
                    ClientConversation.authenticate(userName, passWord);
                    TransactionService transactionService = new TransactionService()
                    {
                        Timeout = sessionTimeout
                    };

                    PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                    {
                        district = districtCode,
                        position = positionID,
                        maxInstances = maxInstance
                    };

                    transID = transactionService.begin(transContext);

                    PurchaseOrderReceiptService purchaseOrderReceiptService = new PurchaseOrderReceiptService()
                    {
                        Timeout = sessionTimeout
                    };

                    PLNSC.POReceiptRef.OperationContext cancelPOContext = new PLNSC.POReceiptRef.OperationContext()
                    {
                        district = transContext.district,
                        position = transContext.position,
                        maxInstances = transContext.maxInstances,
                        returnWarnings = transContext.returnWarnings,
                        trace = transContext.trace,
                        transaction = transID
                    };

                    string pOLineNbr = pOReceiptLine.POLineNbr.ToString() ?? " ";

                    PurchaseOrderReceiptSearchParam searchParam = new PurchaseOrderReceiptSearchParam()
                    {
                        purchaseOrderNumber = pOReceiptLine.PONbr,
                        purchaseOrderItemNumber = pOLineNbr
                    };

                    PurchaseOrderReceiptServiceResult[] searchResult = purchaseOrderReceiptService.search(cancelPOContext, searchParam, null);

                    if (searchResult.Length > 0)
                    {
                        for (int j = 0; j < searchResult.Length;j++)
                        {
                            PLNSC.POReceiptRef.Error[] cancelPOErrors = searchResult[j].errors;
                            PLNSC.POReceiptRef.Message[] cancelPOMessages = searchResult[j].informationalMessages;

                            if (cancelPOErrors.Length > 0)
                            {
                                for (int k = 0; k < cancelPOErrors.Length; k++)
                                {
                                    string errorMessage = (k + 1) + ". " + cancelPOErrors[k].messageText;
                                    result += errorMessage;
                                }
                            }
                            PurchaseOrderReceiptDTO purchaseOrderReceiptDTO = searchResult[j].purchaseOrderReceiptDTO;
                            string receiptRef = purchaseOrderReceiptDTO.receiptReference != null ? purchaseOrderReceiptDTO.receiptReference.Trim() : " ";
                            decimal cancelQty = pOReceiptLine.ReceiptQty ?? 0;
                            PurchaseOrderReceiptDTO canceReciptlDTO = new PurchaseOrderReceiptDTO()
                            {
                                purchaseOrderNumber = purchaseOrderReceiptDTO.purchaseOrderNumber,
                                purchaseOrderItemNumber = purchaseOrderReceiptDTO.purchaseOrderItemNumber,
                                receiptReferenceCancel = "G-" + pOReceipt.ReceiptNbr.Trim() + "RT",
                                districtCode = "SC01",
                                cancelledBy = "ADMIN",
                                isOrderComplete = false,
                                isOrderCompleteSpecified = true,
                                custodianId = purchaseOrderReceiptDTO.custodianId,
                                purchaseRequisitionNumber = purchaseOrderReceiptDTO.purchaseRequisitionNumber,
                                cancelQuantity = cancelQty,
                                cancelQuantitySpecified = true,
                                receiptReference = receiptRef,
                                warehouseId = purchaseOrderReceiptDTO.warehouseId != null ? purchaseOrderReceiptDTO.warehouseId.Trim() : "MAIN",
                                changeNumber = purchaseOrderReceiptDTO.changeNumber,
                                correlationId = purchaseOrderReceiptDTO.correlationId,
                                freightCode = purchaseOrderReceiptDTO.freightCode,
                                requisitionDistrict = purchaseOrderReceiptDTO.requisitionDistrict,
                                receivedBy = purchaseOrderReceiptDTO.receivedBy,
                                receiptNumber_2 = purchaseOrderReceiptDTO.receiptNumber_2,
                                quantityNotInvoiced = purchaseOrderReceiptDTO.quantityNotInvoiced,
                                quantityNotInvoicedSpecified = true,

                            };
                            PurchaseOrderReceiptServiceResult purchaseOrderReceiptServiceResult = purchaseOrderReceiptService.cancel(cancelPOContext, canceReciptlDTO);
                            PLNSC.POReceiptRef.Error[] cancelReceiptError = purchaseOrderReceiptServiceResult.errors;
                            if (cancelReceiptError.Length > 0)
                            {
                                result = cancelReceiptError[0].messageText;
                                transContext.transaction = transID;
                                transactionService.rollback(transContext);
                            }
                            else
                            {
                                result = "OK";
                                transContext.transaction = transID;
                                transactionService.commit(transContext);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        string cancelPONonGoods(POReceipt pOReceipt, POReceiptLine pOReceiptLine)
        {
            string result = "";

            PLNSC.ScreenService.OperationContext SSContextClass = new PLNSC.ScreenService.OperationContext()
            {
                district = "SC01",
                position = "INTPO",
                maxInstances = 1
            };

            ScreenService screenService = new ScreenService()
            {
                Timeout = 3600000
            };

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            try
            {
                screenService.Timeout = 3600000;

                ClientConversation.authenticate("ADMIN", "");

                screenReply = screenService.executeScreen(SSContextClass, "MSO155");
                screenName = screenReply.mapName;

                if (screenName != "MSM155A")
                {
                    throw new PXException(CustomMessage.NotMSO155);
                }

                ScreenNameValueDTO[] fields = {
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO()
                };

                fields[0].fieldName = "OPTION1I";
                fields[0].value = "3";
                fields[1].fieldName = "PO_NO1I";
                fields[1].value = pOReceiptLine.PONbr;
                fields[2].fieldName = "PO_ITEM1I";
                fields[2].value = pOReceiptLine.POLineNbr.ToString();
                fields[3].fieldName = "AUTH_BY1I";
                fields[3].value = "ADMIN";

                submitRequest.screenFields = fields;

                submitRequest.screenKey = "1"; // OK
                screenReply = screenService.submit(SSContextClass, submitRequest);

                screenName = screenReply.mapName;
                errorMsg = screenReply.message;

                if (errorMsg.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                {
                    throw new PXException(errorMsg.Trim());
                }

                if (screenName != "MSM156A")
                {
                    throw new PXException(CustomMessage.NotMSM156A);
                }

                ScreenNameValueDTO[] fields156 = {
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO(),
                    new ScreenNameValueDTO()
                };
                string receiptRef = pOReceipt.ReceiptNbr != null ? pOReceipt.ReceiptNbr.Trim() : " ";
                string cancelValue = Math.Round((pOReceiptLine.CuryExtCost * -1) ?? 0, 2).ToString().Trim();
                fields156[0].fieldName = "VALUE_RECVD1I";
                fields156[0].value = cancelValue;
                fields156[1].fieldName = "RECEIPT_REF1I";
                fields156[1].value = "S-" + receiptRef + "RT";
                fields156[2].fieldName = "RECEIPT_DATE1I";
                fields156[2].value = DateTime.Now.ToString("yyyyMMdd");
                fields156[3].fieldName = "ANSWER1I";
                fields156[3].value = "N";

                submitRequest.screenFields = fields156;

                submitRequest.screenKey = "1"; // OK
                screenReply = screenService.submit(SSContextClass, submitRequest);

                screenName = screenReply.mapName;
                errorMsg = screenReply.message;

                if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                {
                    submitRequest.screenKey = "1"; // OK
                    screenReply = screenService.submit(SSContextClass, submitRequest);

                    screenName = screenReply.mapName;
                    errorMsg = screenReply.message;
                }

                screenService.positionToMenu(SSContextClass);

                result = "OK";
            }
            catch (Exception ex)
            {
                result = ex.Message;
                throw new PXException(ex.Message);
            }

            return result;
        }

        public string Right(string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }
    }
}
