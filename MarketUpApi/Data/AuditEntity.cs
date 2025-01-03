namespace MarketUpApi.Data
{
    public class AuditEntity
    {
        public bool IsDeleted { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
