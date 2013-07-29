using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace ProtoTest.TD.Associative
{
    class Function
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string filePath = "..\\..\\..\\Scripts\\TD\\Associative\\Function\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Category ("SmokeTest")]
 public void T001_Associative_Function_Simple()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T001_Associative_Function_Simple.ds");

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 11);
            
        }

        [Test]
        //Function does not accept single line function / Direct Assignment
        [Category ("SmokeTest")]
 public void T002_Associative_Function_SinglelineFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T002_Associative_Function_SinglelineFunction.ds");

            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 10);
       

        }

        [Test]
        [Category ("SmokeTest")]
 public void T003_Associative_Function_MultilineFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T003_Associative_Function_MultilineFunction.ds");

            
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("d").Payload) == 0);

        }


        [Test]        
        [Category ("SmokeTest")]
 public void T004_Associative_Function_SpecifyReturnType()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T004_Associative_Function_SpecifyReturnType.ds");
            thisTest.Verify("d", 0.33333333333333333);
        }


        [Test]
        [Category("Type System")]
        public void T005_Associative_Function_SpecifyArgumentType()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T005_Associative_Function_SpecifyArgumentType.ds");

            thisTest.Verify("result", 2);

        }

        [Test]
        //Function takes null as argument
        [Category ("SmokeTest")]
 public void T006_Associative_Function_PassingNullAsArgument()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T006_Associative_Function_PassingNullAsArgument.ds");


            //Assert.IsTrue((Double)mirror.GetValue("d").Payload == 0);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T007_Associative_Function_NestedFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T007_Associative_Function_NestedFunction.ds");


            Assert.IsTrue((Double)mirror.GetValue("result").Payload == 2.1);

        }


        [Test]
        //Function does not work if the argument variable is declared before function declaration
        [Category ("SmokeTest")]
 public void T008_Associative_Function_DeclareVariableBeforeFunctionDeclaration()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T008_Associative_Function_DeclareVariableBeforeFunctionDeclaration.ds");

            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 11);

        }

        [Test]
        [Category ("SmokeTest")]
 public void T009_Associative_Function_DeclareVariableInsideFunction()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T009_Associative_Function_DeclareVariableInsideFunction.ds");

            //Verifiction should be done after collection is ready. 

        }


        [Test]
        [Category ("SmokeTest")]
 public void T010_Associative_Function_PassAndReturnBooleanValue()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T010_Associative_Function_PassAndReturnBooleanValue.ds");

            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("result1").Payload) == false);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("result2").Payload));

        }

        [Test]
        [Category ("SmokeTest")]
 public void T011_Associative_Function_FunctionWithoutArgument()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T011_Associative_Function_FunctionWithoutArgument.ds");

            Assert.IsTrue((Int64)mirror.GetValue("result1").Payload == 5);
        }

        [Test]
        [Category ("SmokeTest")]
 public void T012_Associative_Function_MultipleFunctions()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T012_Associative_Function_MultipleFunctions.ds");

            Assert.IsTrue((Int64)mirror.GetValue("result1").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("result2").Payload == 6);

        }


        [Test]
        [Category ("SmokeTest")]
 public void T013_Associative_Function_FunctionWithSameName_Negative()
        {
            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T013_Associative_Function_FunctionWithSameName_Negative.ds");
        }


        [Test]
        //Should return compilation error if a variable has the same name as a function?
        [Category ("SmokeTest")]
 public void T014_Associative_Function_DuplicateVariableAndFunctionName_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T014_Associative_Function_DuplicateVariableAndFunctionName_Negative.ds");

            });

        }

        [Test]
        //Incorrect error message when the argument number is not matching with function declaration. 
        [Category ("SmokeTest")]
 public void T015_Associative_Function_UnmatchFunctionArgument_Negative()
        {
           
                ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T015_Associative_Function_UnmatchFunctionArgument_Negative.ds");

      

        }

        [Test]
        [Category ("SmokeTest")]
 public void T016_Associative_Function_ModifyArgumentInsideFunctionDoesNotAffectItsValue()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T016_Associative_Function_ModifyArgumentInsideFunctionDoesNotAffectItsValue.ds");


            Assert.IsTrue((Int64)mirror.GetValue("input").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("originalInput").Payload == 3);
        }

        [Test]
        //Calling a function before its declaration causes compilation failure
        [Category ("SmokeTest")]
 public void T017_Associative_Function_CallingAFunctionBeforeItsDeclaration()
        {

            ExecutionMirror mirror = thisTest.RunScript(@"..\\..\\..\\Scripts\\TD\\Associative\\Function\\T017_Associative_Function_CallingAFunctionBeforeItsDeclaration.ds");


            Assert.IsTrue((Int64)mirror.GetValue("input").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Z001_Associative_Function_Regress_1454696()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Z001_Associative_Function_Regress_1454696.ds");

            thisTest.Verify("arr2", 1.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void Z002_Defect_1461399()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Z002_Defect_1461399.ds");
            Object[] v1 = new Object[] { null, null, null };

            thisTest.Verify("test", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void Z002_Defect_1461399_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Z002_Defect_1461399_2.ds");
            Object[] v1 = new Object[] { null, null, null };

            thisTest.Verify("test", v1);

        }

        [Test]
        [Category ("SmokeTest")]
 public void Z003_Defect_1456728()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(filePath, "Z003_Defect_1456728.ds");

            Object[] v1 = {null, null};

            thisTest.Verify("a", v1);
        }

        
   

    }
}
