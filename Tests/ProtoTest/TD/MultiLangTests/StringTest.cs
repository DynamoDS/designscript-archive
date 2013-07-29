using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoScript.Runners;
using ProtoTest.TD;
using ProtoTestFx.TD;

namespace ProtoTest.TD.MultiLangTests
{
    class StringTest
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\StringTest\\";
        ProtoScript.Config.RunConfiguration runnerConfig;
        ProtoScript.Runners.DebugRunner fsr;


        [SetUp]
        public void Setup()
        {
            // Specify some of the requirements of IDE.
            ProtoCore.Options options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new ProtoScript.Runners.DebugRunner(core);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_1.ds");

            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_2.ds");

            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_3.ds");

            TestFrameWork.Verify(mirror, "result", false, 0);

        }
        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_4.ds");

            TestFrameWork.Verify(mirror, "result", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_5.ds");

            TestFrameWork.Verify(mirror, "result", false, 0);
        }
        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_6()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_6.ds");

            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_7()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T01_String_IfElse_7.ds");

            TestFrameWork.Verify(mirror, "result", true, 0);
        }

        [Test]
        public void T02_String_Not()
        {
            string errmsg = "1467170 - Sprint 25 - Rev 3142 it must skip the else loop if the conditiona cannot be evaluate to bool it must be skip both if and else";
            thisTest.VerifyRunScriptFile(testPath, "T02_String_Not.ds", errmsg);
            Object v1 = null;
            thisTest.Verify("result", v1, 0);
        }
        [Test]
        [Category("SmokeTest")]
        public void T03_Defect_UndefinedType()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T03_Defect_UndefinedType.ds");
            object v1 = null;
            TestFrameWork.Verify(mirror, "r1", v1, 0);
            TestFrameWork.Verify(mirror, "r2", v1, 0);
            TestFrameWork.Verify(mirror, "c", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Defect_1467100_String()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T04_Defect_1467100_String.ds");
            thisTest.Verify("x", "hello");

        }
        [Test]
        [Category("SmokeTest")]
        public void T04_Defect_1454320_String()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T04_Defect_1454320_String.ds");
            thisTest.Verify("str", "hello world!");
        }

        [Test]
     
        public void T05_String_ForLoop()
        {
            string errmsg = "1467197 - Sprint 25 - Rev 3211 - when using dynamic array(without init as an empty array) within a for loop in imperative block, it returns null ";
            thisTest.VerifyRunScriptFile(testPath, "T05_String_ForLoop.ds", errmsg);
            Object[] v1 = new Object[] { "hello", "world" };
            thisTest.Verify("r", v1, 0);
            
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_String_Class()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T06_String_Class.ds");
            
            thisTest.Verify("str1", "a");
            thisTest.Verify("str2", "foo");
            thisTest.Verify("str3", "foob");
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_String_Replication()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T07_String_Replication.ds");
            Object[] v1 = new Object[] {"ab","ac","ad"};
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_String_Replication_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T07_String_Replication_1.ds");
            Object[] v1 = new Object[] { "ab" };
            
            thisTest.Verify("str", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_String_Replication_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T07_String_Replication_2.ds");
            Object[] v1 = new Object[] { "ab" };
            Object[] v2 = new Object[] { "ac" };
            Object[] v3 = new Object[] {v1,v2};
            thisTest.Verify("str", v3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_String_Inline()
        {

            string errmsg = "1467248 - Sprint25 : rev 3452 : comparison with null should result to false in conditional statements";
            thisTest.VerifyRunScriptFile(testPath, "T08_String_Inline.ds", errmsg);
            Object[] v1 = new Object[] { "hello", "world" };
            thisTest.Verify("r", "b");
            thisTest.Verify("r1", "!Equal");
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_String_Inline_2()
        {

            string errmsg = "1467248 - Sprint25 : rev 3452 : comparison with null should result to false in conditional statements";
            thisTest.VerifyRunScriptFile(testPath, "T08_String_Inline_2.ds", errmsg);
            Object[] v1 = new Object[] { "hello", "world" };
            thisTest.Verify("r", "a");
            thisTest.Verify("r1", "Equal");
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_String_DynamicArr()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T09_String_DynamicArr.ds");
           Object v2 = null;
            Object[] v1 = new Object[] { v2, "11!!", "2!!", v2, v2, "whileLoop!!", "whileLoop!!", v2, v2, " - 2!!", "10!!" };
            thisTest.Verify("r", v1);
       
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_String_ModifierStack()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T10_String_ModifierStack.ds");
            //Assert.Fail("1467201 - Sprint 25 - rev 3239:Replication doesn't work in modifier stack");
            Object[] v1 = new Object[]{"a12","a13"};
            thisTest.Verify("a2", v1);
            thisTest.Verify("a", "b");
            thisTest.Verify("r", "b");

        }


        [Test]
        public void TV1467201_Replicate_ModifierStack_1()
        {
            String code =
                @"

                a =
                {
                    1;
                    + 1 => a1;
                    + { ""2"", ""3"" } => a2;
                    4 => b;
                }

                r = a;
    
                ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { "22", "23" };
            thisTest.Verify("a1",2);
            thisTest.Verify("a2", v1);
            thisTest.Verify("r", 4);

        }

        [Test]
        public void TV1467201_Replicate_ModifierStack_2()
        {
            String code =
                @"

                a =
                {
                    1;
                    + 1 => a1;
                    + { 10, -20 } => a2;
                100;
                }

                r = a;
    
                ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 12, -18 };
            thisTest.Verify("a1", 2);
            thisTest.Verify("a2", v1);
            thisTest.Verify("r",100 );

        }

        [Test]
        public void TV1467201_Replicate_ModifierStack_3()
        {
            String code =
                @"

                a =
                {
                    1;
                    + 1 => a1;
                    + { 10, -20 } => a2;
                    +{""m"",""n"",""o""} => a3;
                }

                r = a;
    
                ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 12, -18 };
            Object[] v2 = new Object[] { "12m", "-18n" };
            thisTest.Verify("a1", 2);
            thisTest.Verify("a2", v1);
            thisTest.Verify("a3", v2);
            thisTest.Verify("r", v2);

        }

        [Test]
        public void TV1467201_Replicate_ModifierStack_4()
        {
            String code =
                @"

                a =
                {
                    1;
                    + 1 => a1;
                    + { 10, -20 } => a2;
                    +{""m"",""n"",""o""} => a3;
                    + {} =>a4;
                }

                r = a;
    
                ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 12, -18 };
            Object[] v2 = new Object[] { "12m", "-18n" };
            Object[] v3 = new Object[] {};
            thisTest.Verify("a1", 2);
            thisTest.Verify("a2", v1);
            thisTest.Verify("a3", v2);
            thisTest.Verify("a4", v3);
            thisTest.Verify("r", v3);

        }

        [Test]
        public void TV1467201_Replicate_ModifierStack_5()
        {
            String code =
                @"

                a =
                {
                    1;
                    + 1 => a1;
                    + { 10, -20 } => a2;
                    +{""m"",""n"",""o""} => a3;
                     {{1,2},{3,4}} =>a4;
                      + {{10},{20}} => a5;
                }
                r = a;
    
                ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 12, -18 };
            Object[] v2 = new Object[] { "12m", "-18n" };
            Object[] v3 = new Object[] { 11};
            Object[] v4 = new Object[] { 23 };
            Object[] v5 = new Object[] { v3, v4 };

            thisTest.Verify("a1", 2);
            thisTest.Verify("a2", v1);
            thisTest.Verify("a3", v2);
            thisTest.Verify("a5", v5);
            thisTest.Verify("r", v5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T11_String_Imperative()
        {
            //Assert.Fail("");
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "T11_String_Imperative.ds");

            Object v1 = null;
            
            thisTest.Verify("a", "a1");
            thisTest.Verify("b", "b1");
            thisTest.Verify("c", "b1");
            thisTest.Verify("m", v1);
            thisTest.Verify("n", v1);

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringInt()
        {
            String code =
                @"

                a = ""["" + ToString(1)+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1]");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringDouble()
        {
            String code =
                @"

                a = ""["" + 1.1+""]"";
    
                ";
               thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1.100000]");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringString()
        {
            String code =
                @"

                a = ""["" + ""1.0""+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1.0]");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringChar()
        {
            String code =
                @"

                a = ""["" + '1'+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1]");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringBool()
        {
            String code =
                @"

                a = ""["" + true+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[true]");

        }


        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringNull()
        {
            String code =
                @"

                a = ""["" + null +""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467263 - Concatenating a string with an integer throws method resolution error");
            thisTest.Verify("a", "[null]");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringPointer_1()
        {
            String code =
                @"

                class A {}
                a  = A.A();
                b = ""a"" + a;
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", "aA{}");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringPointer_2()
        {
            String code =
                @"

                class A {

                    fx:int = 1;
                }
                a  = A.A();
                b = ""a"" + a;
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", "aA{fx = 1}");

        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringArr()
        {
            String code =
                @"

                class A {

                    fx:int = 1;
                }
                a  = A.A();
                arr1 = {1,2};
                arr2 = {1,a};
                b1 = ""a"" + ToString(arr1);
                b2 = ""a"" + ToString(arr2);
                ";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467263 - Concatenating a string with an integer throws method resolution error");
            thisTest.Verify("b1", "a{1,2}");
            thisTest.Verify("b2", "a{1,A{fx = 1}}");

        }




        [Test]

        public void Test()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testPath, "Test.ds");

            
        }

    }

}