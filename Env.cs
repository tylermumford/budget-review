using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BudgetReview
{
    internal static class Env
    {
        private static Dictionary<string, string> Values = new();

        public static string Get(string key, string orDefault = "")
        {
            return Values.GetValueOrDefault(key.ToLowerInvariant(), orDefault);
        }

        public static string GetOrThrow(string key)
        {
            return Values[key.ToLowerInvariant()];
        }

        #region Loading
        public static void Load()
        {
            var fileContents = LoadRawFile(".env");
            PopulateDictionary(fileContents);

            Debug.WriteLine("Env keys loaded: " + string.Join(", ", Values.Keys));
        }

        private static string[] LoadRawFile(string name)
        {
            try
            {
                return File.ReadAllLines(name);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"Env file {name} not found.");
                return new string[0];
            }
        }

        private static void PopulateDictionary(string[] lines)
        {
            var linesReady = lines
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .Where(l => l[0] != '#');

            var pattern = new Regex(@"([\w_]+)='?([^#\n]+)'?");

            foreach (var line in linesReady)
            {
                var m = pattern.Match(line);
                if (!m.Success)
                    continue;
                var varName = m.Groups[1].Value;
                var varValue = m.Groups[2].Value;

                Values[varName.ToLowerInvariant()] = varValue.Trim();
            }
        }
        #endregion
    }
}
