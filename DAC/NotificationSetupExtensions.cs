using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.RQ;
using PX.Objects;
using PX.SM;
using System.Collections.Generic;
using System;
using PLNSC;

namespace PX.Objects.RQ
{
    // Acuminator disable once PX1026 UnderscoresInDacDeclaration [Justification]
    [PXNonInstantiatedExtension]
  public class RQ_RQNotification_ExistingColumn : PXCacheExtension<PX.Objects.RQ.RQNotification>
  {
        #region ReportID  
        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = "Report")]
        [PXSelector(typeof(Search<SiteMap.screenID,
        Where<SiteMap.url, Like<Common.urlReports>, And<Where<SiteMap.screenID, Like<PXModule.rq_>, Or<SiteMap.screenID, Like<PXCustomModule.sc_>>>>>,
        //Where<SiteMap.screenID, Like<PXModule.rq_>, And<SiteMap.url, Like<Common.urlReports>>>,
        OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
        Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
        DescriptionField = typeof(SiteMap.title))]
        public string ReportID { get; set; }
        #endregion
  }
}