using PX.Data;
using System;
using System.Collections;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Hosting;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.GL;
using PLNSC.ProjectService;
using EllipseWebServicesClient;
using PLNSC;
using PLNSC.ProjectEstimateRef;
using PLNSC.RefCodesRef;
using PLNSC.TransactionRef;

namespace PX.Objects.SO
{
    public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry>
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
            } else
            {
                result = "http://ews-elldev.ellipse.plnsc.co.id/ews/services/";
            }

            return result;
        }

        int sessionTimeout = 3600000;
        int maxInstance = 1;

        string districtCode = "SC01";
        string positionID = "INTPO";
        string message = "";
        string userName = "ADMIN";
        string password = "P@ssw0rd";
        
        bool loggedIn = false;

        #region Event Handlers

        [PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void SOOrder_OrderDesc_CacheAttached(PXCache cache)
        {
        }

        protected void SOOrder_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SOOrder sOOrder = (SOOrder)e.Row;
            if (sOOrder.Status == SOOrderStatus.Open && sOOrder.OrderType == SOOrderTypeConstants.SalesOrder)
            {
                String soNbr = sOOrder.OrderNbr;
                if (sOOrder.OrderNbr != null)
                {
                    createDO(sOOrder);
                }
            }
        }

        protected virtual void SOOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            SOOrder row = (SOOrder)e.Row;
            if (row == null) return;
            SOOrderExt sOOrderExt = row.GetExtension<SOOrderExt>();

            this.finalizeDO.SetEnabled(row.OrderType == SOOrderTypeConstants.SalesOrder && row.Status == SOOrderStatus.Completed && row.Hold == false && sOOrderExt.UsrFinalized == false);
        }

        public PXAction<SOOrder> finalizeDO;
        [PXButton()]
        [PXUIField(DisplayName = CustomMessage.finalizeEllipseProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        protected virtual void FinalizeDO()
        {
            SOOrder row = Base.Document.Current;
            SOOrderExt rowExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(row);

            string soNumber = row.OrderNbr;
            string actualEffortResult = actualEffort(row.OrderNbr);
            if (actualEffortResult != "OK")
            {
                throw new PXException(actualEffortResult);
            }

            string finalizeDOResult = finalizeEllipseProject(row.OrderNbr);
            if (finalizeDOResult != "OK")
            {
                throw new PXException(finalizeDOResult);
            }

            Base.Document.Cache.SetValueExt<SOOrderExt.usrFinalized>(row, true);
            Base.Document.Cache.Update(row);

            //Base.Caches[typeof(SOOrder)].SetValueExt<SOOrderExt.usrFinalized>(row, true);
            //Base.Caches[typeof(SOOrder)].Update(rowExt);
            Base.Save.Press();
        }

        public virtual IEnumerable createDO(SOOrder sOOrder)
        {
            bool modifyFlag = false;
            SOOrderExt sOOrderExt = sOOrder.GetExtension<SOOrderExt>();

            if (sOOrder.OrderNbr == null)
            {
                return null;
            }

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

            if (loggedIn == true)
            {
                string createProjectResult = createProject(sOOrder);
                if (createProjectResult != "OK")
                {
                    if (createProjectResult.Trim().Contains("PROJECT ALREADY EXISTS"))
                    {
                        string modifyProjectResult = modifyProject(sOOrder);
                        modifyFlag = true;
                        if (modifyProjectResult != "OK")
                        {
                            throw new PXException(modifyProjectResult);
                        }
                    }
                    else
                    {
                        throw new PXException(createProjectResult);
                    }
                }

                if (!modifyFlag)
                {
                    string authoriseProjectResult = authoriseProject(sOOrder);
                    if (authoriseProjectResult != "OK")
                    {
                        throw new PXException(authoriseProjectResult);
                    }
                }

                string modifyProjectRefCodesResult = modifyProjectRefCodes(sOOrder);
                if (modifyProjectRefCodesResult != "OK")
                {
                    throw new PXException(modifyProjectRefCodesResult);
                }

                string createProjectEstimateResult = createProjectEstimate(sOOrder);
                if (createProjectEstimateResult != "OK")
                {
                    throw new PXException(createProjectEstimateResult);
                }

                string recordProjectActualsResult = recordProjectActuals(sOOrder);
                if (recordProjectActualsResult != "OK")
                {
                    throw new PXException(recordProjectActualsResult);
                }
            }
            return null;
        }

        string createProject(SOOrder sOOrder)
        {
            int? customerSalesAcctId;
            int? customerSalesSubId;
            int? cogsAcctId;
            int? cogsSubId;
            int? invtAcctId;
            int? invtSubId;

            string cogsAcctCd = "";
            string cogsSubCd = "";
            string invtAcctCd = "";
            string invtSubCd = "";
            string custSalesAcctCD = "";
            string custSalesSubCD = "";
            string custCD = "";
            string transID = "";

            string result = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                BAccount2 customer = PXSelect<BAccount2,
                    Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                    And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                    And<BAccount.type, Equal<Required<BAccount.type>>>>>>.Select(Base, sOOrder.CustomerID, sOOrder.CustomerLocationID, "CU");

                Location custLocation = PXSelect<Location,
                    Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                    And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(Base, sOOrder.CustomerID, sOOrder.CustomerLocationID);

                customerSalesAcctId = custLocation.CSalesAcctID;
                customerSalesSubId = custLocation.CSalesSubID;

                Account custSalesAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, customerSalesAcctId);
                Sub custSalesSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, customerSalesSubId);

                custSalesAcctCD = custSalesAccount.AccountCD;
                custSalesSubCD = custSalesSub.SubCD;
                custCD = customer.AcctCD.Trim();

                foreach (SOLine soLine in Base.Transactions.Select(sOOrder.OrderNbr))
                {
                    InventoryItem inventoryItem = PXSelect<InventoryItem,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, soLine.InventoryID);

                    cogsAcctId = inventoryItem.COGSAcctID;
                    cogsSubId = inventoryItem.COGSSubID;
                    invtAcctId = inventoryItem.InvtAcctID;
                    invtSubId = inventoryItem.InvtSubID;

                    if (cogsAcctId == null && cogsSubId == null)
                    {
                        throw new PXException(CustomMessage.COGSAccountEmpty);
                    }

                    Account itemInvtAcct = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, invtAcctId);
                    Sub itemInvtSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, invtSubId);
                    Account invtCogsAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, cogsAcctId);
                    Sub invtCogsSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, cogsSubId);

                    cogsAcctCd = invtCogsAccount.AccountCD;
                    cogsSubCd = invtCogsSub.SubCD;
                    invtAcctCd = itemInvtAcct.AccountCD;
                    invtSubCd = itemInvtSub.SubCD;

                    if (cogsAcctId != null && cogsSubId != null) break;
                }

                EPEmployee ePEmployee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(Base, sOOrder.OwnerID);
                string originatorId = ePEmployee != null ? ePEmployee.AcctCD : "ADMIN";
                string stockItemAccount = custSalesSubCD.Substring(0, 11).Trim() + cogsSubCd.Substring(11, 4).Trim();// + cogsAcctCd.Trim();
                string nonStockItemAccount = cogsSubCd.Trim();// + cogsAcctCd.Trim();
                string itemInventoryAccount = invtSubCd.Trim() + invtAcctCd.Trim();
                DateTime actStartDate = sOOrder.OrderDate ?? DateTime.Now;
                string raisedDate = actStartDate.ToString("yyyyMMdd");
                string planFinishDate = (sOOrder.RequestDate ?? actStartDate.AddDays(60)).ToString("yyyyMMdd");

                try
                {
                    ClientConversation.authenticate(userName, password);
                    PLNSC.ProjectService.OperationContext projectOperationContext = new PLNSC.ProjectService.OperationContext()
                    {
                        district = districtCode,
                        position = positionID,
                        maxInstances = maxInstance,
                        transaction = transID
                    };

                    ProjectService projectService = new ProjectService()
                    {
                        Timeout = sessionTimeout,
                        Url = $"{urlPrefix(dbName)}ProjectService"
                    };

                    ProjectServiceCreateRequestDTO projectRequest = new ProjectServiceCreateRequestDTO()
                    {
                        districtCode = districtCode,
                        projectNo = sOOrder.OrderNbr,
                        projDesc = sOOrder.OrderDesc,
                        originatorId = originatorId,
                        raisedDate = raisedDate,
                        planFinDate = planFinishDate,
                        accountCode = stockItemAccount,
                        accountCodeEnabled = true
                    };

                    ProjectServiceCreateReplyDTO projectReply = projectService.create(projectOperationContext, projectRequest);
                    PLNSC.ProjectService.WarningMessageDTO[] warningMessageDTOs = projectReply.warningsAndInformation;

                    if (warningMessageDTOs.Length > 0)
                    {
                        result = warningMessageDTOs[0].message;
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
                catch (Exception ex)
                {
                    result = ex.Message;
                    transContext.transaction = transID;
                    transactionService.rollback(transContext);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        string finalizeEllipseProject(string sONbr)
        {
            string result = string.Empty;
            string transID = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                ProjectService projectService = new ProjectService()
                {
                    Timeout = transactionService.Timeout,
                    Url = $"{urlPrefix(dbName)}ProjectService"
                };

                PLNSC.ProjectService.OperationContext projectServiceContext = new PLNSC.ProjectService.OperationContext()
                {
                    district = transContext.district,
                    position = transContext.position,
                    maxInstances = transContext.maxInstances
                };

                //ProjectEstimateDTO[] projectEstimateDTOs = new ProjectEstimateDTO[1];
                ProjectServiceFinaliseRequestDTO projectServiceFinaliseRequestDTO = new ProjectServiceFinaliseRequestDTO()
                {
                    districtCode = transContext.district,
                    projectNo = sONbr.Trim(),
                    finalCostInd = "Y"
                };
                ProjectServiceFinaliseReplyDTO projectServiceFinaliseReplyDTO = projectService.finalise(projectServiceContext, projectServiceFinaliseRequestDTO);
                //ProjectEstimateServiceResult[] projectEstimateServiceResults = projectEstimateService.multipleUpdate(projectEstimateOperationContext, projectEstimateDTOs);

                //ProjectEstimateServiceResult projectEstimateServiceResult = projectEstimateServiceResults[0];

                PLNSC.ProjectService.WarningMessageDTO[] errors = projectServiceFinaliseReplyDTO.warningsAndInformation;

                if (errors.Length > 0)
                {
                    result = errors[0].message.Trim();
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
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        string actualEffort(string sONbr)
        {
            string result = string.Empty;
            string transID = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                ProjectService projectService = new ProjectService()
                {
                    Timeout = transactionService.Timeout,
                    Url = $"{urlPrefix(dbName)}ProjectService"
                };

                PLNSC.ProjectService.OperationContext projectServiceContext = new PLNSC.ProjectService.OperationContext()
                {
                    district = transContext.district,
                    position = transContext.position,
                    maxInstances = transContext.maxInstances
                };

                ProjectServiceReadRequestDTO projectServiceReadRequestDTO = new ProjectServiceReadRequestDTO()
                {
                    districtCode = transContext.district,
                    projectNo = sONbr.Trim()
                };

                ProjectServiceReadReplyDTO projectServiceReadReplyDTO = projectService.read(projectServiceContext, projectServiceReadRequestDTO);
                PLNSC.ProjectService.WarningMessageDTO[] errorRead = projectServiceReadReplyDTO.warningsAndInformation;
                if (errorRead.Length > 0)
                {
                    result = errorRead[0].message.Trim();
                    transContext.transaction = transID;
                    transactionService.rollback(transContext);
                    return result;
                }

                string actStrDate = projectServiceReadReplyDTO.actualStrDate;

                ProjectServiceActualsRequestDTO projectServiceActualsRequestDTO = new ProjectServiceActualsRequestDTO()
                {
                    districtCode = transContext.district,
                    projectNo = sONbr.Trim(),
                    actualStrDate = actStrDate,
                    actualFinDate = DateTime.Now.ToString("yyyyMMdd")
                };
                ProjectServiceActualsReplyDTO projectServiceActualReplyDTO = projectService.actuals(projectServiceContext, projectServiceActualsRequestDTO);

                PLNSC.ProjectService.WarningMessageDTO[] errors = projectServiceActualReplyDTO.warningsAndInformation;

                if (errors.Length > 0)
                {
                    result = errors[0].message.Trim();
                    transContext.transaction = transID;
                    transactionService.rollback(transContext);
                    return result;
                }
                else
                {
                    result = "OK";
                    transContext.transaction = transID;
                    transactionService.commit(transContext);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        string createProjectEstimate(SOOrder sOOrder)
        {
            string transID = "";
            string result = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                EPEmployee ePEmployee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(Base, sOOrder.OwnerID);
                string estimatorId = ePEmployee != null ? ePEmployee.AcctCD : "ADMIN";

                ProjectEstimateService projectEstimateService = new ProjectEstimateService()
                {
                    Timeout = transactionService.Timeout,
                    Url = $"{urlPrefix(dbName)}ProjectEstimateService"
                };

                PLNSC.ProjectEstimateRef.OperationContext projectEstimateOperationContext = new PLNSC.ProjectEstimateRef.OperationContext()
                {
                    district = transContext.district,
                    position = transContext.position,
                    maxInstances = transContext.maxInstances
                };

                //ProjectEstimateDTO[] projectEstimateDTOs = new ProjectEstimateDTO[1];
                ProjectEstimateDTO projectEstimateDTO = new ProjectEstimateDTO()
                {
                    districtCode = transContext.district,
                    projectNo = sOOrder.OrderNbr.Trim(),
                    estimateBuildMethod = "T",
                    estimatorId = estimatorId,
                    directEstimateCost = Math.Round(sOOrder.OrderTotal ?? 0, 2),
                    directEstimateCostSpecified = true,
                    directUnallocFinPeriodEst = Math.Round(sOOrder.OrderTotal ?? 0, 2),
                    directUnallocFinPeriodEstSpecified = true,
                    estimateSpreadCode = "A",
                    budgetCode = "RAB"
                };
                ProjectEstimateServiceResult projectEstimateServiceResult = projectEstimateService.update(projectEstimateOperationContext, projectEstimateDTO);
                //ProjectEstimateServiceResult[] projectEstimateServiceResults = projectEstimateService.multipleUpdate(projectEstimateOperationContext, projectEstimateDTOs);

                //ProjectEstimateServiceResult projectEstimateServiceResult = projectEstimateServiceResults[0];

                Error[] errors = projectEstimateServiceResult.errors;

                if (errors.Length > 0)
                {
                    result = errors[0].messageText.Trim();
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
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        string modifyProjectRefCodes(SOOrder sOOrder)
        {
            string transID = "";
            string result = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                BAccount2 customer = PXSelect<BAccount2,
                    Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                    And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                    And<BAccount.type, Equal<Required<BAccount.type>>>>>>.Select(Base, sOOrder.CustomerID, sOOrder.CustomerLocationID, "CU");

                string custCD = customer.AcctCD.Trim();

                PLNSC.RefCodesRef.OperationContext refCodesOperation = new PLNSC.RefCodesRef.OperationContext()
                {
                    district = transContext.district,
                    position = transContext.position,
                    maxInstances = transContext.maxInstances
                };

                RefCodesService refCodesService = new RefCodesService()
                {
                    Timeout = transactionService.Timeout,
                    Url = $"{urlPrefix(dbName)}RefCodesService"
                };

                RefCodesServiceModifyRequestDTO refCodesServiceModifyRequestDTO = new RefCodesServiceModifyRequestDTO()
                {
                    entityType = "PRJ",
                    entityValue = transContext.district.Trim() + sOOrder.OrderNbr.Trim(),
                    refNo = "001",
                    seqNum = "001",
                    refCode = sOOrder.CustomerOrderNbr.Trim()
                };

                RefCodesServiceModifyReplyDTO refCodesServiceModifyReplyDTO = refCodesService.modify(refCodesOperation, refCodesServiceModifyRequestDTO);
                PLNSC.RefCodesRef.WarningMessageDTO[] refCodesErrors = refCodesServiceModifyReplyDTO.warningsAndInformation;
                if (refCodesErrors.Length > 0)
                {
                    result = refCodesErrors[0].message;
                    transactionService.rollback(transContext);
                }
                else
                {
                    if (sOOrder.CustomerID != null)
                    {
                        RefCodesServiceModifyRequestDTO refCodesServiceModifyRequestDTO2 = new RefCodesServiceModifyRequestDTO()
                        {
                            entityType = "PRJ",
                            entityValue = transContext.district.Trim() + sOOrder.OrderNbr.Trim(),
                            refNo = "002",
                            seqNum = "001",
                            refCode = custCD
                        };

                        RefCodesServiceModifyReplyDTO refCodesServiceModifyReplyDTO2 = refCodesService.modify(refCodesOperation, refCodesServiceModifyRequestDTO2);
                        PLNSC.RefCodesRef.WarningMessageDTO[] refCodesErrors2 = refCodesServiceModifyReplyDTO2.warningsAndInformation;
                        if (refCodesErrors2.Length > 0)
                        {
                            result = refCodesErrors2[0].message;
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
                        result = "OK";
                        transContext.transaction = transID;
                        transactionService.commit(transContext);
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        string authoriseProject(SOOrder sOOrder)
        {
            string result = "";
            string transID = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                PLNSC.ProjectService.OperationContext projectOperationContext = new PLNSC.ProjectService.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance,
                    transaction = transID
                };

                ProjectService projectService = new ProjectService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}ProjectService"
                };

                ProjectServiceAuthoriseRequestDTO projectServiceAuthoriseRequestDTO = new ProjectServiceAuthoriseRequestDTO()
                {
                    authsdBy = "ADMIN",
                    projectNo = sOOrder.OrderNbr.Trim()
                };

                ProjectServiceAuthoriseReplyDTO projectServiceAuthoriseReplyDTO = projectService.authorise(projectOperationContext, projectServiceAuthoriseRequestDTO);

                PLNSC.ProjectService.WarningMessageDTO[] authErrors = projectServiceAuthoriseReplyDTO.warningsAndInformation;

                if (authErrors.Length > 0)
                {
                    result = authErrors[0].message;
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
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        string recordProjectActuals(SOOrder sOOrder)
        {
            string result = "";
            string transID = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                string raisedDate = (sOOrder.OrderDate ?? DateTime.Now).ToString("yyyyMMdd");

                PLNSC.ProjectService.OperationContext projectOperationContext = new PLNSC.ProjectService.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance,
                    transaction = transID
                };

                ProjectService projectService = new ProjectService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}ProjectService"
                };

                ProjectServiceActualsRequestDTO projectServiceActualsRequestDTO = new ProjectServiceActualsRequestDTO()
                {
                    actualStrDate = raisedDate,
                    districtCode = "SC01",
                    projectNo = sOOrder.OrderNbr.Trim()
                };

                ProjectServiceActualsReplyDTO projectServiceActualsReplyDTO = projectService.actuals(projectOperationContext, projectServiceActualsRequestDTO);
                PLNSC.ProjectService.WarningMessageDTO[] actualErrors = projectServiceActualsReplyDTO.warningsAndInformation;
                if (actualErrors.Length > 0)
                {
                    result = actualErrors[0].message;
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
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        string modifyProject(SOOrder sOOrder)
        {
            int? customerSalesAcctId;
            int? customerSalesSubId;
            int? cogsAcctId;
            int? cogsSubId;
            int? invtAcctId;
            int? invtSubId;
            int? cARAccountID;

            string cogsAcctCd = "";
            string cogsSubCd = "";
            string invtAcctCd = "";
            string invtSubCd = "";
            string custSalesAcctCD = "";
            string custSalesSubCD = "";
            string cARAccountCD = string.Empty;
            string custCD = "";
            string transID = "";

            string result = "";

            try
            {
                ClientConversation.authenticate(userName, password);
                TransactionService transactionService = new TransactionService()
                {
                    Timeout = sessionTimeout,
                    Url = $"{urlPrefix(dbName)}TransactionService"
                };
                PLNSC.TransactionRef.OperationContext transContext = new PLNSC.TransactionRef.OperationContext()
                {
                    district = districtCode,
                    position = positionID,
                    maxInstances = maxInstance
                };

                transID = transactionService.begin(transContext);

                BAccount2 customer = PXSelect<BAccount2,
                    Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                    And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                    And<BAccount.type, Equal<Required<BAccount.type>>>>>>.Select(Base, sOOrder.CustomerID, sOOrder.CustomerLocationID, "CU");

                Location custLocation = PXSelect<Location,
                    Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                    And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(Base, sOOrder.CustomerID, sOOrder.CustomerLocationID);

                customerSalesAcctId = custLocation.CSalesAcctID;
                customerSalesSubId = custLocation.CSalesSubID;
                cARAccountID = custLocation.CARAccountID;

                Account custSalesAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, customerSalesAcctId);
                Account custARAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, cARAccountID);
                Sub custSalesSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, customerSalesSubId);

                custSalesAcctCD = custSalesAccount.AccountCD;
                custSalesSubCD = custSalesSub.SubCD;
                cARAccountCD = custARAccount.AccountCD;
                custCD = customer.AcctCD.Trim();

                foreach (SOLine soLine in Base.Transactions.Select(sOOrder.OrderNbr))
                {
                    InventoryItem inventoryItem = PXSelect<InventoryItem,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, soLine.InventoryID);

                    cogsAcctId = inventoryItem.COGSAcctID;
                    cogsSubId = inventoryItem.COGSSubID;
                    invtAcctId = inventoryItem.InvtAcctID;
                    invtSubId = inventoryItem.InvtSubID;

                    if (cogsAcctId == null && cogsSubId == null)
                    {
                        throw new PXException(CustomMessage.COGSAccountEmpty);
                    }

                    Account itemInvtAcct = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, invtAcctId);
                    Sub itemInvtSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, invtSubId);
                    Account invtCogsAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, cogsAcctId);
                    Sub invtCogsSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, cogsSubId);

                    cogsAcctCd = invtCogsAccount.AccountCD;
                    cogsSubCd = invtCogsSub.SubCD;
                    invtAcctCd = itemInvtAcct.AccountCD;
                    invtSubCd = itemInvtSub.SubCD;

                    if (cogsAcctId != null && cogsSubId != null) break;
                }

                EPEmployee ePEmployee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(Base, sOOrder.OwnerID);
                string originatorId = ePEmployee != null ? ePEmployee.AcctCD : "ADMIN";
                string stockItemAccount = custSalesSubCD.Substring(0, 11).Trim() + cogsSubCd.Substring(11, 4).Trim();// + cogsAcctCd.Trim();
                string nonStockItemAccount = cogsSubCd.Trim();// + cogsAcctCd.Trim();
                string itemInventoryAccount = invtSubCd.Trim() + invtAcctCd.Trim();
                DateTime actStartDate = sOOrder.OrderDate ?? DateTime.Now;
                string raisedDate = actStartDate.ToString("yyyyMMdd");
                string planFinishDate = (sOOrder.RequestDate ?? actStartDate.AddDays(60)).ToString("yyyyMMdd");

                try
                {
                    //ClientConversation.authenticate(userName, password);
                    PLNSC.ProjectService.OperationContext projectOperationContext = new PLNSC.ProjectService.OperationContext()
                    {
                        district = districtCode,
                        position = positionID,
                        maxInstances = maxInstance,
                        transaction = transID
                    };

                    ProjectService projectService = new ProjectService()
                    {
                        Timeout = sessionTimeout,
                        Url = $"{urlPrefix(dbName)}ProjectService"
                    };

                    ProjectServiceModifyRequestDTO projectRequest = new ProjectServiceModifyRequestDTO()
                    {
                        districtCode = "SC01",
                        projectNo = sOOrder.OrderNbr,
                        projDesc = sOOrder.OrderDesc,
                        originatorId = originatorId,
                        raisedDate = raisedDate,
                        planFinDate = planFinishDate,
                        accountCode = cARAccountCD,
                        accountCodeEnabled = true
                    };

                    ProjectServiceModifyReplyDTO projectReply = projectService.modify(projectOperationContext, projectRequest);
                    PLNSC.ProjectService.WarningMessageDTO[] warningMessageDTOs = projectReply.warningsAndInformation;

                    if (warningMessageDTOs.Length > 0)
                    {
                        result = warningMessageDTOs[0].message;
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
                catch (Exception ex)
                {
                    result = ex.Message;
                    transContext.transaction = transID;
                    transactionService.rollback(transContext);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }
        #endregion
    }
}