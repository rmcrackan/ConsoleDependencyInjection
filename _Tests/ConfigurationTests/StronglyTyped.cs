using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigurationTests
{
	public class Hello
	{
		public string Name { get; set; }
	}

	public enum MyEnum { None = 0, Lots = 1 }
	public class InnerClass
	{
		public string Name { get; set; }
		public bool IsEnabled { get; set; } = true;
	}
	public class MySettings
	{
		public string StringSetting { get; set; }
		public int IntSetting { get; set; }
		public Dictionary<string, InnerClass> Dict { get; set; }
		public List<string> ListOfValues { get; set; }
		public MyEnum AnEnum { get; set; }
		public string Unassigned { get; set; }
	}

	[TestClass]
	public class StronglyTyped
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
		public void get_new_object()
		{
			var appConfig = config
				.GetSection("hello")
				.Get<Hello>();
			appConfig.Name.Should().Be("George");
		}

		[TestMethod]
		public void get_complex_object()
		{
			var myConfig = config
				.GetSection("thereAreMySettings")
				.Get<MySettings>();
			myConfig.StringSetting.Should().Be("My StringSetting Value");
			myConfig.IntSetting.Should().Be(23);

			myConfig.Dict.Count.Should().Be(2);
			myConfig.Dict["FirstKey"].Name.Should().Be("First Class");
			myConfig.Dict["FirstKey"].IsEnabled.Should().BeFalse();
			myConfig.Dict["SecondKey"].Name.Should().Be("Second Class");
			myConfig.Dict["SecondKey"].IsEnabled.Should().BeTrue();

			myConfig.ListOfValues.Count.Should().Be(2);
			myConfig.ListOfValues[0].Should().Be("Value1");
			myConfig.ListOfValues[1].Should().Be("Value2");

			myConfig.AnEnum.Should().Be(MyEnum.Lots);

			myConfig.Unassigned.Should().BeNull();
		}

		[TestMethod]
		public void hydrate_without_section()
		{
			var withName = new Hello();
			config.Bind(withName);
			withName.Name.Should().Be("Ringo");
		}

		[TestMethod]
		public void hydrate_with_section()
		{
			{
				var hello = new Hello();
				config.GetSection("hello").Bind(hello);
				hello.Name.Should().Be("George");
			}

			{
				var hello = new Hello();
				config.Bind("hello", hello);
				hello.Name.Should().Be("George");
			}
		}
	}
}
