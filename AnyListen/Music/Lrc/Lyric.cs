using System;
using System.Text.RegularExpressions;

namespace AnyListen.Music.Lrc
{
    public class Lyric
    {
        public double[] LyricTimeLine;

        public string[] LyricTextLine;

        public Lyric(string lrcText)
        {
            GetLyric(lrcText);
        }

        public void GetLyric(string text)
        {
            text = text.Replace("\r\n", "\r");
            text = Regex.Replace(text, @"[(<]\d+[)>]", "");
            var array = new string[1000];
            var array2 = new string[1000];
            var num5 = 0;
            var num6 = 0;
            var array3 = text.Split('\r', '\n');
            if (array3.Length <=2)
            {
                return;
            }
            foreach (var text3 in array3)
            {
                if (!Regex.IsMatch(text3, @"\[\d+:\d+.\d+\]"))
                {
                    continue;
                }
                var array5 = text3.Trim().Split(']');
                if (string.IsNullOrEmpty(array5[1].Trim()))
                {
                    continue;
                }
                foreach (var text4 in array5)
                {
                    if (!string.IsNullOrEmpty(text4) && text4.Substring(0, 1) == "[")
                    {
                        array[num5] = text4.Substring(1).PadRight(9,'0');
                        num5++;
                    }
                    else
                    {
                        for (var k = num6; k < num5; k++)
                        {
                            array2[k] = text4;
                        }
                        num6 = num5;
                    }
                }
            }
            var num7 = 0;
            for (var l = 0; l < 1000; l++)
            {
                if (array[l] != null) continue;
                num7 = l;
                break;
            }
            for (var l = 0; l < num7; l++)
            {
                var text3 = array[l];
                array[l] = text3.Substring(0, 2) + text3.Substring(3, 2) + text3.Substring(6);
            }
            for (var l = 0; l < num7; l++)
            {
                for (var m = 0; m < num7 - l; m++)
                {
                    try
                    {
                        if (array[m + 1] == null)
                        {
                            break;
                        }
                        if (int.Parse(array[m]) <= int.Parse(array[m + 1])) continue;
                        var text5 = array[m];
                        var text6 = array2[m];
                        array[m] = array[m + 1];
                        array2[m] = array2[m + 1];
                        array[m + 1] = text5;
                        array2[m + 1] = text6;
                    }
                    catch
                    {
                        Console.Write("");
                    }
                }
            }
            LyricTextLine = new string[num7];
            LyricTimeLine = new double[num7];
            for (var l = 0; l < num7; l++)
            {
                if (array[l] == "" || array[l] == null)
                {
                    array[l] = "000000";
                }
                LyricTextLine[l] = array2[l].Trim();
                LyricTimeLine[l] = Convert.ToInt32(array[l].Substring(0, 2))*60 + Convert.ToInt32(array[l].Substring(2, 2)) + Convert.ToInt32(array[l].Substring(4))/1000;
            }
            LyricTimeLine[LyricTimeLine.Length - 1] = 999999;
            LyricTimeLine[0] = 0.0;
        }
    }
}