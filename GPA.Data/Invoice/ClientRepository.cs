﻿using GPA.Common.Entities.Invoice;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Invoice
{
    public interface IClientRepository : IRepository<Client>
    {
        Task UpdateAsync(Client client);
        Task<List<ClientCredit>> GetCredits(Guid clientId);
    }

    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task UpdateAsync(Client client)
        {
            await _context.ClientCredits.Where(x => x.ClientId == client.Id).ExecuteDeleteAsync();
            _context.Client.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClientCredit>> GetCredits(Guid clientId)
        {
           return await _context.ClientCredits
                .Where(x => x.ClientId == clientId)
                .ToListAsync();
        }
    }
}
