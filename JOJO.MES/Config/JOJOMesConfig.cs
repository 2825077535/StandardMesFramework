using JOJO.Mes.Const;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace JOJO.Mes.Config
{
    [Serializable]
    public class JOJOMesConfig: BindableBase
    {
        public JOJOMesConfig() 
        {
            
        }
        private bool _isMesControMachine = false;
        /// <summary>
        /// 是否需要Mes控制软件，不需要情况下，减少线程开辟
        /// </summary>
        public bool IsMesControMachine
        {
            get => _isMesControMachine;
            set => SetProperty(ref _isMesControMachine, value, nameof(IsMesControMachine));
        }

        private Visibility _isShowSelectCustomer = Visibility.Visible;
        /// <summary>
        /// 是否显示选择客户页面
        /// </summary>
        public Visibility IsShowSelectCustomer
        {
            get => _isShowSelectCustomer;
            set => SetProperty(ref _isShowSelectCustomer, value, nameof(IsShowSelectCustomer));
        }

        private bool _isEnableMes = false;
        /// <summary>
        /// 是否开启Mes
        /// </summary>
        public bool IsEnableMes
        {
            get => _isEnableMes;
            set => SetProperty(ref _isEnableMes, value, nameof(IsEnableMes));
        }

        private int _mesTimeOut = 5000;
        /// <summary>
        /// Mes超时时间
        /// </summary>
        public int MesTimeOut
        {
            get => _mesTimeOut;
            set => SetProperty(ref _mesTimeOut, value, nameof(MesTimeOut));
        }

        private string _equipmentID = "SMT01";
        public string EquipmentID
        {
            get => _equipmentID;
            set => SetProperty(ref _equipmentID, value, nameof(EquipmentID));
        }

        private int _port = 8000;
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value, nameof(Port));
        }

        private string _mesAddress = "192.168.1.1";
        public string MesAddress
        {
            get => _mesAddress;
            set => SetProperty(ref _mesAddress, value, nameof(MesAddress));
        }

        private int _selectedTabIndex = 0;
        /// <summary>
        /// 选择客户选项卡的ID
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value, nameof(SelectedTabIndex));
        }

        private Visibility _isShowCustomer = Visibility.Collapsed;
        /// <summary>
        /// 是否显示客户页面
        /// </summary>
        public Visibility IsShowCustomer
        {
            get => _isShowCustomer;
            set => SetProperty(ref _isShowCustomer, value, nameof(IsShowCustomer));
        }

        private string _selectCustomer = "选择Mes客户";
        /// <summary>
        /// 选择的客户名称
        /// </summary>
        public string SelectCustomer
        {
            get => _selectCustomer;
            set => SetProperty(ref _selectCustomer, value, nameof(SelectCustomer));
        }
        private ObservableCollection<string> _CustomerName = new ObservableCollection<string>();
        /// <summary>
        /// 全部Mes客户的选项
        /// </summary>
        public ObservableCollection<string> CustomerName
        {
            get => _CustomerName;
            set { SetProperty(ref _CustomerName, value); }
        }

        public CustomerConfig.Mes1 Mes1 { get; set; } = new CustomerConfig.Mes1();
        public CustomerConfig.Mes2 Mes2 { get; set; } = new CustomerConfig.Mes2();
        public CustomerConfig.Mes3 Mes3 { get; set; } = new CustomerConfig.Mes3();

    }
}
