using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JX3Helper
{
    public class SerializableHelper
    {
        public static T DeserializeSoftTransSetting<T>(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                T local = (T)serializer.Deserialize(stream);
                stream.Close();
                return local;
            }
        }

        public static void SerializeSoftTransSetting<T>(T model, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize((TextWriter)writer, model);
                writer.Close();
            }
        }
    }
}
