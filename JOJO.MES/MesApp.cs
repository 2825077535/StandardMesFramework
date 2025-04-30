using JOJO.Mes.Client;
using JOJO.Mes.Config;
using JOJO.Mes.Const;
using JOJO.Mes.Log;
using JOJO.Mes.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JOJO.Mes
{
    public class MesApp
    {
        #region Singleton

        private static MesApp _instance = null;
        private static readonly object Lock = new object();
        private MesConst _Const = new MesConst();
        private IMesSend _Mes = null;
        private MesEnum _MesEnum = new MesEnum();
        private JOJOMesConfig _MesConfig = new JOJOMesConfig();
        private string EnvironmentAddress = Path.Combine(Environment.CurrentDirectory, "Config");
        private string ConfigPath = Path.Combine(Environment.CurrentDirectory, "Config\\MesConfig.txt");

        #region Mes控制设备
        public delegate bool MachineEventHandler(object sender, MesControllerMachine e);
        public event MachineEventHandler MachineEvent;
        internal bool RaiseEvent(string Name, object Value)
        {
            var args = new MesControllerMachine(Name, Value);
            return MachineEvent.Invoke(this, args);
        }
        #endregion
        public MesModel MesModel { get; } = new MesModel();
        public JOJOMesConfig JOJOMesConfig
        {
            get => _MesConfig;
            set => _MesConfig = value;
        }
        public MesEnum MesEnum
        {
            get => _MesEnum;
        }
        public MesConst Const
        {
            get => _Const;
            set => _Const = value;
        }

        public IMesSend Mes
        {
            get => _Mes;
            set => _Mes = value;
        }
        public static MesApp Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MesApp();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion
        /// <summary>
        /// 创建Mes对象
        /// </summary>
        public bool CreatMes()
        {
            try
            {
                MesEnum.MesCustomer customer = new MesEnum.MesCustomer();
                customer = MesEnum.GetEnumValueFromDescription<MesEnum.MesCustomer>(MesApp.Instance.JOJOMesConfig.SelectCustomer);
                if (!MesApp.Instance.JOJOMesConfig.IsEnableMes)
                {
                    customer = MesEnum.MesCustomer.None;
                }
                switch (customer)
                {
                    case MesEnum.MesCustomer.Mes2:
                        Mes = new Mes2();
                        break;
                    default:
                        Mes = new DefaultMes();
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool MesEnable()
        {
            if (Mes == null)
            {
                return false;
            }
            else
            {
                return Mes.MesEnable();
            }
        }
        public bool SaveMesConfig()
        {
            // 检查 config 文件夹是否存在
            if (!Directory.Exists(EnvironmentAddress))
            {
                try
                {
                    // 创建 config 文件夹
                    Directory.CreateDirectory(EnvironmentAddress);
                    string json = JsonConvert.SerializeObject(MesApp.Instance.JOJOMesConfig, Formatting.Indented);
                    File.WriteAllText(ConfigPath, json);
                    return true;
                }
                catch (Exception ex)
                {
                    MesLog.Error("配置参数序列化失败:" + ex.ToString());
                    return false;
                }
            }
            else
            {
                string json = JsonConvert.SerializeObject(MesApp.Instance.JOJOMesConfig, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                return true;
            }
        }

        public bool GetMesConfig()
        {
            if (!Directory.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    MesApp.Instance.JOJOMesConfig = MesJson.DeserializeObject<JOJOMesConfig>(json);
                }
                catch (Exception ex)
                {
                    MesLog.Error("配置参数反序列化失败:" + ex.ToString());
                }
            }
            else
            {
                return false;
            }
            return true;
        }

    }
    public class MesControllerMachine : EventArgs
    {
        public string Name { get; }
        public Object Value { get; }
        public MesControllerMachine(string Name, object Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
