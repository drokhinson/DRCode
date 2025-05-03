    using System.Drawing;
    using DRLib.Charting;
    using DRLib.CsvUtils;
    using DRLib.Finance;
    using DRLib.Html;
    using DRLib.Instrument;
    using DRLib.MathUtils;
    using DRTest.DRGlobal;

    RunPricingTest();
    RunPlotting();
    Console.WriteLine("DONE");
    
    static void RunPricingTest()
    {
        var numScen = 10000;
        var numSteps = 1;
        var k = 95;
        var t = 1.0;
        var md = new OptMarketData(100, t, Rate: 0.2, Div: 0.01, Vol: 0.1);
        Console.WriteLine(md);

        var res = new List<TestOut>();
        res.AddRange(DoIt(new EuroPut("SPX", 95, DateTime.Today), md));
        res.AddRange(DoIt(new EuroCall("SPX", 95, DateTime.Today), md));
        res.AddRange(DoIt(new DigiCall("SPX", 95, DateTime.Today), md));
        res.AddRange(DoIt(new DigiPut("SPX", 95, DateTime.Today), md));
        res.AddRange(DoIt(new AmericanCall("SPX", 95, DateTime.Today), md));
        res.AddRange(DoIt(new AmericanPut("SPX", 95, DateTime.Today), md));
        res.WriteCsv(Paths.TestFiles + @"\All.csv");
    }

    static void RunPlotting() 
    {        
        var numScen = 10000;
        var numSteps = 1;
        var k = 95;
        var t = 1.0;
        var md = new OptMarketData(100, t, Rate: 0.2, Div: 0.01, Vol: 0.1);
        Console.WriteLine(md);
        
        EuropeanOption put = new EuroPut("SPX", k, DateTime.Today);
        var call = new EuroCall("SPX", k, DateTime.Today);
        
        Console.WriteLine($@"put price mc: {MonteCarlo.Price(put, md, numScen, numSteps, out var ss)}");
        Console.WriteLine($@"put price bs: {BlackScholes.Price(put, md)}");
        Console.WriteLine($@"call price mc: {MonteCarlo.Price(call, ss)}");
        Console.WriteLine($@"call price bs: {BlackScholes.Price(call, md)}");

        var htmlBuilder = new HtmlBuilder();

        htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(ss.ColumnGet(^1), 50, "MonteCarlo_Dist")));

        var seed = 1233;
        var qunt = (int)1e7;

        var x1a = Rand.Generate(qunt, seed);
        var x2a = Rand.NormalDist.BoxMuller(qunt, seed);
        var x2b = Rand.NormalDist.PolarRejection(qunt, seed);

        htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(x1a, name: "RandNums")));
        htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(x2a, name: "BoxMuller")));
        htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(x2b, name: "Polar")));

        static ChartJsElement ToChartJs(Canvas c) => new ChartJsElement(c.Title, c);

        var graphMsg = new Canvas("MonteCarlo_Scenarios", "Steps", "Spot");

        var rnd = new Random();

        for (int r = 0; r < Math.Min(1000, ss.NumRow); r++) {
            var curveMsg = new LinePlot("") { Color = RandColor() };
            curveMsg.Add(0, md.Spot);
            graphMsg.Add(curveMsg);

            for (int c = 0; c < ss.NumCol; c++) {
                curveMsg.Add(c + 1, ss[r, c]);
            }
        }
        Color RandColor() => Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        htmlBuilder.AddWLine(ToChartJs(graphMsg));


        var html = htmlBuilder.RenderHtml();
        File.WriteAllText(Path.Combine(Paths.TestFiles, "dist.html"), html);
    }

    static List<TestOut> DoIt(Option o, OptMarketData md)
    {
        var res = new List<TestOut>();
        var shockSize = 0.01;

        try {
            res.AddRange([
                new (o.GetType().Name, "BlackScholes", "Price", BlackScholes.Price(o, md)),
                new (o.GetType().Name, "BlackScholes", "Delta", BlackScholes.Delta(o, md)),
                new (o.GetType().Name, "BlackScholes", "Gamma", BlackScholes.Gamma(o, md)),
                new (o.GetType().Name, "BlackScholes", "Vega", BlackScholes.Vega(o, md)),
                new (o.GetType().Name, "BlackScholes", "Theta", BlackScholes.Theta(o, md)),
                new (o.GetType().Name, "BlackScholes", "Rho", BlackScholes.Rho(o, md)),
            ]);
        }
        catch { }


        res.AddRange([
            new (o.GetType().Name, "TrinomialTree", "Price", TrinomialTree.Price(o, md)),
            new (o.GetType().Name, "TrinomialTree", "Delta", TrinomialTree.Delta(o, md, shockSize)),
            new (o.GetType().Name, "TrinomialTree", "Gamma", TrinomialTree.Gamma(o, md, shockSize)),
            new (o.GetType().Name, "TrinomialTree", "Vega", TrinomialTree.Vega(o, md, shockSize)),
            new (o.GetType().Name, "TrinomialTree", "Theta", TrinomialTree.Theta(o, md)),
            new (o.GetType().Name, "TrinomialTree", "Rho", TrinomialTree.Rho(o, md, shockSize)),
        ]);

        try {
            var ss = MonteCarlo.GetScenarioSet(10000, 252, md);
            res.AddRange([
                new (o.GetType().Name, "Monte Carlo", "Price", MonteCarlo.Price(o, ss)),
            ]);
        }
        catch { }

        return res;
    }
    
    record TestOut(string Option, string Method, string Type, double Value);
