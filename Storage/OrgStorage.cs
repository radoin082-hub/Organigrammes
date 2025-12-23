using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Entities;

namespace Compta.Ledger.Core.orgTestapp.Storage;

/// <summary>
/// Entity Framework Core implementation of IOrgStorage
/// Uses HierarchyId for all hierarchy operations - no ParentId
/// </summary>
public class OrgStorage : IOrgStorage
{
	private readonly OrgDbContext _context;
	private IDbContextTransaction? _currentTransaction;

	public OrgStorage(OrgDbContext context)
	{
		_context = context;
	}

	/// <summary>
	/// Creates a new node in storage
	/// </summary>
	public Guid CreateNode(Node node)
	{
		_context.Nodes.Add(node);
		return node.Id;
	}

	/// <summary>
	/// Gets a node by ID from storage
	/// </summary>
	public Node? GetNodeById(Guid nodeId)
	{
		return _context.Nodes.Find(nodeId);
	}

	/// <summary>
	/// Gets all nodes ordered by Route (hierarchical order)
	/// </summary>
	public List<Node> GetAllNodes()
	{
		return _context.Nodes
			.OrderBy(n => n.Route)
			.ToList();
	}

	/// <summary>
	/// Gets direct children using HierarchyId - parent is Route.GetAncestor(1)
	/// </summary>
	public List<Node> GetNodesChildren(HierarchyId parentRoute)
	{
		var parentLevel = parentRoute.GetLevel();
		return _context.Nodes
			.Where(n => n.Route.IsDescendantOf(parentRoute) 
			         && n.Route != parentRoute
			         && n.Route.GetLevel() == parentLevel + 1)
			.OrderBy(n => n.Route)
			.ToList();
	}

	/// <summary>
	/// Updates a node in storage
	/// </summary>
	public void UpdateNode(Node node)
	{
		_context.Nodes.Update(node);
	}

	/// <summary>
	/// Deletes a node from storage
	/// </summary>
	public void DeleteNode(Node node)
	{
		_context.Nodes.Remove(node);
	}

	/// <summary>
	/// Deletes all nodes under a route using HierarchyId.IsDescendantOf
	/// </summary>
	public void DeleteNodesByRoute(HierarchyId route)
	{
		var descendants = _context.Nodes
			.Where(n => n.Route.IsDescendantOf(route))
			.OrderByDescending(n => n.Route.GetLevel())
			.ToList();

		foreach (var node in descendants)
		{
			_context.Nodes.Remove(node);
		}
	}

	/// <summary>
	/// Gets all descendants using HierarchyId.IsDescendantOf
	/// </summary>
	public List<Node> GetDescendants(HierarchyId route)
	{
		return _context.Nodes
			.Where(n => n.Route.IsDescendantOf(route) && n.Route != route)
			.OrderBy(n => n.Route)
			.ToList();
	}

	/// <summary>
	/// Gets the count of root nodes (Level 1 = /1/, /2/, etc.)
	/// Root nodes are direct children of "/" (GetRoot)
	/// </summary>
	public int GetRootNodesCount()
	{
		return _context.Nodes.Count(n => n.Route.GetLevel() == 1);
	}

	/// <summary>
	/// Gets the last root node ordered by Route
	/// Root nodes are at Level 1 (like /1/, /2/, /3/)
	/// </summary>
	public Node? GetLastRootNode()
	{
		return _context.Nodes
			.Where(n => n.Route.GetLevel() == 1)
			.OrderBy(n => n.Route)
			.LastOrDefault();
	}

	/// <summary>
	/// Gets the last child under a parent using HierarchyId
	/// Children are nodes where Route.GetAncestor(1) == parentRoute
	/// </summary>
	public Node? GetLastChild(HierarchyId parentRoute)
	{
		var parentLevel = parentRoute.GetLevel();
		return _context.Nodes
			.Where(n => n.Route.IsDescendantOf(parentRoute) 
			         && n.Route != parentRoute
			         && n.Route.GetLevel() == parentLevel + 1)
			.OrderBy(n => n.Route)
			.LastOrDefault();
	}

	/// <summary>
	/// Gets the parent node using HierarchyId.GetAncestor(1)
	/// </summary>
	public Node? GetParent(HierarchyId route)
	{
		if (route.GetLevel() <= 1)
			return null; // Root nodes have no parent
			
		var parentRoute = route.GetAncestor(1);
		return _context.Nodes.FirstOrDefault(n => n.Route == parentRoute);
	}

	/// <summary>
	/// Gets all ancestors using HierarchyId.GetAncestor
	/// </summary>
	public List<Node> GetAncestors(HierarchyId route, int level)
	{
		var ancestors = new List<Node>();
		
		for (int i = 1; i <= level; i++)
		{
			var ancestorRoute = route.GetAncestor(i);
			var ancestor = _context.Nodes.FirstOrDefault(n => n.Route == ancestorRoute);
			if (ancestor != null)
				ancestors.Add(ancestor);
		}
		
		return ancestors;
	}

	/// <summary>
	/// Begins a database transaction
	/// </summary>
	public IDbContextTransaction BeginTransaction()
	{
		_currentTransaction = _context.Database.BeginTransaction();
		return _currentTransaction;
	}

	/// <summary>
	/// Commits the current transaction
	/// </summary>
	public void CommitTransaction()
	{
		if (_currentTransaction != null)
		{
			_currentTransaction.Commit();
			_currentTransaction.Dispose();
			_currentTransaction = null;
		}
	}

	/// <summary>
	/// Rolls back the current transaction
	/// </summary>
	public void RollbackTransaction()
	{
		if (_currentTransaction != null)
		{
			_currentTransaction.Rollback();
			_currentTransaction.Dispose();
			_currentTransaction = null;
		}
	}

	/// <summary>
	/// Saves changes to storage
	/// </summary>
	public void SaveChanges()
	{
		_context.SaveChanges();
	}

	public void Dispose()
	{
		_currentTransaction?.Dispose();
		_context?.Dispose();
	}
}