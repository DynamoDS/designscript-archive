using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.CodeGen
{
    /// <summary>
    /// Stores the code range of all the functions defined in a class or a code block
    /// </summary>
    class FunctionRangeTable
    {
        /// <summary>
        /// The key stands for the function index
        /// The value is the code range of the function, file name, start line, start col, end line, end col
        /// </summary>
        public Dictionary<int, ProtoCore.CodeModel.CodeRange> RangeTable { get; private set; }

        public FunctionRangeTable()
        {
            RangeTable = new Dictionary<int, ProtoCore.CodeModel.CodeRange>();
        }

        public bool AddEntry(int functionIndex, int sline, int scol, int eline, int ecol, string src)
        {
            if (RangeTable.ContainsKey(functionIndex))
                return false;

            ProtoCore.CodeModel.CodeRange range = new ProtoCore.CodeModel.CodeRange()
            {
                StartInclusive = new ProtoCore.CodeModel.CodePoint
                {
                    LineNo = sline,
                    CharNo = scol,
                    SourceLocation = new ProtoCore.CodeModel.CodeFile { FilePath = src }
                },

                EndExclusive = new ProtoCore.CodeModel.CodePoint
                {
                    LineNo = eline,
                    CharNo = ecol,
                    SourceLocation = new ProtoCore.CodeModel.CodeFile { FilePath = src }
                }
            };

            RangeTable[functionIndex] = range;
            return true;
        }
    }

    /// <summary>
    /// Stores the code range of all the classes or code blocks 
    /// Also store the code range information for all the functions defined in one class or code block
    /// </summary>
    class BlockRangeTable
    {
        /// <summary>
        /// The key is the block or class index
        /// The value is the code range
        /// </summary>
        public List<KeyValuePair<int, ProtoCore.CodeModel.CodeRange>> RangeTable { get; set; }
        /// <summary>
        /// The key is the block or class index
        /// The value is the function range table which stores the range information for all the functions in the class or code block
        /// </summary>
        public Dictionary<int, FunctionRangeTable> FunctionTable { get; set; }

        public BlockRangeTable()
        {
            RangeTable = new List<KeyValuePair<int, ProtoCore.CodeModel.CodeRange>>();
            FunctionTable = new Dictionary<int, FunctionRangeTable>();
        }
        public bool AddRangeEntry(int blockId, int sline, int scol, int eline, int ecol, string src)
        {
            ProtoCore.CodeModel.CodeRange range = new ProtoCore.CodeModel.CodeRange()
            {
                StartInclusive = new ProtoCore.CodeModel.CodePoint
                {
                    LineNo = sline,
                    CharNo = scol,
                    SourceLocation = new ProtoCore.CodeModel.CodeFile { FilePath = src }
                },

                EndExclusive = new ProtoCore.CodeModel.CodePoint
                {
                    LineNo = eline,
                    CharNo = ecol,
                    SourceLocation = new ProtoCore.CodeModel.CodeFile { FilePath = src }
                }
            };

            KeyValuePair<int, ProtoCore.CodeModel.CodeRange> entry = 
                new KeyValuePair<int, ProtoCore.CodeModel.CodeRange>(blockId, range);

            RangeTable.Add(entry);
            if (!FunctionTable.ContainsKey(blockId))
                FunctionTable[blockId] = new FunctionRangeTable();
            return true;
        }
        public bool AddFunctionEntry(int blockID, int funcIndex, int sline, int scol, int eline, int ecol, string src)
        {
            return FunctionTable[blockID].AddEntry(funcIndex, sline, scol, eline, ecol, src);
        }
    }

    /// <summary>
    /// one this table is associated with one program, it stores all the code range information 
    /// for all the classes and code blocks 
    /// </summary>
    class CodeRangeTable
    {
        public BlockRangeTable CodeBlock { get; set; }
        public BlockRangeTable ClassBlock { get; set; }

        public CodeRangeTable()
        {
            CodeBlock = new BlockRangeTable();
            ClassBlock = new BlockRangeTable();
        }

        internal void AddCodeBlockRangeEntry(int blockId,
            int sline, int scol, int eline, int ecol, string src)
        {
            if (null == CoreCodeGen.AutoCompleteEngine)
                this.CodeBlock.AddRangeEntry(blockId, sline, scol, eline, ecol, src);
            else
            {
                CoreCodeGen.AutoCompleteEngine.AddScopeIdentifier(
                    blockId, -1, -1, sline, scol, eline, ecol, src);
            }
        }

        internal void AddClassBlockRangeEntry(int classId,
            int sline, int scol, int eline, int ecol, string src)
        {
            if (null == CoreCodeGen.AutoCompleteEngine)
                this.ClassBlock.AddRangeEntry(classId, sline, scol, eline, ecol, src);
            else
            {
                CoreCodeGen.AutoCompleteEngine.AddScopeIdentifier(
                    0, classId, -1, sline, scol, eline, ecol, src);
            }
        }

        internal void AddCodeBlockFunctionEntry(int blockId, int procId,
            int sline, int scol, int eline, int ecol, string src)
        {
            if (null == CoreCodeGen.AutoCompleteEngine)
                this.CodeBlock.AddFunctionEntry(blockId, procId, sline, scol, eline, ecol, src);
            else
            {
                CoreCodeGen.AutoCompleteEngine.AddScopeIdentifier(
                    blockId, -1, procId, sline, scol, eline, ecol, src);
            }
        }

        internal void AddClassBlockFunctionEntry(int classId, int procId,
            int sline, int scol, int eline, int ecol, string src)
        {
            if (null == CoreCodeGen.AutoCompleteEngine)
                this.ClassBlock.AddFunctionEntry(classId, procId, sline, scol, eline, ecol, src);
            else
            {
                CoreCodeGen.AutoCompleteEngine.AddScopeIdentifier(
                    0, classId, procId, sline, scol, eline, ecol, src);
            }
        }
    }

    /// <summary>
    /// Store the location of where the variabls are been defined 
    /// This includes the global variable for language blocks and local variables for functions, does not include the 
    /// class member functions 
    /// </summary>
    class IdentLocationTable
    {
        public Dictionary<ProtoCore.DSASM.SymbolNode, ProtoCore.CodeModel.CodePoint> Table { get; set; }
        public IdentLocationTable()
        {
            Table = new Dictionary<ProtoCore.DSASM.SymbolNode, ProtoCore.CodeModel.CodePoint>();
        }
        public void AddEntry(ProtoCore.DSASM.SymbolNode sn, int line, int col, string file)
        {
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint()
            {
                LineNo = line,
                CharNo = col,
                SourceLocation = new ProtoCore.CodeModel.CodeFile()
                {
                    FilePath = file
                }
            };

            Table[sn] = cp;
        }
    }

    /// <summary>
    /// Store the import hierarchy of the program
    /// </summary>
    class ImportTable
    {
        /// <summary>
        /// key is the file which is importing 
        /// value is the file which is been imported
        /// </summary>
        public List<KeyValuePair<string, string>> Table { get; set; }

        public ImportTable()
        {
            Table = new List<KeyValuePair<string, string>>();
        }

        public void AddEntry(string curFile, string importFile)
        {
            KeyValuePair<string, string> entry = new KeyValuePair<string, string>(curFile, importFile);
            Table.Add(entry);
        }

        /// <summary>
        /// tells if "importFile" is been imported by "curFile"
        /// </summary>
        /// <param name="curFile"></param>
        /// <param name="importFile"></param>
        /// <returns></returns>
        public bool IsImported(string curFile, string importFile)
        {
            IEnumerable<string> importedFiles = Table.Where(x => x.Key == curFile).Select(x => x.Value);

            if (importedFiles.Where(x => x == importFile).Count() > 0)
                return true;

            foreach (string file in importedFiles)
            {
                if (this.IsImported(file, importFile))
                    return true;
            }

            return false;

        }
    }
}
