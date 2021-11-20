using System;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Management.Client;
using EasyNetQ.Scheduling;

namespace MiddleMail.Tests {
	public static class RabbitMQTestHelpers {

		public const string VHOST_NAME = "test";
		public static string Host => Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost";
		public static string Username => Environment.GetEnvironmentVariable("RabbitMQ__Username") ?? "guest";
		public static string Password => Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest";

		public static string ConnectionString => $"host={Host};username={Username};password={Password};virtualHost={VHOST_NAME};prefetchcount=10";
	}
}
