# Project Configuration Standards

## Core Principle
**Minimize project files.** Use centralized `Directory.Build.props` and `Directory.Packages.props` for all shared configuration.

---

## Automatic Inheritance

### All Projects (`src/` and `tests/`)
Inherit from `Directory.Build.props`:
- `TargetFramework`: net10.0
- `ImplicitUsings`: enable
- `Nullable`: enable
- `Deterministic`: true
- `RootNamespace`: Dilcore.$(MSBuildProjectName)

### Test Projects Only (`tests/`)
Additionally inherit from `tests/Directory.Build.props`:
- `IsPackable`: false
- `IsTestProject`: true
- Common test packages: coverlet.collector, JunitXml.TestLogger, Microsoft.NET.Test.Sdk, NUnit, NUnit.Analyzers, NUnit3TestAdapter, Moq, Shouldly
- Global using: NUnit.Framework

---

## Rules

### ❌ NEVER Include in Project Files

**PropertyGroup settings:**
- `TargetFramework`, `ImplicitUsings`, `Nullable`, `Deterministic` (all projects)
- `IsPackable`, `IsTestProject` (test projects)
- `LangVersion` (unless specific override needed)

**Package versions:**
```xml
<!-- ❌ WRONG -->
<PackageReference Include="NUnit" Version="4.3.2" />

<!-- ✅ CORRECT -->
<PackageReference Include="NUnit" />
```

**Common test packages** (test projects):
- coverlet.collector, Microsoft.NET.Test.Sdk, NUnit, NUnit.Analyzers, NUnit3TestAdapter, Moq, Shouldly, JunitXml.TestLogger

**Global usings** (test projects):
```xml
<!-- ❌ WRONG -->
<Using Include="NUnit.Framework" />
```

---

## ✅ Correct Project Files

### Minimal Source Project
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Azure.Identity" />
  </ItemGroup>
</Project>
```

### Minimal Test Project
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\src\MyLibrary\MyLibrary.csproj" />
  </ItemGroup>
</Project>
```

### Test Project with Extra Package
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <!-- Only project-specific packages, common ones inherited -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\src\WebApi\WebApi.csproj" />
  </ItemGroup>
</Project>
```

---

## Central Package Management

All package versions defined in `Directory.Packages.props`.

**Adding new package:**
1. Add to `Directory.Packages.props`: `<PackageVersion Include="NewPackage" Version="1.0.0" />`
2. Reference in project: `<PackageReference Include="NewPackage" />`

**Never** add `Version` attribute to `<PackageReference>` elements.

---

## When to Override

**✅ Acceptable overrides:**
- Project-specific settings not in Directory.Build.props (e.g., `GenerateDocumentationFile`)
- Custom `RootNamespace` for specific project needs
- `OutputType` for executable projects

**❌ Never override:**
- TargetFramework, Nullable, ImplicitUsings, Deterministic
- IsPackable, IsTestProject (test projects)

---

## Migration Checklist

Fix non-compliant projects:
1. Remove: `TargetFramework`, `ImplicitUsings`, `Nullable`, `Deterministic`, `IsPackable`, `IsTestProject`
2. Remove version attributes from all `<PackageReference>` elements
3. Remove common test packages (NUnit, Moq, Shouldly, coverlet.collector, etc.)
4. Remove `<Using Include="NUnit.Framework" />`
5. Verify: `dotnet build path/to/project.csproj`

---

## Quick Reference

| What | Where | Who Inherits |
|------|-------|--------------|
| TargetFramework, Nullable, ImplicitUsings, Deterministic | `Directory.Build.props` | All projects |
| IsPackable, IsTestProject | `tests/Directory.Build.props` | Test projects |
| Common test packages | `tests/Directory.Build.props` | Test projects |
| Package versions | `Directory.Packages.props` | All projects |
| NUnit.Framework using | `tests/Directory.Build.props` | Test projects |
