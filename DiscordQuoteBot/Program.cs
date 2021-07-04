using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DiscordQuoteBot
{
	class Program
	{
		static void Main( string[] args )
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			var config = new DiscordSocketConfig { MessageCacheSize = 100 };
			m_client = new DiscordSocketClient( config );
			m_client.Log += Log;

			var token = Environment.GetEnvironmentVariable( "DiscordQuoteBotToken", EnvironmentVariableTarget.User );
			await m_client.LoginAsync( TokenType.Bot, token );
			await m_client.StartAsync();

			m_client.ReactionAdded += ReactionAdded;
			m_client.JoinedGuild += JoinedGuild;
			foreach ( var guild in m_client.Guilds )
			{
				var quoteBotChannel = guild.Channels.First( x => x.Name == "quotebot" );
			}

			// Block this task until the program is closed.
			await Task.Delay( -1 );
		}

		private async Task JoinedGuild( SocketGuild guild )
		{
			// If the emote we use doesn't exist, make it
			var emote = guild.Emotes.FirstOrDefault( x => x.Name == DiscordUtil.quoteBotEmojiString );
			if ( emote == null )
			{
				Image icon = new Image( "Data\\quotemark.png" );
				await guild.CreateEmoteAsync( DiscordUtil.quoteBotEmojiString, icon );
				Console.WriteLine( $"Created QuoteBot Emoji for guild { guild.Name }" );
			}
			else
			{
				Console.WriteLine( $"QuoteBot Emoji already exists for guild { guild.Name }" );
			}

			// If the channel we use doesn't exist, make it
			var channel = guild.Channels.FirstOrDefault( x => ( x.Name == DiscordUtil.quoteBotEmojiString ) && ( x as SocketTextChannel != null ) );
			if ( channel == null )
			{
				await guild.CreateTextChannelAsync( DiscordUtil.quoteBotEmojiString, x =>
				{
					List<Overwrite> permissionOverwrites = new List<Overwrite>( 2 );

					// Set Everyone to read only
					var everybodyRolePermissions = new OverwritePermissions(
						PermValue.Inherit, // CreateInstantInvite
						PermValue.Inherit, // ManageChannel
						PermValue.Inherit, // addReactions
						PermValue.Inherit, // viewChannel
						PermValue.Deny, // sendMessages
						PermValue.Inherit, // sendTTSMessages
						PermValue.Inherit, // manageMessages
						PermValue.Deny, // embedLinks
						PermValue.Deny, // attachFiles
						PermValue.Inherit, // readMessageHistory
						PermValue.Deny, // mentionEveryone
						PermValue.Inherit, // useExternalEmojis
						PermValue.Inherit, // connect
						PermValue.Inherit, // speak
						PermValue.Inherit, // muteMembers
						PermValue.Inherit, // deafenMembers
						PermValue.Inherit, // moveMembers
						PermValue.Inherit, // useVoiceActivation
						PermValue.Inherit, // manageRoles
						PermValue.Inherit, // manageWebhooks
						PermValue.Inherit, // prioritySpeaker
						PermValue.Inherit  // stream
						);

					// Allow the bot to write
					var botUserPermissions = new OverwritePermissions(
						PermValue.Inherit, // CreateInstantInvite
						PermValue.Inherit, // ManageChannel
						PermValue.Inherit, // addReactions
						PermValue.Inherit, // viewChannel
						PermValue.Allow, // sendMessages
						PermValue.Inherit, // sendTTSMessages
						PermValue.Inherit, // manageMessages
						PermValue.Allow, // embedLinks
						PermValue.Deny, // attachFiles
						PermValue.Inherit, // readMessageHistory
						PermValue.Deny, // mentionEveryone
						PermValue.Inherit, // useExternalEmojis
						PermValue.Inherit, // connect
						PermValue.Inherit, // speak
						PermValue.Inherit, // muteMembers
						PermValue.Inherit, // deafenMembers
						PermValue.Inherit, // moveMembers
						PermValue.Inherit, // useVoiceActivation
						PermValue.Inherit, // manageRoles
						PermValue.Inherit, // manageWebhooks
						PermValue.Inherit, // prioritySpeaker
						PermValue.Inherit  // stream
						);


					permissionOverwrites.Add( new Overwrite( guild.EveryoneRole.Id, PermissionTarget.Role, everybodyRolePermissions ) );
					permissionOverwrites.Add( new Overwrite( m_client.CurrentUser.Id, PermissionTarget.User, botUserPermissions ) );

					x.PermissionOverwrites = permissionOverwrites;
				} );

				Console.WriteLine( $"Created QuoteBot Channel for guild { guild.Name }" );
			}
			else
			{
				Console.WriteLine( $"QuoteBot Channel already exists for guild { guild.Name }" );
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
				// Don't add a quote if folks keep adding their own values
				return;
			}

			var guild = DiscordUtil.GetGuildFromChannelId( m_client, originChannel.Id );
			if ( guild == null )
			{
				Console.WriteLine( $" No guild found for Channel Id { originChannel.Id }!" );
				return;
			}

			var botChannel = await DiscordUtil.GetQuoteBotChannelForGuild( guild, originChannel.Id );
			if ( botChannel == null )
			{
				return;
			}

			string link = $"https://discord.com/channels/{ guild.Id }/{ originChannel.Id }/{ message.Id }";

			var author = new EmbedAuthorBuilder()
				.WithName( message.Author.Username )
				.WithIconUrl( message.Author.GetAvatarUrl() );
			EmbedBuilder builder = new EmbedBuilder
			{
				Url = link,
				Title = message.Content,
				Author = author
			};
			await botChannel.SendMessageAsync( null, false, builder.Build() );
		}

		private Task Log( LogMessage msg )
		{
			Console.WriteLine( msg.ToString() );
			return Task.CompletedTask;
		}

		private DiscordSocketClient m_client;
	}
}
