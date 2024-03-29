﻿namespace GPA.Dtos.Security
{
    public class GPAUserDto
    {
        public Guid? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmailConfirmation { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Deleted { get; set; }
    }
}
