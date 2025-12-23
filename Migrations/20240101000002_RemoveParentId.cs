using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compta.Ledger.Core.orgTestapp.Migrations;

/// <summary>
/// Migration to remove ParentId column from Nodes table
/// Parent relationships are now derived from HierarchyId Route using GetAncestor(1)
/// </summary>
public partial class RemoveParentId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Remove ParentId column - hierarchy is now managed entirely by HierarchyId Route
        migrationBuilder.DropColumn(
            name: "ParentId",
            table: "Nodes");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Re-add ParentId column if rollback is needed
        migrationBuilder.AddColumn<Guid>(
            name: "ParentId",
            table: "Nodes",
            type: "uniqueidentifier",
            nullable: true);
    }
}
