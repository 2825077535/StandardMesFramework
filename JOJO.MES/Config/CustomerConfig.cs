using System;
using System.Collections.Generic;
using System.Windows;

namespace JOJO.Mes.Config
{
    public class CustomerConfig
    {
        [Serializable]
        public class Mes1
        {
            public Visibility IsShowUIVisibility { get; set; } = Visibility.Collapsed;
            public string LineName { get; set; } = "SMT1线";
            public string MachineName { get; set; } = "SMTAOI";
            public string StatueName { get; set; } = "站点名称";
            public int ReNumber { get; set; } = 1;
            public TimeSpan HeartTime { get; set; } = TimeSpan.FromSeconds(30000);
        }
        public class Mes2
        {
            public Visibility IsShowUIVisibility { get; set; } = Visibility.Collapsed;
            public string StatueName { get; set; } = "站点名称";
            public int ReNumber { get; set; } = 1;
            public TimeSpan HeartTime { get; set; } = TimeSpan.FromSeconds(30000);
        }
        public class Mes3
        {
            public string Token { get; set; } = "";
            public int TecId { get; set; } = 0;
            public int workOrderId { get; set; } = 0;
            public int workOrderMaterielId { get; set; } = 0;
            public int workId { get; set; } = 0;
            public int workShopId { get; set; } = 0;
            public int pullId { get; set; } = 0;
            public int equipmentId { get; set; } = 0;
            [NonSerialized]
            public Dictionary<string, int> TecIdList = new Dictionary<string, int>();

        }


    }
}
