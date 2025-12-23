using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Entities;
using Compta.Ledger.Core.orgTestapp.Storage;
using Microsoft.EntityFrameworkCore;

namespace Compta.Ledger.Core.orgTestapp.Services;

/// <summary>
/// Organization service using HierarchyId for tree structure
/// Uses IOrgStorage for data persistence
/// All hierarchy operations are based on HierarchyId Route - no ParentId dependency
/// </summary>
public class OrgService : IOrgService
{
	private readonly IOrgStorage _storage;

	public OrgService(IOrgStorage storage)
	{
		_storage = storage;
	}

	/// <summary>
	/// Creates a new root organization
	/// </summary>
	public Guid CreateOrg(Node node)
	{
		if (node == null)
			throw new ArgumentNullException(nameof(node), "Node cannot be null");

		node.Id = Guid.NewGuid();

		// Get the last root node to generate unique route for new root
		var lastRoot = _storage.GetLastRootNode();

		// Generate unique root HierarchyId: /1/, /2/, /3/, etc.
		node.Route = HierarchyId.GetRoot().GetDescendant(lastRoot?.Route, null);
		node.SortOrder = _storage.GetRootNodesCount();
		node.CreatedAt = DateTime.UtcNow;

		_storage.CreateNode(node);
		_storage.SaveChanges();

		return node.Id;
	}

	/// <summary>
	/// Adds a child node to a parent by parent ID
	/// </summary>
	public Guid AddChildToNode(Guid parentId, Node childNode)
	{
		var parentNode = _storage.GetNodeById(parentId);
		if (parentNode == null)
			throw new KeyNotFoundException($"Parent node with ID {parentId} not found");

		return AddChildToNode(parentNode, childNode);
	}

	/// <summary>
	/// Adds a child node to a parent using HierarchyId
	/// </summary>
	public Guid AddChildToNode(Node parentNode, Node childNode)
	{
		if (parentNode == null)
			throw new ArgumentNullException(nameof(parentNode), "Parent node cannot be null");
		if (childNode == null)
			throw new ArgumentNullException(nameof(childNode), "Child node cannot be null");

		childNode.Id = Guid.NewGuid();
		childNode.CreatedAt = DateTime.UtcNow;

		// Get last child under this parent using HierarchyId
		var lastChild = _storage.GetLastChild(parentNode.Route);

		// Calculate new HierarchyId using GetDescendant
		childNode.Route = parentNode.Route.GetDescendant(
			lastChild?.Route,  // After last child
			null               // Before nothing (append)
		);

		childNode.SortOrder = _storage.GetNodesChildren(parentNode.Route).Count;

		_storage.CreateNode(childNode);
		_storage.SaveChanges();

		return childNode.Id;
	}

	/// <summary>
	/// Gets a node by ID
	/// </summary>
	public Node GetNodeById(Guid nodeId)
	{
		var node = _storage.GetNodeById(nodeId);
		if (node == null)
			throw new KeyNotFoundException($"Node with ID {nodeId} not found");

		return node;
	}

	/// <summary>
	/// Gets all nodes ordered by HierarchyId (hierarchical order)
	/// </summary>
	public List<Node> GetAllNodes()
	{
		var nodes = _storage.GetAllNodes();
		BuildHierarchy(nodes);
		return nodes;
	}

	/// <summary>
	/// Gets children of a node by parent ID
	/// </summary>
	public List<Node> GetNodesChildren(Guid nodeId)
	{
		var node = _storage.GetNodeById(nodeId);
		if (node == null)
			throw new KeyNotFoundException($"Node with ID {nodeId} not found");

		return GetNodesChildren(node);
	}

	/// <summary>
	/// Gets direct children of a node using HierarchyId
	/// </summary>
	public List<Node> GetNodesChildren(Node node)
	{
		if (node == null)
			throw new ArgumentNullException(nameof(node), "Node cannot be null");

		return _storage.GetNodesChildren(node.Route);
	}

	/// <summary>
	/// Deletes a node by ID
	/// </summary>
	public void DeleteNode(Guid nodeId)
	{
		var node = _storage.GetNodeById(nodeId);
		if (node == null)
			throw new KeyNotFoundException($"Node with ID {nodeId} not found");

		DeleteNode(node);
	}

