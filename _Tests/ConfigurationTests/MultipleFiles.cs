using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigurationTests
{
	[TestClass]
	public class MultipleFiles
	{
		IConfigurationRoot config;

		[TestInitialize]
		public void initGetConfig()
		{
			var env = "dev";
			config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{env}.json")
				.Build();
		}

		[TestMethod]
		public void from_file_1()
		{
			var appConfig = config
				.GetSection("hello")
				.Get<Hello>();
			appConfig.Name.Should().Be("George");
		}

		[TestMethod]
		public void from_file_2()
		{
			var appConfig = config
				.GetSection("world")
				.Get<Hello>();
			appConfig.Name.Should().Be("Paul");
		}

		[TestMethod]
		public void overridden()
		{
			var overwriteConfig = config
				.GetSection("overwriteMe")
				.Get<Hello>();
			overwriteConfig.Name.Should().Be("overwrite");
		}
	}
}
