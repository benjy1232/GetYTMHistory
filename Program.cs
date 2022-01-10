using System;
using System.Collections.Generic;
using GetYTMData.FromYoutube;
using GetYTMData.ToSend;
using Newtonsoft.Json;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.IO;

namespace GetYTMData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter User ID Token:");
            string token = Console.ReadLine();
            ReadMusicHistory readHist = new(token);
            if (!readHist.CheckUploaded())
            {
                Console.WriteLine("music-history.json does not exist");
                return;
            }
            bool uploadedExists = readHist.CheckUploaded();
            readHist.SeparateSource(uploadedExists);
        }
    }

    class ReadMusicHistory
    {
        private HttpClient client;
        private string _musicHistoryJson;
        private List<FromYTJson> _totalHistory;
        private Dictionary<string, string> _artist;
        private string ROOT;

        // public List<FromYTJson> MusicHistory {get;set;}

        public bool CheckUploaded()
        {
            if (!File.Exists("music-uploads-metadata.csv"))
            {
                return false;
            }
            TextFieldParser parser = new("music-uploads-metadata.csv");
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.ReadFields();

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();

                if (fields != null)
                {
                    string SongTitle = fields[0].Trim();
                    string SongArtist = fields[2].Trim();

                    if (!_artist.TryAdd(SongTitle, SongArtist))
                    {
                        _artist.Remove(SongArtist);
                    }
                }
            }
            return true;
        }

        public bool DoesFileExist()
        {
            _totalHistory = JsonConvert.DeserializeObject<List<FromYTJson>>(_musicHistoryJson);
            if (_totalHistory.Count == 0)
            {
                return false;
            }
            return true;
        }

        public void SeparateSource(bool uploadedExists)
        {
            List<ToListenBrainz> MusicHistory = new();
            int i = 0;
            int fileNum = 0;

            foreach (FromYTJson x in _totalHistory)
            {
                if (x.header != "YouTube")
                {
                    i++;
                    ToListenBrainz ytmInfo = new(x);
                    if (ytmInfo.track_metadata.artist_name == "Music Library Uploads")
                    {
                        if (!uploadedExists)
                        {
                            ytmInfo.track_metadata.artist_name = "Unknown";
                        }
                        else
                        {
                            string artist;
                            if (_artist.TryGetValue(ytmInfo.track_metadata.track_name, out artist))
                            {
                                ytmInfo.track_metadata.artist_name = artist;
                            }
                            else
                            {
                                ytmInfo.track_metadata.artist_name = "Unknown";
                            }
                            MusicHistory.Add(ytmInfo);
                        }
                    }
                    else if (!ytmInfo.track_metadata.track_name.Contains("https"))
                    {
                        MusicHistory.Add(ytmInfo);
                    }
                }

                string SerializedMusicHistory = JsonConvert.SerializeObject(MusicHistory);

                if (Encoding.UTF8.GetByteCount(SerializedMusicHistory) > 10200)
                {
                    fileNum++;
                    SendToListen(SerializedMusicHistory, i, fileNum);
                    i = 0;
                    MusicHistory.Clear();
                }
            }
        }

        private async void SendToListen(string musicHistory, int numSongs, int fileNum)
        {
            string ToJson = @"{
				""listen_type"": ""import"",
				""payload"":";

            string toInsert = $"{ToJson} {musicHistory}\n}}";
            File.Create($"JsonOut/{fileNum}.json").Close();
            File.WriteAllText($"JsonOut/{fileNum}.json", toInsert);

            StringContent ToSend = new(toInsert, Encoding.UTF8, "application/json");

            if (numSongs > 2686)
            {
                Thread.Sleep(10000);
            }
            var response = await client.PostAsync($"https://{ROOT}/1/submit-listens", ToSend);

            string result = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"{result}\n{fileNum}");
        }

        public ReadMusicHistory(string token)
        {
            _totalHistory = new();
            _musicHistoryJson = "music-history.json";
            _artist = new();
            ROOT = "api.listenbrainz.org";
            client = new();
            client.DefaultRequestHeaders.Authorization = new("Token", token);
        }
    }
}
