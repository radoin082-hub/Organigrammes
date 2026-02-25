using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Compta.Ledger.Core.orgTestapp.Entities;


public class Node
{
	public Guid Id { get; set; }
	
	public HierarchyId Route { get; set; } = HierarchyId.GetRoot();
	
	public string Name { get; set; } = string.Empty;

	[NotMapped]
	public int Level => Route?.GetLevel() ?? 0;	
	public int SortOrder { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }

	[NotMapped]
	public Node? Parent { get; set; }

	[NotMapped]
	public List<Node> Children { get; set; } = new List<Node>();
}
