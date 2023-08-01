---
title: Just-in-Time-Versioning SDK
authors:
  - dgmjr
description: An SDK for ensuring that NuGet package versions are kept in sync just in time.
type: readme
slug: time-versioning-sdk
project: Shared
license: MIT
lastmod: 2023-07-02T00:20:59.751Z
version: 0.0.1
date: 2023-07-02T00:09:50.598Z
keywords:
  - msbuild-sdk
  - just-in-time-versioning
  - versioning
---

# Just-in-Time-Versioning SDK

An SDK for ensuring that NuGet package versions are kept in sync just in time.

## Prerequisites

- [MinVer](https://github.com/adamralph/minver)
- [GitInfo](https://github.com/devlooped/GitInfo)

## Getting Started

Add the following code to any of your project files:

```xml
<Sdk Name="JustInTimeVersioning" />
```

And also add the following to your `global.json` file:

```json
"msbuild-sdks":{
    "JustInTimeVersioning": "the-current-version-of-JustInTimeVersioning"
}
```

Next, you'll want to initialize your just-in-time packages folder by running the following command:

```shell
    msbuild [yourprojectname].*proj -t:InitJustInTimeVersioning
```

This will create the packages folder and populate it with empty `.json` and `.props` files, which will be used to hold the latest versions of your NuGet packages.

