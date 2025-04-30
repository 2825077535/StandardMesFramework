using Common;
using JOJO.Mes.CommModel;
using JOJO.Mes.Const;
using JOJO.Mes.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JOJO.Mes.Client
{
    /// <summary>
    /// 迪比科AOI
    /// </summary>
    internal class DiBiKe : IMesSend
    {
        MesHttp Http = new MesHttp();
        private string tecId { get; set; } = "";
        private string workOrderId { get; set; } = "";
        private string workOrderMaterielId { get; set; } = "";
        private string workId { get; set; } = "";
        private string workShopId { get; set; } = "";
        private string pullId { get; set; } = "";
        private string equipmentId { get; set; } = "";
        public DiBiKe()
        {
            Http.MesUrlAddress = MesApp.Instance.JOJOMesConfig.MesAddress;
            Http.MesUrlTimeOut = MesApp.Instance.JOJOMesConfig.MesTimeOut;

        }
        public Task<bool> AlarmInformation(string message, int Level)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CancelAlarmInformation(string message)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> CheckBoard(MesDynamic dynamic)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = "barCode", MesValue = dynamic.BoardCode });
            datas.Add(new MesProcessData { MesName = "token", MesValue = Http.Token });
            datas.Add(new MesProcessData { MesName = "tecId", MesValue = tecId });
            datas.Add(new MesProcessData { MesName = "workOrderId", MesValue = workOrderId });
            datas.Add(new MesProcessData { MesName = "workOrderMaterielId", MesValue = workOrderMaterielId });
            datas.Add(new MesProcessData { MesName = "workId", MesValue = workId });
            datas.Add(new MesProcessData { MesName = "workShopId", MesValue = workShopId });
            datas.Add(new MesProcessData { MesName = "pullId", MesValue = pullId });
            datas.Add(new MesProcessData { MesName = "equipmentId", MesValue = equipmentId });

            MesProcessData data = new MesProcessData();
            data.MesName = "";
            data.MesValue = datas.ToArray();

            string result = MesJson.SerializeToJson(data);


            string Result = await Http.MesUrlSendAndAccept(result);

            if (Result != null)
            {
                MesProcessData mesHTTPResponse = new MesProcessData();
                mesHTTPResponse = MesJson.DeserializeJson(Result);
                if (mesHTTPResponse.FindValueByPath(new string[] { "code" }).ToString() != "200")
                {
                    MesApp.Instance.Const.SetMachineLog(LanguageHelper.GetInfoMessage(InformationCode.InformationCode_10012));
                    MesApp.Instance.Const.SetMachineLog(LanguageHelper.GetInfoMessage(InformationCode.InformationCode_10013) +
                        mesHTTPResponse.FindValueByPath(new string[] { "msg" }).ToString());

                    return false;
                }
                else
                {
                    MesApp.Instance.Const.SetMachineLog(LanguageHelper.GetInfoMessage(InformationCode.InformationCode_10014) +
                       dynamic.BoardCode);
                }
            }

            return true;
        }

        public void CloseMes()
        {
            Http.Close();
        }

        public async Task<MesProcessData> Dynamic(MesDynamic dynamic)
        {
            string AccessInterface = dynamic.AddressInterface;
            MesProcessData data = new MesProcessData();
            data.MesName = "token";
            data.MesValue = Http.Token;

            switch (AccessInterface)
            {
                case "getTecList":
                    string getWorkOrderMaterielList = MesJson.SerializeToJson(new MesProcessData { MesValue = data });
                    string Result = await Http.MesUrlSendAndAccept(getWorkOrderMaterielList);
                    if (Result == null)
                    {
                        MesApp.Instance.Const.SetMachineLog("");

                        return data;
                    }


                    break;
                default:
                    break;
            }
            return new MesProcessData();
        }

        public bool MesEnable()
        {
            return MesApp.Instance.JOJOMesConfig.IsEnableMes;
        }
        public async Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            bool Enable = false;
            string ErrorMessage = "";
            MesProcessData data = new MesProcessData();

            MesProcessData data1 = new MesProcessData();
            data1.MesName = dynamic.LoginName;
            data1.MesValue = dynamic.LoginPassword;

            data.MesName = "";
            data.MesValue = data1;
            string loginCheck = MesJson.SerializeToJson(data);

            Http.AccessInterface = "getLogin";
            string Result = await Http.MesUrlSendAndAccept(loginCheck);

            MesProcessData result = new MesProcessData();
            List<MesProcessData> resultList1 = new List<MesProcessData>();
            resultList1.Add(new MesProcessData { MesName = "IsEnable", MesValue = true });

            if (Result == null)
            {
                ErrorMessage = LanguageHelper.GetInfoMessage(InformationCode.InformationCode_10000);

                resultList1.Add(new MesProcessData { MesName = "IsOk", MesValue = false });
                resultList1.Add(new MesProcessData { MesName = "ErrorMessage", MesValue = ErrorMessage });
                result.MesValue = resultList1.ToArray();
                return result;
            }
            MesProcessData mesHTTPResponse = new MesProcessData();
            mesHTTPResponse = MesJson.DeserializeJson(Result);
            if (mesHTTPResponse == null)
            {
                resultList1.Add(new MesProcessData { MesName = "IsOk", MesValue = "NG" });
                resultList1.Add(new MesProcessData { MesName = "ErrorMessage", MesValue = ErrorMessage });
                result.MesValue = resultList1.ToArray();
                return result;
            }
            if (mesHTTPResponse.FindValueByPath(new string[] { "code" }).ToString() != "200")
            {
                resultList1.Add(new MesProcessData { MesName = "IsOk", MesValue = "NG" });
                resultList1.Add(new MesProcessData { MesName = "ErrorMessage", MesValue = ErrorMessage });
                result.MesValue = resultList1.ToArray();
                return result;
            }
            Http.Token = mesHTTPResponse.FindValueByPath(new string[] { "token" }).ToString();

            MesDynamic dynamicTecList = new MesDynamic();
            dynamicTecList.AddressInterface = "getTecList";

            return result;

        }

        public Task<bool> OutBoard(MesDynamic dynamic)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessDateTime()
        {
            throw new System.NotImplementedException();
        }

        public void ProcessParameters()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ProcessStop(MesEnum.MachineState on)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemovePCB(MesDynamic data)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Result(MesDynamic dynamic)
        {
            throw new System.NotImplementedException();
        }

        public void SwitchPrograms()
        {
            throw new System.NotImplementedException();
        }
    }
}
