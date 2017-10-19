using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class ComputerHistoryViewModel
    {
        public int Id { get; set; }
        public int CompId { get; set; }

        [Display(Name = "Имя компьютера")]
        public string ComputerName { get; set; }

        [Display(Name = "Предыдущее имя")]
        public string OldName { get; set; }

        [Display(Name = "ID компьютера")]
        public string MbId { get; set; }

        [Display(Name = "Владелец")]
        public string OwnerName { get; set; }

        [Display(Name = "Процессор")]
        public string Cpu { get; set; }

        [Display(Name = "Память")]
        public int Ram { get; set; }

        [Display(Name = "Жесткий диск")]
        public string Hdd { get; set; }

        [Display(Name = "Материнская плата")]
        public string MotherBoard { get; set; }

        [Display(Name = "Видеокарта")]
        public string VideoAdapter { get; set; }

        [Display(Name = "Монитор")]
        public string Monitor { get; set; }

        [Display(Name = "Зафиксировано")]
        public string UpdateDate { get; set; }

        [Display(Name = "Что изменилось")]
        public string Changes { get; set; }

        [Display(Name = "Все установленные программы")]
        public string[] Software { get; set; }

        [Display(Name = "Новые программы")]
        public string[] InstalledSoftware { get; set; }

        [Display(Name = "Удаленные программы")]
        public string[] RemovedSoftware { get; set; }
    }
}