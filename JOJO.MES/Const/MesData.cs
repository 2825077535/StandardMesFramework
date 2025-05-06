using System;
using System.Collections.Generic;
using System.Linq;

namespace JOJO.Mes.Const
{
    public class MesProcessData
    {
        public string MesName { get; set; } = "";
        public object MesValue { get; set; }
        /// <summary>
        /// 序列化类型
        /// 在XML中，需要设置属性值，或者头信息，将数据保存在序列化类型中，进行填充
        /// </summary>
        public Dictionary<string, Dictionary<string,object>> SerializationType { get; set; } = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// 根据指定路径找到对应的值
        /// </summary>
        /// <param name="path">数组路径，从下一级参数开始</param>
        /// <param name="index">当前使用的路径开始编号。仅在函数递归时使用</param>
        /// <returns></returns>
        public object FindValueByPath(string[] path, int index = 0)
        {
            if (this == null)
                return null;
            if (index >= path.Length)
            {
                return this.MesValue;
            }
            if (this.MesValue is Array)
            {
                Array array = this.MesValue as Array;
                foreach (MesProcessData child in array)
                {
                    if (child.MesName == path[index])
                    {
                        return child.FindValueByPath(path, index + 1);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 根据指定路径来修改参数值
        /// </summary>
        /// <param name="path">数组路径，从下一级参数开始</param>
        /// <param name="newValue">新的值</param>
        /// <param name="index">当前使用的路径开始编号。仅在函数递归时使用 </param>
        /// <returns></returns>
        public MesProcessData ModifyValueByPath(string[] path, object newValue, int index = 0)
        {
            if (this == null)
                return null;
            if (index >= path.Length)
            {
                this.MesValue = newValue;
                return this;
            }

            if (this.MesValue is Array array)
            {
                foreach (MesProcessData child in array)
                {
                    if (child.MesName == path[index])
                    {
                        // 递归修改下一级节点
                        child.ModifyValueByPath(path, newValue, index + 1);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// 根据指定路径找到对应的序列化类型字典
        /// </summary>
        /// <param name="path">数组路径，从下一级参数开始</param>
        /// <param name="index">当前使用的路径开始编号。仅在函数递归时使用</param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, object>> FindDicByPath(string[] path, int index = 0)
        {
            if (this == null)
                return null;
            if (index >= path.Length)
            {
                return this.SerializationType;
            }
            if (this.MesValue is Array)
            {
                Array array = this.MesValue as Array;
                foreach (MesProcessData child in array)
                {
                    if (child.MesName == path[index])
                    {
                        return child.FindDicByPath(path, index + 1);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 根据指定路径来修改序列化类型
        /// </summary>
        /// <param name="path">数组路径，从下一级参数开始</param>
        /// <param name="newValue">新的值</param>
        /// <param name="index">当前使用的路径开始编号。仅在函数递归时使用 </param>
        /// <returns></returns>
        public MesProcessData ModifyDicByPath(string[] path, Dictionary<string, Dictionary<string, object>> newValue, int index = 0)
        {
            if (this == null)
                return null;
            if (index >= path.Length)
            {
                this.SerializationType = newValue;
                return this;
            }

            if (this.MesValue is Array array)
            {
                foreach (MesProcessData child in array)
                {
                    if (child.MesName == path[index])
                    {
                        // 递归修改下一级节点
                        child.ModifyValueByPath(path, newValue, index + 1);
                    }
                }
            }
            return this;
        }


    }

}
