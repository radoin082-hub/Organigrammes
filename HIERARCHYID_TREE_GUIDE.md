# ?? HIERARCHYID TREE SYSTEM - COMPLETE GUIDE

**Status**: ? **READY TO IMPLEMENT**

---

## ?? WHAT IS HIERARCHYID?

**HierarchyId** is SQL Server's built-in data type for representing hierarchical data:
- **Efficient** - Compact representation of tree structures
- **Fast** - Optimized for hierarchy queries
- **Path-based** - Uses materialized path pattern
- **Built-in methods** - GetAncestor(), GetDescendant(), IsDescendantOf()

---

## ?? HIERARCHYID STRUCTURE

### **Visual Representation**

```
Root                    /
?? Node 1              /1/
?  ?? Node 1.1         /1/1/
?  ?? Node 1.2         /1/2/
?? Node 2              /2/
   ?? Node 2.1         /2/1/
   ?? Node 2.2         /2/2/
      ?? Node 2.2.1    /2/2/1/
```

### **HierarchyId Values**

```sql
Root:       0x         (HierarchyId.GetRoot())
Node 1:     0x58       (Level 1, First child)
Node 1.1:   0x5AC0     (Level 2, First child of /1/)
Node 1.2:   0x5B40     (Level 2, Second child of /1/)
Node 2:     0x68       (Level 1, Second child)
Node 2.1:   0x6AC0     (Level 2, First child of /2/)
Node 2.2:   0x6B40     (Level 2, Second child of /2/)
Node 2.2.1: 0x6B5AC0   (Level 3, First child of /2/2/)
```

---

## ?? IMPLEMENTATION COMPONENTS

### **1. Entity with HierarchyId**

```csharp
using Microsoft.EntityFrameworkCore;

public class Node
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    
    // ? HierarchyId for efficient tree queries
    public HierarchyId Route { get; set; }
    
    public string Name { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Node? Parent { get; set; }
    public List<Node> Children { get; set; } = new();
}
```

### **2. DbContext Configuration**

```csharp
public class OrgDbContext : DbContext
{
    public DbSet<Node> Nodes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // ? HierarchyId column
            entity.Property(e => e.Route)
                .HasColumnType("hierarchyid")
                .IsRequired();
            
            // ? Index for fast queries
            entity.HasIndex(e => e.Route)
                .IsUnique();
            
            // Parent-child relationship
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

### **3. Service Methods**

```csharp
public class OrgService : IOrgService
{
    private readonly OrgDbContext _context;
    
    /// <summary>
    /// Create root node
    /// </summary>
    public async Task<Result<Guid>> CreateRoot(string name)
    {
        var node = new Node
        {
            Id = Guid.NewGuid(),
            Name = name,
            ParentId = null,
            Route = HierarchyId.GetRoot(),  // ? Root = /
            Level = 0,
            SortOrder = await GetNextSortOrder(null)
        };
        
        _context.Nodes.Add(node);
        await _context.SaveChangesAsync();
        
        return Result.Success(node.Id);
    }
    
    /// <summary>
    /// Add child node
    /// </summary>
    public async Task<Result<Guid>> AddChild(Guid parentId, string name)
    {
        var parent = await _context.Nodes.FindAsync(parentId);
        if (parent == null)
            return Result.Failure<Guid>("Parent not found");
        
        // ? Get last sibling for proper positioning
        var lastSibling = await _context.Nodes
            .Where(n => n.ParentId == parentId)
            .OrderBy(n => n.Route)
            .LastOrDefaultAsync();
        
        var child = new Node
        {
            Id = Guid.NewGuid(),
            Name = name,
            ParentId = parentId,
            // ? Calculate HierarchyId using GetDescendant()
            Route = parent.Route.GetDescendant(
                lastSibling?.Route,  // Last sibling
                null                 // No next sibling (append)
            ),
            Level = parent.Level + 1,
            SortOrder = await GetNextSortOrder(parentId)
        };
        
        _context.Nodes.Add(child);
        await _context.SaveChangesAsync();
        
        return Result.Success(child.Id);
    }
    
