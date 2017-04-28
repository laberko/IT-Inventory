using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

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

        public DateTime? UpdateDate { get; set; }

        // fixed value from AIDA report
        public string Cpu { get; set; }
        // Invent == value edited by hand
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

        public ComputerHistoryItem NewHistory(string changesSummary = "Новый", string installedSoft = null, string removedSoft = null)
        {
            return new ComputerHistoryItem
            {
                HistoryComputer = this,
                HistoryComputerOwner = Owner,
                HistoryUpdated = UpdateDate,
                HistoryCpu = Cpu,
                HistoryMotherBoard = MotherBoard,
                HistoryRam = Ram,
                HistorySoftware = Software,
                HistoryVideoAdapter = VideoAdapter,
                Changes = changesSummary,
                SoftwareInstalled = installedSoft,
                SoftwareRemoved = removedSoft
            };
        }

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
                   Cpu.Split(',')[0] == otherComputer.Cpu.Split(',')[0]
                && Ram == otherComputer.Ram
                && MotherBoard == otherComputer.MotherBoard
                && VideoAdapter == otherComputer.VideoAdapter
                && Software == otherComputer.Software;
        }

        //return array of strings representing changes in configuration
        public string[] CopyConfig(Computer newConfig)
        {
            UpdateDate = DateTime.Now;
            var changes = new string[3];
            var sb = new StringBuilder();
            if (Cpu != newConfig.Cpu)
            {
                Cpu = newConfig.Cpu;
                sb.Append("Процесор, ");
            }
            if (Ram != newConfig.Ram)
            {
                Ram = newConfig.Ram;
                sb.Append("Память, ");
            }
            if (MotherBoard != newConfig.MotherBoard)
            {
                MotherBoard = newConfig.MotherBoard;
                sb.Append("Материнская плата, ");
            }
            if (VideoAdapter != newConfig.VideoAdapter)
            {
                VideoAdapter = newConfig.VideoAdapter;
                sb.Append("Видеокарта, ");
            }
            if (Software != newConfig.Software)
            {
                var oldSoftware = Software.Split(new[] {"[NEW_LINE]"}, StringSplitOptions.None);
                var newSoftware = newConfig.Software.Split(new[] {"[NEW_LINE]"}, StringSplitOptions.None);

                var installedSb = new StringBuilder();
                foreach (var soft in newSoftware.Where(soft => oldSoftware.All(s => s != soft)))
                    installedSb.Append(soft + "[NEW_LINE]");
                var removedSb = new StringBuilder();
                foreach (var soft in oldSoftware.Where(soft => newSoftware.All(s => s != soft)))
                    removedSb.Append(soft + "[NEW_LINE]");

                changes[1] = installedSb.ToString();
                changes[2] = removedSb.ToString();
                Software = newConfig.Software;
                sb.Append("Установленные программы, ");
            }
            var changesSummary = sb.ToString();
            changes[0] = changesSummary.Remove(changesSummary.Length - 2);
            return (changes);
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
            if (UpdateDate == null)
                UpdateDate = DateTime.Now;
        }
    }
}