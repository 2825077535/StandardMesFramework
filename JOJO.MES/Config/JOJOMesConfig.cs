using System;
using System.Windows;

namespace JOJO.Mes.Config
{
    [Serializable]
    public class JOJOMesConfig
    {
        /// <summary>
        /// 是否需要Mes控制软件，不需要情况下，减少线程开辟
        /// </summary>
        public bool IsMesControMachine { get; set; } = false;
        /// <summary>
        /// 是否显示选择客户页面
        /// </summary>
        public string IsShowSelectCustomer { get; set; } = Visibility.Visible.ToString();
        /// <summary>
        /// 是否开启Mes
        /// </summary>
        public bool IsEnableMes { get; set; } = false;
        /// <summary>
        /// Mes超时时间
        /// </summary>
        public int MesTimeOut { get; set; } = 5000;
        public string EquipmentID { get; set; } = "SMT01";
        public int Port = 8000;
        public string MesAddress { get; set; } = "192.168.1.1";
        /// <summary>
        /// 选择客户选项卡的ID
        /// </summary>
        public int SelectedTabIndex { get; set; } = 0;
        /// <summary>
        /// 是否显示客户页面
        /// </summary>
        public string IsShowCustomer { get; set; } = Visibility.Collapsed.ToString();
        /// <summary>
        /// 选择的客户名称
        /// </summary>
        public string SelectCustomer { get; set; } = "选择Mes客户";

        public CustomerConfig.Mes1 Mes1 { get; set; } = new CustomerConfig.Mes1();
        public CustomerConfig.Mes2 Mes2 { get; set; } = new CustomerConfig.Mes2();

    }
}
