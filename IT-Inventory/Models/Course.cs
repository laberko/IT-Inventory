using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IT_Inventory.Models
{
    public class Course
    {
        public int Id { get; set; }
        public int ValuteCode { get; set; }
        public double ValCourse { get; set; }
        public DateTime CourseDate { get; set; }
    }
}