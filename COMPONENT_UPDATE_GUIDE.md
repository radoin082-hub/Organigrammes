# ?? COMPONENT UPDATE GUIDE - QUICK REFERENCE

**Goal**: Update Blazor components to work with Result Pattern service
**Time**: ~30-45 minutes
**Files to Update**: 3 files

---

## ?? PATTERN: How to Update Component Calls

### Pattern 1: Getting a Value

**OLD CODE:**
```csharp
var node = Service.GetNodeById(nodeId);
if (node == null)
{
    // Handle error
}
else
{
    // Use node
}
```

**NEW CODE:**
```csharp
var result = Service.GetNodeById(nodeId);
if (result.IsSuccess)
{
    var node = result.Value;  // ? Get the actual Node
    // Use node
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.Error.Description}");
}
```

---

## ?? PATTERN: Getting a List

**OLD CODE:**
```csharp
var nodes = Service.GetAllNodes();
var count = nodes.Count;
foreach (var node in nodes)
{
    // Use node
}
```

**NEW CODE:**
```csharp
var result = Service.GetAllNodes();
if (result.IsSuccess)
{
    var nodes = result.Value;  // ? Get the actual List<Node>
    var count = nodes.Count;
    foreach (var node in nodes)
    {
        // Use node
    }
}
else
{
    // Handle error
}
```

---

## ?? PATTERN: Void Operations (No Return Value)

**OLD CODE:**
```csharp
try
{
    Service.DeleteNode(nodeId);
    // Success
}
catch (Exception ex)
{
    // Handle error
}
```

**NEW CODE:**
```csharp
var result = Service.DeleteNode(nodeId);
if (result.IsSuccess)
{
    // Success
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.Error.Description}");
}
```

---

## ?? PATTERN: Error Type Handling

**CODE:**
```csharp
var result = Service.MoveNode(node, newParent);
if (result.IsSuccess)
{
    // Success
}
else
{
    // Handle specific error types
    switch (result.Error.Type)
    {
        case ErrorType.NotFound:
            Console.WriteLine("Node not found");
            break;
        case ErrorType.Conflict:
            Console.WriteLine("Circular reference detected");
            break;
        case ErrorType.Validation:
            Console.WriteLine($"Validation failed: {result.Error.Description}");
            break;
        default:
            Console.WriteLine($"Error: {result.Error.Description}");
            break;
    }
}
```

---

## ?? FILES TO UPDATE

### File 1: Categories.razor.cs

**Lines to change:**
- Line 14: Statistics calculation (GetAllNodes)
- Any GetAllNodes() calls

```csharp
// BEFORE
var allNodes = Service.GetAllNodes();
var stats = new
{
    Total = allNodes.Count,
    Roots = allNodes.Count(n => !n.ParentId.HasValue),
    MaxLevel = allNodes.Max(n => n.Level)
};

// AFTER
var result = Service.GetAllNodes();
if (result.IsSuccess)
{
    var allNodes = result.Value;
    var stats = new
    {
        Total = allNodes.Count,
        Roots = allNodes.Count(n => !n.ParentId.HasValue),
        MaxLevel = allNodes.Count > 0 ? allNodes.Max(n => n.Level) : 0
    };
}
```

---

### File 2: TreeNode.razor.cs

**Lines to change:**
- Line 136: ToggleMoveForm() - GetAllNodes() call
- Line 169: GetAllNodes with Where() filter

```csharp
// BEFORE (line 169)
AvailableParents = Service.GetAllNodes()
    .Where(n => n.Id != Node.Id && !IsDescendant(Node, n))
    .ToList();

// AFTER
var allNodesResult = Service.GetAllNodes();
if (allNodesResult.IsSuccess)
{
    AvailableParents = allNodesResult.Value
        .Where(n => n.Id != Node.Id && !IsDescendant(Node, n))
        .ToList();
}
else
{
    AvailableParents = new List<Node>();
}
```

---

### File 3: CategoryTree.razor.cs

**Lines to change:**
- Any GetAllNodes() calls
- Any GetNodeById() calls

---

## ? TESTING AFTER UPDATE

After updating, test these scenarios:

1. ? **GetAllNodes** - Statistics display correctly
2. ? **GetNodeById** - Node lookups work
3. ? **CreateOrg** - New org creation succeeds
4. ? **AddChild** - Child nodes can be added
5. ? **MoveNode** - Nodes can be moved
6. ? **DeleteNode** - Nodes can be deleted
7. ? **RenameNode** - Nodes can be renamed
8. ? **Error cases** - Errors are handled gracefully

---

## ?? QUICK CHECKLIST

- [ ] Update Categories.razor.cs
- [ ] Update TreeNode.razor.cs  
- [ ] Update CategoryTree.razor.cs
- [ ] Build succeeds (dotnet build)
- [ ] No compilation errors
- [ ] Test each feature works
- [ ] Commit to git

---

**Status**: Ready for component updates
**Effort**: ~30-45 minutes
**Complexity**: Low

?? **Service is done! Components are next!**

