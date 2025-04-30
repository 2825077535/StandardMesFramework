using JOJO.Mes.Const;
using JOJO.Mes.Log;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JOJO.Mes.Client
{
    /// <summary>
    /// Mes2_webSeverAOI
    /// </summary>
    internal class Mes2_webSever : IMesSend
    {
        MesModel MesModel=new MesModel();
        BasicHttpBinding binding = new BasicHttpBinding();
        Service1Soap channel;
        ChannelFactory<Service1Soap> factory;
        public Mes2_webSever()
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
            factory = new ChannelFactory<Service1Soap>(binding, address);
            // 创建服务通道
            channel = factory.CreateChannel();
        }

        // 服务契约接口，需要与服务端的服务契约保持一致
        [ServiceContract]
        interface Service1Soap
        {
            /// <summary>
            /// 过站检查
            /// </summary>
            /// <param name="p_barcode"></param>
            /// <param name="p_process"></param>
            /// <param name="p_type"></param>
            /// <returns></returns>
            [OperationContract(Action = "http://tempuri.org/getEmapFromSer")]
            string getEmapFromSer(string p_barcode, string p_process, string p_type);
            /// <summary>
            /// 上传检测结果，NG子板的编号
            /// </summary>
            /// <param name="p_barcode"></param>
            /// <param name="p_ng_no"></param>
            /// <param name="p_line"></param>
            /// <param name="p_mc"></param>
            /// <param name="p_type"></param>
            /// <returns></returns>
            [OperationContract(Action = "http://tempuri.org/LinerCCDUpload")]
            string LinerCCDUpload(string p_barcode, string p_ng_no, string p_line, string p_mc, string p_type);
        }
        public Task<MesProcessData> Result(MesDynamic dynamic)
        {
            try
            {
                List<string> NGSubBoardId = new List<string>();
                foreach (var item in dynamic.TPNGs)
                {
                    if (!NGSubBoardId.Contains(item.SubBoardId)&&item.SubBoardId!= "Board")
                    {
                        NGSubBoardId.Add(Regex.Match(item.SubBoardId, @"\d+").Value);
                    }
                }
                string result = string.Join(",", NGSubBoardId);
                MesLog.Info($"发送Mes2检测数据：{dynamic.BoardCode},{result},{MesApp.Instance.JOJOMesConfig.Mes2.LineName},{MesApp.Instance.JOJOMesConfig.EquipmentID}");
                var Result = channel.LinerCCDUpload(dynamic.BoardCode, result, MesApp.Instance.JOJOMesConfig.Mes2.LineName,
                    MesApp.Instance.JOJOMesConfig.EquipmentID, "");
                MesLog.Info("接收Mes2检测数据：" + Result);
                if (Result.Contains("NG"))
                {
                    MesModel.SetMachineLog("Mes2检测数据NG:" + Result);
                }
                return Task.FromResult(new MesProcessData() { MesValue=MesModel.SetResultOKEnable()});
            }
            catch (Exception ex)
            {
                MesLog.Error("接收Mes2检测数据异常:" + ex.ToString());
                MesModel.SetMachineLog("接收Mes2检测数据异常");
                return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false) });
            }
        }
        public bool MesEnable()
        {
            return MesApp.Instance.JOJOMesConfig.IsEnableMes;
        }
        public Task<MesProcessData> CheckBoard(MesDynamic dynamic)
        {
            MesProcessData data = new MesProcessData();
            List<MesProcessData> datas = new List<MesProcessData>();
            try
            {
                MesLog.Info($"发送Mes2条码校验：{dynamic.BoardCode},linerccd");
                var Result = channel.getEmapFromSer(dynamic.BoardCode, "linerccd", "");
                MesLog.Info("接收Mes2条码校验：" + Result);
                MesProcessData Str = new MesProcessData ();
                Str.MesName = "HuaTong_getEmapFromSer";
                Str.MesValue = Result;
                datas.Add(Str);
                if (Str.MesValue.ToString().Contains("OK"))
                {
                    datas.Add(new MesProcessData { MesValue = MesModel.SetResultOKEnable() });
                    data.MesValue = datas.ToArray();
                    return Task.FromResult(data);
                }
                else
                {
                    datas.Add(new MesProcessData { MesValue = MesModel.SetResultOKEnable(false) });
                    data.MesValue = datas.ToArray();
                    MesModel.SetMachineLog($"Mes2Mes条码校验失败：{Result}");
                }
                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                MesModel.SetMachineLog("Mes2Mes条码校验失败");
                MesLog.Error("Mes2条码校验失败" + ex.ToString());
                datas.Add(new MesProcessData { MesName = "IsOk", MesValue = false });
                data.MesValue = datas.ToArray();
                return Task.FromResult(data);
            }
        }

        public void CloseMes()
        {
            // 关闭通道和工厂
            ((ICommunicationObject)channel).Close();
            factory.Close();
        }

        public Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> RemovePCB(MesDynamic data)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> ProcessParameters()
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> AlarmInformation(string message, int Level)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> CancelAlarmInformation(string message)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> ProcessStop(MesEnum.MachineState on)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> SwitchPrograms()
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> OutBoard(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }

        public Task<MesProcessData> Dynamic(MesDynamic dynamic)
        {
            return Task.FromResult(new MesProcessData() { MesValue = MesModel.SetResultOKEnable(false, false) });
        }
    }
}
