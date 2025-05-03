namespace DRLib.Charting;

public record class Canvas(string Title, string XAxis, string YAxis)
{
    public readonly List<ChartObj> Objects = new();
}

public abstract record ChartObj(string Name);

public abstract record Scatter(string Name) : ChartObj(Name)
{
    public readonly List<PointMsg> Points = new();
}

public record PointMsg(double X, double Y);