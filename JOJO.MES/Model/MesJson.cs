using JOJO.Mes.Const;
using JOJO.Mes.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


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
            // 解析 JSON 字符串为 JObject
            JObject jsonObject = JObject.Parse(Json);
            MesProcessData data = new MesProcessData();
            // 开始遍历解析 JSON 对象
            data = TraverseJsonNodes(jsonObject);
            return data;
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
                if (item.Value.Type == JTokenType.String)
                {
                    MesProcessData Jdata = new MesProcessData();

                    Jdata.MesName = item.Key.ToString();
                    Jdata.MesValue = item.Value.ToString();
                    datas.Add(Jdata);
                }
                else
                    datas.Add(TraverseJsonNodes((JObject)item.Value));
            }
            data.MesValue = datas.ToArray();
            return data;
        }
        private static readonly object Lock = new object();

        /// <summary>
        /// 将传入的 Object 对象序列化为 JSON 格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToJson(object obj)
        {
            try
            {
                lock (Lock)
                {
                    StringBuilder sb = new StringBuilder();
                    SerializeObject(obj, sb);
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                MesLog.Error("Mes Json序列化失败：" + ex.ToString());
                return "";
            }
        }

        private static void SerializeObject(object obj, StringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            Type type = obj.GetType();

            if (type.IsPrimitive || type == typeof(string))
            {
                sb.Append(JsonValue(obj));
                return;
            }

            if (type.IsArray)
            {
                sb.Append("[");
                Array array = (Array)obj;
                for (int i = 0; i < array.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    SerializeObject(array.GetValue(i), sb);
                }
                sb.Append("]");
                return;
            }

            sb.Append("{");
            sb.AppendLine();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool first = true;
            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(obj);
                if (fieldValue == null)
                {
                    continue;
                }
                if (!first)
                {
                    sb.Append(",");
                    sb.AppendLine();
                }
                first = false;
                sb.Append($"\"{field.Name}\": ");
                SerializeObject(fieldValue, sb);
            }
            sb.AppendLine();
            sb.Append("}");
        }

        private static string JsonValue(object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            if (obj is string str)
            {
                return $"\"{EscapeString(str)}\"";
            }
            if (obj is bool boolValue)
            {
                return boolValue.ToString().ToLower();
            }
            if (obj is char)
            {
                return $"\"{obj}\"";
            }
            return obj.ToString();
        }

        private static string EscapeString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c == '\\' || c == '"')
                {
                    sb.Append('\\');
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
