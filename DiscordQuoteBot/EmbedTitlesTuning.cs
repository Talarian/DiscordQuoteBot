using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordQuoteBot
{
	class EmbedTitlesTuning
	{
		public static void InstatiateEmbedTitlesTuning()
		{
			if ( m_embedTitles.Count == 0 )
			{
				StreamReader reader = new StreamReader( Path.Combine( "Data", "embedtitles.txt" ) );

				do
				{
					string line = reader.ReadLine();
					if ( !string.IsNullOrEmpty( line ) )
						m_embedTitles.Add( line );
				}
				while ( !reader.EndOfStream );
				reader.Close();

				m_instantiated = true;
			}
		}

		public static string GetRandomEmbedTitle( string usernameToAdd )
		{
			if ( !m_instantiated )
				throw new Exception( "Must call InstatiateEmbedTitlesTuning before you can use GetRandomEmbedTitle!" );

			var random = new Random();
			var title = m_embedTitles[ random.Next( 0, m_embedTitles.Count ) ];
			return title.Replace( "{name}", usernameToAdd );
		}

		private static List<string> m_embedTitles = new List<string>();
		private static bool m_instantiated = false;
	}
}
