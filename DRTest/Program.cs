var startTime = DateTime.Now;



var endTime = DateTime.Now;
var dur = (endTime - startTime).TotalSeconds;
var durStr = dur < 60 ? $"{dur:N3} sec." : $"{dur / 60.0:N2} mins";
Console.WriteLine($"Done. {durStr}");
Console.ReadLine();