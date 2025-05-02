using DRLib.Instrument;
using DRLib.MathUtils;

namespace DRLib.Finance;

public static class MonteCarlo
{
    public static ScenarioSet GetScenarioSet(int numScenarios, int numSteps, OptMarketData md)
    {
        return new ScenarioSet(numScenarios, numSteps) {
            SetMd = md,
        }.Generate();
    }

    public static double Price(Option opt, OptMarketData md, int numScenarios, int numSteps, out ScenarioSet ss)
    {
        ss = GetScenarioSet(numScenarios, numSteps, md);
        return Price(opt, ss);
    }

    public static double Price(Option opt, ScenarioSet ss)
    {
        var payoff = opt switch {
            EuropeanOption or DigitalOption => ss.ColumnAvg(^1, opt.CalcPayoff),
            _ => throw new NotImplementedException()
        };

        var pv = payoff * Math.Exp(-ss.Rate * ss.T);
        return pv;
    }
}

public class ScenarioSet(int numScenarios, int numSteps) : DoubleMatrix(numScenarios, numSteps)
{
    public double Vol;
    public double Rate;
    public double Div;
    public double S0;
    public double T;

    public int NumScenarios => NumRow;
    public int NumSteps => NumCol;
    public double DeltaT => T / NumSteps;

    public ScenarioSet Generate(int? seed = null)
    {
        // Populate random numbers
        Rand.NormalDist.BoxMuller(Data, seed);

        CrossApply(x => {
            var (rnd, i) = x;
            var isFirstStep = i % NumSteps == 0;
            var s_tMinus1 = isFirstStep ? S0 : Data[i - 1];
            var drift = (Rate - Div - Vol * Vol / 2) * DeltaT;
            var nu = Vol * Math.Sqrt(DeltaT) * rnd;

            return s_tMinus1 * Math.Exp(drift + nu);
        });

        return this;
    }

    public OptMarketData SetMd
    {
        set {
            Vol = value.Vol;
            Rate = value.Rate;
            Div = value.Div;
            S0 = value.Spot;
            T = value.Time;
        }
    }
}