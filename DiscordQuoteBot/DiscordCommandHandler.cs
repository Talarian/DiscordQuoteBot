using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	public class DiscordCommandHandler
	{
		private readonly IServiceProvider m_services;
		private readonly CommandService m_commands;
		private readonly DiscordSocketClient m_client;

		public DiscordCommandHandler( IServiceProvider services, CommandService commands, DiscordSocketClient client )
		{
			m_services = services;
			m_commands = commands;
			m_client = client;
		}

		public async Task InitializeAsync()
		{
			await m_commands.AddModulesAsync(
				assembly: Assembly.GetEntryAssembly(),
				services: m_services );
			m_client.MessageReceived += HandleCommandAsync;
		}

		public async Task HandleCommandAsync( SocketMessage msg )
		{
			// Don't process the command if it was a system message
			var message = msg as SocketUserMessage;
			if ( message == null ) return;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;

			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if ( !( message.HasCharPrefix( '!', ref argPos ) ||
				message.HasMentionPrefix( m_client.CurrentUser, ref argPos ) ) ||
				message.Author.IsBot )
				return;

			var context = new SocketCommandContext( m_client, message );

			await m_commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: m_services );
		}
	}
}
