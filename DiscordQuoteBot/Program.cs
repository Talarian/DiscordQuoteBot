using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	class Program
	{
		static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			m_client = new DiscordSocketClient();
			m_client.Log += Log;

			var token = Environment.GetEnvironmentVariable("DiscordQuoteBotToken", EnvironmentVariableTarget.User);
			await m_client.LoginAsync(TokenType.Bot, token);
			await m_client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private DiscordSocketClient m_client;
	}
}
