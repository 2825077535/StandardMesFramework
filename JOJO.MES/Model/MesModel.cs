using JOJO.Mes.Const;
using JOJO.Mes.Log;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JOJO.Mes.Model
{
    public class MesModel
    {
        /// <summary>
        /// 在UI上打印日志
        /// </summary>
        public const string MachineLog = nameof(MachineLog);
        /// <summary>
        /// 设备报警
        /// </summary>
        public const string MachinAlarm = nameof(MachinAlarm);
        /// <summary>
        /// Mes连接
        /// </summary>
        public const string MesConnect = nameof(MesConnect);
        /// <summary>
        /// Mes断开连接
        /// </summary>
        public const string MesDisConnect = nameof(MesDisConnect);
        /// <summary>
        /// Mes控制设备运行
        /// </summary>
        public const string MesMachineRun = nameof(MesMachineRun);
        /// <summary>
        /// Mes控制设备暂停
        /// </summary>
        public const string MesMachineStop = nameof(MesMachineStop);
        public const string MesCommandID = nameof(MesCommandID);
        public const string MesControllerMachineName = nameof(MesControllerMachineName);
        public const string MesCommandValue = nameof(MesCommandValue);

        const string IsOK = nameof(IsOK);
        const string IsEnable = nameof(IsEnable);
        const string SetResult = nameof(SetResult);

        /// <summary>
        /// 打印UI日志
        /// </summary>
        /// <param name="message"></param>
        public void SetMachineLog(string message)
        {
            MesLog.Info("打印设备日志：" + message);
            SetMesControllerMachineResult(MachineLog, message);
            return;
        }
        /// <summary>
        /// 设置设备报警
        /// </summary>
        public void SetMachinAlarm(string Message = "")
        {
            MesLog.Info("设备报警：" + Message);
            SetMesControllerMachineResult(MachinAlarm, Message);
            return;
        }
        /// <summary>
        /// 设置Mes连接
        /// </summary>
        public async Task<bool> SetMesConnect()
        {
            MesLog.Info("Mes已连接");
            return await SetMesControllerMachineResult(MesConnect, "");
        }
        /// <summary>
        /// 设置Mes断开连接
        /// </summary>
        public async Task<bool> SetMesDisConnect()
        {
            MesLog.Info("Mes断开连接");
            return await SetMesControllerMachineResult(MesDisConnect, "");
        }
        /// <summary>
        /// 设置Mes控制设备启动
        /// </summary>
        public async Task<bool> SetMesMachineRun()
        {
            MesLog.Info("Mes控制设备启动");
            return await SetMesControllerMachineResult(MesMachineRun, "");

        }
        /// <summary>
        /// 设置Mes控制设备暂停
        /// </summary>
        public async Task<bool> SetMesMachineStop()
        {
            MesLog.Info("Mes控制设备暂停");
            return await SetMesControllerMachineResult(MesMachineStop, "");
        }
        #region 设置函数返回结果
        /// <summary>
        /// 
        /// </summary>
        /// <param name=Name></param>
        /// <param name="Value"></param>
        /// <param name="NeedReturn"></param>
        /// <returns></returns>
        public async Task<bool> SetMesControllerMachineResult(string Name, object Value)
        {
            await Task.Run(() => 
            {
                return MesApp.Instance.RaiseEvent(Name, Value);
            }); 
            return false;
        }


        /// <summary>
        /// 查找Mes控制设置的指令ID
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public string FindDataCommandID(MesProcessData Data)
        {
            return (string)Data.FindValueByPath(new string[] { MesCommandID });
        }
        /// <summary>
        /// 查找数据中是否有Mes控制设备的指令名称
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public string FindDataMesMachineName(MesProcessData Data)
        {
            return (string)Data.FindValueByPath(new string[] { MesControllerMachineName });
        }
        /// <summary>
        /// 查找数据中是否有Mes控制设备的指令值
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public object FindDataMesCommandValue(MesProcessData Data)
        {
            return Data.FindValueByPath(new string[] { MesCommandValue });
        }

        /// <summary>
        /// 设置函数执行状态
        /// </summary>
        /// <param name="isOK"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        public MesProcessData SetResultOKEnable(bool isOK = true, bool isEnable = true)
        {
            MesProcessData Enable = new MesProcessData();
            Enable.MesName = IsEnable;
            Enable.MesValue = isEnable;
            MesProcessData OK = new MesProcessData();
            OK.MesName = IsOK;
            OK.MesValue = isOK;
            List<MesProcessData> list = new List<MesProcessData>();
            list.Add(Enable);
            list.Add(OK);
            MesProcessData data = new MesProcessData();
            data.MesName = SetResult;
            data.MesValue = list.ToArray();
            return data;
        }

        /// <summary>
        /// 查找结果是否OK
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool FindDataIsOK(MesProcessData Data)
        {
            return (bool)Data.FindValueByPath(new string[] { SetResult, IsOK });
        }
        /// <summary>
        /// 查找结果是否允许使用
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool FindDataIsEnable(MesProcessData Data)
        {
            return (bool)Data.FindValueByPath(new string[] { SetResult, IsEnable });
        }
        #endregion

        /// <summary>
        /// 编辑返回数据
        /// </summary>
        /// <param name="Returndatas"></param>
        /// <returns></returns>
        public MesProcessData ReturnData(List<MesProcessData> Returndatas)
        {
            MesProcessData Returndata = new MesProcessData();
            Returndata.MesValue = Returndatas.ToArray();
            return Returndata;
        }
    }
    /// <summary>
    /// 用于矫正系统时间
    /// </summary>
    internal class UpdateTime
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SYSTEMTIME time);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }

        public static bool SetDate(DateTime dt)
        {
            SYSTEMTIME st;

            st.year = (short)dt.Year;
            st.month = (short)dt.Month;
            st.dayOfWeek = (short)dt.DayOfWeek;
            st.day = (short)dt.Day;
            st.hour = (short)dt.Hour;
            st.minute = (short)dt.Minute;
            st.second = (short)dt.Second;
            st.milliseconds = (short)dt.Millisecond;
            bool rt = SetLocalTime(ref st);
            return rt;
        }
    }
}
