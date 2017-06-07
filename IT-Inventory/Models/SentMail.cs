using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class SentMail
    {
        [Key]
        public int Id { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public DateTime Date { get; set; }

        public StaticData.MailType Type { get; set; }

    }
}