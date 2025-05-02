using DRLib.Instrument;
using DRLib.MathUtils;

namespace DRLib.Finance;

public static class BlackScholes
{
    public static double Price(Option opt, OptMarketData md)
    {
        return opt switch {
            EuropeanOption => Euro(opt.Strike, md, opt.IsCall),
            DigitalOption => Digital(opt.Strike, md, opt.IsCall),
            _ => throw new NotImplementedException()
        };
    }

    public static double Euro(double k, OptMarketData md, bool isCall)
    {
        var (s, t, r, div, vol) = md;

        var d1 = D1(s, k, t, r, div, vol);
        var d2 = D2(d1, t, vol);

        if (isCall)
            return s * Math.Exp(-div * t) * Probability.NormCDF(d1) -
                    k * Math.Exp(-r * t) * Probability.NormCDF(d2);

        return -s * Math.Exp(-div * t) * Probability.NormCDF(-d1) +
                k * Math.Exp(-r * t) * Probability.NormCDF(-d2);
    }

    /// <summary> Option with fixed payoff at pre-determined strike price. </summary>
    public static double Digital(double k, OptMarketData md, bool isCall)
    {
        var (s, t, r, div, vol) = md;

        var d1 = D1(s, k, t, r, div, vol);
        var d2 = D2(d1, t, vol);

        if (isCall)
            return Math.Exp(-r * t) * Probability.NormCDF(d2);

        return Math.Exp(-r * t) * Probability.NormCDF(-d2);
    }

    /// <summary> Uses Newton-Rapson method to calculated implied vol given option price </summary>
    public static double ImpliedVol(Option opt, double s, double t, double r, double div, double price)
    {
        return RootFinding.NewtonRapson(
            v => Price(opt, new OptMarketData(s, t, r, div, v)),  // option price function
            v => Vega(opt, new OptMarketData(s, t, r, div, v)),          // slope of option price function with respect to change in vol
            price);
    }

    public static double Delta(Option opt, OptMarketData md)
    {
        var (s, t, r, div, vol) = md;
        var k = opt.Strike;

        if (opt is not EuropeanOption)
            return ShockGreeks.Delta(x => Price(opt, x), md);

        var d1 = D1(s, k, t, r, div, vol);
        return opt.IsCall ?
            Math.Exp(-div * t) * Probability.NormCDF(d1) :
            Math.Exp(-div * t) * (Probability.NormCDF(d1) - 1);
    }

    public static double Gamma(Option opt, OptMarketData md)
    {
        var (s, t, r, div, vol) = md;
        var k = opt.Strike;

        if (opt is not EuropeanOption)
            return ShockGreeks.Gamma(x => Price(opt, x), md);

        double d1 = D1(s, k, t, r, div, vol);
        return Probability.NormPDF(d1) * Math.Exp(-div * t) / (s * vol * Math.Sqrt(t));
    }

    public static double Theta(Option opt, OptMarketData md)
    {
        var (s, t, r, div, vol) = md;
        var k = opt.Strike;

        if (opt is not EuropeanOption)
            return ShockGreeks.Theta(x => Price(opt, x), md);

        var d1 = D1(s, k, t, r, div, vol);
        var d2 = D2(d1, t, vol);

        var top = -s * Probability.NormPDF(d1) * vol * Math.Exp(-div * t);

        return opt.IsCall
            ? top / (2 * Math.Sqrt(t)) + div * s * Probability.NormCDF(d1)
                * Math.Exp(-div * t) - r * k * Math.Exp(-r * t) * Probability.NormCDF(d2)
            : top / (2 * Math.Sqrt(t)) - div * s * Probability.NormCDF(-d1)
                * Math.Exp(-div * t) + r * k * Math.Exp(-r * t) * Probability.NormCDF(-d2);
    }

    public static double Vega(Option opt, OptMarketData md)
    {
        var (s, t, r, div, vol) = md;

        if (opt is not EuropeanOption)
            return ShockGreeks.Vega(x => Price(opt, x), md, 0.01);

        double d1 = D1(s, opt.Strike, t, r, div, vol);
        return s * Math.Sqrt(t) * Probability.NormPDF(d1) * Math.Exp(-div * t);
    }

    public static double Rho(Option opt, OptMarketData md)
    {
        var (s, t, r, div, vol) = md;
        var k = opt.Strike;

        if (opt is not EuropeanOption)
            return ShockGreeks.Rho(x => Price(opt, x), md);

        var d1 = D1(s, k, t, r, div, vol);
        var d2 = D2(d1, t, vol);
        return opt.IsCall ?
            k * t * Math.Exp(-r * t) * Probability.NormCDF(d2) :
            -k * t * Math.Exp(-r * t) * Probability.NormCDF(-d2);
    }

    private static double D1(double s, double k, double t, double r, double div, double vol)
    {
        return (Math.Log(s / k) + (r - div + vol * vol / 2) * t) / (vol * Math.Sqrt(t));
    }

    private static double D2(double d1, double t, double vol)
    {
        return d1 - vol * Math.Sqrt(t);
    }
}