namespace ProtoCore
{
    namespace Mirror
    {
        public static class Reflection
        {

            /// <summary>
            ///  Returns a runtime mirror that can be reflected upon
            /// </summary>
            /// <returns></returns>
            public static RuntimeMirror Reflect(string varname, int blockDecl, ProtoCore.Core core)
            {
                return new RuntimeMirror(varname, blockDecl, core);
            }

            
            /// <summary>
            ///  Returns class mirror that can be reflected upon
            ///  The ClassMirror is intended to be used at statically at compile time
            /// </summary>
            /// <returns></returns>
            //public static ClassMirror Reflect(string className, ProtoCore.Core core)
            //{
            //    return new ClassMirror(className, core);
            //}

            public static LibraryMirror Reflect(string assemblyName, ProtoLanguage.CompileStateTracker compileState)
            {
                return new LibraryMirror(assemblyName, compileState);
            }
        }
    }
}
