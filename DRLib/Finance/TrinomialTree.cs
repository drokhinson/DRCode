using DRLib.Instrument;
using DRLib.MathUtils;

namespace DRLib.Finance;

public static class TrinomialTree
{
    public static double Price(Option opt, OptMarketData md, int numSteps = 500)
    {
        var (s, t, r, div, vol) = md;

        //Assemble inputs for price evaluation
        var dt = t / numSteps;
        var prob = CalcProbability(dt, r, div, vol);
        var discount = Math.Exp(-r * dt);


        // Build price matrix to for early excersize
        double[][] spotPriceMatrix = null;
        if(opt is AmericanCall or AmericanPut)
            spotPriceMatrix = BuildPriceMatrix(s, dt, vol, numSteps);


        var priceVector = BuildPriceVector(s, dt, vol, numSteps);
        for (int i = 0; i < priceVector.Length; i++)
            priceVector[i] = opt.CalcPayoff(priceVector[i]);

        //Iterate through each time step generating a new vector of option prices until convergance on initial price.
        for (int i = 0; i < numSteps; i++) {
            var prices = new double[priceVector.Length - 2];
            for (int j = 1; j < priceVector.Length - 1; j++) {
                var price = discount * (prob.Dwn * priceVector[j - 1] + prob.Flat * priceVector[j] +
                                        prob.Up * priceVector[j + 1]);
                
                if(opt is AmericanCall or AmericanPut)
                    prices[j - 1] = Math.Max(price, opt.CalcPayoff(spotPriceMatrix[numSteps - 1 - i][j - 1]));
                else
                    prices[j - 1] = price;
            }
            priceVector = prices;
        }

        return priceVector[0];
    }

    /// <summary> Uses Newton-Rapson method to calculated implied vol given option price </summary>
    public static double ImpliedVol(Option opt, double s, double t, double r, double div, double price, double shockSize = 0.001, int numSteps = 500)
    {
        if (opt is not EuroCall or EuroPut or AmericanCall or AmericanPut)
            throw new NotImplementedException();

        return RootFinding.NewtonRapson(
            v => Price(opt, new OptMarketData(s, t, r, div, v), numSteps),                                 // option price function
            v => Vega(opt, new OptMarketData(s, t, r, div, v), shockSize, numSteps),    // slope of option price function with respect to change in vol
            price);
    }

    private static double[] BuildPriceVector(double s, double dt, double vol, int n)
    {
        //Creates an array of prices possible after n steps in the evaluation
        var res = new double[n * 2 + 1];
        var dx = vol * Math.Sqrt(3 * dt);
        var up = Math.Exp(dx);
        var down = Math.Exp(-dx);

        for (int i = 0; i < n * 2 + 1; i++) {
            if (i < n) res[i] = 
                    s * Math.Pow(down, n - i);
            else res[i] = 
                    s * Math.Pow(up, i - n);
        }
        return res;
    }

    /// <summary>
    /// Builds jagged array containing the possible spot prices at each time step
    /// </summary>
    private static double[][] BuildPriceMatrix(double s, double dt, double vol, int n)
    {
        
        var res = new double[n + 1][];

        var dx = vol * Math.Sqrt(3 * dt);
        var up = Math.Exp(dx);
        var down = Math.Exp(-dx);

        for (int i = 0; i <= n; i++) {
            var size = i * 2 + 1;
            var stepPrice = new double[size];
            for (int j = 0; j < size; j++) {
                if (j < i) stepPrice[j] = s * Math.Pow(down, i - j);
                else stepPrice[j] = s * Math.Pow(up, j - i);
            }
            res[i] = stepPrice;
        }
        return res;
    }

    /// <summary> Calculates probabilities of up, down, and no movment of spot </summary>
    private static (double Up, double Flat, double Dwn) CalcProbability(double dt, double r, double div, double vol)
    {
        var p_res = new Dictionary<int, double>();
        var dx = vol * Math.Sqrt(3 * dt);
        var v = r - div - 0.5 * Math.Pow(vol, 2);
        var c = (Math.Pow(vol, 2) * dt + Math.Pow(v, 2) * Math.Pow(dt, 2)) / Math.Pow(dx, 2);

        var pUp = 0.5 * (c + v * dt / dx);
        var pFlat = 1 - c;
        var pDwn = 0.5 * (c - v * dt / dx);

        return (pUp, pFlat, pDwn);
    }


    public static double Delta(Option opt, OptMarketData md, double shockSize, int numSteps = 500) 
        => ShockGreeks.Delta(x => Price(opt, x, numSteps), md, shockSize);

    public static double Gamma(Option opt, OptMarketData md, double shockSize, int numSteps = 500) 
        => ShockGreeks.Gamma(x => Price(opt, x, numSteps), md, shockSize);

    public static double Theta(Option opt, OptMarketData md, int numSteps = 500) 
        => ShockGreeks.Theta(x => Price(opt, x, numSteps), md);

    public static double Vega(Option opt, OptMarketData md, double shockSize, int numSteps = 500)
        => ShockGreeks.Vega(x => Price(opt, x, numSteps), md, shockSize);

    public static double Rho(Option opt, OptMarketData md, double shockSize, int numSteps = 500) 
        => ShockGreeks.Rho(x => Price(opt, x, numSteps), md, shockSize);
}