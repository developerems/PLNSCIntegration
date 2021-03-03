using PLNSC;
using PX.Data;

namespace PX.Objects.IN
{
    public class INUnitMaint_Extension : PXGraphExtension<INUnitMaint>
    {
        #region Event Handlers

        protected void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)

        {

            var row = (INUnit)e.Row;
            var strFromUnit = row.FromUnit.ToString().Trim();
            var strToUnit = row.ToUnit.ToString().Trim();
            int valueFromUnit = strFromUnit.Length;
            int valueToUnit = strToUnit.Length;

            if (valueFromUnit > 4)
            {
                sender.RaiseExceptionHandling<INUnit.fromUnit>(e.Row, row.FromUnit, new PXSetPropertyException(CustomMessage.MoreThanFourChar));
            }

            if (valueToUnit > 4)
            {
                sender.RaiseExceptionHandling<INUnit.toUnit>(e.Row, row.ToUnit, new PXSetPropertyException(CustomMessage.MoreThanFourChar));
            }
        }

        #endregion
    }
}