using DRTest;
using DRTest.DRGlobal;
using DRLib.CsvUtils;
using DRLib.MathUtils;

var quants = new double[] { 5, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8 };

var res = new List<SpeedTest>();

foreach (var _q in quants) {
    var q = (int)_q;
    
    UsefulMethods.RunTimed($"DAVID RANDOM\t{q:N0}", () => Rand.Generate(q), out var dur);
    
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

        
    UsefulMethods.RunTimed($"JUST RANDOM\t{q:N0}", () =>
    {
        var rr = new Random();
        var res = new double[q];
        for (int i = 0; i < q; i++)
            res[i] = rr.NextDouble();
        return res;
    }, out var durOg);

    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
    
    UsefulMethods.RunTimed($"DAVID RANDOM\t{q:N0}", () => Rand.Generate(q), out var dur2);
    
    
    res.Add(new(q, dur, durOg, dur2));
    
    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
}

res.WriteCsv(Paths.TestFiles + "/SpeedTest.csv");

//Console.ReadLine();

public record SpeedTest(int QuantNum, double DavidDur, double CSharpDur, double DavidDur2);