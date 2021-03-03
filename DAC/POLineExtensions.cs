using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.PO
{
    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
    public class PO_POLine_ExistingColumn : PXCacheExtension<PX.Objects.PO.POLine>
    {
        #region InventoryID  
        [PXMergeAttributes(Method = MergeMethod.Append)]

        [PXCustomizeSelectorColumns(
        typeof(PX.Objects.IN.InventoryItem.inventoryCD),
        typeof(PX.Objects.IN.InventoryItem.descr),
        typeof(PX.Objects.IN.InventoryItem.itemClassID),
        typeof(PX.Objects.IN.InventoryItem.itemStatus),
        typeof(PX.Objects.IN.InventoryItem.itemType),
        typeof(PX.Objects.IN.InventoryItem.baseUnit),
        typeof(PX.Objects.IN.InventoryItem.salesUnit),
        typeof(PX.Objects.IN.InventoryItem.purchaseUnit),
        typeof(PX.Objects.IN.InventoryItem.basePrice),
        typeof(PX.Objects.IN.InventoryItemExt.usrAlternateIDs))]
        public int? InventoryID { get; set; }
        #endregion
  }
}