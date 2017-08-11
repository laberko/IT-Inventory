using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModels
{
    public class SupportIndexViewModel
    {
        public List<SupportRequest> SupportRequests;
        public Pager Pager { get; set; }
        public string SearchString { get; set; }
        public bool IsItUser { get; set; }
        public int UserId { get; set; }
    }
}