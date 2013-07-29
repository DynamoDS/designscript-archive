using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class UtilitiesTest
    {
        [Test]
        public void SwapValidAssignment()
        {
            Assert.AreEqual("b = t1;", Utilities.SwapAssignment("t1  = b;", 0));
        }

        [Test]
        public void SwapMultilineValidAssignment()
        {
            string origin = "a = b + c; \n c = 10; \n t1 = b; \n sth = 123; \n \n";
            string expected = "a = b + c; \n c = 10; \nb = t1; \n sth = 123; \n \n";
            Assert.AreEqual(expected, Utilities.SwapAssignment(origin, 2));
        }

        [Test]
        public void DiscardMultipleLineValidAssignment()
        {
            int[] lineNumbers = { 0, 2, 4 };
            string origin = "a = b ; \n c = 10; \n t1 = b; \n sth = 123; \n 100=x;\n";
            string expected = "a ; \n c = 10; \n t1 ; \n sth = 123; \n 100;\n";
            Assert.AreEqual(expected, Utilities.DiscardTemporaryVariableAssignment(origin, lineNumbers));
        }

        [Test]
        public void DiscardSingleLineValidAssignment()
        {
            int[] lineNumbers = { 0 };
            string origin = "b = x;";
            string expected = "b ;";
            Assert.AreEqual(expected, Utilities.DiscardTemporaryVariableAssignment(origin, lineNumbers));
        }

        [Test]
        public void ExtractReplicationGuideFromText00()
        {
            List<int> guides = new List<int>();
            string text = " <   101   >   <   203   >   ";
            // string text = ""; null; "       ";
            // string text = " <   101   >   <  xyz >   <   203   >   ";
            bool result = FunctionNode.ExtractReplicationGuideFromText(guides, text);

            Assert.AreEqual(true, result);
            Assert.AreEqual(2, guides.Count);
            Assert.AreEqual(101, guides[0]);
            Assert.AreEqual(203, guides[1]);
        }

        [Test]
        public void ExtractReplicationGuideFromText01()
        {
            List<int> guides = new List<int>();
            guides.Add(1);
            guides.Add(4);
            guides.Add(3);
            string text = " <   101>  <  203   >  ";

            bool result = FunctionNode.ExtractReplicationGuideFromText(guides, text);

            Assert.AreEqual(true, result);
            Assert.AreEqual(2, guides.Count);
            Assert.AreEqual(101, guides[0]);
            Assert.AreEqual(203, guides[1]);
        }

        [Test]
        public void ExtractReplicationGuidesFromText02()
        {
            List<int> guides = null;
            string text = " <   101>  <  203   >  ";

            Assert.Throws<ArgumentNullException>(() =>
            {
                FunctionNode.ExtractReplicationGuideFromText(guides, text);
            });
        }

        [Test]
        public void ExtractReplicationGuidesFromText03()
        {
            List<int> guides = new List<int>();
            string text = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                FunctionNode.ExtractReplicationGuideFromText(guides, text);
            });
        }

        [Test]
        public void ExtractReplicationGuidesFromText04()
        {
            List<int> guides = new List<int>();
            string text = " <   101>  <  abc   >  <203> ";

            bool result = FunctionNode.ExtractReplicationGuideFromText(guides, text);

            Assert.AreEqual(false, result);
            Assert.AreEqual(0, guides.Count);
        }


        [Test]
        public void TestIsValidVariableName()
        {
            string validVariable1 = "Var1";
            string validVariable2 = "Var1_2323_21dasda";
            string invalidVariable1 = "2var";
            string invalidVariable2 = "Var1_2323_21da.sda";
            string invalidVariable3 = "+var2";

            Assert.AreEqual(true, Utilities.IsValidVariableName(validVariable1));
            Assert.AreEqual(true, Utilities.IsValidVariableName(validVariable2));

            Assert.AreEqual(false, Utilities.IsValidVariableName(invalidVariable1));
            Assert.AreEqual(false, Utilities.IsValidVariableName(invalidVariable2));
            Assert.AreEqual(false, Utilities.IsValidVariableName(invalidVariable3));
        }

        [Test]
        public void TestGetBackupFileName00()
        {
            uint graphId = 1234;
            string sessionName = Guid.NewGuid().ToString("D").ToLower();
            string actualName = Utilities.GetBackupFileName(sessionName, graphId, -1);

            string expectedName = string.Format(UiStrings.BackupFileNameFmt,
                sessionName, graphId.ToString(), "*"); // Test wildcard name.

            Assert.AreEqual(expectedName, actualName);
        }

        [Test]
        public void TestGetBackupFileName01()
        {
            int index = 5678;
            uint graphId = 1234;
            string sessionName = Guid.NewGuid().ToString("D").ToLower();
            string actualName = Utilities.GetBackupFileName(sessionName, graphId, index);

            string expectedName = string.Format(UiStrings.BackupFileNameFmt,
                sessionName, graphId.ToString(), index); // Test indexed name.

            Assert.AreEqual(expectedName, actualName);
        }

        [Test]
        public void TestGetBackupFileName02()
        {
            int index = 5678;
            string sessionName = Guid.NewGuid().ToString("D").ToLower();
            string actualName = Utilities.GetBackupFileName(sessionName, 0, index);

            string expectedName = string.Format(UiStrings.BackupFileNameFmt,
                sessionName, "*", index); // Test graph id being '0'.

            Assert.AreEqual(expectedName, actualName);
        }

        [Test]
        public void TestGetBackupFileName03()
        {
            int index = 5678;
            string sessionName = Guid.NewGuid().ToString("D").ToLower();
            string actualName = Utilities.GetBackupFileName(sessionName, uint.MaxValue, index);

            string expectedName = string.Format(UiStrings.BackupFileNameFmt,
                sessionName, "*", index); // Test graph id being 'uint.MaxValue'.

            Assert.AreEqual(expectedName, actualName);
        }

        [Test]
        public void TestGetBackupFileIndex00()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                int index = -1; uint graphId = uint.MaxValue;
                Utilities.GetBackupFileIndex(null, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex01()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                int index = -1; uint graphId = uint.MaxValue;
                Utilities.GetBackupFileIndex("", ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex02()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // With less than 8 hyphens.
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G.bin";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex03()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // With more than 8 hyphens.
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G-I-J.bin";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex04()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Without any period character.
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G-I";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex05()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Period goes before the hyphen character.
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G.I-bin";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex06()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // "G" is missing after session name and before "I".
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS--I.bin";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex07()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // "I" should have been an integer value.
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-1024-I.bin";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex08()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // "G" should have been an integer value.
                int index = -1; uint graphId = uint.MaxValue;
                string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G-4096.bin";
                Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            });
        }

        [Test]
        public void TestGetBackupFileIndex09()
        {
            // This test should pass.
            string name = "dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-1024-4096.bin";
            int index = -1; uint graphId = uint.MaxValue;
            Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            Assert.AreEqual(1024, graphId);
            Assert.AreEqual(4096, index);
        }

        [Test]
        public void TestGetBackupFileIndex10()
        {
            // This test should pass even a full path with both additional hyphen and dot is provided.
            string name = @"C:\settings.config\user-data\dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-1024-4096.bin";
            int index = -1; uint graphId = uint.MaxValue;
            Utilities.GetBackupFileIndex(name, ref graphId, ref index);
            Assert.AreEqual(1024, graphId);
            Assert.AreEqual(4096, index);
        }

        [Test]
        public void TestGetBackupFileFolder()
        {
            // The backup folder must end with a backslash.
            string backupFolder = Utilities.GetBackupFileFolder();
            Assert.AreEqual(true, backupFolder.EndsWith(@"\DesignScriptStudio\Backup\"));
        }

        [Test]
        public void TestIsLiteralValue()
        {
            string variableName = "a;";
            string complexVarName = "a[1][2].x[2];";
            string number = "1;";
            string complexNumber = "1.2;";
            string complexNumer2 = "1,2000.03;";
            string numberWith2dot = "1..2;";
            string numberWith2colon = "1,,2;";
            string numberStartWithDot = ".1;";
            string multipleLineCBN = "a=b+c;\n1;";

            Assert.AreEqual(false, Utilities.IsLiteralValue(variableName));
            Assert.AreEqual(false, Utilities.IsLiteralValue(complexVarName));
            Assert.AreEqual(true, Utilities.IsLiteralValue(number));
            Assert.AreEqual(true, Utilities.IsLiteralValue(complexNumber));
            Assert.AreEqual(true, Utilities.IsLiteralValue(complexNumer2));
            Assert.AreEqual(false, Utilities.IsLiteralValue(numberStartWithDot));
            Assert.AreEqual(false, Utilities.IsLiteralValue(numberWith2colon));
            Assert.AreEqual(false, Utilities.IsLiteralValue(numberWith2dot));
            Assert.AreEqual(false, Utilities.IsLiteralValue(multipleLineCBN));
        }
    }
}
