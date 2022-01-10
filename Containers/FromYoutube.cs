using System;
using System.Collections.Generic;

namespace GetYTMData.FromYoutube
{
	public class subtitles
	{
		public string name {get;set;}
		public string url{get;set;}
		public subtitles()
		{
			name = "";
			url = "";
		}
	}

	public class FromYTJson
	{
		public string header {get;set;}
		public string title {get;set;}
		public List<subtitles> subtitles {get;set;}
		public string time {get;set;}
		public FromYTJson()
		{
			header = "";
			title = "";
			time = "";
			subtitles = new();
		}
	}
}
