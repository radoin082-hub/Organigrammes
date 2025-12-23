using Compta.Ledger.Core.orgTestapp.Entities;

namespace Compta.Ledger.Core.orgTestapp.App;

public interface IOrgService
{
	Guid CreateOrg(Node node);

	Guid AddChildToNode(Guid parentId, Node childNode);

	Guid AddChildToNode(Node parentNode, Node childNode);

	Node GetNodeById(Guid nodeId);

	List<Node> GetAllNodes();

	List<Node> GetNodesChildren(Guid nodeId);

	List<Node> GetNodesChildren(Node node);

	void DeleteNode(Guid nodeId);

	void DeleteNode(Node node);

	void MoveNode(Guid nodeId, Guid newParentId);

	void MoveNode(Node node, Node newParentNode);

	void RenameNode(Guid nodeId, string newName);

	void RenameNode(Node node, string newName);
}
