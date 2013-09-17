

using System;
using ProtoCore;
using ProtoCore.Utils;
using System.Collections.Generic;

namespace ProtoScript
{
    public static class CompilerUtils
    {
        private static ProtoLanguage.CompileStateTracker BuildCompilerState(ProtoLanguage.CompileOptions options)
        {
            ProtoLanguage.CompileStateTracker compileState = new ProtoLanguage.CompileStateTracker(options);
            compileState.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive());
            compileState.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive());
            return compileState;
        }

        public static ProtoLanguage.CompileStateTracker BuildDefaultCompilerState(bool isDeltaExecution = false, Dictionary<string, Object> context = null)
        {
            if (isDeltaExecution)
            {
                return BuildLiveRunnerCompilerState(context);
            }

            ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();
            ProtoLanguage.CompileStateTracker compileState = BuildCompilerState(options);
            if (null != context)
            {
                compileState.AddContextData(context);
            }
            return compileState;
        }

        public static ProtoLanguage.CompileStateTracker BuildLiveRunnerCompilerState(Dictionary<string, Object> context = null)
        {
            ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();
            options.IsDeltaExecution = true;
            ProtoLanguage.CompileStateTracker compileState = BuildCompilerState(options);
            if (null != context)
            {
                compileState.AddContextData(context);
            }
            return compileState;
        }

        public static ProtoLanguage.CompileStateTracker BuildDebuggertCompilerState(Dictionary<string, Object> context = null)
        {
            ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();
            options.IDEDebugMode = true;
            ProtoLanguage.CompileStateTracker compileState = BuildCompilerState(options);
            if (null != context)
            {
                compileState.AddContextData(context);
            }
            return compileState;
        }
    }
}
