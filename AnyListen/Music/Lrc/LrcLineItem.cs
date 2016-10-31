namespace AnyListen.Music.Lrc
{
    public class LrcLineItem
    {
        public string TimeString { get; set; }
        public string Text { get; set; }
        public double Time { get; set; }

        public LrcLineItem(string text, double time)
        {
            Text = text;
            Time = time;

            //[02:13.60]歌词
            var timeint = (int)Time;
            TimeString = "[" + (timeint / 60).ToString("d2") + ":" + (timeint % 60).ToString("d2") + (Time - timeint).ToString("f2").Substring(1, 3) + "]" + Text;
        }

        public override string ToString()
        {
            return TimeString;
        }
    }
}