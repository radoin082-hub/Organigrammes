using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Entities;

namespace Compta.Ledger.Core.orgTestapp.Storage;

/// <summary>
/// Repository interface for organizational hierarchy data access
/// Pure data access layer - uses HierarchyId for all hierarchy operations
/// No ParentId - parent/child relationships derived from HierarchyId Route
/// </summary>
public interface IOrgStorage
{
	/// <summary>
	/// Creates a new node in storage
	/// </summary>
	Guid CreateNode(Node node);

	/// <summary>
	/// Gets a node by ID from storage
	/// </summary>
	Node? GetNodeById(Guid nodeId);

	/// <summary>
	/// Gets all nodes from storage ordered by Route
	/// </summary>
	List<Node> GetAllNodes();

	/// <summary>
	/// Gets direct children of a node using HierarchyId
	/// Children are nodes where Route.GetAncestor(1) == parentRoute
	/// </summary>
	List<Node> GetNodesChildren(HierarchyId parentRoute);

	/// <summary>
	/// Updates a node in storage
	/// </summary>
	void UpdateNode(Node node);

	/// <summary>
	/// Deletes a node from storage
	/// </summary>
	void DeleteNode(Node node);

	/// <summary>
	/// Deletes a node and all descendants using HierarchyId.IsDescendantOf
	/// </summary>
	void DeleteNodesByRoute(HierarchyId route);

	/// <summary>
	/// Gets all descendants using HierarchyId.IsDescendantOf
	/// </summary>
	List<Node> GetDescendants(HierarchyId route);

	/// <summary>
	/// Gets the count of root nodes (Level 1 = /1/, /2/, etc.)
	/// </summary>
	int GetRootNodesCount();

	/// <summary>
	/// Gets the last root node ordered by Route
	/// </summary>
	Node? GetLastRootNode();

	/// <summary>
	/// Gets the last child under a parent using HierarchyId
	/// </summary>
	Node? GetLastChild(HierarchyId parentRoute);

	/// <summary>
	/// Gets the parent node using HierarchyId.GetAncestor(1)
	/// </summary>
	Node? GetParent(HierarchyId route);

	/// <summary>
	/// Gets all ancestors using HierarchyId.GetAncestor
	/// </summary>
	List<Node> GetAncestors(HierarchyId route, int level);

	/// <summary>
	/// Begins a database transaction
	/// </summary>
	IDbContextTransaction BeginTransaction();

	/// <summary>
	/// Commits the current transaction
	/// </summary>
	void CommitTransaction();

	/// <summary>
	/// Rolls back the current transaction
	/// </summary>
	void RollbackTransaction();

	/// <summary>
	/// Saves changes to storage
	/// </summary>
	void SaveChanges();
}
