using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	public class RandomQuoteModule : ModuleBase<SocketCommandContext>
	{
		[Command( "randomquote" )]
		[Summary( "Grabs a random quote from the #quotebot channel" )]
		public async Task RandomQuoteAsync()
		{
			Console.WriteLine( $"RandomQuote Called for { Context.Guild.Name } in Channel { Context.Channel.Name }" );

			// Ooof, this is gonna get expensive, also limited to last 500 messages
			// Longer term, we're going to need a way to build our own cache and fill it slowly over time
			// To grab random quotes from
			// Note Discord has a command timeout of 3 seconds

			using ( PerfTimer perfTimer = new PerfTimer( "RandomQuoteAsync" ) )
			{
				var quoteBotChannel = await DiscordUtil.GetQuoteBotChannelForGuild( Context.Guild, Context.Channel.Id );
				var messages = await quoteBotChannel.GetMessagesAsync( 500 ).FlattenAsync();
				var messageList = messages.ToList();
				var random = new Random();

				var randomMessage = messageList[ random.Next( 0, messageList.Count ) ];
				var embed = randomMessage.Embeds.First() as Embed;

				await Context.Channel.SendMessageAsync( null, false, embed );
			}
		}
	}
}
