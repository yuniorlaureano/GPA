﻿using GPA.Entities;
using GPA.Entities.Common;

namespace GPA.Common.Entities.Inventory
{
    public class Product : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string BarCode { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public Guid UnitId { get; set; }
        public Unit Unit { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public Guid? ProductLocationId { get; set; }
        public ProductLocation? ProductLocation { get; set; }

        public Guid ItemId { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}
