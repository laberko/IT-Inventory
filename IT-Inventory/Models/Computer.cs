using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace IT_Inventory.Models
{
    public class Computer
    {
        public Computer()
        {
            SupportRequests = new HashSet<SupportRequest>();
        }
        [Key]
        public int Id { get; set; }

        [Display(Name = "Имя компьютера")]
        public string ComputerName { get; set; }

        public string Cpu { get; set; }

        public int Ram { get; set; }

        public string MotherBoard { get; set; }

        public string VideoAdapter { get; set; }

        public string Software { get; set; }

        [Display(Name = "Владелец")]
        public virtual Person Owner { get; set; }

        public virtual ICollection<SupportRequest> SupportRequests { get; set; }

        public bool Equals(Computer otherComputer)
        {
            return ComputerName == otherComputer.ComputerName 
                && Cpu == otherComputer.Cpu
                && Ram == otherComputer.Ram
                && MotherBoard == otherComputer.MotherBoard
                && VideoAdapter == otherComputer.VideoAdapter;
        }

        public void CopyConfig(Computer otherComputer)
        {
            ComputerName = otherComputer.ComputerName;
            Cpu = otherComputer.Cpu;
            Ram = otherComputer.Ram;
            MotherBoard = otherComputer.MotherBoard;
            VideoAdapter = otherComputer.VideoAdapter;
        }
    }
}