	/// <summary>
	/// Deletes a node and all its descendants using HierarchyId
	/// </summary>
	public void DeleteNode(Node node)
	{
		if (node == null)
			throw new ArgumentNullException(nameof(node), "Node cannot be null");

		// Delete all descendants using HierarchyId (includes the node itself)
		_storage.DeleteNodesByRoute(node.Route);
		_storage.SaveChanges();
	}

	/// <summary>
	/// Moves a node to a new parent by IDs
	/// </summary>
	public void MoveNode(Guid nodeId, Guid newParentId)
	{
		var node = _storage.GetNodeById(nodeId);
		if (node == null)
			throw new KeyNotFoundException($"Node with ID {nodeId} not found");

		var newParentNode = _storage.GetNodeById(newParentId);
		if (newParentNode == null)
			throw new KeyNotFoundException($"Parent node with ID {newParentId} not found");

		MoveNode(node, newParentNode);
	}

	/// <summary>
	/// Moves a node to a new parent using HierarchyId
	/// </summary>
	public void MoveNode(Node node, Node newParentNode)
	{
		if (node == null)
			throw new ArgumentNullException(nameof(node), "Node cannot be null");
		if (newParentNode == null)
			throw new ArgumentNullException(nameof(newParentNode), "Parent node cannot be null");

		// Prevent circular reference using IsDescendantOf
		if (newParentNode.Route.IsDescendantOf(node.Route))
			throw new InvalidOperationException("Cannot move a node to one of its descendants");

		// Get last child under new parent
		var lastChild = _storage.GetLastChild(newParentNode.Route);

		var oldRoute = node.Route;
		var newRoute = newParentNode.Route.GetDescendant(
			lastChild?.Route,
			null
		);

		// Get all descendants to update their routes
		var descendants = _storage.GetDescendants(oldRoute);

		// Update node's route
		node.Route = newRoute;
		node.UpdatedAt = DateTime.UtcNow;
		_storage.UpdateNode(node);

		// Update all descendants' routes using GetReparentedValue
		foreach (var descendant in descendants)
		{
			descendant.Route = descendant.Route.GetReparentedValue(oldRoute, newRoute)!;
			descendant.UpdatedAt = DateTime.UtcNow;
			_storage.UpdateNode(descendant);
		}

		_storage.SaveChanges();
	}

	/// <summary>
	/// Renames a node by ID
	/// </summary>
	public void RenameNode(Guid nodeId, string newName)
	{
		var node = _storage.GetNodeById(nodeId);
		if (node == null)
			throw new KeyNotFoundException($"Node with ID {nodeId} not found");

		RenameNode(node, newName);
	}

	/// <summary>
	/// Renames a node
	/// </summary>
	public void RenameNode(Node node, string newName)
	{
		if (node == null)
			throw new ArgumentNullException(nameof(node), "Node cannot be null");
		if (string.IsNullOrWhiteSpace(newName))
			throw new ArgumentException("Node name cannot be empty", nameof(newName));

		node.Name = newName.Trim();
		node.UpdatedAt = DateTime.UtcNow;
		_storage.UpdateNode(node);
		_storage.SaveChanges();
	}

	private void BuildHierarchy(List<Node> nodes)
	{
		foreach (var node in nodes)
		{
			node.Children.Clear();
			node.Parent = null;
		}

		foreach (var node in nodes)
		{
			var level = node.Route.GetLevel();
			if (level > 1)
			{
				var parentRoute = node.Route.GetAncestor(1);
				var parent = nodes.FirstOrDefault(n => n.Route == parentRoute);

				if (parent != null)
				{
					node.Parent = parent;
					if (!parent.Children.Contains(node))
					{
						parent.Children.Add(node);
					}
				}
			}
		}

		foreach (var node in nodes)
		{
			if (node.Children.Any())
			{
				var sortedChildren = node.Children
					.OrderBy(c => c.Route)
					.ToList();

				node.Children.Clear();
				foreach (var child in sortedChildren)
				{
					node.Children.Add(child);
				}
			}
		}
	}
}