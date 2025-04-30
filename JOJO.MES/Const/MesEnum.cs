using System;
using System.ComponentModel;
using System.Reflection;

namespace JOJO.Mes.Const
{
    public class MesEnum
    {
        /// <summary>
        /// 设备状态
        /// </summary>
        public enum MachineState
        {
            Null = -1,
            /// <summary>
            /// 停止按钮
            /// </summary>
            stop = 0,
            /// <summary>
            /// 开始运行按钮
            /// </summary>
            start = 1,
            /// <summary>
            /// 设备红灯
            /// </summary>
            RedLight = 2,
            /// <summary>
            /// 设备黄灯
            /// </summary>
            YelloLight = 3,
            /// <summary>
            /// 设备绿灯
            /// </summary>
            GreenLight = 4,
            /// <summary>
            /// 连接设备按钮
            /// </summary>
            MachineConnect = 5,
            /// <summary>
            /// 设备急停
            /// </summary>
            MachineStop = 6,
            /// <summary>
            /// 设备回原
            /// </summary>
            InitMachine = 7,
            /// <summary>
            /// 设备进板
            /// </summary>
            EnterBoard = 8,
            /// <summary>
            /// 设备检测中
            /// </summary>
            DetectBoard = 9,
            /// <summary>
            /// 设备出板
            /// </summary>
            OutBoard = 10,
            /// <summary>
            /// 设备变轨
            /// </summary>
            OrbitChange = 11,
            /// <summary>
            /// 设备等待进板
            /// </summary>
            AwaitEnterBoard = 12,
            /// <summary>
            /// 设备等待出板
            /// </summary>
            AwaitOutBoard = 13,

        }
        /// <summary>
        /// 用户枚举
        /// </summary>
        public enum MesCustomer
        {
            [Description("不使用MES")]
            None = -1,
            [Description("Mes1")]
            Mes1 = 1,
            [Description("Mes1")]
            Mes2 = 2,
        }
        /// <summary>
        /// 获取枚举的Description值
        /// </summary>
        /// <param name=value></param>
        /// <returns></returns>
        public string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return value.ToString();
        }
        /// <summary>
        /// 根据description的值找到对应的枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public T GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (GetDescription(value).Equals(description))
                {
                    return value;
                }
            }
            return default(T);
        }

    }

}
