using OptionsPriceFinder.Models;
using System.Text;

namespace OptionsPriceFinder.Utils
{
    public static class CsvGenerator
    {
        public static void GenerateCsv(List<OptionResult> results, string outputPath)
        {
            StringBuilder csv = new StringBuilder();
            
            // Add header
            csv.AppendLine("Symbol,CurrentPrice,FridayDate,CallOption1Percent,CallOption2Percent,CallOption3Percent");
            
            // Add data rows
            foreach (var result in results)
            {
                csv.AppendLine($"{result.Symbol},{result.CurrentPrice},{result.FridayDate}," +
                               $"{result.CallOption1Percent},{result.CallOption2Percent},{result.CallOption3Percent}");
            }
            
            // Write to file
            File.WriteAllText(outputPath, csv.ToString());
        }
    }
}
