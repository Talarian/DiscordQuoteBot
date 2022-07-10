using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	class DiscordEventHandler
	{
		private readonly DiscordSocketClient m_client;

		public DiscordEventHandler( DiscordSocketClient client )
		{
			m_client = client;
		}

		public Task InitializeAsync()
		{
			m_client.Ready += Ready;
			m_client.ReactionAdded += ReactionAdded;
			m_client.JoinedGuild += JoinedGuild;

			return Task.CompletedTask;
		}

		private Task Ready()
		{
			Console.WriteLine( $"Logged in as { m_client.CurrentUser.Username }" );
			return Task.CompletedTask;
		}

		private async Task ReactionAdded( Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> originChannel, SocketReaction reaction )
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

			EmbedBuilder builder = GenerateEmbed( originChannel.Id, message, guild );

			try
			{
				await botChannel.SendMessageAsync( null, false, builder.Build(), null, AllowedMentions.None );
			}
			catch ( Exception e )
			{
				Console.WriteLine( $"Exception sending Quote for guild { guild.Name }; { e.Message }" );
				return;
			}
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

		private static EmbedBuilder GenerateEmbed( ulong originChannelId, IUserMessage message, SocketGuild guild )
		{
			string link = $"https://discord.com/channels/{ guild.Id }/{ originChannelId }/{ message.Id }";

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
	}
}
