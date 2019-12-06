using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Schoeneman_EntityFramework
{
	// How to take advantage of EntityFramework.Core in .net Core 2.2 console applications
	// https://medium.com/@lschoeneman/d76417ded5eb
	class Program
	{
		static void Main(string[] args)
		{
			// Create service collection and configure our services
			var services = ConfigureServices();

			// Generate a provider so we can use DI
			var serviceProvider = services.BuildServiceProvider();

			// Kick off our actual code
			serviceProvider.GetService<ConsoleApplication>().Run();
		}

		private static IServiceCollection ConfigureServices()
		{
			var services = new ServiceCollection();

			// set up the objects we need to get to configuration settings
			var config = LoadConfiguration();

			// add logging. specify which config section to use. set a default logging level (but config file will override it)
			services.AddLogging(logging => {
				logging.AddConfiguration(config.GetSection("Logging"));
				logging.AddConsole();
			}).Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

			// add the config to our DI container for later use
			services.AddSingleton(config);

			services.AddTransient<ITestService, TestService>();

			// set up our database by adding the db context to our
			// services collection. Then set up our options for injection.
			// the call to AddDbContext in our ConfigureServices method will release/dispose it when its out of scope
			services.AddDbContext<SampleDbContext>(opts =>
				opts.UseSqlServer(config.GetConnectionString("Storage"))
				//// or add nuget pkg Microsoft.EntityFrameworkCore.InMemory
				//opts.UseInMemoryDatabase(databaseName: "database")
				);

			// IMPORTANT! Register our application entry point
			services.AddTransient<ConsoleApplication>();

			return services;
		}

		private static IConfiguration LoadConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			// other optional NuGet pkg.s allow for AddEnvironmentVariables, AddXmlFile

			return builder.Build();
		}
	}
	public class ConsoleApplication
	{
		private IConfiguration _configuration { get; }
		private ILogger<ConsoleApplication> _logger { get; }
		private ITestService _testService { get; }
		private SampleDbContext _sampleDbContext { get; }

		public ConsoleApplication(
			IConfiguration configuration,
			ILogger<ConsoleApplication> logger,
			ITestService testService,
			SampleDbContext dbContext)
		{
			_configuration = configuration;
			_logger = logger;
			_testService = testService;
			_sampleDbContext = dbContext;
		}

		public void Run()
		{
			Console.WriteLine($"{nameof(ConsoleApplication)}.{nameof(Run)}");

			var location = _configuration.GetValue<string>("Location");
			Console.WriteLine($"{nameof(location)}: {location}");

			_logger.LogDebug("Log this");
			// annoyingly, this logger seems to be queued/buffered.
			// if the program ends before it has time to print, we won't see the message
			System.Threading.Thread.Sleep(100);

			_testService.DoSomethingUseful();

			var result = _sampleDbContext
				.SampleEntities
				.SingleAsync(a => a.Name == "foo")
				.GetAwaiter()
				.GetResult();
		}
	}
	public class SampleEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	public class SampleDbContext : DbContext
	{
		public DbSet<SampleEntity> SampleEntities { get; set; }

		private IConfiguration _configuration { get; }
		private ILogger<SampleDbContext> _logger { get; }

		public SampleDbContext(
			IConfiguration configuration,
			ILogger<SampleDbContext> logger,
			DbContextOptions dbContextOptions)
			: base(dbContextOptions)
		{
			_configuration = configuration;
			_logger = logger;
		}
	}
	public interface ITestService
	{
		void DoSomethingUseful();
	}
	public class TestService : ITestService
	{
		public void DoSomethingUseful()
			=> Console.WriteLine($"{nameof(TestService)}.{nameof(DoSomethingUseful)}");
	}
}
