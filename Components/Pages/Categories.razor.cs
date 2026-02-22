using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Entities;
using Microsoft.AspNetCore.Components;

namespace Compta.Ledger.Core.orgTestapp.Components.Pages
{
    public partial class Categories
    {
        [Inject] public IOrgService OrgService { get; set; } = null!;

        private int TotalNodes;
        private int RootNodesCount;
        private int MaxLevel;
        protected override async Task OnInitializedAsync()
        {
            var a = await OrgService.GetAllNodes();
            TotalNodes = a.Value.Count();
            RootNodesCount = a.Value.Count(n => n.Route.GetLevel() == 1);
            MaxLevel=a.Value.Any() ? a.Value.Max(n => n.Level) : 0;

        }
        private void LoadExampleData()
        {
            var products = new Node { Name = "Products" };
            OrgService.CreateOrg(products);

            var electronics = new Node { Name = "Electronics" };
            OrgService.createChildToNode(products, electronics);

            var phones = new Node { Name = "Phones" };
            OrgService.createChildToNode(electronics, phones);

            var smartphones = new Node { Name = "Smartphones" };
            OrgService.createChildToNode(phones, smartphones);

            var laptops = new Node { Name = "Laptops" };
            OrgService.createChildToNode(electronics, laptops);

            var furniture = new Node { Name = "Furniture" };
            OrgService.createChildToNode(products, furniture);

            var chairs = new Node { Name = "Chairs" };
            OrgService.createChildToNode(furniture, chairs);

            var tables = new Node { Name = "Tables" };
            OrgService.createChildToNode(furniture, tables);

            var clothing = new Node { Name = "Clothing" };
            OrgService.createChildToNode(products, clothing);

            StateHasChanged();
        }
    }
}