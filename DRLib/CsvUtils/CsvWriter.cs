using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DRLib.CsvUtils;

public static class CsvWriter
{
    [Flags]
    public enum Options
    {
        Properties = 1 << 1,
        Fields = 1 << 2,
        
        AllMembers = Properties | Fields,
    }

    public static string GetUniqueFileName(string filePath)
    {
        int i = 1;
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var ext = Path.GetExtension(filePath);
        var dir = Path.GetDirectoryName(filePath);

        while (File.Exists(filePath)) {
            filePath = Path.Combine(dir, $"{fileName}_{i++}{ext}");
        }
        
        return filePath;
    }
    
    public static void WriteCsv<T>(this IEnumerable<T> data, string filePath, Options opt = Options.AllMembers)
    {
        var csvStr = ToCsvString(data, opt);
        var toWrite = GetUniqueFileName(filePath);
        File.WriteAllText(toWrite, csvStr);
    }
    
    public static string ToCsvString<T>(this IEnumerable<T> data, Options opts = Options.AllMembers) =>
        ToDelimString(data, ",",  opts);
    
    public static string ToDelimString<T>(this IEnumerable<T> data, string delim, Options opts = Options.AllMembers)
    {
        var type =  typeof(T);
        var list = data.ToArray();
        
        var includeProps = opts.HasFlag(Options.Properties);
        var includeFields = opts.HasFlag(Options.Fields);

        var binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.GetField;
        var members = new List<MemberInfo>();
        if (includeProps)
            members.AddRange(type.GetProperties(binding));
        if (includeFields)
            members.AddRange(type.GetFields(binding));
        
        var headerRow = members.Select(GetColumnName).ToArray();
        
        var writer =  new StringWriter();
        writer.WriteLine(string.Join(delim, headerRow));

        foreach (var i in list) {
            var row = members.Select(m => GetValue(m, i)).ToArray();      
            writer.WriteLine(string.Join(delim, row));
        }

        return writer.ToString();
        
        static string GetColumnName(MemberInfo member)
        {
            var attr =  member.GetCustomAttribute<DisplayAttribute>();
            if (attr == null)
                return member.Name;
            
            return attr.Name ?? attr.ShortName ??  member.Name;
        }
        
        object GetValue(MemberInfo member, T i)
        {
            return member switch {
                PropertyInfo pi => pi?.GetValue(i),
                FieldInfo fi => fi.GetValue(i),
                _ => throw new NotImplementedException()
            };
        }
    }
}