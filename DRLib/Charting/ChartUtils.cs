namespace DRLib.Charting;

public static class ChartUtils
{
    public static Chart PlotDist(double[] vals, int numBins = 30, string name = "Dist")
    {
        // Calculate the range of the data
        var min = vals.Min();
        var max = vals.Max();
        var binWidth = (max - min) / numBins;

        // Initialize bins
        var bins = new Dictionary<(double Min, double Max), int>();

        for (int i = 0; i < numBins; i++) {
            var strt = min + i * binWidth;
            bins.Add((strt, strt + binWidth), 0);
        }

        // Populate bins
        foreach (var b in bins.Keys) 
            bins[b] = vals.Count(r => r >= b.Min && r < b.Max);


        var graph = new Chart(name, "Bins", "Values");

        var curve = graph.Add(new BarPlot("Data"));

        foreach (var bin in bins) {
            double binMidpoint = (bin.Key.Min + bin.Key.Max) / 2;
            curve.Add(binMidpoint, bin.Value);
        }

        return graph;
    }
}