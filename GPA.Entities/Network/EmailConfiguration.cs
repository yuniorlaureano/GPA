﻿namespace GPA.Entities.Network
{
    public class EmailConfiguration
    {
        public Guid Id { get; set; }
        public string Provider { get; set; }
        public string Value { get; set; }
        public string From { get; set; }
        public bool Current { get; set; }
    }
}
