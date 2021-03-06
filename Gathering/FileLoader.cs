using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace BudgetReview.Gathering
{
    internal class FileLoader
    {
        private readonly DataSet<RawDataGroup> Results;
        private readonly string SearchDir;

        public FileLoader(DataSet<RawDataGroup> results, string searchDir)
        {
            Results = results;
            SearchDir = searchDir;
        }

        public void AddFile(Source source, string filenameRegex)
        {
            string[] files = Directory.GetFiles(SearchDir);

            var file = ChooseFile(files, filenameRegex, source);
            if (file == null)
                return;

            var d = new RawDataGroup
            {
                Source = source,
                ContentLines = File.ReadAllLines(file.FullName),
            };

            Results.Add(d);
        }

        private static FileInfo? ChooseFile(string[] files, string filenameRegex, Source source)
        {
            return MostRecentMatch(files, filenameRegex, source);
        }

        private static FileInfo? MostRecentMatch(string[] files, string regex, Source source)
        {
            var pattern = new Regex(regex);
            var fileList = new List<string>(files);

            var matches = fileList.Where(f => pattern.IsMatch(f));

            if (matches.Count() == 1)
            {
                Log.Debug($"{source}: One match: {matches.First()}");
                return new FileInfo(matches.First());
            }

            if (!matches.Any())
            {
                Log.Warning($"{source}: No matches");
                return null;
            }

            // When multiple matches, return most recent
            var chosenOne = matches
                .OrderByDescending(f => File.GetCreationTimeUtc(f))
                .Select(f => new FileInfo(f))
                .First();
            Log.Debug($"{source}: {matches.Count()} matches, most recent: {chosenOne.Name}");
            return chosenOne;
        }
    }
}
