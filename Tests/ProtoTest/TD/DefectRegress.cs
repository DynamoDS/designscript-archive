using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoTestFx.TD;

namespace ProtoTest.TD
{
    class DefectRegress
    {

        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        string testCasePath = "..\\..\\..\\Scripts\\TD\\Regress\\";

        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new PInvokeModuleHelper());
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455643()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455643.ds");
            thisTest.Verify("areEqual1", true);
            thisTest.Verify("areEqual2", false);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455621()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455621.ds");
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455158()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455158.ds");

            thisTest.Verify("x3", 3);
            thisTest.Verify("y3", 4);

        }


        [Test]                
 public void Regress_1455618()
        {
            //Assert.Fail("1467188 - Sprint24 : rev 3170: REGRESSION : instantiating a class more than once with same argument is causing DS to go into infinite loop!");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455618.ds");

            thisTest.Verify("x", 1.0);
            thisTest.Verify("y", 1.0);
            thisTest.Verify("z", 1.0);
            thisTest.Verify("h", 0.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455584()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455584.ds");

            thisTest.Verify("multiply1", 40.0);
        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455729()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455729.ds");
            thisTest.Verify("a", 2);//Compilation test.
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455738()
        {
       
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455738.ds");
            thisTest.Verify("b", 8);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455276()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455276.ds");
            thisTest.Verify("dist", 100.0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454980()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454980.ds");
            thisTest.Verify("result", 9);
        }


        [Test]              
 public void Regress_1455568()
        {

            //Assert.Fail("1467188 - Sprint24 : rev 3170: REGRESSION : instantiating a class more than once with same argument is causing DS to go into infinite loop!");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455568.ds");
            thisTest.Verify("RX", 1.0);
            thisTest.Verify("RY", 1.0);
            thisTest.Verify("RZ", 1.0);
            thisTest.Verify("RH", 1.0);

        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1455291()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455291.ds");
            thisTest.Verify("a", 5);
            //This is more of a test for compilation. 
        }


        [Test]
    
 public void Regress_1454075()
        {
            string dscode = @"
                class Vector{
	constructor Vector() {}
}
v = Vector.Vector();";


            ExecutionMirror mirror = thisTest.RunScriptSource(dscode);
            //Compilation test. 
        }

        [Test]
       
 public void Regress_1454723()
        {
            string dscode = @"
def sqrt : double  (val : double )
{
    result = 0.0;
    result = [Imperative]
             {
                return = 10.0 * val;
             }
    return = result;
}
ten;
[Imperative]
{
    val = 10;
    ten = sqrt(val);
}
";


            ExecutionMirror mirror = thisTest.RunScriptSource(dscode);
            thisTest.Verify("ten", 100.0);
        }

        [Test]
     
 public void Regress_1457064()
        {
            string dscode = @"
def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr;
}

xCount =3;
dummy = 1;
rangeExpression = 0.0..(180*dummy)..#xCount;
result = Scale(rangeExpression, 2);
";


            ExecutionMirror mirror = thisTest.RunScriptSource(dscode);
            object[] expectedResult = { 0.0, 180.0, 360.0 };
            thisTest.Verify("result", expectedResult, 0);
        }


        [Test]
   
 public void Regress_1456921()
        {
            string dscode = @"
b = 10.0;
a = 0.0;
d = a..b..c;";

            // Assert.Fail("1456921 - Sprint 20: Rev 2088: (negative), null expected when using an undefined variable ranged expression"); 
            string errmsg = "DNL-1467454 Rev 4596 : Regression : CompilerInternalException coming from undefined variable used in range expression";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(dscode, errmsg);
            object expectedResultc = null;
            thisTest.Verify("d", expectedResultc);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454697()
        {
            String code =
            @"    def foo : double (array : double[])
    {
        return = 1.0 ;
    }
    
    arr = {1,2,3};
    arr2 = foo(arr);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Verification
            object arr2 = 1.0;
            thisTest.Verify("arr2", arr2, 0);

        }

        [Test]        
 public void Regress_1457179()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457179.ds");

            //Verification
            object result1 = 180.0;
            object result2 = 180.0;

            thisTest.Verify("result1", result1, 0);
            thisTest.Verify("result2", result2, 0);
        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458561 ()
        {

            Object[] t1 = new Object[] { 10, 20 };
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458561.ds");
            thisTest.Verify("t1", t1, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458785()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458785.ds");
            thisTest.Verify("a3", 2, 0);
          


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458785_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458785_2.ds");
            object[] expectedResult = new Object[] { 1 };
            thisTest.Verify("a1", expectedResult, 0);
            thisTest.Verify("a2", 3, 0);
        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458785_3()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458785_3.ds");
            object expectedResult = null;
            thisTest.Verify("y1", expectedResult, 0);
            thisTest.Verify("z1", 2, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458785_4()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458785_4.ds");

        }
           [Test]
        [Category ("SmokeTest")]
 public void Regress_1454692()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454692.ds");
            thisTest.Verify("x", 6, 0);
             
                thisTest.Verify("y", 3, 0);


        }
           [Test]
           [Category ("SmokeTest")]
 public void Regress_1454692_2()
           {

               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454692_2.ds");
               Object[] expectedresult = new Object[] { 0.0, 1.0, 2.0, 3.0 };
               thisTest.Verify("num", 4, 0);
               thisTest.Verify("arr", expectedresult, 0);

           }
           [Test]
           [Category ("SmokeTest")]
 public void Regress_1455935()
           {

               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455935.ds");
               thisTest.Verify("b", 1, 0);
               thisTest.Verify("c", 1, 0);
               thisTest.Verify("d", 3, 0);

           }
           [Test]
           [Category ("SmokeTest")]
 public void Regress_1457862()
           {
               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457862.ds");
               Object[] x1 = new Object[] { 1.0, 2.0, 3.0, 4.0 };
               Object[] a = new Object[] { 1.0, 2.0, 3.0, 4.0 };
               Object[] a1 = new Object[] { 1, 2, 3, 4 };
               Object[] a2 = new Object[] { 1, 2, 3, 4 };
               thisTest.Verify("a3", 1, 0);
               thisTest.Verify("a4", 3, 0);
           }

           [Test]
           [Category ("SmokeTest")]
 public void Regress_1457885()
           {

               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457885.ds");
               Object[] c1 = new Object[] {5};
              // Object[] c2 = new Object[] {0.2,0.3};
               thisTest.Verify("c",c1, 0);
//               thisTest.Verify("a",c2, 0);

           }
           [Test]          
 public void Regress_1454966()
           {

               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966.ds");
               //Assert.Fail("1460965 - Sprint 18 : Rev 1700 : Design Issue : Accessing properties from collection of class instances using the dot operator should yield a collection of the class properties");
               //thisTest.Verify("a1", new Object[] { new Object[] { 1, 1, 1 }, new Object[] { 2, 2, 2 }, new Object[] { 3, 3, 3 } });
               thisTest.Verify("a1", 1);
            

           }
        [Test]
           [Category ("SmokeTest")]
 public void Regress_1454966_2()
           {

               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_2.ds");
               thisTest.Verify("x", 3, 0);


           }
        [Test]
    
        [Category("Dot Op")]
 public void Regress_1454966_3()
        {

         //    Assert.Fail("1454966 - Sprint15: Rev 720 : [Geometry Porting] Assignment operation where the right had side is Class.Constructor.Property is not working"); 
            string errmsg = "DNL-1467177 sprint24: rev 3152 : REGRESSION : Replication should not be supported in Imperative scope";

            ExecutionMirror mirror = thisTest.VerifyRunScriptFile(testCasePath, "Regress_1454966_3.ds", errmsg);
            Object[] x = new Object[] { 1, 2, 3 };
            //Object[] a1 = new Object[] { 1, 1, 1 };
            Object[][] a1 = new Object[][] { new object[] { 1, 1, 1 }, new object[] { 2, 2, 2 }, new object[] { 3, 3, 3 } };

            thisTest.Verify("x", x);
            thisTest.Verify("a1", a1);
            //thisTest.Verify("a2", a2);
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 2);
            thisTest.Verify("t3", 3);
            thisTest.Verify("a3", 6);
        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454966_4()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_4.ds");
            Object[] x = new Object[] { 1.3,3.0,5.0 };
            object c = null;
            thisTest.Verify("getval", x, 0);

        }
        [Test]

 public void Regress_1454966_5()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_5.ds");
            Object[] x = new Object[] { 1.3, 3.0, 5.0 };

            //Assert.Fail("1454966 - Sprint15: Rev 720 : [Geometry Porting] Assignment operation where the right had side is Class.Constructor.Property is not working");
            
            thisTest.Verify("getval", x, 0);
            thisTest.Verify("getval2", 1.3, 0);
            thisTest.Verify("getval3", 5.0, 0);
            thisTest.Verify("getval4", 5.0, 0);
            thisTest.Verify("getval5", 5.0, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454966_6()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_6.ds");
            thisTest.Verify("c", 1.3, 0);
        }

        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454966_7()
        {
            Object[] x = new Object[] { 1.0, 2.0, 3.0 };
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_7.ds");
            thisTest.Verify("a1", x, 0);
        }

        [Test]
        public void Regress_1454966_8()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_8.ds");

            thisTest.Verify("t1", 2,0);
        }
        [Test]
        public void Regress_1454966_9()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_9.ds");
            thisTest.Verify("b2", 0, 0);
        }
        
        [Test]
        public void Regress_1454966_10()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454966_10.ds");
            thisTest.Verify("t1", 1, 0);
        }
        
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456895()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456895.ds");
            Object[] x = new Object[] { 3,3 };
            thisTest.Verify("d", x, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456895_2()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456895_2.ds");
            Object[] x = new Object[] { 3, 3 };
            thisTest.Verify("c", x, 0);
            thisTest.Verify("d", 3, 0);


        }
      
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456895_3()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456895_3.ds");
            Object[] arr = new Object[] { 0.0, 1.0, 2.0, 3.0 };
            thisTest.Verify("arr", arr, 0);
            thisTest.Verify("num", 4, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456713()
        {

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456713.ds");
            thisTest.Verify("a", 2.3, 0);
            thisTest.Verify("b", 6.9, 0);
            thisTest.Verify("c", 2.32, 0);
            thisTest.Verify("d", 6.96, 0);
            thisTest.Verify("e1", 0.31, 0);
            thisTest.Verify("f", 0.93, 0);
            thisTest.Verify("g", 1.1, 0);
            thisTest.Verify("h", 2.53, 0);
            thisTest.Verify("i", 0.99999, 0);
            thisTest.Verify("j", 1.99998, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454511()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454511.ds");
            thisTest.Verify("x", 0, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456758()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456758.ds");
            thisTest.Verify("a", -1, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1459175()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459175.ds");
            object expectedResultc = null;
            thisTest.Verify("a1", expectedResultc, 0);
            thisTest.Verify("a2", expectedResultc, 0);
            thisTest.Verify("a3", expectedResultc, 0);

        }
        [Test, Ignore]
        [Category ("Type System")]
 public void Regress_1459175_2()
        {
            Assert.Inconclusive();
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459175_2.ds");
            
            thisTest.Verify("test", 1, 0);
            thisTest.Verify("test2", 10, 0);
            

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1457903()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457903.ds");
            object expectedResultc = null;
            thisTest.Verify("a", expectedResultc, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458918_1()
        {
            // Assert.Fail("1467247 - Sprint25: rev 3448 : REGRESSION : Runtime exception thrown when setting a private property of a class instance");
            
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458918_1.ds");
            thisTest.Verify("a1", 3, 0);
            thisTest.Verify("a2", null, 0);
        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1458918_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458918_2.ds");

            thisTest.Verify("test3", 5, 0);
            thisTest.Verify("test5", 4, 0);
        }
        [Test]
        [Category ("Type System")]
 public void Regress_1454918_1()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454918_1.ds");
            thisTest.Verify("d", 2.5);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454918_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454918_2.ds");
            thisTest.Verify("d", 6, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454918_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454918_3.ds");
            thisTest.Verify("d", 5.0, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454918_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454918_4.ds");
            object c = null;
            thisTest.Verify("d", c, 0);


        }
        [Test]
        [Category("Type System")]
 public void Regress_1454918_5()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454918_5.ds");
            object c = null;
            thisTest.Verify("d", c, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1454918_6()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454918_6.ds");
         
            thisTest.Verify("d", 5.0, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611.ds");

            thisTest.Verify("y", 3, 0);


        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_2.ds");

            thisTest.Verify("numpts", 2, 0);


        }
         [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611_3()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_3.ds");
        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611_4()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_4.ds");
            thisTest.Verify("numpts", 5, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611_5()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_5.ds");
            thisTest.Verify("numpts", 1, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611_6()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_6.ds");
            object c = null;
            thisTest.Verify("numpts", c, 0);

        }
        [Test]
        [Category ("SmokeTest")]
 public void Regress_1456611_7()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_7.ds");
            object c = null;
            thisTest.Verify("numpts", c, 0);

        }
        [Test]
 public void Regress_1456611_8()
        {
            //Assert.Fail("Sub-recursion calls with auto promotion on jagged arrays is not working");

            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_8.ds");
            
            thisTest.Verify("t1", 0, 0);
            thisTest.Verify("t2", 10, 0);
            thisTest.Verify("t3", 5, 0);

        }
        [Test]
        [Category ("Type System")]
 public void Regress_1456611_9()
        {
            //Assert.Fail("DNL-1467208 Auto-upcasting of int -> int[] is not happening on function return");
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1456611_9.ds");
            thisTest.Verify("numpts", 0, 0);

        }
          [Test]
           [Category ("SmokeTest")]
 public void Regress_1459372()
           {
               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459372.ds");

               Object[] x = new Object[] { 2, 3, 2 };
               thisTest.Verify("collection", x, 0);

           }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1459512()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459512.ds");
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1459171_1()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459171_1.ds");

          }
          [Test]
          [Category("SmokeTest")]
          public void Regress_1459171_2()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459171_2.ds");
              object []e1 = {  new object[] {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12},
                               new object[] {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14},
                               new object[] {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16},
                               new object[] {8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18},
                               new object[] {10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}
                             };
              
              thisTest.Verify("e1", e1);
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458916()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458916.ds");
              

              
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458915()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458915.ds");

              thisTest.Verify("c1", 1, 0);
              thisTest.Verify("c2", 2, 0);

          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458915_2()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458915_2.ds");
              thisTest.Verify("t", 1, 0);
            


          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458915_3()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458915_3.ds");

              thisTest.Verify("x1", 5, 0);
              thisTest.Verify("y1", 5, 0);
              thisTest.Verify("y2", 5, 0);

          }
         
         
         [Test]
          [Category ("SmokeTest")]
 public void Regress_1459584()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584.ds");
              Object[] numpts = new object[] { 0, 10 };
              thisTest.Verify("test", 0, 0);
              thisTest.Verify("test2", 10, 0);

          }
         [Test]
         [Category ("Type System")]
 public void Regress_1459584_1()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_1.ds");
             thisTest.Verify("a", 0, 0);
             thisTest.Verify("b", 10, 0);
             thisTest.Verify("c1", 2, 0);

         }
         [Test]
         [Category ("Type System")]
 public void Regress_1459584_2()
         {
             //Assert.Fail("1467196 Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_2.ds");
             thisTest.Verify("a", 0, 0);
             thisTest.Verify("b", null, 0);


         }
         [Test]
         [Category ("Type System")]
 public void Regress_1459584_3()
         {
             //Assert.Fail("1467196 Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_3.ds");
          //   thisTest.Verify("a", 0, 0);
           //  thisTest.Verify("b", 10, 0);

         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1459584_4()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_4.ds");
             thisTest.Verify("numpts", 1, 0);

         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1459584_5()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_5.ds");
             
             thisTest.Verify("numpts", 1.0, 0);
         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1459584_6()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_6.ds");
             Object[] numpts = new object[] { 0, 10 };
             thisTest.Verify("test", 0, 0);
             thisTest.Verify("test2", 10, 0);

         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1459584_7()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_7.ds");
             object c = null;
             thisTest.Verify("numpts", c, 0);

         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1459584_8()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459584_8.ds");
             object c = null;
             thisTest.Verify("numpts", c, 0);
         }
       
          [Test]
         [Category ("SmokeTest")]
 public void Regress_1458475()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458475.ds");
              thisTest.Verify("b1", 2, 0);


          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458475_2()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458475_2.ds");
              thisTest.Verify("b1", 2, 0);


          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458187()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458187.ds");
              thisTest.Verify("a", 6.0, 0);
             thisTest.Verify("b", 6, 0);
             thisTest.Verify("c", 0, 0);


          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458187_2()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458187_2.ds");
              
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1458187_3()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1458187_3.ds");
              object c = null;
              thisTest.Verify("a", c, 0);
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1454926()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1454926.ds");
              object result = null;
              thisTest.Verify("result", result, 0);
              thisTest.Verify("result2", result, 0);
          }
          [Test]
          [Category("Escalate")]
          [Category ("SmokeTest")]
 public void Regress_1455283()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455283.ds");
              thisTest.Verify("x", 10.0, 0);
              thisTest.Verify("y", 20.0, 0);
              thisTest.Verify("z", 30.0, 0);
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1455283_1()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1455283_1.ds");
              thisTest.Verify("b1", 1, 0);
              thisTest.Verify("b2", 1.5, 0);
              thisTest.Verify("b3", 2, 0);
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1459900()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459900.ds");
              
              
          }
          [Test]
          [Category ("SmokeTest")]
 public void Regress_1459900_1()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459900_1.ds");


          }
         [Test]
          [Category ("SmokeTest")]
 public void Regress_1459762()
          {
              ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1459762.ds");
             object r2=null;
             thisTest.Verify("r2", r2, 0);

          }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1452951()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1452951.ds");
             thisTest.Verify("x", 9, 0);

         }
         [Test]
           [Category ("SmokeTest")]
 public void Regress_1457023()
           {
               ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457023.ds");
               Object[] arr = new object[] { 0.0, 1.0,2.0,3.0 };
               thisTest.Verify("arr", arr, 0);
               thisTest.Verify("num", 4, 0);
           }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1457023_1()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457023_1.ds");
             Object[] arr = new object[] { 0.0, 1.0, 2.0, 3.0 };
             thisTest.Verify("arr", arr, 0);
            
             thisTest.Verify("num", 4, 0);
         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1457023_2()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457023_2.ds");
            
             Object[] b2 = new object[] { 2.0, 3.0, 4.0 };
             thisTest.Verify("b2", b2, 0);

             
         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1457023_3()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457023_3.ds");

             thisTest.Verify("vec_len", 12.5, 0);

         }
         [Test]
         [Category ("SmokeTest")]
 public void Regress_1457023_4()
         {
             //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor "); 
             
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1457023_4.ds");

             thisTest.Verify("a1", 1);

         }

         [Test]
        [Category ("SmokeTest")]
 public void myTest()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "test.ds");
        }

        [Test]
        [Category("Type System")]
         public void Regress_1462308()
         {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1462308.ds");
            // Assert.Fail("1467119 Sprint24 : rev 2807 : Type conversion issue with char");

             thisTest.Verify("f", 102);
             thisTest.Verify("F", 70);

         }

        
        [Test]
        public void Regress_1467091()
        {
             ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1467091.ds");
             TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
             if (!core.Options.SuppressFunctionResolutionWarning)
             {
                 TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
             }
             thisTest.Verify("y1", null);
             thisTest.Verify("y2", null);
        }

        [Test]
        public void Regress_1467094_1()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1467094_1.ds");
            thisTest.Verify("x", null);
            thisTest.Verify("y", null);
            thisTest.Verify("z", 1);
        }

        [Test]
        public void Regress_1467094_2()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1467094_2.ds");
            thisTest.Verify("t4", null);
        }

        [Test]
        public void Regress_1467104()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1467104.ds");
            thisTest.Verify("aa", null);
        }

        [Test]
        public void Regress_1467107()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1467107.ds");
            thisTest.Verify("y", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kDereferencingNonPointer);
        }

        [Test]
        public void Regress_1467117()
        {
            ExecutionMirror mirror = thisTest.RunScriptFile(testCasePath, "Regress_1467117.ds");
            thisTest.Verify("a", 1);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void Regress_1467273()
        {
            string code = @"
def foo(x:var[]..[]) { return = 2; }
def foo(x:var[]) { return = 1; }
d = foo({1,2});
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 1);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void Regress_1467318()
        {
            string code = @"
class A {
x;
constructor A() {
x = {2, 3}; }
}
a = A.A();
t = a.x;
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2,3});
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void Regress_1467505_gc_issue()
        {
            string code = @"
class A
{
    def _Dispose()
    {
        Print(""A._Dispose()"");
    }
}

def foo()
{
    [Imperative]
    {
        a = A.A();
    }
}

foo();
";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //thisTest.Verify("t", new object[] { 2, 3 });
            thisTest.VerifyBuildWarningCount(0);
        }

    }
}



