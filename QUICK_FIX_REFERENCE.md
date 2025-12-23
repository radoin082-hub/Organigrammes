# ? QUICK FIX REFERENCE

## The Problem
```
Error: An error occurred while saving the entity changes
```

## The Root Cause
```csharp
// ? WRONG - Uses Parse() which fails with EF Core
node.Route = HierarchyId.Parse($"/{nextRootNumber}/");
```

## The Solution (3 Changes)

### 1?? IOrgStorage.cs - Add Interface Method
```csharp
Node? GetLastRootNode();
```

### 2?? OrgStorage.cs - Implement Method
```csharp
public Node? GetLastRootNode()
{
    return _context.Nodes
        .Where(n => n.ParentId == null)
        .OrderBy(n => n.Route)
        .LastOrDefault();
}
```

### 3?? OrgService.cs - Fix CreateOrg
```csharp
// ? CORRECT - Uses HierarchyId API properly
var rootCount = _storage.GetRootNodesCount();

if (rootCount == 0)
    node.Route = HierarchyId.GetRoot();
else
{
    var lastRoot = _storage.GetLastRootNode();
    node.Route = HierarchyId.GetRoot()
        .GetDescendant(lastRoot?.Route, null);
}
```

## Before vs After

| Before | After |
|--------|-------|
| ? Parse fails | ? API works |
| ? Save error | ? Saves cleanly |
| ? No categories | ? Categories added |

## Test It
1. Add category "Products"
2. ? Should appear!

---

**Status**: Ready to implement!
