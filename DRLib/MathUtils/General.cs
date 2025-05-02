namespace DRLib.MathUtils;

public static class General
{
    public static double StndErr(IEnumerable<double> vals, double? estimate = null)
    {
        return Math.Sqrt(Variance(vals, estimate) / vals.Count());
    }

    public static double StndErrAntithetic(IEnumerable<double> vals, double? estimate = null)
    {
        return Math.Sqrt(3.0 / 4.0 * Variance(vals, estimate) / vals.Count());
    }


    public static double Variance(IEnumerable<double> vals, double? estimate = null)
    {
        if (estimate == null)
            estimate = vals.Average();

        return vals.Sum(r => Math.Pow(r - estimate.Value, 2)) / (vals.Count() - 1);
    }

    public static double StndDev(IEnumerable<double> vals, double? estimate = null)
    {
        estimate ??= vals.Average();
        var sum = vals.Sum(r => Math.Pow(r - estimate.Value, 2));

        return Math.Sqrt(sum / (vals.Count() - 1));
    }
}