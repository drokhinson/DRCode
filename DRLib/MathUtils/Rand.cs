namespace DRLib.MathUtils;

public static class Rand
{
    private const int NumCores = 15;
    private static int GetNumCores(int quantity) => quantity < 1000 ? 1 : NumCores;

    public static double[] GenerateSlow(int quantity, int? seed = null, double[] res = null)
    {
        res ??= new double[quantity];

        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        for (int i = 0; i < quantity; i++) {
            res[i] = rand.NextDouble();
        }

        return res;
    }

    public static double[] Generate(int quantity, int? seed = null, double[] res = null)
    {
        res ??= new double[quantity];

        var numProcessors = GetNumCores(quantity);
        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        var seeds = new int[numProcessors];
        for (int si = 0; si < numProcessors; si++)
            seeds[si] = rand.Next();

        var batchSize = quantity / numProcessors;
        Parallel.For(0, numProcessors, x => {
            int rSeed = seeds[x];

            var rGen = new Random(rSeed);
            var toGen = batchSize;
            if (x == numProcessors - 1)
                toGen += quantity % numProcessors;
            for (int i = 0; i < toGen; i++)
                res[x * batchSize + i] = rGen.NextDouble();
        });

        return res;
    }

    /// <summary> 
    /// Pseudo-random numbers/Splitmix64 (default pseudo-random number generator algorithm in Java) <br />
    /// Mostly used for seed generation for Random number generator
    /// </summary>
    public class SplitMix
    {
        public ulong X;

        public ulong Next()
        {
            ulong z = X += 0x9e3779b97f4a7c15;
            z = (z ^ z >> 30) * 0xbf58476d1ce4e5b9;
            z = (z ^ z >> 27) * 0x94d049bb133111eb;
            return z ^ z >> 31;
        }

        public static ulong GetSeed(int id) => GetSeed((ulong)id);
        public static ulong GetSeed(DateTime dt) => GetSeed((ulong)dt.Ticks);
        public static ulong GetSeed(ulong id)
        {
            ulong ret = id;
            var r = new SplitMix();
            r.X = id;
            for (int i = 0; i < 100; i++)
                ret = r.Next();

            return ret;
        }
    }

    public static class NormalDist
    {
        /// <summary> Generates array of size 'quant' normally distributed random numbers using Box-Muller method </summary>
        public static double[] BoxMuller(int quant, int? seed = null) => BoxMuller(new double[quant], seed);

        /// <summary> Fills 'res' array with normally distributed random numbers using Box-Muller method </summary>
        public static double[] BoxMuller(double[] res, int? seed = null)
        {
            var rndNumQuant = res.Length % 2 == 0 ? res.Length : res.Length + 1; // ensure even number of rndNums generated

            var rand = Generate(rndNumQuant, seed);

            var maxIndex = rndNumQuant / 2;

            Parallel.For(0, maxIndex, x => {
                var i = x * 2;
                var n1 = rand[i];
                var n2 = rand[i + 1];

                res[i] = Math.Sqrt(-2 * Math.Log(n1)) * Math.Cos(2 * Math.PI * n2);
                if (i + 1 == res.Length)
                    return;

                res[i + 1] = Math.Sqrt(-2 * Math.Log(n1)) * Math.Sin(2 * Math.PI * n2);
            });

            return res;
        }

        /// <summary> 
        /// Generates array of size 'quant' normally distributed random numbers using the Polar-Rejection method <br />
        /// Account for rejection by generating extra normally distributed numbers using BoxMuller
        /// </summary>
        public static double[] PolarRejection(int quant, int? seed = null) => PolarRejection(new double[quant], seed);

        /// <summary> 
        /// Fills 'res' array with normally distributed random numbers using the Polar-Rejection method <br />
        /// Account for rejection by generating extra normally distributed numbers using BoxMuller
        /// </summary>
        public static double[] PolarRejection(double[] res, int? seed = null)
        {
            var quant = res.Length;
            var rndNumQuant = (int)(quant * 1.25); // generate extra random numbers to account for expected 21.47% rejection rate
            rndNumQuant = rndNumQuant % 2 == 0 ? rndNumQuant : rndNumQuant + 1; // ensure even number of rndNums generated

            var rand = Generate(rndNumQuant, seed);

            int numsGen = 0;
            for (int i = 0; i < rndNumQuant; i += 2) {
                var n1 = rand[i] * 2 - 1;
                var n2 = rand[i + 1] * 2 - 1;
                var w = Math.Pow(n1, 2) + Math.Pow(n2, 2);

                if (w > 1)
                    continue; // reject

                var c = Math.Sqrt(-2 * Math.Log(w) / w);

                res[numsGen++] = c * n1;
                if (numsGen == quant)
                    break;

                res[numsGen++] = c * n2;
                if (numsGen == quant)
                    break;
            }

            if (numsGen == quant)
                return res;

            // fill in rejected values using BoxMuller
            var toFill = quant - numsGen;
            var newSeed = seed.HasValue ? SplitMix.GetSeed((ulong)(seed.Value + numsGen)) : SplitMix.GetSeed(DateTime.Now);
            var bmNrmRnd = BoxMuller(toFill, (int)newSeed);
            for (int i2 = 0; i2 < toFill; i2++)
                res[i2 + quant - toFill] = bmNrmRnd[i2];

            return res;
        }
    }
}