using MarketUpApi.Data;
using MarketUpApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace MarketUpApi.Entities
{
    public class Role : AuditEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }


        [StringLength(250)]
        public string Description { get; set; }

        public bool IsBase { get; set; }

        public RoleCode? Code { get; set; }

        public long? ParentId { get; set; }

        public virtual Role Parent { get; set; }

        public RoleStatus Status { get; set; }
    }
}
