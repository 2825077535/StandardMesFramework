using JOJO.Mes.Const;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JOJO.Mes.Client
{
    /// <summary>
    /// 华通炉前AOI
    /// </summary>
    internal class HuaTong : IMesSend
    {
        BasicHttpBinding binding = new BasicHttpBinding();
        IServiceContract channel;
        ChannelFactory<IServiceContract> factory;
        const string getEmapFromSer = "getEmapFromSer";
        const string LinerCCDUpload = "LinerCCDUpload";
        string targetAddress = MesApp.Instance.JOJOMesConfig.MesAddress;
        List<int> SubBoardNoCheck = new List<int>();

        public HuaTong()
        {
            // 创建自定义绑定
            // 设置绑定的属性，例如超时等，这里使用默认值
            binding.CloseTimeout = TimeSpan.FromMinutes(MesApp.Instance.JOJOMesConfig.MesTimeOut);
            binding.OpenTimeout = TimeSpan.FromMinutes(MesApp.Instance.JOJOMesConfig.MesTimeOut);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(MesApp.Instance.JOJOMesConfig.MesTimeOut);
            binding.SendTimeout = TimeSpan.FromMinutes(MesApp.Instance.JOJOMesConfig.MesTimeOut);

            // 创建服务端点地址
            EndpointAddress address = new EndpointAddress(MesApp.Instance.JOJOMesConfig.MesAddress);

            // 创建通道工厂，这里的 IServiceContract 是你要调用的服务契约接口
            factory = new ChannelFactory<IServiceContract>(binding, address);

            // 创建服务通道
            channel = factory.CreateChannel();


        }

        // 服务契约接口，需要与服务端的服务契约保持一致
        [ServiceContract]
        interface IServiceContract
        {
            /// <summary>
            /// 过站检查
            /// </summary>
            /// <param name="p_barcode"></param>
            /// <param name="p_process"></param>
            /// <param name="p_type"></param>
            /// <returns></returns>
            [OperationContract]
            string getEmapFromSer(string p_barcode, string p_process, string p_type);
            /// <summary>
            /// 检验指定位号是否检测
            /// </summary>
            /// <param name="p_barcode"></param>
            /// <param name="p_ng_no"></param>
            /// <param name="p_line"></param>
            /// <param name="p_mc"></param>
            /// <param name="p_type"></param>
            /// <returns></returns>
            [OperationContract]
            string LinerCCDUpload(string p_barcode, string p_ng_no, string p_line, string p_mc, string p_type);
        }

        Task<bool> IMesSend.RemovePCB(MesDynamic data)
        {
            return Task.FromResult(true);
        }

        void IMesSend.ProcessParameters()
        {
        }

        Task<bool> IMesSend.Result(MesDynamic dynamic)
        {
            return Task.FromResult(true);
        }



        bool IMesSend.MesEnable()
        {
            return MesApp.Instance.JOJOMesConfig.IsEnableMes;
        }

        Task<bool> IMesSend.AlarmInformation(string message, int Level)
        {
            return Task.FromResult(true);
        }

        Task<bool> IMesSend.CancelAlarmInformation(string message)
        {
            return Task.FromResult(true);

        }

        Task<bool> IMesSend.ProcessStop(MesEnum.MachineState on)
        {
            return Task.FromResult(true);

        }


        void IMesSend.SwitchPrograms()
        {

        }

        Task<bool> IMesSend.CheckBoard(MesDynamic dynamic)
        {
            string Result = channel.getEmapFromSer(dynamic.BoardCode, dynamic.ProduceName, "");
            MesProcessData data = MesXml.DeserializeXml(Result);
            string re = data.FindValueByPath(new string[] { }).ToString();
            if (re.Contains("OK"))
            {
                string[] numberStrings = Regex.Matches(re, @"\d+")
                       .Cast<Match>()
                       .Select(m => m.Value)
                       .ToArray();
                // 将数字字符串数组转换为整数数组
                SubBoardNoCheck = numberStrings.Select(numStr => int.Parse(numStr))
                                         .ToList();

                return Task.FromResult(true);
            }

            return Task.FromResult(false);

        }

        Task<bool> IMesSend.OutBoard(MesDynamic dynamic)
        {
            return Task.FromResult(true);

        }



        public void CloseMes()
        {
            // 关闭通道和工厂
            ((ICommunicationObject)channel).Close();
            factory.Close();
        }

        public MesDynamic Dynamic(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }

        public Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }

        Task<MesProcessData> IMesSend.Dynamic(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }
    }
}
