﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calamari.Integration.Packages;
using Calamari.Util;
#if USE_NUGET_V2_LIBS
using Calamari.NuGet.Versioning;
#else
using NuGet.Versioning;
#endif

namespace Calamari.Integration.FileSystem
{
    public class PackageStore
    {
        private readonly IPackageExtractor packageExtractorFactory;
        readonly ICalamariFileSystem fileSystem = CalamariPhysicalFileSystem.GetPhysicalFileSystem();
        readonly string rootDirectory = Path.Combine(TentacleHome, "Files");

        private static string TentacleHome
        {
            get
            {
                var tentacleHome = Environment.GetEnvironmentVariable("TentacleHome");
                if (tentacleHome == null)
                {
                    Log.Error("Environment variable 'TentacleHome' has not been set.");
                }
                return tentacleHome;
            }
        }

        public PackageStore(IPackageExtractor packageExtractorFactory)
        {
            this.packageExtractorFactory = packageExtractorFactory;
        }

        public string GetPackagesDirectory()
        {
            return rootDirectory;
        }

        public StoredPackage GetPackage(string packageFullPath)
        {
            var zip = PackageMetadata(packageFullPath);
            if (zip == null)
                return null;
            
            var package = ExtendedPackageMetadata(packageFullPath, PackageMetadata(packageFullPath));
            if (package == null)
                return null;

            return new StoredPackage(package, packageFullPath);
        }

        public StoredPackage GetPackage(ExtendedPackageMetadata metadata)
        {
            var name = GetNameOfPackage(metadata);
            fileSystem.EnsureDirectoryExists(rootDirectory);

            foreach (var file in PackageFiles(name))
            {
                var storedPackage = GetPackage(file);
                if (storedPackage == null)
                    continue;

                if (!string.Equals(storedPackage.Metadata.Id, metadata.Id, StringComparison.OrdinalIgnoreCase) || NuGetVersion.Parse(storedPackage.Metadata.Version) != NuGetVersion.Parse(metadata.Version))
                    continue;

                if (string.IsNullOrWhiteSpace(metadata.Hash))
                    return storedPackage;

                if (metadata.Hash == storedPackage.Metadata.Hash)
                    return storedPackage;
            }

            return null;
        }

        private IEnumerable<string> PackageFiles(string name)
        {
            var patterns = packageExtractorFactory.Extensions.Select(e => name + e +"-*").ToArray();
            return fileSystem.EnumerateFilesRecursively(rootDirectory, patterns);
        }

        public IEnumerable<StoredPackage> GetNearestPackages(string packageId, NuGetVersion version, int take = 5)
        {
            fileSystem.EnsureDirectoryExists(rootDirectory);
            var zipPackages =
                from filePath in PackageFiles(packageId +"*")
                let zip = PackageMetadata(filePath)
                where zip != null && zip.Id == packageId && new NuGetVersion(zip.Version) <= version
                orderby zip.Version descending
                select new {zip, filePath};

            return
                from zipPackage in zipPackages.Take(take)
                let package = ExtendedPackageMetadata(zipPackage.filePath, zipPackage.zip)
                where package != null
                select new StoredPackage(package, zipPackage.filePath);
        }

        PackageMetadata PackageMetadata(string file)
        {
            try
            {
                return packageExtractorFactory.GetMetadata(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static ExtendedPackageMetadata ExtendedPackageMetadata(string file, PackageMetadata metadata)
        {
            try
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    return new ExtendedPackageMetadata
                    {
                        Id = metadata.Id,
                        Version = metadata.Version,
                        FileExtension = metadata.FileExtension,
                        Hash = HashCalculator.Hash(stream),
                    };
                }
            }
            catch (IOException)
            {
                return null;
            }
        }

        static string GetNameOfPackage(PackageMetadata metadata)
        {
            return metadata.Id + "." + metadata.Version + "*";
        }
    }
}
