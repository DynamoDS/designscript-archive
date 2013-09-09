//#define __RUN_TEST_FILE     

using System;
using ProtoCore;
using ProtoScript.Runners;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using System.IO;

namespace ProtoTestExe
{
    class Program
    {
        static void Run(string filename, bool verbose)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Cannot find file " + filename);
                return;
            }
            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            core.Options.DumpByteCode = verbose;
            core.Options.Verbose = verbose;
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
            ExecutionMirror mirror = runner.LoadAndExecute(filename, core);
        }

        static void DevRun()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();


            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
#if DEBUG
            core.Options.DumpByteCode = true;
            core.Options.Verbose = true;
#else
            core.Options.DumpByteCode = false;
            core.Options.Verbose = false;
#endif
            
#if __RUN_TEST_FILE
            
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
            ExecutionMirror mirror = runner.LoadAndExecute(@"c:\jun\AutodeskResearch\git\designscript\Scripts\jun.ds", core);
          
#else
            /*
            var runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            var fsr = new ProtoScript.Runners.DebugRunner(core);
            fsr.LoadAndPreStart(@"c:\project\designscript\test.ds", runnerConfig);
            fsr.Run();
            */


            
            //ProtoTest.TD.OtherMiscTests.MiscTest test = new ProtoTest.TD.OtherMiscTests.MiscTest();
            //test.TestDynamicSetValueAfterExecution();
            


            //ProtoTest.DebugTests.RunEqualityTests test = new ProtoTest.DebugTests.RunEqualityTests();
            //test.DebugTest_Jun_ReplicationGuide();

            //ProtoTest.Associative.MicroFeatureTests test = new ProtoTest.Associative.MicroFeatureTests();
            //test.Setup();
            //test.TestContextInject01();


            ProtoFFITests.ContextInjectionTests test = new ProtoFFITests.ContextInjectionTests();
            test.Setup();
            test.TestContextInjectDummy();



            //ProtoTest.Associative.MethodsFocusTeam test = new ProtoTest.Associative.MethodsFocusTeam();
            //test.Setup();
            //test.T014_DotOp_UserDefinedClass_03();
            
#endif

            long ms = sw.ElapsedMilliseconds;
            sw.Stop();
            Console.WriteLine(ms);
            Console.ReadLine();
        }


        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                Run(args[0], false);
            }
            else
            {
                DevRun();
            }
        }
    }
}
