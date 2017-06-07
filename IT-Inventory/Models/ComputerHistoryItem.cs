using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class ComputerHistoryItem
    {
        [Key]
        public int Id { get; set; }

        public virtual Computer HistoryComputer { get; set; }

        public virtual Person HistoryComputerOwner { get; set; }

        public DateTime? HistoryUpdated { get; set; }

        public string HistoryCpu { get; set; }

        public int HistoryRam { get; set; }

        public string HistoryHdd { get; set; }

        public string HistoryMotherBoard { get; set; }

        public string HistoryVideoAdapter { get; set; }

        public string HistorySoftware { get; set; }

        public string Changes { get; set; }

        public string SoftwareInstalled { get; set; }

        public string SoftwareRemoved { get; set; }
    }
}