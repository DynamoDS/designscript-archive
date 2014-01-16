using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTestFx;

namespace Autodesk.DesignScript.Geometry.Tests
{
    [TestFixture]
    class ProtoGeometryTests
    {
        bool generateBaseFiles = false;
        string AutoCADExePath = GetAutoCADExePath();
        string OutputPath = GetOutputPath();
        string ScrPath = null;

        [SetUp]
        public void SetUp()
        {
            GeometryTestFrame.ScriptDir = @".\Scripts\Geometry\";
            GeometryTestFrame.BaseDir = @".\Scripts\GeometryBase\";
            ScrPath = Path.GetTempPath();
            ScrPath = Path.Combine(ScrPath, "DesignScript");

            object obj = GeometryTestFrame.GetRegistryValue("Software\\Autodesk\\DesignScript", "GenerateBaseFiles");
            if (null != obj)
            {
                int rst;
                if (int.TryParse(obj.ToString(), out rst))
                    generateBaseFiles = (rst == 1);
            }

            if (generateBaseFiles)
            {
                if (!Directory.Exists(ScrPath))
                {
                    Directory.CreateDirectory(ScrPath);
                }
                else
                {
                    DirectoryInfo dir = new DirectoryInfo(ScrPath);
                    dir.GetFiles().ToList().ForEach(file => file.Delete());
                }
            }
        }

        private static string GetAutoCADExePath()
        {
            object obj = GeometryTestFrame.GetRegistryValue("Software\\Autodesk\\DesignScript", "AcadExePath");
            if (null != obj)
            {
                return Path.Combine(obj.ToString(), "accoreconsole.exe");
            }
            return string.Empty;
        }

        private static string GetOutputPath()
        {
            object obj = GeometryTestFrame.GetRegistryValue("Software\\Autodesk\\DesignScript", "OutputPath");
            if (null != obj)
            {
                return obj.ToString();
            }
            return string.Empty;
        }

        private void PointArrayCreation()
        {
            String filePath = @".\Scripts\Geometry\point_creation_array.ds";
            GeometryTestFrame.RunAndCompare(filePath);
        }

        private void LineCreation()
        {
            String filePath = @".\Scripts\Geometry\line_creation.ds";
            GeometryTestFrame.RunAndCompare(filePath);
        }

        private void TestAll()
        {
            List<string> failedFiles = new List<string>();
            List<string> successfulFiles = new List<string>();
            var filepathes = Directory.EnumerateFiles(GeometryTestFrame.ScriptDir);
            foreach (var filepath in filepathes)
            {
                if (filepath.EndsWith(@".ds"))
                {
                    if (!GeometryTestFrame.RunAndCompareNoAssert(filepath))
                    {
                        failedFiles.Add(filepath);
                        Console.WriteLine("FAILED for case: {0}", filepath);
                    }
                    else
                    {
                        successfulFiles.Add(filepath);
                        Console.WriteLine("SUCCESSFUL for case: {0}", filepath);
                    }
                }
            }
            Console.WriteLine("------------------------SUMMARY-------------------------");
            foreach (var item in failedFiles)
            {
                Console.WriteLine("FAILED for case: {0}", item);
            }
            foreach (var item in successfulFiles)
            {
                Console.WriteLine("SUCCESSFUL for case: {0}", item);
            }
            Console.WriteLine("FAILED cases' count: {0}", failedFiles.Count);
            Console.WriteLine("SUCCESSFUL cases' count: {0}", successfulFiles.Count);
        }

        [Test, TestCaseSource("GetTestCases")]
        [Category("AdharnessTestCases")]
        public void AdharnessTestCases(string test)
        {
            string filePath = Path.Combine(GeometryTestFrame.ScriptDir, test + ".ds");
            if (!generateBaseFiles)
                GeometryTestFrame.RunAndCompare(filePath);
            else
                RunAndGenerateBase(filePath);
        }

        [Test, TestCaseSource("GetSampleTestCases")]
        [Category("SampleTestCases")]
        public void SampleTestCases(string test)
        {
            string filePath = Path.Combine(GeometryTestFrame.ScriptDir, test + ".ds");
            if (!generateBaseFiles)
                GeometryTestFrame.RunAndCompare(filePath);
            else
                RunAndGenerateBase(filePath);
        }

        private void RunAndGenerateBase(string filePath)
        {
            string scrContent = string.Format(@"
(command ""_netload"" ""AcDesignScriptTest.dll"")
(command ""_createbase"" ""{0}"" ""{1}"")
", filePath, OutputPath);

            scrContent = scrContent.Replace(@"\", @"\\");
            string scrFilePath = Path.Combine(ScrPath, Guid.NewGuid().ToString() + ".scr");
            StreamWriter streamWriter = File.CreateText(scrFilePath);
            streamWriter.WriteLine(scrContent);
            streamWriter.Close();

            Process accoreconsole = new Process();
            accoreconsole.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            accoreconsole.StartInfo.FileName = AutoCADExePath;
            accoreconsole.StartInfo.Arguments = string.Format("/s \"{0}\"", scrFilePath);
            try
            {
                if (accoreconsole.Start())
                {
                    if (!accoreconsole.WaitForExit(720000))
                    {
                        accoreconsole.Kill();
                        Assert.Fail("The case takes so much time to run!");
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Exception with message: " + e.Message);
            }
        }

        private static string[] GetTestCases()
        {
            GeometryTestFrame.ScriptDir = @".\Scripts\Geometry\";
            GeometryTestFrame.BaseDir = @".\Scripts\GeometryBase\";
            List<string> result = new List<string>();
            var filepathes = Directory.EnumerateFiles(GeometryTestFrame.ScriptDir);
            foreach (var filepath in filepathes)
            {
                if (filepath.EndsWith(@".ds"))
                {
                    result.Add(Path.GetFileNameWithoutExtension(filepath));
                }
            }
            return result.ToArray();
        }

        private static string[] GetSampleTestCases()
        {
            GeometryTestFrame.ScriptDir = @".\Scripts\Geometry\";
            GeometryTestFrame.BaseDir = @".\Scripts\GeometryBase\";
            List<string> result = new List<string>();
#if ENABLE_SAMPLE_TEST_CASES
            var filepathes = Directory.EnumerateFiles(GeometryTestFrame.ScriptDir);
            foreach (var filepath in filepathes)
            {
                string[] parts = Path.GetFileNameWithoutExtension(filepath).Split('.');
                int id = 0;
                if (!int.TryParse(parts[0], out id))
                    continue;
                if (id < 900001 || id > 900241)
                    continue;
                if (filepath.EndsWith(@".ds"))
                {
                    result.Add(Path.GetFileNameWithoutExtension(filepath));
                }
            }
#endif
            return result.ToArray();
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
