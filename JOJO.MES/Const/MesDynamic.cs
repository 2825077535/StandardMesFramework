using System;
using System.Collections.Generic;

namespace JOJO.Mes.Const
{
    /// <summary>
    /// 用于函数间的数据中转
    /// </summary>
    public class MesDynamic
    {
        /// <summary>
        /// 程序路径
        /// </summary>
        public string ProduceAddress { get; set; } = "";
        /// <summary>
        /// 程序名称
        /// </summary>
        public string ProduceName { get; set; } = "";
        /// <summary>
        /// OKNG图像存储路径
        /// </summary>
        public string OKNGImageSavePath { get; set; } = "";
        /// <summary>
        /// 当前使用的轨道ID
        /// </summary>
        public string TrackId { get; set; } = "";
        /// <summary>
        /// 整板高度
        /// </summary>
        public double BoardHeight { get; set; } = 0;
        /// <summary>
        /// 整板宽度
        /// </summary>
        public double BoardWidth { get; set; } = 0;
        /// <summary>
        /// B面还是T面
        /// </summary>
        public string BottomTop { get; set; } = MesConst.Bottom;
        /// <summary>
        /// 当前模块是否使用
        /// </summary>
        public bool IsUse {  get; set; }=true;
        /// <summary>
        /// 子板Id
        /// </summary>
        public List<string> SubBoardId { get; set; } = new List<string>();
        /// <summary>
        /// 开始检测时间
        /// </summary>
        public DateTime ProcessStartTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 结束检测时间
        /// </summary>
        public DateTime ProcessEndTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 复判总结果
        /// </summary>
        public string VerifiedBoardResult { get; set; } = "";
        /// <summary>
        /// 设备判断总结果
        /// </summary>
        public string MachineBoardResult { get; set; } = "";
        /// <summary>
        /// 每个子板复判结果
        /// </summary>
        public Dictionary<string, string> VerifiedSubBoardResult { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 每个子板设备判断结果
        /// </summary>
        public Dictionary<string, string> MachineSubBoardResult { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 每个子板的子板条码
        /// </summary>
        public Dictionary<string, string> SubBoardCodes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// NG元件数量
        /// </summary>
        public int NGTPNumber { get; set; } = 0;
        /// <summary>
        /// 获取当前整板检测时间
        /// </summary>
        public double TimeCost { get; set; } = 0.0;
        /// <summary>
        /// 通讯接口
        /// </summary>
        public string AddressInterface { get; set; } = "";
        /// <summary>
        /// 整板条码
        /// </summary>
        public string BoardCode { get; set; } = "";
        public List<string> Codes { get; set; }=new List<string>();
        /// <summary>
        /// 设备ID
        /// </summary>
        public string EquipMentID { get; set; } = "";
        /// <summary>
        /// 校准系统时间
        /// </summary>
        public DateTime ChangeTime { get; set; } = DateTime.Now;
        /// <summary>
        /// TP的数量
        /// </summary>
        public int TPNumber { get; set; } = 0;
        /// <summary>
        /// 登录用户名
        /// </summary>
        public string LoginName { get; set; } = "";
        /// <summary>
        /// 登录用户密码
        /// </summary>
        public string LoginPassword { get; set; } = "";

        /// <summary>
        /// 存放对应NG元件的信息
        /// </summary>
        public List<TPNGCode> TPNGs { get; set; } = new List<TPNGCode>();
        public class TPNGCode
        {
            /// <summary>
            /// 位号
            /// </summary>
            public string TagNumber { get; set; } = "";
            /// <summary>
            /// 料号
            /// </summary>
            public string PartNumber { get; set; } = "";
            /// <summary>
            /// 全部NG编码
            /// </summary>
            public string NGCode { get; set; } = "";
            /// <summary>
            /// 全部NG名称
            /// </summary>
            public List<string> NGCodeName { get; set; } = new List<string>();
            /// <summary>
            /// 子板ID
            /// </summary>
            public string SubBoardId { get; set; } = "";
            /// <summary>
            /// 子板条码
            /// </summary>
            public string SubBoardCode { get; set; } = "";
        }

    }
}
