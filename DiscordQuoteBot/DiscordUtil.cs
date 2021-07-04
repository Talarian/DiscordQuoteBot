using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	class DiscordUtil
	{
		public static SocketGuild GetGuildFromChannelId( DiscordSocketClient client, ulong snowflakeIdOfChannelReceived )
		{
			foreach ( var guild in client.Guilds )
			{
				if ( guild.Channels.Any( x => x.Id == snowflakeIdOfChannelReceived ) )
				{
					return guild;
				}
			}

			Console.WriteLine( $" No guild found for Channel Id { snowflakeIdOfChannelReceived }!" );
			return null;
		}

		public static async Task SendErrorToGuild( ulong snowflakeIdOfChannelReceived, SocketGuild guildReceived, string errorMessage )
		{
			try
			{
				var receivedChannel = guildReceived.Channels.FirstOrDefault( x => x.Id == snowflakeIdOfChannelReceived ) as SocketTextChannel;
				if ( receivedChannel != null )
				{
					await receivedChannel.SendMessageAsync( errorMessage );
				}
			}
			catch ( Exception e )
			{
				Console.WriteLine( $"Exception sending Error for guild { guildReceived.Name }; { e.Message }" );
			}
			finally
			{
				Console.WriteLine( $" { errorMessage } on server { guildReceived.Name }!" );
			}
		}

		public static async Task<SocketTextChannel> GetQuoteBotChannelForGuild( SocketGuild guild, ulong snowflakeIdOfChannelReceived )
		{
			// Once we have the guild, let's get the QuoteBot channel
			var channel = guild.Channels.FirstOrDefault( x => x.Name == "quotebot" );
			if ( channel == null )
			{
				await SendErrorToGuild( snowflakeIdOfChannelReceived, guild, "No \"quotebot\" channel set up on server" );
				return null;
			}

			var textChannel = channel as SocketTextChannel;
			if ( textChannel == null )
			{
				await SendErrorToGuild( snowflakeIdOfChannelReceived, guild, $"\"quotebot\" channel is not a text channel" );
				return null;
			}

			return textChannel;
		}

		public static bool ReactionIsQuoteBot( SocketReaction reaction )
		{
			return reaction.Emote.Name.Contains( quoteBotEmojiString );
		}

		public static bool MessageAlreadyHasReaction( IUserMessage message )
		{
			var reaction = message.Reactions.FirstOrDefault( x => x.Key.Name.Contains( quoteBotEmojiString ) );
			return reaction.Key != null && reaction.Value.ReactionCount > 1;
		}

		public static string quoteBotEmojiString = "quotebot";
	}
}
