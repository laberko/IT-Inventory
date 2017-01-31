using System.ComponentModel.DataAnnotations;


namespace IT_Inventory.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Display(Name = "Учетная запись")]
        public string AccountName { get; set; }

        [Display(Name = "Сотрудник ДИТ")]
        public bool IsItUser { get; set; }
    }
}