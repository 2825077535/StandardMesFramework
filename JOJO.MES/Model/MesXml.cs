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

        /// <summary>
        /// 不带反馈的发送信息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="obj"></param>
        public static async void SendObjectAsXml(Socket socket, string xmlString)
        {
            try
            {
                byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlString);
                await Task.Run(() =>
                {
                    // 通过Socket发送数据
                    socket.Send(xmlBytes, 0, xmlBytes.Length, SocketFlags.None);
                });
            }
            catch (Exception ex)
            {
                MesLog.Error("发送不带反馈的Socket数据失败：" + ex.ToString());
            }
        }
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
        public static string SerializeToXml(object obj)
        {
            try
            {
                lock (Lock)
                {
                    StringWriter stringWriter = new StringWriter();

                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                    {
                        //添加输入对象的头，
                        xmlWriter.WriteStartDocument();
                        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (FieldInfo field in fields)
                        {
                            object fieldValue = field.GetValue(obj);
                            string fieldName = field.Name;

                            if (fieldValue != null)
                            {
                                Type valueType = fieldValue.GetType();
                                if (valueType.IsPrimitive || valueType == typeof(string) || !valueType.IsArray)
                                {
                                    if (field.Name.Contains("MesValue"))
                                    {
                                        xmlWriter.WriteValue(fieldValue);
                                    }
                                    else if (field.Name.Contains("MesName"))
                                    {
                                        xmlWriter.WriteStartElement(fieldValue.ToString());
                                        continue;
                                    }
                                    else
                                    {
                                        xmlWriter.WriteStartElement(field.FieldType.Name.ToString());
                                        xmlWriter.WriteValue(fieldValue);
                                        xmlWriter.WriteEndElement();
                                        continue;
                                    }
                                    xmlWriter.WriteEndElement();

                                }
                                else if (valueType.IsArray)
                                {

                                    Array array = (Array)fieldValue;
                                    foreach (object item in array)
                                    {
                                        if (item != null)
                                        {
                                            string subXml = SerializeToXml(item);
                                            xmlWriter.WriteRaw(subXml);
                                        }
                                    }

                                }
                                else
                                {
                                    string subXml = SerializeToXml(fieldValue);
                                    xmlWriter.WriteRaw(subXml);
                                    string xml = xmlWriter.ToString();
                                }
                            }
                        }
                        //xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                    }
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                MesLog.Error("序列化XML失败：" + ex.ToString());
                return "";
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
