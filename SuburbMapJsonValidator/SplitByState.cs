using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class SplitByState
{
    public static void Run(string inputPath, string outputDir)
    {
        string json = File.ReadAllText(inputPath);
        var geo = JObject.Parse(json);

        var features = geo["features"]?.ToList();
        if (features == null || features.Count == 0)
        {
            Console.WriteLine("No features found.");
            return;
        }

        var grouped = features
            .Where(f => f["properties"]?["state_name"] != null)
            .GroupBy(f => f["properties"]["state_name"].ToString());

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        foreach (var group in grouped)
        {
            string state = group.Key;

            var stateGeo = new JObject
            {
                ["type"] = "FeatureCollection",
                ["features"] = new JArray(group)
            };

            string outputPath = Path.Combine(outputDir, $"{state}.json");
            File.WriteAllText(outputPath, stateGeo.ToString(Formatting.None));

            Console.WriteLine($"Saved {group.Count()} features for {state} → {outputPath}");
        }

        Console.WriteLine("✅ Splitting completed!");
    }
}
