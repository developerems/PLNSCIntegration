using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects;
using PX.SM;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using PLNSC;

namespace PX.Objects.AP
{
    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
  public class AP_APNotification_ExistingColumn : PXCacheExtension<PX.Objects.AP.APNotification>
  {
        #region ReportID  
        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = "Report")]
        [PXSelector(typeof(Search<SiteMap.screenID,
        Where<SiteMap.url, Like<Common.urlReports>, And<Where<SiteMap.screenID, Like<PXModule.ap_>, Or<SiteMap.screenID, Like<PXCustomModule.sc_>>>>>,
        //Where<SiteMap.screenID, Like<PXModule.ap_>, And<SiteMap.url, Like<Common.urlReports>>>,
        OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
        Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
        DescriptionField = typeof(SiteMap.title))]
        public string ReportID { get; set; }
        #endregion
  }
}