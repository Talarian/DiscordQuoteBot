using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Discord.Commands;

namespace DiscordQuoteBot
{
	class Program
	{
		static void Main( string[] args )
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			EmbedTitlesTuning.InstatiateEmbedTitlesTuning();

			var config = new DiscordSocketConfig { MessageCacheSize = 1000 };
			var client = new DiscordSocketClient( config );
			client.Log += Log;

			// Linux doesn't have User environment variables, and Windows claims Process inherits the User variables, but in practice that didn't work
			var token = Environment.GetEnvironmentVariable( "DiscordQuoteBotToken", EnvironmentVariableTarget.Process );
			if ( string.IsNullOrEmpty( token ) )
			{
				token = Environment.GetEnvironmentVariable( "DiscordQuoteBotToken", EnvironmentVariableTarget.User );
			}

			CommandService commandService = new CommandService();
			DependencyInjector dependencyInjector = new DependencyInjector( commandService, client );
			var serviceProvider = dependencyInjector.BuildServiceProvider();

			m_commandHandler = new DiscordCommandHandler( serviceProvider, commandService, client );
			await m_commandHandler.InitializeAsync();

			m_eventHandler = new DiscordEventHandler( client );
			await m_eventHandler.InitializeAsync();

			await client.LoginAsync( TokenType.Bot, token );
			await client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay( -1 );
		}

		private Task Log( LogMessage msg )
		{
			Console.WriteLine( msg.ToString() );
			return Task.CompletedTask;
		}

		private DiscordCommandHandler m_commandHandler = null;
		private DiscordEventHandler m_eventHandler = null;
	}
}
