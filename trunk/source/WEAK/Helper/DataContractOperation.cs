using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace WEAK.Helper
{
    public static class DataContractOperation
    {
        #region Methods

        public static void Save<T>(T instance, string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(fileStream, instance);
            }
        }

        public static T Load<T>(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(fileStream);
            }
        }

        public static void SaveMany<T>(IEnumerable<T> instances, string path)
        {
            XElement element = SaveMany(instances);
            element.Save(path);
        }

        public static IEnumerable<T> LoadMany<T>(string path)
        {
            return LoadMany<T>(XElement.Load(path));
        }

        public static XElement Save<T>(T instance)
        {
            XElement ret = new XElement(typeof(T).FullName);

            using (XmlWriter writer = ret.CreateWriter())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(writer, instance);
                writer.Flush();
            }

            return ret;
        }

        public static T Load<T>(XElement element)
        {
            using (XmlReader reader = element.Elements().FirstOrDefault().CreateReader())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(reader);
            }
        }

        public static XElement SaveMany<T>(IEnumerable<T> instances)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            XElement ret = new XElement(typeof(T).FullName);

            using (XmlWriter writer = ret.CreateWriter())
            {
                foreach (T element in instances)
                {
                    serializer.WriteObject(writer, element);
                    writer.Flush();
                }
            }

            return ret;
        }

        public static IEnumerable<T> LoadMany<T>(XElement element)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            foreach (XElement child in element.Elements())
            {
                using (XmlReader reader = child.CreateReader())
                {
                    yield return (T)serializer.ReadObject(reader);
                }
            }
        }

        #endregion
    }
}
