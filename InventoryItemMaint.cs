using PLNSC;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
    public class InventoryItemMaint_Extension : PXGraphExtension<InventoryItemMaint>
    {
        #region Event Handlers

        [PXOverride]
        public void Persist(System.Action del)
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                InventoryItem item = Base.Item.Current;
                if (item != null && Base.itemxrefrecords.Cache.IsDirty)
                {
                    string alternateIDs = string.Empty;
                    foreach (INItemXRef crossRef in Base.itemxrefrecords.Select())
                    {
                        alternateIDs = string.IsNullOrEmpty(alternateIDs) ?
                            crossRef.Descr : alternateIDs + "; " + crossRef.Descr;
                    }
                    item.GetExtension<InventoryItemExt>().UsrAlternateIDs = alternateIDs;
                    Base.Item.Update(item);
                }

                del();
                ts.Complete();
            }
        }


        public PXAction<PX.Objects.IN.InventoryItem> RecalcAlternateIDs;

        // Acuminator disable once PX1013 PXActionHandlerInvalidReturnType [Justification]
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Generate Part No")]
        protected void recalcAlternateIDs()
        {
            PXLongOperation.StartOperation(Base, () =>
            {
                InventoryItemMaint itemMaint = PXGraph.CreateInstance<InventoryItemMaint>();
                var items = PXSelect<InventoryItem, Where<InventoryItem.stkItem, Equal<boolTrue>>>.Select(itemMaint);
                foreach (InventoryItem item in items)
                {
                    itemMaint.Clear();
                    itemMaint.Item.Current = item;
                    itemMaint.itemxrefrecords.Cache.IsDirty = true;
                    itemMaint.Actions.PressSave();
                }
            });
        }

        protected void InventoryItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {

            var row = (InventoryItem)e.Row;
            var strBaseUnit = row.BaseUnit.ToString().Trim();
            var strSalesUnit = row.SalesUnit.ToString().Trim();
            var strPurchaseUnit = row.PurchaseUnit.ToString().Trim();
            int valueBaseUnit = strBaseUnit.Length;
            int valueSalesUnit = strSalesUnit.Length;
            int valuePurchaseUnit = strPurchaseUnit.Length;

            if (valueBaseUnit > 4)
            {
                sender.RaiseExceptionHandling<InventoryItem.baseUnit>(e.Row, row.BaseUnit, new PXSetPropertyException(CustomMessage.BUMoreThanFourChar));
            }

            if (valueSalesUnit > 4)
            {
                sender.RaiseExceptionHandling<InventoryItem.salesUnit>(e.Row, row.SalesUnit, new PXSetPropertyException(CustomMessage.SUMoreThanFourChar));
            }

            if (valuePurchaseUnit > 4)
            {
                sender.RaiseExceptionHandling<InventoryItem.purchaseUnit>(e.Row, row.PurchaseUnit, new PXSetPropertyException(CustomMessage.PUMoreThanFourChar));
            }

        }

        #endregion
    }

}