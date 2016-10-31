using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music.Track.WebApi.AnyListen
{
    public static class CommonHelper
    {
        public static bool Is45Install()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                return  ndpKey != null && Convert.ToInt32(ndpKey.GetValue("Release")) >= 378389;
            }
        }

        public static string GetHtmlContent(string url)
        {
            try
            {
                var uri = new Uri(url);
                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                myHttpWebRequest.Method = "GET";
                myHttpWebRequest.Timeout = 5000;
                myHttpWebRequest.Accept = @"text/html,application/xhtml+xml,application/xml;*/*";
                myHttpWebRequest.UserAgent = @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
                var response = (HttpWebResponse)myHttpWebRequest.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK) return null;
                // ReSharper disable once AssignNullToNotNullAttribute
                var responseReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var result = responseReader.ReadToEnd();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetLocation(string getUrl)
        {
            try
            {
                var xmRequest = (HttpWebRequest)WebRequest.Create(getUrl);
                xmRequest.Referer = getUrl;
                xmRequest.Accept = @"text/html,application/xhtml+xml,application/xml;*/*";
                xmRequest.UserAgent = @"Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.101 Safari/537.36";
                xmRequest.Method = "GET";
                xmRequest.ContentType = "text/html; charset=utf-8";
                var response = (HttpWebResponse)xmRequest.GetResponse();
                if (response.StatusCode != HttpStatusCode.BadRequest && response.StatusCode != HttpStatusCode.Forbidden && response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.NotFound)
                {
                    return response.ResponseUri.ToString();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string GetDownloadUrl(SongResult song, int bitRate, int prefer, bool isFormat)
        {
            string link;
            switch (bitRate)
            {
                case 0:
                    switch (prefer)
                    {
                        case 0:
                            if (!string.IsNullOrEmpty(song.FlacUrl))
                            {
                                link = song.FlacUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.ApeUrl))
                            {
                                link = song.ApeUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.WavUrl))
                            {
                                link = song.WavUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.SqUrl))
                            {
                                link = song.SqUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.HqUrl))
                            {
                                link = song.HqUrl;
                            }
                            else
                            {
                                link = song.LqUrl;
                            }
                            break;
                        case 1:
                            if (!string.IsNullOrEmpty(song.ApeUrl))
                            {
                                link = song.ApeUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.FlacUrl))
                            {
                                link = song.FlacUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.WavUrl))
                            {
                                link = song.WavUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.SqUrl))
                            {
                                link = song.SqUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.HqUrl))
                            {
                                link = song.HqUrl;
                            }
                            else
                            {
                                link = song.LqUrl;
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(song.WavUrl))
                            {
                                link = song.WavUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.FlacUrl))
                            {
                                link = song.FlacUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.ApeUrl))
                            {
                                link = song.ApeUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.SqUrl))
                            {
                                link = song.SqUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.HqUrl))
                            {
                                link = song.HqUrl;
                            }
                            else
                            {
                                link = song.LqUrl;
                            }
                            break;
                    }
                    break;
                case 1:
                    if (!string.IsNullOrEmpty(song.SqUrl))
                    {
                        link = song.SqUrl;
                    }
                    else if (!string.IsNullOrEmpty(song.HqUrl))
                    {
                        link = song.HqUrl;
                    }
                    else
                    {
                        link = song.LqUrl;
                    }
                    break;
                case 2:
                    if (!string.IsNullOrEmpty(song.HqUrl))
                    {
                        link = song.HqUrl;
                    }
                    else if (!string.IsNullOrEmpty(song.SqUrl))
                    {
                        link = song.SqUrl;
                    }
                    else
                    {
                        link = song.LqUrl;
                    }
                    break;
                default:
                    link = song.LqUrl;
                    break;
            }
            if (isFormat)
            {
                if (link.ToLower().Contains(".flac"))
                {
                    link = "flac";
                }
                else if (link.ToLower().Contains(".ape"))
                {
                    link = "ape";
                }
                else if (link.ToLower().Contains(".wav"))
                {
                    link = "ape";
                }
                else if (link.ToLower().Contains(".ogg"))
                {
                    link = "ogg";
                }
                else if (link.ToLower().Contains(".aac"))
                {
                    link = "acc";
                }
                else if (link.ToLower().Contains(".wma"))
                {
                    link = "wma";
                }
                else
                {
                    link = "mp3";
                }
            }
            return link;
        }

        public static string GetFormat(string url)
        {
            string link;
            if (url.ToLower().Contains(".flac"))
            {
                link = "flac";
            }
            else if (url.ToLower().Contains(".ape"))
            {
                link = "ape";
            }
            else if (url.ToLower().Contains(".wav"))
            {
                link = "ape";
            }
            else if (url.ToLower().Contains(".ogg"))
            {
                link = "ogg";
            }
            else if (url.ToLower().Contains(".aac"))
            {
                link = "acc";
            }
            else if (url.ToLower().Contains(".wma"))
            {
                link = "wma";
            }
            else
            {
                link = "mp3";
            }
            return "." + link;
        }

        public static string NumToTime(int num)
        {
            var mins = num / 60;
            var seds = num % 60;
            string time;
            if (mins.ToString(CultureInfo.InvariantCulture).Length == 1)
            {
                time = @"0" + mins;
            }
            else
            {
                time = mins.ToString(CultureInfo.InvariantCulture);
            }
            time += ":";
            if (seds.ToString(CultureInfo.InvariantCulture).Length == 1)
            {
                time += @"0" + seds;
            }
            else
            {
                time += seds.ToString(CultureInfo.InvariantCulture);
            }
            return time;
        }
        public static void AddLog(string str)
        {
            str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + str + "\r\n";
            var path = Path.Combine(Environment.CurrentDirectory, "Log");
            if (Directory.Exists(path))
            {
                try
                {
                    File.AppendAllText(Path.Combine(path, DateTime.Now.ToString("yyyy_MM_dd") + ".log"), str, Encoding.Default);
                }
                catch (Exception)
                {
                    //文件被占用
                }
            }
            else
            {
                Directory.CreateDirectory(path);
                var fs = File.Create(Path.Combine(path, DateTime.Now.ToString("yyyy_MM_dd") + ".log"));
                fs.Write(Encoding.Default.GetBytes(str), 0, Encoding.Default.GetBytes(str).Length);
                fs.Close();
            }
        }

        public static string RemoveSpicalChar(string input)
        {
            return Regex.Replace(input, @"[?:*""<>|\/]", "");
        }


        public static string GetLrc(string name, string artist, int length)
        {
            var url = "http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword=" + artist + "-" +
                          name + "&duration=" + length + "&hash=";
            var html = GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            var json = JObject.Parse(html);
            if (json["status"].ToString() == "404")
            {
                return "";
            }
            var hash = json["candidates"].First["accesskey"].ToString();
            var mid = json["candidates"].First["id"].ToString();
            url =
                "http://lyrics.kugou.com/download?ver=1&client=pc&id=" + mid + "&accesskey=" + hash + "&fmt=lrc&charset=utf8";
            html = GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            json = JObject.Parse(html);
            var str = DecodeBase64(Encoding.UTF8, json["content"].ToString());
            return str;
        }

        public static string DecodeBase64(Encoding encode, string result)
        {
            return encode.GetString(Convert.FromBase64String(result));
        }
    }
}