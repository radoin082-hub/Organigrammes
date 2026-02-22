using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Entities;

namespace Compta.Ledger.Core.orgTestapp.Storage;

public interface IOrgStorage
{
	Task<Guid> CreateNode(Node node);
	Task<Node?> GetNodeById(Guid nodeId);
	Task<List<Node>> GetAllNodes();
	Task<List<Node>> GetNodesChildren(HierarchyId parentRoute);
	Task<bool> UpdateNode(Node node);

    Task<bool> DeleteNode(int id);
	Task DeleteNodesByRoute(HierarchyId route);
    Task<List<Node>> GetDescendants(HierarchyId route);
	Task<int> GetRootNodesCount();
	Task<Node?> GetLastRootNode();
	Task<Node?> GetLastChild(HierarchyId parentRoute);
	Task<Node?> GetParent(HierarchyId route);
    Task<List<Node>> GetAncestors(HierarchyId route, int level);
	Task<IDbContextTransaction> BeginTransaction();
	Task CommitTransaction();
	Task RollbackTransaction();
    Task SaveChanges();
	ValueTask DisposeAsync();
}
