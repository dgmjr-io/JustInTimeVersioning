---
author: David G. Moore, Jr,
author_email: david@dgmjr.io
title: README.md
modified: 2022-11-19-04:04:44
created: 2022-11-10-09:07:36
license: MIT
---

# JustInTimeVersioning

A utility to keep central version records in sync with the repository.  This is useful for projects with multiple repositories that need to share a common version record.

## Installation

The following command will initialize the repository with no version numbers in the root of the repository, up one directory.

```bash
dotnet build --Target:InitJustInTimeVersioning JustInTimeVersioning.csproj
```

To control which directory is used, use the `JustInTimeVersioningInitPackagesDirectory` property like this:

```bash
dotnet build --Target:InitJustInTimeVersioning --property:JustInTimeVersioningInitPackagesDirectory=C:\JustInTimeVersioning
```

This will create the following directory structure:

## Usage

Now, simply refer to the JustInTimeVersioning `.csproj` or `nuget` package from your `.csproj` files and they will automatically update the version number in the
