using CsvHelper;
using PlaywrightCSharpFramework.Config;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class CsvUserReader
{
    public static FrameworkSettings settings = SettingsLoader.Load();

    public static List<UserData> ReadCsv(string file = "users.csv")
    {
        string fullPath = Path.Combine(settings.BaseDirectory, file);

        using var reader = new StreamReader(fullPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<UserData>().ToList();
    }
}