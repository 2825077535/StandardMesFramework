using JOJO.Mes;
using JOJO.Mes.Const;
using MesUI.ViewModelBase;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MesUI.ViewModels
{
    public class MesClientUIViewModel: DialogViewModelBase
    {
        public DelegateCommand<SelectionChangedEventArgs> SelectionChangedCommand_Mes3 { get; }
        public DelegateCommand OpenMesUI { get; set; }

        public MesClientUIViewModel() 
        {
            foreach (MesEnum.MesCustomer customer in Enum.GetValues(typeof(MesEnum.MesCustomer)))
            {
                string description = MesApp.Instance.MesEnum.GetDescription(customer);
                if (!MesApp.Instance.JOJOMesConfig.CustomerName.Contains(description)) 
                {
                    MesApp.Instance.JOJOMesConfig.CustomerName.Add(description);
                }
            }

            SelectionChangedCommand_Mes3 = new DelegateCommand<SelectionChangedEventArgs>(OnSelectionChanged_Mes3);
            OpenMesUI = new DelegateCommand(OpenMesUIExecute);
            MesApp.Instance.JOJOMesConfig.Mes3.TecIdList.Add(new JOJO.Mes.Config.CustomerConfig.Mes3.MesShow() { Name = "smt", ID = 1 });
        }
        private void OpenMesUIExecute()
        {
            var customer = MesApp.Instance.MesEnum.GetEnumValueFromDescription<MesEnum.MesCustomer>(MesApp.Instance.JOJOMesConfig.SelectCustomer);
            MesApp.Instance.JOJOMesConfig.IsShowSelectCustomer = Visibility.Collapsed;
            MesApp.Instance.JOJOMesConfig.IsShowCustomer = Visibility.Visible;

            switch (customer)
            {
                case MesEnum.MesCustomer.Mes1:
                    MesApp.Instance.JOJOMesConfig.Mes1.IsShowUIVisibility = Visibility.Visible;
                    MesApp.Instance.JOJOMesConfig.SelectedTabIndex = 1;
                    break;
                case MesEnum.MesCustomer.Mes2:
                    MesApp.Instance.JOJOMesConfig.Mes2.IsShowUIVisibility = Visibility.Visible;
                    MesApp.Instance.JOJOMesConfig.SelectedTabIndex = 2;
                    break;
                case MesEnum.MesCustomer.Mes3:
                    MesApp.Instance.JOJOMesConfig.Mes3.IsShowUIVisibility = Visibility.Visible;
                    MesApp.Instance.JOJOMesConfig.SelectedTabIndex = 3;
                    break;
                default:
                    MesApp.Instance.JOJOMesConfig.IsShowSelectCustomer = Visibility.Visible;
                    MesApp.Instance.JOJOMesConfig.IsShowCustomer = Visibility.Collapsed;
                    break;
            }
        }
        private void OnSelectionChanged_Mes3(SelectionChangedEventArgs e)
        {
            if (e.Source is ComboBox combox)
            {
                MesDynamic dynamic = new MesDynamic();
                string Name =combox.Name;
                switch (Name)
                {
                    case "Mes3TecId":
                        MesApp.Instance.JOJOMesConfig.Mes3.TecId = (int)combox.SelectedValue;
                        dynamic.AddressInterface = "getMaterielList";
                        MesApp.Instance.Mes.Dynamic(dynamic);
                        break;
                    case "Mes3WorkOrderMaterielId":
                        MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderMaterielId = (int)combox.SelectedValue;
                        dynamic.AddressInterface = "getWorkOrderList";
                        MesApp.Instance.Mes.Dynamic(dynamic);
                        break;
                    case "Mes3WorkOrderId":
                        MesApp.Instance.JOJOMesConfig.Mes3.WorkOrderId = (int)combox.SelectedValue;
                        dynamic.AddressInterface = "getWorkList";
                        MesApp.Instance.Mes.Dynamic(dynamic);
                        break;
                    case "Mes3WorkId":
                        MesApp.Instance.JOJOMesConfig.Mes3.WorkId = (int)combox.SelectedValue;
                        dynamic.AddressInterface = "getWorkShopList";
                        MesApp.Instance.Mes.Dynamic(dynamic);
                        break;
                    case "Mes3WorkShopId":
                        MesApp.Instance.JOJOMesConfig.Mes3.WorkShopId = (int)combox.SelectedValue;
                        dynamic.AddressInterface = "getPullList";
                        MesApp.Instance.Mes.Dynamic(dynamic);
                        break;
                    case "Mes3PullId":
                        MesApp.Instance.JOJOMesConfig.Mes3.PullId = (int)combox.SelectedValue;
                        dynamic.AddressInterface = "getEquipmentList";
                        MesApp.Instance.Mes.Dynamic(dynamic);
                        break;
                    case "Mes3EquipmentId":
                        MesApp.Instance.JOJOMesConfig.Mes3.EquipmentId = (int)combox.SelectedValue;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
