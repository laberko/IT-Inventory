using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Inventory.Models
{
    public class Person
    {
        public Person()
        {
            SupportRequests = new HashSet<SupportRequest>();
            //Computers = new HashSet<Computer>();
        }

        [Key]
        public int Id { get; set; }

        public bool NonExisting { get; set; }

        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Display(Name = "Учетная запись")]
        public string AccountName { get; set; }

        [Display(Name = "Почта")]
        public string Email { get; set; }

        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Должность")]
        public string Position { get; set; }

        [Display(Name = "День рождения")]
        public DateTime? Birthday { get; set; }

        [Display(Name = "Учетная запись создана")]
        public DateTime? CreationDate { get; set; }

        [Display(Name = "Департамент")]
        public virtual Department Dep { get; set; }

        public int Dep1Index { get; set; }

        public virtual Department Dep2 { get; set; }

        public int Dep2Index { get; set; }

        [Display(Name = "Группа")]
        public string Group { get; set; }

        public byte[] PhotoBytes { get; set; }

        public virtual ICollection<SupportRequest> SupportRequests { get; set; }

        //[Display(Name = "Компьютер(ы)")]
        //public virtual ICollection<Computer> Computers { get; set; }

        [NotMapped]
        public string ShortName => FullName.GetShortName();

        [NotMapped]
        public string BirthdayString => Birthday?.ToString("d MMMM yyyy") ?? string.Empty;

        [NotMapped]
        public string CreationString => CreationDate?.ToString("d MMMM yyyy") ?? string.Empty;

        [NotMapped]
        public string DepartmentString => Dep2 == null ? Dep.Name : Dep.Name + ", " + Dep2.Name;

        [NotMapped]
        public bool IsInGroup
        {
            get
            {
                if (Dep2 == null)
                    return Dep.Name != Group;
                return Dep.Name != Group && Dep2.Name != Group;
            }
        }
    }
}