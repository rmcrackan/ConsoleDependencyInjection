using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;

namespace StructureMapTests
{
	// https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/
	[TestClass]
	public class StructureMapExamples
	{
		public static List<string> log { get; } = new List<string>();

		[TestMethod]
		public void auto_config_dependency_injection()
		{
			var services = new ServiceCollection()
				//// can also manually chain dependencies here as normal if needed
				//.AddSingleton<IMyLog, MyLog>()
				;

			var container = new Container();
			container.Configure(config =>
			{
				config.Scan(_ =>
				{
					_.AssemblyContainingType(typeof(StructureMapExamples));
					_.WithDefaultConventions();
				});
				config.Populate(services);
			});
			var serviceProvider = container.GetInstance<IServiceProvider>();

			var bar = serviceProvider.GetService<IBarService>();
			bar.DoSomeRealWork();

			log.Count.Should().Be(3);
			log[0].Should().Be("#1");
			log[1].Should().Be("#2");
			log[2].Should().Be("#3");
		}
	}

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
		private readonly IMyLog _log;
		public FooService(IMyLog log) => _log = log;
		public void DoThing(int number) => _log.Print($"#{number}");
	}

	public interface IMyLog { void Print(string str); }
	public class MyLog : IMyLog
	{
		public void Print(string str) => StructureMapExamples.log.Add(str);
	}
}
