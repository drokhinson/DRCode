namespace DRLib.MathUtils;

public static class RootFinding
{
    public static double NewtonRapson(double shock, Func<double, double> f_x, double x0,
        double yFinal = 0, double err = 1e-4, int loopMax = (int)1e3)
    {
        return NewtonRapson(f_x, df_dx, x0, yFinal, err, loopMax);

        double df_dx(double x)
        {
            var up = x * 1.0 + shock;
            var dn = x * 1.0 - shock;
            return (f_x(up) - f_x(dn)) / (up - dn);
        }
    }


    public static double NewtonRapson(Func<double, double> f_x, Func<double, double> df_dx, double x0,
        double yFinal = 0, double err = 1e-4, int loopMax = (int)1e3)
    {
        int i = 0;

        double yEst;
        var x = x0;
        do {
            var slope = df_dx(x);
            yEst = f_x(x);
            var dx = (yFinal - yEst) / slope;
            x += dx;

        } while (Math.Abs(yFinal - yEst) > err && i < loopMax);

        return x;
    }
}