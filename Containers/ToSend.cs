using System;
using GetYTMData.FromYoutube;

namespace GetYTMData.ToSend
{
	public class track_metadata
	{
		public string track_name {get;set;}
		public string artist_name {get;set;}
		
		public track_metadata()
		{
			track_name = "";
			artist_name = "";
		}
	}

	public class ToListenBrainz
	{
		public track_metadata track_metadata{get;set;}
		public long listened_at {get;set;}
		public ToListenBrainz(FromYTJson temp)
		{
			track_metadata = new();
			track_metadata.track_name = temp.title;
			DateTime _time;
			DateTime.TryParse(temp.time, out _time);
			listened_at = ((DateTimeOffset)_time).ToUnixTimeSeconds();
			if(temp.subtitles != null)
			{
				try
				{
					track_metadata.artist_name = temp.subtitles[0].name;
					if(track_metadata.artist_name.Contains("- Topic"))
					{
						int numChar = track_metadata.artist_name.Length;
						track_metadata.artist_name = track_metadata.artist_name.Remove(numChar - 8, 8);
					}
				}
				catch(Exception)
				{
					Console.WriteLine(track_metadata.track_name);
					throw;
				}
			}
			if(track_metadata.track_name.Contains("Watched "))
			{
				track_metadata.track_name = track_metadata.track_name.Remove(0,8);
			}
			if(track_metadata.artist_name == null)
			{
				track_metadata.artist_name = "Unknown";
			}
		}
	}
}
