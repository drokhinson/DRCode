namespace DRLib.Finance;

public static class ShockGreeks
{
    private const double GreekShock = 0.001; //size of shock for greeks calculation

    public static double Delta(Func<OptMarketData, double> priceCalc, OptMarketData baseMarketData, double shockSize = GreekShock)
    {
        var sUp = baseMarketData.Spot * (1 + shockSize);
        var sDn = baseMarketData.Spot * (1 - shockSize);

        var pUp = priceCalc(baseMarketData with { Spot = sUp });
        var pDn = priceCalc(baseMarketData with { Spot = sDn });

        return (pUp - pDn) / (sUp - sDn);
    }

    public static double Gamma(Func<OptMarketData, double> priceCalc, OptMarketData baseMarketData, double shockSize = GreekShock)
    {
        var sBase = baseMarketData.Spot;
        var sUp = baseMarketData.Spot * (1 + shockSize);
        var sDn = baseMarketData.Spot * (1 - shockSize);

        var pBase = priceCalc(baseMarketData);
        var pUp = priceCalc(baseMarketData with { Spot = sUp });
        var pDn = priceCalc(baseMarketData with { Spot = sDn });

        return ((pUp - pBase) / (sUp - sBase) - (pBase - pDn) / (sBase - sDn)) / (0.5 * (sUp - sDn));
    }

    /// <summary> Change in option value one day closer to expriation </summary>
    public static double Theta(Func<OptMarketData, double> priceCalc, OptMarketData baseMarketData)
    {
        var tMinus1 = baseMarketData.Time - 1.0 / 252.0;

        var pBase = priceCalc(baseMarketData);
        var pTMinus1 = priceCalc(baseMarketData with { Time = tMinus1 });

        return (pBase - pTMinus1) / (baseMarketData.Time - tMinus1);
    }

    public static double Vega(Func<OptMarketData, double> priceCalc, OptMarketData baseMarketData, double shockSize = GreekShock)
    {
        var sUp = baseMarketData.Vol * (1 + shockSize);
        var sDn = baseMarketData.Vol * (1 - shockSize);

        var pUp = priceCalc(baseMarketData with { Vol = sUp });
        var pDn = priceCalc(baseMarketData with { Vol = sDn });

        return (pUp - pDn) / (sUp - sDn);
    }

    public static double Rho(Func<OptMarketData, double> priceCalc, OptMarketData baseMarketData, double shockSize = GreekShock)
    {
        var sUp = baseMarketData.Rate * (1 + shockSize);
        var sDn = baseMarketData.Rate * (1 - shockSize);

        var pUp = priceCalc(baseMarketData with { Rate = sUp });
        var pDn = priceCalc(baseMarketData with { Rate = sDn });

        return (pUp - pDn) / (sUp - sDn);
    }
}

public record OptMarketData(double Spot, double Time, double Rate, double Div, double Vol);