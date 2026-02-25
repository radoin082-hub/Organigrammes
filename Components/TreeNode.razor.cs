using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Entities;
using Compta.Ledger.Core.orgTestapp.Models;
using Compta.Ledger.Core.orgTestapp.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Globalization;
using System.Threading.Tasks;

namespace Compta.Ledger.Core.orgTestapp.Components;

public partial class TreeNode
{
    [Parameter] public Node Node { get; set; } = null!;
    [Parameter] public IOrgService Service { get; set; } = null!;
    [Parameter] public EventCallback OnNodeDeleted { get; set; }
    [Parameter] public EventCallback OnNodeMoved { get; set; }
    [Inject] public IJSRuntime JS { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource>? localizer { get; set; }
    private bool showButtons = false;
    private bool showCreateChildModal = false;
    private bool isEditingName = false;
    private bool isExpanded = true;
    private bool isDragOver = false;
    private bool isLTR = true;
    private string editingNameValue = string.Empty;
    private NodeModel nodeModel = new();
    private static Node? draggedNode = null;

    protected override Task OnInitializedAsync()
    {
        var cul = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        isLTR = cul == "ar" ? false : true;

        isExpanded = Node.Level <= 2;
        return base.OnInitializedAsync();
    }
    private void toggleExpanded() => isExpanded = !isExpanded;

    private void startEditing()
    {
        isEditingName = true;
        editingNameValue = Node.Name;
    }

    private async Task saveEdited()
    {
        if (!string.IsNullOrWhiteSpace(editingNameValue) && editingNameValue != Node.Name)
        {
            var result = await Service.RenameNode(Node, editingNameValue);
            if (result.IsSuccess)
            {

            }
            else
            {


            }
        }
        isEditingName = false;
        editingNameValue = string.Empty;
        showButtons = false;
    }

    private void cancelEditing()
    {
        isEditingName = false;
        editingNameValue = string.Empty;
    }

    private async Task keyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await saveEdited();
        else if (e.Key == "Escape") cancelEditing();
    }

    private void openChildModal()
    {
        nodeModel = new();
        showCreateChildModal = true;
    }

    private void closeChildModal()
    {
        showCreateChildModal = false;
        nodeModel = new();
    }

    private async Task keyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await createChild();
        else if (e.Key == "Escape") closeChildModal();
    }

    private async Task createChild()
    {

        var newChild = new Node { Name = nodeModel.Name };
        var result = await Service.createChildToNode(Node, newChild);
        if (result.IsSuccess)
        {

        }
        else
        {

        }
        nodeModel = new();
        showCreateChildModal = false;
        await OnNodeMoved.InvokeAsync();

    }

    private async Task deleteNode()
    {
        if (await JS.InvokeAsync<bool>("confirm", $"{localizer!["Delete"]} '{Node.Name}' {localizer!["Node.AndAllItsChildren"]}"))
        {
            var result = await Service.DeleteNode(Node);
            if (result.IsSuccess)
            {

            }
            else
            {

            }
            await OnNodeDeleted.InvokeAsync();
        }
    }

    private void dragStart(DragEventArgs e) { draggedNode = Node; }
    private void dragEnd(DragEventArgs e) { draggedNode = null; isDragOver = false; }
    private void dragOver(DragEventArgs e) { isDragOver = true; }
    private void dragLeave(DragEventArgs e) { isDragOver = false; }

    private async Task dropNode(DragEventArgs e)
    {
        isDragOver = false;
        if (draggedNode is null || draggedNode.Id == Node.Id) { draggedNode = null; return; }

        if (Node.Route.IsDescendantOf(draggedNode.Route)) { draggedNode = null; return; }

        var result = await Service.MoveNode(draggedNode, Node);
        if (result.IsSuccess)
        {

        }
        else
        {

        }
        await OnNodeMoved.InvokeAsync();
        draggedNode = null;
    }
}