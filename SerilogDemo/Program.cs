using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleConfigDemo
{
	public interface IBarService { void DoSomeRealWork(); }
	public class BarService : IBarService
	{
		private IFooService _fooService { get; }
		public BarService(IFooService fooService) => _fooService = fooService;
		public void DoSomeRealWork()
		{
			for (int i = 0; i < 3; i++)
				_fooService.DoThing(i + 1);
		}
	}

	public interface IFooService { void DoThing(int number); }
	public class FooService : IFooService
	{
		private readonly ILogger<FooService> _logger;
		public FooService(ILogger<FooService> logger) => _logger = logger;
		public void DoThing(int number) => _logger.LogInformation($"#{number}");
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			// full list of serilog sinks:
			// https://github.com/serilog/serilog/wiki/Provided-Sinks

			// can use instance
			var log = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.File(@"logs\my_log_.log", rollingInterval: RollingInterval.Day)
				.CreateLogger();

			// or can use global
			Log.Logger = log;

			serilogExamples();


			// allow for serilog injection
			var serviceProvider = new ServiceCollection()
				.AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
				.AddSingleton<IFooService, FooService>()
				.AddSingleton<IBarService, BarService>()
				.BuildServiceProvider();


			Log.Debug("Starting application");

			var bar = serviceProvider.GetService<IBarService>();
			bar.DoSomeRealWork();

			Log.Debug("done");
		}

		class Point { public int X { get; set; } public int Y { get; set; } }
		private static void serilogExamples()
		{
			Log.Debug("1: debug");
			Log.Error("2: error");
			Log.Warning("3: warn");
			Log.Information("4: info");

			int a = 10, b = 0;
			var p = new Point { X = 1, Y = 2 };
			Log.Debug("p.ToString(): {Point}", p);
			Log.Debug("p serialized: {@Point}", p);

			var x = (9, 10);
			Log.Information("x.ToString(): {x}", x);
			Log.Information("x serialized: {@x}", x);

			var position = new { Latitude = 25, Longitude = 134 };
			var elapsedMs = 34;
			Log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

			// paramterized logs
			try
			{
				Log.Debug("Dividing {A} by {B}", a, b);
				Console.WriteLine(a / b);
			}
			catch (DivideByZeroException ex)
			{
				Log.Error(ex, "Something went wrong");
			}
		}
	}
}
