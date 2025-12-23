# ?? COMPLETE GUIDE: PUSH TO NUGET STEP BY STEP

---

## ?? STEP 1: CREATE CLASS LIBRARIES

### 1.1 Open Terminal in Your Solution Root

```bash
cd C:\Users\radoi\source\repos\ContextProject\orgTestapp
```

### 1.2 Create OrgTestapp.Core (Class Library)

```bash
dotnet new classlib -n OrgTestapp.Core -f net10.0
```

**Expected Output:**
```
The template "Class Library" was created successfully.
```

### 1.3 Create OrgTestapp.Components (Razor Class Library)

```bash
dotnet new razorclasslib -n OrgTestapp.Components -f net10.0
```

**Expected Output:**
```
The template "Razor Class Library" was created successfully.
```

### 1.4 Check Project Structure

```bash
tree
```

Should look like:
```
orgTestapp/
??? OrgTestapp.Core/
?   ??? Class1.cs
?   ??? OrgTestapp.Core.csproj
??? OrgTestapp.Components/
?   ??? Components/
?   ?   ??? ExampleComponent.razor
?   ??? wwwroot/
?   ??? OrgTestapp.Components.csproj
??? orgTestapp/
?   ??? Components/
?   ??? Pages/
?   ??? orgTestapp.csproj
??? orgTestapp.sln
```

---

## ?? STEP 2: ADD DEPENDENCIES

### 2.1 Add to OrgTestapp.Core

```bash
cd OrgTestapp.Core
dotnet add package HierarchyId
dotnet add package ResultPattern.Core
cd ..
```

### 2.2 Add to OrgTestapp.Components

```bash
cd OrgTestapp.Components
dotnet add package Microsoft.AspNetCore.Components.Web
dotnet add package OrgTestapp.Core --version 1.0.0
cd ..
```

### 2.3 Verify Dependencies

Open `.csproj` files and check `<ItemGroup>` section has packages.

---

## ?? STEP 3: MOVE YOUR CODE

### 3.1 Create Folders

```bash
# In OrgTestapp.Core
mkdir OrgTestapp.Core\Entities
mkdir OrgTestapp.Core\Services
mkdir OrgTestapp.Core\Storage

# In OrgTestapp.Components
mkdir OrgTestapp.Components\Components
mkdir OrgTestapp.Components\wwwroot\css
```

### 3.2 Copy Core Files

**From original `orgTestapp\Entities\Node.cs`:**
? **To** `OrgTestapp.Core\Entities\Node.cs`

**From original `orgTestapp\Services\IOrgService.cs`:**
? **To** `OrgTestapp.Core\Services\IOrgService.cs`

**From original `orgTestapp\Services\OrgService.cs`:**
? **To** `OrgTestapp.Core\Services\OrgService.cs`

**From original `orgTestapp\Storage\IOrgStorage.cs`:**
? **To** `OrgTestapp.Core\Storage\IOrgStorage.cs`

**From original `orgTestapp\Storage\OrgStorage.cs`:**
? **To** `OrgTestapp.Core\Storage\OrgStorage.cs`

### 3.3 Copy Component Files

**From original `orgTestapp\Components\CategoryTree.razor`:**
? **To** `OrgTestapp.Components\Components\CategoryTree.razor`

**From original `orgTestapp\Components\CategoryTree.razor.cs`:**
? **To** `OrgTestapp.Components\Components\CategoryTree.razor.cs`

**From original `orgTestapp\Components\TreeNode.razor`:**
? **To** `OrgTestapp.Components\Components\TreeNode.razor`

**From original `orgTestapp\Components\TreeNode.razor.cs`:**
? **To** `OrgTestapp.Components\Components\TreeNode.razor.cs`

**From original `orgTestapp\wwwroot\css\site.css`:**
? **To** `OrgTestapp.Components\wwwroot\css\site.css`

### 3.4 Delete Original Class1.cs

```bash
# Delete template files
rm OrgTestapp.Core\Class1.cs
rm OrgTestapp.Components\Components\ExampleComponent.razor
```

---

## ?? STEP 4: UPDATE PROJECT FILES

### 4.1 Update OrgTestapp.Core.csproj

Open `OrgTestapp.Core\OrgTestapp.Core.csproj` and replace content:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    
    <!-- NuGet Metadata -->
    <PackageId>OrgTestapp.Core</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Hierarchical organization management core library with smart level calculation and drag-drop support</Description>
    <PackageProjectUrl>https://github.com/yourusername/OrgTestapp</PackageProjectUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/yourusername/OrgTestapp</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>hierarchy;organization;dragdrop;net10;core;library</PackageTags>
    <PackageReleaseNotes>Initial release with complete hierarchical organization support and smart routing</PackageReleaseNotes>
    
    <!-- Documentation -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="HierarchyId" Version="1.0.0" />
    <PackageReference Include="ResultPattern.Core" Version="1.0.0" />
  </ItemGroup>

