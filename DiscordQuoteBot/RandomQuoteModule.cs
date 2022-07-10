using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	public class RandomQuoteModule : ModuleBase<SocketCommandContext>
	{
		private QuoteCache m_quoteCache;

		public RandomQuoteModule( QuoteCache quoteCache )
		{
			m_quoteCache = quoteCache;
		}

		[Command( "randomquote" )]
		[Summary( "Grabs a random quote from the #quotebot channel" )]
		public async Task RandomQuoteAsync()
		{
			Console.WriteLine( $"RandomQuote Called for { Context.Guild.Name } in Channel { Context.Channel.Name }" );
			await RandomQuoteRunAsync();
		}

		[Command( "rq" )]
		public async Task RandomQuoteAsyncShort()
		{
			Console.WriteLine( $"rq Called for { Context.Guild.Name } in Channel { Context.Channel.Name }" );
			await RandomQuoteRunAsync();
		}

		private async Task RandomQuoteRunAsync()
		{
			// Ooof, this is gonna get expensive, also limited to last 500 messages
			// Longer term, we're going to need a way to build our own cache and fill it slowly over time
			// To grab random quotes from
			// Note Discord has a command timeout of 3 seconds

			using ( PerfTimer perfTimer = new PerfTimer( "RandomQuoteAsync" ) )
			{
				var embed = await m_quoteCache.GetRandomQuoteEmbedAsync( Context );

				await Context.Channel.SendMessageAsync( null, false, embed, null, AllowedMentions.None );
			}
		}
	}
}
