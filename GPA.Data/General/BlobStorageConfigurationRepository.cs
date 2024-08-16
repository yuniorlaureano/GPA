using GPA.Entities.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GPA.Data.General
{
    public interface IBlobStorageConfigurationRepository : IRepository<BlobStorageConfiguration>
    {
        Task CreateConfigurationAsync(BlobStorageConfiguration blobStorageConfiguration);
        Task UpdateConfigurationAsync(BlobStorageConfiguration blobStorageConfiguration);
    }

    public class BlobStorageConfigurationRepository : Repository<BlobStorageConfiguration>, IBlobStorageConfigurationRepository
    {
        public BlobStorageConfigurationRepository(GPADbContext _dbContext) : base(_dbContext)
        {
        }

        public async Task CreateConfigurationAsync(BlobStorageConfiguration blobStorageConfiguration)
        {
            var query = @"
                IF(@Current = 1)
                BEGIN
                    UPDATE [GPA].[General].[BlobStorageConfigurations] 
                        SET [Current] = 0
                END

                INSERT INTO [GPA].[General].[BlobStorageConfigurations]
                (
                     [Identifier]
                    ,[Provider]
                    ,[PublicUrl]
                    ,[Value]
                    ,[Current]
                    ,[CreatedBy]
                    ,[CreatedAt]                    
                    ,[Deleted]

                )
                VALUES 
                (
                    @Identifier,
                    @Provider,
                    @PublicUrl,
                    @Value,
                    @Current,
                    @CreatedBy,
                    @CreatedAt,	
                    0	
                )
";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Identifier", SqlDbType.NVarChar) { Value = blobStorageConfiguration.Identifier },
                new SqlParameter("@Provider", SqlDbType.NVarChar) { Value = blobStorageConfiguration.Provider },
                new SqlParameter("@PublicUrl", SqlDbType.NVarChar) { Value = blobStorageConfiguration.PublicUrl },
                new SqlParameter("@Value", SqlDbType.NVarChar) { Value = blobStorageConfiguration.Value },
                new SqlParameter("@Current", SqlDbType.Bit) { Value = blobStorageConfiguration.Current },
                new SqlParameter("@CreatedBy", SqlDbType.UniqueIdentifier) { Value = blobStorageConfiguration.CreatedBy },
                new SqlParameter("@CreatedAt", SqlDbType.DateTimeOffset) { Value = blobStorageConfiguration.CreatedAt },
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters);
        }

        public async Task UpdateConfigurationAsync(BlobStorageConfiguration blobStorageConfiguration)
        {
            var query = @"
                    IF EXISTS(SELECT 1 FROM [GPA].[General].[BlobStorageConfigurations] WHERE Id = @Id)
                    BEGIN
                        IF(@Current = 1)
                        BEGIN
                            UPDATE [GPA].[General].[BlobStorageConfigurations]
                                SET [Current] = 0
                        END

                        UPDATE [GPA].[General].[BlobStorageConfigurations]
                            SET 
                                 [Identifier] = @Identifier
                                ,[Provider] = @Provider
                                ,[PublicUrl] = @PublicUrl
                                ,[Value] = @Value
                                ,[Current] = @Current
                                ,[UpdatedBy] = @UpdatedBy
                                ,[UpdatedAt] = @UpdatedAt
                        WHERE Id = @Id
                    END
";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = blobStorageConfiguration.Id },
                new SqlParameter("@Identifier", SqlDbType.NVarChar) { Value = blobStorageConfiguration.Identifier },
                new SqlParameter("@Provider", SqlDbType.NVarChar) { Value = blobStorageConfiguration.Provider },
                new SqlParameter("@PublicUrl", SqlDbType.NVarChar) { Value = blobStorageConfiguration.PublicUrl },
                new SqlParameter("@Value", SqlDbType.NVarChar) { Value = blobStorageConfiguration.Value },
                new SqlParameter("@Current", SqlDbType.Bit) { Value = blobStorageConfiguration.Current },
                new SqlParameter("@UpdatedBy", SqlDbType.UniqueIdentifier) { Value = blobStorageConfiguration.UpdatedBy },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTimeOffset) { Value = blobStorageConfiguration.UpdatedAt },
            };

            await _context.Database.ExecuteSqlRawAsync(query, parameters);
        }
    }
}
