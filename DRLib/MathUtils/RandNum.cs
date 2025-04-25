using System.Collections.Concurrent;

namespace MathUtils;

public static class RandNum
{
    public static double[] GetRandomNumber(int quantity, int? seed = null)
    {
        var res = new double[quantity];
        var numProcessors = quantity > 1e6 ? 12 : 1; // fire up 1 processor per 1e6
        
        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        
        var lo = new object();
        var batchSize = (int)(quantity / numProcessors);
        Parallel.For(0, numProcessors, x =>
        {
            int rSeed;
            lock (lo)
            {
                rSeed = rand.Next();
            }

            var rGen = new Random(rSeed);
            var toGen = batchSize;
            if (x == numProcessors - 1)
                toGen += quantity % numProcessors;
            
            for (int i = 0; i < toGen; i++)
                res[x * batchSize + i] = (rGen.NextDouble());
        });
        
        return  res;
    }
    
    /// <summary>
    ///  Generates two normally distributed random numbers using the Box-Muller method
    /// </summary>
    public static double[] GetBoxMuller(int quantity, int? seed = null)
    {
        var rand = GetRandomNumber(quantity * 2, seed);

        var res =  new double[quantity];
        for (int i = 0; i < quantity; i++)
        {
            var n1 = rand[i];
            var n2 = rand[i + 1];
            res[i * 2] = Math.Sqrt(-2 * Math.Log(n1)) * Math.Cos(2 * Math.PI * n2);
            res[i * 2 + 1] = Math.Sqrt(-2 * Math.Log(n1)) * Math.Sin(2 * Math.PI * n2);
        }

        return res;
    }

    /// <summary>
    ///  Generates two normally distributed random numbers using the Polar-Rejection method
    /// </summary>
    public static double[] RndmNumPolarRejection(int quantity, int? seed = null)
    {
        var rand = GetRandomNumber(quantity * 2, seed);

        var res =  new double[quantity];
        for (int i = 0; i < quantity; i++)
        {
            var n1 = rand[i];
            var n2 = rand[i + 1];
            var (nr1, nr2) = PolarRejection(n1, n2);
            res[i * 2] = nr1;
            res[i * 2 + 1] = nr2;
        }

        return res;

        static (double, double) PolarRejection(double n1, double n2)
        {
            double w;
            do
            {
                n1 = n1 * 2 - 1;
                n2 = n2 * 2 - 1;
                w = Math.Pow(n1, 2) + Math.Pow(n2, 2);
            } while (w > 1);

            var c = Math.Sqrt(-2 * Math.Log(w) / w);

            var r1 = c * n1;
            var r2 = c * n2;
            return (r1, r2);
        }
    }

}