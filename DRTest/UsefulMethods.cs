namespace DRTest;

public static class UsefulMethods
{
    public static T RunTimed<T>(string desc, Func<T> func)
    {
        var startTime = DateTime.Now;

        var res = func();
        
        var endTime = DateTime.Now;
        var dur = (endTime - startTime).TotalSeconds;
        var durStr = dur < 60 ? $"{dur:N3} sec." : $"{dur / 60.0:N2} mins";
        Console.WriteLine($"COMPLETED [{desc}]. {durStr}\n\n");
        
        return res;
    }
}