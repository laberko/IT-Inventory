using System;
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
        public DateTime FileDate;
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
                Report report;
                
                var rootDir = new DirectoryInfo(@"\\rivs.org\it\ConfigReporting\ConfigReports");
                var hostDirs = rootDir.GetDirectories();
                var hostDir = hostDirs.FirstOrDefault(d => d.Name == hostName.ToUpper());
                if (hostDir == null || hostDir.GetDirectories().Length == 0)
                    return null;
                var hostReportDirs = hostDir.GetDirectories().OrderByDescending(d => d.CreationTime).ToList();
                
                //take only the last report
                FileInfo reportFile = null;
                foreach (var dir in hostReportDirs)
                {
                    reportFile = dir.GetFiles().OrderByDescending(f => f.CreationTime).FirstOrDefault();
                    //if some file found - go further
                    if (reportFile != null)
                        break;
                }
                //if no file found - exit
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

                report.FileDate = reportFile.LastWriteTime;
                report.UserName = Path.GetFileNameWithoutExtension(reportFileName);
                report.UserFullName = (@"RIVS\" + report.UserName).GetUserName();
                report.CompName = hostName;

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


        public DateTime ReportDate
        {
            get
            {
                try
                {
                    DateTime reportDate;
                    return DateTime.TryParse(Page[1].Group[0].Item[8].Value.Replace(" / ", " "), out reportDate)
                        ? reportDate
                        : FileDate;
                }
                catch
                {
                    return FileDate;
                }
            }
        }

        public bool IsNotebook => CompName.Contains("-NB") || Page[1].Group[0].Item[0].Value.Contains("(Mobile)");

        public string Cpu => Page[1].Group[1].Item[0].Value;

        public string MotherBoard => Page[1].Group[1].Item[1].Value;

        public string MbId
        {
            get
            {
                try
                {
                    //macbook and some hp notebooks
                    if (Page[3].Group == null)
                        return Page[3].Device[2].Group[0].Item[3].Value;

                    //get Mb ID
                    var value = Page[1].Group.Length > 9 ? Page[1].Group[9].Item[6].Value : Page[3].Group[0].Item[0].Value;

                    //a couple of old notebooks
                    if (value.Contains("Acer") || value.Contains("MICRO-STAR"))
                        value += Page[1].Group[1].Item[4].Value;

                    //some buggy motherboards with ununique ids
                    if (value == "00020003-00040005-00060007-00080009"
                        || value == "60E48A46-EBE2DD11-84983085-A93F2287")
                    {
                        //value = Page[3].Group[0].Item[0].Value;
                        value += Page[1].Group.FirstOrDefault(g => g.Title == "Сеть").Item.FirstOrDefault(i => i.Title == "Первичный адрес MAC").Value;
                    }

                    return value;
                }

                catch
                {
                    //return mac address at least
                    return Page[1].Group.FirstOrDefault(g => g.Title == "Сеть").Item.FirstOrDefault(i => i.Title == "Первичный адрес MAC").Value;
                }
            }
        }

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
                return hddString.Length < 3 ? string.Empty : hddString.Remove(hddString.Length - 2);
            }
        }

        public string VideoAdapter
        {
            get
            {
                var page = Page.FirstOrDefault(p => p.Title == "Видео Windows");
                var videoAdapter = page == null ? string.Empty : page.Device[0].Group[0].Item[0].Value;
                if (videoAdapter.Length > 3)
                    return videoAdapter;
                if (Page[1] != null)
                    return Page[1].Group[2].Item[1].Value;
                return page?.Device[1] == null ? string.Empty : page.Device[1].Title;
            }
        }

        public string Monitor
        {
            get
            {
                var page = Page.FirstOrDefault(p => p.Title == "Монитор");
                if (page == null)
                    return string.Empty;
                var sb = new StringBuilder();
                foreach (var dev in page.Device)
                {
                    if (dev.Title.Contains("NoDB"))
                        continue;
                    sb.Append(dev.Title);
                    sb.Append(", ");
                }
                var monitors = sb.ToString();
                if (monitors.Length < 3)
                    return string.Empty;
                return monitors.Remove(monitors.Length - 2);
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
