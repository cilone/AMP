using System;
using System.IO;
using System.Xml.Serialization;
using AnyListen.ViewModelBase;

namespace AnyListen.Settings
{
    [Serializable]
    public abstract class SettingsBase : PropertyChangedBase
    {
        public abstract void SetStandardValues();

        public abstract void Save(string programPath);

        protected void Save<T>(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, this);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }
    }
}
