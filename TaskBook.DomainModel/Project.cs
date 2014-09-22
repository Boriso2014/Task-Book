﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBook.DomainModel
{
    public class Project: Entity
    {
        [Required]
        [MaxLength(32)]
        [Index(IsUnique = true)]
        public string Title { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTimeOffset? DeletedDate { get; set; }

        public ICollection<ProjectUsers> Users { get; set; }
    }
}