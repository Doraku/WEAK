using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace WEAK.Helper
{
    /// <summary>
    /// Provides methods to do load and save operation on data contract decored classes.
    /// </summary>
    public static class DataContractOperation
    {
        #region Fields

        private const string _containerName = "Container";

        #endregion

        #region Methods

        /// <summary>
        /// Saves an instance to a XElement and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="instance">The instance to save.</param>
        /// <returns>The XElement representing the serialised instance.</returns>
        /// <exception cref="ArgumentNullException">instance is null.</exception>
        /// <exception cref="InvalidDataContractException">The type being serialized does not conform to data contract rules. For example,
        /// the System.Runtime.Serialization.DataContractAttribute attribute has not been applied to the type.</exception>
        /// <exception cref="SerializationException">There is a problem with the instance being written.</exception>
        public static XElement Save<T>(T instance)
        {
            instance.CheckParameter(nameof(instance));

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            XElement container = new XElement(_containerName);

            using (XmlWriter writer = container.CreateWriter())
            {
                serializer.WriteObject(writer, instance);
                writer.Flush();
            }

            return container.Elements().First();
        }

        /// <summary>
        /// Loads an instance from a XElement.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="element">The XElement representing the serialised instance.</param>
        /// <returns>The instance.</returns>
        /// <exception cref="ArgumentNullException">element is null.</exception>
        public static T Load<T>(XElement element)
        {
            element.CheckParameter(nameof(element));

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            using (XmlReader reader = element.CreateReader())
            {
                return (T)serializer.ReadObject(reader);
            }
        }

        /// <summary>
        /// Saves multiple instances as XElement and returns them.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="instances">The instances to save.</param>
        /// <returns>The XElements representing the serialised instances.</returns>
        /// <exception cref="ArgumentNullException">instances is null or contains null element(s).</exception>
        /// <exception cref="InvalidDataContractException">The type being serialized does not conform to data contract rules. For example,
        /// the System.Runtime.Serialization.DataContractAttribute attribute has not been applied to the type.</exception>
        /// <exception cref="SerializationException">There is a problem with the instance being written.</exception>
        public static IEnumerable<XElement> SaveMany<T>(IEnumerable<T> instances)
        {
            instances.CheckParameter(nameof(instances));

            foreach (T instance in instances)
            {
                yield return Save(instance);
            }
        }

        /// <summary>
        /// Loads instances from XElements.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="elements">The XElements representing the serialised instances.</param>
        /// <returns>The instances.</returns>
        /// <exception cref="ArgumentNullException">elements is null or contains null element(s).</exception>
        public static IEnumerable<T> LoadMany<T>(IEnumerable<XElement> elements)
        {
            elements.CheckParameter(nameof(elements));

            foreach (XElement element in elements)
            {
                yield return Load<T>(element);
            }
        }

        /// <summary>
        /// Saves an instance to a file.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="instance">The instance to save.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <exception cref="ArgumentNullException">instance is null.</exception>
        /// <exception cref="InvalidDataContractException">The type being serialized does not conform to data contract rules. For example,
        /// the System.Runtime.Serialization.DataContractAttribute attribute has not been applied to the type.</exception>
        /// <exception cref="SerializationException">There is a problem with the instance being written.</exception>
        public static void Save<T>(T instance, string filePath)
        {
            Save(instance).Save(filePath);
        }

        /// <summary>
        /// Loads an instance from a file.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The instance.</returns>
        public static T Load<T>(string filePath)
        {
            return Load<T>(XElement.Load(filePath));
        }

        /// <summary>
        /// Saves multiple instances to a file.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="instances">The instances to save.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <exception cref="ArgumentNullException">instances is null or contains null element(s).</exception>
        /// <exception cref="InvalidDataContractException">The type being serialized does not conform to data contract rules. For example,
        /// the System.Runtime.Serialization.DataContractAttribute attribute has not been applied to the type.</exception>
        /// <exception cref="SerializationException">There is a problem with the instance being written.</exception>
        public static void SaveMany<T>(IEnumerable<T> instances, string filePath)
        {
            XElement container = new XElement(_containerName);

            container.Add(SaveMany(instances));

            container.Save(filePath);
        }

        /// <summary>
        /// Loads instances from a file.
        /// </summary>
        /// <typeparam name="T">The type of the data contract.</typeparam>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The instances.</returns>
        public static IEnumerable<T> LoadMany<T>(string filePath)
        {
            return LoadMany<T>(XElement.Load(filePath).Elements());
        }

        #endregion
    }
}
