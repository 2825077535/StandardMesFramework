using JOJO.Mes.Const;
using JOJO.Mes.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace JOJO.Mes.Model
{
    internal class MesXml
    {
        public const string _XmlAttribute = nameof(XmlAttribute);

        public static T DeserializeObject<T>(string Xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(Xml);
            T deserializedData = (T)serializer.Deserialize(stringReader);
            stringReader.Dispose();
            return deserializedData;
        }
        /// <summary>
        /// 反序列化为MesProcessData
        /// </summary>
        /// <param name="Xml"></param>
        /// <returns></returns>
        public static MesProcessData DeserializeXml(string Xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Xml);

            MesProcessData data = new MesProcessData();
            data = TraverseNodes(xmlDoc.DocumentElement);
            return data;
        }
        static MesProcessData TraverseNodes(XmlNode node)
        {
            MesProcessData data1 = new MesProcessData();
            data1.MesName = node.Name;
            if (node.HasChildNodes)
            {
                if (node.FirstChild.NodeType == XmlNodeType.Text)
                {
                    data1.MesValue = node.InnerText;
                    return data1;
                }
                List<object> datas = new List<object>();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    datas.Add(TraverseNodes(childNode));

                }
                data1.MesValue = datas.ToArray();
            }
            else
            {
                data1.MesValue = node.InnerText;
            }
            return data1;
        }

        private static readonly object Lock = new object();
        /// <summary>
        /// 将传入的Object对象序列化为XML格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static XElement SerializeToXml(object obj)
        {
            try
            {
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                string Name = "";
                Object Value = new object();
                Dictionary<string, Dictionary<string, object>> Properties = new Dictionary<string, Dictionary<string, object>>();
                if (fields[0].Name.Contains("Name"))
                {
                    Name = fields[0].GetValue(obj).ToString();
                    Value = fields[1].GetValue(obj);
                }
                else
                {
                    Name = fields[1].GetValue(obj).ToString();
                    Value = fields[0].GetValue(obj);
                }
                Properties = (Dictionary<string, Dictionary<string,object>>)fields[2].GetValue(obj);

                XElement xmlWriter = new XElement(Name);
                if (Value.GetType().IsArray)
                {
                    Array array = (Array)Value;
                    foreach (object item in array)
                    {
                        if (item != null)
                        {
                            XElement subXml = SerializeToXml(item);
                            xmlWriter.Add(subXml);
                        }
                    }
                }
                else
                {
                    if (Properties.ContainsKey(_XmlAttribute))
                    {
                        foreach (var item in Properties[_XmlAttribute])
                        {
                            xmlWriter.SetAttributeValue(item.Key, item.Value);
                        }
                    }
                    xmlWriter.SetValue(Value);
                }
                return xmlWriter;
            }
            catch (Exception ex)
            {
                MesLog.Error(ex.ToString());
                return null;
            }
        }
        private static XElement RecursiveProcess(XElement element)
        {
            List<XElement> newChildren = new List<XElement>();
            foreach (XElement child in element.Elements())
            {
                XElement name = child.Element("MesName");
                XElement value = child.Element("MesValue");
                if (name != null && value != null)
                {
                    XElement newElement;
                    if (value.HasElements)
                    {
                        // 如果Value有子元素，递归处理
                        XElement processedValue = RecursiveProcess(value);
                        newElement = new XElement(name.Value, processedValue.Nodes());
                    }
                    else
                    {
                        newElement = new XElement(name.Value, value.Value);
                    }
                    newChildren.Add(newElement);
                }
                else
                {
                    if (child.Nodes().Count() > 1)
                    {
                        // 如果没有Name和Value，继续递归处理子元素
                        XElement recursivelyProcessedChild = RecursiveProcess(child);
                        newChildren.Add(recursivelyProcessedChild);
                    }
                    else
                    {
                        newChildren.Add(child);
                    }

                }
            }
            element.RemoveAll();
            element.Add(newChildren);
            return element;
        }
    }

}
