using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordQuoteBot
{
	class DependencyInjector
	{
		private readonly CommandService m_commands;
		private readonly DiscordSocketClient m_client;

		public DependencyInjector( CommandService commands, DiscordSocketClient client )
		{
			m_commands = commands;
			m_client = client;
		}

		public IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.AddSingleton( m_client )
			.AddSingleton( m_commands )
			.BuildServiceProvider();
	}
}
