    using System.Drawing;
    using DRLib.Charting;
    using DRLib.CsvUtils;
    using DRLib.Finance;
    using DRLib.Html;
    using DRLib.Html.UserUI;
    using DRLib.Html.UserUI.Elements.Buttons;
    using DRLib.Html.UserUI.Elements.Text;
    using DRLib.Instrument;
    using DRLib.MathUtils;
    using DRTest.DRGlobal;
    using DRLib.Html.UserUI.Events;
    using DRLib.Html.UserUI.Events.Form;
    using DRLib.Html.UserUI.Events.Mouse;
    using DRLib.Html.Utils;

    //RunPricingTest();
    //RunPlotting();
    //TestCharts();
    RunEventTest();
    Console.WriteLine("DONE");

    static void RunEventTest()
    {
        var hb = new HtmlBuilder();
        
        // ADD BUTTON
        var b1 = hb.AddWLine(new Button("button1", "Button 1"));
        b1.AddEvent(new OnClick(), (_, _) => {
            Console.WriteLine("Button 1 Clicked");
            return "done";
        });

        // ADD BUTTON
        var b2 = hb.AddWLine(new Button("button2", "julia button 2"));
        b2.AddEvent(new OnDblClick(), (_, _) => {
            Console.WriteLine("Button 2 DblClicked");
            return "done";
        });

        b2.BackColor(Color.Cyan);

        // ADD TEXTBOX
        var text1 = hb.AddWLine(new Password("password"));
        text1.AddLabel("Enter Secret password");
        text1.AddEvent(new OnChange(), (_, arg) => {
            Console.WriteLine($"Password entered: {arg.TargetValue}");
            return "done";
        });
        
        
        var html = hb.RenderHtml();

        File.WriteAllText(Paths.TestFiles + "/listenerTest.html", html);

        string[] prefixes = { CallCSharpJScript.DefaultHost };
        var server = new HtmlEventServer(prefixes);
        server.LoadListeners(hb.Html);
        server.Start();
    }
    
    static void TestCharts()
    {
        var chart = new Chart("Test Chart", "Time", "Money");
        var c1 = chart.Add(new LinePlot("Bar1"));
        c1.Add(1, 10);
        c1.Add(2, 3);
        c1.Add(3, 15);
        c1.Add(3, -1);

        var c2 = chart.Add(new BarPlot("Bar2") {
            Color = Color.Red
        });
        c2.Add(2, 12);
        var c3 = chart.Add(new BarPlot("Bar2") {
            Color = Color.Yellow
        });
        c3.Add(1, 11);

        var hb = new HtmlBuilder();
        hb.Add(new ChartJsElement("scatter", chart));
        var html = hb.RenderHtml();
        File.WriteAllText(Paths.TestFiles + "/chartTest2.html", html);
        
    }
    
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
        res.WriteToCsv(Paths.TestFiles + @"\Pricing.csv");
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

        //htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(x1a, name: "RandNums")));
        //htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(x2a, name: "BoxMuller")));
        //htmlBuilder.AddWLine(ToChartJs(ChartUtils.PlotDist(x2b, name: "Polar")));

        static ChartJsElement ToChartJs(Chart c) => new ChartJsElement(c.Title, c);

        var graphMsg = new Chart("MonteCarlo_Scenarios", "Steps", "Spot");

        var rnd = new Random();

        for (int r = 0; r < Math.Min(100, ss.NumRow); r++) {
            var curveMsg = new LinePlot("") { Color = RandColor() };
            curveMsg.Add(0, md.Spot);
            graphMsg.Add(curveMsg);

            for (int c = 0; c < ss.NumCol; c++) {
                curveMsg.Add(c + 1, ss[r, c]);
            }
        }
        Color RandColor() => Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        //htmlBuilder.AddWLine(ToChartJs(graphMsg));


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
