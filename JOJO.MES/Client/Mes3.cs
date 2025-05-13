using JOJO.Mes.CommModel;
using JOJO.Mes.Config;
using JOJO.Mes.Const;
using JOJO.Mes.Log;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JOJO.Mes.Client
{
    internal class Mes3 : IMesSend
    {
        MesModel MesModel=new MesModel();
        private MesHttp MesHttp=new MesHttp();
        // 或者完整属性实现
        private string _token = "";
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
            }
        }
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
                    MesValue = Token
                };
                MesProcessData data3 = new MesProcessData()
                {
                    MesName = "tecId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.TecId
                };
                MesProcessData data4 = new MesProcessData()
                {
                    MesName = "workOrderId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderId
                };
                MesProcessData data5 = new MesProcessData()
                {
                    MesName = "workOrderMaterielId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderMaterielId
                };
                MesProcessData data6 = new MesProcessData()
                {
                    MesName = "workId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkId
                };
                MesProcessData data7 = new MesProcessData()
                {
                    MesName = "workShopId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkShopId
                };
                MesProcessData data8 = new MesProcessData()
                {
                    MesName = "pullId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.PullId
                };
                MesProcessData data9 = new MesProcessData()
                {
                    MesName = "equipmentId",
                    MesValue = MesApp.Instance.JOJOMesConfig.Mes3.EquipmentId
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

                MesProcessData data = await MesSend(datas, dynamic);
                List<MesProcessData> Returndatas = new List<MesProcessData>();
                if (data.FindValueByPath(new string[] {"Code"}).ToString()=="200")
                {
                    Returndatas.Add(MesModel.SetResultOKEnable(true));
                }
                else
                {
                    string Message= data.FindValueByPath(new string[] { "Message" }).ToString();
                    MesLog.Error(Message);
                    MesModel.SetMachineLog(Message);
                    Returndatas.Add(MesModel.SetResultOKEnable(false));
                }
                return MesModel.ReturnData(Returndatas);
            }
            else
            {
                List<MesProcessData> Returndatas = new List<MesProcessData>();
                Returndatas.Add(MesModel.SetResultOKEnable(false,false));
                return MesModel.ReturnData(Returndatas);
            }
        }
        public async Task<MesProcessData> MesSend(List<MesProcessData> datas, MesDynamic dynamic)
        {
            if (dynamic.AddressInterface== "getLogin")
            {
                MesHttp.UseToken = true;
            }
            else MesHttp.UseToken = false;
            MesHttp.AccessInterface= dynamic.AddressInterface;
            MesProcessData Top = new MesProcessData();
            Top.MesName = "";
            Top.MesValue = datas.ToArray();
            string Json = MesJson.Serialize(Top);
            string Result = await MesHttp.MesUrlSendAndAccept(Json);
            return MesJson.DeserializeJson(Result);
        }
        public void CloseMes()
        {
            MesHttp.Close();
        }

        public async Task<MesProcessData> Dynamic(MesDynamic dynamic)
        {
            string MesInterface=dynamic.AddressInterface;
            List<MesProcessData> datas = new List<MesProcessData>();
            MesProcessData data=new MesProcessData();
            switch (MesInterface)
            {
                case "getTecList":
                    datas.Add(new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    break;
                case "getWorkOrderList":
                    datas.Add( new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    datas.Add(new MesProcessData()
                    {
                        MesName = "workOrderMaterielId",
                        MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderMaterielId,
                    });
                    break;
                case "getMaterielList":
                    datas.Add(new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    datas.Add(new MesProcessData()
                    {
                        MesName = "tecId",
                        MesValue = MesApp.Instance.JOJOMesConfig.Mes3.TecId,
                    });
                    break;
                case "getWorkList":
                    datas.Add(new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    datas.Add(new MesProcessData()
                    {
                        MesName = "workOrderMaterielId",
                        MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderMaterielId,
                    });
                    datas.Add(new MesProcessData()
                    {
                        MesName = "workOrderId",
                        MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderId,
                    });
                    datas.Add(new MesProcessData()
                    {
                        MesName = "tecId",
                        MesValue = MesApp.Instance.JOJOMesConfig.Mes3.TecId,
                    });
                    break;
                case "getWorkShopList":
                    datas.Add(new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    break;
                case "getPullList":
                    datas.Add(new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    datas.Add(new MesProcessData()
                    {
                        MesName = "workShopId",
                        MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkShopId,
                    });
                    break;
                case "getEquipmentList":
                    datas.Add(new MesProcessData()
                    {
                        MesName = "token",
                        MesValue = Token,
                    });
                    break;
                default:
                    break;
            }
            data = await MesSend(datas, dynamic);
            object Datas=new object();
            switch (MesInterface)
            {
                case "getTecList":
                    Datas=data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas1)
                    {
                        foreach (MesProcessData Data in ArrayDatas1)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.TecIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "tec_name" }).ToString(),ID= int.Parse( Data.FindValueByPath(new string[] { "tec_id" }).ToString()) });
                        }
                    }
                    break;
                case "getWorkOrderList":
                    Datas = data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas2)
                    {
                        foreach (MesProcessData Data in ArrayDatas2)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "materiel_erp_code" }).ToString(), ID = int.Parse(Data.FindValueByPath(new string[] { "materiel_id" }).ToString()) });
                        }
                    }
                    break;
                case "getMaterielList":
                    Datas = data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas3)
                    {
                        foreach (MesProcessData Data in ArrayDatas3)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderMaterielIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "work_order_nubmer" }).ToString(), ID = int.Parse(Data.FindValueByPath(new string[] { "work_order_id" }).ToString()) });
                        }
                    }
                    break;
                case "getWorkList":
                    Datas = data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas4)
                    {
                        foreach (MesProcessData Data in ArrayDatas4)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.WorkIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "work_name" }).ToString(), ID = int.Parse(Data.FindValueByPath(new string[] { "work_id" }).ToString()) });
                        }
                    }
                    break;
                case "getWorkShopList":
                    Datas = data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas5)
                    {
                        foreach (MesProcessData Data in ArrayDatas5)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.WorkShopIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "workshop_name" }).ToString(), ID = int.Parse(Data.FindValueByPath(new string[] { "workshop_id" }).ToString()) });
                        }
                    }
                    break;
                case "getPullList":
                    Datas = data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas6)
                    {
                        foreach (MesProcessData Data in ArrayDatas6)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.WorkShopIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "label_name" }).ToString(), ID = int.Parse(Data.FindValueByPath(new string[] { "label_id" }).ToString()) });
                        }
                    }
                    break;
                case "getEquipmentList":
                    Datas = data.FindValueByPath(new string[] { "data" });
                    if (Datas is Array ArrayDatas7)
                    {
                        foreach (MesProcessData Data in ArrayDatas7)
                        {
                            MesApp.Instance.JOJOMesConfig.Mes3.WorkShopIdList.Add(new CustomerConfig.Mes3.MesShow() { Name = Data.FindValueByPath(new string[] { "equipment_name" }).ToString(), ID = int.Parse(Data.FindValueByPath(new string[] { "equipment_id" }).ToString()) });
                        }
                    }
                    break;
                default:
                    break;
            }
            List<MesProcessData> Returndatas = new List<MesProcessData>();
            if (data.FindValueByPath(new string[] { "Code" }).ToString() == "200")
            {
                Returndatas.Add(MesModel.SetResultOKEnable(true));
                Returndatas.Add(data);
            }
            else
            {
                string Message = data.FindValueByPath(new string[] { "Message" }).ToString();
                MesLog.Error(Message);
                MesModel.SetMachineLog(Message);
                Returndatas.Add(MesModel.SetResultOKEnable(false));
            }
            return MesModel.ReturnData(Returndatas);
        }

        public bool MesEnable()
        {
            return MesApp.Instance.JOJOMesConfig.IsEnableMes;
        }

        public async Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            MesProcessData data1 = new MesProcessData()
            {
                MesName = "user",
                MesValue = dynamic.LoginName
            };
            MesProcessData data2 = new MesProcessData()
            {
                MesName = "password",
                MesValue = dynamic.LoginPassword
            };
            datas.Add(data1);
            datas.Add(data2);
            
            dynamic.AddressInterface = "getLogin";
            MesProcessData data = await MesSend(datas, dynamic);
            List<MesProcessData> Returndatas = new List<MesProcessData>();
            if (data.FindValueByPath(new string[] { "Code" }).ToString() == "200")
            {
                Token = data.FindValueByPath(new string[] { "Token" }).ToString();
                Returndatas.Add(MesModel.SetResultOKEnable(true));
            }
            else
            {
                string Message = data.FindValueByPath(new string[] { "Message" }).ToString();
                MesLog.Error(Message);
                MesModel.SetMachineLog(Message);
                Returndatas.Add(MesModel.SetResultOKEnable(false));
            }
            return MesModel.ReturnData(Returndatas);
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

        public async Task<MesProcessData> Result(MesDynamic dynamic)
        {
            string MesInterface = dynamic.AddressInterface;
            List<MesProcessData> datas = new List<MesProcessData>();
            MesProcessData data = new MesProcessData();
            datas.Add(new MesProcessData()
            {
                MesName = "token",
                MesValue = Token,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "tecId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.TecId,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "workOrderMaterielId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderMaterielId,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "workOrderId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderId,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "workId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkId,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "workShopId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.WorkShopId,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "pullId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.PullId,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "barCode",
                MesValue = dynamic.BoardCode,
            });
            datas.Add(new MesProcessData()
            {
                MesName = "equipmentId",
                MesValue = MesApp.Instance.JOJOMesConfig.Mes3.EquipmentId,
            });
            List<MesProcessData> uploadData=new List<MesProcessData>();
            uploadData.Add(new MesProcessData()
            {
                MesName = "detection",
                MesValue = dynamic.MachineBoardResult=="1"?"OK":"NG",
            });
            uploadData.Add(new MesProcessData()
            {
                MesName = "filePath",
                MesValue = dynamic.OKNGImageSavePath,
            });
            List<MesProcessData> header = new List<MesProcessData>();
            header.Add(new MesProcessData()
            {
                MesName = "ProgramName",
                MesValue = dynamic.ProduceName,
            });
            header.Add(new MesProcessData()
            {
                MesName = "LineCode",
                MesValue = dynamic.TrackId,
            });
            header.Add(new MesProcessData()
            {
                MesName = "TotalElement",
                MesValue = dynamic.TPNumber.ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "TotalBoardPart",
                MesValue = dynamic.SubBoardId.Count.ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "GoodQuantityByStation",
                MesValue = (dynamic.TPNumber-dynamic.NGTPNumber).ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "DefectiveQuantityByStation",
                MesValue = dynamic.NGTPNumber.ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "BeginTestTime",
                MesValue = dynamic.ProcessStartTime.ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "EndTestTime",
                MesValue = dynamic.ProcessEndTime.ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "TestTime",
                MesValue = dynamic.TimeCost.ToString(),
            });
            header.Add(new MesProcessData()
            {
                MesName = "Size",
                MesValue = dynamic.BoardHeight.ToString()+"*"+dynamic.BoardWidth.ToString(),
            });
            List<MesProcessData> details = new List<MesProcessData>();
            List<MesProcessData> detail = new List<MesProcessData>();
            foreach (var item in dynamic.VerifiedSubBoardResult)
            {
                detail.Add(new MesProcessData()
                {
                    MesName = "ProduceCode",
                    MesValue = dynamic.SubBoardCodes[item.Key],
                });
                detail.Add(new MesProcessData()
                {
                    MesName = "PartCode",
                    MesValue =  item.Key,
                });
                detail.Add(new MesProcessData()
                {
                    MesName = "ResultByStation",
                    MesValue = dynamic.VerifiedSubBoardResult[item.Key] =="0"?"2":"1",
                });
                var TNNgCodeList = dynamic.TPNGs.AsParallel().
                                                Where(x => x.SubBoardId == item.Key && x.NGCodeName.Count>0).
                                                Select(x =>
                                                {
                                                    string StrRe = "";
                                                    foreach (var item in x.NGCodeName)
                                                    {
                                                        StrRe= string.Join(",", x.PartNumber + "-" + x.TagNumber + "_" + item);
                                                    }
                                                    return StrRe;
                                                }).ToList();

                detail.Add(new MesProcessData()
                {
                    MesName = "NGCode",
                    MesValue = string.Join(",", TNNgCodeList),
                });
                details.Add(new MesProcessData() { MesName = "", MesValue = detail.ToArray() });
            }
            List<MesProcessData> detectionDeatails =new List<MesProcessData>();
            detectionDeatails.Add(new MesProcessData() { MesName="heard",MesValue=header.ToArray()});
            detectionDeatails.Add(new MesProcessData() { MesName = "details", MesValue = details.ToArray() });
            uploadData.Add(new MesProcessData() { MesName = "detectionDeatails", MesValue = detectionDeatails.ToArray() });
            datas.Add(new MesProcessData() { MesName = "uploadData", MesValue = uploadData.ToArray() });
            data = await MesSend(datas, dynamic);
            List<MesProcessData> Returndatas = new List<MesProcessData>();
            if (data.FindValueByPath(new string[] { "Code" }).ToString() == "200")
            {
                Returndatas.Add(MesModel.SetResultOKEnable(true));
            }
            else
            {
                string Message = data.FindValueByPath(new string[] { "Message" }).ToString();
                MesLog.Error(Message);
                MesModel.SetMachineLog(Message);
                Returndatas.Add(MesModel.SetResultOKEnable(false));
            }
            return MesModel.ReturnData(Returndatas);
        }

        public async Task<MesProcessData> SwitchPrograms()
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }
    }
}
