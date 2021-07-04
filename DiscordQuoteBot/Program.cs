using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

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
			m_client = new DiscordSocketClient( config );
			m_client.Log += Log;

			// Linux doesn't have User environment variables, and Windows claims Process inherits the User variables, but in practice that didn't work
			var token = Environment.GetEnvironmentVariable( "DiscordQuoteBotToken", EnvironmentVariableTarget.Process );
			if ( string.IsNullOrEmpty( token ) )
			{
				token = Environment.GetEnvironmentVariable( "DiscordQuoteBotToken", EnvironmentVariableTarget.User );
			}

			await m_client.LoginAsync( TokenType.Bot, token );
			await m_client.StartAsync();

			m_client.Ready += Ready;
			m_client.ReactionAdded += ReactionAdded;
			m_client.JoinedGuild += JoinedGuild;

			// Block this task until the program is closed.
			await Task.Delay( -1 );
		}

		private Task Ready()
		{
			Console.WriteLine( $"Logged in as { m_client.CurrentUser.Username }" );
			return Task.CompletedTask;
		}

		private async Task JoinedGuild( SocketGuild guild )
		{
			try
			{
				await DiscordUtil.UploadEmojiToGuild( guild, Path.Join( "Data", "quotemark.png" ) );
			}
			catch ( Exception e )
			{
				Console.WriteLine( $"Exception adding QuoteBot Emoji for guild { guild.Name }; { e.Message }" );
				return;
			}

			try
			{
				await DiscordUtil.CreateChannelInGuild( guild, m_client.CurrentUser );
			}
			catch ( Exception e )
			{
				Console.WriteLine( $"Exception adding QuoteBot Channel for guild { guild.Name }; { e.Message }" );
				return;
			}
		}

		private async Task ReactionAdded( Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction )
		{
			if ( !DiscordUtil.ReactionIsQuoteBot( reaction ) )
			{
				return;
			}

			var message = await cachedMessage.GetOrDownloadAsync();
			if ( DiscordUtil.MessageAlreadyHasReaction( message ) )
			{
				return;
			}

			var guild = DiscordUtil.GetGuildFromChannelId( m_client, originChannel.Id );
			if ( guild == null )
			{
				return;
			}

			var botChannel = await DiscordUtil.GetQuoteBotChannelForGuild( guild, originChannel.Id );
			if ( botChannel == null )
			{
				return;
			}

			EmbedBuilder builder = GenerateEmbed( originChannel, message, guild );

			try
			{
				await botChannel.SendMessageAsync( null, false, builder.Build() );
			}
			catch ( Exception e )
			{
				Console.WriteLine( $"Exception sending Quote for guild { guild.Name }; { e.Message }" );
				return;
			}
		}

		private static EmbedBuilder GenerateEmbed( ISocketMessageChannel originChannel, IUserMessage message, SocketGuild guild )
		{
			string link = $"https://discord.com/channels/{ guild.Id }/{ originChannel.Id }/{ message.Id }";

			var author = new EmbedAuthorBuilder()
				.WithName( message.Author.Username )
				.WithIconUrl( message.Author.GetAvatarUrl() );
			EmbedBuilder builder = new EmbedBuilder
			{
				Url = link,
				Title = EmbedTitlesTuning.GetRandomEmbedTitle( message.Author.Username ),
				Author = author,
				Description = message.Content
			};
			return builder;
		}

		private Task Log( LogMessage msg )
		{
			Console.WriteLine( msg.ToString() );
			return Task.CompletedTask;
		}

		private DiscordSocketClient m_client;
	}
}
