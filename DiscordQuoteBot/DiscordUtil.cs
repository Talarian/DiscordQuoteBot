using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
	class DiscordUtil
	{
		public static async Task UploadEmojiToGuild( SocketGuild guild, string imagePath )
		{
			var emote = guild.Emotes.FirstOrDefault( x => x.Name == DiscordUtil.quoteBotEmojiString );
			if ( emote == null )
			{
				Image icon = new Image( imagePath );
				await guild.CreateEmoteAsync( DiscordUtil.quoteBotEmojiString, icon );
				Console.WriteLine( $"Created QuoteBot Emoji for guild { guild.Name }" );
			}
			else
			{
				Console.WriteLine( $"QuoteBot Emoji already exists for guild { guild.Name }" );
			}
		}

		public static async Task CreateChannelInGuild( SocketGuild guild, IUser botUser )
		{
			var channel = guild.Channels.FirstOrDefault( x => ( x.Name == DiscordUtil.quoteBotEmojiString ) && ( x as SocketTextChannel != null ) );
			if ( channel == null )
			{
				await guild.CreateTextChannelAsync( DiscordUtil.quoteBotEmojiString, x =>
				{
					x.Topic = "Add the :quotebot: reaction to quote something! Use !rq or !randomquote to pull a quote at random from the #quotebot channel!";
					x.PermissionOverwrites = GeneratePermissionsOverwrites( guild, botUser.Id );
				} );

				Console.WriteLine( $"Created QuoteBot Channel for guild { guild.Name }" );
			}
			else
			{
				Console.WriteLine( $"QuoteBot Channel already exists for guild { guild.Name }" );
				var permissionsOverwriteForBot = channel.GetPermissionOverwrite( botUser );
				if ( permissionsOverwriteForBot == null )
				{
					await channel.AddPermissionOverwriteAsync( botUser, GenerateBotPermissions() );
					Console.WriteLine( $"Added Permissions for QuoteBot Channel for guild { guild.Name }" );
				}
			}
		}

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

		private static List<Overwrite> GeneratePermissionsOverwrites( SocketGuild guild, ulong botUserId )
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

			OverwritePermissions botUserPermissions = GenerateBotPermissions();

			permissionOverwrites.Add( new Overwrite( guild.EveryoneRole.Id, PermissionTarget.Role, everybodyRolePermissions ) );
			permissionOverwrites.Add( new Overwrite( botUserId, PermissionTarget.User, botUserPermissions ) );
			return permissionOverwrites;
		}

		private static OverwritePermissions GenerateBotPermissions()
		{
			// Allow the bot to write
			return new OverwritePermissions(
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
		}

		public static string quoteBotEmojiString = "quotebot";
	}
}
