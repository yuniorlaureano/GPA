﻿namespace GPA.Entities.General
{
    public class Unit : Entity<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
