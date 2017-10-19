using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IT_Inventory.Models
{
    public class Weather
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public string Desctiption { get; set; }
        public string Icon { get; set; }
        public int Temp { get; set; }
        public int MaxTemp { get; set; }
        public int MinTemp { get; set; }
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public DateTime Updated { get; set; }
    }
}