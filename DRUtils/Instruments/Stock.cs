using FinanceLib.FinancialCalculators;

namespace FinanceLib.Instruments
{
    public static class StockUtils
    {

    }

    public sealed class Stock : Instrument
    {
        public string CompanyName;
        public string BloombergTicker;
        public string Description;

        public Stock(string underlyingId, string company, string bbgTicker, string description)
        {
            UnderlyingId = underlyingId;
            CompanyName = company;
            BloombergTicker = bbgTicker;
            Description = description;
        }

        public override InstrumentType GetInstrumentType()
        {
            return InstrumentType.Stock;
        }

        public override string GetDisplayName()
        {
            return InstrumentType.Stock + " " + BloombergTicker;
        }
        
        public override Dictionary<Greeks, double> CalcGreeks(DateTime valuationDateTime, double vol = -1.0)
        {
            var res = new Dictionary<Greeks, double>();
            var greeks = Enum.GetValues(typeof(Greeks)).Cast<Greeks>().ToList();
            foreach(var g in greeks)
                res.Add(g, 0.0);
            res[Greeks.Delta] = 1.0;
            return res;
        }

        public override int SaveToDb()
        {
            InstrumentId = InstrumentUtils.IsDuplicate(this);
            if (InstrumentId == -1) {
                using (var db = new InstrumentDB())
                {
                    if (db.Db_Instrument.Any())
                        InstrumentId = db.Db_Instrument.Max(r => r.InstrumentId) + 1;
                    else
                        InstrumentId = 1;

                    var instrumentInfo = GetInstrumentInfo();
                    var stockInfo = GetStockInfo();

                    db.Db_Instrument.Add(instrumentInfo);
                    db.Inst_Stock.Add(stockInfo);
                    db.SaveChanges();
                }
            }
            return InstrumentId;
        }

        private Inst_Stock GetStockInfo()
        {
            return new Inst_Stock
            {
                InstrumentId = InstrumentId,
                CompanyName = CompanyName,
                BloombergTicker = BloombergTicker,
                Description = Description,
            };
        }
    }
}
