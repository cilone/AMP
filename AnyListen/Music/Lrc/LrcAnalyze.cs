using System;
using System.Collections.Generic;
using System.IO;

namespace AnyListen.Music.Lrc
{
    public class LrcAnalyze
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }

        readonly string _content;
        readonly List<LrcLineItem> _lrcList;

        public LrcLineItem[] Lrcs { get; set; }

        //public LrcAnalyze(string filepath)
        //{
        //    _lrcList = new List<LrcLineItem>();
        //    using (var reader = new StreamReader(filepath, Encoding.UTF8))
        //    {
        //        _content = reader.ReadToEnd();
        //    }
        //    Analyze();
        //}

        public LrcAnalyze(string lrcContext)
        {
            _lrcList = new List<LrcLineItem>();
            _content = lrcContext;
            Analyze();
        }

        private void Analyze()
        {
            var reader = new StringReader(_content);

            var lineStr = reader.ReadLine();
            while (lineStr != null)
            {
                if (lineStr.Length > 3)
                {
                    switch (lineStr.Substring(1, 2))
                    {
                        case "ti":
                            Title = lineStr.Substring(4, lineStr.Length - 5);
                            break;
                        case "ar":
                            Artist = lineStr.Substring(4, lineStr.Length - 5);
                            break;
                        case "al":
                            Album = lineStr.Substring(4, lineStr.Length - 5);
                            break;
                        default:
                            GetLrcStr(lineStr);
                            break;
                    }
                }
                lineStr = reader.ReadLine();

            }
            //排序（冒泡）
            SortLrcLine();
        }

        private void GetLrcStr(string lineStr)
        {
            var num = (lineStr.LastIndexOf(']') + 1) / 10;
            var text = lineStr.Substring(num * 10);

            try
            {
                for (var i = 0; i < num; i++)
                {
                    var timestr = lineStr.Substring(i * 10 + 1, 8);
                    var time = Convert.ToDouble(timestr.Substring(0, 2)) * 60 + Convert.ToDouble(timestr.Substring(3, 5));
                    _lrcList.Add(new LrcLineItem(text, time));
                }
            }
            catch (FormatException)
            {
            }
        }

        private void SortLrcLine()
        {
            Lrcs = _lrcList.ToArray();
            for (var i = 0; i < Lrcs.Length - 1; i++)
            {
                for (var j = i + 1; j < Lrcs.Length; j++)
                {
                    if (Lrcs[i].Time > Lrcs[j].Time)
                    {
                        var temp = Lrcs[j];
                        Lrcs[j] = Lrcs[i];
                        Lrcs[i] = temp;
                    }
                }

            }
        }

        //获得相应时间的歌词，返回索引
        public int GetIndex(double time)
        {
            for (var i = 0; i < Lrcs.Length; i++)
            {
                if (Lrcs[i].Time > time)
                {
                    return i - 1;
                }
            }
            return Lrcs.Length - 1;
        }

        //返回多行歌词（count*2+1行）
        public string[] GetLrcStrings(int count, double time)
        {
            var index = GetIndex(time);

            var lrcstrings = new string[count * 2 + 1];
            for (var i = 0; i < count; i++)
            {
                if (index - count + i > 0)
                    lrcstrings[i] = Lrcs[index - count + i].Text;
                else
                    lrcstrings[i] = string.Empty;
            }
            lrcstrings[count] = Lrcs[index].Text;
            for (var i = 0; i < count; i++)
            {
                if (index + i < Lrcs.Length)
                    lrcstrings[count + i + 1] = Lrcs[index + i+1].Text;
                else
                    lrcstrings[i] = string.Empty;
            }
            return lrcstrings;
        }
    }
}