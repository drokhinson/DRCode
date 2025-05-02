namespace DRLib.Instrument;

public enum OptionType
{
    European,
    American,
    Digital,
    Asian,
}

public static class OptionUtils
{
    public static double CalcPayoff(this Option opt, double spot)
    {
        var payoff = opt switch {
            EuroCall or AmericanCall => spot - opt.Strike,
            EuroPut or AmericanPut => opt.Strike - spot,
            DigiCall => spot > opt.Strike ? 1 : 0,
            DigiPut => spot < opt.Strike ? 1 : 0,
            _ => throw new NotImplementedException()
        };

        return payoff > 0 ? payoff : 0;
    }
}