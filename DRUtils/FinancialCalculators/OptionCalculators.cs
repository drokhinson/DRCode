using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FinanceLib.Instruments;

namespace FinanceLib.FinancialCalculators
{
    public enum PricingModel
    {
        BlackScholes,
        TrinomialTree,
        MonteCarlo
    }

    public enum Greeks
    {
        Delta,
        Gamma,
        Rho,
        Vega,
        Theta
    }

    public class VarianceReduction
    {
        public bool Antithetic { get; set; }
        public bool ControlVariate { get; set; }

        public VarianceReduction(bool anti, bool cv)
        {
            Antithetic = anti;
            ControlVariate = cv;
        }
    }

    public static class BlackScholes
    {
        //Black-Scholes Greeks Calculator
        public static double CalcPrice(Option option)
        {
            
            var d2 = CalcD2(option.Spot, option.K, option.T, option.R, option.Div, option.Vol);

            if (option.GetOptionType() == OptionType.Digital)
                return CalcDigital((DigitalOption)option, d2);

            return CalcEuro((EuropeanOption)option, d2);

        }

        private static double CalcEuro(EuropeanOption o, double d2)
        {
            var d1 = CalcD1(o.Spot, o.K, o.T, o.R, o.Div, o.Vol);

            if (o.IsCall)
                return o.Spot * Math.Exp(-o.Div * o.T) * GetNormCDF(d1) -
                       o.K * Math.Exp(-o.R * o.T) * GetNormCDF(d2);

            return -o.Spot * Math.Exp(-o.Div * o.T) * GetNormCDF(-d1) +
                  o.K * Math.Exp(-o.R * o.T) * GetNormCDF(-d2);
        }

        private static double CalcDigital(DigitalOption option, double d2)
        {
            if (option.IsCall)
                return option.Rebate * Math.Exp(-option.R * option.T) * GetNormCDF(d2);

            return option.Rebate * Math.Exp(-option.R * option.T) * GetNormCDF(-d2);
        }

        public static double CalcVol(Option option, double price)
        {
            var s = option.Spot;
            var k = option.K;
            var t = option.T;
            var r = option.R;
            var d = option.Div;
            var isCall = option.IsCall;

            //Estimate option volatility using Newton method
            double err = 0.0001;
            double vol = 2.0;
            double priceE = 0.0;
            int i = 0;

            while (Math.Abs(price - priceE) > err && i < 1000)
            {
                var vegaE = CalcVega(s, k, t, r, d, vol);
                priceE = CalcPrice(new EuropeanOption(s, k, t, r, d, vol, isCall));
                var dx = (price - priceE) / vegaE;
                vol += dx;
                i++;
            }
            return vol;
        }

        public static Dictionary<Greeks, double> AssembleGreeks(Option option)
        {
            var s = option.Spot;
            var k = option.K;
            var t = option.T;
            var r = option.R;
            var d = option.Div;
            var vol = option.Vol;
            var isCall = option.IsCall;

            var resDic = new Dictionary<Greeks, double>();
            resDic.Add(Greeks.Delta, CalcDelta(s, k, t, r, d, vol, isCall));
            resDic.Add(Greeks.Gamma, CalcGamma(s, k, t, r, d, vol));
            resDic.Add(Greeks.Theta, CalcTheta(s, k, t, r, d, vol, isCall));
            resDic.Add(Greeks.Rho, CalcRho(s, k, t, r, d, vol, isCall));
            resDic.Add(Greeks.Vega, CalcVega(s, k, t, r, d, vol));
            return resDic;
        }

        #region Private Classes
        public static double CalcD1(double s, double k, double t, double r, double div, double vol)
        {
            return (Math.Log(s / k) + (r - div + Math.Pow(vol, 2) / 2) * t) / (vol * Math.Sqrt(t));
        }

        public static double CalcD2(double s, double k, double t, double r, double div, double vol)
        {
            return CalcD1(s, k, t, r, div, vol) - vol * Math.Sqrt(t);
        }

        public static double GetNormCDF(double val)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (val < 0)
                sign = -1;
            val = Math.Abs(val) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * val);
            double y = 1.0 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.Exp(-val * val);

