using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceLib.FinancialCalculators
{
    public static class FinanceCalculators
    {
        public static double StndErr(List<double> vals, double? estimate = null)
        {
            return Math.Sqrt(Variance(vals, estimate) / vals.Count());
        }

        public static double StndErrAntithetic(List<double> vals, double? estimate = null)
        {
            return Math.Sqrt(3.0/4.0 * Variance(vals, estimate) / (vals.Count()));
        }


        public static double Variance(List<double> vals, double? estimate = null)
        {
            if (estimate == null) 
                estimate = vals.Average();

            return vals.Sum(r => Math.Pow(r - estimate.Value, 2)) / (vals.Count() - 1);
        }

        public static double StndDev(List<double> vals, double? estimate = null)
        {
            double sum = 0.0;
            if (estimate == null) estimate = vals.Average();
            foreach (var n in vals) sum += Math.Pow(n - estimate.Value, 2);

            return Math.Sqrt(sum / (vals.Count() - 1));
        }

        public static double Spline(double xVal, double[,] data)
        {
            throw new NotImplementedException();
        }
    }


}