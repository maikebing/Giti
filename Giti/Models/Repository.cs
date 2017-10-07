using System;
namespace Giti.Models
{
    public class Repository : BaseEntity
    {
        public Guid Id { get; set; }
		public string UserId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string DefaultBranch { get; set; }	

        public virtual ApplicationUser User { get; set; }
    }
}
