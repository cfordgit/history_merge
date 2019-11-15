using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace HistoryMerge
{
    class Program
    {
        public static IServiceProvider _serviceProvider;
        public static IConfiguration Configuration;

        static void Main(string[] args)
        {
            Configure();
            RegisterServices();

            DataInitializer dataInitializer = new DataInitializer(_serviceProvider.GetService<IConfiguration>(), _serviceProvider.GetService<ILogger>());
            dataInitializer.Init();

            Console.WriteLine("Merge records? Y/N");
            if (Console.ReadLine().ToUpper() == "Y")
            {
                DataMerger dataMerger = new DataMerger(_serviceProvider.GetService<IConfiguration>(), _serviceProvider.GetService<ILogger>());
                dataMerger.MergeData();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }

            DisposeServices();
        }

        private static void Configure()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            collection.AddScoped<ILogger, Logger>();
            collection.AddSingleton<IConfiguration>(Configuration);
            _serviceProvider = collection.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
