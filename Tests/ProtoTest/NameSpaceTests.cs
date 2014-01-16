using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.NameSpace;

namespace ProtoTest
{
    [TestFixture]
    public class NameSpaceTests
    {
        [Test]
        public void SymbolName()
        {
            Symbol symbol = new Symbol("Com.Autodesk.Designscript.ProtoGeometry.Point");
            Assert.AreEqual("Point", symbol.Name);
            Assert.AreEqual("Com.Autodesk.Designscript.ProtoGeometry.Point", symbol.FullName);
        }

        [Test]
        public void NameMatching()
        {
            Symbol symbol = new Symbol("Com.Autodesk.Designscript.ProtoGeometry.Point");
            Assert.IsTrue(symbol.Match("Com.Autodesk.Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Match("ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Match("Designscript.Point"));
            Assert.IsTrue(symbol.Match("Autodesk.Point"));
            Assert.IsTrue(symbol.Match("Com.Point"));
            Assert.IsTrue(symbol.Match("Com.Autodesk.Point"));
            Assert.IsTrue(symbol.Match("Com.Designscript.Point"));
            Assert.IsTrue(symbol.Match("Com.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Match("Autodesk.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Match("Autodesk.Designscript.Point"));
            Assert.IsTrue(symbol.Match("Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Match("Point"));
            Assert.IsFalse(symbol.Match("Autodesk.Com.Designscript.Point"));
            Assert.IsFalse(symbol.Match("Com.ProtoGeometry.Autodesk.Point"));
            Assert.IsFalse(symbol.Match("Com.Designscript.Autodesk.Point"));
            Assert.IsFalse(symbol.Match("Autodesk"));
        }

        [Test]
        public void AddSymbols()
        {
            SymbolTable table = new SymbolTable();
            Assert.IsTrue(table.AddSymbol("Com.Autodesk.Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(table.AddSymbol("Autodesk.Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(table.AddSymbol("Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(table.AddSymbol("ProtoGeometry.Point"));
            Assert.IsTrue(table.AddSymbol("Designscript.Point"));
            Assert.IsTrue(table.AddSymbol("Com.Autodesk.Designscript.ProtoGeometry.Vector"));
            Assert.AreEqual(6, table.GetSymbolCount());
        }

        [Test]
        public void GetSymbol()
        {
            SymbolTable table = new SymbolTable();
            table.AddSymbol("Com.Autodesk.Designscript.ProtoGeometry.Point");
            var symbol = table.GetMatchingSymbols("Com.Autodesk.Point").First();
            Assert.AreEqual("Point", symbol.Name);
            Assert.AreEqual("Com.Autodesk.Designscript.ProtoGeometry.Point", symbol.FullName);
            Assert.AreEqual(symbol.FullName, table.GetFullyQualifiedName("Point"));
            Assert.AreEqual(symbol.FullName, table.GetFullyQualifiedName("Autodesk.ProtoGeometry.Point"));
            Assert.AreEqual(symbol.FullName, table.GetFullyQualifiedName("Designscript.ProtoGeometry.Point"));
            Assert.IsEmpty(table.GetMatchingSymbols("Com.Designscript.Autodesk.Point"));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(()=>table.GetMatchingSymbols("Com.Autodesk.Designscript.ProtoGeometry"));
        }

        [Test]
        public void GetSymbolMultipleInput()
        {
            SymbolTable table = new SymbolTable();
            table.AddSymbol("Autodesk.ProtoGeometry.Point");
            table.AddSymbol("Autodesk.Designscript.Point");
            table.AddSymbol("Com.Autodesk.Point");
            Assert.AreEqual(3, table.GetSymbolCount());
            Assert.AreEqual("Com.Autodesk.Point", table.GetFullyQualifiedName("Com.Point"));
            Assert.AreEqual("Autodesk.ProtoGeometry.Point", table.GetFullyQualifiedName("ProtoGeometry.Point"));
            Assert.AreEqual("Autodesk.Designscript.Point", table.GetFullyQualifiedName("Designscript.Point"));

            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(()=>table.GetFullyQualifiedName("Point"));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => table.GetFullyQualifiedName("Autodesk.Point"));
        }
    }
}