    /// <summary>
    /// Get all descendants (children, grandchildren, etc.)
    /// </summary>
    public async Task<List<Node>> GetDescendants(Guid nodeId)
    {
        var node = await _context.Nodes.FindAsync(nodeId);
        if (node == null)
            return new List<Node>();
        
        // ? Fast query using IsDescendantOf()
        return await _context.Nodes
            .Where(n => n.Route.IsDescendantOf(node.Route) 
                     && n.Id != nodeId)
            .OrderBy(n => n.Route)
            .ToListAsync();
    }
    
    /// <summary>
    /// Get ancestors (parent, grandparent, etc.)
    /// </summary>
    public async Task<List<Node>> GetAncestors(Guid nodeId)
    {
        var node = await _context.Nodes.FindAsync(nodeId);
        if (node == null)
            return new List<Node>();
        
        // ? Fast query using GetAncestor()
        var ancestors = new List<Node>();
        var currentLevel = node.Level - 1;
        
        while (currentLevel >= 0)
        {
            var ancestorRoute = node.Route.GetAncestor(node.Level - currentLevel);
            var ancestor = await _context.Nodes
                .FirstOrDefaultAsync(n => n.Route == ancestorRoute);
            
            if (ancestor != null)
                ancestors.Add(ancestor);
            
            currentLevel--;
        }
        
        return ancestors;
    }
    
    /// <summary>
    /// Move node to new parent
    /// </summary>
    public async Task<Result> MoveNode(Guid nodeId, Guid newParentId)
    {
        var node = await _context.Nodes.FindAsync(nodeId);
        var newParent = await _context.Nodes.FindAsync(newParentId);
        
        if (node == null || newParent == null)
            return Result.Failure("Node or parent not found");
        
        // ? Prevent circular reference
        if (newParent.Route.IsDescendantOf(node.Route))
            return Result.Failure("Cannot move to descendant");
        
        // ? Calculate new route
        var lastSibling = await _context.Nodes
            .Where(n => n.ParentId == newParentId)
            .OrderBy(n => n.Route)
            .LastOrDefaultAsync();
        
        var oldRoute = node.Route;
        var newRoute = newParent.Route.GetDescendant(
            lastSibling?.Route, 
            null
        );
        
        // ? Update node and all descendants
        var descendants = await GetDescendants(nodeId);
        
        node.ParentId = newParentId;
        node.Route = newRoute;
        node.Level = newParent.Level + 1;
        
        // ? Update all descendant routes
        foreach (var descendant in descendants)
        {
            var relativeRoute = oldRoute.GetDescendant(descendant.Route, null);
            descendant.Route = newRoute.GetDescendant(relativeRoute, null);
            descendant.Level = newRoute.GetLevel() + 
                              (descendant.Route.GetLevel() - newRoute.GetLevel());
        }
        
        await _context.SaveChangesAsync();
        return Result.Success();
    }
    
