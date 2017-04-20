using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        //value from AIDA report
        public string Cpu { get; set; }
        //value edited by hand
        public string CpuInvent { get; set; }

        public int Ram { get; set; }
        public int RamInvent { get; set; }

        public string MotherBoard { get; set; }
        public string MotherBoardInvent { get; set; }

        public string VideoAdapter { get; set; }
        public string VideoAdapterInvent { get; set; }

        public string Software { get; set; }
        public string SoftwareInvent { get; set; }

        [Display(Name = "Владелец")]
        public virtual Person Owner { get; set; }

        public virtual ICollection<SupportRequest> SupportRequests { get; set; }

        //check if any modifications were made during support operations
        [NotMapped]
        public bool HasModifications
        {
            get
            {
                return SupportRequests.Any(request => 
                request.FinishTime != null
                || !string.IsNullOrEmpty(request.SoftwareInstalled) 
                || !string.IsNullOrEmpty(request.SoftwareRemoved) 
                || !string.IsNullOrEmpty(request.SoftwareRepaired) 
                || !string.IsNullOrEmpty(request.SoftwareUpdated) 
                || !string.IsNullOrEmpty(request.HardwareInstalled) 
                || !string.IsNullOrEmpty(request.HardwareReplaced));
            }
        }

        public bool Equals(Computer otherComputer)
        {
            return 
                   ComputerName == otherComputer.ComputerName 
                && Cpu == otherComputer.Cpu
                && Ram == otherComputer.Ram
                && MotherBoard == otherComputer.MotherBoard
                && VideoAdapter == otherComputer.VideoAdapter
                && Software == otherComputer.Software;
        }

        public void CopyConfig(Computer otherComputer)
        {
            ComputerName = otherComputer.ComputerName;
            Cpu = otherComputer.Cpu;
            Ram = otherComputer.Ram;
            MotherBoard = otherComputer.MotherBoard;
            VideoAdapter = otherComputer.VideoAdapter;
            Software = otherComputer.Software;
        }

        public void FillInventedData()
        {
            if (string.IsNullOrEmpty(CpuInvent))
                CpuInvent = Cpu;
            if (RamInvent == 0)
                RamInvent = Ram;
            if (string.IsNullOrEmpty(MotherBoardInvent))
                MotherBoardInvent = MotherBoard;
            if (string.IsNullOrEmpty(VideoAdapterInvent))
                VideoAdapterInvent = VideoAdapter;
            if (string.IsNullOrEmpty(SoftwareInvent))
                SoftwareInvent = Software;
        }
    }
}