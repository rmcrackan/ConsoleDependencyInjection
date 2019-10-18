using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigurationTests
{
	public class Foo
	{
		public string Bar { get; set; }
	}

	[TestClass]
	public class ReloadFileChange
	{
		string file { get; } = "appsettings.json";
		IConfigurationRoot config;

		[TestInitialize]
		public void initGetConfig()
		{
			config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(file, optional: false, reloadOnChange: true)
				.Build();
		}

		[TestMethod]
		public void reload_on_change()
		{
			var oldValue = "old value";
			var newValue = "new value";
			var json1 = $"\"bar\": \"{oldValue}\"";
			var json2 = $"\"bar\": \"{newValue}\"";

			Foo reload() => config.GetSection("foobar").Get<Foo>();

			// original
			var myConfig = reload();
			myConfig.Bar.Should().Be(oldValue);

			// change file
			File.WriteAllText(file, File.ReadAllText(file).Replace(json1, json2));
			// reload is NOT instantanious. seems to take 280+ ms
			System.Threading.Thread.Sleep(300);

			// in-place setting not changed. must re-load
			myConfig.Bar.Should().Be(oldValue);

			myConfig = reload();
			myConfig.Bar.Should().Be(newValue);

			// revert file
			File.WriteAllText(file, File.ReadAllText(file).Replace(json2, json1));
			System.Threading.Thread.Sleep(300);

			// verify reverted
			myConfig = reload();
			myConfig.Bar.Should().Be(oldValue);
		}
	}
}
