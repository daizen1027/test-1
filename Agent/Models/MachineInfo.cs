using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;


namespace Agent.Models
{
    public class MachineInfo
    {
        public int Id { get; set; }

        [Display(Name = "機器號碼")]//(Machine Number)
        public string MachineNo { get; set; }

        [Display(Name = "代理商編號")]//(Agent ID)
        public string AgentId { get; set; }

        [Display(Name = "代理商名稱")]//(Agent Name)
        public string AgentName{ get; set; }
        [Display(Name = "出貨日期")]//(Delivery Date)
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; }
    }


    public class MachineInfoViewModel
    {
        //public List<MachineInfo> MachineInfos { get; set; }

        public IPagedList<MachineInfo> MachineInfos { get; set; }

        public SelectList AgentNames { get; set; }

        [Display(Name = "代理商")]//(Agent)
        public string MachineInfoAgentName { get; set; }        

        [Display(Name = "開始日期")]//(Start Date)
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);

        [Display(Name = "結束日期")]//(End Date)
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Today.AddMonths(1);


    }

    public class MachineInfoCreate
    {
        public int Id { get; set; }

        [Display(Name = "機器號碼")]//(Machine Number)
        public string MachineNo { get; set; }

        [Display(Name = "代理商編號")]//(Agent ID)
        public string AgentId { get; set; }

        [Display(Name = "代理商名稱")]//(Agent Name)
        public string AgentName { get; set; }
        [Display(Name = "出貨日期")]//(Delivery Date)
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        public SelectList AgentNames { get; set; }
    }
}



