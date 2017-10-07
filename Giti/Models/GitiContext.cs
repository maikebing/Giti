using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Giti.Models
{
	public class GitiContext : IdentityDbContext<ApplicationUser>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GitiContext(DbContextOptions options,IHttpContextAccessor httpContextAccessor) : base(options)
		{
			_httpContextAccessor = httpContextAccessor;
		}

        public GitiContext(DbContextOptions options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			// Customize the ASP.NET Identity model and override the defaults if needed.
			// For example, you can rename the ASP.NET Identity table names and more.
			// Add your customizations after calling base.OnModelCreating(builder);
		}

        public DbSet<Repository> Repositories { get; set; } 

		public override int SaveChanges()
		{
			AddTimestamps();
			return base.SaveChanges();
		}

        public override Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            AddTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
		
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

		private void AddTimestamps()
		{
           
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            var currentUsername = !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.User?.Identity?.Name)
				? _httpContextAccessor.HttpContext.User.Identity.Name
				: "Anonymous";

			foreach (var entity in entities)
			{
				try
				{
					if (entity.State == EntityState.Added)
					{
						((BaseEntity)entity.Entity).DateCreated = DateTime.UtcNow;
						((BaseEntity)entity.Entity).UserCreated = currentUsername;
                        ((BaseEntity)entity.Entity).UserCreatedIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
					}

					((BaseEntity)entity.Entity).DateModified = DateTime.UtcNow;
					((BaseEntity)entity.Entity).UserModified = currentUsername;
					((BaseEntity)entity.Entity).UserModifiedIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
				}
                //ToDo: log execption
				catch { }

			}
		}

	}

}
