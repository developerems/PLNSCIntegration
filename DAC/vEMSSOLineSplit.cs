using System;
using PX.Data;

namespace PLNSC
{
  [Serializable]
  public class vEMSSOLineSplit : IBqlTable
  {
    #region Sotype
    [PXDBString(2, IsKey = true, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Sotype")]
    public virtual string Sotype { get; set; }
    public abstract class sotype : IBqlField { }
    #endregion

    #region Sonbr
    [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Sonbr")]
    public virtual string Sonbr { get; set; }
    public abstract class sonbr : IBqlField { }
    #endregion

    #region SOLineNbr
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "SOLine Nbr")]
    public virtual int? SOLineNbr { get; set; }
    public abstract class sOLineNbr : IBqlField { }
    #endregion

    #region Potype
    [PXDBString(2, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Potype")]
    public virtual string Potype { get; set; }
    public abstract class potype : IBqlField { }
    #endregion

    #region Ponbr
    [PXDBString(15, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Ponbr")]
    public virtual string Ponbr { get; set; }
    public abstract class ponbr : IBqlField { }
    #endregion

    #region POLineNbr
    [PXDBInt()]
    [PXUIField(DisplayName = "POLine Nbr")]
    public virtual int? POLineNbr { get; set; }
    public abstract class pOLineNbr : IBqlField { }
    #endregion
  }
}