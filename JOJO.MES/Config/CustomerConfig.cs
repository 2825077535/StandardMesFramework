using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace JOJO.Mes.Config
{
    public class CustomerConfig
    {
        [Serializable]
        public class Mes1 : BindableBase
        {
            private Visibility _isShowUIVisibility = Visibility.Collapsed;
            public Visibility IsShowUIVisibility
            {
                get => _isShowUIVisibility;
                set => SetProperty(ref _isShowUIVisibility, value, nameof(IsShowUIVisibility));
            }

            private string _lineName = "SMT1线";
            public string LineName
            {
                get => _lineName;
                set => SetProperty(ref _lineName, value, nameof(LineName));
            }

            private string _machineName = "SMTAOI";
            public string MachineName
            {
                get => _machineName;
                set => SetProperty(ref _machineName, value, nameof(MachineName));
            }

            private string _statueName = "站点名称";
            public string StatueName
            {
                get => _statueName;
                set => SetProperty(ref _statueName, value, nameof(StatueName));
            }

            private int _reNumber = 1;
            public int ReNumber
            {
                get => _reNumber;
                set => SetProperty(ref _reNumber, value, nameof(ReNumber));
            }

            private TimeSpan _heartTime = TimeSpan.FromSeconds(30000);
            public TimeSpan HeartTime
            {
                get => _heartTime;
                set => SetProperty(ref _heartTime, value, nameof(HeartTime));
            }
        }
        public class Mes2 : BindableBase
        {
            private Visibility _isShowUIVisibility = Visibility.Collapsed;
            public Visibility IsShowUIVisibility
            {
                get => _isShowUIVisibility;
                set => SetProperty(ref _isShowUIVisibility, value, nameof(IsShowUIVisibility));
            }

            private string _statueName = "站点名称";
            public string StatueName
            {
                get => _statueName;
                set => SetProperty(ref _statueName, value, nameof(StatueName));
            }

            private int _reNumber = 1;
            public int ReNumber
            {
                get => _reNumber;
                set => SetProperty(ref _reNumber, value, nameof(ReNumber));
            }

            private TimeSpan _heartTime = TimeSpan.FromSeconds(30000);
            public TimeSpan HeartTime
            {
                get => _heartTime;
                set => SetProperty(ref _heartTime, value, nameof(HeartTime));
            }
        }
        public class Mes3 : BindableBase
        {
            private Visibility _isShowUIVisibility = Visibility.Collapsed;
            public Visibility IsShowUIVisibility
            {
                get => _isShowUIVisibility;
                set => SetProperty(ref _isShowUIVisibility, value, nameof(IsShowUIVisibility));
            }
            private int _tecId = 0;
            public int TecId
            {
                get => _tecId;
                set => SetProperty(ref _tecId, value, nameof(TecId));
            }
            private ObservableCollection<MesShow> _tecIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> TecIdList
            {
                get => _tecIdList;
                set => SetProperty(ref _tecIdList, value, nameof(TecIdList));
            }

            private int _workOrderId = 0;
            public int WorkOrderId
            {
                get => _workOrderId;
                set => SetProperty(ref _workOrderId, value, nameof(WorkOrderId));
            }
            private ObservableCollection<MesShow> _workOrderIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> WorkOrderIdList
            {
                get => _workOrderIdList;
                set => SetProperty(ref _workOrderIdList, value, nameof(WorkOrderIdList));
            }
            private int _workOrderMaterielId = 0;
            public int WorkOrderMaterielId
            {
                get => _workOrderMaterielId;
                set => SetProperty(ref _workOrderMaterielId, value, nameof(WorkOrderMaterielId));
            }
            private ObservableCollection<MesShow> _workOrderMaterielIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> WorkOrderMaterielIdList
            {
                get => _workOrderMaterielIdList;
                set => SetProperty(ref _workOrderMaterielIdList, value, nameof(WorkOrderMaterielIdList));
            }
            private int _workId = 0;
            public int WorkId
            {
                get => _workId;
                set => SetProperty(ref _workId, value, nameof(WorkId));
            }
            private ObservableCollection<MesShow> _workIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> WorkIdList
            {
                get => _workIdList;
                set => SetProperty(ref _workIdList, value, nameof(WorkIdList));
            }
            private int _workShopId = 0;
            public int WorkShopId
            {
                get => _workShopId;
                set => SetProperty(ref _workShopId, value, nameof(WorkShopId));
            }
            private ObservableCollection<MesShow> _workShopIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> WorkShopIdList
            {
                get => _workShopIdList;
                set => SetProperty(ref _workShopIdList, value, nameof(WorkShopIdList));
            }
            private int _pullId = 0;
            public int PullId
            {
                get => _pullId;
                set => SetProperty(ref _pullId, value, nameof(PullId));
            }
            private ObservableCollection<MesShow> _pullIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> PullIdList
            {
                get => _pullIdList;
                set => SetProperty(ref _pullIdList, value, nameof(PullIdList));
            }
            private int _equipmentId = 0;
            public int EquipmentId
            {
                get => _equipmentId;
                set => SetProperty(ref _equipmentId, value, nameof(EquipmentId));
            }
            private ObservableCollection<MesShow> _equipmentIdList = new ObservableCollection<MesShow>();
            [JsonIgnore]
            public ObservableCollection<MesShow> EquipmentIdList
            {
                get => _equipmentIdList;
                set => SetProperty(ref _equipmentIdList, value, nameof(EquipmentIdList));
            }
            public class MesShow
            {
                public int ID { get; set; }
                public string Name { get; set; }
            }

        }
    }
}
