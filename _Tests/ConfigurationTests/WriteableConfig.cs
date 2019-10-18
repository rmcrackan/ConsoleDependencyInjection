using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfigurationTests
{
	public class CustomSettings
	{
		public string StringSetting { get; set; }
		public List<string> ListOfValues { get; set; }
	}

	[TestClass]
	public class WriteableConfig
	{
		string file { get; } = "appsettings.dev.json";
		IConfigurationRoot config;

		[TestInitialize]
		public void initGetConfig()
		{
			config = new ConfigurationBuilder()
				   .SetBasePath(Directory.GetCurrentDirectory())
				   .AddJsonFile(file, optional: false, reloadOnChange: true)
				   .Build();
		}

		// update-able config file. uses IOptions pattern
		// modified for console use: https://stackoverflow.com/questions/40970944/how-to-update-values-into-appsetting-json
		[TestMethod]
		public void update()
		{
			var serviceProvider = new ServiceCollection()
				.ConfigureWritable<CustomSettings>(config.GetSection("updateMe"), file)
				.AddTransient<MyTestClass>()
				.BuildServiceProvider();
			serviceProvider.GetService<MyTestClass>().Update();
		}
	}

	public class MyTestClass
	{
		private IWritableOptions<CustomSettings> _options { get; }
		private CustomSettings _value { get; }

		public MyTestClass(IWritableOptions<CustomSettings> options)
		{
			_options = options;

			// BAD. see below: "does NOT update"
			_value = _options.Value;
		}

		public void Update()
		{
			var origString = "orig value";
			var newString = "new value";

			// initial IOptions and options.Value begin the same
			_options.Value.StringSetting.Should().Be(origString);
			_options.Value.ListOfValues.Count.Should().Be(2);
			_value.StringSetting.Should().Be(origString);
			_value.ListOfValues.Count.Should().Be(2);

			// update
			_options.Update(opt => {
				opt.StringSetting = newString;
				opt.ListOfValues.Add("new val");
			});

			// doesn't update immediately
			_options.Value.StringSetting.Should().Be(origString);
			_options.Value.ListOfValues.Count.Should().Be(2);
			_value.StringSetting.Should().Be(origString);
			_value.ListOfValues.Count.Should().Be(2);
			System.Threading.Thread.Sleep(300);

			// new reference DOES update via IOptions
			_options.Value.StringSetting.Should().Be(newString);
			_options.Value.ListOfValues.Count.Should().Be(3);

			// old reference does NOT update via Value
			_value.StringSetting.Should().Be(origString);
			_value.ListOfValues.Count.Should().Be(2);

			// reset config file
			_options.Update(opt => {
				opt.StringSetting = origString;
				opt.ListOfValues.RemoveAt(opt.ListOfValues.Count - 1);
			});
			System.Threading.Thread.Sleep(300);
		}
	}

	public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
	{
		void Update(Action<T> applyChanges);
	}

	public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
	{
		private readonly IOptionsMonitor<T> _options;
		private readonly string _section;
		private readonly string _file;

		public WritableOptions(
			IOptionsMonitor<T> options,
			string section,
			string file)
		{
			_options = options;
			_section = section;
			_file = file;
		}

		public T Value => _options.CurrentValue;
		public T Get(string name) => _options.Get(name);

		public void Update(Action<T> applyChanges)
		{
			var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(_file));
			var sectionObject
				= jObject.TryGetValue(_section, out JToken section)
				? JsonConvert.DeserializeObject<T>(section.ToString())
				: (Value ?? new T());

			applyChanges(sectionObject);

			jObject[_section] = JToken.Parse(JsonConvert.SerializeObject(sectionObject));
			File.WriteAllText(_file, JsonConvert.SerializeObject(jObject, Formatting.Indented));
		}
	}

	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureWritable<T>(
			this IServiceCollection services,
			IConfigurationSection section,
			string file = "appsettings.json") where T : class, new()
		{
			services.Configure<T>(section);
			services.AddTransient<IWritableOptions<T>>(provider =>
			{
				var options = provider.GetService<IOptionsMonitor<T>>();
				return new WritableOptions<T>(options, section.Key, file);
			});

			return services;
		}
	}
}
