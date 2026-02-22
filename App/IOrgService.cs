using Compta.Ledger.Core.orgTestapp.Entities;
using Ids.ResultPattern;

namespace Compta.Ledger.Core.orgTestapp.App;

public interface IOrgService
{
    ValueTask<Result<Guid>> CreateOrg(Node node);

    ValueTask<Result<Guid>> createChildToNode(Guid parentId, Node childNode);

    ValueTask<Result<Guid>> createChildToNode(Node parentNode, Node childNode);

    ValueTask<Result<Node>> GetNodeById(Guid nodeId);

    ValueTask<Result<List<Node>>> GetAllNodes();

    ValueTask<Result<List<Node>>> GetNodesChildren(Guid nodeId);

    ValueTask<Result<List<Node>>> GetNodesChildren(Node node);

	ValueTask<Result> DeleteNode(Guid nodeId);

    ValueTask<Result> DeleteNode(Node node);

    ValueTask<Result> MoveNode(Guid nodeId, Guid newParentId);

    ValueTask<Result> MoveNode(Node node, Node newParentNode);

    ValueTask<Result> RenameNode(Guid nodeId, string newName);

    ValueTask<Result> RenameNode(Node node, string newName);
}
