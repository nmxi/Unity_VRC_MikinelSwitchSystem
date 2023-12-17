using System.IO;
using mikinel.vrc.SwitchSystem.Editor;
using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    public static class Version
    {
        public static string CurrentVersion()
        {
            var AssetDirectoryPath = Application.dataPath;
            var upDirectory = Path.Combine(AssetDirectoryPath, "../");
            var packageJsonPath = Path.Combine(upDirectory, $"{Config.packagePath}/package.json");
            var packageJson = File.ReadAllText(packageJsonPath);
            
            var version = packageJson.Split(new[] {"\"version\": \""}, System.StringSplitOptions.None)[1];
            version = version.Split(new[] {"\","}, System.StringSplitOptions.None)[0];
            return version;
        }
    }
}