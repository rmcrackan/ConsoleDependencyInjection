using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigurationTests
{
	// https://weblog.west-wind.com/posts/2016/may/23/strongly-typed-configuration-settings-in-aspnet-core
	// Strongly Typed Configuration Settings in ASP.NET Core
	[TestClass]
	public class IOptions
	{
		IConfiguration config;

		[TestInitialize]
		public void initGetConfig()
		{
			config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();
		}

		[TestMethod]
		public void inject_IOptions()
		{
			var serviceProvider = new ServiceCollection()
				.Configure<IOptionsExample>(config.GetSection("ioptionsExample"))
				.AddTransient<InjectIOptions>()
				.BuildServiceProvider();
			serviceProvider.GetService<InjectIOptions>().Run();
		}
		class InjectIOptions
		{
			private IOptionsExample _ioptionsExample { get; }
			public InjectIOptions(IOptions<IOptionsExample> settings)
				=> _ioptionsExample = settings.Value;
			public void Run()
			{
				_ioptionsExample.MaxItemsPerList.Should().Be(10);
				_ioptionsExample.ApplicationName.Should().Be("app name");
			}
		}

		[TestMethod]
		public void inject_IConfiguration()
		{
			var serviceProvider = new ServiceCollection()
				.AddSingleton(config)
				.AddTransient<InjectIConfiguration>()
				.BuildServiceProvider();
			serviceProvider.GetService<InjectIConfiguration>().Run();
		}
		class InjectIConfiguration
		{
			private IConfiguration _config { get; }
			public InjectIConfiguration(IConfiguration rawConfig)
				=> _config = rawConfig;
			public void Run()
			{
				_config.GetValue<string>("ioptionsExample:ApplicationName").Should().Be("app name");
				_config["ioptionsExample:ApplicationName"].Should().Be("app name");
				_config["name"].Should().Be("Ringo");
			}
		}
	}

	public class IOptionsExample
	{
		public string ApplicationName { get; set; } = "hardcoded default";
		public int MaxItemsPerList { get; set; } = -1;
	}
}
