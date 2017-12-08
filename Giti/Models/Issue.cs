using System;
namespace Giti.Models
{
    public class Issue : BaseEntity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid RepositoryId { get; set; }
        public long Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPull { get; set; }
        public bool IsClosed { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Repository Repository { get; set; }
    }
}
