using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Entities;
using Compta.Ledger.Core.orgTestapp.Extention;
using Compta.Ledger.Core.orgTestapp.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace Compta.Ledger.Core.orgTestapp.Components;

public partial class CategoryTree
{
    [Inject] public IOrgService OrgService { get; set; } = default!;

    private List<Node> rootNodes = new();
    private List<Node> filteredRoots = new();

    private bool showRootModal = false;
    private NodeModel nodeModel =new();
    private string searchText = string.Empty;
    private bool isLoading = false;
    protected bool isLTR => CultureInfo.CurrentCulture.TwoLetterISOLanguageName != "ar";
    protected override async Task OnInitializedAsync()
    {
        await LoadRootNodes();
    }

    private async Task OnSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await ApplySearch();
        }
    }



    private async Task ApplySearch()
    {
        isLoading = true;
        StateHasChanged();

        await Task.Delay(300);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            filteredRoots.Clear();
        }
        else
        {
            filteredRoots = rootNodes
                .Select(n => FilterNode(n, searchText))
                .Where(n => n != null)
                .ToList()!;
        }

        isLoading = false;
        StateHasChanged();
    }




    private bool NodeMatches(Node node, string term)
    {
        if (node.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            return true;

        return node.Children?.Any(child => NodeMatches(child, term)) == true;
    }
    private Node? FilterNode(Node node, string term)
    {
        bool isMatch = node.Name.Contains(term, StringComparison.OrdinalIgnoreCase);

        var filteredChildren = node.Children?
            .Select(child => FilterNode(child, term))
            .Where(child => child != null)
            .ToList();

        if (isMatch)
        {
            return new Node
            {
                Id = node.Id,
                Name = node.Name,
                Children = node.Children ?? []
            };
        }

        if (filteredChildren != null && filteredChildren.Any())
        {
            return new Node
            {
                Id = node.Id,
                Name = node.Name,
                Route = node.Route,
                Children = filteredChildren!
            };
        }

        return null;
    }


    private void ClearSearch()
    {
        searchText = string.Empty;
        filteredRoots.Clear();
    }

    private async Task LoadRootNodes()
    {
        isLoading = true;
        StateHasChanged();

        var result = await OrgService.GetAllNodes();

        if (result.IsSuccess)
        {
            rootNodes = result.Value
                .Where(n => n.Level == 1)
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
                await ApplySearch();
        }

        await Task.Delay(500);

        isLoading = false;
    }

    private void OpenModal()
    {
        nodeModel = new();
        showRootModal = true;
    }

    private void CloseModal()
    {
        showRootModal = false;
        nodeModel = new();
    }

    private async Task KeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await AddRootCategory();
        else if (e.Key == "Escape")
            CloseModal();
    }

    private async Task AddRootCategory()
    {



        var result = await OrgService.CreateOrg(new Node { Name = nodeModel.Name });

        if (result.IsSuccess)
        {
            CloseModal();
            await LoadRootNodes();
        }
    }
    private async Task NodeDeleted() => await LoadRootNodes();
    private async Task NodeMoved() => await LoadRootNodes();
}
