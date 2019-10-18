using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schoeneman_Configuration
{
	// Taking advantage of configuration in .net core 2.2+ console applications
	// https://medium.com/@lschoeneman/edbcdb2e2c1b
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
			// add the config to our DI container for later use
			services.AddSingleton(config);

			services.AddTransient<ITestService, TestService>();

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
		private ITestService _testService { get; }

		public ConsoleApplication(IConfiguration configuration, ITestService testService)
		{
			_configuration = configuration;
			_testService = testService;
		}

		public void Run()
		{
			Console.WriteLine($"{nameof(ConsoleApplication)}.{nameof(Run)}");

			var location = _configuration.GetValue<string>("Location");
			Console.WriteLine($"{nameof(location)}: {location}");

			_testService.DoSomethingUseful();
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
