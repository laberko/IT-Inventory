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
        }

        [Key]
        public int Id { get; set; }

        //level in Dep
        public int LevelDep1 { get; set; }

        //level in Dep2
        public int LevelDep2 { get; set; }

        //person is not working anymore
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

        //person's office
        [Display(Name = "Офис")]
        public virtual Office Office { get; set; }
        
        //main department of the person
        [Display(Name = "Департамент")]
        public virtual Department Dep { get; set; }

        //sub-department in the main department
        [Display(Name = "Группа")]
        public virtual SubDepartment SubDep { get; set; }

        //order in Dep or in SubDep if not null
        public int Dep1Index { get; set; }

        //second department
        public virtual Department Dep2 { get; set; }

        //order in Dep2
        public int Dep2Index { get; set; }

        //person's picture
        public byte[] PhotoBytes { get; set; }

        public virtual ICollection<SupportRequest> SupportRequests { get; set; }

        [NotMapped]
        public string ShortName => FullName.GetShortName();

        [NotMapped]
        public string BirthdayString => Birthday?.ToString("d MMMM yyyy") ?? string.Empty;

        [NotMapped]
        public string CreationString => CreationDate?.ToString("d MMMM yyyy") ?? string.Empty;

        [NotMapped]
        public string DepartmentString => Dep2 == null ? Dep.Name : Dep.Name + ", " + Dep2.Name;
    }
}