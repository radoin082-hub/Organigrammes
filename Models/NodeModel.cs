using Compta.Ledger.Core.orgTestapp.Resources;
using System.ComponentModel.DataAnnotations;
namespace Compta.Ledger.Core.orgTestapp.Models;

public class NodeModel
{
    [Required(
        ErrorMessageResourceType = typeof(Resource),
        ErrorMessageResourceName = "Node_Required"
    )]
    public string Name { get; set; } = string.Empty;
}
