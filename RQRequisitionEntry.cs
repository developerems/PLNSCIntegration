using EllipseWebServicesClient;
using PLNSC;
using PLNSC.ProjectService;
using PLNSC.ScreenService;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace PX.Objects.RQ
{
    public class RQRequisitionEntry_Extension : PXGraphExtension<RQRequisitionEntry>
    {
        public Boolean RequesRefresh = false;

        #region Requisition Handlers

        [PXDBString(40, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void RQRequisition_Description_CacheAttached(PXCache cache)
        {
        }

        public abstract class usrShipFrom : IBqlField { }
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Receipt Point")]
        [PXSelector(typeof(Search<FOBPoint.fOBPointID>), DescriptionField = typeof(FOBPoint.description), CacheGlobal = true)]
        [PXDefault(typeof(Search<Location.vFOBPointID,
                                 Where<Location.bAccountID, Equal<Current<RQRequisition.vendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        protected void RQRequisition_FOBPoint_CacheAttached(PXCache sender) { }

        public PXAction<RQRequisition> createQuote;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Create Quote", Enabled = false, Visible = false)]
        public virtual IEnumerable CreateQuote(PXAdapter adapter)
        {
            SOOrderEntry sograph = PXGraph.CreateInstance<SOOrderEntry>();
            List<SOOrder> list = new List<SOOrder>(adapter.Get<SOOrder>().Cast<SOOrder>());

            foreach (RQRequisition item in adapter.Get<RQRequisition>().Cast<RQRequisition>())
            {
                RQRequisition result = item;
                //RQRequisitionOrder reqOrder =
                //PXSelectJoin<RQRequisitionOrder,
                //  InnerJoin<SOOrder,
                //         On<SOOrder.orderNbr, Equal<RQRequisitionOrder.orderNbr>,
                //        And<SOOrder.status, Equal<SOOrderStatus.open>>>>,
                //  Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
                //    And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>>>>
                //    .Select(Base, item.ReqNbr);

                RQRequisitionOrder reqOrder =
                    PXSelectJoin<RQRequisitionOrder,
                        InnerJoin<SOOrder,
                            On<SOOrder.orderNbr, Equal<RQRequisitionOrder.orderNbr>>>,
                        Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
                            And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>,
                            And<SOOrder.status, Equal<SOOrderStatus.open>>>>>.Select(Base, item.ReqNbr);


                if (item.CustomerID != null & reqOrder == null)
                {
                    Base.Document.Current = item;

                    bool validateResult = true;
                    foreach (RQRequisitionLine line in Base.Lines.Select(item.ReqNbr))
                    {
                        if (!ValidateOpenState(line, PXErrorLevel.Error))
                            validateResult = false;
                    }
                    if (!validateResult)
                        throw new PXRowPersistingException(typeof(RQRequisition).Name, item, Messages.UnableToCreateOrders);

                    sograph.TimeStamp = Base.TimeStamp;
                    sograph.Document.Current = null;
                    foreach (PXResult<RQRequisitionLine, InventoryItem> r in
                      PXSelectJoin<RQRequisitionLine,
                      LeftJoin<InventoryItem,
                            On<InventoryItem.inventoryID, Equal<RQRequisitionLine.inventoryID>>>,
                      Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisition.reqNbr>>>>.Select(Base, item.ReqNbr))
                    {
                        RQRequisitionLine l = r;
                        RQRequisitionLineExt lExt = l.GetExtension<RQRequisitionLineExt>();
                        InventoryItem i = r;
                        RQBidding bidding =
                          item.VendorID == null ?
                          PXSelect<RQBidding,
                          Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                            And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                            And<RQBidding.orderQty, Greater<decimal0>>>>,
                            OrderBy<Desc<RQBidding.quoteUnitCost>>>
                            .SelectSingleBound(Base, new object[] { l }) :
                          PXSelect<RQBidding,
                          Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                            And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                            And<RQBidding.vendorID, Equal<Current<RQRequisition.vendorID>>,
                            And<RQBidding.vendorLocationID, Equal<Current<RQRequisition.vendorLocationID>>>>>>>
                            .SelectSingleBound(Base, new object[] { l, item });

                        if (sograph.Document.Current == null)
                        {
                            SOOrder order = (SOOrder)sograph.Document.Cache.CreateInstance();
                            order.OrderType = "QT";
                            order = sograph.Document.Insert(order);
                            order = PXCache<SOOrder>.CreateCopy(sograph.Document.Search<SOOrder.orderNbr>(order.OrderNbr));
                            order.CustomerID = item.CustomerID;
                            order.CustomerLocationID = item.CustomerLocationID;
                            order = PXCache<SOOrder>.CreateCopy(sograph.Document.Update(order));
                            order.CuryID = item.CuryID;
                            order.CuryInfoID = CopyCurrenfyInfo(sograph, item.CuryInfoID);
                            sograph.Document.Update(order);
                            sograph.Save.Press();
                            order = sograph.Document.Current;
                            list.Add(order);

                            RQRequisitionOrder link = new RQRequisitionOrder();
                            link.OrderCategory = RQOrderCategory.SO;
                            link.OrderType = order.OrderType;
                            link.OrderNbr = order.OrderNbr;
                            Base.ReqOrders.Insert(link);
                        }

                        SOLine line = (SOLine)sograph.Transactions.Cache.CreateInstance();
                        SOLineExt lineExt = line.GetExtension<SOLineExt>();
                        line.OrderType = sograph.Document.Current.OrderType;
                        line.OrderNbr = sograph.Document.Current.OrderNbr;
                        line = PXCache<SOLine>.CreateCopy(sograph.Transactions.Insert(line));
                        line.InventoryID = l.InventoryID;
                        if (line.InventoryID != null)
                            line.SubItemID = l.SubItemID;
                        line.UOM = l.UOM;
                        line.Qty = l.OrderQty;
                        lineExt.UsrCuryAdditionalCost = lExt.UsrCuryAdditionalCost;
                        lineExt.UsrCuryEstShipCost = lExt.UsrCuryEstShipCost;
                        lineExt.UsrCuryIntermediateCost = lExt.UsrCuryIntermediateCost;
                        lineExt.UsrCuryROKCost = lExt.UsrCuryROKCost;
                        if (l.SiteID != null)
                            line.SiteID = l.SiteID;

                        if (l.IsUseMarkup == true)
                        {
                            string curyID = item.CuryID;
                            decimal profit = (1m + l.MarkupPct.GetValueOrDefault() / 100m);
                            line.ManualPrice = true;
                            decimal unitPrice = l.EstUnitCost.GetValueOrDefault();
                            decimal curyUnitPrice = l.CuryEstUnitCost.GetValueOrDefault();
                            decimal curyTotalCost = l.CuryEstExtCost.GetValueOrDefault();
                            decimal totalCost = l.EstExtCost.GetValueOrDefault();

                            if (bidding != null && bidding.MinQty <= line.OrderQty && bidding.OrderQty >= line.OrderQty)
                            {
                                curyID = (string)Base.Bidding.GetValueExt<RQBidding.curyID>(bidding);
                                unitPrice = bidding.QuoteUnitCost.GetValueOrDefault();
                                curyUnitPrice = bidding.CuryQuoteUnitCost.GetValueOrDefault();
                                curyTotalCost = l.CuryEstExtCost.GetValueOrDefault();
                                totalCost = l.EstExtCost.GetValueOrDefault();
                            }

                            if (curyID == sograph.Document.Current.CuryID)
                                //line.CuryUnitPrice = curyUnitPrice * profit;
                                line.CuryUnitPrice = (curyTotalCost / l.OrderQty) * profit;
                            else
                            {
                                //line.UnitPrice = unitPrice * profit;
                                line.UnitPrice = (totalCost / l.OrderQty) * profit;
                                PXCurrencyAttribute.CuryConvCury<SOLine.curyUnitPrice>(
                                  sograph.Transactions.Cache,
                                  line);
                            }
                        }

                        line = PXCache<SOLine>.CreateCopy(sograph.Transactions.Update(line));
                        RQRequisitionLine upd = PXCache<RQRequisitionLine>.CreateCopy(l);
                        l.QTOrderNbr = line.OrderNbr;
                        l.QTLineNbr = line.LineNbr;
                        Base.Lines.Update(l);
                    }
                    using (PXTransactionScope scope = new PXTransactionScope())
                    {
                        try
                        {
                            if (sograph.IsDirty) sograph.Save.Press();
                            RQRequisition upd = PXCache<RQRequisition>.CreateCopy(item);
                            upd.Quoted = true;
                            result = Base.Document.Update(upd);
                            Base.Save.Press();
                        }
                        catch
                        {
                            Base.Clear();
                            throw;
                        }
                        scope.Complete();
                    }
                }
                else
                {
                    RQRequisition upd = PXCache<RQRequisition>.CreateCopy(item);
                    upd.Quoted = true;
                    result = Base.Document.Update(upd);
                    Base.Save.Press();
                }
                yield return result;
            }
            if (list.Count == 1 && adapter.MassProcess == true)
            {
                sograph.Clear();
                sograph.SelectTimeStamp();
                sograph.Document.Current = list[0];
                throw new PXRedirectRequiredException(sograph, SO.Messages.SOOrder);
            }
        }

        public PXAction<RQRequisition> createQTOrder;
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        [PXUIField(DisplayName = Messages.CreateQuotation)]
        public virtual IEnumerable CreateQTOrder(PXAdapter adapter)
        {
            Base.createQTOrder.PressButton();
            PXGraph.InstanceCreated.AddHandler<SOOrderEntry>(delegate (SOOrderEntry graph)
            {
                Base.FieldUpdated.AddHandler<RQRequisitionLine.qTLineNbr>(delegate (PXCache cache, PXFieldUpdatedEventArgs e)
                {
                    RQRequisition req = Base.Document.Current;
                    RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();
                    RQRequisitionLine reqline = (RQRequisitionLine)e.Row;
                    RQRequisitionLineExt reqlineext = reqline.GetExtension<RQRequisitionLineExt>();

                    if (reqline.QTOrderNbr != null && reqline.QTLineNbr != null)
                    {
                        SOOrder order = graph.Document.Current;
                        SOLine line = PXSelect<SOLine,
                          Where<SOLine.orderType, Equal<SOOrderTypeConstants.quoteOrder>,
                            And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                            And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>
                              .Select(graph, reqline.QTOrderNbr, reqline.QTLineNbr);
                        SOLineExt lineExt = line.GetExtension<SOLineExt>();

                        SOOrder qt = PXSelect<SOOrder,
                            Where<SOOrder.orderType, Equal<SOOrderTypeConstants.quoteOrder>,
                              And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                .Select(graph, reqline.QTOrderNbr);
                        SOOrderExt qtExt = qt.GetExtension<SOOrderExt>();

                        decimal profit = (1m + reqline.MarkupPct.GetValueOrDefault() / 100m);
                        decimal unitcost = (reqline.EstExtCost ?? 0m) / (reqline.OrderQty ?? 0m);
                        decimal extcost = reqlineext.CuryEstExtCost ?? 0m;
                        decimal intermediateCost = reqlineext.UsrCuryIntermediateCost ?? 0m;
                        decimal additionalCost = reqlineext.UsrCuryAdditionalCost ?? 0m;
                        decimal estShipCost = reqlineext.UsrCuryEstShipCost ?? 0;
                        decimal rokCost = reqlineext.UsrCuryROKCost ?? 0;
                        decimal estShipRate = reqlineext.UsrEstShipRate ?? 0;
                        decimal rokRate = reqlineext.UsrROK ?? 0;
                        decimal curyInspectorCost = reqExt.UsrCuryInspectorCost ?? 0;
                        decimal inspectorCost = reqExt.UsrInspectorCost ?? 0;

                        if (line != null)
                        {

                            if (reqline.IsUseMarkup == true)
                            {
                                unitcost = unitcost * profit;
                                extcost = extcost * profit;
                            }

                            if (req.CuryID != order.CuryID)
                            {
                                unitcost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, unitcost);
                                extcost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, extcost);
                                intermediateCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, intermediateCost);
                                additionalCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, additionalCost);
                                estShipCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, estShipCost);
                                rokCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, rokCost);
                            }

                            line.ManualPrice = true;
                            lineExt.UsrCuryAdditionalCost = additionalCost;
                            lineExt.UsrCuryEstShipCost = estShipCost;
                            lineExt.UsrCuryIntermediateCost = intermediateCost;
                            lineExt.UsrCuryROKCost = rokCost;
                            lineExt.UsrEstShipRate = estShipRate;
                            lineExt.UsrROK = rokRate;
                            lineExt.UsrReqNbr = reqline.ReqNbr;
                            lineExt.UsrReqLineNbr = reqline.LineNbr;
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryAdditionalCost>(lineExt, additionalCost);
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryIntermediateCost>(lineExt, intermediateCost);
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryEstShipCost>(lineExt, estShipCost);
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryROKCost>(lineExt, rokCost);
                            graph.Transactions.Cache.SetValueExt<SOLine.curyUnitPrice>(line, unitcost);
                            line = graph.Transactions.Update(line);
                            graph.Transactions.Cache.SetValueExt<SOLine.curyExtPrice>(line, extcost);
                            line = graph.Transactions.Update(line);

                            qtExt.UsrIncoterm = reqExt.UsrIncoterm;
                            qtExt.UsrPreDoNbr = reqExt.UsrPreDoNbr;
                            qtExt.UsrPreDoDate = reqExt.UsrPreDoDate;
                            qtExt.UsrCuryInspectorCost = curyInspectorCost;
                            qtExt.UsrInspectorCost = inspectorCost;
                            qt.OrderDesc = req.Description;
                            qt.CustomerOrderNbr = reqExt.UsrCustOrderNbr;

                            qt = graph.Document.Update(qt);
                        }
                    }
                    else
                    {
                        //this.createQuote.PressButton(adapter);
                    }
                });

                Base.FieldUpdated.AddHandler<RQRequisitionLine.qTOrderNbr>(delegate (PXCache cache, PXFieldUpdatedEventArgs e)
                {
                    RQRequisition req = Base.Document.Current;
                    RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();
                    RQRequisitionLine reqline = (RQRequisitionLine)e.Row;
                    RQRequisitionLineExt reqlineext = reqline.GetExtension<RQRequisitionLineExt>();

                    if (reqline.QTOrderNbr != null && reqline.QTLineNbr != null)
                    {
                        SOOrder order = graph.Document.Current;
                        SOLine line = PXSelect<SOLine,
                          Where<SOLine.orderType, Equal<SOOrderTypeConstants.quoteOrder>,
                            And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                            And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>
                              .Select(graph, reqline.QTOrderNbr, reqline.QTLineNbr);
                        SOLineExt lineExt = line.GetExtension<SOLineExt>();

                        SOOrder qt = PXSelect<SOOrder,
                            Where<SOOrder.orderType, Equal<SOOrderTypeConstants.quoteOrder>,
                              And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                .Select(graph, reqline.QTOrderNbr);
                        SOOrderExt qtExt = qt.GetExtension<SOOrderExt>();

                        decimal profit = (1m + reqline.MarkupPct.GetValueOrDefault() / 100m);
                        decimal unitcost = (reqline.EstExtCost ?? 0m) / (reqline.OrderQty ?? 0m);
                        decimal extcost = reqlineext.CuryEstExtCost ?? 0m;
                        decimal intermediateCost = reqlineext.UsrCuryIntermediateCost ?? 0m;
                        decimal additionalCost = reqlineext.UsrCuryAdditionalCost ?? 0m;
                        decimal estShipCost = reqlineext.UsrCuryEstShipCost ?? 0;
                        decimal rokCost = reqlineext.UsrCuryROKCost ?? 0;
                        decimal estShipRate = reqlineext.UsrEstShipRate ?? 0;
                        decimal rokRate = reqlineext.UsrROK ?? 0;
                        decimal curyInspectorCost = reqExt.UsrCuryInspectorCost ?? 0;
                        decimal inspectorCost = reqExt.UsrInspectorCost ?? 0;

                        if (line != null)
                        {

                            if (reqline.IsUseMarkup == true)
                            {
                                unitcost = unitcost * profit;
                                extcost = extcost * profit;
                            }

                            if (req.CuryID != order.CuryID)
                            {
                                unitcost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, unitcost);
                                extcost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, extcost);
                                intermediateCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, intermediateCost);
                                additionalCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, additionalCost);
                                estShipCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, estShipCost);
                                rokCost = Tools.ConvertCurrency<SOLine.curyInfoID>(cache, line, rokCost);
                            }

                            line.ManualPrice = true;
                            lineExt.UsrCuryAdditionalCost = additionalCost;
                            lineExt.UsrCuryEstShipCost = estShipCost;
                            lineExt.UsrCuryIntermediateCost = intermediateCost;
                            lineExt.UsrCuryROKCost = rokCost;
                            lineExt.UsrEstShipRate = estShipRate;
                            lineExt.UsrROK = rokRate;
                            lineExt.UsrReqNbr = reqline.ReqNbr;
                            lineExt.UsrReqLineNbr = reqline.LineNbr;
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryAdditionalCost>(lineExt, additionalCost);
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryIntermediateCost>(lineExt, intermediateCost);
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryEstShipCost>(lineExt, estShipCost);
                            //graph.Transactions.Cache.SetValueExt<SOLineExt.usrCuryROKCost>(lineExt, rokCost);
                            graph.Transactions.Cache.SetValueExt<SOLine.curyUnitPrice>(line, unitcost);
                            line = graph.Transactions.Update(line);
                            graph.Transactions.Cache.SetValueExt<SOLine.curyExtPrice>(line, extcost);
                            line = graph.Transactions.Update(line);

                            qtExt.UsrIncoterm = reqExt.UsrIncoterm;
                            qtExt.UsrPreDoNbr = reqExt.UsrPreDoNbr;
                            qtExt.UsrPreDoDate = reqExt.UsrPreDoDate;
                            qtExt.UsrCuryInspectorCost = curyInspectorCost;
                            qtExt.UsrInspectorCost = inspectorCost;
                            qtExt.UsrPurchMethod = reqExt.UsrPurchMethod;
                            qt.OrderDesc = req.Description;

                            qt = graph.Document.Update(qt);
                        }
                    }
                    else
                    {
                        //this.createQuote.PressButton(adapter);
                    }
                });
            });

            return Base.CreateQTOrder(adapter);
        }

        public Boolean preventRecursion = false;
        public Boolean preventLoop = false;
        public Boolean preventRepeat = false;

        public PXAction<RQRequisition> createPOOrder;
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        [PXUIField(DisplayName = Messages.CreateOrders)]
        public virtual IEnumerable CreatePOOrder(PXAdapter adapter)
        {
            foreach (RQRequisition item in adapter.Get<RQRequisition>())
            {
                Base.Document.Current = item;
                bool validateResult = true;
                bool qTPendingApproval = false;
                bool qTCustOrderNbrNull = false;

                foreach (RQRequisitionLine line in Base.Lines.Select(item.ReqNbr))
                {
                    if (!ValidateOpenState(line, PXErrorLevel.Error))
                        validateResult = false;
                }

                if (!IsDateCorrect(item))
                    throw new PXRowPersistingException(typeof(RQRequisition).Name, item, CustomMessage.FutureQuoteDate);

                if (!IsQuoteApproved(item))
                    qTPendingApproval = true;

                if (!IsQTCustomerOrderNbrExists(item))
                    qTCustOrderNbrNull = true;

                if (!validateResult)
                    throw new PXRowPersistingException(typeof(RQRequisition).Name, item, Messages.UnableToCreateOrders);

                if(qTPendingApproval)
                    throw new PXRowPersistingException(typeof(RQRequisition).Name, item, CustomMessage.UsdoWaitApproval);

                if (qTCustOrderNbrNull)
                    throw new PXRowPersistingException(typeof(RQRequisition).Name, item, CustomMessage.CustOrderEmpty);

            }

            Base.createPOOrder.PressButton();
            RQRequisition req = Base.Document.Current;
            RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();

            RQBiddingVendor rbv = PXSelect<RQBiddingVendor,
                Where<RQBiddingVendor.reqNbr, Equal<Required<RQBiddingVendor.reqNbr>>,
                And<RQBiddingVendor.vendorID, Equal<Required<RQBiddingVendor.vendorID>>>>>.Select(Base, req.ReqNbr, req.VendorID);

            RQBiddingVendorExt rbvExt = rbv.GetExtension<RQBiddingVendorExt>();

            VendorPaymentMethodDetail vPMBeneficiaryAcctNo = PXSelect<VendorPaymentMethodDetail,
                Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
                And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "1");

            VendorPaymentMethodDetail vPMBeneficiaryName = PXSelect<VendorPaymentMethodDetail,
                Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
                And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "2");

            VendorPaymentMethodDetail vPMBankRoutingNumber = PXSelect<VendorPaymentMethodDetail,
                Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
                And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "3");

            VendorPaymentMethodDetail vPMBankName = PXSelect<VendorPaymentMethodDetail,
                Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
                And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "4");

            VendorPaymentMethodDetail vPMBankAddress = PXSelect<VendorPaymentMethodDetail,
                Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
                And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "5");

            VendorPaymentMethodDetail vPMSwiftCode = PXSelect<VendorPaymentMethodDetail,
                Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
                And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "6");

            BAccount vOwner = PXSelectJoin<BAccount,
                LeftJoin<EPEmployee, On<BAccount.bAccountID, Equal<EPEmployee.bAccountID>>,
                LeftJoin<BAccount2, On<EPEmployee.userID, Equal<BAccount2.ownerID>>>>,
                Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                And<BAccount2.type, Equal<Required<BAccount2.type>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "VE");

            RQBidding rb = PXSelect<RQBidding,
                Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
                And<RQBidding.vendorID, Equal<Required<RQBidding.vendorID>>>>>.Select(Base, req.ReqNbr, req.VendorID);

            PXGraph.InstanceCreated.AddHandler<POOrderEntry>(delegate (POOrderEntry graph)
            {
                graph.RowUpdated.AddHandler<POOrder>(delegate (PXCache cache, PXRowUpdatedEventArgs e)
                {
                    POOrder order = (POOrder)e.Row;

                    if (!preventRepeat && order.OrderNbr != null)
                    {
                        if (rbv.ReqNbr != null)
                        {
                            try
                            {
                                preventRepeat = true;
                                POOrder porder = PXSelect<POOrder,
                                    Where<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>,
                                    And<POOrder.orderType, Equal<Required<POOrder.orderType>>>>>.Select(graph, order.OrderNbr, order.OrderType);

                                POOrderExt poExt = porder.GetExtension<POOrderExt>();

                                //BAccount baAccount = PXSelect<BAccount,
                                //    Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(Base, req.VendorID);

                                if (rbvExt.UsrIncoterm != null)
                                {
                                    poExt.UsrIncoterm = rbvExt.UsrIncoterm;
                                }

                                if (rbvExt.UsrLeadTime != null)
                                {
                                    poExt.UsrLeadTime = rbvExt.UsrLeadTime;
                                }

                                if (rbvExt.UsrRefDueDate != null)
                                {
                                    poExt.UsrRefDueDate = rbvExt.UsrRefDueDate;
                                }

                                if (rbvExt.UsrLeadFrom != null)
                                {
                                    poExt.UsrLeadFrom = rbvExt.UsrLeadFrom;
                                }

                                if (rbvExt.UsrShipFrom != null)
                                {
                                    poExt.UsrShipFrom = rbvExt.UsrShipFrom;
                                }

                                if (rbvExt.UsrShipTo != null)
                                {
                                    poExt.UsrShipTo = rbvExt.UsrShipTo;
                                }

                                if (rbvExt.UsrPaymentTerms != null)
                                {
                                    porder.TermsID = rbvExt.UsrPaymentTerms;
                                }

                                if (vPMBankName != null)
                                {
                                    poExt.UsrAdvisingBank = vPMBankName.DetailValue;
                                }

                                if (vPMBeneficiaryName != null)
                                    poExt.UsrAdvBankBranch = vPMBeneficiaryName.DetailValue;

                                if (vPMBeneficiaryAcctNo != null)
                                    poExt.UsrAdvAccountNbr = vPMBeneficiaryAcctNo.DetailValue;

                                if (vPMSwiftCode != null)
                                    poExt.UsrAdvSWIFT = vPMSwiftCode.DetailValue;

                                if (vPMBankAddress != null)
                                    poExt.UsrAdvBankAddress = vPMBankAddress.DetailValue;

                                //if (vOwner != null)
                                //    porder.EmployeeID = vOwner.BAccountID;

                                if (reqExt.UsrPurchMethod != 2)
                                {
                                    if (rbvExt.UsrLeadTime > 0)
                                    {
                                        if (rbvExt.UsrRefDueDate != null)
                                        {
                                            int offset = rbvExt.UsrLeadTime ?? 1;
                                            porder.ExpectedDate = rbvExt.UsrRefDueDate.Value.AddDays(offset);
                                        }
                                    }

                                }
                                else
                                {
                                    porder.ExpectedDate = rbvExt.UsrRefDueDate.Value.AddDays(30);
                                }

                                porder = graph.Document.Update(porder);
                            }
                            finally
                            {
                                preventRepeat = false;
                            }
                        }
                    }
                });

                graph.RowInserted.AddHandler<POOrder>(delegate (PXCache cache, PXRowInsertedEventArgs e)
                {
                    createPR(adapter, rbv, rbvExt);
                });
            });

            PXGraph.InstanceCreated.AddHandler<SOOrderEntry>(delegate (SOOrderEntry sograph)
            {
                sograph.FieldUpdated.AddHandler<SOLine.origLineNbr>(delegate (PXCache cache, PXFieldUpdatedEventArgs e)
                {
                    SOLine sline = (SOLine)e.Row;
                    SOOrder sorder = sograph.Document.Current;

                    if (!preventLoop && sline.OrigOrderType != null && sline.OrigOrderNbr != null && sline.OrigLineNbr != null)
                    {
                        SOOrder qtorder = PXSelect<SOOrder,
                            Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                            And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(sograph, sline.OrigOrderType, sline.OrigOrderNbr);

                        SOOrderExt qtorderExt = qtorder.GetExtension<SOOrderExt>();

                        SOLine qtline = PXSelect<SOLine,
                            Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                            And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                            And<SOLine.lineNbr, Equal<Required<SOLine.origLineNbr>>>>>>
                            .Select(sograph, sline.OrigOrderType, sline.OrigOrderNbr, sline.OrigLineNbr);

                        SOLineExt qtlineext = qtline.GetExtension<SOLineExt>();

                        if (qtline != null)
                        {
                            try
                            {
                                preventLoop = true;
                                decimal addCost = qtlineext.UsrAdditionalCost ?? 0;
                                decimal curyAddCost = qtlineext.UsrCuryAdditionalCost ?? 0;
                                decimal rokCost = qtlineext.UsrROKCost ?? 0;
                                decimal curyRokCost = qtlineext.UsrCuryROKCost ?? 0;
                                decimal shipCost = qtlineext.UsrEstShipCost ?? 0;
                                decimal curyShipCost = qtlineext.UsrCuryEstShipCost ?? 0;
                                decimal intCost = qtlineext.UsrIntermediateCost ?? 0;
                                decimal curyIntCost = qtlineext.UsrCuryIntermediateCost ?? 0;


                                SOOrder order = PXSelect<SOOrder,
                                    Where<SOOrder.orderType, Equal<SOOrderTypeConstants.salesOrder>,
                                    And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(sograph, sline.OrderNbr);

                                SOOrderExt orderExt = order.GetExtension<SOOrderExt>();

                                SOLine line = PXSelect<SOLine,
                                    Where<SOLine.orderType, Equal<Required<SOOrder.orderType>>,
                                    And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                                    And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>.Select(sograph, sline.OrderType, sline.OrderNbr, sline.LineNbr);

                                SOLineExt lineExt = line.GetExtension<SOLineExt>();

                                orderExt.UsrPreDoNbr = qtorderExt.UsrPreDoNbr;
                                orderExt.UsrPreDoDate = qtorderExt.UsrPreDoDate;

                                if (rbvExt.UsrLeadTime > 0)
                                {
                                    if (rbvExt.UsrRefDueDate != null)
                                    {
                                        int offset = rbvExt.UsrLeadTime ?? 1;
                                        order.RequestDate = rbvExt.UsrRefDueDate.Value.AddDays(offset);
                                    }
                                }

                                order.CustomerOrderNbr = qtorder.CustomerOrderNbr;
                                order.OrderDesc = qtorder.OrderDesc;
                                orderExt.UsrPurchMethod = qtorderExt.UsrPurchMethod;

                                order = sograph.Document.Update(order);

                                lineExt.UsrAdditionalCost = addCost;
                                lineExt.UsrCuryAdditionalCost = curyAddCost;
                                lineExt.UsrEstShipCost = shipCost;
                                lineExt.UsrCuryEstShipCost = curyShipCost;
                                lineExt.UsrIntermediateCost = intCost;
                                lineExt.UsrCuryIntermediateCost = curyIntCost;
                                lineExt.UsrROKCost = rokCost;
                                lineExt.UsrCuryROKCost = curyRokCost;
                                lineExt.UsrReqNbr = qtlineext.UsrReqNbr;
                                lineExt.UsrReqLineNbr = qtlineext.UsrReqLineNbr;

                                line = sograph.Transactions.Update(line);
                            }
                            finally
                            {
                                preventLoop = false;
                            }
                        }
                    }
                });

                //sograph.RowUpdated.AddHandler<SOOrder>(delegate (PXCache cache, PXRowUpdatedEventArgs e)
                //{
                //    SOOrder soOrder = (SOOrder)e.Row;
                //    if (soOrder.OrderNbr != null)
                //    {
                //        if (soOrder.OrderNbr.Contains("D"))
                //        {
                //            createDO(adapter);
                //        }
                //    }

                    
                //});
            });

            Base.RowSelecting.AddHandler<RQBidding>(delegate (PXCache cache, PXRowSelectingEventArgs e)
            {
                RQBidding row = e.Row as RQBidding;
                RQBiddingExt rowext = row.GetExtension<RQBiddingExt>();

                if (row.CuryID == null)
                {
                    using (new PXConnectionScope())
                    {
                        CurrencyInfo ci = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(Base, row.CuryInfoID);
                        if (ci != null)
                        {
                            row.CuryID = ci.CuryID;
                        }
                    }
                    row.CuryQuoteUnitCost = row.QuoteQty > 0 ? rowext.CuryQuoteExtCost / row.QuoteQty : rowext.CuryQuoteExtCost;
                }
            });

            return Base.CreatePOOrder(adapter);
        }

        //public PXAction<RQRequisition> integrate;
        //[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        //[PXUIField(DisplayName = "Integrate")]
        //public virtual IEnumerable Integrate(PXAdapter adapter)
        //{
        //    string hostName = Dns.GetHostName();
        //    string siteName = HostingEnvironment.SiteName;
        //    string url = HttpContext.Current.Request.Url.AbsoluteUri;

        //    this.createPOOrder.Press();

        //    createDO(adapter);

        //    RQRequisition req = Base.Document.Current;
        //    RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();

        //    RQBiddingVendor rbv = PXSelect<RQBiddingVendor,
        //        Where<RQBiddingVendor.reqNbr, Equal<Required<RQBiddingVendor.reqNbr>>,
        //        And<RQBiddingVendor.vendorID, Equal<Required<RQBiddingVendor.vendorID>>>>>.Select(Base, req.ReqNbr, req.VendorID);

        //    RQBiddingVendorExt rbvExt = rbv.GetExtension<RQBiddingVendorExt>();

        //    VendorPaymentMethodDetail vPMBeneficiaryAcctNo = PXSelect<VendorPaymentMethodDetail,
        //        Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
        //        And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
        //        And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "1");

        //    VendorPaymentMethodDetail vPMBeneficiaryName = PXSelect<VendorPaymentMethodDetail,
        //        Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
        //        And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
        //        And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "2");

        //    VendorPaymentMethodDetail vPMBankRoutingNumber = PXSelect<VendorPaymentMethodDetail,
        //        Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
        //        And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
        //        And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "3");

        //    VendorPaymentMethodDetail vPMBankName = PXSelect<VendorPaymentMethodDetail,
        //        Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
        //        And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
        //        And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "4");

        //    VendorPaymentMethodDetail vPMBankAddress = PXSelect<VendorPaymentMethodDetail,
        //        Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
        //        And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
        //        And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "5");

        //    VendorPaymentMethodDetail vPMSwiftCode = PXSelect<VendorPaymentMethodDetail,
        //        Where<VendorPaymentMethodDetail.bAccountID, Equal<Required<VendorPaymentMethodDetail.bAccountID>>,
        //        And<VendorPaymentMethodDetail.locationID, Equal<Required<VendorPaymentMethodDetail.locationID>>,
        //        And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "6");

        //    BAccount vOwner = PXSelectJoin<BAccount,
        //        LeftJoin<EPEmployee, On<BAccount.bAccountID, Equal<EPEmployee.bAccountID>>,
        //        LeftJoin<BAccount2, On<EPEmployee.userID, Equal<BAccount2.ownerID>>>>,
        //        Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
        //        And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
        //        And<BAccount2.type, Equal<Required<BAccount2.type>>>>>>.Select(Base, req.VendorID, req.VendorLocationID, "VE");

        //    createPR(adapter, rbv, rbvExt);

        //    return null;
        //}

        protected virtual void RQRequisition_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            RQRequisition row = (RQRequisition)e.Row;
            RQRequisitionExt rowExt = row.GetExtension<RQRequisitionExt>();
            if (row == null)
                return;

            bool rHold = row.Hold == true;
            bool custOrder = row.CustomerID != null;
            bool gaRequest = (row.CustomerID == null && rowExt.UsrPurchMethod == 2) || row.CustomerID != null;
            bool quoted = row.Quoted ?? false;

            if (row.CustomerID != null)
            {
                ValidatePreDO(sender, e);
            }
            
            if (!gaRequest)
            {
                ValidateDO(sender, e);
            }

            if (RequesRefresh)
            {
                // Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
                CalculateAddCost();
                Base.Lines.View.RequestRefresh();
                Base.Document.View.RequestRefresh();
                RequesRefresh = false;
            }

            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrPreDoNbr>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrPreDoDate>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrGoodsShipRate>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrServShipRate>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrCuryTotalShipCost>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrCuryTotalROK>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrCuryTotalItem>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrCuryInspectorCost>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrCuryGrandTotal>(sender, row, custOrder);
            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrROK>(sender, row, custOrder);

            PXUIFieldAttribute.SetVisible<RQRequisitionExt.usrDONbr>(sender, row, custOrder == false);
            
            //PXDefaultAttribute.SetPersistingCheck<RQRequisitionExt.usrDONbr>(sender, row, gaRequest ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
            //PXDefaultAttribute.SetPersistingCheck<RQRequisitionExt.usrPreDoNbr>(sender, row, custOrder ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            //PXDefaultAttribute.SetPersistingCheck<RQRequisitionExt.usrPreDoDate>(sender, row, custOrder ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            Base.Transfer.SetEnabled(rHold);
            Base.Merge.SetEnabled(rHold);
            Base.ViewLineDetails.SetEnabled(row.Status == RQRequisitionStatus.Released);
            Base.AddRequestLine.SetEnabled(rHold);
            Base.AddRequestContent.SetEnabled(rHold);
            CMSetup setup = Base.cmsetup.Current;
            PXUIFieldAttribute.SetVisible<RQRequest.curyID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
            bool noLines = Base.Lines.Select().Count == 0;

            bool enableCurrenty = false;

            if (noLines)
            {
                if (Base.customer.Current != null)
                    enableCurrenty = Base.customer.Current.AllowOverrideCury == true;
                else if (Base.vendor.Current != null)
                    enableCurrenty = Base.vendor.Current.AllowOverrideCury == true;
                else
                    enableCurrenty = true;
            }

            PXUIFieldAttribute.SetEnabled<RQRequisition.customerID>(sender, row, true);
            PXUIFieldAttribute.SetEnabled<RQRequisition.customerLocationID>(sender, row, true);
            PXUIFieldAttribute.SetEnabled<RQRequisition.curyID>(sender, row, enableCurrenty);
            PXUIFieldAttribute.SetVisible<RQRequisition.quoted>(sender, row, row.CustomerLocationID != null);
            Base.createPOOrder.SetEnabled(IsQuoteApproved(row));
            
            Base.addInvBySite.SetEnabled(rHold && !(bool)row.Released);
            POShipAddress shipAddress = Base.Shipping_Address.Select();
            PORemitAddress remitAddress = Base.Remit_Address.Select();

            bool enableAddressValidation = (row.Released == false && row.Cancelled == false)
                && ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
                || (remitAddress != null && remitAddress.IsDefaultAddress == false && remitAddress.IsValidated == false));

            Base.validateAddresses.SetEnabled(enableAddressValidation);
            Base.Vendors.Cache.AllowUpdate = Base.Vendors.Cache.AllowInsert = Base.Vendors.Cache.AllowDelete = row.Status != RQRequisitionStatus.Released;

            //if (InvokeBaseHandler != null)
            //InvokeBaseHandler(sender, e);

        }

        protected virtual void RQRequisition_UsrCuryInspectorCost_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CalculateAddCost();
        }

        protected virtual void RQRequisition_UsrROK_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            RQRequisition req = Base.Document.Current;
            RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();

            List<RQRequisitionLine> lines = new List<RQRequisitionLine>();
            
            foreach (RQRequisitionLine line in Base.Lines.Select())
            {
                lines.Add(line);
            } 

            for (int i = 0; i < lines.Count; i++)
            {
                RQRequisitionLine reqLine = lines[i];
                RQRequisitionLineExt reqLineExt = reqLine.GetExtension<RQRequisitionLineExt>();


                if (reqExt.UsrROK <= 0)
                {
                    reqLineExt.UsrROK = 0;
                }
                else
                {
                    reqLineExt.UsrROK = reqExt.UsrROK;
                }

                Base.Lines.Cache.Update(reqLine); 
            }

            CalculateAddCost();
        }

        protected virtual void RQRequisition_UsrGoodsShipRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            RQRequisition req = Base.Document.Current;
            RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();

            List<RQRequisitionLine> lines = new List<RQRequisitionLine>();

            foreach (RQRequisitionLine line in Base.Lines.Select())
            {
                lines.Add(line);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                RQRequisitionLine reqLine = lines[i];
                RQRequisitionLineExt reqLineExt = reqLine.GetExtension<RQRequisitionLineExt>();

                if (reqLine.LineType == PO.POLineType.GoodsForInventory || reqLine.LineType == PO.POLineType.GoodsForSalesOrder ||
                    reqLine.LineType == PO.POLineType.GoodsForDropShip || reqLine.LineType == PO.POLineType.GoodsForManufacturing ||
                    reqLine.LineType == PO.POLineType.GoodsForReplenishment)
                {
                    reqLineExt.UsrEstShipRate = reqExt.UsrGoodsShipRate;
                }
                else
                {
                    reqLineExt.UsrEstShipRate = reqExt.UsrServShipRate;
                }

                Base.Lines.Cache.Update(reqLine);
            }

            CalculateAddCost();
        }

        protected virtual void RQRequisition_UsrServShipRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            RQRequisition req = Base.Document.Current;
            RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();

            List<RQRequisitionLine> lines = new List<RQRequisitionLine>();

            foreach (RQRequisitionLine line in Base.Lines.Select())
            {
                lines.Add(line);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                RQRequisitionLine reqLine = lines[i];
                RQRequisitionLineExt reqLineExt = reqLine.GetExtension<RQRequisitionLineExt>();

                if (reqLine.LineType == PO.POLineType.GoodsForInventory || reqLine.LineType == PO.POLineType.GoodsForSalesOrder ||
                    reqLine.LineType == PO.POLineType.GoodsForDropShip || reqLine.LineType == PO.POLineType.GoodsForManufacturing ||
                    reqLine.LineType == PO.POLineType.GoodsForReplenishment)
                {
                    reqLineExt.UsrEstShipRate = reqExt.UsrGoodsShipRate;
                }
                else
                {
                    reqLineExt.UsrEstShipRate = reqExt.UsrServShipRate;
                }

                Base.Lines.Cache.Update(reqLine);
            }

            CalculateAddCost();
        }

        protected virtual void RQRequisition_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            RQRequisition row = e.Row as RQRequisition;

            foreach (RQRequisitionLine line in Base.Lines.Select())
            {
                RQRequisitionLineExt lineext = Base.Lines.Cache.GetExtension<RQRequisitionLineExt>(line);
                if (row.VendorID == null || row.VendorLocationID == null)
                {
                    lineext.UsrPatternCost = 0;
                    lineext.UsrCuryPatternCost = 0;
                }
                else
                {
                    RQBidding bidding = PXSelect<RQBidding,
                      Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
                        And<RQBidding.lineNbr, Equal<Required<RQBidding.lineNbr>>,
                        And<RQBidding.vendorID, Equal<Required<RQBidding.vendorID>>,
                        And<RQBidding.vendorLocationID, Equal<Required<RQBidding.vendorLocationID>>>>>>>.Select(Base, line.ReqNbr, line.LineNbr, row.VendorID, row.VendorLocationID);
                    if (bidding != null)
                    {
                        RQBiddingExt biddingext = bidding.GetExtension<RQBiddingExt>();

                        //Pattern
                        lineext.UsrCuryPatternCost = Tools.ConvertCurrency<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, biddingext.UsrPatternCost ?? 0);
                        //Unit Cost
                        line.CuryEstUnitCost = Tools.ConvertCurrency<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, bidding.QuoteUnitCost ?? 0);
                        //Ext Cost
                        line.CuryEstExtCost = Tools.ConvertCurrency<RQRequisitionLine.curyInfoID>(Base.Lines.Cache, line, bidding.QuoteExtCost ?? 0);
                    }
                }
                Base.Lines.Update(line);
            }
        }
        #endregion

        #region RequisitionLine Handler

        protected virtual void RQRequisitionLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if(!sender.ObjectsEqual<RQRequisitionLineExt.usrCuryIntermediateCost, RQRequisitionLineExt.usrCuryAdditionalCost, 
                RQRequisitionLineExt.usrCuryEstShipCost, RQRequisitionLineExt.usrCuryROKCost>(e.Row, e.OldRow))
            {
                RequesRefresh = true;
            }
        }

        protected virtual void RQRequisitionLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            CalculateAddCost();
        }

        protected virtual void RQRequisitionLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            RQRequisitionLine row = (RQRequisitionLine)e.Row;
            RQRequisitionLineExt rowExt = row.GetExtension<RQRequisitionLineExt>();

            if (row == null)
                return;

            PXPersistingCheck persistingCheckMode = Base.Document.Current.Hold == true
                ? PXPersistingCheck.Nothing
                : PXPersistingCheck.NullOrBlank;

            PXDefaultAttribute.SetPersistingCheck<RQRequisitionLine.inventoryID>(sender, row, persistingCheckMode);

            if (row.InventoryID == null)
            {
                if (Base.Document.Current.Hold != true)
                    sender.DisplayFieldError<RQRequisitionLine.inventoryID>(row, null, ErrorMessages.FieldIsEmpty,
                                                                            PXUIFieldAttribute.GetDisplayName<RQRequisitionLine.inventoryID>(sender));
                else
                    sender.ClearFieldSpecificError<RQRequisitionLine.inventoryID>(row, ErrorMessages.FieldIsEmpty,
                                                                                  PXUIFieldAttribute.GetDisplayName<RQRequisitionLine.inventoryID>(sender));
            }

            //PXUIFieldAttribute.SetEnabled<RQRequisitionLine.uOM>(sender, row, !(row.ByRequest == true));
            PXUIFieldAttribute.SetEnabled<RQRequisitionLine.orderQty>(sender, row, !(row.ByRequest == true));
            PXUIFieldAttribute.SetEnabled<RQRequestLine.subItemID>(sender, row, row.InventoryID != null && row.LineType == POLineType.GoodsForInventory);

            PXUIFieldAttribute.SetEnabled<RQRequisitionLine.siteID>(sender, row, true);
            PXUIFieldAttribute.SetEnabled<RQRequisitionLine.markupPct>(sender, row, row.IsUseMarkup == true);

            PXUIFieldAttribute.SetEnabled<RQRequisitionLine.lineType>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.inventory>());

            if (Base.Document.Current.Status == RQRequisitionStatus.PendingApproval ||
            Base.Document.Current.Status == RQRequisitionStatus.Open ||
            Base.Document.Current.Status == RQRequisitionStatus.PendingQuotation)
            {
                this.ValidateOpenState(row, PXErrorLevel.Warning);
            }

            PXUIFieldAttribute.SetEnabled<RQRequisitionLine.alternateID>(sender, row, Base.Document.Current.VendorLocationID != null);

            ValidateItemQty(sender, e);
        }

        #endregion

        #region Functions

        public virtual void CalculateAddCost()
        {
            RQRequisition req = Base.Document.Current;
            RQRequisitionExt reqExt = req.GetExtension<RQRequisitionExt>();

            Decimal intermediate = 0;
            List<RQRequisitionLine> lines = new List<RQRequisitionLine>();

            foreach (RQRequisitionLine line in Base.Lines.Select())
            {
                RQRequisitionLineExt lineExt = line.GetExtension<RQRequisitionLineExt>();
                intermediate += lineExt.UsrCuryIntermediateCost ?? 0;

                lines.Add(line);
            }

            Decimal additional = (reqExt.UsrCuryInspectorCost ?? 0);
            Decimal running = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                RQRequisitionLine line = lines[i];
                RQRequisitionLineExt lineExt = line.GetExtension<RQRequisitionLineExt>();

                if (intermediate <= 0)
                {
                    lineExt.UsrCuryAdditionalCost = 0;
                }
                else
                {
                    if (i >= (lines.Count - 1))
                    {
                        lineExt.UsrCuryAdditionalCost = additional - running;
                    }
                    else
                    {
                        Decimal val = additional * ((lineExt.UsrCuryIntermediateCost ?? 0) / intermediate);
                        lineExt.UsrCuryAdditionalCost = PXDBCurrencyAttribute.RoundCury<RQRequisition.curyInfoID>(Base.Lines.Cache, line, val);
                        running += lineExt.UsrCuryAdditionalCost ?? 0;
                    }
                }

                Base.Lines.Cache.Update(line);
            }
        }

        private bool ValidateOpenState(RQRequisitionLine row, PXErrorLevel level)
        {
            bool result = true;
            Type[] requestOnOpen =
              row.LineType == POLineType.GoodsForInventory && row.InventoryID != null
                ? new Type[] { typeof(RQRequisitionLine.uOM), typeof(RQRequisitionLine.siteID), typeof(RQRequisitionLine.subItemID) }
                : row.LineType == POLineType.NonStock
                    ? new Type[] { typeof(RQRequisitionLine.uOM), typeof(RQRequisitionLine.siteID), }
                    : new Type[] { typeof(RQRequisitionLine.uOM) };


            foreach (Type type in requestOnOpen)
            {
                object value = Base.Lines.Cache.GetValue(row, type.Name);
                if (value == null)
                {
                    Base.Lines.Cache.RaiseExceptionHandling(type.Name, row, null,
                      new PXSetPropertyException(Messages.ShouldBeDefined, level));
                    result = false;
                }
                else
                    Base.Lines.Cache.RaiseExceptionHandling(type.Name, row, value, null);
            }
            return result;
        }

        private long? CopyCurrenfyInfo(PXGraph graph, long? SourceCuryInfoID)
        {
            CurrencyInfo curryInfo = Base.currencyinfo.Select(SourceCuryInfoID);
            curryInfo.CuryInfoID = null;
            graph.Caches[typeof(CurrencyInfo)].Clear();
            curryInfo = (CurrencyInfo)graph.Caches[typeof(CurrencyInfo)].Insert(curryInfo);
            return curryInfo.CuryInfoID;
        }

        protected virtual bool IsQuoteApproved(RQRequisition doc)
        {
            bool result = true;

            SOOrder orders = PXSelectJoin<SOOrder,
                LeftJoin<RQRequisitionOrder,
                             On<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>,
                             And<RQRequisitionOrder.orderType, Equal<SOOrder.orderType>,
                            And<RQRequisitionOrder.orderNbr, Equal<SOOrder.orderNbr>>>>>,
            Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisition.reqNbr>>,
            And<RQRequisitionOrder.orderType, Equal<Required<RQRequisitionOrder.orderType>>,
            And<SOOrder.status, Equal<SOOrderStatus.pendingApproval>>>>>.Select(Base, doc.ReqNbr, "QT");

            if (orders != null)
                result = false;

            return result;
        }

        protected virtual bool IsDateCorrect(RQRequisition doc)
        {
            bool result = true;

            SOOrder orders = PXSelectJoin<SOOrder,
                LeftJoin<RQRequisitionOrder,
                             On<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>,
                             And<RQRequisitionOrder.orderType, Equal<SOOrder.orderType>,
                            And<RQRequisitionOrder.orderNbr, Equal<SOOrder.orderNbr>>>>>,
            Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisition.reqNbr>>,
            And<RQRequisitionOrder.orderType, Equal<Required<RQRequisitionOrder.orderType>>>>>.Select(Base, doc.ReqNbr, "QT");

            if (orders != null)
            {
                string ordDate = (orders.OrderDate ?? DateTime.Now).ToString("yyyyMMdd");
                string nowDate = DateTime.Now.ToString("yyyyMMdd");

                int nOrdDate = 0;
                int nNowDate = 0;

                if (Int32.TryParse(ordDate.Trim(), out nOrdDate))
                {
                    if (Int32.TryParse(nowDate.Trim(), out nNowDate))
                    {
                        if (nOrdDate > nNowDate)
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        protected virtual bool IsQTCustomerOrderNbrExists(RQRequisition doc)
        {
            bool result = true;

            SOOrder orders = PXSelectJoin<SOOrder,
                LeftJoin<RQRequisitionOrder,
                             On<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>,
                             And<RQRequisitionOrder.orderType, Equal<SOOrder.orderType>,
                            And<RQRequisitionOrder.orderNbr, Equal<SOOrder.orderNbr>>>>>,
            Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisition.reqNbr>>,
            And<RQRequisitionOrder.orderType, Equal<Required<RQRequisitionOrder.orderType>>,
            And<SOOrder.status, Equal<SOOrderStatus.open>,
            And<SOOrder.customerOrderNbr, IsNull>>>>>.Select(Base, doc.ReqNbr, "QT");

            if (orders != null)
                result = false;

            return result;
        }

        public virtual void ValidatePreDO(PXCache sender, PXRowSelectedEventArgs e)
        {
            RQRequisition row = (RQRequisition)e.Row;
            if (row == null) return;

            RQRequisitionExt rowExt = row.GetExtension<RQRequisitionExt>();
            bool isErr = false;

            if(row.Hold == false && row.Released == false)
            {
                if(rowExt.UsrPreDoDate == null)
                {
                    sender.RaiseExceptionHandling<RQRequisitionExt.usrPreDoDate>(e.Row, rowExt.UsrPreDoDate, new PXSetPropertyException(CustomMessage.PredoDateCantBeEmpty));
                    isErr = true;
                }

                if (rowExt.UsrPreDoNbr == null)
                {
                    sender.RaiseExceptionHandling<RQRequisitionExt.usrPreDoNbr>(e.Row, rowExt.UsrPreDoNbr, new PXSetPropertyException(CustomMessage.PredoNbrCantBeEmpty));
                    isErr = true;
                }
            }

            if (!isErr)
            {
                sender.RaiseExceptionHandling<RQRequisitionExt.usrPreDoDate>(e.Row, null, null);
                sender.RaiseExceptionHandling<RQRequisitionExt.usrPreDoNbr>(e.Row, null, null);
            }
        }

        public virtual void ValidateDO(PXCache sender, PXRowSelectedEventArgs e)
        {
            RQRequisition row = (RQRequisition)e.Row;
            if (row == null) return;

            RQRequisitionExt rowExt = row.GetExtension<RQRequisitionExt>();
            bool isErr = false;

            if (row.Hold == false && row.Released == false)
            {
                if (rowExt.UsrDONbr == null)
                {
                    sender.RaiseExceptionHandling<RQRequisitionExt.usrDONbr>(e.Row, rowExt.UsrDONbr, new PXSetPropertyException(CustomMessage.DoNbrCantBeEmpty));
                    isErr = true;
                }
            }

            if (!isErr)
            {
                sender.RaiseExceptionHandling<RQRequisitionExt.usrDONbr>(e.Row, null, null);
            }
        }

        public virtual void ValidateItemQty(PXCache sender, PXRowSelectedEventArgs e)
        {
            RQRequisitionLine row = (RQRequisitionLine)e.Row;
            if (row == null)
                return;
            bool isErr = false;

            if (row.OrderQty.Equals(0))
            {
                sender.RaiseExceptionHandling<RQRequisitionLine.orderQty>(e.Row, row.OrderQty, new PXSetPropertyException(CustomMessage.QtyMustGreaterZero));
                isErr = true;
            }

            if (isErr)
            {
                sender.RaiseExceptionHandling<RQRequisitionLine.orderQty>(e.Row, null, null);
            }
        }

        public class PXLongOperationCallBack : IPXCustomInfo
        {
            public void Complete(PXLongRunStatus status, PXGraph graph)
            {
                switch (status)
                {
                    case PXLongRunStatus.Aborted:
                        break;

                    case PXLongRunStatus.Completed:
                        break;

                    case PXLongRunStatus.InProcess:
                        break;

                    case PXLongRunStatus.NotExists:
                        break;
                }
            }
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

        static class PSContextClass
        {
            private static PLNSC.ProjectService.OperationContext c = new PLNSC.ProjectService.OperationContext();
            public static PLNSC.ProjectService.OperationContext projectContext
            {
                get { return c; }
                set { c = value; }
            }
        }

        public virtual IEnumerable createDO(PXAdapter adapter)
        {
            bool loggedIn = false;
            //string hostName = Dns.GetHostName();
            //string url = HttpContext.Current.Request.Url.AbsoluteUri;
            //string siteName = HostingEnvironment.SiteName;

            foreach (SOOrder soOrder in adapter.Get<SOOrder>())
            {
                if (soOrder == null)
                {
                    return null;
                }

                PSContextClass.projectContext.district = "SC01";
                PSContextClass.projectContext.position = "INTPO";
                PSContextClass.projectContext.maxInstances = 100;

                ProjectService projectService = new ProjectService();
                ProjectServiceCreateRequestDTO projectRequest = new ProjectServiceCreateRequestDTO();
                ProjectServiceCreateReplyDTO projectReply = new ProjectServiceCreateReplyDTO();

                try
                {
                    ClientConversation.authenticate("ADMIN", "");
                }
                catch (Exception ex)
                {
                    throw new PXException(ex.Message);
                }
                loggedIn = true;

                if (loggedIn == true)
                {
                    try
                    {
                        projectService.Timeout = 3600000;
                        ClientConversation.authenticate("ADMIN", "");

                        projectRequest.districtCode = "SC01";
                        projectRequest.projectNo = soOrder.OrderNbr;
                        projectRequest.projDesc = soOrder.OrderDesc;
                        //projectRequest.accountCode = soOrder.ac
                    }
                    catch (Exception ex)
                    {
                        throw new PXException(ex.Message);
                    }
                }
            }

            return adapter.Get();
        }
        
        public virtual IEnumerable createPR(PXAdapter adapter, RQBiddingVendor rbv, RQBiddingVendorExt rbvExt)
        {
            //HttpRequest request = HttpContext.Current.Request;
            //HttpResponse response = HttpContext.Current.Response;
            //HttpContext httpContext = new HttpContext(request, response);

            //Uri uri = httpContext.Request.UrlReferrer;

            ScreenDTO screenReply = new ScreenDTO();
            ScreenSubmitRequestDTO submitRequest = new ScreenSubmitRequestDTO();

            bool loggedIn = false;
            bool modifyFlag = false;
            string screenName = "";
            string errMess = "";
            string doNbr = "";

            foreach (RQRequisition requisition in adapter.Get<RQRequisition>())
            {
                if (requisition.VendorID == null)
                {
                    throw new PXException(CustomMessage.EmptyVendor);
                }

                RQRequisitionExt requisitionExt = requisition.GetExtension<RQRequisitionExt>();
                if (requisitionExt != null)
                {
                    doNbr = requisitionExt.UsrDONbr != null ? requisitionExt.UsrDONbr.Trim() : string.Empty;
                }

                foreach (RQRequisitionLine line in Base.Lines.Select(requisition.ReqNbr))
                {
                    try
                    {
                        ClientConversation.authenticate("ADMIN", "");
                    }
                    catch (Exception ex)
                    {
                        throw new PXException(ex.Message);
                    }
                    loggedIn = true;

                    if (loggedIn == true)
                    {
                        try
                        {
                            ClientConversation.authenticate("ADMIN", "");
                            ScreenService screenService = new ScreenService()
                            {
                                Timeout = 3600000
                            };
                            PLNSC.ScreenService.OperationContext screenOperationContext = new PLNSC.ScreenService.OperationContext()
                            {
                                district = "SC01",
                                position = "INTPO",
                                maxInstances = 1
                            };
                            screenReply = screenService.executeScreen(screenOperationContext, "MSO230");
                            screenName = screenReply.mapName;

                            if (screenName != "MSM230A")
                            {
                                throw new PXException(CustomMessage.NotMSO230);
                            }

                            ScreenNameValueDTO[] fields = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                            fields[0].fieldName = "OPTION1I";
                            fields[0].value = "1";
                            fields[1].fieldName = "PREQ_NO1I";
                            fields[1].value = line.ReqNbr;
                            fields[2].fieldName = "PREQ_ITEM_NO1I";
                            fields[2].value = line.LineNbr.ToString();

                            submitRequest.screenFields = fields;

                            submitRequest.screenKey = "1"; // OK
                            screenReply = screenService.submit(screenOperationContext, submitRequest);

                            screenName = screenReply.mapName;
                            errMess = screenReply.message;
                            if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                            {
                                if (errMess.Trim().Contains("ITEM NUMBER ALREADY USED"))
                                {
                                    ScreenNameValueDTO[] fieldsMod = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                                    fieldsMod[0].fieldName = "OPTION1I";
                                    fieldsMod[0].value = "3";
                                    fieldsMod[1].fieldName = "PREQ_NO1I";
                                    fieldsMod[1].value = line.ReqNbr;
                                    fieldsMod[2].fieldName = "PREQ_ITEM_NO1I";
                                    fieldsMod[2].value = line.LineNbr.ToString();

                                    submitRequest.screenFields = fieldsMod;
                                    submitRequest.screenKey = "1"; // OK
                                    screenReply = screenService.submit(screenOperationContext, submitRequest);

                                    screenName = screenReply.mapName;
                                    errMess = screenReply.message;
                                    modifyFlag = true;
                                }
                                else
                                {
                                    throw new PXException(errMess.Trim());
                                }
                            }

                            if (line.LineNbr.ToString() != null)
                            {
                                String dataLine = line.LineNbr.ToString();
                                ScreenFieldDTO[] screenFieldDTOs = screenReply.screenFields;

                                if (screenName == "MSM23BA")
                                {
                                    try
                                    {
                                        ScreenNameValueDTO[] fields23b = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                                        String currentDate = DateTime.Now.ToString("yyyyMMdd");

                                        fields23b[0].fieldName = "REQ_BY_DATE1I";
                                        if (rbvExt.UsrRefDueDate != null)
                                        {
                                            int offset = rbvExt.UsrLeadTime ?? 30;
                                            fields23b[0].value = rbvExt.UsrRefDueDate.Value.AddDays(offset).ToString("yyyyMMdd");
                                        }
                                        else
                                        {
                                            fields23b[0].value = rbvExt.UsrRefDueDate.Value.AddDays(30).ToString("yyyyMMdd");
                                        }

                                        fields23b[1].fieldName = "PRIORITY_CODE1I";
                                        fields23b[1].value = "1";
                                        fields23b[2].fieldName = "DEL_MOD1I";
                                        fields23b[2].value = "Y";

                                        submitRequest.screenFields = fields23b;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new PXException(ex.Message);
                                    }

                                    try
                                    {
                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenOperationContext, submitRequest);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new PXException(ex.Message);
                                    }

                                    screenName = screenReply.mapName;
                                    errMess = screenReply.message;
                                    if (errMess.Trim() != "")
                                    {
                                        throw new PXException(errMess.Trim());
                                    }
                                }

                                if (screenName == "MSM232A")
                                {
                                    string costAllocAcct = "";
                                    if (line.LineType == POLineType.GoodsForInventory || 
                                        line.LineType == POLineType.GoodsForSalesOrder || 
                                        line.LineType == POLineType.GoodsForDropShip || 
                                        line.LineType == POLineType.GoodsForManufacturing || 
                                        line.LineType == POLineType.GoodsForReplenishment)
                                    {
                                        Account invtAccount = PXSelectJoin<Account,
                                            InnerJoin<InventoryItem,
                                            On<Account.accountID, Equal<InventoryItem.invtAcctID>>>,
                                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, line.InventoryID);
                                        costAllocAcct = invtAccount.AccountCD.Trim();
                                    }
                                    else
                                    {
                                        Account cogsAccount = PXSelectJoin<Account,
                                            InnerJoin<InventoryItem,
                                            On<Account.accountID, Equal<InventoryItem.cOGSAcctID>>>,
                                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, line.InventoryID);

                                        Sub cogsSub = PXSelectJoin<Sub,
                                            InnerJoin<InventoryItem,
                                            On<Sub.subID, Equal<InventoryItem.cOGSSubID>>>,
                                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, line.InventoryID);

                                        SOOrder dONumber;

                                        if (requisitionExt.UsrPurchMethod != 2)
                                        {
                                            dONumber = PXSelect<SOOrder, Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>.Select(Base, requisitionExt.UsrDONbr);
                                            int custId = dONumber.CustomerID ?? 0;
                                            int custLocationId = dONumber.CustomerLocationID ?? 0;

                                            BAccount2 customer = PXSelect<BAccount2,
                                                Where<BAccount2.bAccountID, Equal<Required<BAccount2.bAccountID>>,
                                                And<BAccount2.defLocationID, Equal<Required<BAccount2.defLocationID>>,
                                                And<BAccount2.type, Equal<Required<BAccount2.type>>>>>>.Select(Base, custId, custLocationId, "CU");

                                            Location custLocation = PXSelect<Location,
                                                Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                                                And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(Base, custId, custLocationId);

                                            string customerAcctCD = customer.AcctCD != null ? customer.AcctCD.Trim() : " ";
                                            int custSalesSub = custLocation.CSalesSubID ?? 0;
                                            Sub customerSub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(Base, custSalesSub);
                                            string custSub = customerSub.SubCD.Trim();
                                            costAllocAcct = custSub + cogsAccount.AccountCD.Trim();
                                        }
                                        else
                                        {
                                            costAllocAcct = (cogsSub != null ? cogsSub.SubCD.Trim() : string.Empty) + (cogsAccount != null ? cogsAccount.AccountCD.Trim() : string.Empty);
                                        }
                                    }

                                    if (requisition.CustomerID != null)
                                    {
                                        ScreenNameValueDTO[] fields232 = { new ScreenNameValueDTO() };

                                        fields232[0].fieldName = "COST_CEN_ACCT1I1";
                                        fields232[0].value = "100605210";

                                        submitRequest.screenFields = fields232;
                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenOperationContext, submitRequest);
                                        errMess = screenReply.message;
                                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                                        {
                                            throw new PXException(errMess.Trim());
                                        }
                                    }
                                    else
                                    {
                                        ScreenNameValueDTO[] fields232 = { new ScreenNameValueDTO(), new ScreenNameValueDTO(), new ScreenNameValueDTO() };

                                        fields232[0].fieldName = "COST_CEN_ACCT1I1";
                                        fields232[0].value = costAllocAcct;
                                        fields232[1].fieldName = "WO_PROJECT1I1";
                                        fields232[1].value = doNbr;
                                        fields232[2].fieldName = "PROJECT_IND1I1";
                                        fields232[2].value = doNbr != "" ? "P" : string.Empty;

                                        submitRequest.screenFields = fields232;
                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenOperationContext, submitRequest);
                                        errMess = screenReply.message;
                                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                                        {
                                            throw new PXException(errMess.Trim());
                                        }
                                    }

                                    if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                                    {
                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenOperationContext, submitRequest);
                                    }

                                    screenName = screenReply.mapName;
                                    errMess = screenReply.message;
                                    if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                                    {
                                        throw new PXException(errMess.Trim());
                                    }
                                }

                                if (screenName == "MSM23EA")
                                {
                                    try
                                    {
                                        ScreenNameValueDTO[] fields23e = { new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO(),
                                            new ScreenNameValueDTO() };

                                        string lineType = "";
                                        if (line.LineType == "GI")
                                        {
                                            lineType = "G";
                                        }
                                        else
                                        {
                                            lineType = "S";
                                        }

                                        fields23e[0].fieldName = "PREQ_TYPE1I";
                                        fields23e[0].value = lineType;

                                        fields23e[1].fieldName = "QTY_REQD1I";

                                        if (lineType == "G")
                                        {
                                            fields23e[1].value = line.OrderQty.ToString();
                                        }
                                        else
                                        {
                                            fields23e[1].value = "";
                                        }

                                        RQBidding rb = PXSelect<RQBidding,
                                            Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
                                            And<RQBidding.vendorID, Equal<Required<RQBidding.vendorID>>,
                                            And<RQBidding.lineNbr, Equal<Required<RQBidding.lineNbr>>>>>>.Select(Base, requisition.ReqNbr, requisition.VendorID, line.LineNbr);

                                        CurrencyInfo currencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(Base, rb.CuryInfoID);

                                        String rbCuryId = rb.CuryID;
                                        String currencyType = currencyInfo.CuryID.Trim();
                                        String rbCuryQuoteUnitCost = Math.Round(rb.CuryQuoteUnitCost ?? 0, 2).ToString();
                                        String rbQty = Math.Round(rb.QuoteQty ?? 0, 2).ToString();
                                        String rbExtCost = (Math.Round(rb.CuryQuoteUnitCost ?? 0, 2) * Math.Round(rb.QuoteQty ?? 0, 2)).ToString();
                                        String rbLineNbr = rb.LineNbr.ToString();

                                        BAccount rbVendor = PXSelect<BAccount,
                                            Where<BAccount.bAccountID,
                                            Equal<Required<BAccount.bAccountID>>>>.Select(Base, rb.VendorID);

                                        String rbVendorCD = rbVendor.AcctCD.Trim();

                                        fields23e[2].fieldName = "UOM1I";
                                        fields23e[2].value = line.UOM;

                                        fields23e[3].fieldName = "EST_PRICE1I";
                                        if (lineType == "G")
                                        {
                                            fields23e[3].value = rbCuryQuoteUnitCost;
                                        }
                                        else
                                        {
                                            fields23e[3].value = rbExtCost;
                                        }

                                        String descLine = line.Description != null ? line.Description.Trim() : "";

                                        if (descLine != "")
                                        {
                                            Regex regex = new Regex("[*'\",_&#^@%]±");
                                            descLine = Regex.Replace(descLine, "[*'\",_&#^@%]±", delegate (Match match)
                                            {
                                                string v = match.ToString();
                                                return v.Replace(";", string.Empty);
                                            });
                                            //descLine = regex.Replace(descLine, string.Empty);
                                            descLine = descLine.Trim();
                                        }

                                        String descLine1 = "";
                                        String descLine2 = "";
                                        String descLine3 = "";

                                        if (descLine != "")
                                        {
                                            if (descLine.Length > 40)
                                            {
                                                descLine1 = descLine.Substring(0, 40).Trim();
                                                if (descLine.Substring(40).Trim().Length > 40)
                                                {
                                                    descLine2 = descLine.Substring(40, 40).Trim();
                                                    if (descLine.Substring(80).Trim().Length > 0)
                                                    {
                                                        if (descLine.Substring(80).Trim().Length > 40)
                                                        {
                                                            descLine3 = descLine.Substring(80, 40).Trim();
                                                        }
                                                        else
                                                        {
                                                            descLine3 = descLine.Substring(80).Trim();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        descLine3 = "";
                                                    }
                                                }
                                                else
                                                {
                                                    descLine2 = descLine.Substring(40).Trim();
                                                }
                                            }
                                            else
                                            {
                                                descLine1 = descLine;
                                                descLine2 = "";
                                                descLine3 = "";
                                            }
                                        }

                                        fields23e[4].fieldName = "DESC_LINE_11I";
                                        fields23e[4].value = descLine1;

                                        fields23e[5].fieldName = "DESC_LINE_21I";
                                        fields23e[5].value = descLine2;

                                        fields23e[6].fieldName = "DESC_LINE_31I";
                                        fields23e[6].value = descLine3;

                                        fields23e[7].fieldName = "ACT_GROSS_PR1I";
                                        if (lineType == "G")
                                        {
                                            fields23e[7].value = rbCuryQuoteUnitCost;
                                        }
                                        else
                                        {
                                            fields23e[7].value = rbExtCost;
                                        }

                                        BAccount vendor = PXSelect<BAccount,
                                            Where<BAccount.bAccountID,
                                            Equal<Required<BAccount.bAccountID>>>>.Select(Base, requisition.VendorID);

                                        fields23e[8].fieldName = "SUPPLIER_NO1I";
                                        fields23e[8].value = vendor.AcctCD.Trim();

                                        fields23e[9].fieldName = "CURRENCY_TYPE1I";
                                        fields23e[9].value = currencyType;

                                        fields23e[10].fieldName = "PRICE_CODE1I";
                                        fields23e[10].value = rbvExt.UsrPaymentTerms != null ? rbvExt.UsrPaymentTerms.Trim() : "NM";

                                        fields23e[11].fieldName = "SUPP_LEAD_TIME1I";
                                        fields23e[11].value = rbvExt.UsrLeadTime != null ? rbvExt.UsrLeadTime.ToString() : "30";

                                        fields23e[12].fieldName = "FREIGHT_CODE1I";
                                        fields23e[12].value = "NA";

                                        fields23e[13].fieldName = "DELIV_LOCATION1I";
                                        fields23e[13].value = "02";

                                        fields23e[14].fieldName = "PURCH_OFFICER1I";
                                        fields23e[14].value = "ADMIN";

                                        submitRequest.screenFields = fields23e;

                                        submitRequest.screenKey = "1"; // OK
                                        screenReply = screenService.submit(screenOperationContext, submitRequest);

                                        screenName = screenReply.mapName;
                                        errMess = screenReply.message;

                                        if (errMess.Trim() != "" && !screenReply.functionKeys.Contains("XMIT-Confirm"))
                                        {
                                            throw new PXException(errMess.Trim());
                                        }

                                        if (screenReply.functionKeys.Contains("XMIT-Confirm"))
                                        {
                                            submitRequest.screenKey = "1"; // OK
                                            screenReply = screenService.submit(screenOperationContext, submitRequest);

                                            screenName = screenReply.mapName;
                                            errMess = screenReply.message;

                                            if (errMess.Trim() != "")
                                            {
                                                throw new PXException(errMess.Trim());
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new PXException(ex.Message);
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
                }
            }
            return adapter.Get();
        }
        #endregion
    }
}