</Project>
```

### 4.2 Update OrgTestapp.Components.csproj

Open `OrgTestapp.Components\OrgTestapp.Components.csproj` and replace content:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    
    <!-- NuGet Metadata -->
    <PackageId>OrgTestapp.Components</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Professional Blazor components for hierarchical organization management with drag-and-drop, modals, and expand/collapse</Description>
    <PackageProjectUrl>https://github.com/yourusername/OrgTestapp</PackageProjectUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/yourusername/OrgTestapp</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>blazor;components;hierarchy;organization;dragdrop;ui;reusable</PackageTags>
    <PackageReleaseNotes>Initial release with CategoryTree and TreeNode components featuring modals, drag-drop, and responsive design</PackageReleaseNotes>
    
    <!-- Documentation -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="10.0.0" />
    <PackageReference Include="OrgTestapp.Core" Version="1.0.0" />
  </ItemGroup>

</Project>
```

---

## ?? STEP 5: BUILD & TEST

### 5.1 Clean & Build

```bash
dotnet clean
dotnet build -c Release
```

### 5.2 Fix Any Errors

If build fails:
- Check namespace declarations match folder structure
- Ensure all using statements are correct
- Verify no circular dependencies

---

## ?? STEP 6: CREATE NUGET PACKAGES

### 6.1 Create Output Folder

```bash
mkdir packages
```

### 6.2 Pack Libraries

```bash
# Pack Core
dotnet pack OrgTestapp.Core/OrgTestapp.Core.csproj -c Release -o ./packages

# Pack Components
dotnet pack OrgTestapp.Components/OrgTestapp.Components.csproj -c Release -o ./packages
```

### 6.3 Verify Packages Created

```bash
dir packages
```

Should show:
```
OrgTestapp.Core.1.0.0.nupkg
OrgTestapp.Components.1.0.0.nupkg
```

---

## ?? STEP 7: SETUP NUGET ACCOUNT

### 7.1 Register on NuGet.org

1. Go to https://www.nuget.org
2. Click "Register"
3. Fill in:
   - Username (e.g., `radoin082`)
   - Email
   - Password
4. Click "Create Account"
5. Check email, click verification link

### 7.2 Create API Key

1. Login to https://www.nuget.org
2. Click your profile avatar
3. Click "API Keys"
4. Click "Create New"
5. Name: `OrgTestapp-Release`
6. Select "Push new packages and package versions"
7. Click "Create"
8. **COPY AND SAVE** the key somewhere safe!

**Example key format:**
```
oy2ai...rest of key...gq
```

---

## ?? STEP 8: PUBLISH TO NUGET

### 8.1 Configure NuGet Credentials

```bash
dotnet nuget update source nuget.org -u __USERNAME__ -p YOUR_API_KEY --store-password-in-clear-text
```

Replace:
- `YOUR_API_KEY` - Your actual API key from Step 7.2
- Keep `__USERNAME__` as is (NuGet uses this placeholder)

### 8.2 Push Core Package

```bash
dotnet nuget push packages/OrgTestapp.Core.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### 8.3 Push Components Package

```bash
dotnet nuget push packages/OrgTestapp.Components.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### 8.4 Wait for Processing

- Usually takes **a few minutes**
- Check email for confirmation

---

## ? STEP 9: VERIFY PACKAGES

### 9.1 Check NuGet.org

1. Go to https://www.nuget.org/packages
2. Search for "OrgTestapp.Core"
3. Should see your package!

### 9.2 Install Locally to Test

```bash
# Create test project
dotnet new console -n TestApp

# Install packages
cd TestApp
dotnet add package OrgTestapp.Core
dotnet add package OrgTestapp.Components
```

### 9.3 Verify Installation

Check `.csproj` file has:
```xml
<ItemGroup>
  <PackageReference Include="OrgTestapp.Core" Version="1.0.0" />
  <PackageReference Include="OrgTestapp.Components" Version="1.0.0" />
</ItemGroup>
```

---

## ?? SUCCESS!

**Your packages are now available on NuGet!**

### Share with Others

```
?? OrgTestapp.Core
https://www.nuget.org/packages/OrgTestapp.Core

?? OrgTestapp.Components  
https://www.nuget.org/packages/OrgTestapp.Components
```

### Install Command

```bash
dotnet add package OrgTestapp.Core
dotnet add package OrgTestapp.Components
```

---

## ?? NEXT STEPS

1. ? Add to GitHub
2. ?? Create comprehensive README
3. ?? Add CI/CD pipeline
4. ?? Track downloads on NuGet
5. ?? Plan future versions
6. ?? Engage with users

---

**Congratulations!** You've published professional NuGet packages! ??
