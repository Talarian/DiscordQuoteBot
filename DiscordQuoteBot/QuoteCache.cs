using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	// TODO: This only works while running on a single server; extend to handle multiple GuildIds
	public class QuoteCache
	{
		private ulong messageIdOldest = ulong.MaxValue;
		private ulong messageIdNewest = 0;
		private bool noMoreOldMessages = false;

		private List<IMessage> messages = new List<IMessage>();

		public async Task<Embed> GetRandomQuoteEmbedAsync( SocketCommandContext context )
		{
			await UpdateCache( context );

			var random = new Random();
			var randomMessage = messages[ random.Next( 0, messages.Count ) ];
			return randomMessage.Embeds.First() as Embed;
		}

		private async Task UpdateCache( SocketCommandContext context )
		{
			var quoteBotChannel = await DiscordUtil.GetQuoteBotChannelForGuild( context.Guild, context.Channel.Id );

			if ( messages.Count == 0 )
			{
				var syncMessages = await quoteBotChannel.GetMessagesAsync().FlattenAsync();
				messages = syncMessages.ToList();

				if ( messages.Count > 0 )
				{
					messageIdNewest = messages.First().Id;
					messageIdOldest = messages.Last().Id;
				}
			}
			else
			{
				var syncNewMessages = await quoteBotChannel.GetMessagesAsync( messageIdNewest, Direction.After ).FlattenAsync();
				if ( syncNewMessages.Count() > 0 )
				{
					messages.AddRange( syncNewMessages );
					messageIdNewest = syncNewMessages.First().Id;
				}

				if ( !noMoreOldMessages )
				{
					var syncOldMessages = await quoteBotChannel.GetMessagesAsync( messageIdOldest, Direction.Before ).FlattenAsync();
					if ( syncOldMessages.Count() > 0 )
					{
						messages.AddRange( syncOldMessages );
						messageIdOldest = syncOldMessages.Last().Id;
					}
					else
					{
						noMoreOldMessages = true;
					}
				}
			}
		}
	}
}
