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

        public bool IsNotebook { get; set; }

        //unique motherboard id
        public string MbId { get; set; }

        [Display(Name = "Имя компьютера")]
        public string ComputerName { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime? LastReportDate { get; set; }

        public string Cpu { get; set; }

        public int Ram { get; set; }
        public int RamFixed { get; set; }

        public string Hdd { get; set; }
        public string HddFixed { get; set; }

        public string MotherBoard { get; set; }
        public string MotherBoardFixed { get; set; }

        public string VideoAdapter { get; set; }
        public string VideoAdapterFixed { get; set; }

        public string Monitor { get; set; }
        public string MonitorFixed { get; set; }

        public string Software { get; set; }

        [Display(Name = "Владелец")]
        public virtual Person Owner { get; set; }

        public virtual ICollection<SupportRequest> SupportRequests { get; set; }

        public ComputerHistoryItem NewHistory(string changesSummary = "Новый", string installedSoft = null, string removedSoft = null, string oldName = null)
        {
            return new ComputerHistoryItem
            {
                HistoryComputer = this,
                HistoryComputerOwner = Owner,
                HistoryUpdated = LastReportDate,
                HistoryCpu = Cpu,
                HistoryMotherBoard = MotherBoard,
                HistoryRam = Ram,
                HistoryHdd = Hdd,
                HistorySoftware = Software,
                HistoryVideoAdapter = VideoAdapter,
                HistoryMonitor = Monitor,
                Changes = changesSummary,
                SoftwareInstalled = installedSoft,
                SoftwareRemoved = removedSoft,
                OldName = oldName
            };
        }

        //check if any modifications were made during support operations
        [NotMapped]
        public bool HasModifications
        {
            get
            {
                return SupportRequests.Any(request => 
                request.FinishTime != null &&
                  (!string.IsNullOrEmpty(request.SoftwareInstalled) 
                || !string.IsNullOrEmpty(request.SoftwareRemoved) 
                || !string.IsNullOrEmpty(request.SoftwareRepaired) 
                || !string.IsNullOrEmpty(request.SoftwareUpdated) 
                || request.HardwareId != 0 
                ));
            }
        }

        //compare two configurations
        public bool Equals(Computer otherComputer)
        {
            return
                IsNotebook == otherComputer.IsNotebook
                && Ram == otherComputer.Ram
                && Hdd == otherComputer.Hdd
                && Cpu == otherComputer.Cpu
                && MotherBoard == otherComputer.MotherBoard
                && VideoAdapter == otherComputer.VideoAdapter
                && Monitor == otherComputer.Monitor
                && Software == otherComputer.Software
                && Owner == otherComputer.Owner
                && MbId == otherComputer.MbId;
        }

        //check if configuration changed
        public bool IsConfigChanged()
        {
            return
                Ram != RamFixed
                //for desktops check any hdd string changes
                || (!IsNotebook && Hdd != HddFixed)
                //for notebooks exclude add/remove hdd events (portable hdd)
                || (IsNotebook && Hdd != HddFixed && (!string.IsNullOrEmpty(HddFixed) && !Hdd.Contains(HddFixed)) && (!string.IsNullOrEmpty(Hdd) && !HddFixed.Contains(Hdd)))
                || MotherBoard != MotherBoardFixed
                || VideoAdapter != VideoAdapterFixed
                //for notebooks exclude monitor change events
                || (!IsNotebook && Monitor != MonitorFixed);
        }

        //return array of strings representing changes in configuration and update data
        public string[] CopyConfig(Computer newConfig)
        {
            LastReportDate = newConfig.LastReportDate;
            UpdateDate = DateTime.Now;
            var changes = new string[3];
            try
            {
                var sb = new StringBuilder();
                if (IsNotebook != newConfig.IsNotebook)
                    IsNotebook = newConfig.IsNotebook;
                if (Ram != newConfig.Ram)
                {
                    Ram = newConfig.Ram;
                    sb.Append("память, ");
                }
                if (Cpu != newConfig.Cpu)
                {
                    Cpu = newConfig.Cpu;
                    //we don't write any info about cpu change because cpu frequency is changing frequently
                }
                if (Hdd != newConfig.Hdd)
                {
                    Hdd = newConfig.Hdd;
                    sb.Append("диск, ");
                }
                if (MotherBoard != newConfig.MotherBoard)
                {
                    MotherBoard = newConfig.MotherBoard;
                    sb.Append("материнская плата, ");
                }
                if (VideoAdapter != newConfig.VideoAdapter)
                {
                    VideoAdapter = newConfig.VideoAdapter;
                    sb.Append("видеокарта, ");
                }
                if (Monitor != newConfig.Monitor)
                {
                    Monitor = newConfig.Monitor;
                    if (!IsNotebook)
                        sb.Append("монитор(ы), ");
                }

                if (MbId != newConfig.MbId)
                    MbId = newConfig.MbId;

                if (newConfig.Software != null && Software != newConfig.Software)
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
                    sb.Append("установленные программы, ");
                }

                if (Owner != newConfig.Owner)
                {
                    Owner = newConfig.Owner;
                    sb.Append("пользователь, ");
                }

                var changesSummary = sb.ToString();
                if (changesSummary.Length > 4)
                {
                    var changesString = changesSummary.Remove(changesSummary.Length - 2);
                    changes[0] = char.ToUpper(changesString[0]) + changesString.Substring(1);
                }
                return changes;
            }
            catch (Exception ex)
            {
                ex.WriteToLogAsync(source: "Computers");
                return changes;
            }
        }

        public bool FillFixedData()
        {
            var modified = false;
            if (RamFixed == 0)
            {
                RamFixed = Ram;
                modified = true;
            }
            if (string.IsNullOrEmpty(HddFixed))
            {
                HddFixed = Hdd;
                modified = true;
            }
            if (string.IsNullOrEmpty(MotherBoardFixed))
            {
                MotherBoardFixed = MotherBoard;
                modified = true;
            }
            if (string.IsNullOrEmpty(VideoAdapterFixed))
            {
                VideoAdapterFixed = VideoAdapter;
                modified = true;
            }
            if (string.IsNullOrEmpty(MonitorFixed))
            {
                MonitorFixed = Monitor;
                modified = true;
            }
            if (UpdateDate == null)
            {
                UpdateDate = DateTime.Now;
                modified = true;
            }
            return modified;
        }
    }
}