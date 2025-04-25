using FinanceLib.Instruments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceLib.FinancialCalculators
{
    public static class MonteCarloCalculator
    {
        public static List<double> CalcPriceList(Option option, List<double[]> scenarios, int numSteps)
        {
            //TODO have one option price calc method and have the option contain a delegate that gives the payoff function.
            var optionType = option.GetOptionType();
            switch (optionType) {
            case OptionType.European:
                return CalcEuroPrice((EuropeanOption)option, scenarios, numSteps);
            default:
                throw new Exception("Option Type not avaialbe");
            }
        }

        private static List<double> CalcEuroPrice(EuropeanOption o, List<double[]> scenarios, int numSteps)
        {
            var res = new ConcurrentBag<double>();
            Parallel.ForEach(scenarios, (scenario) => {
                var payoff = CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall);
                res.Add(payoff * Math.Exp(-o.R * o.T));
            });
            return res.ToList();
        }

        private static double CalcPayoff(double s, double k, bool isCall)
        {
            //Calculates payoff for call/put options at a given spot and strike
            double payoff = isCall ? s - k : k - s;
            return Math.Max(payoff, 0.0);
        }


        public static List<double[]> GenerateScenarioSet(Option o, int numSteps, bool antithetic, int numScenarios, out List<double[]> rndNums, int seed = 1111)
        {
            rndNums = new List<double[]>();
            var scenarios = new List<double[]>();

            for(int i = 0; i < numScenarios; i++) {
                var paths = GenerateScenario(o, numSteps, antithetic, seed, out var rndVector);
                rndNums.Add(rndVector);
                scenarios.AddRange(paths);
            }
            return scenarios;
        }

        public static List<double[]> GenerateScenarioSet(Option o, int numSteps, bool antithetic, List<double[]> rndNumList)
        {
            var scenarios = new List<double[]>();
            foreach (var rndVector in rndNumList)
                scenarios.AddRange(GenerateScenario(o, numSteps, antithetic, rndVector));
            return scenarios;
        }

        public static List<double[]> GenerateScenario(Option o, int numSteps, bool antithetic, int seed,  out double[] rndNumVector)
        {
            rndNumVector = GenerateRandNumVector(numSteps, seed);
            return GenerateScenario(o, numSteps, antithetic, rndNumVector);
        }

        public static List<double[]> GenerateScenario(Option o, int numSteps, bool antithetic, double[] rndNums)
        {
            var scenario = new double[numSteps];
            var scenario2 = new double[numSteps];

            var mu = o.R - o.Div;
            var dt = o.T / Convert.ToDouble(numSteps);
            var drift = Math.Exp((mu - Math.Pow(o.Vol,2) / 2.0) * dt);

            for (int j = 0; j < numSteps; j++) {
                scenario[j] = CalcStep(rndNums[j], drift, o.Vol, dt, j == 0 ? 1.0 : scenario[j - 1]);
                if (antithetic)
                    scenario2[j] = CalcStep(-rndNums[j], drift, o.Vol, dt, j == 0 ? 1.0 : scenario[j - 1]);
            }

            var res = new List<double[]> { scenario };
            if (antithetic)
                res.Add(scenario2);

            return res;
        }

        private static double CalcStep(double rand, double drift, double vol, double dt, double prev)
        {
            var chng = drift * Math.Exp(rand * vol * Math.Sqrt(dt));
            return prev * chng;
        }

        private static double[] GenerateRandNumVector(int numSteps, int seed)
        {
            return MathUtils.RandNum.RndmNumPolarRejection(numSteps, seed);
        }

        public static double[] GenerateCorrelatedRandVector(int seed, double[] rndNums, double correlation)
        {
            var res = new double[rndNums.Length];
            var nrmRand = MathUtils.RandNum.RndmNumPolarRejection(rndNums.Length, seed);

            var mult = Math.Sqrt(1 - Math.Pow(correlation, 2));
            for (int j = 0; j < rndNums.Length; j++)
            {
                res[j] = rndNums[j] * correlation + nrmRand[0] * mult;
                j++;
                if (j < rndNums.Length)
                    res[j] = rndNums[j] * correlation + nrmRand[1] * mult;
            }
            return res;
        }
    }

    public class CorrelatedDigiOption : Option
    {
        public double[] RandVector1;
        public double[] RandVector2;

        public double[] Scenarios1;
        public double[] Scenarios2;

        private readonly double Rate;
        private readonly double Time;


        public CorrelatedDigiOption(double t, double r, double d, double vol1, double vol2, double correlation, int numSteps, int seed)
        {
            Rate = r;
            Time = t;
            var stock1Data = new EuropeanOption(100, 100, t, r, d, vol1, true);
            Scenarios1 = MonteCarloCalculator.GenerateScenario(stock1Data, numSteps, false, seed, out RandVector1).First();
            RandVector2 = MonteCarloCalculator.GenerateCorrelatedRandVector(seed, RandVector1, correlation);
            var stock2Data = new EuropeanOption(100, 100, t, r, d, vol2, true);
            Scenarios2 = MonteCarloCalculator.GenerateScenario(stock2Data, numSteps, false, RandVector2).First();
        }

        public double CalcDigiPrice(double spot, double stock1Limit, double stock2Limit)
        {
            if (Scenarios1[Scenarios1.Length - 1] * spot > 22.5 && Scenarios2[Scenarios2.Length - 1] * spot < 17.50)
                return 1.0 * Math.Exp(-Rate * Time);
            return 0.0;
        }
    }
}
