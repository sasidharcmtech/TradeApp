using System;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Formats.Asn1;

namespace TradeApp
{
    class Program
    {
        private static ILogger<Program> _logger;

        static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Set up dependency injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog();
                })
                .AddScoped<Program>()
                .BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<Program>>();

            _logger.LogInformation("Folder Monitor");
            _logger.LogInformation("Press any key to exit.");

            // Set up the file system watcher
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = @"C:\temp";
            watcher.Filter = "Trades.csv";

            // Subscribe to the Created event
            watcher.Created += OnFileCreated;

            // Start monitoring
            watcher.EnableRaisingEvents = true;

            // Wait for user input to exit
            Console.ReadKey();

            // Stop monitoring
            watcher.EnableRaisingEvents = false;
        }

        static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Read the CSV file
                using (var reader = new StreamReader(e.FullPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // Read the CSV records into a list
                    List<TradeModel> trades = csv.GetRecords<TradeModel>().ToList();

                    // Delete the old data from the Trade table and insert the data
                    using (var dbContext = new TradeDbContext())
                    {
                        dbContext.Database.ExecuteSqlRaw("DELETE FROM Trade");

                        // Save the trades to the database
                        dbContext.Trade.AddRange(trades);
                        dbContext.SaveChanges();
                    }
                }

                // Move the file to the archive folder with a timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string archivePath = Path.Combine(@"C:\temp\archive", $"Trades_{timestamp}.csv");
                File.Move(e.FullPath, archivePath);

                _logger.LogInformation("File processed and moved to the archive folder.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file");
            }
        }
    }
}
