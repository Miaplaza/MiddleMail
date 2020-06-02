using System;

namespace MiddleMail.Exceptions {
	public class ConfigurationMissingException : Exception {

		public ConfigurationMissingException(string configurationKey) : base($"Configuration missing for configuration key {configurationKey}.") { }
	}
}
