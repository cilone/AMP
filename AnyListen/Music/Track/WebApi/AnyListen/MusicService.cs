using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AnyListen.Settings;
using Newtonsoft.Json;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public class MusicService
    {
        public static async Task<List<WebTrackResultBase>> MusicSearch(string type, string subType, string key, string id,int page = 1, int size = 30)
        {
            try
            {
                using (var web = new WebClient { Proxy = null, Encoding = Encoding.UTF8 })
                {
                    var url = "http://vip.itwusun.com/music/"+subType+"/" + type;
                    switch (subType)
                    {
                        case "album":
                            url +=  "?id=" + id;
                            break;
                        case "artist":
                            url += "?id=" + id + "&p=" + page + "&s=" + size;
                            break;
                        case "collect":
                            url += "?id=" + id + "&p=" + page + "&s=" + size;
                            break;
                        case "song":
                            url += "?id=" + id;
                            break;
                        default:
                            url += "?k=" + key + "&p=" + page + "&s=" + size;
                            break;
                    }
                    url += "&sign=" + AnyListenSettings.Instance.Config.PersonalCode;
                    var results = JsonConvert.DeserializeObject<List<SongResult>>(await web.DownloadStringTaskAsync(url));
                    return results.Select(x => new AnyListenWebResult
                    {
                        Duration = TimeSpan.FromMilliseconds(x.Length * 1000),
                        Year = string.IsNullOrEmpty(x.Year) ? 0 : Convert.ToUInt32(x.Year.Substring(0, 4)),
                        Title = x.SongName,
                        Uploader = x.ArtistName,
                        Album = x.AlbumName,
                        BitRate = x.BitRate,
                        Result = x,
                        Views = 0,
                        ImageUrl = x.PicUrl,
                        Url = string.IsNullOrEmpty(x.SqUrl)? (string.IsNullOrEmpty(x.HqUrl)? x.LqUrl : x.HqUrl) : x.SqUrl,
                        Genres = new List<Genre>(),
                        Description = "",
                        WebTrack = x
                    }).Cast<WebTrackResultBase>().ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}