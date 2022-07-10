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
		private readonly QuoteCache m_quoteCache;

		public DependencyInjector( CommandService commands, DiscordSocketClient client, QuoteCache quoteCache )
		{
			m_commands = commands;
			m_client = client;
			m_quoteCache = quoteCache;
		}

		public IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.AddSingleton( m_client )
			.AddSingleton( m_commands )
			.AddSingleton( m_quoteCache )
			.BuildServiceProvider();
	}
}
