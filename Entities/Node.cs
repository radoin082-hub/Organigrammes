using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compta.Ledger.Core.orgTestapp.Entities;

/// <summary>
/// Node entity using HierarchyId for tree structure
/// Route is the ONLY source of truth for hierarchy - no ParentId column
/// Parent/Children relationships are computed from HierarchyId Route
/// </summary>
public class Node
{
	public Guid Id { get; set; }
	
	/// <summary>
	/// HierarchyId Route - the ONLY source of truth for hierarchy
	/// Examples: /1/ (root), /1/1/ (child of /1/), /1/1/2/ (grandchild)
	/// </summary>
	public HierarchyId Route { get; set; } = HierarchyId.GetRoot();
	
	public string Name { get; set; } = string.Empty;
	
	/// <summary>
	/// Level - derived from Route.GetLevel()
	/// Level 1 = root nodes (/1/, /2/, etc.)
	/// Level 2 = children of roots (/1/1/, /1/2/, etc.)
	/// </summary>
	[NotMapped]
	public int Level => Route?.GetLevel() ?? 0;
	
	public int SortOrder { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }

	/// <summary>
	/// Parent - computed from Route.GetAncestor(1), NOT stored in database
	/// </summary>
	[NotMapped]
	public Node? Parent { get; set; }
	
	/// <summary>
	/// Children - computed from nodes where Route.GetAncestor(1) == this.Route
	/// NOT stored in database
	/// </summary>
	[NotMapped]
	public List<Node> Children { get; set; } = new List<Node>();
}
