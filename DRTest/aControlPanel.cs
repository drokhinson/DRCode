
using DRTest;
using DRLib.CsvUtils;

var tList = new List<Test>() {
    new("David", 31) { BoolField = false },
    new("Bill", 1),
    new("Dinah", 144),
    new("Julia", 29),
    new("Zeus", 48) { BoolField = false },
};

tList.WriteCsv(DRTest.DRGlobal.Paths.DrSrcFiles + $"\\Test1.csv");
tList.WriteCsv(DRTest.DRGlobal.Paths.DrSrcFiles + $"\\Test2.csv", CsvWriter.CsvOptions.Properties);
//CompareRandomSpeed((int)1e8);

//Console.ReadLine();


static void CompareRandomSpeed(int num)
{
    UsefulMethods.RunTimed($"JUST RANDOM\t{num:N0}", () =>
    {
        var rr = new Random();
        var res = new double[num];
        for (int i = 0; i < num; i++)
            res[i] = rr.NextDouble();
        return res;
    });

    GC.Collect();
    
    var xx = UsefulMethods.RunTimed($"DAVID RANDOM\t{num:N0}", () => MathUtils.RandNum.GetRandomNumber(num));
}

public record Test(string Name, int Age)
{
    public bool BoolField = true;
    private string FuUUK = "fook";
}