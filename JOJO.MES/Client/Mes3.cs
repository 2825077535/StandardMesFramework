using JOJO.Mes.CommModel;
using JOJO.Mes.Config;
using JOJO.Mes.Const;
using JOJO.Mes.Log;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace JOJO.Mes.Client
{
    internal class Mes3 : IMesSend
    {
        MesModel MesModel=new MesModel();
        private MesHttp MesHttp=new MesHttp();
        public Mes3 ()
        {
            MesHttp.MesUrlAddress = MesApp.Instance.JOJOMesConfig.MesAddress;
        }
        public async Task<MesProcessData> AlarmInformation(string message, int Level)
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public async Task<MesProcessData> CancelAlarmInformation(string message)
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public async Task<MesProcessData> CheckBoard(MesDynamic dynamic)
        {
            //是否启用mes过站检查
            if (dynamic.IsUse)
            {
                string TimeID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                List<MesProcessData> datas = new List<MesProcessData>();

                MesProcessData data1 = new MesProcessData()
                {
                    MesName = "barCode",
                    MesValue= dynamic.BoardCode
                };
                MesProcessData data2 = new MesProcessData()
                {
                    MesName = "token",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.Token
                };
                MesProcessData data3 = new MesProcessData()
                {
                    MesName = "tecId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.tecId
                };
                MesProcessData data4 = new MesProcessData()
                {
                    MesName = "workOrderId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.workOrderId
                };
                MesProcessData data5 = new MesProcessData()
                {
                    MesName = "workOrderMaterielId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.workOrderMaterielId
                };
                MesProcessData data6 = new MesProcessData()
                {
                    MesName = "workId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.workId
                };
                MesProcessData data7 = new MesProcessData()
                {
                    MesName = "workShopId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.workShopId
                };
                MesProcessData data8 = new MesProcessData()
                {
                    MesName = "pullId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.pullId
                };
                MesProcessData data9 = new MesProcessData()
                {
                    MesName = "equipmentId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.equipmentId
                };
                datas.Add(data1);
                datas.Add(data2);
                datas.Add(data3);
                datas.Add(data4);
                datas.Add(data5);
                datas.Add(data6);
                datas.Add(data7);
                datas.Add(data8);
                datas.Add(data9);
                MesDynamic dynamic1 = new MesDynamic();
                dynamic1.AddressInterface = "checkAvailable";
                List<MesProcessData> Returndatas = new List<MesProcessData>();
                bool SendISOK= await MesSend(datas, dynamic1);

               
                if (Result == null)
                {
                    SendISOK=false;
                   MesLog.Error("发送Http数据，返回结果为Null：");
                }
                else
                {
                    mesHTTPResponse = MesController.DeserializeFromJson<_check_out>(Result);
                    if (mesHTTPResponse.code != 200)
                    {
                        List<MesProcessData> Returndatas = new List<MesProcessData>();
                        Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
                        return MesModel.ReturnData(Returndatas);
                    }
                    else
                    {
                        LogController.Instance.Log(LanguageHelper.GetInfoMessage(InformationCode.InformationCode_10014) + testCheck.barCode);
                    }
                }
               
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);

                
            }
        }
        public async Task<bool> MesSend(List<MesProcessData> datas, MesDynamic dynamic)
        {
            MesProcessData Top = new MesProcessData();
            Top.MesName = "";
            Top.MesValue = datas.ToArray();
            string Json = MesJson.SerializeToJson(Top);
            string Result = await MesHttp.MesUrlSendAndAccept(Json,out HttpResponseMessage response);
            if (Result.code != 200)
            {
                List<MesProcessData> Returndatas = new List<MesProcessData>();
                Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
                return MesModel.ReturnData(Returndatas);
            }

            MesSend_Accept(Top);
            return await AwaitReceive(Top);
        }
        public void CloseMes()
        {
            throw new NotImplementedException();
        }

        public Task<MesProcessData> Dynamic(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }

        public bool MesEnable()
        {
            throw new NotImplementedException();
        }

        public Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }

        public async Task<MesProcessData> OutBoard(MesDynamic dynamic)
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public async Task<MesProcessData> ProcessParameters()
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public async Task<MesProcessData> ProcessStop(MesEnum.MachineState on)
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public async Task<MesProcessData> RemovePCB(MesDynamic data)
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public Task<MesProcessData> Result(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }

        public async Task<MesProcessData> SwitchPrograms()
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }
    }
}
