using System;

namespace JOJO.Mes.Const
{
    public class MesConst
    {
        /// <summary>
        /// 获取系统当前时间
        /// </summary>
        public DateTime NowTime
        {
            get => DateTime.Now;
        }
        /// <summary>
        /// 获取时间条码，精确到毫秒
        /// </summary>
        public string NowTimeF
        {
            get { return DateTime.Now.ToString("yyyyMMddHHmmssfff"); }
        }
        /// <summary>
        /// 程序名称
        /// </summary>
        public string ProduceName { get; set; } = "";
        /// <summary>
        /// 程序路径
        /// </summary>
        public string ProduceAddress { get; set; } = "";

        private MesEnum.MachineState _ProcessState = MesEnum.MachineState.Null;
        /// <summary>
        /// 设置设备状态,
        /// </summary>
        public MesEnum.MachineState ProcessState
        {
            get
            {
                return _ProcessState;
            }
            set
            {
                _ProcessState = value;
                MesApp.Instance.Mes.ProcessStop(value);
            }
        }
        public TimeSpan CommandOutTime = TimeSpan.FromSeconds(5);
        public const string Bottom = nameof(Bottom);
        public const string Top = nameof(Top);

    }
}
