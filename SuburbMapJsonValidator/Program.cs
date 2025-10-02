using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main(string[] args)
    {
        string jsonPath = "suburbsJsonFile.json";   
        string csvPath = "investmentsCsvFile.csv";         
        string outputPath = "modifiedSuburbs.json";

        // --- Step 1: Read JSON ---
        var jsonText = File.ReadAllText(jsonPath);
        var geoJson = JObject.Parse(jsonText);

        // --- Step 2: Read CSV ---
        var dealStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        }))
        {
            // Try reading header
            if (csv.Read())
            {
                bool hasHeader = false;

                try
                {
                    csv.ReadHeader();
                    hasHeader = true;
                }
                catch
                {
                    // No header present → fallback
                    hasHeader = false;
                }

                do
                {
                    string dealState;
                    if (hasHeader)
                        dealState = csv.GetField("DealState"); // by name
                    else
                        dealState = csv.GetField(0);           // first column

                    if (!string.IsNullOrWhiteSpace(dealState))
                        dealStates.Add(dealState.Trim());
                }
                while (csv.Read());
            }
        }

        // --- Step 3: State mapping (short form) ---
        var stateMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "New South Wales", "NSW" },
            { "Victoria", "VIC" },
            { "Queensland", "QLD" },
            { "South Australia", "SA" },
            { "Western Australia", "WA" },
            { "Tasmania", "TAS" },
            { "Northern Territory", "NT" },
            { "Australian Capital Territory", "ACT" }
        };

        // --- Step 4: Modify JSON ---
        foreach (var feature in geoJson["features"]!)
        {
            var properties = feature["properties"] as JObject;
            if (properties == null) continue;

            var steName = properties["ste_name"]?[0]?.ToString();
            string suburb = null;
            if (properties["scc_name"] != null)
            {
                var suburbValues = properties["scc_name"].ToObject<List<string>>();

                if (suburbValues != null && suburbValues.Count > 0)
                {
                    // Take first value, strip anything in parentheses e.g. "Armstrong Creek (Vic.)" → "Armstrong Creek"
                    suburb = System.Text.RegularExpressions.Regex.Replace(suburbValues[0], @"\s*\(.*?\)", "").Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(steName) && stateMap.TryGetValue(steName, out var shortState))
            {
                // add short state name
                properties["state_name"] = shortState;
                properties["suburb"] = suburb;

                //// only add complete_suburb if this state is in CSV list
                //if (dealStates.Contains(shortState) && !string.IsNullOrWhiteSpace(suburb))
                //{
                    properties["complete_suburb"] = $"{suburb} - {shortState}";
                //}
            }
        }

        // Write output ---
        File.WriteAllText(outputPath, geoJson.ToString());
        Console.WriteLine($"Modified GeoJSON saved to {outputPath}");



        string inputFile = "modifiedSuburbs.json";
        string outputDir = "States";

        SplitByState.Run(inputFile, outputDir);
        Console.WriteLine($"Split Files saved to {outputPath}");
    }
}
