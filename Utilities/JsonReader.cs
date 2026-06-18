using Newtonsoft.Json;
using PlaywrightCSharpFramework.Config;
using System;
using System.Collections.Generic;
using System.IO;

public class UserData
{
    [JsonProperty("testname")]
    public string? Testname { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("expected")]
    public string Expected { get; set; } = string.Empty;
}

public class JsonReader
{
    public static FrameworkSettings settings = SettingsLoader.Load();
    public static List<UserData> ReadUsers(string file = "users.json")
    {
        string fullPath = Path.Combine(settings.BaseDirectory, file);
        string json = File.ReadAllText(fullPath);
        return JsonConvert.DeserializeObject<List<UserData>>(json) ?? new List<UserData>();
    }
}
