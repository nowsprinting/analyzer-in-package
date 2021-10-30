// Copyright (c) 2021 Koji Hasegawa
// This software is released under the MIT License.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

namespace AnalyzerImporter
{
    /// <summary>
    /// TODO: あなたのライブラリのasmdef下に、アナライザDLLとこのファイルを置いてください
    /// TODO: 英語で
    ///
    /// 1. asmdef依存関係を考慮していないアナライザを除去します
    /// 2. このasmを依存関係に含むcsprojに、このasm下のアナライザを適用します
    /// 
    /// </summary>
    /// <remarks>
    /// Required same as the assembly definition file (.asmdef) name and assembly name.
    /// </remarks>
    public class AnalyzerImporter : AssetPostprocessor
    {
        private static readonly XNamespace s_xNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static string MyAssemblyName => typeof(AnalyzerImporter).Assembly.GetName().Name;

        public static string MyAssemblyRoot => AssetDatabase
            .FindAssets(MyAssemblyName, new string[] { "Packages" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .First()
            .Replace($"/{MyAssemblyName}.asmdef", "");
        // NOTE: required same as the assembly definition file (.asmdef) name and assembly name.

        public static IEnumerable<XElement> MyAnalyzers => AssetDatabase
            .FindAssets("l:RoslynAnalyzer", new string[] { MyAssemblyRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(Path.GetFullPath)
            .Select(x => new XElement(s_xNamespace + "Analyzer", new XAttribute("Include", x)));

        public static IEnumerable<string> MyRelativePathAnalyzers => AssetDatabase
            .FindAssets("l:RoslynAnalyzer", new string[] { MyAssemblyRoot })
            .Select(AssetDatabase.GUIDToAssetPath);

        public static string RemoveRelativePathAnalyzers(string content)
        {
            foreach (var relativePathAnalyzer in MyRelativePathAnalyzers)
            {
                content = content.Replace($"<Analyzer Include=\"{relativePathAnalyzer}\" />", "");
            }

            return content;
        }

        public static bool ExistProjectReferenceInCsproj(string content)
        {
            return
                content.Contains($"<AssemblyName>{MyAssemblyName}</AssemblyName>") ||
                content.Contains($"<ProjectReference Include=\"{MyAssemblyName}.csproj\">");
        }

        private static string OnGeneratedCSProject(string path, string content)
        {
            content = RemoveRelativePathAnalyzers(content);

            if (!ExistProjectReferenceInCsproj(content))
            {
                return content;
            }

            var xDocument = XDocument.Parse(content);
            xDocument.Root?.Add(new XElement(s_xNamespace + "ItemGroup", MyAnalyzers.ToArray() as object[]));
            return $"{xDocument.Declaration}{Environment.NewLine}{xDocument.Root}";
        }
    }
}
#endif
