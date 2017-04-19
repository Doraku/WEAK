using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using NFluent;
using WEAK.Serialization;
using Xunit;

namespace WEAK.Test.Serialization
{
    public class DataContractOperationTest
    {
        #region Types

        [DataContract]
        private class Dummy
        {
            [DataMember]
            public string Property { get; set; }
        }

        #endregion

        #region Methods

        [Fact]
        public void Save_Should_throw_ArgumentNullException_When_instance_is_null()
        {
            Dummy instance = null;

            Check
                .ThatCode(() => DataContractOperation.Save(instance))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "instance");
        }

        [Fact]
        public void Save_Should_return_XElement()
        {
            Dummy instance = new Dummy { Property = "test" };

            XElement element = DataContractOperation.Save(instance);

            Check.That(element).IsNotNull();
        }

        [Fact]
        public void Load_Should_throw_ArgumentNullException_When_element_is_null()
        {
            XElement element = null;
            Check
                .ThatCode(() => DataContractOperation.Load<Dummy>(element))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "element");
        }

        [Fact]
        public void Load_Should_return_instance()
        {
            Dummy instance = new Dummy { Property = "instance" };

            XElement element = DataContractOperation.Save(instance);
            Dummy loadedInstance = DataContractOperation.Load<Dummy>(element);

            Check.That(loadedInstance).IsNotNull();
            Check.That(loadedInstance.Property).IsEqualTo(instance.Property);
        }

        [Fact]
        public void SaveMany_Should_throw_ArgumentNullException_When_instances_is_null()
        {
            IEnumerable<Dummy> instances = null;

            Check
                .ThatCode(() => DataContractOperation.SaveMany<Dummy>(instances).ToList())
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "instances");
        }

        [Fact]
        public void SaveMany_Should_return_XElements()
        {
            Dummy instance1 = new Dummy { Property = "instance1" };
            Dummy instance2 = new Dummy { Property = "instance2" };

            List<XElement> elements = DataContractOperation.SaveMany<Dummy>(new[] { instance1, instance2 }).ToList();

            Check.That(elements).IsNotNull();
            Check.That(elements.Count).IsEqualTo(2);
        }

        [Fact]
        public void LoadMany_Should_throw_ArgumentNullException_When_elements_is_null()
        {
            IEnumerable<XElement> elements = null;
            Check
                .ThatCode(() => DataContractOperation.LoadMany<Dummy>(elements).ToList())
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "elements");
        }

        [Fact]
        public void LoadMany_Should_return_instances()
        {
            Dummy instance1 = new Dummy { Property = "instance1" };
            Dummy instance2 = new Dummy { Property = "instance2" };

            List<XElement> elements = DataContractOperation.SaveMany<Dummy>(new[] { instance1, instance2 }).ToList();
            List<Dummy> loadedInstances = DataContractOperation.LoadMany<Dummy>(elements).ToList();

            Check.That(loadedInstances).IsNotNull();
            Check.That(loadedInstances.Count).IsEqualTo(2);
            Check.That(loadedInstances[0].Property).IsEqualTo(instance1.Property);
            Check.That(loadedInstances[1].Property).IsEqualTo(instance2.Property);
        }

        [Fact]
        public void Save_filePath_Should_throw_ArgumentNullException_When_instance_is_null()
        {
            string filePath = $"{nameof(Save_filePath_Should_throw_ArgumentNullException_When_instance_is_null)}.txt";
            Dummy instance = null;

            Check
                .ThatCode(() => DataContractOperation.Save(instance, filePath))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "instance");
        }

        [Fact]
        public void Save_filePath_Should_save_in_file()
        {
            string filePath = $"{nameof(Save_filePath_Should_save_in_file)}.txt";
            Dummy instance = new Dummy { Property = "test" };

            DataContractOperation.Save(instance, filePath);

            XElement element = XElement.Load(filePath);

            Check.That(element).IsNotNull();

            File.Delete(filePath);
        }

        [Fact]
        public void Load_filePath_Should_return_instance()
        {
            string filePath = $"{nameof(Load_filePath_Should_return_instance)}.txt";
            Dummy instance = new Dummy { Property = "instance" };

            DataContractOperation.Save(instance, filePath);
            Dummy loadedInstance = DataContractOperation.Load<Dummy>(filePath);

            Check.That(loadedInstance).IsNotNull();
            Check.That(loadedInstance.Property).IsEqualTo(instance.Property);

            File.Delete(filePath);
        }

        [Fact]
        public void SaveMany_filePath_Should_throw_ArgumentNullException_When_instances_is_null()
        {
            string filePath = $"{nameof(SaveMany_filePath_Should_throw_ArgumentNullException_When_instances_is_null)}.txt";
            IEnumerable<Dummy> instances = null;

            Check
                .ThatCode(() => DataContractOperation.SaveMany<Dummy>(instances, filePath))
                .Throws<ArgumentNullException>()
                .WithProperty("ParamName", "instances");
        }

        [Fact]
        public void SaveMany_filePath_Should_save_in_file()
        {
            string filePath = $"{nameof(SaveMany_filePath_Should_save_in_file)}.txt";
            Dummy instance1 = new Dummy { Property = "instance1" };
            Dummy instance2 = new Dummy { Property = "instance2" };

            DataContractOperation.SaveMany<Dummy>(new[] { instance1, instance2 }, filePath);
            XElement element = XElement.Load(filePath);

            Check.That(element).IsNotNull();
            Check.That(element.Elements().Count()).IsEqualTo(2);

            File.Delete(filePath);
        }

        [Fact]
        public void LoadMany_filePath_Should_return_instances()
        {
            string filePath = $"{nameof(LoadMany_filePath_Should_return_instances)}.txt";
            Dummy instance1 = new Dummy { Property = "instance1" };
            Dummy instance2 = new Dummy { Property = "instance2" };

            DataContractOperation.SaveMany(new[] { instance1, instance2 }, filePath);
            List<Dummy> loadedInstances = DataContractOperation.LoadMany<Dummy>(filePath).ToList();

            Check.That(loadedInstances).IsNotNull();
            Check.That(loadedInstances.Count).IsEqualTo(2);
            Check.That(loadedInstances[0].Property).IsEqualTo(instance1.Property);
            Check.That(loadedInstances[1].Property).IsEqualTo(instance2.Property);

            File.Delete(filePath);
        }

        #endregion
    }
}
