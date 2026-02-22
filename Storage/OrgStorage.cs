using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Compta.Ledger.Core.orgTestapp.Storage;

public class OrgStorage : IOrgStorage, IAsyncDisposable
{
    private readonly IDbContextFactory<OrgDbContext> contextFactory;
    private IDbContextTransaction? transaction;

    public OrgStorage(IDbContextFactory<OrgDbContext> _contextFactory)
    {
        contextFactory = _contextFactory;
    }

    public async Task<Guid> CreateNode(Node node)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        context.Nodes.Add(node);
        await context.SaveChangesAsync();
        return node.Id;
    }

    public async Task<Node?> GetNodeById(Guid nodeId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return await context.Nodes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == nodeId);
    }


    public async Task<List<Node>> GetAllNodes()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return await context.Nodes
            .AsNoTracking()
            .OrderBy(n => n.Route)
            .ToListAsync();
    }

    public async Task<List<Node>> GetNodesChildren(HierarchyId parentRoute)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var parentLevel = parentRoute.GetLevel();
        return await context.Nodes
            .Where(n => n.Route.IsDescendantOf(parentRoute)
                     && n.Route != parentRoute
                     && n.Route.GetLevel() == parentLevel + 1)
            .OrderBy(n => n.Route)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> UpdateNode(Node node)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var existing = await context.Nodes.FindAsync(node.Id);
        if (existing is null)
            return false;

        context.Entry(existing).CurrentValues.SetValues(node);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteNode(int id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var node = await context.Nodes.FindAsync(id);
        if (node is null)
            return false;

        context.Nodes.Remove(node);
        return await context.SaveChangesAsync() > 0;
    }


    public async Task DeleteNodesByRoute(HierarchyId route)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var descendants = await context.Nodes
            .Where(n => n.Route.IsDescendantOf(route))
            .OrderByDescending(n => n.Route.GetLevel())
            .ToListAsync();

        if (descendants.Count == 0)
            return;

        context.Nodes.RemoveRange(descendants);
        await context.SaveChangesAsync();
    }

    public async Task<List<Node>> GetDescendants(HierarchyId route)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return await context.Nodes
            .Where(n => n.Route.IsDescendantOf(route) && n.Route != route)
            .OrderBy(n => n.Route)
            .ToListAsync();
    }

    public async Task<int> GetRootNodesCount()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return await context.Nodes
            .Where(n => n.Route.GetLevel() == 1)
            .CountAsync();
    }

    public async Task<Node?> GetLastRootNode()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return await context.Nodes
            .AsNoTracking()
            .Where(n => n.Route.GetLevel() == 1)
            .OrderBy(n => n.Route)
            .LastOrDefaultAsync();
    }

    public async Task<Node?> GetLastChild(HierarchyId parentRoute)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var parentLevel = parentRoute.GetLevel();
        return await context.Nodes
            .Where(n => n.Route.IsDescendantOf(parentRoute)
                     && n.Route != parentRoute
                     && n.Route.GetLevel() == parentLevel + 1)
            .OrderBy(n => n.Route)
            .LastOrDefaultAsync();
    }

    public async Task<Node?> GetParent(HierarchyId route)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        if (route.GetLevel() <= 1)
            return null;

        var parentRoute = route.GetAncestor(1);

        return await context.Nodes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Route == parentRoute);
    }
    public async Task<List<Node>> GetAncestors(HierarchyId route, int level)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var ancestors = new List<Node>();

        for (int i = 1; i <= level; i++)
        {
            var ancestorRoute = route.GetAncestor(i);
            var ancestor = await context.Nodes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Route == ancestorRoute);

            if (ancestor is not null)
                ancestors.Add(ancestor);
        }
        return ancestors;
    }
    public async Task<IDbContextTransaction> BeginTransaction()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        transaction = await context.Database.BeginTransactionAsync();
        return transaction;
    }

    public async Task CommitTransaction()
    {
        if (transaction is not null)
        {
            await transaction.CommitAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }
    public async Task RollbackTransaction()
    {
        if (transaction is not null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task SaveChanges()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        await context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        if (transaction is not null)
        {
            await transaction.DisposeAsync();
            transaction = null;
        }

        if (context is not null)
        {
            await context.DisposeAsync();
        }
    }

}