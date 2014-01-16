using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.NameSpace
{
    /// <summary>
    /// FullyQualifiedSymbolName class
    /// </summary>
    class Symbol
    {
        private string[] _namespaces;
        private string _symbolname;
        
        private static string[] GetNameSpaces(string name, out string symbolname)
        {
            var names = name.Split('.');
            int size = names.Length;
            symbolname = names[size-1];
            return names;
        }

        /// <summary>
        /// Constructs a FullyQualifiedSymbolName with the given fullname.
        /// </summary>
        /// <param name="fullname">fullname for the symbol including namespaces.
        /// </param>
        public Symbol(string fullname)
        {
            FullName = fullname;
            _namespaces = GetNameSpaces(fullname, out _symbolname);
        }

        /// <summary>
        /// Gets fully qualified symbol name.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets symbol name
        /// </summary>
        public string Name { get { return _symbolname; } }

        /// <summary>
        /// Gets symbol id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Checks if all of the namespace prefixes specified in the given 
        /// partially qualified name appear in this namespace in the same order.
        /// For Example:
        /// A full namespace "Com.Autodesk.Designscript.ProtoGeometry.Point" 
        /// will match all of the following partial namespaces
        /// Com.Autodesk.Designscript.ProtoGeometry.Point
        /// Point
        /// DesignScript.Point
        /// ProtoGeometry.Point
        /// Autodesk.DesignScript.Point
        /// whereas it won't match Com.DesignScript.Autodesk.Point
        /// </summary>
        /// <param name="partialname">Partially qualified symbol name</param>
        /// <returns>returns true if partial name matches this</returns>
        public bool Match(string partialname)
        {
            string symbol;
            var given = GetNameSpaces(partialname, out symbol);

            //Match the symbold name first
            if (!this.Name.Equals(symbol))
                return false;

            int index = 0;
            for (int i = 0; i < _namespaces.Length; ++i)
            {
                if (_namespaces[i].Equals(given[index]))
                    ++index;
            }
            return index == given.Length;
        }

        /// <summary>
        /// Checks equality based on FullName
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Symbol symbol = obj as Symbol;
            if (null == symbol)
                return false;

            return this.FullName.Equals(symbol.FullName);
        }

        /// <summary>
        /// Gets hascode based on FullName
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.FullName.GetHashCode();
        }
    }

    /// <summary>
    /// SymbolTable class
    /// </summary>
    class SymbolTable
    {
        private Dictionary<string, HashSet<Symbol>> _symbolTable;
        public SymbolTable()
        {
            _symbolTable = new Dictionary<string, HashSet<Symbol>>();
        }

        /// <summary>
        /// Adds the given symbol to this symbol table
        /// </summary>
        /// <param name="fullname">Fully qualified name for the symbol</param>
        /// <returns>True if symbol is added successfully, false if the symbol was 
        /// already present in the table.</returns>
        public bool AddSymbol(string fullname)
        {
            return AddSymbol(new Symbol(fullname));
        }

        /// <summary>
        /// Adds the given symbol to this symbol table
        /// </summary>
        /// <param name="qualifiedSymbol">FullyQualifiedSymbolName</param>
        /// <returns>True if symbol is added successfully, false if the symbol was 
        /// already present in the table.</returns>
        public bool AddSymbol(Symbol qualifiedSymbol)
        {
            string symbolName = qualifiedSymbol.Name;

            HashSet<Symbol> container = null;
            if (!_symbolTable.TryGetValue(symbolName, out container))
            {
                container = new HashSet<Symbol>();
                _symbolTable.Add(symbolName, container);
            }
            return container.Add(qualifiedSymbol);
        }

        /// <summary>
        /// Gets all matching symbols for the given partially qualified symbol.
        /// </summary>
        /// <param name="partialName">Partially qualified symbol</param>
        /// <returns>An array of all matched symbols</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public Symbol[] GetMatchingSymbols(string partialName)
        {
            string symbolName = partialName.Split('.').Last();
            HashSet<Symbol> container = null;
            if (!_symbolTable.TryGetValue(symbolName, out container))
                throw new System.Collections.Generic.KeyNotFoundException(string.Format("Failed to get unique matching symbol for {0}.", partialName));

            return container.Where((Symbol sym) => sym.Match(partialName)).ToArray();
        }

        /// <summary>
        /// Returns fully qualified name for the given partial name if it 
        /// resolves to a unique symbol.
        /// </summary>
        /// <param name="partialName">partial symbol name</param>
        /// <returns>Fully qualified name.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public string GetFullyQualifiedName(string partialName)
        {
            var symbols = GetMatchingSymbols(partialName);
            if (symbols == null || symbols.Length != 1)
                throw new System.Collections.Generic.KeyNotFoundException(string.Format("Failed to get unique matching symbol for {0}.", partialName));

            return symbols.First().FullName;
        }

        /// <summary>
        /// Gets total symbol count in the table
        /// </summary>
        /// <returns>Symbol count</returns>
        public int GetSymbolCount()
        {
            int count = 0;
            foreach (var item in _symbolTable)
            {
                count += item.Value.Count;
            }
            return count;
        }
    }
}
