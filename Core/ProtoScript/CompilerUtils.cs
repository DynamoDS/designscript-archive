

using System;
using ProtoCore;
using ProtoCore.Utils;
using System.Collections.Generic;

namespace ProtoScript
{
    public static class CompilerUtils
    {
        public static ProtoLanguage.CompileStateTracker BuildDefaultCompilerState(Dictionary<string, Object> context = null)
        {
            ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();

            ProtoLanguage.CompileStateTracker compileState = new ProtoLanguage.CompileStateTracker(options);

            compileState.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive());
            compileState.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive());

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

            ProtoLanguage.CompileStateTracker compileState = new ProtoLanguage.CompileStateTracker(options);

            compileState.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive());
            compileState.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive());

            if (null != context)
            {
                compileState.AddContextData(context);
            }

            return compileState;
        }
    }
}
