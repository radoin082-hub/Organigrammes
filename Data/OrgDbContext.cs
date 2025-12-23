using Compta.Ledger.Core.orgTestapp.Entities;
using Microsoft.EntityFrameworkCore;

namespace Compta.Ledger.Core.orgTestapp.Data;

/// <summary>
/// Entity Framework DbContext for organizational hierarchy data
/// Uses HierarchyId only - no ParentId dependency
/// </summary>
public class OrgDbContext : DbContext
{
	public OrgDbContext(DbContextOptions<OrgDbContext> options) : base(options)
	{
	}

	/// <summary>
	/// Nodes table - contains all organizational hierarchy nodes
	/// </summary>
	public DbSet<Node> Nodes { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Configure Node entity
		modelBuilder.Entity<Node>(entity =>
		{
			// Primary key
			entity.HasKey(e => e.Id);

			// Table name
			entity.ToTable("Nodes");

			// Properties
			entity.Property(e => e.Id)
				.HasColumnName("Id")
				.IsRequired();

			entity.Property(e => e.Name)
				.HasColumnName("Name")
				.HasMaxLength(255)
				.IsRequired();

			// HierarchyId Route - PRIMARY hierarchy field
			entity.Property(e => e.Route)
				.HasColumnName("Route")
				.HasColumnType("hierarchyid")
				.IsRequired();

			// Level is computed from Route.GetLevel() - ignore for EF
			entity.Ignore(e => e.Level);

			// SortOrder is no longer persisted in the database
			entity.Ignore(e => e.SortOrder);

			entity.Property(e => e.CreatedAt)
				.HasColumnName("CreatedAt")
				.HasColumnType("datetime2")
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.HasColumnName("UpdatedAt")
				.HasColumnType("datetime2")
				.IsRequired(false);

			// Parent and Children are navigation properties only (not mapped to columns)
			entity.Ignore(e => e.Parent);
			entity.Ignore(e => e.Children);

			// Unique index on Route for fast hierarchy queries
			entity.HasIndex(e => e.Route)
				.HasDatabaseName("IX_Nodes_Route")
				.IsUnique();
		});
	}
}