            return 0.5 * (1.0 + sign * y);
        }

        public static double GetNormPDF(double val)
        {
            return Math.Exp(-Math.Pow(val, 2) / 2) / Math.Sqrt(2 * Math.PI);
        }
        #endregion

        #region Greeks

        public static double CalcDelta(Option option)
        {
            return CalcDelta(option.Spot, option.K, option.T, option.R, option.Div, option.Vol, option.IsCall);
        }
        public static double CalcDelta(double s, double k, double t, double r, double div, double vol, bool isCall)
        {
            var d1 = CalcD1(s, k, t, r, div, vol);
            return isCall ? Math.Exp(-div * t) * GetNormCDF(d1) : Math.Exp(-div * t) * (GetNormCDF(d1) - 1);
        }

        public static double CalcGamma(double s, double k, double t, double r, double div, double vol)
        {
            double d1 = CalcD1(s, k, t, r, div, vol);
            return GetNormPDF(d1) * Math.Exp(-div * t) / (s * vol * Math.Sqrt(t));
        }

        public static double CalcTheta(double s, double k, double t, double r, double div, double vol, bool isCall)
        {
            var d1 = CalcD1(s, k, t, r, div, vol);
            var d2 = CalcD2(s, k, t, r, div, vol);

            return isCall
                ? -s * GetNormPDF(d1) * vol * Math.Exp(-div * t) / (2 * Math.Sqrt(t)) + div * s * GetNormCDF(d1)
                  * Math.Exp(-div * t) - r * k * Math.Exp(-r * t) * GetNormCDF(d2)
                : -s * GetNormPDF(d1) * vol * Math.Exp(-div * t) / (2 * Math.Sqrt(t)) - div * s * GetNormCDF(-d1)
                  * Math.Exp(-div * t) + r * k * Math.Exp(-r * t) * GetNormCDF(-d2);
        }

        public static double CalcVega(double s, double k, double t, double r, double div, double vol)
        {
            double d1 = CalcD1(s, k, t, r, div, vol);
            return s * Math.Sqrt(t) * GetNormPDF(d1) * Math.Exp(-div * t);
        }

        public static double CalcRho(double s, double k, double t, double r, double div, double vol, bool isCall)
        {
            var d2 = CalcD2(s, k, t, r, div, vol);
            return isCall ? k * t * Math.Exp(-r * t) * GetNormCDF(d2) : -k * t * Math.Exp(-r * t) * GetNormCDF(-d2);
        }
        #endregion
    }

    public static class TrinomialTree
    {
        //Trinomial tree calculation of option price
        private const double GreekShock = 0.001; //size of shock for greeks calculation

        public static double CalcPrice(Option option, int numSteps)
        {
            var s = option.Spot;
            var k = option.K;
            var t = option.T;
            var r = option.R;
            var div = option.Div;
            var vol = option.Vol;
            var isCall = option.IsCall;
            var isEuro = option.GetOptionType() == OptionType.European;
            //Selects European or American option and calls function to calculate price of an option using a 
            // trinomial tree.
            return isEuro
                ? CalcPriceEuropean(s, k, t, r, div, vol, isCall, numSteps)
                : CalcPriceAmerican(s, k, t, r, div, vol, isCall, numSteps);
        }

        public static double CalcVol(Option option, double price, int numSteps = 252)
        {
            var s = option.Spot;
            var k = option.K;
            var t = option.T;
            var r = option.R;
            var d = option.Div;
            var isCall = option.IsCall;
            var isEuro = option.GetOptionType() == OptionType.European;
            
            //Estimate option volatility using Newton method
            double err = 0.0001;
            double vol = 2.0;
            double priceE = 0.0;
            int i = 0;

            while (Math.Abs(price - priceE) > err && i < 1000)
            {
                var vegaE = CalcVega(s, k, t, r, d, vol, isCall, isEuro);
                priceE = isEuro ? 
                    CalcPrice(new EuropeanOption(s, k, t, r, d, vol, isCall), numSteps) :
                    CalcPrice(new AmericanOption(s, k, t, r, d, vol, isCall), numSteps);
                var dx = (price - priceE) / vegaE;
                vol += dx;
                i++;
            }
            return vol;
        }

        public static Dictionary<Greeks, double> AssembleGreeks(Option option, int numSteps)
        {
            var s = option.Spot;
            var k = option.K;
            var t = option.T;
            var r = option.R;
            var d = option.Div;
            var vol = option.Vol;
            var isCall = option.IsCall;
            var isEuro = option.GetOptionType() == OptionType.European;

            var resDic = new Dictionary<Greeks, double>();
            resDic.Add(Greeks.Delta, CalcDelta(s, k, t, r, d, vol, isCall, isEuro, numSteps));
            resDic.Add(Greeks.Gamma, CalcGamma(s, k, t, r, d, vol, isCall, isEuro, numSteps));
            resDic.Add(Greeks.Theta, CalcTheta(s, k, t, r, d, vol, isCall, isEuro, numSteps));
            resDic.Add(Greeks.Rho, CalcRho(s, k, t, r, d, vol, isCall, isEuro, numSteps));
            resDic.Add(Greeks.Vega, CalcVega(s, k, t, r, d, vol, isCall, isEuro, numSteps));
            return resDic;
        }

        #region Private Classes
        private static double CalcPriceEuropean(double s, double k, double t, double r, double div,
            double vol, bool isCall, int numSteps)
        {
            //Assemble inputs for price evaluation
            var dt = t / numSteps;
            var prob = CalcProbability(dt, r, div, vol);
            var discount = Math.Exp(-r * dt);
            var priceVector = BuildPriceVector(s, dt, vol, numSteps);

            //Iterate through each time step generating a new vector of option prices until convergance on initial price.
            for (int i = 0; i < priceVector.Length; i++) priceVector[i] = CalcPayoff(priceVector[i], k, isCall);
            for (int i = 0; i < numSteps; i++) {
                var prices = new double[priceVector.Length - 2];
                for (int j = 1; j < priceVector.Length - 1; j++)
                    prices[j - 1] = discount * (prob[-1] * priceVector[j - 1] + prob[0] * priceVector[j] + prob[1] * priceVector[j + 1]);
                priceVector = prices;
            }

            return priceVector[0];
        }

        private static double CalcPriceAmerican(double s, double k, double t, double r, double div,
            double vol, bool isCall, int numSteps)
        {
            //Assemble inputs for price evaluation
            var dt = t / numSteps;
            var prob = CalcProbability(dt, r, div, vol);
            var discount = Math.Exp(-r * dt);
            var priceMatrix =
                BuildPriceMatrix(s, dt, vol, numSteps); //Matrix of spots used to check for early excersise

            var priceVector = BuildPriceVector(s, dt, vol, numSteps);
            for (int i = 0; i < priceVector.Length; i++) priceVector[i] = CalcPayoff(priceVector[i], k, isCall);

            //Iterate through each time step generating a new vector of option prices until convergance on initial price.
            for (int i = 0; i < numSteps; i++) {
                var prices = new double[priceVector.Length - 2];
                for (int j = 1; j < priceVector.Length - 1; j++) {
                    var price = discount * (prob[-1] * priceVector[j - 1] + prob[0] * priceVector[j] +
                                            prob[1] * priceVector[j + 1]);
                    prices[j - 1] = Math.Max(price, CalcPayoff(priceMatrix[numSteps - 1 - i][j - 1], k, isCall));
                }
                priceVector = prices;
            }

            return priceVector[0];
        }

        private static double CalcPriceAmericanCV(double s, double k, double t, double r, double div, 
            double vol, bool isCall, int numSteps)
        {
            //Assemble inputs for price evaluation
            var dt = t / numSteps;
            var prob = CalcProbability(dt, r, div, vol);
            var discount = Math.Exp(-r * dt);
            var priceMatrix =
                BuildPriceMatrix(s, dt, vol, numSteps); //Matrix of spots used to check for early excersise

            var priceVector = BuildPriceVector(s, dt, vol, numSteps);
            var euroPriceVector = BuildPriceVector(s, dt, vol, numSteps);
            for (int i = 0; i < priceVector.Length; i++)
                priceVector[i] = CalcPayoff(priceVector[i], k, isCall);

            //Iterate through each time step generating a new vector of option prices until convergance on initial price.
            for (int i = 0; i < numSteps; i++) {
                var prices = new double[priceVector.Length - 2];
                var euroPrices = new double[priceVector.Length - 2];
                for (int j = 1; j < priceVector.Length - 1; j++) {
                    var euroPrice = discount * (prob[-1] * priceVector[j - 1] + prob[0] * priceVector[j] +
                                            prob[1] * priceVector[j + 1]);
                    euroPriceVector[j - 1] = euroPrice;
                    prices[j - 1] = Math.Max(euroPrice, CalcPayoff(priceMatrix[numSteps - 1 - i][j - 1], k, isCall));
                }
                priceVector = prices;
            }

            return priceVector[0];
        }

        private static double[] BuildPriceVector(double s, double dt, double vol, int n)
        {
            //Creates an array of prices possible after n steps in the evaluation
            var res = new double[n * 2 + 1];
            var dx = vol * Math.Sqrt(3 * dt);
            var up = Math.Exp(dx);
            var down = Math.Exp(-dx);

            for (int i = 0; i < n * 2 + 1; i++)
            {
                if (i < n) res[i] = s * Math.Pow(down, n - i);
                else res[i] = s * Math.Pow(up, i - n);
            }
            return res;
        }

        private static double[][] BuildPriceMatrix(double s, double dt, double vol, int n)
        {
            //Creates a array of arrays continaing the possible spot prices at each time step
            var res = new double[n + 1][];

            var dx = vol * Math.Sqrt(3 * dt);
            var up = Math.Exp(dx);
            var down = Math.Exp(-dx);

            for (int i = 0; i <= n; i++)
            {
                var size = i * 2 + 1;
                var stepPrice = new double[size];
                for (int j = 0; j < size; j++)
                {
                    if (j < i) stepPrice[j] = s * Math.Pow(down, i - j);
                    else stepPrice[j] = s * Math.Pow(up, j - i);
                }
                res[i] = stepPrice;
            }
            return res;
        }

        private static double CalcPayoff(double s, double k, bool isCall)
        {
            //Calculates payoff for call/put options at a given spot and strike
            double payoff;
            if (isCall) payoff = s - k;
            else payoff = k - s;

            if (payoff < 0) payoff = 0;
            return payoff;
        }

        private static Dictionary<int, double> CalcProbability(double dt, double r, double div, double vol)
        {
            //Creates dictionary containing probablitiy of up shock, down shock, and no movments of spot
            var p_res = new Dictionary<int, double>();
            var dx = vol * Math.Sqrt(3 * dt);
            var v = r - div - 0.5 * Math.Pow(vol, 2);
            var c = (Math.Pow(vol, 2) * dt + Math.Pow(v, 2) * Math.Pow(dt, 2)) / Math.Pow(dx, 2);

            p_res.Add(1, 0.5 * (c + v * dt / dx));
            p_res.Add(0, 1 - c);
            p_res.Add(-1, 0.5 * (c - v * dt / dx));

            return p_res;
        }
        #endregion

        #region Greeks
        public static double CalcDelta(double s, double k, double t, double r, double div,
            double vol, bool isCall, bool isEuropean, int numSteps = 252)
        {
            //Calculate Delta by shocking spot and finding option price difference
            var sUp = s * (1 + GreekShock);
            var sDown = s * (1 - GreekShock);

            var price1 = isEuropean
                ? CalcPriceEuropean(sUp, k, t, r, div, vol, isCall, numSteps)
                : CalcPriceAmerican(sUp, k, t, r, div, vol, isCall, numSteps);
            var price2 = isEuropean
                ? CalcPriceEuropean(sDown, k, t, r, div, vol, isCall, numSteps - 2)
                : CalcPriceAmerican(sDown, k, t, r, div, vol, isCall, numSteps - 2);

            return (price2 - price1) / (sDown - sUp);
        }

        public static double CalcGamma(double s, double k, double t, double r, double div,
            double vol, bool isCall, bool isEuropean, int numSteps = 252)
        {
            var sUp = s * (1 + GreekShock);
            var sDn = s * (1 - GreekShock);

            var price = isEuropean
                ? CalcPriceEuropean(s, k, t, r, div, vol, isCall, numSteps)
                : CalcPriceAmerican(s, k, t, r, div, vol, isCall, numSteps);
            var priceUp = isEuropean
                ? CalcPriceEuropean(sUp, k, t, r, div, vol, isCall, numSteps)
                : CalcPriceAmerican(sUp, k, t, r, div, vol, isCall, numSteps);
            var priceDn = isEuropean
                ? CalcPriceEuropean(sDn, k, t, r, div, vol, isCall, numSteps)
                : CalcPriceAmerican(sDn, k, t, r, div, vol, isCall, numSteps);

            return ((priceUp - price) / (sUp - s) - (price - priceDn) / (s - sDn)) / (0.5 * (sUp - sDn));
        }

        public static double CalcTheta(double s, double k, double t, double r, double div,
            double vol, bool isCall, bool isEuropean, int numSteps = 252)
        {
            //Calculate gamma by finding option price difference after one day closer to expiration
            var tMinus1 = t - 1.0 / 252.0;

            var price1 = isEuropean
                ? CalcPriceEuropean(s, k, t, r, div, vol, isCall, numSteps)
                : CalcPriceAmerican(s, k, t, r, div, vol, isCall, numSteps);
            var price2 = isEuropean
                ? CalcPriceEuropean(s, k, tMinus1, r, div, vol, isCall, numSteps - 2)
                : CalcPriceAmerican(s, k, tMinus1, r, div, vol, isCall, numSteps - 2);

            return (price2 - price1) / (t - tMinus1);
        }

        public static double CalcVega(double s, double k, double t, double r, double div,
            double vol, bool isCall, bool isEuropean, int numSteps = 252)
        {
            //Calculate Vega by shocking volatility and finding option price difference
            var volUp = vol * (1 + GreekShock);
            var volDown = vol * (1 - GreekShock);

            var price1 = isEuropean
                ? CalcPriceEuropean(s, k, t, r, div, volUp, isCall, numSteps)
                : CalcPriceAmerican(s, k, t, r, div, volUp, isCall, numSteps);
            var price2 = isEuropean
                ? CalcPriceEuropean(s, k, t, r, div, volDown, isCall, numSteps - 2)
                : CalcPriceAmerican(s, k, t, r, div, volDown, isCall, numSteps - 2);

            return (price2 - price1) / (volDown - volUp);
        }

        public static double CalcRho(double s, double k, double t, double r, double div,
            double vol, bool isCall, bool isEuropean, int numSteps = 252)
        {
            //Calculate Rho by shocking rate and finding option price difference
            var rUp = r * (1 + GreekShock);
            var rDown = r * (1 - GreekShock);

            var price1 = isEuropean
                ? CalcPriceEuropean(s, k, t, rUp, div, vol, isCall, numSteps)
                : CalcPriceAmerican(s, k, t, rUp, div, vol, isCall, numSteps);
            var price2 = isEuropean
                ? CalcPriceEuropean(s, k, t, rDown, div, vol, isCall, numSteps - 2)
                : CalcPriceAmerican(s, k, t, rDown, div, vol, isCall, numSteps - 2);

            return (price2 - price1) / (rDown - rUp);
        }
        #endregion

    }

    public static class MonteCarlo
    {
        public static List<double> CalcPriceList(Option option, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var optionType = option.GetOptionType();
            switch (optionType) {
                case OptionType.European:
                    return CalcEuroPrice((EuropeanOption)option, scenarios, numSteps, useControlVariate, parallelize);
                //case OptionType.American:
                //    return CalcAmericanPrice((AmericanOption)option, scenarios, numSteps, useControlVariate, parallelize);
                case OptionType.Asian:
                    return CalcAsianPrice((AsianOption)option, scenarios, numSteps, useControlVariate, parallelize);
                case OptionType.Digital:
                    return CalcDigitalPrice((DigitalOption)option, scenarios, numSteps, useControlVariate, parallelize);
                case OptionType.Barrier:
                    return CalcBarrierPrice((BarrierOption)option, scenarios, numSteps, useControlVariate, parallelize);
                case OptionType.Lookback:
                    return CalcLookbackPrice((LookbackOption)option, scenarios, numSteps, useControlVariate, parallelize);
                case OptionType.Range:
                    return CalcRangePrice((RangeOption)option, scenarios, numSteps, useControlVariate, parallelize);
                default:
                    throw new Exception("Option Type not avaialbe");
            }
        }
        public static double CalcPrice(Option option, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            return CalcPriceList(option, scenarios, numSteps, useControlVariate, parallelize).Average();
        }

        public static Dictionary<Greeks, double> AssembleGreeks(Option option, List<double[]> rndNums, List<double[]> scenarios,
            int numSteps, int numScenarios, VarianceReduction redux, bool parallelize, double price)
        {
            var greeks = Enum.GetValues(typeof(Greeks)).Cast<Greeks>().ToList();
            var resDic = new Dictionary<Greeks, double>();
            
            foreach (var g in greeks)
                resDic.Add(g, GreekCalc(g, option, rndNums, scenarios, numSteps, numScenarios, redux, parallelize, price));
            return resDic;
        }

        public static double GreekCalc(Greeks g, Option option, List<double[]> rndNums, List<double[]> scenarios,
            int numSteps, int numScenarios, VarianceReduction redux, bool parallelize, double price)
        {
            switch (g)
            {
                case Greeks.Delta:
                    return CalcDelta(option, scenarios, numSteps, redux.ControlVariate, parallelize);
                case Greeks.Gamma:
                    return CalcGamma(option, scenarios, numSteps, redux.ControlVariate, parallelize, price);
                case Greeks.Theta:
                    return CalcTheta(option, scenarios, rndNums, numSteps, numScenarios, redux, parallelize, price);
                case Greeks.Rho:
                    return CalcRho(option, rndNums, numSteps, numScenarios, redux, parallelize);
                case Greeks.Vega:
                    return CalcVega(option, rndNums, numSteps, numScenarios, redux, parallelize);
                default:
                    return -1.0;
            }
        }

        #region Scenario Generation
        private static double[] GenerateRandNumVector(int numSteps, int seed)
        {
            return MathUtils.RandNum.RndmNumPolarRejection(numSteps, seed);
        }

        public static double[] GenerateMCPath(Option o, int numSteps, out double[] rndNumVector, int seed)
        {
            rndNumVector = GenerateRandNumVector(numSteps, seed);
            return GenerateMCPathFromRandNums(o, numSteps, rndNumVector);
        }
        public static double[] GenerateMCPathFromRandNums(Option o, int numSteps, double[] rndNums, bool antithetic = false)
        {
            var scenario = new double[numSteps];

            var mu = o.R - o.Div;
            var dt = o.T / Convert.ToDouble(numSteps);
            var drift = Math.Exp((mu - Math.Pow(o.Vol, 2) / 2.0) * dt);

            for (int j = 0; j < numSteps; j++)
            {
                var rand = antithetic ? -rndNums[j] : rndNums[j];
                var chng = drift * Math.Exp(rand * o.Vol * Math.Sqrt(dt));
                if (j == 0) scenario[j] = chng;
                else scenario[j] = scenario[j - 1] * chng;
            }

            return scenario;
        }

        public static List<double[]> CompileScenariosFromRandNums(Option o, List<double[]> rndNumList, int numSteps, bool antithetic = false)
        {
            var scenarios = new List<double[]>();
            foreach (var rndVector in rndNumList) {
                scenarios.Add(GenerateMCPathFromRandNums(o, numSteps, rndVector));
                if (antithetic)
                    scenarios.Add(GenerateMCPathFromRandNums(o, numSteps, rndVector, true));
            }
            return scenarios;
        }
        #endregion

        #region Private Classes
        private static List<double> CalcEuroPrice(EuropeanOption o, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var res = new ConcurrentBag<double>();

            if (parallelize)
                Parallel.ForEach(scenarios, (scenario) =>
                {
                    var payoff = CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                });
            else
                foreach(var scenario in scenarios) {
                    var payoff = CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }

            return res.ToList();
        }

        private static List<double> CalcAsianPrice(AsianOption o, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var res = new ConcurrentBag<double>();
            var startAvg = o.StartAverage;
            var endAvg = o.EndAverage;

            if (parallelize)
                Parallel.ForEach(scenarios, (scenario) => {
                    var spotAvg = 0.0;
                    for (int i = startAvg; i < endAvg; i++)
                        spotAvg += o.Spot * scenario[i];
                    var payoff = CalcPayoff(spotAvg/numSteps, o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }); else
                foreach (var scenario in scenarios) {
                    var spotAvg = 0.0;
                    for (int i = 0; i < endAvg; i++)
                        spotAvg += o.Spot * scenario[i];
                    var payoff = CalcPayoff(spotAvg / numSteps, o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }
            return res.ToList();
        }

        private static List<double> CalcDigitalPrice(DigitalOption o, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var res = new ConcurrentBag<double>();
            var rebate = o.Rebate;

            if (parallelize)
                Parallel.ForEach(scenarios, (scenario) =>
                {
                    var payoff = 0.0;
                    if(CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall) > 0)
                        payoff = rebate;
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                });
            else
                foreach (var scenario in scenarios)
                {
                    var payoff = 0.0;
                    if (CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall) > 0)
                        payoff = rebate;
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }
            return res.ToList();
        }

        private static List<double> CalcBarrierPrice(BarrierOption o, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var res = new ConcurrentBag<double>();
            var barrier = o.Barrier / o.Spot;

            if (parallelize)
                Parallel.ForEach(scenarios, (scenario) => {
                    bool barrierHit = false;
                    for (int i = 0; i < numSteps - 1; i++) {
                        barrierHit = o.IsUp ? barrier < scenario[i] : barrier > scenario[i];
                        if (barrierHit)
                            i = numSteps;
                    }
                    var payoff = 0.0;
                    if(barrierHit && o.IsIn)
                        payoff = CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall);
                    else if (!barrierHit && !o.IsIn)
                        payoff = CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall);

                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                });
            else
                foreach (var scenario in scenarios) {
                    bool barrierHit = false;
                    for (int i = 0; i < numSteps - 1; i++) {
                        barrierHit = o.IsUp ? barrier < scenario[i] : barrier > scenario[i];
                        if (barrierHit)
                            i = numSteps;
                    }
                    var payoff = 0.0;
                    if (barrierHit && o.IsIn || !barrierHit && !o.IsIn)
                        payoff = CalcPayoff(o.Spot * scenario[numSteps - 1], o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }
            return res.ToList();
        }

        private static List<double> CalcLookbackPrice(LookbackOption o, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var res = new ConcurrentBag<double>();

            if (parallelize)
                Parallel.ForEach(scenarios, (scenario) =>
                {
                    var spot = o.Spot * (o.IsCall ? scenario.ToList().Max() : scenario.ToList().Min());
                    var payoff = CalcPayoff(spot, o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                });
            else
                foreach (var scenario in scenarios)
                {
                    var spot = o.Spot * (o.IsCall ?  scenario.ToList().Max() : scenario.ToList().Min());
                    var payoff = CalcPayoff(spot, o.K, o.IsCall);
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }

            return res.ToList();
        }

        private static List<double> CalcRangePrice(RangeOption o, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            var res = new ConcurrentBag<double>();

            if (parallelize)
                Parallel.ForEach(scenarios, (scenario) =>
                {
                    var maxSpot = o.Spot * scenario.ToList().Max();
                    var minSpot = o.Spot * scenario.ToList().Min();
                    var payoff = maxSpot - minSpot;
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                });
            else
                foreach (var scenario in scenarios)
                {
                    var maxSpot = o.Spot * scenario.ToList().Max();
                    var minSpot = o.Spot * scenario.ToList().Min();
                    var payoff = maxSpot - minSpot;
                    if (useControlVariate) payoff -= CalcCVValue(o, scenario, numSteps);
                    res.Add(payoff * Math.Exp(-o.R * o.T));
                }

            return res.ToList();
        }


        private static double CalcCVValue(Option o, double[] scenario, int numSteps)
        {
            double res = 0.0;

            var sInitial = o.Spot;
            var mu = o.R - o.Div;
            var dt = o.T / Convert.ToDouble(numSteps);

            var delta = BlackScholes.CalcDelta(o);

            for (int j = 0; j < numSteps; j++)
            {
                var sPrev = j == 0 ? 1 : scenario[j - 1];
                res += delta * sInitial * (scenario[j] - sPrev * Math.Exp(mu * dt));

                var t = o.T - dt * (j + 1);
                var s = sInitial * scenario[j];
                delta = BlackScholes.CalcDelta(s, o.K, t, o.R, o.Div, o.Vol, o.IsCall);
            }

            return res;
        }

        private static double CalcPayoff(double s, double k, bool isCall)
        {
            //Calculates payoff for call/put options at a given spot and strike
            double payoff = isCall ? s - k : k - s;
            return Math.Max(payoff, 0.0);
        }
        #endregion

        #region Greeks
        private const double GreekShock = 0.001; //size of shock for greeks calculation

        public static double CalcDelta(Option option, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize)
        {
            //Calculate Delta by shocking spot and finding option price difference
            var initialS = option.Spot;
            var sUp = initialS * (1 + GreekShock);
            var sDown = initialS * (1 - GreekShock);

            option.Spot = sUp;
            var priceUp = CalcPrice(option, scenarios, numSteps, useControlVariate, parallelize);
            option.Spot = sDown;
            var priceDwn = CalcPrice(option, scenarios, numSteps, useControlVariate, parallelize);
            option.Spot = initialS;

            return (priceDwn - priceUp) / (sDown - sUp);
        }

        public static double CalcGamma(Option option, List<double[]> scenarios, int numSteps, bool useControlVariate, bool parallelize, 
            double price)
        {
            //Calculate Gamma by shocking spot and finding option price difference
            var sInitial = option.Spot;
            var sUp = sInitial * (1 + GreekShock);
            var sDown = sInitial * (1 - GreekShock);

            option.Spot = sUp;
            var priceUp = CalcPrice(option, scenarios, numSteps, useControlVariate, parallelize);
            option.Spot = sDown;
            var priceDwn = CalcPrice(option, scenarios, numSteps, useControlVariate, parallelize);
            option.Spot = sInitial;

            return (priceUp - 2 * price + priceDwn) / Math.Pow(sInitial * GreekShock, 2);
        }

        public static double CalcTheta(Option option, List<double[]> scenarios, List<double[]> rndNums, int numSteps, 
            int numScenarios, VarianceReduction redux, bool parallelize, double price)
        {          
            //Calculate theta by finding option price difference after one day closer to expiration
            var initialT = option.T;
            var dt = initialT / 252.0;
            var tminus1 = initialT - dt;
            option.T = tminus1;

            var scenariosDwn = CompileScenariosFromRandNums(option, rndNums, numSteps, redux.Antithetic);
            var priceDwn = CalcPrice(option, scenariosDwn, numSteps, redux.ControlVariate, parallelize);
            option.T = initialT;

            return (priceDwn - price) / (dt);
        }

        public static double CalcVega(Option option, List<double[]> rndNums, int numSteps, int numScenarios, 
            VarianceReduction redux, bool parallelize)
        {
            //Calculate Vega by shocking volatility and finding option price difference
            var vInitial = option.Vol;
            var vUp = vInitial * (1 + GreekShock);
            var vDown = vInitial * (1 - GreekShock);

            option.Vol = vUp;
            var scenarios = CompileScenariosFromRandNums(option, rndNums, numSteps, redux.Antithetic);
            var priceUp = CalcPrice(option, scenarios, numSteps, redux.ControlVariate, parallelize);

            option.Vol = vDown;
            scenarios = CompileScenariosFromRandNums(option, rndNums, numSteps, redux.Antithetic);
            var priceDwn = CalcPrice(option, scenarios, numSteps, redux.ControlVariate, parallelize);
            option.Vol = vInitial;

            return (priceDwn - priceUp) / (vDown - vUp);
        }

        public static double CalcRho(Option option, List<double[]> rndNums, int numSteps, int numScenarios,
            VarianceReduction redux, bool parallelize)
        {
            //Calculate Rho by shocking rate and finding option price difference
            var rInitial = option.R;
            var rUp = rInitial * (1 + GreekShock);
            var rDown = rInitial * (1 - GreekShock);

            option.R = rUp;
            var scenarios = CompileScenariosFromRandNums(option, rndNums, numSteps, redux.Antithetic);
            var priceUp = CalcPrice(option, scenarios, numSteps, redux.ControlVariate, parallelize);

            option.R = rDown;
            scenarios = CompileScenariosFromRandNums(option, rndNums, numSteps, redux.Antithetic);
            var priceDwn = CalcPrice(option, scenarios, numSteps, redux.ControlVariate, parallelize);
            option.R = rInitial;

            return (priceDwn - priceUp) / (rDown - rUp);
        }
        #endregion

    }
}
