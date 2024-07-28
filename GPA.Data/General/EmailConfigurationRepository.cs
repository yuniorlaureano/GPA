using GPA.Entities.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GPA.Data.General
{
    public interface IEmailConfigurationRepository : IRepository<EmailConfiguration>
    {
        Task CreateConfigurationAsync(EmailConfiguration emailConfiguration);
        Task UpdateConfigurationAsync(EmailConfiguration emailConfiguration);
    }

    public class EmailConfigurationRepository : Repository<EmailConfiguration>, IEmailConfigurationRepository
    {
        public EmailConfigurationRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task CreateConfigurationAsync(EmailConfiguration emailConfiguration)
        {
            var query = @"
                DECLARE 
                    @CreatedAt DATETIMEOFFSET = SYSDATETIMEOFFSET();

                IF(@Current = 1)
                BEGIN
	                UPDATE [GPA].[General].[EmailConfigurations] 
		                SET [Current] = 0
                END

                INSERT INTO [GPA].[General].[EmailConfigurations]
                (
                     [Identifier]
                    ,[Engine]
                    ,[Value]
                    ,[From]
                    ,[Current]
                    ,[CreatedBy]
                    ,[CreatedAt]                    
                    ,[Deleted]

                )
                VALUES 
                (
	                @Identifier,
                    @Engine,
                    @Value,
                    @From,
	                @Current,
	                @CreatedBy,
	                @CreatedAt,	
	                0	
                )
";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Identifier", SqlDbType.NVarChar) { Value = emailConfiguration.Identifier },
                new SqlParameter("@Engine", SqlDbType.NVarChar) { Value = emailConfiguration.Engine },
                new SqlParameter("@Value", SqlDbType.NVarChar) { Value = emailConfiguration.Value },
                new SqlParameter("@From", SqlDbType.NVarChar) { Value = emailConfiguration.From },
                new SqlParameter("@Current", SqlDbType.Bit) { Value = emailConfiguration.Current },
                new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) { Value = emailConfiguration.CreatedBy },
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters);
        }

        public async Task UpdateConfigurationAsync(EmailConfiguration emailConfiguration)
        {
            var query = @"
                DECLARE 
                    @UpdatedAt DATETIMEOFFSET = SYSDATETIMEOFFSET();

                IF EXISTS(SELECT 1 FROM [GPA].[General].[EmailConfigurations] WHERE Id = @Id)
                BEGIN
	                IF(@Current = 1)
	                BEGIN
		                UPDATE [GPA].[General].[EmailConfigurations] 
			                SET [Current] = 0
	                END

	                UPDATE [GPA].[General].[EmailConfigurations] 
		                SET 
			                 [Identifier] = @Identifier
			                ,[Engine] = @Engine
			                ,[Value] = @Value
			                ,[From] = @From
			                ,[Current] = @Current
			                ,[UpdatedBy] = @UpdatedBy
			                ,[UpdatedAt] = @UpdatedAt
	                WHERE Id = @Id
                END
";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = emailConfiguration.Id },
                new SqlParameter("@Identifier", SqlDbType.NVarChar) { Value = emailConfiguration.Identifier },
                new SqlParameter("@Engine", SqlDbType.NVarChar) { Value = emailConfiguration.Engine },
                new SqlParameter("@Value", SqlDbType.NVarChar) { Value = emailConfiguration.Value },
                new SqlParameter("@From", SqlDbType.NVarChar) { Value = emailConfiguration.From },
                new SqlParameter("@Current", SqlDbType.Bit) { Value = emailConfiguration.Current },
                new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) { Value = emailConfiguration.CreatedBy },
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters);
        }
    }
}
