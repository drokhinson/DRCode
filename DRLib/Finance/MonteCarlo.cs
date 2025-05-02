using DRLib.MathUtils;

namespace DRLib.Finance;

public static class MonteCarlo
{
    public class ScenarioSet(int numScenarios, int numSteps) : Matrix(numScenarios, numSteps + 1)
    {
        public double Vol;
        public double Rate;
        public double Div;
        public double S0;
        public double T;

        public int NumScenarios => NumRow;
        public int NumSteps => NumCol - 1;
        public double DeltaT => T / NumSteps;

        public void Generate(int? seed = null)
        {
            // Populate random numbers
            Rand.NormalDist.BoxMuller(Data.Length, seed, Data);
            ColumnSet(0, () => S0); // set 0  index to starting index value

            CrossApply(x => {
                var (rnd, i) = x;
                var S_tMinus1 = Data[i - 1];
                var drift = (Rate - Div - Math.Pow(Vol, 2) / 2) * DeltaT;
                var stoch = Vol * Math.Sqrt(DeltaT) * rnd;

                return S_tMinus1 * Math.Exp(drift + stoch);
            }, 1);
        }

        public OptMarketData SetMd
        {
            set {
                Vol = value.Vol;
                Rate = value.Rate;
                S0 = value.Spot;
                T = value.Time;
            }
        }
    }
}