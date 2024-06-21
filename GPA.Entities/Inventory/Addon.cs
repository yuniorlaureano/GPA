﻿using GPA.Entities;

namespace GPA.Common.Entities.Inventory
{
    public class Addon : Entity<Guid>
    {
        public string Concept { get; set; }
        public bool IsDiscount { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }

        public ICollection<ProductAddon> ProductAddons { get; set; }
    }
}
