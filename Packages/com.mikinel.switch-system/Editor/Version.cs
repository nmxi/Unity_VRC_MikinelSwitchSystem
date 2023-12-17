using System.IO;
using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    public static class Version
    {
        public static string CurrentVersion()
        {
            var AssetDirectoryPath = Application.dataPath;
            var upDirectory = Path.Combine(AssetDirectoryPath, "../");
            var packageDirectoryPath = Path.Combine(upDirectory, "Packages");
            var packageJsonPath = Path.Combine(packageDirectoryPath, "com.mikinel.switch-system/package.json");
            var packageJson = File.ReadAllText(packageJsonPath);
            
            var version = packageJson.Split(new[] {"\"version\": \""}, System.StringSplitOptions.None)[1];
            version = version.Split(new[] {"\","}, System.StringSplitOptions.None)[0];
            return version;
        }
    }
}