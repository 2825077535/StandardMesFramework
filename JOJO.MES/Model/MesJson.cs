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
            var jsonBuilder = SerializeToJson(obj).Value.ToString();
            if (jsonBuilder.StartsWith("[") && jsonBuilder.EndsWith("]"))
            {
                // 删除第一个 [ 和最后一个 ]
                jsonBuilder = jsonBuilder.Substring(1, jsonBuilder.Length - 2).Trim();
            }
            return jsonBuilder.ToString();
        }
        public static JProperty SerializeToJson(object obj)
        {
            try
            {
                lock (Lock)
                {
                    JProperty pairs;
                    var jsonBuilder = new System.Text.StringBuilder();
                    FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    string Name = "";
                    Object Value = new object();
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

                    if (Value.GetType().IsArray)
                    {
                        Array array = (Array)Value;
                        JArray array2 = new JArray();
                        JObject obj1 = new JObject();
                        foreach (object item in array)
                        {
                            if (item != null)
                            {
                                JProperty subJson = SerializeToJson(item);
                                obj1.Add(subJson);
                            }
                        }
                        array2.Add(obj1);
                        pairs = new JProperty(Name, array2);
                    }
                    else
                    {
                        pairs = new JProperty(Name, Value);
                    }
                    return pairs;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
