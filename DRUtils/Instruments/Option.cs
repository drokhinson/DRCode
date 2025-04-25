using Database;
using FinanceLib.DataManager;
using FinanceLib.FinancialCalculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceLib.Instruments
{
    public enum OptionType
    {
        European,
        American,
        Asian,
        Digital,
        Barrier,
        Lookback,
        Range,
    }

    public abstract class Option : Instrument
    {
        public double Spot { get; set; } //gets set in code or from db
        public double K { get; set; }
        public double R { get; set; } //gets set in code or from db
        public DateTime ExpirationDate;
        public double T { get; set; } //calculate from ExpirationDate
        public double Div = 0.0; //gets set in code
        public double Vol { get; set; } //gets set in code
        public bool IsCall { get; set; }

        protected OptionType Type { get; set; }
    }

    public sealed class EuropeanOption : Option
    {
        public EuropeanOption(double s, double k, double t, double r, double d, double vol, bool isCall, 
            string undId = "SPX", DateTime expireDate = new DateTime())
        {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.European;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }



        public override double CalcPrice(DateTime valuationDateTime, double vol)
        {
            Vol = vol;
            GetMarketData(valuationDateTime);
            return BlackScholes.CalcPrice(this);
        }

        public override Dictionary<Greeks, double> CalcGreeks(DateTime valuationDateTime, double vol)
        {
            GetMarketData(valuationDateTime);
            Vol = vol;
            return BlackScholes.AssembleGreeks(this);
        }
    }

    public sealed class AmericanOption : Option
    {
        public AmericanOption(double s, double k, double t, double r, double d, double vol, bool isCall,
            string undId = "SPX", DateTime expireDate = new DateTime())
        {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.American;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }

        public AmericanOption(Db_Instrument instrumentInfo, Inst_Option optionInfo)
        {
            InstrumentId = instrumentInfo.InstrumentId;
            UnderlyingId = instrumentInfo.UnderlyingId;

            Type = OptionType.American;
            K = optionInfo.Strike;
            ExpirationDate = optionInfo.ExpirationDate;
            IsCall = optionInfo.IsCall;
        }

        public override double CalcPrice(DateTime valuationDateTime, double vol)
        {
            Vol = vol;
            GetMarketData(valuationDateTime);
            return TrinomialTree.CalcPrice(this, NumSteps);
        }

        public override Dictionary<Greeks, double> CalcGreeks(DateTime valuationDateTime, double vol)
        {
            GetMarketData(valuationDateTime);
            Vol = vol;
            return TrinomialTree.AssembleGreeks(this, NumSteps);
        }
    }

    public sealed class AsianOption : Option
    {
        public int StartAverage { get; set; }
        public int EndAverage { get; set; }

        public AsianOption(double s, double k, double t, double r, double d, double vol, bool isCall,
            int start, int end, string undId = "SPX", DateTime expireDate = new DateTime())
        {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.Asian;
            StartAverage = start;
            EndAverage = end;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }

        public AsianOption(Db_Instrument instrumentInfo, Inst_Option optionInfo)
        {
            InstrumentId = instrumentInfo.InstrumentId;
            UnderlyingId = instrumentInfo.UnderlyingId;

            Type = OptionType.Asian;
            K = optionInfo.Strike;
            ExpirationDate = optionInfo.ExpirationDate;
            IsCall = optionInfo.IsCall;
        }
    }

    public sealed class DigitalOption : Option
    {
        public double Rebate { get; set; }
        //CV for this function is the gamma of a european

        public DigitalOption(double s, double k, double t, double r, double d, double vol, bool isCall,
            double rebate, string undId = "SPX", DateTime expireDate = new DateTime())
        {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.Digital;
            Rebate = rebate;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }

        public DigitalOption(Db_Instrument instrumentInfo, Inst_Option optionInfo, double rebate)
        {
            InstrumentId = instrumentInfo.InstrumentId;
            UnderlyingId = instrumentInfo.UnderlyingId;

            Type = OptionType.Digital;
            K = optionInfo.Strike;
            ExpirationDate = optionInfo.ExpirationDate;
            IsCall = optionInfo.IsCall;

            Rebate = rebate;
        }

        public override double CalcPrice(DateTime valuationDateTime, double vol)
        {
            Vol = vol;
            GetMarketData(valuationDateTime);
            return BlackScholes.CalcPrice(this);
        }

        public override int SaveToDb()
        {
            InstrumentId = InstrumentUtils.IsDuplicate(this);
            if (InstrumentId == -1) {
                SaveGenericInfo();
                SaveDigitalInfo();
            }
            return InstrumentId;
        }

        private void SaveDigitalInfo()
        {
            using (var db = new InstrumentDB())
            {
                var digitalInfo = new Inst_Option_Digital
                {
                    InstrumentId = InstrumentId,
                    Rebate = Rebate,
                };
                db.Inst_Option_Digital.Add(digitalInfo);
                db.SaveChanges();
            }
        }
    }

    public sealed class BarrierOption : Option
    {
        public bool IsUp { get; set; }
        public bool IsIn { get; set; }
        public double Barrier { get; set; } //multiple barrier options????
        //If you set a barrier to a very high value the result will approach price of euro

        public BarrierOption(double s, double k, double t, double r, double d, double vol, bool isCall,
            double barrier, bool isUp, bool isIn, string undId = "SPX", DateTime expireDate = new DateTime())
            {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.Barrier;
            Barrier = barrier;
            IsUp = isUp;
            IsIn = isIn;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }

        public BarrierOption(Db_Instrument instrumentInfo, Inst_Option optionInfo, Inst_Option_Barrier barrierInfo)
        {
            InstrumentId = instrumentInfo.InstrumentId;
            UnderlyingId = instrumentInfo.UnderlyingId;

            Type = OptionType.Barrier;
            K = optionInfo.Strike;
            ExpirationDate = optionInfo.ExpirationDate;
            IsCall = optionInfo.IsCall;

            Barrier = barrierInfo.Barrier;
            IsUp = barrierInfo.IsUp;
            IsIn = barrierInfo.IsIn;
        }

        public override int SaveToDb()
        {
            InstrumentId = InstrumentUtils.IsDuplicate(this);
            if (InstrumentId == -1) {
                SaveGenericInfo();
                SaveBarrierInfo();
            }
            return InstrumentId;
        }

        private void SaveBarrierInfo()
        {
            using (var db = new InstrumentDB())
            {
                var barrierInfo = new Inst_Option_Barrier
                {
                    InstrumentId = InstrumentId,
                    Barrier = Barrier,
                    IsIn = IsIn, 
                    IsUp = IsUp
                };
                db.Inst_Option_Barrier.Add(barrierInfo);
                db.SaveChanges();
            }
        }
    }

    public sealed class LookbackOption : Option
    {
        public bool IsFixed { get; set; } //is case of implementing floating lookback

        public LookbackOption(double s, double k, double t, double r, double d, double vol, bool isCall,
            string undId = "SPX", DateTime expireDate = new DateTime())
        {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.Lookback;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }

        public LookbackOption(Db_Instrument instrumentInfo, Inst_Option optionInfo, bool isFixed)
        {
            InstrumentId = instrumentInfo.InstrumentId;
            UnderlyingId = instrumentInfo.UnderlyingId;

            Type = OptionType.Lookback;
            K = optionInfo.Strike;
            ExpirationDate = optionInfo.ExpirationDate;
            IsCall = optionInfo.IsCall;

            IsFixed = isFixed;
        }

        public override int SaveToDb()
        {
            InstrumentId = InstrumentUtils.IsDuplicate(this);
            if (InstrumentId == -1) {
                SaveGenericInfo();
                SaveLookbackInfo();
            }
            return InstrumentId;
        }

        private void SaveLookbackInfo()
        {
            using (var db = new InstrumentDB())
            {
                var lookbackInfo = new Inst_Option_Lookback
                {
                    InstrumentId = InstrumentId,
                    IsFixed = IsFixed
                };
                db.Inst_Option_Lookback.Add(lookbackInfo);
                db.SaveChanges();
            }
        }
    }

    public sealed class RangeOption : Option
    {
        public RangeOption(double s, double k, double t, double r, double d, double vol, bool isCall,
            string undId = "SPX", DateTime expireDate = new DateTime())
        {
            Spot = s;
            K = k;
            T = t;
            R = r;
            Div = d;
            Vol = vol;
            IsCall = isCall;
            Type = OptionType.Range;

            UnderlyingId = undId;
            ExpirationDate = expireDate;
        }

        public RangeOption(Db_Instrument instrumentInfo, Inst_Option optionInfo)
        {
            InstrumentId = instrumentInfo.InstrumentId;
            UnderlyingId = instrumentInfo.UnderlyingId;

            Type = OptionType.Range;
            K = optionInfo.Strike;
            ExpirationDate = optionInfo.ExpirationDate;
            IsCall = optionInfo.IsCall;
        }
    }
}
