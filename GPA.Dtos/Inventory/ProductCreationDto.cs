﻿namespace GPA.Common.DTOs.Inventory
{
    public class ProductCreationDto
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string BarCode { get; set; }
        public DetailedDate? ExpirationDate { get; set; }
        public byte Type { get; set; }
        public Guid UnitId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ProductLocationId { get; set; }
        public Guid[]? Addons { get; set; }
    }
}
