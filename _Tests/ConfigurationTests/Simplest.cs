using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigurationTests
{
	[TestClass]
	public class Simplest
	{
		IConfigurationRoot config;

		[TestInitialize]
		public void initGetConfig()
		{
			config = new ConfigurationBuilder()
				   .SetBasePath(Directory.GetCurrentDirectory())
				   .AddJsonFile("appsettings.json")
				   .Build();
		}

		[TestMethod]
		public void get_by_string_name()
		{
			config["name"].Should().Be("Ringo");
		}

		[TestMethod]
		public void get_by_compound_string_name()
		{
			config["hello:name"].Should().Be("George");
			config["overwriteMe:name"].Should().Be("virtual");
		}
	}
}
