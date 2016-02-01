using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SuperGlue.Localization
{
    public static class XmlExtensions
    {
        public static XmlElement With(this XmlElement node, Action<XmlElement> action)
        {
            action(node);
            return node;
        }

        public static XmlDocument FromFile(this XmlDocument document, string fileName)
        {
            document.Load(fileName);
            return document;
        }

        public static XmlDocument XmlFromFileWithRoot(this string fileName, string root)
        {
            if (File.Exists(fileName))
                return FromFile(new XmlDocument(), fileName);

            var document = new XmlDocument();

            WithRoot(document, root);

            return document;
        }

        public static XmlElement WithRoot(this XmlDocument document, string elementName)
        {
            var element = document.CreateElement(elementName);
            document.AppendChild(element);
            return element;
        }

        public static XmlDocument WithXmlText(this XmlDocument document, string xml)
        {
            document.LoadXml(xml);
            return document;
        }

        public static XmlElement WithFormattedText(this XmlElement element, string text)
        {
            if (element.OwnerDocument == null) return element;

            var cdataSection = element.OwnerDocument.CreateCDataSection(text);
            element.AppendChild(cdataSection);

            return element;
        }

        public static XmlElement AddElement(this XmlNode element, string name)
        {
            if (element.OwnerDocument == null) return null;

            var newElement = element.OwnerDocument.CreateElement(name);
            element.AppendChild(newElement);
            return newElement;
        }

        public static void AddComment(this XmlNode element, string text)
        {
            if (element.OwnerDocument == null) return;

            var comment = element.OwnerDocument.CreateComment(text);
            element.AppendChild(comment);
        }

        public static XmlElement AddElement(this XmlNode element, string name, Action<XmlElement> action)
        {
            if (element.OwnerDocument == null) return null;

            var element1 = element.OwnerDocument.CreateElement(name);
            element.AppendChild(element1);
            action(element1);

            return element1;
        }

        public static XmlElement WithInnerText(this XmlElement node, string text)
        {
            node.InnerText = text;
            return node;
        }

        public static XmlElement WithAtt(this XmlElement element, string key, string attValue)
        {
            element.SetAttribute(key, attValue);
            return element;
        }

        public static XmlElement WithAttributes(this XmlElement element, string text)
        {
            var str1 = text;
            var chArray1 = new[]
            {
        ','
            };
            foreach (var strArray in from str2 in str1.Split(chArray1) let chArray2 = new[]
            {
                ':'
            } select str2.Split(chArray2))
            {
                WithAtt(element, strArray[0].Trim(), strArray[1].Trim());
            }

            return element;
        }

        public static void SetAttributeOnChild(this XmlElement element, string childName, string attName, string attValue)
        {
            (element[childName] ?? AddElement(element, childName)).SetAttribute(attName, attValue);
        }

        public static XmlElement WithProperties(this XmlElement element, Dictionary<string, string> properties)
        {
            foreach (var keyValuePair in properties)
                element.SetAttribute(keyValuePair.Key, keyValuePair.Value);
            return element;
        }

        public static void ForEachElement(this XmlNode node, Action<XmlElement> action)
        {
            foreach (var xmlElement in node.ChildNodes.OfType<XmlElement>())
                action(xmlElement);
        }
    }
}