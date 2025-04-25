using Database;
using FinanceLib.FinancialCalculators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceLib.Instruments
{
    public enum InstrumentType
    {
        Option,
        Stock
    }

    public abstract class Instrument
    {
        public int InstrumentId;
        public string UnderlyingId;

        public abstract string GetDisplayName();

        public abstract InstrumentType GetInstrumentType();

        public abstract double CalcPrice(DateTime valuationDateTime, double vol);

        public abstract Dictionary<Greeks, double> CalcGreeks(DateTime valuationDateTime, double vol);

        public abstract int SaveToDb();

        public override string ToString()
        {
            return InstrumentId + " - " + GetDisplayName();
        }
    }
}