    /// <summary>
    /// Delete node and descendants
    /// </summary>
    public async Task<Result> DeleteNode(Guid nodeId)
    {
        var node = await _context.Nodes.FindAsync(nodeId);
        if (node == null)
            return Result.Failure("Node not found");
        
        // ? Get all descendants to delete
        var descendants = await _context.Nodes
            .Where(n => n.Route.IsDescendantOf(node.Route))
            .ToListAsync();
        
        _context.Nodes.RemoveRange(descendants);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
    
    private async Task<int> GetNextSortOrder(Guid? parentId)
    {
        var count = await _context.Nodes
            .CountAsync(n => n.ParentId == parentId);
        return count;
    }
}
```

---

## ?? COMMON QUERIES

### **1. Get Tree Structure**

```csharp
// Get entire tree ordered hierarchically
var tree = await _context.Nodes
    .OrderBy(n => n.Route)
    .ToListAsync();
```

### **2. Get Direct Children**

```csharp
// Get immediate children only
var children = await _context.Nodes
    .Where(n => n.ParentId == parentId)
    .OrderBy(n => n.Route)
    .ToListAsync();
```

### **3. Get Subtree**

```csharp
// Get node and all descendants
var parent = await _context.Nodes.FindAsync(nodeId);
var subtree = await _context.Nodes
    .Where(n => n.Route.IsDescendantOf(parent.Route))
    .OrderBy(n => n.Route)
    .ToListAsync();
```

### **4. Get Path to Root**

```csharp
// Get all ancestors
var node = await _context.Nodes.FindAsync(nodeId);
var path = new List<Node>();
var currentLevel = node.Level;

while (currentLevel > 0)
{
    var ancestorRoute = node.Route.GetAncestor(currentLevel);
    var ancestor = await _context.Nodes
        .FirstOrDefaultAsync(n => n.Route == ancestorRoute);
    if (ancestor != null)
        path.Insert(0, ancestor);
    currentLevel--;
}
```

### **5. Get Siblings**

```csharp
// Get nodes at same level with same parent
var siblings = await _context.Nodes
    .Where(n => n.ParentId == node.ParentId && n.Id != node.Id)
    .OrderBy(n => n.Route)
    .ToListAsync();
```

---

## ?? KEY HIERARCHYID METHODS

### **GetRoot()**
```csharp
var root = HierarchyId.GetRoot();  // Returns: /
```

### **GetDescendant(child1, child2)**
```csharp
// Insert between existing children
var newRoute = parent.Route.GetDescendant(
    lastChild?.Route,    // Before this
    null                 // After this (append)
);
```

### **IsDescendantOf(parent)**
```csharp
// Check if node is under parent
if (node.Route.IsDescendantOf(parent.Route))
{
    // Node is a descendant
}
```

### **GetAncestor(levels)**
```csharp
// Get ancestor N levels up
var grandparent = node.Route.GetAncestor(2);
```

### **GetLevel()**
```csharp
// Get depth in tree
var level = node.Route.GetLevel();  // 0 = root, 1 = first level, etc.
```

---

## ? ADVANTAGES OF HIERARCHYID

### **1. Performance**
- ? Single query for subtrees
- ? Indexed lookups
- ? Fast ancestor/descendant checks

### **2. Simplicity**
- ? Built-in methods
- ? SQL Server optimized
- ? No recursive queries needed

### **3. Flexibility**
- ? Easy reordering
- ? Move subtrees
- ? Insert between siblings

---

## ?? MIGRATION FROM PARENTID

If you want to convert from ParentId to HierarchyId:

```sql
-- Add HierarchyId column
ALTER TABLE Nodes 
ADD Route hierarchyid;

-- Create recursive CTE to calculate routes
WITH NodeHierarchy AS (
    -- Root nodes
    SELECT 
        Id,
        ParentId,
        CAST('/' AS varchar(max)) AS Path,
        CAST(hierarchyid::GetRoot() AS hierarchyid) AS Route,
        0 AS Level
    FROM Nodes
    WHERE ParentId IS NULL
    
    UNION ALL
    
    -- Child nodes
    SELECT 
        n.Id,
        n.ParentId,
        CAST(nh.Path + CAST(n.Id AS varchar(36)) + '/' AS varchar(max)),
        CAST(nh.Route.GetDescendant(NULL, NULL) AS hierarchyid),
        nh.Level + 1
    FROM Nodes n
    INNER JOIN NodeHierarchy nh ON n.ParentId = nh.Id
)
UPDATE n
SET 
    n.Route = nh.Route,
    n.Level = nh.Level
FROM Nodes n
INNER JOIN NodeHierarchy nh ON n.Id = nh.Id;

-- Create index
CREATE UNIQUE INDEX IX_Nodes_Route ON Nodes(Route);
```

---

## ?? RESULT

With HierarchyId, you get:
- ? **Efficient tree queries**
- ? **Fast ancestor/descendant operations**
- ? **Easy subtree moves**
- ? **SQL Server optimized**
- ? **Built-in tree methods**

---

**Status**: ? **READY TO IMPLEMENT**
**Performance**: ?? **EXCELLENT**
**Complexity**: ?? **MODERATE**
