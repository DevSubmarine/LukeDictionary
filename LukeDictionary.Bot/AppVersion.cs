using System;
using System.Diagnostics;
using System.Reflection;

namespace DevSubmarine.LukeDictionary
{
    /// <summary>Utility class for retrieving app version.</summary>
    /// <remarks>To update this version, change package information in project properties.</remarks>
    public static class AppVersion
    {
        private static readonly Lazy<string> _version = new Lazy<string>(() => GetVersion(Assembly.GetEntryAssembly(), false));
        private static readonly Lazy<string> _versionWithRevision = new Lazy<string>(() => GetVersion(Assembly.GetEntryAssembly(), true));

        /// <summary>Gets app version, without revision.</summary>
        public static string Version => _version.Value;
        /// <summary>Gets app version, with revision (if any).</summary>
        public static string VersionWithRevision => _versionWithRevision.Value;

        /// <summary>Gets file version of the provided assembly.</summary>
        /// <param name="assembly">Assembly to get version of.</param>
        /// <param name="includeRevision">Whether to include revision. If revision is 0, it'll be skipped regardless.</param>
        /// <returns>String representing assembly file version.</returns>
        public static string GetVersion(Assembly assembly, bool includeRevision = false)
        {
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(assembly.Location);

            string result = $"{version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}";
            if (version.FilePrivatePart != 0 && includeRevision)
                result += $" r{version.FilePrivatePart}";

            return result;
        }
    }
}
