namespace MarketUpApi.Data
{
    public interface IAppDbContext
    {
        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
