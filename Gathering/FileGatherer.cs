using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace BudgetReview.Gathering
{
    internal class FileGatherer
    {
        private readonly DataSet<RawDataItem> Results;
        private readonly string SearchDir;

        public FileGatherer(DataSet<RawDataItem> results, string searchDir)
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

            var d = new RawDataItem
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
                Debug.WriteLine($"{source}: One match: {matches.First()}");
                return new FileInfo(matches.First());
            }

            if (!matches.Any())
            {
                Debug.WriteLine($"{source}: No matches");
                return null;
            }

            // When multiple matches, return most recent
            var chosenOne = matches
                .OrderByDescending(f => File.GetCreationTimeUtc(f))
                .Select(f => new FileInfo(f))
                .First();
            Debug.WriteLine($"{source}: {matches.Count()} matches, most recent: {chosenOne.Name}");
            return chosenOne;
        }
    }
}
