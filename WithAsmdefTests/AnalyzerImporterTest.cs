// Copyright (c) 2021 Koji Hasegawa
// This software is released under the MIT License.

using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AnalyzerImporter
{
    [TestFixture]
    public class AnalyzerImporterTest
    {
        [Test]
        public void MyAssemblyName_gotName()
        {
            var actual = AnalyzerImporter.MyAssemblyName;
            Assert.That(actual, Is.EqualTo("AnalyzerInPackageAsm"));
        }

        [Test]
        public void GetAssemblyRoot_gotRootPathForAssetDatabase()
        {
            var actual = AnalyzerImporter.MyAssemblyRoot;
            Assert.That(actual, Is.EqualTo("Packages/com.nowsprinting.analyzer-in-package/WithAsmdef"));
        }

        private static string AnalyzerElementString(string dllName)
        {
            XNamespace xNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            var path = Path.GetFullPath("Packages/com.nowsprinting.analyzer-in-package/WithAsmdef");
            return new XElement(xNamespace + "Analyzer", new XAttribute("Include", $"{path}/{dllName}")).ToString();
        }

        [Test]
        public void MyAnalyzers_gotAnalyzerNodes()
        {
            var actual = AnalyzerImporter.MyAnalyzers.ToArray();
            Assert.That(actual.Length, Is.EqualTo(1));
            Assert.That(actual[0].ToString(), Is.EqualTo(AnalyzerElementString("AnalyzerInPackageWithAsmdef.dll")));
        }

        [Test]
        public void MyRelativePathAnalyzers_gotRelativePaths()
        {
            var actual = AnalyzerImporter.MyRelativePathAnalyzers;
            Assert.That(actual,
                Is.EqualTo(new[]
                {
                    $"Packages/com.nowsprinting.analyzer-in-package/WithAsmdef/AnalyzerInPackageWithAsmdef.dll",
                }));
        }

        [Test]
        public void RemoveRelativePathAnalyzers_removed()
        {
            var content = AssetDatabase
                .LoadAssetAtPath<TextAsset>(
                    "Packages/com.nowsprinting.analyzer-in-package/WithAsmdefTests/SpecifiedReferences_csproj.txt")
                .text;
            var expected = AssetDatabase
                .LoadAssetAtPath<TextAsset>(
                    "Packages/com.nowsprinting.analyzer-in-package/WithAsmdefTests/SpecifiedReferences_csproj_RemovedRelativePathAnalyzers.txt")
                .text;
            var actual = AnalyzerImporter.RemoveRelativePathAnalyzers(content);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("NoReferences_csproj.txt", false)]
        [TestCase("SpecifiedReferences_csproj.txt", true)]
        [TestCase("SpecifiedReferences_csproj_upm.txt", true)]
        public void ExistProjectReferenceInCsproj_correct(string file, bool expected)
        {
            var content = AssetDatabase
                .LoadAssetAtPath<TextAsset>($"Packages/com.nowsprinting.analyzer-in-package/WithAsmdefTests/{file}")
                .text;
            var actual = AnalyzerImporter.ExistProjectReferenceInCsproj(content);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
