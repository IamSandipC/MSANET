﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MenuRecommendation.Models
{
    public class Menu
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
