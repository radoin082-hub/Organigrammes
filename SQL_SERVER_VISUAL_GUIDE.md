# ?? SQL SERVER STYLE - VISUAL GUIDE

---

## ? BEFORE vs AFTER

### **BEFORE (Generic Tree)**
```
? Category 1
? Category 2
  ? Child 1
  ? Child 2
```

### **AFTER (SQL Server Style)**
```
[+] ?? Category 1
[?] ?? Category 2
  |
  ?? [+] ?? Child 1
  ?? [+] ?? Child 2
```

---

## ?? KEY FEATURES

### **1. Expand/Collapse Box**

**Collapsed:**
```
???
?+?  ? Square box with plus
???
```

**Expanded:**
```
???
???  ? Square box with minus
???
```

### **2. Folder Icons**

**Collapsed:**
```
[+] ?? Folder Name  ? Closed folder (yellow)
```

**Expanded:**
```
[?] ?? Folder Name  ? Open folder (yellow)
```

### **3. Tree Lines**

```
[?] ?? Root
  :
  ?? [+] ?? Child 1
  ?? [?] ?? Child 2
  :    :
  :    ?? [+] ?? Grandchild
  :    ??     Item
  ?? [+] ?? Child 3

(: = dotted line)
```

---

## ?? COLORS

| Element | Color | Hex |
|---------|-------|-----|
| Box border | Gray | #999999 |
| Box hover | Blue | #0078d4 |
| Folder icon | Gold | #ffca28 |
| Folder hover | Orange | #ffa000 |
| Background hover | Light blue | #f0f6ff |
| Tree lines | Light gray | #d0d0d0 |

---

## ?? SIZES

| Element | Size |
|---------|------|
| Expand box | 18x18 px |
| + / ? symbol | 14px bold |
| Folder icon | 1rem |
| Node text | 0.9rem |
| Tree indent | 20px per level |

---

## ??? INTERACTIONS

### **Hover on Expand Box:**
```
[+] ? Changes to blue border
     Blue background
```

### **Hover on Node:**
```
Background: White ? Light blue
Folder: Yellow ? Orange
```

### **Click Expand:**
```
[+] ?? Name  ? Expands
[?] ?? Name  ? Shows children
```

---

## ?? COMPLETE EXAMPLE

```
[?] ?? Products [3]
  :
  ?? [+] ?? Electronics [5]
  ?? [?] ?? Furniture [2]
  :    :
  :    ?? [+] ?? Chairs [0]
  :    ??     Tables [0]
  ?? [+] ?? Clothing [8]
```

**Just like SQL Server!** ?

---

## ? WHAT CHANGED

? Expand button: Square box with +/?
? Folder icons: Open/closed folders
? Tree lines: Dotted lines
? Colors: SSMS blue theme
? Fonts: Segoe UI
? Spacing: Compact SSMS style

---

**Status**: ? COMPLETE
**Style**: ?? SQL SERVER MANAGEMENT STUDIO
