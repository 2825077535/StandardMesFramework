using JOJO.Mes.Const;
using JOJO.Mes.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace JOJO.Mes.Model
{
    internal class MesJson
    {
        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        public static string SerializeObject<T>(T Data)
        {
            return JsonConvert.SerializeObject(Data);
        }

        /// <summary>
        /// 反序列化为 MesProcessData
        /// </summary>
        /// <param name="Json"></param>
        /// <returns></returns>
        public static MesProcessData DeserializeJson(string Json)
        {
            try
            {
                JObject jsonObject = JObject.Parse(Json);
                MesProcessData data = new MesProcessData();
                // 开始遍历解析 JSON 对象
                data = TraverseJsonNodes(jsonObject);
                return data;
            }
            catch (Exception ex)
            {
                MesLog.Error(ex.ToString());
                return null;
            }
        }
        static MesProcessData TraverseJsonNodes(JObject jsonObject)
        {
            MesProcessData data = new MesProcessData();
            data.MesName = jsonObject.Path;
            if (jsonObject.Count == 1)
            {
                data.MesValue = jsonObject.First.ToString();
            }
            List<MesProcessData> datas = new List<MesProcessData>();
            foreach (var item in jsonObject)
            {
                if (item.Value.Type != JTokenType.Array)
                {
                    MesProcessData Jdata = new MesProcessData();

                    Jdata.MesName = item.Key.ToString();
                    Jdata.MesValue = item.Value.ToString();
                    datas.Add(Jdata);
                }
                else
                {
                    foreach (var item1 in item.Value)
                    {
                        MesProcessData Jdata = new MesProcessData();
                        Jdata = TraverseJsonNodes((JObject)item1);
                        Jdata.MesName = item.Key;
                        datas.Add(Jdata);
                    }
                }
            }
            data.MesValue = datas.ToArray();
            return data;
        }
        private static readonly object Lock = new object();

        public static string Serialize(MesProcessData obj)
        {
            var jsonBuilder = new System.Text.StringBuilder();
            jsonBuilder.Append(SerializeToJson(obj));
            int firstBracketIndex = jsonBuilder.ToString().IndexOf('[');
            if (firstBracketIndex != -1)
            {
                jsonBuilder.Remove(firstBracketIndex, 1);
            }

            // 查找最后一个 ] 的索引
            int lastBracketIndex = jsonBuilder.ToString().LastIndexOf(']');
            if (lastBracketIndex != -1)
            {
                jsonBuilder.Remove(lastBracketIndex, 1);
            }
            return jsonBuilder.ToString();
        }
        public static string SerializeToJson(object obj)
        {
            try
            {
                lock (Lock)
                {
                    var jsonBuilder = new System.Text.StringBuilder();
                    FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    bool isFirstField = true;

                    foreach (FieldInfo field in fields)
                    {
                        object fieldValue = field.GetValue(obj);
                        string fieldName = field.Name;

                        if (fieldValue != null)
                        {
                            Type valueType = fieldValue.GetType();

                            if (!isFirstField)
                            {
                                jsonBuilder.Append(",");
                            }

                            if (valueType.IsPrimitive || valueType == typeof(string))
                            {
                                if (field.Name.Contains("MesValue"))
                                {
                                    if (valueType == typeof(string))
                                    {
                                        jsonBuilder.Append($"\"{fieldValue.ToString()}\"");
                                    }
                                    else
                                    {
                                        jsonBuilder.Append(fieldValue.ToString());
                                    }
                                }
                                else if (field.Name.Contains("MesName"))
                                {
                                    if (fieldValue.ToString() == "")
                                    {
                                        continue;
                                    }
                                    jsonBuilder.Append($"\"{fieldValue.ToString()}\":");
                                    isFirstField = true;
                                    continue;
                                }
                                else
                                {
                                    jsonBuilder.Append($"\"{field.Name}\":");
                                    if (valueType == typeof(string))
                                    {
                                        jsonBuilder.Append($"\"{fieldValue.ToString()}\"");
                                    }
                                    else
                                    {
                                        jsonBuilder.Append(fieldValue.ToString());
                                    }
                                }
                            }
                            else if (valueType.IsArray)
                            {
                                jsonBuilder.Append("[{");
                                Array array = (Array)fieldValue;
                                bool isFirstItem = true;
                                foreach (object item in array)
                                {
                                    if (item != null)
                                    {
                                        if (!isFirstItem)
                                        {
                                            jsonBuilder.Append(",");
                                        }
                                        string subJson = SerializeToJson(item);
                                        jsonBuilder.Append(subJson);
                                        isFirstItem = false;
                                    }
                                }
                                jsonBuilder.Append("}]");
                            }
                            else
                            {
                                jsonBuilder.Append($"\"{field.Name}\":");
                                string subJson = SerializeToJson(fieldValue);
                                jsonBuilder.Append(subJson);
                            }

                            isFirstField = false;
                        }
                    }
                    return jsonBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

    }
}
