namespace DRTest.DRGlobal;

public static class Paths
{
    public static readonly string Desktop =  Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static readonly string Root =  @"C:\CodeProjects\DRCode\DRTest";
    public static readonly string TestFiles =  Path.Combine(Root, "TestFiles");
}