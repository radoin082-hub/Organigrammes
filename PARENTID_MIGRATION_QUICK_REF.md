# ? PARENTID MIGRATION - QUICK REFERENCE

**Goal**: Replace Route (HierarchyId) with ParentId-based hierarchy

---

## ?? FILES TO CHANGE

### 1. **Node.cs** - Make Route nullable
```csharp
public HierarchyId? Route { get; set; }  // Changed from HierarchyId to HierarchyId?
```

### 2. **OrgService.cs** - Remove Route calculations

**In CreateOrg():**
```csharp
// REMOVE:
node.Route = HierarchyId.GetRoot();

// KEEP:
node.ParentId = null;
node.Level = 0;
node.SortOrder = _nodes.Count(n => n.ParentId == null);
```

**In AddChildToNode():**
```csharp
// REMOVE:
childNode.Route = parentNode.Route.GetDescendant(...);

// KEEP:
childNode.ParentId = parentNode.Id;
childNode.Level = parentNode.Level + 1;
childNode.SortOrder = parentNode.Children.Count;
```

**In MoveNode():**
```csharp
// REMOVE:
node.Route = newParentNode.Route.GetDescendant(...);

// KEEP:
node.ParentId = newParentNode.Id;
node.Level = newParentNode.Level + 1;
node.SortOrder = newParentNode.Children.Count;
```

### 3. **OrgStorage.cs** - Replace Route queries

**In GetAllNodes():**
```csharp
// REPLACE:
.OrderBy(n => n.Route)

// WITH:
.OrderBy(n => n.Level)
.ThenBy(n => n.SortOrder)
.ThenBy(n => n.Name)
```

**In GetNodesChildren():**
```csharp
// REPLACE:
.OrderBy(n => n.Route)

// WITH:
.OrderBy(n => n.SortOrder)
.ThenBy(n => n.Name)
```

**In DeleteNodesByNode():**
```csharp
// REPLACE:
var descendants = _context.Nodes
    .Where(n => n.Route.IsDescendantOf(node.Route))
    .OrderByDescending(n => n.Route.GetLevel())
    .ToList();

// WITH:
var descendants = GetAllDescendantsRecursive(node.Id);
var orderedDescendants = descendants
    .OrderByDescending(n => n.Level)
    .ToList();
```

**In GetDescendants():**
```csharp
// REPLACE:
var descendants = _context.Nodes
    .Where(n => n.Route.IsDescendantOf(node.Route) && n.Route != node.Route)
    .ToList();

// WITH:
return GetAllDescendantsRecursive(node.Id);
```

**ADD new helper method:**
```csharp
private List<Node> GetAllDescendantsRecursive(Guid parentId)
{
    var descendants = new List<Node>();
    var directChildren = _context.Nodes
        .Where(n => n.ParentId == parentId)
        .ToList();
    
    foreach (var child in directChildren)
    {
        descendants.Add(child);
        descendants.AddRange(GetAllDescendantsRecursive(child.Id));
    }
    
    return descendants;
}
```

**In GetLastRootNode():**
```csharp
// REPLACE:
.OrderBy(n => n.Route)

// WITH:
.OrderBy(n => n.SortOrder)
.ThenBy(n => n.CreatedAt)
```

**In GetLastChild() and GetLastSibling():**
```csharp
// REPLACE:
.OrderBy(n => n.Route)

// WITH:
.OrderBy(n => n.SortOrder)
.ThenBy(n => n.CreatedAt)
```

### 4. **IOrgStorage.cs** - ADD new methods

```csharp
// ADD these NEW method signatures:

bool IsAncestor(Guid potentialAncestorId, Guid nodeId);
List<Node> GetAncestors(Guid nodeId);
```

**And IMPLEMENT in OrgStorage.cs:**
```csharp
public bool IsAncestor(Guid potentialAncestorId, Guid nodeId)
{
    var currentNode = GetNodeById(nodeId);
    
    while (currentNode?.ParentId != null)
    {
        if (currentNode.ParentId == potentialAncestorId)
            return true;
        
        currentNode = GetNodeById(currentNode.ParentId.Value);
    }
    
    return false;
}

public List<Node> GetAncestors(Guid nodeId)
{
    var ancestors = new List<Node>();
    var currentNode = GetNodeById(nodeId);
    
    while (currentNode?.ParentId != null)
    {
        var parent = GetNodeById(currentNode.ParentId.Value);
        if (parent != null)
        {
            ancestors.Add(parent);
            currentNode = parent;
        }
        else
        {
            break;
        }
    }
    
    return ancestors;
}
```

---

## ? VERIFICATION

After changes, verify:
1. Build successful
2. Can create root categories
3. Can add children
4. Can move nodes
5. Can delete nodes (with cascade)
6. Drag and drop works

---

## ?? KEY POINTS

**What we're doing:**
- ? Keeping ParentId as hierarchy field
- ? Using Level and SortOrder for ordering
- ? Using recursion for descendants
- ? Using ParentId chain for ancestors
- ? Removing Route (HierarchyId) calculations

**Benefits:**
- ? Works with any database
- ? Simpler code
- ? Easier to understand
- ? No SQL Server dependency

---

**Status**: ?? READY TO APPLY
