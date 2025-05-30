using System.Drawing;

namespace DRLib.Charting;

public record Chart(string Title, string XAxis, string YAxis)
{
    public readonly List<ChartObj> Objects = new();

    public T Add<T>(T ob) where T : ChartObj
    {
        Objects.Add(ob);
        return ob;
    } 
}

public abstract record ChartObj(string Name)
{
    public Color Color { get; set; } = Color.Blue;
}

public abstract record PointPlot(string Name) : ChartObj(Name)
{
    public readonly List<Point> Points = new();
    
    public Point Add(double x, double y) => Add(new (x, y));

    public Point Add(Point p)
    {
        Points.Add(p);
        return p;
    }
}

public record Point(double X, double Y);

public record ScatterPlot(string Name) : PointPlot(Name);
public record LinePlot(string Name) : PointPlot(Name);
public record BarPlot(string Name) : PointPlot(Name);
public record HorizontalBarPlot(string Name) : PointPlot(Name);