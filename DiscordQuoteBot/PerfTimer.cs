using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DiscordQuoteBot
{
	public class PerfTimer : IDisposable
	{
		private Stopwatch m_stopwatch;
		private string m_eventName;

		public PerfTimer( string eventName )
		{
			m_stopwatch = Stopwatch.StartNew();
			m_eventName = eventName;
		}

		public void Dispose()
		{
			m_stopwatch.Stop();
			Console.WriteLine( $"PerfTimer for { m_eventName }: { m_stopwatch.ElapsedMilliseconds } milliseconds" );
		}
	}
}
