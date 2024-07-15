using AutoFixture;
using GPA.Common.Entities.Security;
using GPA.Data;
using GPA.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Security.Service
{
    public class UsersServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly GPADbContext _context;

        public UsersServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependenyBuilder.GetServices();
            _context = _services.GetRequiredService<GPADbContext>();
        }

        [Fact]
        public async Task ShouldCreate20Users()
        {
            var passwordHasher = new PasswordHasher<GPAUser>();
            var users = new List<GPAUser>();
            for (int i = 0; i < 20; i++)
            {
                var user = _fixture
                    .Build<GPAUser>()
                    .With(x => x.FirstName, "User " + i)
                    .With(x => x.LastName, "User " + i)
                    .With(x => x.UserName, "user" + i)
                    .With(x => x.NormalizedUserName, "user" + i)
                    .With(x => x.Email, $"user{i}@test.com")
                    .Without(x => x.UserRoles)
                    .Without(x => x.UserTokens)
                    .Without(x => x.UserLogins)
                    .Without(x => x.Profiles)
                    .Create();
                user.PasswordHash = passwordHasher.HashPassword(user, "user" + i);
                users.Add(user);
            }
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            Assert.True(true);
        }
    }
}