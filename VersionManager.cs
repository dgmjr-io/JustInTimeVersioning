/*
 * VersionManager.cs
 *
 *   Created: 2022-11-10-09:07:36
 *   Modified: 2022-11-19-04:05:35
 *
 *   Author: David G. Moore, Jr, <david@dgmjr.io>
 *
 *   Copyright © 2022-2023 David G. Moore, Jr,, All Rights Reserved
 *      License: MIT (https://opensource.org/licenses/MIT)
 */

namespace JustInTimeVersioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static System.IO.Path;

public class VersionManager : IDisposable
{
    private readonly Mutex _mutex = new();
    private const int MutexTimeout = 10000;
    public VersionManager(TaskLog log)
    {
        Log = log;
        _mutex.WaitOne(MutexTimeout);
    }

    private TaskLog Log { get; }

    public virtual void SaveVersions()
    {
        var sortedVersions = Versions.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        File.WriteAllText(VersionsJsonFilePath, Serialize(sortedVersions, new Jso(Jso.Default) { WriteIndented = true }));
        var versionsProps = new XDocument(
            new XComment("<auto-generated />"),
            new XComment("This file is automatically generated by Dgmjr.Versioning. Do not edit."),
            new XElement("Project",
                new XElement("PropertyGroup",
                    new XElement("JustInTimeVersioningVersion", sortedVersions["JustInTimeVersioningSdk"])),
                new XElement("ItemGroup",
                    sortedVersions.Select(kvp =>
                        new XElement("PackageReference",
                            new XAttribute("Update", kvp.Key),
                            new XAttribute("Version", $"[{kvp.Value}, )"),
                            new XAttribute("Condition", $"'$(PackageId)' != '{kvp.Key}'"))))));
        versionsProps.Save(VersionsPropsFilePath);
    }

    public DirectoryInfo CurrentDirectory => new(Directory.GetCurrentDirectory());
    public virtual string Configuration { get; set; } = "Local";
    public virtual string VersionsPropsFileName { get; set; } = "Packages/Versions.{0}.props";
    public virtual string VersionsJsonFileName { get; set; } = "Packages/Versions.{0}.json";
    public virtual string VersionsJsonFilePath => Format(Combine(GetDirectoryNameOfFileAbove(CurrentDirectory.FullName, Format(VersionsJsonFileName, Configuration)), VersionsJsonFileName), Configuration);
    public virtual string VersionsPropsFilePath => Format(Combine(GetDirectoryNameOfFileAbove(CurrentDirectory.FullName, Format(VersionsPropsFileName, Configuration)), VersionsPropsFileName), Configuration);

    public virtual IStringDictionary Versions => _versions ??= InitializeVersionsDictionary();

    private IStringDictionary? _versions = null;
    private IStringDictionary InitializeVersionsDictionary()
    {
        if (!File.Exists(VersionsJsonFilePath))
        {
            File.WriteAllText(VersionsJsonFilePath, "{}");
        }
        if (!File.Exists(VersionsPropsFilePath))
        {
            File.WriteAllText(VersionsPropsFilePath, "<Project />");
        }
        return Deserialize<StringDictionary>(
            File.ReadAllText(VersionsJsonFilePath))!;
    }

    public virtual string GetPathOfFileAbove(string fileName)
    {
        WriteLine($"Looking for {fileName} in {Directory.GetCurrentDirectory()}");
        var currentDirectory = Directory.GetCurrentDirectory();
        var directoryInfo = new DirectoryInfo(currentDirectory);
        var lookingForFile = new FileInfo(Combine(directoryInfo.FullName, fileName));
        var lookingForDirectory = new DirectoryInfo(Combine(directoryInfo.FullName, fileName));
        while (directoryInfo != null && !lookingForFile.Exists && !lookingForDirectory.Exists)
        {
            directoryInfo = directoryInfo.Parent;
            lookingForFile = new FileInfo(Combine(directoryInfo.FullName, fileName));
            lookingForDirectory = new DirectoryInfo(Combine(directoryInfo.FullName, fileName));
        }

        return Combine(directoryInfo.FullName, fileName);
    }

    public virtual string GetDirectoryNameOfFileAbove(string startingDirectory, string fileName)
    {
        var currentDirectory = startingDirectory;
        var directoryInfo = new DirectoryInfo(currentDirectory);
        var lookingForFile = new FileInfo(Combine(directoryInfo.FullName, fileName));
        var lookingForDirectory = new DirectoryInfo(Combine(directoryInfo.FullName, fileName));
        while (directoryInfo is not null && !lookingForFile.Exists && !lookingForDirectory.Exists)
        {
            Log.LogMessage($"Looking for {fileName} in {directoryInfo}...");
            directoryInfo = directoryInfo.Parent;
            lookingForFile = new FileInfo(Combine(directoryInfo.FullName, fileName));
            lookingForDirectory = new DirectoryInfo(Combine(directoryInfo.FullName, fileName));
        }

        return directoryInfo.FullName;
    }

    public virtual string GetPathOfRootDirectory(DirectoryInfo currentDirectory, string lookingForDirectoryName, DirectoryInfo? lastDirectoryWhereTheThingWasFound = default)
    {
        WriteLine($"Looking for {lookingForDirectoryName} in {currentDirectory.FullName}");
        if (currentDirectory == null)
        {
            return lastDirectoryWhereTheThingWasFound.FullName;
        }
        else if (currentDirectory.Name == lookingForDirectoryName)
        {
            return currentDirectory.FullName;
        }
        else if (currentDirectory.GetFileSystemInfos(lookingForDirectoryName).Any())
        {
            lastDirectoryWhereTheThingWasFound = new DirectoryInfo(currentDirectory.GetFileSystemInfos(lookingForDirectoryName).First().FullName);
        }
        return GetPathOfRootDirectory(currentDirectory.Parent, lookingForDirectoryName, currentDirectory);
    }

    private static readonly object @lock = "Lock";
    private bool _disposedValue;

    public virtual string? GetVersion(string packageName)
        => Versions.ContainsKey(packageName) ? Versions[packageName] : null;

    public virtual void SaveVersion(string packageName, string version)
    {
        lock (@lock)
        {
            Versions[packageName] = version;
            SaveVersions();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _mutex.ReleaseMutex();
            }

            _versions = null;
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
