using AutoFixture;
using GPA.Business.Services.Invoice;
using GPA.Common.DTOs.Invoice;
using GPA.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GPA.Tests.Invoice.Service
{
    public class ClientServiceTest
    {
        private readonly IFixture _fixture;
        private readonly IServiceProvider _services;
        private readonly IClientService _clientService;

        public ClientServiceTest()
        {
            var x = CleanUpDbFixture.Current;
            _fixture = new Fixture();
            _services = DependencyBuilder.GetServices();
            _clientService = _services.GetRequiredService<IClientService>();
        }

        [Fact]
        public async Task ShouldGetOne()
        {
            var client = _fixture
                .Build<ClientDto>()
                .With(x => x.Name, "Alicia")
                .With(x => x.LastName, "Meriñez")
                .With(x => x.Credits, new ClientCreditDto[] 
                {
                    new ClientCreditDto
                    {
                        Credit = 500,
                        Concept = "General"
                    },
                    new ClientCreditDto
                    {
                        Credit = 100,
                        Concept = "Dadiba"
                    }
                })
                .Without(x => x.Id)
                .Create();

            var dto = await _clientService.AddAsync(client);
            var existing = await _clientService.GetByIdAsync(dto.Id.Value);

            Assert.Equal(dto.Id, existing?.Id);
        }

        [Fact]
        public async Task ShouldGetAll()
        {
            for (int i = 0; i < 3; i++)
            {
                var client = _fixture
                    .Build<ClientDto>()
                    .Without(x => x.Id)
                    .Create();

                await _clientService.AddAsync(client);
            }

            var availables = await _clientService.GetAllAsync(new GPA.Common.DTOs.RequestFilterDto { Page = 1, PageSize = 3 });
            Assert.Equal(availables?.Data?.Count(), 3);
        }

        [Fact]
        public async Task ShouldCreateOne()
        {
            var client = _fixture
                .Build<ClientDto>()
                .With(x => x.Name, "Joaquin")
                .With(x => x.LastName, "Amilcar")
                .With(x => x.Credits, new ClientCreditDto[]
                {
                    new ClientCreditDto
                    {
                        Credit = 500,
                        Concept = "General"
                    },
                    new ClientCreditDto
                    {
                        Credit = 100,
                        Concept = "Dadiba"
                    }
                })
                .Without(x => x.Id)
                .Create();

            var added = await _clientService.AddAsync(client);
            Assert.NotNull(added);
        }

        [Fact]
        public async Task ShouldUpdate()
        {
            var client = _fixture
                .Build<ClientDto>()
                .With(x => x.Name, "Mario")
                .With(x => x.LastName, "Aljazar")
                .With(x => x.Credits, new ClientCreditDto[]
                {
                    new ClientCreditDto
                    {
                        Credit = 500,
                        Concept = "General"
                    },
                    new ClientCreditDto
                    {
                        Credit = 100,
                        Concept = "Dadiba"
                    }
                })
                .Without(x => x.Id)
                .Create();

            var added = await _clientService.AddAsync(client);
            var existing = await _clientService.GetByIdAsync(added.Id.Value);

            existing.Name = "Modified Name";

            await _clientService.UpdateAsync(existing);

            var updated = await _clientService.GetByIdAsync(added.Id.Value);

            Assert.NotEqual(updated.Name, added.Name);
        }

        [Fact]
        public async Task DeleteOne()
        {
            var client = _fixture
                .Build<ClientDto>()
                .Without(x => x.Id)
                .Create();

            var added = await _clientService.AddAsync(client);
            await _clientService.RemoveAsync(added.Id.Value);
            var existing = await _clientService.GetByIdAsync(added.Id.Value);

            Assert.Null(existing);
        }
    }
}