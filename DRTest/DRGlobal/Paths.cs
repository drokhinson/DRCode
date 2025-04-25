namespace DRTest.DRGlobal;

public static class Paths
{
    public static readonly string Desktop =  Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static readonly string DrRoot =  @"C:\Users\drokh\Documents\DRCode\DRLib";
    public static readonly string DrSrcFiles =  Path.Combine(DrRoot, "Files");
}