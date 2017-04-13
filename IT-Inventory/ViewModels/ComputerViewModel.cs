﻿using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class ComputerViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Имя компьютера")]
        public string ComputerName { get; set; }

        [Display(Name = "Процессор")]
        public string Cpu { get; set; }

        [Display(Name = "Память")]
        public int Ram { get; set; }

        [Display(Name = "Материнская плата")]
        public string MotherBoard { get; set; }

        [Display(Name = "Видеокарта")]
        public string VideoAdapter { get; set; }

        [Display(Name = "Владелец")]
        public string Owner { get; set; }

        [Display(Name = "Владелец")]
        public int OwnerId { get; set; }

        [Display(Name = "Установленные программы")]
        public string[] Software { get; set; }

        [Display(Name = "Установленные программы")]
        public string SoftwareString
        {
            get
            {
                return string.Join("\n", Software);
            }
            set
            {
                Software = value.Replace("\r", "").Split(new[] { "\n" }, StringSplitOptions.None);
            }
        }

        public bool HasRequests { get; set; }
    }
}