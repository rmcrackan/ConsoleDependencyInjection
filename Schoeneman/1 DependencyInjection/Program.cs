using System;
using Microsoft.Extensions.DependencyInjection;

namespace Schoeneman_DependencyInjection
{
	// How to take advantage of Dependency Injection in .Net Core 2.2 + Console Applications
	// https://medium.com/swlh/274e50a6c350
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
			var services = new ServiceCollection()
				.AddTransient<ITestService, TestService>()
				
				// IMPORTANT! Register our application entry point
				.AddTransient<ConsoleApplication>();

			return services;
		}
	}
	public class ConsoleApplication
	{
		private ITestService _testService { get; }
		public ConsoleApplication(ITestService testService) => _testService = testService;
		public void Run()
		{
			Console.WriteLine($"{nameof(ConsoleApplication)}.{nameof(Run)}");
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
