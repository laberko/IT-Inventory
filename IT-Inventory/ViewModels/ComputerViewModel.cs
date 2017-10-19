using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class ComputerViewModel
    {
        public int Id { get; set; }
        public bool IsNotebook { get; set; }
        public bool IsConfigChanged { get; set; }

        [Display(Name = "Имя компьютера")]
        public string ComputerName { get; set; }

        [Display(Name = "ID компьютера")]
        public string MbId { get; set; }

        [Display(Name = "Процессор")]
        public string Cpu { get; set; }

        [Display(Name = "Память")]
        public int Ram { get; set; }
        public int RamFixed { get; set; }

        [Display(Name = "Жесткий диск")]
        public string Hdd { get; set; }
        public string HddFixed { get; set; }

        [Display(Name = "Материнская плата")]
        public string MotherBoard { get; set; }
        public string MotherBoardFixed { get; set; }

        [Display(Name = "Видеокарта")]
        public string VideoAdapter { get; set; }
        public string VideoAdapterFixed { get; set; }

        [Display(Name = "Монитор")]
        public string Monitor { get; set; }
        public string MonitorFixed { get; set; }

        [Display(Name = "Владелец")]
        public string Owner { get; set; }

        [Display(Name = "Владелец")]
        public int OwnerId { get; set; }

        [Display(Name = "Обновлен")]
        public string LastReportDate { get; set; }

        [Display(Name = "Установленные программы")]
        public string[] Software { get; set; }

        [Display(Name = "Установленные программы")]
        public string SoftwareString
        {
            get
            {
                return string.Join("\n", Software);
            }
            set
            {
                Software = value.Replace("\r", "").Split(new[] { "\n" }, StringSplitOptions.None);
            }
        }

        public bool HasRequests { get; set; }

        public bool HasModifications { get; set; }
    }
}