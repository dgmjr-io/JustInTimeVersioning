//
// SaveVersionNumberCentrally.cs
//
//   Created: 2022-10-24-04:48:49
//   Modified: 2022-10-29-12:13:24
//
//   Author: David G. Moore, Jr, <david@dgmjr.io>
//
//   Copyright © 2022-2023 David G. Moore, Jr,, All Rights Reserved
//      License: MIT (https://opensource.org/licenses/MIT)
//

namespace JustInTimeVersioning;

public class SaveVersionNumberCentrally : MSBTask, IDisposable
{
    [MSBF.Required]
    public string PackageName { get; set; } = string.Empty;

    [MSBF.Required]
    public string Version { get; set; } = string.Empty;
    [MSBF.Required]
    public string Configuration { get; set; } = "Local";
    public string VersionsJsonFileName { get => VersionManager.VersionsJsonFileName; set => VersionManager.VersionsJsonFileName = value; }
    public string VersionsPropsFileName { get => VersionManager.VersionsPropsFileName; set => VersionManager.VersionsPropsFileName = value; }
    private VersionManager? _versionManager = null!;
    private bool disposedValue;

    public VersionManager VersionManager => _versionManager ??= new VersionManager(Log);

    public override bool Execute()
    {
        VersionManager.Configuration = Configuration;
        VersionManager.SaveVersion(PackageName, Version);
        Log.LogMessage($"Saved version {Version} for package {PackageName} to {VersionManager.VersionsPropsFilePath}.");
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            _versionManager?.Dispose();
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SaveVersionNumberCentrally()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
