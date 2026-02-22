using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Entities;
using Compta.Ledger.Core.orgTestapp.Helpers;
using Compta.Ledger.Core.orgTestapp.Resources;
using Compta.Ledger.Core.orgTestapp.Storage;
using Ids.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Compta.Ledger.Core.orgTestapp.Services;

public class OrgService
    (IOrgStorage orgStorage,
     ILogger<OrgService> logger,
     IStringLocalizer<Resource> localizer,
     IGuidProvider guid,
     IDateProvider date) : IOrgService
{
    public async ValueTask<Result<Guid>> CreateOrg(Node node)
    {
        try
        {
            if (node is null)
                return ErrorCode.NullValue;

            node.Id = guid.NewV7();
            var root = HierarchyId.GetRoot();
            var lastRoot = await orgStorage.GetLastRootNode();

            if (lastRoot is null)
            {
                node.Route = root.GetDescendant(null, null);
            }
            else
            {
                node.Route = root.GetDescendant(lastRoot.Route, null);
            }
            node.SortOrder = await orgStorage.GetRootNodesCount();
            node.CreatedAt = date.UtcNow;

            await orgStorage.CreateNode(node);
            await orgStorage.SaveChanges();

            return node.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating organization node with name {NodeName}", node?.Name);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result<Guid>> createChildToNode(Guid parentId, Node childNode)
    {
        try
        {
            var parentNode = await orgStorage.GetNodeById(parentId);
            if (parentNode is null)
                return ErrorCode.NullValue;

            return await createChildToNode(parentNode, childNode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding child node to parent with ID {ParentId}", parentId);
            return ErrorCode.Exception(ex);

        }

    }

    public async ValueTask<Result<Guid>> createChildToNode(Node parentNode, Node childNode)
    {
        try
        {
            if (parentNode is null || childNode is null)
                return ErrorCode.NullValue;

            childNode.Id = guid.NewV7();
            childNode.CreatedAt = date.UtcNow;

            var lastChild = await orgStorage.GetLastChild(parentNode.Route);

            childNode.Route = parentNode.Route.GetDescendant(lastChild?.Route);

            var nodeChildren = await orgStorage.GetNodesChildren(parentNode.Route);
            childNode.SortOrder = nodeChildren.Count();

            await orgStorage.CreateNode(childNode);
            await orgStorage.SaveChanges();

            return childNode.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding child node with name {ChildNodeName} to parent node with ID {ParentId}", childNode?.Name, parentNode.Id);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result<Node>> GetNodeById(Guid nodeId)
    {
        try
        {
            var node = await orgStorage.GetNodeById(nodeId);
            if (node is null)
                return ErrorCode.NotFound(localizer["Node.NotFound.Title"], localizer["Node.NotFound.Description"]);

            return node;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.GetByIdException"], nodeId);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result<List<Node>>> GetAllNodes()
    {
        try
        {
            var nodes = await orgStorage.GetAllNodes();
            await buildHierarchy(nodes);
            return nodes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.GetAllException"]);
            return ErrorCode.Exception(ex);

        }

    }

    public async ValueTask<Result<List<Node>>> GetNodesChildren(Guid nodeId)
    {
        try
        {
            var node = await orgStorage.GetNodeById(nodeId);
            if (node is null)
                return ErrorCode.NotFound(localizer["Node.NotFound.Title"], localizer["Node.NotFound.Description"]);

            return await GetNodesChildren(node!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.GetChildrenException"], nodeId);
            return ErrorCode.Exception(ex);

        }

    }
    public async ValueTask<Result<List<Node>>> GetNodesChildren(Node node)
    {
        try
        {
            if (node is null)
                return ErrorCode.NullValue;

            var result = await orgStorage.GetNodesChildren(node.Route);
            return result is not null
                 ? result
                 : ErrorCode.NotFound(localizer["Node.NotFound.Title"], localizer["Node.NotFound.Description"]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.GetChildrenException"], node.Id);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result> DeleteNode(Guid nodeId)
    {
        try
        {
            var node = await orgStorage.GetNodeById(nodeId);
            if (node is null)
                return ErrorCode.NotFound(localizer["Node.NotFound.Title"], localizer["Node.NotFound.Description"]);

            await DeleteNode(node);
            return Result.Success();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.DeleteException"], nodeId);
            return ErrorCode.Exception(ex);
        }

    }
    public async ValueTask<Result> DeleteNode(Node node)
    {
        try
        {
            if (node is null)
                return ErrorCode.NullValue;

            await orgStorage.DeleteNodesByRoute(node.Route);
            await orgStorage.SaveChanges();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.DeleteException"], node.Id);
            return ErrorCode.Exception(ex);

        }

    }

    public async ValueTask<Result> MoveNode(Guid nodeId, Guid newParentId)
    {
        try
        {
            var node = await orgStorage.GetNodeById(nodeId);
            if (node is null)
                return ErrorCode.NotFound(localizer["Node.NotFound.Title"], localizer["Node.NotFound.Description"]);

            var newParentNode = await orgStorage.GetNodeById(newParentId);
            if (newParentNode is null)
                return ErrorCode.NotFound(localizer["Node.NotFound.Title"], localizer["Node.NotFound.Description"]);

            await MoveNode(node, newParentNode!);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.MoveException"], nodeId, newParentId);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result> MoveNode(Node node, Node newParentNode)
    {
        try
        {
            if (node is null || newParentNode is null)
                return ErrorCode.NullValue;

            if (newParentNode.Route.IsDescendantOf(node.Route))
                return ErrorCode.Failure(localizer["Node.MoveToDescendant.Title"], localizer["Node.MoveToDescendant.Description"]);

            var lastChild = await orgStorage.GetLastChild(newParentNode.Route);

            var oldRoute = node.Route;
            var newRoute = newParentNode.Route.GetDescendant(lastChild?.Route, null);
            var descendants = await orgStorage.GetDescendants(oldRoute);

            node.Route = newRoute;
            node.UpdatedAt = date.UtcNow;

            if (!(await orgStorage.UpdateNode(node)))
                return ErrorCode.Failure(localizer["Node.Update.Title"], localizer["Node.Update.Description"]);

            foreach (var descendant in descendants)
            {
                descendant.Route = descendant.Route.GetReparentedValue(oldRoute, newRoute)!;
                descendant.UpdatedAt = date.UtcNow;

                if (!(await orgStorage.UpdateNode(descendant)))
                    return ErrorCode.Failure(localizer["Node.Update.Title"], localizer["Node.Update.Description"]);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.MoveException"], node.Id, newParentNode.Id);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result> RenameNode(Guid nodeId, string newName)
    {
        try
        {
            var node = await orgStorage.GetNodeById(nodeId);
            if (node is null)
                return ErrorCode.NotFound("Node.NotFound.Title", localizer["Node.NotFound.Description"]);

            var result = await RenameNode(node, newName);

            return result.IsSuccess
                ? Result.Success()
                : ErrorCode.Failure(localizer["Node.Rename.Title"], localizer["Node.Rename.Description"]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.RenameException"], nodeId);
            return ErrorCode.Exception(ex);
        }

    }

    public async ValueTask<Result> RenameNode(Node node, string newName)
    {
        try
        {
            if (node is null || string.IsNullOrWhiteSpace(newName))
                return ErrorCode.NullValue;

            node.Name = newName.Trim();
            node.UpdatedAt = date.UtcNow;
            await orgStorage.UpdateNode(node);
            await orgStorage.SaveChanges();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, localizer["Node.RenameException"], node.Id);
            return ErrorCode.Exception(ex);
        }

    }

    private async Task buildHierarchy(List<Node> nodes)
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

                if (parent is not null)
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
        await Task.CompletedTask;
    }
}