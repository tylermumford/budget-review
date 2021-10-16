using System;

namespace BudgetReview.Parsing
{
    internal class RootParser
    {
        public DataSet<ParsedDataItem> Parse(DataSet<RawDataItem> rawData)
        {
            var result = new DataSet<ParsedDataItem>();

            foreach (var item in rawData)
            {
                ParseByType(result, item);
            }

            return result;
        }

        private void ParseByType(DataSet<ParsedDataItem> result, RawDataItem item)
        {
            ParsedDataItem itemResult;
            switch (item.Source)
            {
                case Source.MACU:
                    itemResult = SourceParser<MacuLineItem>.Parse(item, Source.MACU);
                    break;
                case Source.Amazon:
                    itemResult = SourceParser<AmazonLineItem>.Parse(item, Source.Amazon);
                    goto invert;
                case Source.CitiCard:
                    itemResult = SourceParser<CitiCardLineItem>.Parse(item, Source.CitiCard);
                    goto invert;
                case Source.ChaseCard:
                    itemResult = SourceParser<ChaseCardLineItem>.Parse(item, Source.ChaseCard);
                    goto invert;
                default:
                    throw new NotImplementedException();

                invert:
                    foreach (var line in itemResult.LineItems)
                        line.Amount = -line.Amount;
                    break;
            }

            result.Add(itemResult);
        }
    }
}
