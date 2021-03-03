using PLNSC;
using PX.Data;

namespace PX.Objects.IN
{
    public class NonStockItemMaint_Extension : PXGraphExtension<NonStockItemMaint>

    {
        #region Event Handlers


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