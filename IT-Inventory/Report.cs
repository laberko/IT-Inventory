﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IT_Inventory
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Report
    {
        public DateTime ReportDate;
        public string UserFullName;
        public string UserName;
        public string CompName;
        private string _langField;
        private ReportPage[] _pageField;

        public static async Task<Report> GetReportAsync(string hostName)
        {
            var reportFileName = string.Empty;
            var log = string.Empty;
            try
            {
                DateTime reportDate;
                Report report;
                
                var rootDir = new DirectoryInfo(@"\\rivs.org\it\ConfigReporting\ConfigReports");
                var hostDirs = rootDir.GetDirectories();
                var hostDir = hostDirs.FirstOrDefault(d => d.Name == hostName.ToUpper());
                if (hostDir == null)
                    return null;
                var hostReportDirs = hostDir.GetDirectories().OrderByDescending(d => d.CreationTime).ToList();
                //take only the last report
                if (!DateTime.TryParse(hostReportDirs.First().Name, out reportDate))
                    return null;
                var reportFile = hostReportDirs.First().GetFiles().OrderByDescending(f => f.CreationTime).FirstOrDefault();
                if (reportFile == null)
                    return null;
                reportFileName = reportFile.FullName;

                using (var fileStream = new FileStream(reportFileName, FileMode.Open))
                {
                    try
                    {
                        var serializer = new XmlSerializer(typeof (Report));
                        report = (Report) serializer.Deserialize(fileStream);
                    }
                    catch
                    {
                        fileStream.Close();
                        File.Delete(reportFileName);
                        log = "File with broken XML DOM deleted:\n";
                        throw;
                    }
                }
                report.UserName = Path.GetFileNameWithoutExtension(reportFileName);
                report.UserFullName = (@"RIVS\" + report.UserName).GetUserName();
                report.CompName = hostName;
                report.ReportDate = reportDate;

                //remove folders older than 10 days but keep at least 10 folders
                var span = new TimeSpan(10, 0, 0, 0);
                foreach (var dir in hostReportDirs.Skip(10).Where(dir => DateTime.Now - dir.CreationTime > span))
                    dir.Delete(true);

                return report;
            }
            catch (Exception ex)
            {
                log += reportFileName + "\n" + ex.Message;
                await log.WriteToLogAsync(EventLogEntryType.Warning, "Report");
                ex.WriteToLogAsync(source: "Report");
                return null;
            }
        }


        public string Cpu => Page[1].Group[1].Item[0].Value;

        public string MotherBoard => Page[1].Group[1].Item[1].Value;

        public int Ram
        {
            get
            {
                int valueInt;
                var valueString = Page[1].Group[1].Item[3].Value.Split(' ')[0];
                if (!int.TryParse(valueString, out valueInt))
                    return valueInt;
                var valueGb = Math.Round(((double) valueInt)/1024);
                return (int) valueGb;
            }
        }

        public string Hdd
        {
            get
            {
                //int valueInt;
                //var item = Page[1].Group[5].Item.FirstOrDefault(i => i.Title == "Общий объём");
                //var valueString = item == null ? string.Empty : item.Value;
                //if (valueString.Contains("МБ"))
                //{
                //    valueString = valueString.Split(' ')[0];
                //    int.TryParse(valueString, out valueInt);
                //    valueInt = valueInt/1024;
                //}
                //else if (valueString.Contains("ГБ"))
                //{
                //    valueString = valueString.Split('.')[0];
                //    int.TryParse(valueString, out valueInt);
                //}
                //else
                //    return 0;
                //return valueInt;
                var items = Page[1].Group[4].Item.Where(i => i.Title == "Дисковый накопитель" && !i.Value.Contains("USB")).ToArray();
                if (items.Length == 0)
                    return string.Empty;
                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    sb.Append(item.Value.Replace(" SCSI Disk Device", "").Replace(" ATA Device", ""));
                    sb.Append(", ");
                }
                var hddString = sb.ToString();
                return hddString.Remove(hddString.Length - 2);
            }
        }

        public string VideoAdapter
        {
            get
            {
                var page = Page.FirstOrDefault(p => p.Title == "Видео Windows");
                return page == null ? string.Empty : page.Device[0].Group[0].Item[0].Value;
            }
        }

        public string Software
        {
            get
            {
                var page = Page.FirstOrDefault(p => p.Title == "Установленные программы");
                if (page == null)
                    return null;
                string[] exclusions = {"Hotfix", "MUI", "C++", "Update", "Proof", "Service Pack", "Framework", "Runtime", "Пакет", "NVIDIA", "CCC", "Redistributable", "Средства" , "Засоби", "Языковой" };
                var softList = (from item 
                          in page.Device
                          where !exclusions.Any(item.Title.Contains)
                          select item.Title).Distinct();
                var softString = new StringBuilder();
                foreach (var item in softList)
                    softString.Append(item + "[NEW_LINE]");
                return softString.ToString();
            }
        }



        public string Lang
        {
            get
            {
                return _langField;
            }
            set
            {
                _langField = value;
            }
        }

        [XmlElement("Page")]
        public ReportPage[] Page
        {
            get
            {
                return _pageField;
            }
            set
            {
                _pageField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPage
    {
        private string _titleField;
        private byte _iconField;
        private string _menuTitleField;
        private byte _menuIconField;
        private bool _menuIconFieldSpecified;
        private ReportPageDevice[] _deviceField;
        private ReportPageGroup[] _groupField;
        private ReportPageItem[] _itemField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        public string MenuTitle
        {
            get
            {
                return _menuTitleField;
            }
            set
            {
                _menuTitleField = value;
            }
        }

        public byte MenuIcon
        {
            get
            {
                return _menuIconField;
            }
            set
            {
                _menuIconField = value;
            }
        }

        [XmlIgnore]
        public bool MenuIconSpecified
        {
            get
            {
                return _menuIconFieldSpecified;
            }
            set
            {
                _menuIconFieldSpecified = value;
            }
        }

        [XmlElement("Device")]
        public ReportPageDevice[] Device
        {
            get
            {
                return _deviceField;
            }
            set
            {
                _deviceField = value;
            }
        }

        [XmlElement("Group")]
        public ReportPageGroup[] Group
        {
            get
            {
                return _groupField;
            }
            set
            {
                _groupField = value;
            }
        }

        [XmlElement("Item")]
        public ReportPageItem[] Item
        {
            get
            {
                return _itemField;
            }
            set
            {
                _itemField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageDevice
    {
        private string _titleField;
        private byte _iconField;
        private bool _iconFieldSpecified;
        private ReportPageDeviceGroup[] _groupField;
        private ReportPageDeviceItem[] _itemField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        [XmlIgnore]
        public bool IconSpecified
        {
            get
            {
                return _iconFieldSpecified;
            }
            set
            {
                _iconFieldSpecified = value;
            }
        }

        [XmlElement("Group")]
        public ReportPageDeviceGroup[] Group
        {
            get
            {
                return _groupField;
            }
            set
            {
                _groupField = value;
            }
        }

        [XmlElement("Item")]
        public ReportPageDeviceItem[] Item
        {
            get
            {
                return _itemField;
            }
            set
            {
                _itemField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageDeviceGroup
    {
        private string _titleField;
        private byte _iconField;
        private ReportPageDeviceGroupItem[] _itemField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        [XmlElement("Item")]
        public ReportPageDeviceGroupItem[] Item
        {
            get
            {
                return _itemField;
            }
            set
            {
                _itemField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageDeviceGroupItem
    {
        private string _titleField;
        private byte _iconField;
        private ushort _idField;
        private bool _idFieldSpecified;
        private string _valueField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        public ushort Id
        {
            get
            {
                return _idField;
            }
            set
            {
                _idField = value;
            }
        }

        [XmlIgnore]
        public bool IdSpecified
        {
            get
            {
                return _idFieldSpecified;
            }
            set
            {
                _idFieldSpecified = value;
            }
        }

        public string Value
        {
            get
            {
                return _valueField;
            }
            set
            {
                _valueField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageDeviceItem
    {
        private string _titleField;
        private ushort _idField;
        private bool _idFieldSpecified;
        private string _valueField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public ushort Id
        {
            get
            {
                return _idField;
            }
            set
            {
                _idField = value;
            }
        }

        [XmlIgnore]
        public bool IdSpecified
        {
            get
            {
                return _idFieldSpecified;
            }
            set
            {
                _idFieldSpecified = value;
            }
        }

        public string Value
        {
            get
            {
                return _valueField;
            }
            set
            {
                _valueField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageGroup
    {
        private string _titleField;
        private byte _iconField;
        private ReportPageGroupItem[] _itemField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        [XmlElement("Item")]
        public ReportPageGroupItem[] Item
        {
            get
            {
                return _itemField;
            }
            set
            {
                _itemField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageGroupItem
    {
        private string _titleField;
        private byte _iconField;
        private ushort _idField;
        private bool _idFieldSpecified;
        private string _valueField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        public ushort Id
        {
            get
            {
                return _idField;
            }
            set
            {
                _idField = value;
            }
        }

        [XmlIgnore]
        public bool IdSpecified
        {
            get
            {
                return _idFieldSpecified;
            }
            set
            {
                _idFieldSpecified = value;
            }
        }

        public string Value
        {
            get
            {
                return _valueField;
            }
            set
            {
                _valueField = value;
            }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class ReportPageItem
    {
        private string _titleField;
        private byte _iconField;
        private ushort _idField;
        private string _valueField;

        public string Title
        {
            get
            {
                return _titleField;
            }
            set
            {
                _titleField = value;
            }
        }

        public byte Icon
        {
            get
            {
                return _iconField;
            }
            set
            {
                _iconField = value;
            }
        }

        public ushort Id
        {
            get
            {
                return _idField;
            }
            set
            {
                _idField = value;
            }
        }

        public string Value
        {
            get
            {
                return _valueField;
            }
            set
            {
                _valueField = value;
            }
        }
    }
}
