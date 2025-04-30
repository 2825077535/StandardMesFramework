using JOJO.Mes.Const;
using System.Threading.Tasks;

namespace JOJO.Mes
{
    public interface IMesSend
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns></returns>
        Task<MesProcessData> MesLogin(MesDynamic dynamic);
        /// <summary>
        /// 上报拿板情况
        /// </summary>
        Task<MesProcessData> RemovePCB(MesDynamic data);
        /// <summary>
        /// 上传工艺参数
        /// </summary>
        Task<MesProcessData> ProcessParameters();
        /// <summary>
        /// 上传整板测试结果
        /// </summary>
        Task<MesProcessData> Result(MesDynamic dynamic);
        /// <summary>
        /// Mes启用
        /// </summary>
        /// <returns></returns>
        bool MesEnable();
        /// <summary>
        /// 上传报警信息,
        /// </summary>
        /// <param name="message"></param>
        /// <param name="Level">级别：1为警告黄灯，2为红灯报警</param>
        Task<MesProcessData> AlarmInformation(string message, int Level);
        /// <summary>
        /// 消除报警信息
        /// </summary>
        /// <param name="message"></param>
        Task<MesProcessData> CancelAlarmInformation(string message);
        /// <summary>
        /// 发送设备状态
        /// </summary>
        Task<MesProcessData> ProcessStop(MesEnum.MachineState on);
        /// <summary>
        /// 切换程序
        /// </summary>
        Task<MesProcessData> SwitchPrograms();
        /// <summary>
        /// 过站检测
        /// </summary>
        /// <param name="BoardCode"></param>
        /// <returns></returns>
        Task<MesProcessData> CheckBoard(MesDynamic dynamic);
        /// <summary>
        /// 设备出板
        /// </summary>
        /// <returns></returns>
        Task<MesProcessData> OutBoard(MesDynamic dynamic);
        /// <summary>
        /// 关闭Mes
        /// </summary>
        void CloseMes();
        /// <summary>
        /// 动态接口，用于在特殊情况下调用的接口
        /// </summary>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        Task<MesProcessData> Dynamic(MesDynamic dynamic);
    }
}
