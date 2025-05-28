using System;
using System.Collections.Generic;
using System.Text;
using FidelityOptionsScraper.Models;

namespace FidelityOptionsScraper.Utils
{
    public static class CsvGenerator
    {
        /// <summary>
        /// Generates a CSV file from a list of option results
        /// </summary>
        /// <param name="results">List of option results</param>
        /// <param name="outputPath">Path to save the CSV file</param>
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
            
            Console.WriteLine($"CSV file saved to: {Path.GetFullPath(outputPath)}");
        }
    }
}
