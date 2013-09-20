

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Operator = ProtoCore.DSASM.Operator;
using UnaryOperator = ProtoCore.DSASM.UnaryOperator;
using RangeStepOperator = ProtoCore.DSASM.RangeStepOperator;

namespace DesignScript.Parser {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _char = 5;
	public const int _period = 6;
	public const int _openbracket = 7;
	public const int _closebracket = 8;
	public const int _openparen = 9;
	public const int _closeparen = 10;
	public const int _not = 11;
	public const int _neg = 12;
	public const int _pipe = 13;
	public const int _lessthan = 14;
	public const int _greaterthan = 15;
	public const int _lessequal = 16;
	public const int _greaterequal = 17;
	public const int _equal = 18;
	public const int _notequal = 19;
	public const int _endline = 20;
	public const int _rangeop = 21;
	public const int _kw_native = 22;
	public const int _kw_class = 23;
	public const int _kw_constructor = 24;
	public const int _kw_def = 25;
	public const int _kw_external = 26;
	public const int _kw_extend = 27;
	public const int _kw_public = 28;
	public const int _kw_private = 29;
	public const int _kw_protected = 30;
	public const int _kw_heap = 31;
	public const int _kw_if = 32;
	public const int _kw_elseif = 33;
	public const int _kw_else = 34;
	public const int _kw_while = 35;
	public const int _kw_for = 36;
	public const int _Kw_double = 37;
	public const int _Kw_int = 38;
	public const int _Kw_var = 39;
	public const int _Kw_function = 40;
	public const int _Kw_import = 41;
	public const int _Kw_from = 42;
	public const int _Kw_prefix = 43;
	public const int _Kw_static = 44;
	public const int _Kw_break = 45;
	public const int _Kw_continue = 46;
	public const int _literal_true = 47;
	public const int _literal_false = 48;
	public const int _literal_null = 49;
	public const int maxT = 70;
	public const int _inlinecomment = 71;
	public const int _blockcomment = 72;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

    private ProtoCore.Core core = null;

public DesignScript.Parser.Associative.CodeBlockNode root { get; set; }
	public Dictionary<string, DesignScript.Parser.Associative.ImportNode> GlobalModuleTable {get; set;}

	private int localVarCount = 0;
	private bool isGlobalScope = true;
    private bool codeSegmentStarted = false;

    private bool isLeft = false; // check if it is left hand side of the assignment expression

	private bool isLeftNode() { return isLeft; }
	private bool isArrayAccess()
	{
		Token pt = la;
		if( _ident == pt.kind ) 
		{
			pt = scanner.Peek();
			scanner.ResetPeek();
			if( _openbracket == pt.kind ) {
				return true;
			}
		}
		return false;
	}

	private bool IsTypedVariable()
    {
        Token pt = la;

        if (_ident == pt.kind) {
            pt = scanner.Peek();
            scanner.ResetPeek();
            if (":" == pt.val) 
                return true;
        }
        scanner.ResetPeek();
        return false;
    }
	
	private bool isFunctionCall()
	{
		Token pt = la;
		if( _ident == pt.kind ) 
		{
			pt = scanner.Peek();
			scanner.ResetPeek();
			if( _openparen == pt.kind ) {
				return true;
			}
		}
		return false;
	}
   
    private bool IsReplicationGuideIdent()
	{
		Token pt = la;

		if( _lessthan == pt.kind ) 
		{
			pt = scanner.Peek();
			if( _number == pt.kind ) 
			{
				pt = scanner.Peek();
				scanner.ResetPeek();
				if( _greaterthan == pt.kind ) 
				{
					return true;
				}
			}
		
		}
		scanner.ResetPeek();
		return false;
	}

	private bool hasIndices()
    {
        Token pt = la;
        if( "[" == pt.val )
		{ 
			scanner.ResetPeek();
			return true;
        }
		else return false;
    }

	private bool hasReturnType()
    {
        Token pt = la;
        if( _ident == pt.kind ) 
        {
            pt = scanner.Peek();
            scanner.ResetPeek();
            if( _ident == pt.kind ) {
                return true;
            }
        }
        return false;
    }
    
	private bool IsNumber()
    {
        Token pt = la;

        if (pt.val == "-") {
            pt = scanner.Peek();
            scanner.ResetPeek();
        }

        return ((_number == pt.kind) || (_float == pt.kind));
    }

	private bool isVariableDecl()
    {
        Token pt = la;
        if( _ident == pt.kind ) 
        {
            pt = scanner.Peek();
            scanner.ResetPeek();
            if( _ident == pt.kind ) {
                return true;
            }
        }

        if (_kw_heap == pt.kind)
        {
            pt = scanner.Peek();
            if (_ident == pt.kind)
            {
                pt = scanner.Peek();
                if (_ident == pt.kind)
                {
                    scanner.ResetPeek();
                    return true;
                }
            }
        }
        scanner.ResetPeek();
        return false;
    }

    private string GetImportedModuleFullPath(string moduleLocation)
    {
        try
        {
            string fileName = moduleLocation.Replace("\"", String.Empty);
            if (Path.IsPathRooted(fileName) && File.Exists(fileName))
                return Path.GetFullPath(fileName);

            string rootModuleName = core.Options.RootModulePathName;
            string rootModuleDirectory = Directory.GetCurrentDirectory();
            if (rootModuleName != null)
                rootModuleDirectory = Path.GetDirectoryName(rootModuleName);

            string fullPathName = Path.Combine(rootModuleDirectory, fileName);
            if (File.Exists(fullPathName))
                return Path.GetFullPath(fullPathName);

            foreach (string directory in core.Options.IncludeDirectories)
            {
                fullPathName = Path.Combine(rootModuleDirectory, fileName);
                if (File.Exists(fullPathName))
                    return Path.GetFullPath(fullPathName);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

	private bool HasMoreAssignmentStatements()
    {
        Token pt = la;

        if (pt.kind != _ident)
            return false;

        bool gotAssignmentToken = false;
        bool gotEndLineToken = false;
        bool isAssignmentStatement = false;

        while (true) {
            pt = scanner.Peek();
            if (pt.kind == 0) {
                break;
            }
            else if (pt.val == "=") {
                isAssignmentStatement = true;
                break;
            }
            else if (pt.kind == _endline) {
                isAssignmentStatement = gotAssignmentToken;
                break;
            }
            else if (pt.val == "{")
                break;
        }

        scanner.ResetPeek();
        return isAssignmentStatement;
    }

	private bool IsModifierStack()
    {
        Token pt = la;
        if (pt.val != "{")
        {
            scanner.ResetPeek();
            return false;
        }

        int counter = 1;
        pt = scanner.Peek();
        while (counter != 0 && pt.kind != _EOF)
        {
            if (pt.val == "{")
                counter++;
            else if (pt.val == "}")
                counter--;

            if (pt.val == ";" && counter == 1)
            {
                scanner.ResetPeek();
                return true;
            }

            pt = scanner.Peek();
        }

        scanner.ResetPeek();
        return false;
    }


	
	public Parser(Scanner scanner, ProtoCore.Core coreObj) {
		this.scanner = scanner;
		errors = new Errors();
        core = coreObj;
		root = new DesignScript.Parser.Associative.CodeBlockNode();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }
				if (la.kind == 71) {
				CommentNode cNode = new CommentNode(la.col, la.line, la.val, CommentNode.CommentType.Inline); 
				root.Body.Add(cNode); 
				}
				if (la.kind == 72) {
				CommentNode cNode = new CommentNode(la.col, la.line, la.val, CommentNode.CommentType.Block); 
				root.Body.Add(cNode); 
				}

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void DesignScriptParser() {
		DesignScript.Parser.Associative.CodeBlockNode code = null;
		code = new DesignScript.Parser.Associative.CodeBlockNode();
		code.Line = la.line;
		code.Col = la.col;
		Node node = null;
		
		while (la.kind != _EOF) {
			bool valid = false; 
			if (la.kind == 41) {
				Import_Statement(out node);
				valid = true; 
			} else if (StartOf(1)) {
				Assoc_Statement(out node);
				valid = true; 
			} else if (la.kind == 25 || la.kind == 26) {
				Assoc_FunctionDecl(out node);
				valid = true; 
			} else if (la.kind == 23) {
				Assoc_ClassDecl(out node);
				valid = true; 
			} else if (la.kind == 7) {
				Assoc_LanguageBlock(out node);
				valid = true; 
			} else SynErr(71);
			if(false == valid)
			   Get();
			if (node != null)
			   code.Body.Add(node);
			node = null;
			
		}
		root.Body.Add(code); 
	}

	void Import_Statement(out Node node) {
		DesignScript.Parser.Associative.ImportNode importNode = null;
		importNode = new DesignScript.Parser.Associative.ImportNode();
		
		while (!(la.kind == 0 || la.kind == 41)) {SynErr(72); Get();}
		Expect(41);
		if (t.kind == _Kw_import) importNode.KwImport.SetValue(t.val, t.line, t.col); 
		if (la.val == "(") importNode.OpenParen.SetValue(la.val, la.line, la.col); 
		Expect(9);
		if (la.kind == 4) {
			Get();
			if (t.kind == _textstring) { DesignScript.Parser.Associative.StringNode path = new DesignScript.Parser.Associative.StringNode() { value = t.val, Line = t.line, Col = t.col }; importNode.Path = path; } 
		} else if (la.kind == 1) {
			Get();
			if (t.kind == _ident) importNode.Identifier.SetValue(t.val, t.line, t.col); 
			Expect(42);
			if (t.kind == _Kw_from) importNode.KwFrom.SetValue(t.val, t.line, t.col); 
			Expect(4);
			if (t.kind == _textstring) { DesignScript.Parser.Associative.StringNode path = new DesignScript.Parser.Associative.StringNode() { value = t.val, Line = t.line, Col = t.col }; importNode.Path = path; } 
		} else SynErr(73);
		if (la.val == ")" && importNode.OpenParen.Value != null) importNode.CloseParen.SetValue(la.val, la.line, la.col); else if (la.val != ")") importNode.OpenParen.Value = null; 
		Expect(10);
		if (la.kind == 43) {
			Get();
			if (t.kind == _Kw_prefix) importNode.KwPrefix.SetValue(t.val, t.line, t.col); 
			Expect(1);
			if (t.kind == _ident) importNode.PrefixIdent.SetValue(t.val, t.line, t.col);  
		}
		Expect(20);
		if (t.val == ";")
		   importNode.EndLine.SetValue(t.val, t.line, t.col);
		else
		{
		   if (null != importNode.OpenParen)
		       importNode.OpenParen.SetValue(string.Empty, -1, -1);
		   if (null != importNode.CloseParen)
		       importNode.CloseParen.SetValue(string.Empty, -1, -1);
		}
		
		// We only allow "import" statements at the beginning of files, 
		// so if any actual code started (anything that is not an import
		// statement), we will mark this import statement as invalid.
		// 
		if (false != codeSegmentStarted)
		{
		   if (null != importNode.KwImport)
		   importNode.KwImport.SetValue(string.Empty, -1, -1);
		}
		
		string ModuleName = null; 
		if (importNode.Path != null && importNode.Path.value != null && (ModuleName = GetImportedModuleFullPath(importNode.Path.value)) != null && Path.GetExtension(ModuleName) == ".ds")
		{
		   if (GlobalModuleTable == null) 
		   {
		       GlobalModuleTable = new Dictionary<string, DesignScript.Parser.Associative.ImportNode>();
		       if (core.Options.RootModulePathName != null)
		           GlobalModuleTable[core.Options.RootModulePathName] = null;
		   }
		
		   if (GlobalModuleTable.ContainsKey(ModuleName)) 
		   {
		       importNode.CodeNode = null;
		       importNode.HasBeenImported = true;
		   }
		   else 
		   {
		
		       GlobalModuleTable[ModuleName] = importNode;
		
		       string curDirectory = Directory.GetCurrentDirectory();
		       Directory.SetCurrentDirectory(Path.GetDirectoryName(ModuleName));
		
		       DesignScript.Parser.Scanner scanner = new DesignScript.Parser.Scanner(ModuleName);
		       DesignScript.Parser.Parser parser = new DesignScript.Parser.Parser(scanner, core);
		       parser.GlobalModuleTable = GlobalModuleTable;
		
		       parser.Parse();
		       Directory.SetCurrentDirectory(curDirectory);
		
		       //if (parseErrors.ToString() != String.Empty)
		           //core.BuildStatus.LogSyntaxError(parseErrors.ToString());
		       //core.BuildStatus.errorCount += parser.errors.count;
		
		       importNode.CodeNode = parser.root as DesignScript.Parser.Associative.CodeBlockNode;
		   }
		} 
		
		node = importNode;
		
		
	}

	void Assoc_Statement(out Node node) {
		codeSegmentStarted = true;
		DesignScript.Parser.Associative.StatementNode Node = new DesignScript.Parser.Associative.StatementNode(); 
		Node stmtNode = null; DesignScript.Parser.Associative.IDEHelpNode Endline = Node.endLine; 
		Node.Line = la.line; Node.Col = la.col;
		
		while (!(StartOf(2))) {SynErr(74); Get();}
		if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Assoc_FunctionalStatement(out stmtNode, ref Endline);
		} else if (la.kind == 7) {
			Assoc_LanguageBlock(out stmtNode);
		} else if (la.kind == 20) {
			Get();
			if (t.val == ";") Endline.SetValue(t.val, t.line, t.col); 
		} else SynErr(75);
		Node.Statement = stmtNode;  Node.endLine = Endline; node = Node; 
	}

	void Assoc_FunctionDecl(out Node node) {
		codeSegmentStarted = true;
		DesignScript.Parser.Associative.FunctionDefinitionNode f = null;
		f = new DesignScript.Parser.Associative.FunctionDefinitionNode()
		{
		   Line = la.line,
		   Col = la.col
		};
		
		if (la.kind == 26) {
			Get();
			if (t.val == "external")
			   f.Kwexternal.SetValue(t.val, t.line, t.col);
			
			if (la.kind == 22) {
				Get();
				if (t.val == "native")
				   f.Kwnative.SetValue(t.val, t.line, t.col);
				
			}
			Assoc_ExternalLibraryReference(ref f);
		}
		Expect(25);
		if (t.val == "def")
		   f.Kwdef.SetValue(t.val, t.line, t.col);
		else
		   Get();
		
		DesignScript.Parser.Associative.IDEHelpNode IdentName = f.name;
		DesignScript.Parser.Associative.TypeNode IDEType = new DesignScript.Parser.Associative.TypeNode(); 
		
		DesignScript.Parser.Associative.ArgumentSignatureNode argumentSignature = null;
		DesignScript.Parser.Associative.Pattern pattern = null;
		
		Assoc_MethodSignature(out argumentSignature, out pattern, ref IdentName, ref IDEType);
		f.IDEReturnType = IDEType;
		f.name = IdentName;
		f.Pattern = pattern;
		f.Singnature = argumentSignature;
		
		while (!(StartOf(3))) {SynErr(76); Get();}
		Node functionBody = null; 
		if (la.kind == 20) {
			Get();
			f.endLine.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 53) {
			Get();
			DesignScript.Parser.Associative.CodeBlockNode func = new DesignScript.Parser.Associative.CodeBlockNode(); 
			DesignScript.Parser.Associative.BinaryExpressionNode binaryExpr = new DesignScript.Parser.Associative.BinaryExpressionNode();
			binaryExpr.op.SetValue(t.val, t.line, t.col);
			Node expr;
			
			Assoc_Expression(out expr);
			binaryExpr.RightNode = expr;
			List<Node> body = new List<Node>(); 
			body.Add(binaryExpr);
			func.Body = body;
			f.FunctionBody = func as DesignScript.Parser.Associative.CodeBlockNode; 
			
			Expect(20);
			f.endLine.SetValue(t.val, t.line, t.col); 
			
		} else if (la.kind == 50) {
			Assoc_FunctionalMethodBodyMultiLine(out functionBody);
			f.FunctionBody = functionBody as DesignScript.Parser.Associative.CodeBlockNode; 
		} else SynErr(77);
		node = f; 
	}

	void Assoc_ClassDecl(out Node node) {
		codeSegmentStarted = true;
		DesignScript.Parser.Associative.ClassDeclNode classnode;
		classnode = new DesignScript.Parser.Associative.ClassDeclNode()
		{
		   Line = la.line,
		   Col = la.col
		};
		
		if (la.val == "class") classnode.Kwclass.SetValue(la.val, la.line, la.col);      
		Expect(23);
		if (la.kind == _ident) classnode.ClassName.SetValue(la.val, la.line, la.col);    
		Expect(1);
		if (la.kind == 27) {
			if (la.kind == _kw_extend) classnode.Kwextend.SetValue(la.val, la.line, la.col); 
			Get();
			if (la.kind == _ident) classnode.AddExt(la.val, la.line, la.col);                
			Expect(1);
			if (la.kind == _ident) classnode.AddExt(la.val, la.line, la.col);                
			while (la.kind == 1) {
				Get();
			}
		}
		if (la.val == "{") classnode.openBrace.SetValue(la.val, la.line, la.col);        
		Expect(50);
		while (la.kind != _EOF && la.val != "}") {
			while (!(StartOf(4))) {SynErr(78); Get();}
			int kwstaticline = -1, kwstaticcol = -1;             
			if (la.kind == 28 || la.kind == 29 || la.kind == 30) {
				classnode.AddAccessLabel(la.val, la.line, la.col);   
				if (la.kind == 28) {
					Get();
				} else if (la.kind == 29) {
					Get();
				} else {
					Get();
				}
			}
			if (la.kind == 44) {
				Get();
				if (t.kind == _Kw_static) kwstaticline = t.line; kwstaticcol = t.col; 
			}
			if (la.kind == 24) {
				Node constr = null; 
				Assoc_constructordecl(out constr);
				if (kwstaticline != -1)
				{
				   DesignScript.Parser.Associative.ConstructorDefinitionNode constrNode = null;
				   constrNode = (constr as DesignScript.Parser.Associative.ConstructorDefinitionNode);
				   if (null != constrNode)
				       constrNode.KwStatic.SetValue("static", kwstaticline, kwstaticcol);
				}
				
				classnode.funclist.Add(constr);
				
			} else if (la.kind == 1) {
				Node varnode = null; 
				Assoc_vardecl(out varnode);
				classnode.varlist.Add(varnode);
				if (kwstaticline != -1)
				{
				   DesignScript.Parser.Associative.VarDeclNode varDeclNode = null;
				   varDeclNode = (varnode as DesignScript.Parser.Associative.VarDeclNode);
				   if (null != varDeclNode)
				       varDeclNode.KwStatic.SetValue("static", kwstaticline, kwstaticcol);
				}
				
				while (!(la.kind == 0 || la.kind == 20)) {SynErr(79); Get();}
				if (la.val == ";") classnode.AddVarDeclComma(la.val, la.line, la.col); 
				Expect(20);
			} else if (la.kind == 25 || la.kind == 26) {
				Node funcnode = null; 
				Assoc_FunctionDecl(out funcnode);
				if (kwstaticline != -1)
				{
				   DesignScript.Parser.Associative.FunctionDefinitionNode funcDefNode = null;
				   funcDefNode = (funcnode as DesignScript.Parser.Associative.FunctionDefinitionNode);
				   if (null != funcDefNode)
				       funcDefNode.KwStatic.SetValue("static", kwstaticline, kwstaticcol);
				}
				classnode.funclist.Add(funcnode);
				
			} else SynErr(80);
		}
		if (la.val == "}" && classnode.openBrace.Value != null)
		   classnode.closeBrace.SetValue(la.val, la.line, la.col);
		else if (la.val != "}")
		   classnode.openBrace.Value = null;
		
		Expect(51);
		node = classnode; 
	}

	void Assoc_LanguageBlock(out Node node) {
		codeSegmentStarted = true;
		DesignScript.Parser.Associative.LanguageBlockNode langblock = null;
		langblock = new DesignScript.Parser.Associative.LanguageBlockNode();
		
		if (la.val == "[")
		   langblock.openBracket.SetValue(la.val, la.line, la.col);
		
		Expect(7);
		langblock.Line = t.line;
		langblock.Col = t.col;
		
		if (la.val == "Imperative" || la.val == "Associative")
		   langblock.language.SetValue(la.val, la.line, la.col);
		
		if (la.val == "Imperative")
		   langblock.languageblock.language = ProtoCore.Language.kImperative;
		else if (la.val == "Associative")
		   langblock.languageblock.language = ProtoCore.Language.kAssociative;
		
		Expect(1);
		while (la.kind == 52) {
			string comma = null; string key = null; string equal = null;                     
			Get();
			if (t.val == ",") comma = t.val; int comma_line = t.line; int comma_col = t.col; 
			Expect(1);
			if (t.kind == _ident) key = t.val; int key_line = t.line; int key_col = t.col;   
			Expect(53);
			if (t.val == "=") equal = t.val; int equal_line = t.line; int equal_col = t.col; 
			Expect(4);
			DesignScript.Parser.Associative.StringNode stringNode = null;
			stringNode = new DesignScript.Parser.Associative.StringNode();
			if (t.kind == _textstring)
			{
			   stringNode.value = t.val;
			   stringNode.Line = t.line;
			   stringNode.Col = t.col;
			}
			
			langblock.AddProperty(comma, comma_line, comma_col, key, key_line, key_col, equal, equal_line, equal_col, stringNode);
			
		}
		if (la.val == "]" && langblock.openBracket.Value != null) langblock.closeBracket.SetValue(la.val, la.line, la.col); else if (la.val != "]") langblock.openBracket.Value = null; 
		Expect(8);
		if (la.val == "{") langblock.openBrace.SetValue(la.val, la.line, la.col); 
		Expect(50);
		Node codeNode = null; 
		if (langblock.languageblock.language == ProtoCore.Language.kAssociative) {
			Hydrogen(out codeNode);
		} else if (langblock.languageblock.language == ProtoCore.Language.kImperative) {
			Imperative(out codeNode);
		} else SynErr(81);
		if (langblock.languageblock.language == ProtoCore.Language.kInvalid) {
			int openCurlyBraceCount = 0, closeCurlyBraceCount = 0;
			DesignScript.Parser.Associative.CodeBlockNode codeBlockInvalid;
			codeBlockInvalid = new DesignScript.Parser.Associative.CodeBlockNode();
			
			while (closeCurlyBraceCount <= openCurlyBraceCount) {
				if (la.kind == 7) {
					Assoc_LanguageBlock(out codeNode);
					codeBlockInvalid.Body.Add(codeNode); 
				} else if (la.kind == 50) {
					Get();
					openCurlyBraceCount++; 
				} else if (la.kind == 51) {
					if (la.val == "}" && langblock.openBrace.Value != null)
					   langblock.closeBrace.SetValue(la.val, la.line, la.col);
					else if (la.val != "}")
					   langblock.openBrace.Value = null;
					
					Get();
					closeCurlyBraceCount++; 
				} else if (la.kind == 0) {
					Get();
					Expect(51);
					break; 
				} else if (true) {
					Get(); 
				} else SynErr(82);
			}
			codeNode = codeBlockInvalid; 
		} else if (true) {
			if (la.val == "}" && langblock.openBrace.Value != null)
			   langblock.closeBrace.SetValue(la.val, la.line, la.col);
			else if (la.val != "}")
			   langblock.openBrace.Value = null;
			
			Expect(51);
		} else SynErr(83);
		langblock.code = codeNode; node = langblock; 
	}

	void Hydrogen(out Node node) {
		DesignScript.Parser.Associative.CodeBlockNode code = null;
		code = new DesignScript.Parser.Associative.CodeBlockNode();
		code.Line = la.line; code.Col = la.col; node = null;
		
		while (la.kind != _EOF && la.val != "}") {
			bool valid = false; 
			if (la.kind == 41) {
				Import_Statement(out node);
				valid = true; 
			} else if (StartOf(1)) {
				Assoc_Statement(out node);
				valid = true; 
			} else if (la.kind == 25 || la.kind == 26) {
				Assoc_FunctionDecl(out node);
				valid = true; 
			} else if (la.kind == 23) {
				Assoc_ClassDecl(out node);
				valid = true; 
			} else if (la.kind == 7) {
				Assoc_LanguageBlock(out node);
				valid = true; 
			} else SynErr(84);
			if (false == valid)
			   Get();
			if (node != null)
			   code.Body.Add(node);
			
		}
		node = code; 
	}

	void Assoc_FunctionalStatement(out Node node, ref DesignScript.Parser.Associative.IDEHelpNode Endline) {
		DesignScript.Parser.Associative.BinaryExpressionNode expressionNode = new DesignScript.Parser.Associative.BinaryExpressionNode(); 
		while (!(StartOf(5))) {SynErr(85); Get();}
		Node leftNode = null; 
		isLeft = true; 
		Assoc_DecoratedIdentifier(out leftNode);
		expressionNode.Line = leftNode.Line; expressionNode.Col = leftNode.Col; expressionNode.LeftNode = leftNode; 
		isLeft = false; 
		node = leftNode; 
		if(!(leftNode is DesignScript.Parser.Associative.PostFixNode)) {
		Expect(53);
		Node rightNode = null; 
		if (t.val == "=") { expressionNode.op.SetValue(t.val, t.line, t.col); }
		else { expressionNode.op.SetValue(null, 0, 0); }
		
		if (la.val == "[") {
			Assoc_LanguageBlock(out rightNode);
			Endline = null; 
		} else if (HasMoreAssignmentStatements()) {
			Assoc_FunctionalStatement(out rightNode, ref Endline);
		} else if (!IsModifierStack()) {
			Assoc_Expression(out rightNode);
			while (!(la.kind == 0 || la.kind == 20)) {SynErr(86); Get();}
			Expect(20);
			if(t.val == ";") Endline.SetValue(t.val, t.line, t.col); else Endline.SetValue(null, -1, -1); 
		} else if (IsModifierStack()) {
			DesignScript.Parser.Associative.ModifierStackNode mstack = new DesignScript.Parser.Associative.ModifierStackNode(); 
			if (la.val == "{") { mstack.openSharpBrace.SetValue(la.val, la.line, la.col); mstack.Line = la.line; mstack.Col = la.col; }
			else { 	mstack.openSharpBrace.SetValue(null, 0, 0); }
			
			Expect(50);
			string name = null; 
			DesignScript.Parser.Associative.BinaryExpressionNode expression = new DesignScript.Parser.Associative.BinaryExpressionNode(); ; Node expression2 = null; 
			Assoc_Expression(out expression2);
			if (la.kind == 55) {
				string _arr = null; int _line = 0; int _col = 0; 
				Get();
				if (t.val == "=>") _arr = t.val; _line = t.line; _col = t.col; 
				Expect(1);
				if (t.kind == _ident) { name = t.val; mstack.AddArrow(_arr, _line, _col, t.val, t.line, t.col); } else { name = null; mstack.AddArrow(_arr, _line, _col, null, 0, 0); }
			}
			expression.LeftNode = leftNode;
			expression.op = expressionNode.op;
			expression.RightNode = expression2;
			mstack.AddElementNode(expression, name);
			
			while (!(la.kind == 0 || la.kind == 20)) {SynErr(87); Get();}
			Expect(20);
			if (t.val == ";") mstack.AddEndLine(t.val, t.line, t.col); else mstack.AddEndLine(null, 0, 0); 
			while (StartOf(6)) {
				name = null; 
				bool bHasOperator = false; expression = new DesignScript.Parser.Associative.BinaryExpressionNode(); expression2 = null; 
				bHasOperator = true; DesignScript.Parser.Associative.IDEHelpNode IDEop = expression.op; 
				if (StartOf(7)) {
					Assoc_BinaryOps(ref IDEop);
				}
				Assoc_Expression(out expression2);
				if (la.kind == 55) {
					string _arr = null; int _line = 0; int _col = 0; 
					Get();
					if (t.val == "=>") _arr = t.val; _line = t.line; _col = t.col; 
					Expect(1);
					if (t.kind == _ident) { name = t.val; mstack.AddArrow(_arr, _line, _col, t.val, t.line, t.col); } else { name = null; mstack.AddArrow(_arr, _line, _col, null, 0, 0); }
				}
				if(!bHasOperator)
				{
				expression.RightNode = expression2;
				expression.LeftNode = leftNode; 
				expression.op = expressionNode.op;
				mstack.AddElementNode(expression, name);
				}
				else
				{ 
				expression.LeftNode = leftNode;
				DesignScript.Parser.Associative.BinaryExpressionNode expression3 = new DesignScript.Parser.Associative.BinaryExpressionNode();
				expression3.LeftNode = leftNode;
				expression3.op = IDEop;
				expression3.RightNode = expression2;
				expression.LeftNode = leftNode;
				expression.RightNode = expression3;
				expression.op = expressionNode.op;
				mstack.AddElementNode(expression, name);
				}
				
				Expect(20);
				if (t.val == ";") mstack.AddEndLine(t.val, t.line, t.col); else mstack.AddEndLine(null, 0, 0); 
			}
			if (la.val == "}" && mstack.openSharpBrace.Value != null) mstack.closeBrace.SetValue(la.val, la.line, la.col); else if (la.val != "}") mstack.openSharpBrace.SetValue(null, 0, 0); rightNode = mstack;
			Expect(51);
		} else SynErr(88);
		expressionNode.RightNode = rightNode; node = expressionNode;
		                              if (rightNode is DesignScript.Parser.Associative.ExprListNode) {
		                                  DesignScript.Parser.Associative.IdentifierNode identNode = 
		                                      expressionNode.LeftNode as DesignScript.Parser.Associative.IdentifierNode;
		                                  if (null != identNode) {
		                                      DesignScript.Parser.Associative.IDEHelpNode identValue = identNode.IdentValue;
		                                      rightNode.Name = ((null != identValue) ? identValue.Value : null);
		                                  }
		                              }
		                          } 
	}

	void Assoc_StatementList(out List<Node> NodeList) {
		NodeList = new List<Node>(); 
		while (StartOf(1)) {
			Node node = null; 
			Assoc_Statement(out node);
			if (node != null) NodeList.Add(node); 
		}
	}

	void Assoc_constructordecl(out Node constrNode) {
		DesignScript.Parser.Associative.ConstructorDefinitionNode constr = new DesignScript.Parser.Associative.ConstructorDefinitionNode() { Line = la.line, Col = la.col }; 
		Expect(24);
		constr.Kwconstructor.SetValue(t.val, t.line, t.col); 
		DesignScript.Parser.Associative.IDEHelpNode IdentName = constr.name; DesignScript.Parser.Associative.TypeNode IDEType = new DesignScript.Parser.Associative.TypeNode(); 
		DesignScript.Parser.Associative.ArgumentSignatureNode argumentSignature = null; DesignScript.Parser.Associative.Pattern pattern = null; 
		Assoc_MethodSignature(out argumentSignature, out pattern,  ref IdentName, ref IDEType);
		constr.name = IdentName; constr.IDEReturnType = IDEType; constr.Pattern = pattern; constr.Signature = argumentSignature; 
		if (la.kind == 54) {
			Get();
			constr.SetColonToken(t); 
			Assoc_BaseConstructorCall(constr);
		}
		Node functionBody = null; 
		Assoc_FunctionalMethodBodyMultiLine(out functionBody);
		constr.FunctionBody = functionBody as DesignScript.Parser.Associative.CodeBlockNode; 
		constrNode = constr; 
	}

	void Assoc_vardecl(out Node node) {
		DesignScript.Parser.Associative.VarDeclNode varDeclNode = new DesignScript.Parser.Associative.VarDeclNode(); 
		Expect(1);
		varDeclNode.Line = t.line; varDeclNode.Col = t.col; varDeclNode.name.SetValue(t.val, t.line, t.col); 
		if (t.kind == _ident) varDeclNode.name.SetValue(t.val, t.line, t.col); 
		if (la.kind == 54) {
			DesignScript.Parser.Associative.TypeNode IDEType = new DesignScript.Parser.Associative.TypeNode(); 
			Assoc_TypeRestriction(out IDEType);
			varDeclNode.IDEArgumentType = IDEType; 
		}
		if (la.kind == 53) {
			Get();
			if (t.val == "=") varDeclNode.equal.SetValue(t.val, t.line, t.col); 
			Node rhsNode = null; 
			Assoc_Expression(out rhsNode);
			varDeclNode.NameNode = rhsNode; 
		}
		node = varDeclNode; 
	}

	void Imperative(out Node codeBlockNode) {
		Node node = null; 
		                       DesignScript.Parser.Imperative.CodeBlockNode codeblock = new DesignScript.Parser.Imperative.CodeBlockNode();
		  codeblock.Col = la.col;
		codeblock.Line = la.line;
		
		while (la.kind != _EOF && la.val !="}") {
			bool valid = false; 
			if (StartOf(8)) {
				Imperative_stmt(out node);
				valid = true; 
			} else if (la.kind == 25) {
				Imperative_functiondecl(out node);
				valid = true; 
			} else SynErr(89);
			if(!valid) Get(); 
			if (node != null)
			{
			   codeblock.Body.Add(node);
			} 
			
		}
		codeBlockNode = codeblock; 
	}

	void Assoc_MethodSignature(out DesignScript.Parser.Associative.ArgumentSignatureNode argumentSign, out DesignScript.Parser.Associative.Pattern pattern, ref DesignScript.Parser.Associative.IDEHelpNode IDEName, ref DesignScript.Parser.Associative.TypeNode typeNode) {
		Expect(1);
		if (t.kind == _ident) IDEName.SetValue(t.val, t.line, t.col); 
		if (la.kind == 54) {
			Assoc_TypeRestriction(out typeNode);
		}
		Assoc_ArgumentSignatureDefinition(out argumentSign);
		pattern = null; 
		if (la.kind == 13) {
			Assoc_PatternExpression(out pattern);
		}
	}

	void Assoc_BaseConstructorCall(DesignScript.Parser.Associative.ConstructorDefinitionNode constr) {
		DesignScript.Parser.Associative.FunctionCallNode f = null;
		if (la.val == "base")
		{
		   Get();
		   constr.CreateBaseConstructorNode();
		   f = constr.BaseConstructorNode;
		   constr.KwBase.SetValue(t.val, t.line, t.col);
		}
		
		Expect(6);
		constr.SetDotToken(t); 
		if (la.kind == 1) {
			Node identNode = null; 
			Assoc_Ident(out identNode);
			constr.SetBaseConstructor(identNode); 
		}
		Assoc_Arguments(ref f);
	}

	void Assoc_FunctionalMethodBodyMultiLine(out Node funcBody) {
		DesignScript.Parser.Associative.CodeBlockNode functionBody = new DesignScript.Parser.Associative.CodeBlockNode(); 
		List<Node> body = new List<Node>(); 
		
		if (la.val == "{") functionBody.openBrace.SetValue(la.val, la.line, la.col); functionBody.Line = la.line; functionBody.Col = la.col; 
		Expect(50);
		Assoc_StatementList(out body);
		functionBody.Body =body;  
		if (la.val == "}" && functionBody.openBrace.Value != null) functionBody.closeBrace.SetValue(la.val, la.line, la.col); else if (la.val != "}") functionBody.openBrace.Value = null; 
		Expect(51);
		funcBody = functionBody; 
	}

	void Assoc_Ident(out Node node) {
		DesignScript.Parser.Associative.IdentifierNode identNode = new DesignScript.Parser.Associative.IdentifierNode(); 
		Expect(1);
		if (t.kind == _ident) { 
		if (t.val != "return") identNode.IdentValue.SetValue(t.val, t.line, t.col); 
		else identNode.IdentValueReturn.SetValue(t.val, t.line, t.col); } 
		#if ENABLE_INC_DEC_FIX 
		if (la.kind == 66 || la.kind == 67) {
			DesignScript.Parser.Associative.PostFixNode pfNode = new DesignScript.Parser.Associative.PostFixNode(); 
			DesignScript.Parser.Associative.IDEHelpNode pfOperator = pfNode.OperatorPos; 
			Associative_PostFixOp(ref pfOperator);
			pfNode.OperatorPos = pfOperator; pfNode.Line = node.Line; pfNode.Col = node.Col; pfNode.Identifier = node;  
			node = pfNode; 
		}
		#endif 
		node = identNode; 
	}

	void Assoc_Arguments(ref DesignScript.Parser.Associative.FunctionCallNode f) {
		if (la.val == "(") f.openParen.SetValue(la.val, la.line, la.col); 
		Expect(9);
		if (StartOf(9)) {
			Node n; 
			Assoc_Expression(out n);
			f.FormalArguments.Add(n); 
			while (WeakSeparator(52,9,10) ) {
				f.AddComma(t.val, t.line, t.col); 
				Assoc_Expression(out n);
				f.FormalArguments.Add(n); 
			}
		}
		if (la.val == ")" && f.openParen.Value != null) f.closeParen.SetValue(la.val, la.line, la.col); else if (la.val != ")") f.openParen.SetValue(null, 0, 0); 
		Expect(10);
	}

	void Assoc_ExternalLibraryReference(ref DesignScript.Parser.Associative.FunctionDefinitionNode f) {
		if (la.val == "(") f.libOpenBrace.SetValue(la.val, la.line, la.col); 
		Expect(9);
		Expect(4);
		DesignScript.Parser.Associative.StringNode libName = new DesignScript.Parser.Associative.StringNode(); 
		if (t.kind == _textstring) { libName.value = t.val; libName.Line = t.line; libName.Col = t.col; } else Get(); 
		if (la.val == ")" && f.libOpenBrace.Value != null) f.libCloseBrace.SetValue(la.val, la.line, la.col); else if (la.val != ")") f.libOpenBrace.Value = null; f.libName = libName; 
		Expect(10);
	}

	void Assoc_Expression(out Node node) {
		node = null; 
		Assoc_LogicalExpression(out node);
		while (la.kind == 56) {
			Assoc_TernaryOp(ref node);
		}
	}

	void Assoc_TypeRestriction(out DesignScript.Parser.Associative.TypeNode IDEType) {
		IDEType = new DesignScript.Parser.Associative.TypeNode(); 
		ExpectWeak(54, 11);
		if (t.val == ":") IDEType.colon.SetValue(t.val, t.line, t.col); 
		if (StartOf(12)) {
			if (la.kind == 37) {
				Get();
			} else if (la.kind == 38) {
				Get();
			} else if (la.kind == 39) {
				Get();
			} else {
				Get();
			}
			IDEType.BuildInTypeSetValue(t.val, t.line, t.col); 
		} else if (la.kind == 1) {
			Get();
			IDEType.UserDefinedTypeSetValue(t.val, t.line, t.col); 
		} else SynErr(90);
		while (la.kind == 7) {
			string openBracket = null; string closeBracket = null; 
			if (la.val == "[")  openBracket = la.val; int bracket_line = la.line; int bracket_col = la.col; 
			Get();
			if (la.val == "]") closeBracket = la.val; 
			Expect(8);
			if (openBracket != null && closeBracket != null) IDEType.AddBracket(openBracket, bracket_line, bracket_col, closeBracket, t.line, t.col); 
			if (la.kind == 21) {
				Get();
				IDEType.op.SetValue(t.val, t.line, t.col); 
				string multiDimOpenBracket = null; string multiDimCloseBracket = null; 
				if (la.val == "[")  multiDimOpenBracket = la.val; int bracket2_line = la.line; int bracket2_col = la.col; 
				Expect(7);
				if (la.val == "]")  multiDimCloseBracket = la.val; 
				Expect(8);
				if (multiDimOpenBracket != null && multiDimCloseBracket != null) 
				IDEType.AddMultiDimNodes(multiDimOpenBracket, bracket2_line, bracket2_col, multiDimCloseBracket, t.line, t.col); 
			}
		}
	}

	void Assoc_ArgumentSignatureDefinition(out DesignScript.Parser.Associative.ArgumentSignatureNode argumentSign) {
		DesignScript.Parser.Associative.ArgumentSignatureNode argumentSignature = new DesignScript.Parser.Associative.ArgumentSignatureNode() { Line = la.line, Col = la.col }; 
		while (!(la.kind == 0 || la.kind == 9)) {SynErr(91); Get();}
		if (la.val == "(") argumentSignature.openBrace.SetValue(la.val, la.line, la.col); 
		Expect(9);
		if (la.kind == 1) {
			Node arg;
			Assoc_FunctionParameterDecl(out arg);
			argumentSignature.AddArgument(arg); 
			while (WeakSeparator(52,13,10) ) {
				if (t.val == ",") argumentSignature.AddComma(t.val, t.line, t.col); 
				Assoc_FunctionParameterDecl(out arg);
				argumentSignature.AddArgument(arg); 
			}
		}
		if (la.val == ")" && argumentSignature.openBrace.Value != null) argumentSignature.closeBrace.SetValue(la.val, la.line, la.col); else if (la.val != ")") argumentSignature.openBrace.Value = null; argumentSign = argumentSignature; 
		Expect(10);
	}

	void Assoc_PatternExpression(out DesignScript.Parser.Associative.Pattern pattern) {
		DesignScript.Parser.Associative.Pattern p = new DesignScript.Parser.Associative.Pattern(); 
		Expect(13);
		p.bitOr.SetValue(t.val, t.line, t.col); p.Line = t.line; p.Col = t.col; 
		Node exp = null; 
		Assoc_Expression(out exp);
		p.Expression = exp; pattern = p; 
	}

	void Assoc_FunctionParameterDecl(out Node node) {
		DesignScript.Parser.Associative.VarDeclNode varDeclNode = new DesignScript.Parser.Associative.VarDeclNode(); 
		Expect(1);
		varDeclNode.Line = t.line; varDeclNode.Col = t.col; varDeclNode.name.SetValue(t.val, t.line, t.col); 
		if (t.kind == _ident) varDeclNode.name.SetValue(t.val, t.line, t.col); 
		if (la.kind == 54) {
			DesignScript.Parser.Associative.TypeNode IDEType = new DesignScript.Parser.Associative.TypeNode(); 
			Assoc_TypeRestriction(out IDEType);
			varDeclNode.IDEArgumentType = IDEType; 
		}
		if (la.kind == 53) {
			Get();
			if (t.val == "=") varDeclNode.equal.SetValue(t.val, t.line, t.col); 
			Node rhsNode = null; 
			Assoc_Expression(out rhsNode);
			varDeclNode.NameNode = rhsNode; 
		}
		node = varDeclNode; 
	}

	void Assoc_BinaryOps(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		if (la.kind == 12 || la.kind == 57) {
			Assoc_AddOp(ref IDEop);
		} else if (la.kind == 58 || la.kind == 59 || la.kind == 60) {
			Assoc_MulOp(ref IDEop);
		} else if (StartOf(14)) {
			Assoc_ComparisonOp(ref IDEop);
		} else if (la.kind == 63 || la.kind == 64) {
			Assoc_LogicalOp(ref IDEop);
		} else SynErr(92);
	}

	void Assoc_AddOp(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		if (la.kind == 57) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 12) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else SynErr(93);
	}

	void Assoc_MulOp(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		if (la.kind == 58) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 59) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 60) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else SynErr(94);
	}

	void Assoc_ComparisonOp(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		switch (la.kind) {
		case 15: {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			break;
		}
		case 17: {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			break;
		}
		case 14: {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			break;
		}
		case 16: {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			break;
		}
		case 18: {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			break;
		}
		case 19: {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			break;
		}
		default: SynErr(95); break;
		}
	}

	void Assoc_LogicalOp(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		if (la.kind == 63) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 64) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else SynErr(96);
	}

	void Assoc_DecoratedIdentifier(out Node node) {
		node = null; 
		if (IsTypedVariable()) {
			Assoc_vardecl(out node);
		} else if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Assoc_IdentifierList(out node);
		} else SynErr(97);
	}

	void Assoc_LogicalExpression(out Node node) {
		node = null; 
		Assoc_ComparisonExpression(out node);
		while (la.kind == 63 || la.kind == 64) {
			DesignScript.Parser.Associative.BinaryExpressionNode binaryNode = new DesignScript.Parser.Associative.BinaryExpressionNode(); 
			DesignScript.Parser.Associative.IDEHelpNode IDEop = binaryNode.op; binaryNode.LeftNode = node; 
			Assoc_LogicalOp(ref IDEop);
			binaryNode.op = IDEop; 
			node = null; 
			Assoc_ComparisonExpression(out node);
			binaryNode.RightNode = node; 
			node = binaryNode; 
		}
	}

	void Assoc_TernaryOp(ref Node node) {
		while (la.kind == 56) {
			DesignScript.Parser.Associative.InlineConditionalNode inlineConNode = new DesignScript.Parser.Associative.InlineConditionalNode(); 
			inlineConNode.ConditionExpression = node; 
			Get();
			if (t.val == "?") inlineConNode.Question.SetValue(t.val, t.line, t.col); 
			node = null; 
			Assoc_Expression(out node);
			inlineConNode.TrueExpression = node; 
			Expect(54);
			if (t.val == ":") inlineConNode.Colon.SetValue(t.val, t.line, t.col); 
			node = null; 
			Assoc_Expression(out node);
			inlineConNode.FalseExpression = node; 
			node = inlineConNode; 
		}
	}

	void Assoc_UnaryExpression(out Node node) {
		DesignScript.Parser.Associative.UnaryExpressionNode an = new DesignScript.Parser.Associative.UnaryExpressionNode(); 
		node = null; 
		if (StartOf(15)) {
			Assoc_NegExpression(out an);
		} else if (StartOf(16)) {
			Assoc_BitUnaryExpression(out an);
		} else SynErr(98);
		node = an; 
	}

	void Assoc_NegExpression(out DesignScript.Parser.Associative.UnaryExpressionNode node) {
		node = new Associative.UnaryExpressionNode() ; 
		DesignScript.Parser.Associative.IDEHelpNode op = node.op; 
		Node exprNode = null; 
		Assoc_negop(out op);
		if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Assoc_IdentifierList(out exprNode);
		} else if (la.kind == 9) {
			Get();
			Assoc_Expression(out exprNode);
			Expect(10);
		} else SynErr(99);
		DesignScript.Parser.Associative.UnaryExpressionNode unary = new DesignScript.Parser.Associative.UnaryExpressionNode(); 
		unary.op = op;
		unary.Expression = exprNode;
		node = unary;
		                 
	}

	void Assoc_BitUnaryExpression(out DesignScript.Parser.Associative.UnaryExpressionNode node) {
		DesignScript.Parser.Associative.UnaryExpressionNode unary = new DesignScript.Parser.Associative.UnaryExpressionNode(); 
		DesignScript.Parser.Associative.IDEHelpNode IDEop = unary.op; 
		Assoc_unaryop(out IDEop);
		Node exprNode = null; 
		Assoc_Factor(out exprNode);
		unary.Expression = exprNode; 
		unary.op = IDEop; 
		node = unary; 
	}

	void Assoc_unaryop(out DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		IDEop = new Associative.IDEHelpNode(Associative.IDEHelpNode.NodeType.PunctuationNode); 
		if (la.kind == 11) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 65) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
			#if ENABLE_INC_DEC_FIX 
		} else if (la.kind == 66 || la.kind == 67) {
			Associative_PostFixOp(ref IDEop);
			#endif 
		} else SynErr(100);
		#if ENABLE_INC_DEC_FIX
		#else
		if (la.val == "++" || la.val == "--") Get();
		#endif
		
	}

	void Assoc_Factor(out Node node) {
		node = null; 
		if (IsNumber()) {
			Assoc_Number(out node);
		} else if (la.kind == 47) {
			Get();
			node = new DesignScript.Parser.Associative.BooleanNode(){ value = t.val, Line = t.line, Col = t.col }; 
		} else if (la.kind == 48) {
			Get();
			node = new DesignScript.Parser.Associative.BooleanNode(){ value = t.val, Line = t.line, Col = t.col }; 
		} else if (la.kind == 49) {
			Get();
			node = new DesignScript.Parser.Associative.NullNode(){ value = t.val, Line = t.line, Col = t.col }; 
		} else if (la.kind == 5) {
			Assoc_Char(out node);
		} else if (la.kind == 4) {
			Assoc_String(out node);
		} else if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Assoc_IdentifierList(out node);
		} else if (StartOf(17)) {
			Assoc_UnaryExpression(out node);
		} else if (la.kind == 7) {
			Assoc_LanguageBlock(out node);
		} else SynErr(101);
		if(IsReplicationGuideIdent()) { 
		Assoc_ReplicationGuideIdent(ref node);
		} 
	}

	void Assoc_negop(out DesignScript.Parser.Associative.IDEHelpNode op) {
		op = new Associative.IDEHelpNode(Associative.IDEHelpNode.NodeType.PunctuationNode); 
		if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
		} else if (la.kind == 12) {
			Get();
			op.SetValue(t.val, t.line, t.col); 
		} else SynErr(102);
	}

	void Assoc_IdentifierList(out Node node) {
		node = null; 
		Assoc_NameReference(out node);
		while (la.kind == 6) {
			DesignScript.Parser.Associative.IdentifierListNode identlist = new DesignScript.Parser.Associative.IdentifierListNode(); 
			Get();
			identlist.dot.SetValue(t.val, t.line, t.col); 
			Node rhs = null; 
			Assoc_NameReference(out rhs);
			identlist.LeftNode = node; 
			identlist.Optr = Operator.dot; 
			identlist.RightNode = rhs; 
			node = identlist; 
			
		}
	}

	void Assoc_ComparisonExpression(out Node node) {
		Assoc_RangeExpr(out node);
		while (StartOf(14)) {
			DesignScript.Parser.Associative.BinaryExpressionNode binaryNode = new DesignScript.Parser.Associative.BinaryExpressionNode(); 
			DesignScript.Parser.Associative.IDEHelpNode IDEop = binaryNode.op; 
			Assoc_ComparisonOp(ref IDEop);
			binaryNode.op = IDEop; 
			Node expr2 = null; 
			Assoc_RangeExpr(out expr2);
			binaryNode.LeftNode = node; binaryNode.RightNode = expr2; 
			node = binaryNode; 
		}
	}

	void Assoc_RangeExpr(out Node node) {
		Assoc_ArithmeticExpression(out node);
		if (la.kind == 21) {
			DesignScript.Parser.Associative.RangeExprNode rnode = new DesignScript.Parser.Associative.RangeExprNode(); 
			rnode.FromNode = node; rnode.Line = node.Line; rnode.Col = node.Col; 
			Get();
			rnode.op.SetValue(t.val, t.line, t.col); node = null; 
			Assoc_ArithmeticExpression(out node);
			rnode.ToNode = node; 
			if (!(t.val == "..")&&(la.val == "..")) { 
			if (la.kind == 21) {
				Get();
				rnode.stepOp.SetValue(t.val, t.line, t.col); DesignScript.Parser.Associative.IDEHelpNode stepOp2 = rnode.stepOp2; 
				Assoc_rangeStepOperator(ref stepOp2);
				rnode.stepOp2 = stepOp2; node = null; 
				Assoc_ArithmeticExpression(out node);
				rnode.StepNode = node; 
			}
			} 
			node = rnode; 
		}
	}

	void Assoc_ArithmeticExpression(out Node node) {
		Assoc_Term(out node);
		while (la.kind == 12 || la.kind == 57) {
			DesignScript.Parser.Associative.BinaryExpressionNode binaryNode = new DesignScript.Parser.Associative.BinaryExpressionNode(); 
			DesignScript.Parser.Associative.IDEHelpNode IDEop = binaryNode.op; 
			Assoc_AddOp(ref IDEop);
			binaryNode.op = IDEop; binaryNode.LeftNode = node; node = null; 
			Assoc_Term(out node);
			binaryNode.RightNode = node; 
			node = binaryNode; 
		}
	}

	void Assoc_rangeStepOperator(ref DesignScript.Parser.Associative.IDEHelpNode stepOp2) {
		if (la.kind == 65 || la.kind == 68) {
			if (la.kind == 68) {
				Get();
				stepOp2.SetValue(t.val, t.line, t.col); 
			} else {
				Get();
				stepOp2.SetValue(t.val, t.line, t.col); 
			}
		}
	}

	void Assoc_BitOp(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		if (la.kind == 61) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 62) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 13) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else SynErr(103);
	}

	void Associative_PostFixOp(ref DesignScript.Parser.Associative.IDEHelpNode IDEop) {
		if (la.kind == 66) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else if (la.kind == 67) {
			Get();
			IDEop.SetValue(t.val, t.line, t.col); 
		} else SynErr(104);
	}

	void Assoc_Term(out Node node) {
		Assoc_interimfactor(out node);
		while (la.kind == 58 || la.kind == 59 || la.kind == 60) {
			DesignScript.Parser.Associative.BinaryExpressionNode binaryNode = new DesignScript.Parser.Associative.BinaryExpressionNode(); 
			DesignScript.Parser.Associative.IDEHelpNode IDEop = binaryNode.op; binaryNode.LeftNode = node; 
			Assoc_MulOp(ref IDEop);
			binaryNode.op = IDEop; node = null; 
			Assoc_interimfactor(out node);
			binaryNode.RightNode = node; 
			node = binaryNode; 
		}
	}

	void Assoc_interimfactor(out Node node) {
		Assoc_Factor(out node);
		while (la.kind == 13 || la.kind == 61 || la.kind == 62) {
			DesignScript.Parser.Associative.BinaryExpressionNode binaryNode = new DesignScript.Parser.Associative.BinaryExpressionNode(); 
			DesignScript.Parser.Associative.IDEHelpNode IDEop = binaryNode.op; 
			Assoc_BitOp(ref IDEop);
			binaryNode.op = IDEop; node = null; 
			Assoc_Factor(out node);
			binaryNode.RightNode = node; 
			node = binaryNode; 
		}
	}

	void Assoc_Number(out Node node) {
		node = null; String localvalue = null; int line = -1; int col = -1; 
		if (la.kind == 12) {
			Get();
			localvalue = "-"; line = t.line; col = t.col; 
		}
		if (la.kind == 2) {
			Get();
			DesignScript.Parser.Associative.IntNode _node = null;
			if (t.kind == _number)
			{
			_node = new DesignScript.Parser.Associative.IntNode() { value = localvalue + t.val };
			_node.Line = t.line; _node.Col = t.col;  
			_node.sign.SetValue(localvalue, line, col);
			_node.IDEValue.SetValue(t.val, t.line, t.col); 
			}
			node = _node; 
			
		} else if (la.kind == 3) {
			Get();
			DesignScript.Parser.Associative.DoubleNode _node = null;
			if (t.kind == _float)
			{
			_node= new DesignScript.Parser.Associative.DoubleNode() { value = localvalue + t.val }; 
			_node.Line = t.line; 
			_node.Col = t.col;  
			_node.sign.SetValue(localvalue, line, col);
			_node.IDEValue.SetValue(t.val, t.line, t.col); 
			}
			node = _node; 
			
		} else SynErr(105);
	}

	void Assoc_Char(out Node node) {
		node = null; 
		Expect(5);
		DesignScript.Parser.Associative.CharNode _node = null;
		if (t.val.Length <= 2) {
		errors.SemErr(t.line, t.col, "Empty character literal.");
		}
		if (t.kind == _char)
		{
		_node = new DesignScript.Parser.Associative.CharNode() { value = t.val};
		_node.IDEValue.SetValue(t.val, t.line, t.col);
		_node.Line = t.line;
		_node.Col = t.col;
		}
		node = _node;
		
	}

	void Assoc_String(out Node node) {
		node = null; 
		Expect(4);
		DesignScript.Parser.Associative.StringNode _node = null;
		node = new DesignScript.Parser.Associative.StringNode(); 
		if (t.kind == _textstring)
		{
		_node = new DesignScript.Parser.Associative.StringNode() { value = t.val };
		_node.IDEValue.SetValue(t.val, t.line, t.col);
		_node.Line = t.line;
		_node.Col = t.col;
		}
		node = _node;
		
	}

	void Assoc_ReplicationGuideIdent(ref Node node) {
		DesignScript.Parser.Associative.ReplicationGuideNode RepGuideNode = new DesignScript.Parser.Associative.ReplicationGuideNode(); 
		RepGuideNode.factor = node; 
		List<Node> guides = new List<Node>();
		string _open = null; string _close = null; Node numNode = null; 
		string _num = null; int num_line = -1; int num_col = -1; 
		if (la.val == "<") _open = la.val; int _line = la.line; int _col = la.col; 
		Expect(14);
		Expect(2);
		if (t.kind == _number) { _num = t.val; num_line = t.line; num_col = t.col; } 
		if (la.val == ">") _close = la.val; 
		Expect(15);
		if (_open != null && _close != null) RepGuideNode.AddBrackets(_open, _line, _col, _num, num_line, num_col, _close, t.line, t.col); 
		guides.Add(numNode); 
		while (la.kind == 14) {
			_open = null; _close = null; numNode = null; 
			if (la.val == "<") _open = la.val; _line = la.line; _col = la.col; 
			Get();
			Expect(2);
			if (t.kind == _number) { _num = t.val; num_line = t.line; num_col = t.col; } 
			if (la.val == ">") _close = la.val; 
			Expect(15);
			if (_open != null && _close != null) RepGuideNode.AddBrackets(_open, _line, _col, _num, num_line, num_col, _close, t.line, t.col); 
			guides.Add(numNode); 
		}
		RepGuideNode.ReplicationGuides = guides; 
		node = RepGuideNode; 
	}

	void Assoc_ParenExp(out Node node) {
		Node exp = null; DesignScript.Parser.Associative.ParenExpressionNode pNode = new DesignScript.Parser.Associative.ParenExpressionNode(); 
		if (la.val == "(") pNode.openParen.SetValue(la.val, la.line, la.col); else pNode.openParen.SetValue(null, 0, 0); pNode.Line = pNode.openParen.Line; pNode.Col = pNode.openParen.Col; 
		Expect(9);
		Assoc_Expression(out exp);
		pNode.expression = exp; 
		if (la.val == ")" && pNode.openParen.Value != null) pNode.closeParen.SetValue(la.val, la.line, la.col); else if (la.val != ")") pNode.openParen.SetValue(null, 0, 0); 
		Expect(10);
		node = pNode; 
	}

	void Assoc_ArrayExprList(out Node node) {
		DesignScript.Parser.Associative.ExprListNode exprlist = new DesignScript.Parser.Associative.ExprListNode(); exprlist.Line = la.line; exprlist.Col = la.col; 
		if (la.val == "{") exprlist.openBrace.SetValue(la.val, la.line, la.col);  
		Expect(50);
		if (StartOf(9)) {
			Assoc_Expression(out node);
			exprlist.list.Add(node); 
			while (la.kind == 52) {
				Get();
				exprlist.AddComma(t.val, t.line, t.col); 
				Assoc_Expression(out node);
				exprlist.list.Add(node); 
			}
		}
		if (la.val == "}" && exprlist.openBrace.Value != null) exprlist.closeBrace.SetValue(la.val, la.line, la.col); else if (la.val != "}") exprlist.openBrace.Value = null; 
		Expect(51);
		node = exprlist; 
	}

	void Assoc_arrayIndices(out DesignScript.Parser.Associative.ArrayNode array) {
		array = new DesignScript.Parser.Associative.ArrayNode(); 
		Node exp = null; 
		if (la.val == "[") array.openBracket.SetValue(la.val, la.line, la.col); array.Line = la.line; array.Col = la.col; 
		Expect(7);
		if (StartOf(9)) {
			Assoc_Expression(out exp);
		}
		array.Expr = exp; 
		array.Type = null;
		
		if (la.val == "]" && array.openBracket.Value != null) array.closeBracket.SetValue(la.val, la.line, la.col); else if (la.val != "]") array.openBracket.Value = null; 
		Expect(8);
		while (la.kind == 7) {
			DesignScript.Parser.Associative.ArrayNode array2 = new DesignScript.Parser.Associative.ArrayNode(); 
			if (la.val == "[") array2.openBracket.SetValue(la.val, la.line, la.col); array2.Line = la.line; array2.Col = la.col; 
			Get();
			exp = null; 
			if (StartOf(9)) {
				Assoc_Expression(out exp);
			}
			array2.Expr = exp; 
			array2.Type = null;
			
			if (la.val == "]" && array2.openBracket.Value != null) array2.closeBracket.SetValue(la.val, la.line, la.col); else if (la.val != "]") array2.openBracket.Value = null; array2.Type = array; array = array2;
			Expect(8);
		}
	}

	void Assoc_arrayIdent(out Node node) {
		Assoc_Ident(out node);
		DesignScript.Parser.Associative.IdentifierNode var = node as DesignScript.Parser.Associative.IdentifierNode; 
		if (la.kind == 7) {
			DesignScript.Parser.Associative.ArrayNode array = null; 
			Assoc_arrayIndices(out array);
			var.ArrayDimensions = array; 
		}
	}

	void Assoc_NameReference(out Node node) {
		node = null; 
		DesignScript.Parser.Associative.ArrayNode array = null; 
		if (la.kind == 9) {
			Assoc_ParenExp(out node);
		} else if (la.kind == 1 || la.kind == 50) {
			if (isFunctionCall()) {
				Assoc_FunctionCall(out node);
			} else if (la.kind == 1) {
				Assoc_arrayIdent(out node);
			} else {
				Assoc_ArrayExprList(out node);
			}
		} else SynErr(106);
		if(hasIndices()) { 
		Assoc_arrayIndices(out array);
		array.Ident = node; node = array; 
		} 
	}

	void Assoc_FunctionCall(out Node node) {
		Assoc_Ident(out node);
		DesignScript.Parser.Associative.FunctionCallNode f = new DesignScript.Parser.Associative.FunctionCallNode(); f.Line = node.Line; f.Col = node.Col; 
		Assoc_Arguments(ref f);
		f.Function = node; 
		node = f; 
	}

	void Imperative_stmt(out Node node) {
		node = null; 
		if (isFunctionCall()) {
			Imperative_functioncall(out node);
			Expect(20);
			DesignScript.Parser.Imperative.FunctionCallNode fcNode = node as DesignScript.Parser.Imperative.FunctionCallNode; 
			fcNode.EndlinePos.setValue(t.col, t.line, t.val); 
			node = fcNode; 
		} else if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Imperative_assignstmt(out node);
		} else if (la.kind == 32) {
			Imperative_ifstmt(out node);
		} else if (la.kind == 35) {
			Imperative_whilestmt(out node);
		} else if (la.kind == 36) {
			Imperative_forloop(out node);
		} else if (la.kind == 7) {
			Imperative_languageblock(out node);
		} else if (la.kind == 45) {
			Get();
			DesignScript.Parser.Imperative.BreakNode bnode = new DesignScript.Parser.Imperative.BreakNode(); 
			if (t.kind == _Kw_break) bnode.KwBreak.setValue(t.col, t.line, t.val); 
			Expect(20);
			if (t.kind == _endline) bnode.EndLine.setValue(t.col, t.line, t.val); 
			node = bnode; 
		} else if (la.kind == 46) {
			Get();
			DesignScript.Parser.Imperative.ContinueNode cnode = new DesignScript.Parser.Imperative.ContinueNode(); 
			if (t.kind == _Kw_continue) cnode.KwContinue.setValue(t.col, t.line, t.val); 
			Expect(20);
			if (t.kind == _endline) cnode.EndLine.setValue(t.col, t.line, t.val); 
			node = cnode; 
		} else if (la.kind == 20) {
			Get();
			DesignScript.Parser.Imperative.IDEHelpNode endlineNode = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			node = endlineNode; 
		} else SynErr(107);
	}

	void Imperative_functiondecl(out Node node) {
		DesignScript.Parser.Imperative.FunctionDefinitionNode funcDecl = new DesignScript.Parser.Imperative.FunctionDefinitionNode(); 
		Expect(25);
		funcDecl.KeywordPos.setValue(t.col, t.line, t.val); 
		funcDecl.Col = t.col; funcDecl.Line = t.line; 
		Expect(1);
		funcDecl.Name = t.val;
		                         funcDecl.NamePos.setValue(t.col, t.line, t.val); 
		if (la.kind == 54) {
			Get();
			funcDecl.TypeColonPos = new DesignScript.Parser.Imperative.IDEHelpNode(); 
			funcDecl.TypeColonPos.setValue(t.col, t.line, t.val); 
			funcDecl.ReturnTypePos = new DesignScript.Parser.Imperative.IDEHelpNode(); 
			if (StartOf(12)) {
				if (la.kind == 38) {
					Get();
				} else if (la.kind == 37) {
					Get();
				} else if (la.kind == 39) {
					Get();
				} else {
					Get();
				}
				funcDecl.ReturnTypePos.Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.KeywordNode; 
			} else if (la.kind == 1) {
				Get();
				funcDecl.ReturnTypePos.Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.TypeNode; 
			} else SynErr(108);
			funcDecl.ReturnType = new ProtoCore.Type() { Name = t.val }; 
			funcDecl.ReturnTypePos.setValue(t.col, t.line, t.val); 
			while (la.kind == 7) {
				string _open = null; int _open_line = 0; int _open_col = 0; string _close = null; if (la.val == "[") { _open = la.val; _open_line = la.line; _open_col = la.col; } 
				Get();
				if(la.val == "]") _close = la.val; 
				Expect(8);
				if (_open != null && _close != null) { funcDecl.Brackets.Add(new DesignScript.Parser.Imperative.IDEHelpNode() { Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode, Value = _open, Line = _open_line, Col = _open_col }); funcDecl.Brackets.Add(new DesignScript.Parser.Imperative.IDEHelpNode() { Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode, Value = _close, Line = t.line, Col = t.col }); } 
			}
		}
		if (la.val == "(") funcDecl.OpenParenPos.setValue(la.col, la.line, la.val); 
		Expect(9);
		if (la.kind == 1) {
			DesignScript.Parser.Imperative.ArgumentSignatureNode args = new DesignScript.Parser.Imperative.ArgumentSignatureNode(); 
			args.Col = t.col;
			args.Line = t.line; 
			Node argdecl = null; 
			Imperative_vardecl(out argdecl);
			args.AddArgument(argdecl as DesignScript.Parser.Imperative.VarDeclNode); 
			while (la.kind == 52) {
				Get();
				DesignScript.Parser.Imperative.IDEHelpNode commaPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };
				funcDecl.ArgCommaPosList.Add(commaPos); 
				Imperative_vardecl(out argdecl);
				args.AddArgument(argdecl as DesignScript.Parser.Imperative.VarDeclNode); 
			}
			funcDecl.Signature = args; 
		}
		if (la.val == ")" && funcDecl.OpenParenPos.Value != null) funcDecl.CloseParenPos.setValue(la.col, la.line, la.val); else if (la.val != ")") funcDecl.OpenParenPos.Value = null; 
		Expect(10);
		isGlobalScope = false; 
		if (la.val == "{") funcDecl.OpenCurlyBracePos.setValue(la.col, la.line, la.val); 
		Expect(50);
		funcDecl.FunctionBody = new DesignScript.Parser.Imperative.CodeBlockNode(); 
		funcDecl.FunctionBody.Col = t.col;
		  funcDecl.FunctionBody.Line = t.line;
		List<Node> body = null;
		
		Imperative_stmtlist(out body);
		if (la.val == "}" && funcDecl.OpenCurlyBracePos.Value != null) funcDecl.CloseCurlyBracePos.setValue(la.col, la.line, la.val); else if (la.val != "}") funcDecl.OpenCurlyBracePos.Value = null; 
		Expect(51);
		funcDecl.localVars = localVarCount;
		funcDecl.FunctionBody.Body = body;
		node = funcDecl; 
		
		isGlobalScope = true;
		localVarCount=  0;
		
	}

	void Imperative_languageblock(out Node node) {
		node = null; 
		DesignScript.Parser.Imperative.LanguageBlockNode langblock = new DesignScript.Parser.Imperative.LanguageBlockNode(); 
		
		if (la.val == "[") langblock.OpenParenPos.setValue(la.col, la.line, la.val); 
		Expect(7);
		langblock.Col = t.col; langblock.Line = t.line; 
		Expect(1);
		if (t.val == "Imperative" || t.val == "Associative") langblock.IdentPos.setValue(t.col, t.line, t.val); 
		if( 0 == t.val.CompareTo("Imperative")) {
		langblock.CodeBlock.language = ProtoCore.Language.kImperative;
		}
		else if( 0 == t.val.CompareTo("Associative")) {
		langblock.CodeBlock.language = ProtoCore.Language.kAssociative; 
		}
		
		while (la.kind == 52) {
			Get();
			string key; 
			DesignScript.Parser.Imperative.IDEHelpNode commaPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };
			langblock.ParaPosList.Add(commaPos); 
			Expect(1);
			key = t.val; 
			DesignScript.Parser.Imperative.IDEHelpNode identPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.IdentNode };
			langblock.ParaPosList.Add(identPos); 
			Expect(53);
			DesignScript.Parser.Imperative.IDEHelpNode assignPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };
			langblock.ParaPosList.Add(assignPos); 
			Expect(4);
			DesignScript.Parser.Imperative.StringNode textstringPos = new DesignScript.Parser.Imperative.StringNode() { Col = t.col, Line = t.line, value = t.val }; 
			langblock.TextStringList.Add(textstringPos); 
			if ("fingerprint" == key)
			{
			langblock.CodeBlock.fingerprint = t.val; 
			langblock.CodeBlock.fingerprint = langblock.CodeBlock.fingerprint.Remove(0,1); 
			langblock.CodeBlock.fingerprint = langblock.CodeBlock.fingerprint.Remove(langblock.CodeBlock.fingerprint.Length-1,1); 
			}
			else if ("version" == key)
			{
			langblock.CodeBlock.version = t.val; 
			langblock.CodeBlock.version = langblock.CodeBlock.version.Remove(0,1); 
			langblock.CodeBlock.version = langblock.CodeBlock.version.Remove(langblock.CodeBlock.version.Length-1,1);
			}
			
		}
		if (la.val == "]" && langblock.OpenParenPos.Value != null) langblock.CloseParenPos.setValue(la.col, la.line, la.val); else if (la.val != "]") langblock.OpenParenPos.Value = null; 
		Expect(8);
		if (la.val == "{") langblock.OpenCurlyBracePos.setValue(la.col, la.line, la.val); 
		Expect(50);
		Node codeBlockNode = null; 
		if (langblock.CodeBlock.language == ProtoCore.Language.kAssociative ) {
			Hydrogen(out codeBlockNode);
		} else if (langblock.CodeBlock.language == ProtoCore.Language.kImperative ) {
			Imperative(out codeBlockNode);
		} else SynErr(109);
		if (langblock.CodeBlock.language == ProtoCore.Language.kInvalid ) {
			int openCurlyBraceCount = 0, closeCurlyBraceCount = 0; 
			DesignScript.Parser.Imperative.CodeBlockNode codeBlockInvalid = new DesignScript.Parser.Imperative.CodeBlockNode(); 
			while (closeCurlyBraceCount <= openCurlyBraceCount) {
				if (la.kind == 7) {
					Imperative_languageblock(out codeBlockNode);
					codeBlockInvalid.Body.Add(codeBlockNode); 
				} else if (la.kind == 50) {
					Get();
					openCurlyBraceCount++; 
				} else if (la.kind == 51) {
					if (la.val == "}" && langblock.OpenCurlyBracePos.Value != null) langblock.CloseCurlyBracePos.setValue(la.col, la.line, la.val); else if(la.val != "}") langblock.OpenCurlyBracePos.Value = null; 
					Get();
					closeCurlyBraceCount++; 
				} else if (la.kind == 0) {
					Get();
					Expect(51);
					break; 
				} else if (true) {
					Get(); 
				} else SynErr(110);
			}
			codeBlockNode = codeBlockInvalid; 
		} else if (true) {
			if (la.val == "}" && langblock.OpenCurlyBracePos.Value != null) langblock.CloseCurlyBracePos.setValue(la.col, la.line, la.val); else if(la.val != "}") langblock.OpenCurlyBracePos.Value = null; 
			Expect(51);
		} else SynErr(111);
		langblock.CodeBlockNode = codeBlockNode; 
		node = langblock; 
	}

	void Imperative_functioncall(out Node node) {
		Expect(1);
		DesignScript.Parser.Imperative.IdentifierNode function = new DesignScript.Parser.Imperative.IdentifierNode() { Value = t.val, Name = t.val, Col = t.col, Line = t.line }; 
		List<Node> arglist = new List<Node>(); 
		                          List<DesignScript.Parser.Imperative.IDEHelpNode> argCommaPosList = new List<DesignScript.Parser.Imperative.IDEHelpNode>();
		DesignScript.Parser.Imperative.IDEHelpNode openParenPos = null; DesignScript.Parser.Imperative.IDEHelpNode closeParenPos = null; 
		if (la.val == "(") openParenPos = new DesignScript.Parser.Imperative.IDEHelpNode()  { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
		Expect(9);
		if (StartOf(18)) {
			Node argNode = null;		 
			Imperative_expr(out argNode);
			arglist.Add(argNode); 
			while (la.kind == 52) {
				Get();
				DesignScript.Parser.Imperative.IDEHelpNode commaPos = new DesignScript.Parser.Imperative.IDEHelpNode()  { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
				argCommaPosList.Add(commaPos); 
				Imperative_expr(out argNode);
				arglist.Add(argNode); 
			}
		}
		if (la.val == ")" && openParenPos != null) closeParenPos = new DesignScript.Parser.Imperative.IDEHelpNode()  { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; else if (la.val != ")") openParenPos = null; 
		Expect(10);
		DesignScript.Parser.Imperative.FunctionCallNode funcNode = new DesignScript.Parser.Imperative.FunctionCallNode(); 
		funcNode.Col = function.Col; 
		funcNode.Line = function.Line;
		                             funcNode.OpenParenPos = openParenPos;
		                             funcNode.CloseParenPos = closeParenPos;
		                             funcNode.ArgCommaPosList = argCommaPosList;
		funcNode.Function = function;
		funcNode.FormalArguments = arglist;
		node = funcNode; 
		
	}

	void Imperative_assignstmt(out Node node) {
		node = null; 
		Node lhsNode = null; 
		isLeft = true; 
		Imperative_DecoratedIdentifier(out lhsNode);
		node = lhsNode; 
		isLeft = false; 
		if (!(lhsNode is DesignScript.Parser.Imperative.PostFixNode)) { 
		DesignScript.Parser.Imperative.IDEHelpNode operatorPos = null; 
		Expect(53);
		if (t.val == "=") operatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
		DesignScript.Parser.Imperative.IDEHelpNode endline = null; 
		Node rhsNode = null; 
		if (la.val == "[") {
			Imperative_languageblock(out rhsNode);
		} else if (HasMoreAssignmentStatements()) {
			Imperative_assignstmt(out rhsNode);
		} else if (StartOf(18)) {
			Imperative_expr(out rhsNode);
			Expect(20);
			if (t.kind == _endline) endline = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
		} else SynErr(112);
		DesignScript.Parser.Imperative.BinaryExpressionNode bNode = new DesignScript.Parser.Imperative.BinaryExpressionNode();
		bNode.LeftNode = lhsNode;
		bNode.RightNode = rhsNode;
		bNode.Optr = Operator.assign;
		bNode.OperatorPos = operatorPos;
		bNode.Col = lhsNode.Col;
		bNode.Line = lhsNode.Line;
		bNode.EndlinePos = endline;
		node = bNode;
		
		} 
	}

	void Imperative_ifstmt(out Node node) {
		DesignScript.Parser.Imperative.IfStmtNode ifStmtNode = new DesignScript.Parser.Imperative.IfStmtNode(); 
		List<Node> body = null; 
		Expect(32);
		ifStmtNode.Col = t.col; ifStmtNode.Line = t.line; 
		ifStmtNode.KeywordPos.setValue(t.col, t.line, t.val); 
		if (la.val == "(") ifStmtNode.OpenParenPos.setValue(la.col, la.line, la.val); 
		Expect(9);
		Imperative_expr(out node);
		ifStmtNode.IfExprNode = node; 
		if (la.val == ")" && ifStmtNode.OpenParenPos.Value != null) 
		ifStmtNode.CloseParenPos.setValue(la.col, la.line, la.val);
		else if (la.val != ")") ifStmtNode.OpenParenPos.Value = null; 
		Expect(10);
		if (la.kind == 50) {
			if (la.val == "{")  ifStmtNode.OpenCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Get();
			Imperative_stmtlist(out body);
			ifStmtNode.IfBody = body; 
			if (la.val == "}" && ifStmtNode.OpenCurlyBracePos != null)  ifStmtNode.CloseCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };  else if (la.val != "}") ifStmtNode.OpenCurlyBracePos = null; 
			Expect(51);
		} else if (StartOf(8)) {
			Node singleStmt = null; 
			Imperative_stmt(out singleStmt);
			ifStmtNode.IfBody.Add(singleStmt); 
		} else SynErr(113);
		while (la.kind == 33) {
			DesignScript.Parser.Imperative.ElseIfBlock elseifBlock = new DesignScript.Parser.Imperative.ElseIfBlock(); 
			Get();
			elseifBlock.Col = t.col; elseifBlock.Line = t.line; 
			elseifBlock.KeywordPos.setValue(t.col, t.line, t.val); 
			if (la.val == "(") elseifBlock.OpenParenPos.setValue(la.col, la.line, la.val); 
			Expect(9);
			Imperative_expr(out node);
			elseifBlock.Expr = node; 
			if (la.val == ")" && elseifBlock.OpenParenPos.Value != null) elseifBlock.CloseParenPos.setValue(la.col, la.line, la.val); else if (la.val != ")") elseifBlock.OpenParenPos.Value = null; 
			Expect(10);
			if (la.kind == 50) {
				if (la.val == "{") elseifBlock.OpenCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
				Get();
				Imperative_stmtlist(out body);
				elseifBlock.Body = body; 
				if (la.val == "}" && elseifBlock.OpenCurlyBracePos != null) elseifBlock.CloseCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; else if(la.val != "}") elseifBlock.CloseCurlyBracePos.Value = null; 
				Expect(51);
			} else if (StartOf(8)) {
				Node singleStmt = null; 
				Imperative_stmt(out singleStmt);
				elseifBlock.Body.Add(singleStmt); 
			} else SynErr(114);
			ifStmtNode.ElseIfList.Add(elseifBlock); 
		}
		if (la.kind == 34) {
			Get();
			ifStmtNode.ElsePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.KeywordNode }; 
			if (la.kind == 50) {
				if (la.val == "{") ifStmtNode.ElseOpenCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
				Get();
				Imperative_stmtlist(out body);
				ifStmtNode.ElseBody = body; 
				if (la.val == "}" && ifStmtNode.ElseOpenCurlyBracePos != null) ifStmtNode.ElseCloseCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; else if (la.val != "}") ifStmtNode.ElseOpenCurlyBracePos = null; 
				Expect(51);
			} else if (StartOf(8)) {
				Node singleStmt = null; 
				Imperative_stmt(out singleStmt);
				ifStmtNode.ElseBody.Add(singleStmt); 
			} else SynErr(115);
		}
		node = ifStmtNode; 
	}

	void Imperative_whilestmt(out Node node) {
		DesignScript.Parser.Imperative.WhileStmtNode whileStmtNode = new DesignScript.Parser.Imperative.WhileStmtNode(); 
		List<Node> body = null; 
		Expect(35);
		whileStmtNode.Col = t.col; whileStmtNode.Line = t.line; 
		whileStmtNode.KeywordPos.setValue(t.col, t.line, t.val); 
		if (la.val == "(") whileStmtNode.OpenParenPos.setValue(la.col, la.line, la.val); 
		Expect(9);
		Imperative_expr(out node);
		whileStmtNode.Expr = node; 
		if (la.val == ")" && whileStmtNode.OpenParenPos.Value != null) whileStmtNode.CloseParenPos.setValue(la.col, la.line, la.val); else if (la.val != ")") whileStmtNode.OpenParenPos.Value = null; 
		Expect(10);
		if (la.val == "{") whileStmtNode.OpenCurlyBracePos.setValue(la.col, la.line, la.val); 
		Expect(50);
		Imperative_stmtlist(out body);
		whileStmtNode.Body = body; 
		if (la.val == "}" && whileStmtNode.OpenCurlyBracePos.Value != null) whileStmtNode.CloseCurlyBracePos.setValue(la.col, la.line, la.val); else if (la.val != "}") whileStmtNode.OpenCurlyBracePos.Value = null; 
		Expect(51);
		node = whileStmtNode; 
	}

	void Imperative_forloop(out Node forloop) {
		Node node = null;	
		DesignScript.Parser.Imperative.ForLoopNode loopNode = new DesignScript.Parser.Imperative.ForLoopNode();										
		List<Node> body = null;   
		
		Expect(36);
		loopNode.Col = t.col; loopNode.Line = t.line; 
		loopNode.KeywordPos.setValue(t.col, t.line, t.val); 
		if (la.val == "(") loopNode.OpenParenPos.setValue(la.col, la.line, la.val);
		Expect(9);
		Imperative_Ident(out node);
		loopNode.id = node; 
		Expect(69);
		loopNode.KwInPos.setValue(t.col, t.line, t.val); 
		Imperative_expr(out node);
		loopNode.expression = node; 
		if (la.val == ")" && loopNode.OpenParenPos.Value != null) loopNode.CloseParenPos.setValue(la.col, la.line, la.val); else if (la.val != ")") loopNode.OpenParenPos.Value = null; 
		Expect(10);
		if (la.kind == 50) {
			if (la.val == "{") loopNode.OpenCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Get();
			Imperative_stmtlist(out body);
			loopNode.body = body; 
			if (la.val == "}" && loopNode.OpenCurlyBracePos != null) loopNode.CloseCurlyBracePos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; else if (la.val != "}") loopNode.OpenCurlyBracePos = null; 
			Expect(51);
		} else if (StartOf(8)) {
			Node singleStmt = null; 
			Imperative_stmt(out singleStmt);
			loopNode.body.Add(singleStmt); 
		} else SynErr(116);
		forloop = loopNode; 
	}

	void Imperative_stmtlist(out List<Node> NodeList) {
		NodeList = new List<Node>(); 
		while (StartOf(8)) {
			Node node = null; 
			Imperative_stmt(out node);
			if (node != null)
			{
			        NodeList.Add(node);
			    } 
			 
		}
	}

	void Imperative_DecoratedIdentifier(out Node node) {
		node = null; 
		if (IsTypedVariable()) {
			Imperative_vardecl(out node);
		} else if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Imperative_identifierList(out node);
		} else SynErr(117);
	}

	void Imperative_expr(out Node node) {
		node = null; 
		Imperative_binexpr(out node);
		while (la.kind == 56) {
			DesignScript.Parser.Imperative.InlineConditionalNode inlineConNode = new DesignScript.Parser.Imperative.InlineConditionalNode(); 
			inlineConNode.ConditionExpression = node; 
			Get();
			if (t.val == "?") inlineConNode.QuestionPos.setValue(t.col, t.line, t.val); 
			node = null; 
			Imperative_expr(out node);
			inlineConNode.TrueExpression = node; 
			Expect(54);
			if (t.val == ":") inlineConNode.ColonPos.setValue(t.col, t.line, t.val); 
			node = null; 
			Imperative_expr(out node);
			inlineConNode.FalseExpression = node; 
			node = inlineConNode; 
		}
	}

	void Imperative_Ident(out Node node) {
		ProtoCore.DSASM.UnaryOperator op = ProtoCore.DSASM.UnaryOperator.None; 
		DesignScript.Parser.Imperative.IdentifierNode var = null;
		
		Expect(1);
		int ltype = (0 == String.Compare(t.val, "return")) ? (int)ProtoCore.PrimitiveType.kTypeReturn : (int)ProtoCore.PrimitiveType.kTypeVar;
		DesignScript.Parser.Imperative.IdentifierNode var2 = new DesignScript.Parser.Imperative.IdentifierNode() 
		{
		// TODO Jun: Move the primitive types into a class table 
		Col = t.col,
		Line = t.line,
		Name = t.val, 
		type = ltype,
		datatype = (ProtoCore.PrimitiveType)ltype 
		}; 
		if (t.val == "return") var2.Return.setValue(t.col, t.line, t.val); else var2.Value = t.val;
		var = var2;
		
		#if ENABLE_INC_DEC_FIX 
		if (la.kind == 66 || la.kind == 67) {
			Imperative_PostFixOp(out op);
			DesignScript.Parser.Imperative.PostFixNode pfNode = new DesignScript.Parser.Imperative.PostFixNode();
			         pfNode.OperatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };
			pfNode.Operator = op;
			pfNode.Identifier = var;
			         pfNode.Line = var.Line;
			         pfNode.Col = var.Col;
			var = pfNode;							
			
		}
		#endif 
		node = var; 
	}

	void Imperative_binexpr(out Node node) {
		node = null;
		Imperative_logicalexpr(out node);
		while (la.kind == 63 || la.kind == 64) {
			Operator op; 
			Imperative_logicalop(out op);
			DesignScript.Parser.Imperative.IDEHelpNode operatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Node rhsNode = null; 
			Imperative_logicalexpr(out rhsNode);
			DesignScript.Parser.Imperative.BinaryExpressionNode bNode = new DesignScript.Parser.Imperative.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			bNode.Col = node.Col;
			bNode.Line = node.Line;
			bNode.OperatorPos = operatorPos;
			node = bNode;
			
		}
	}

	void Imperative_identifierList(out Node node) {
		node = null; 
		
		Imperative_NameReference(out node);
		while (la.kind == 6) {
			Get();
			Node rnode = null; 
			DesignScript.Parser.Imperative.IDEHelpNode dotPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Imperative_NameReference(out rnode);
			DesignScript.Parser.Imperative.IdentifierListNode bnode = new DesignScript.Parser.Imperative.IdentifierListNode(); 
			                                List<DesignScript.Parser.Imperative.IDEHelpNode> dotPosList = new List<DesignScript.Parser.Imperative.IDEHelpNode>();
			dotPosList.Add(dotPos);
			bnode.LeftNode = node; 
			bnode.Optr = Operator.dot; 
			bnode.RightNode = rnode; 
			bnode.Col = node.Col;
			bnode.Line = node.Line;
			                                bnode.DotPosList = dotPosList;
			node = bnode; 
			
		}
	}

	void Imperative_NameReference(out Node node) {
		node = null;
		DesignScript.Parser.Imperative.ArrayNode array = null;   
		if (la.kind == 9) {
			Imperative_parenExp(out node);
		} else if (la.kind == 1 || la.kind == 50) {
			if (isFunctionCall()) {
				Imperative_functioncall(out node);
			} else if (la.kind == 1) {
				Imperative_Ident(out node);
			} else {
				DesignScript.Parser.Imperative.ExprListNode exprlist = new DesignScript.Parser.Imperative.ExprListNode(); 
				if (la.val == "{") exprlist.OpenCurlyBracePos.setValue(la.col, la.line, la.val); 
				Get();
				if (StartOf(18)) {
					Imperative_expr(out node);
					exprlist.list.Add(node); 
					exprlist.Col = t.col;
					exprlist.Line = t.line; 
					while (la.kind == 52) {
						Get();
						DesignScript.Parser.Imperative.IDEHelpNode commaPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };
						exprlist.ExprCommaPosList.Add(commaPos); 
						Imperative_expr(out node);
						exprlist.list.Add(node); 
					}
				}
				if (la.val == "}" && exprlist.OpenCurlyBracePos.Value != null) exprlist.CloseCurlyBracePos.setValue(la.col, la.line, la.val); else if (la.val != "}") exprlist.OpenCurlyBracePos.Value = null; 
				Expect(51);
				node = exprlist; 
			}
		} else SynErr(118);
		if (la.kind == 7) {
			Imperative_arrayIndices(out array);
			array.Ident = node; node = array; 
		}
	}

	void Imperative_parenExp(out Node node) {
		Node exp = null; DesignScript.Parser.Imperative.ParenExpressionNode pNode = new DesignScript.Parser.Imperative.ParenExpressionNode(); 
		if (la.val == "(") pNode.openParen.setValue(la.col, la.line, la.val); else pNode.openParen.setValue(0, 0, null); pNode.Line = pNode.openParen.Line; pNode.Col = pNode.openParen.Col; 
		Expect(9);
		Imperative_expr(out exp);
		pNode.expression = exp; 
		if (la.val == ")" && pNode.openParen.Value != null) pNode.closeParen.setValue(la.col, la.line, la.val); else if (la.val != ")") pNode.openParen.setValue(0, 0, null); 
		Expect(10);
		node = pNode; 
	}

	void Imperative_arrayIndices(out DesignScript.Parser.Imperative.ArrayNode array) {
		array = new DesignScript.Parser.Imperative.ArrayNode(); 
		DesignScript.Parser.Imperative.IDEHelpNode openBracketPos = null; 
		if (la.val == "[") openBracketPos = new DesignScript.Parser.Imperative.IDEHelpNode()  { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
		Expect(7);
		Node node = null; 
		if (StartOf(18)) {
			Imperative_expr(out node);
		}
		array.Col = t.col; 
		array.Line = t.line;
		array.OpenBracketPos = openBracketPos;
		array.Expr = node; 
		array.Type = null;
		
		if (la.val == "]" && array.OpenBracketPos != null) array.CloseBracketPos.setValue(la.col, la.line, la.val); else if (la.val != "]") array.OpenBracketPos = null; 
		Expect(8);
		while (la.kind == 7) {
			DesignScript.Parser.Imperative.IDEHelpNode openBracketPos2 = null; 
			if (la.val == "[") openBracketPos2 = new DesignScript.Parser.Imperative.IDEHelpNode()  { Col = la.col, Line = la.line, Value = la.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Get();
			node = null; 
			if (StartOf(18)) {
				Imperative_expr(out node);
			}
			DesignScript.Parser.Imperative.ArrayNode array2 = new DesignScript.Parser.Imperative.ArrayNode();
			array2.Col = t.col; 
			array2.Line = t.line;
			array2.OpenBracketPos = openBracketPos2;
			array2.Expr = node; 
			array2.Type = array;
			
			
			if (la.val == "]" && array2.OpenBracketPos != null) array2.CloseBracketPos.setValue(la.col, la.line, la.val); else if (la.val != "]") array2.OpenBracketPos = null;  
			Expect(8);
			array = array2; 
		}
	}

	void Imperative_vardecl(out Node node) {
		DesignScript.Parser.Imperative.VarDeclNode varDeclNode = new DesignScript.Parser.Imperative.VarDeclNode(); 
		Expect(1);
		varDeclNode.Line = t.line; varDeclNode.Col = t.col; varDeclNode.name.setValue(t.col, t.line, t.val); 
		if (t.kind == _ident) varDeclNode.name.setValue(t.col, t.line, t.val); 
		if (la.kind == 54) {
			DesignScript.Parser.Imperative.TypeNode IDEType = new DesignScript.Parser.Imperative.TypeNode(); 
			Imperative_TypeRestriction(out IDEType);
			varDeclNode.IDEArgumentType = IDEType; 
		}
		if (la.kind == 53) {
			Get();
			if (t.val == "=") varDeclNode.equal.setValue(t.col, t.line, t.val); 
			Node rhsNode = null; 
			Imperative_expr(out rhsNode);
			varDeclNode.NameNode = rhsNode; 
		}
		node = varDeclNode; 
	}

	void Imperative_unaryexpr(out Node node) {
		DesignScript.Parser.Imperative.UnaryExpressionNode an = new DesignScript.Parser.Imperative.UnaryExpressionNode(); 
		node = null; 
		if (StartOf(15)) {
			Imperative_NegExpression(out an);
		} else if (StartOf(16)) {
			Imperative_BitUnaryExpression(out an);
		} else SynErr(119);
		node = an; 
	}

	void Imperative_NegExpression(out DesignScript.Parser.Imperative.UnaryExpressionNode node) {
		node = new Imperative.UnaryExpressionNode() ; 
		DesignScript.Parser.Imperative.IDEHelpNode op = node.Operator; 
		Node exprNode = null; 
		Imperative_negop(out op);
		if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Imperative_identifierList(out exprNode);
		} else if (la.kind == 9) {
			Get();
			Imperative_expr(out exprNode);
			Expect(10);
		} else SynErr(120);
		DesignScript.Parser.Imperative.UnaryExpressionNode unary = new DesignScript.Parser.Imperative.UnaryExpressionNode(); 
		unary.Operator = op;
		unary.Expression = exprNode;
		node = unary;
		
		                 
	}

	void Imperative_BitUnaryExpression(out DesignScript.Parser.Imperative.UnaryExpressionNode node) {
		DesignScript.Parser.Imperative.UnaryExpressionNode unary = new DesignScript.Parser.Imperative.UnaryExpressionNode(); 
		DesignScript.Parser.Imperative.IDEHelpNode IDEop = unary.Operator; 
		Imperative_unaryop(out IDEop);
		Node exprNode = null; 
		Imperative_factor(out exprNode);
		unary.Expression = exprNode; 
		unary.Operator = IDEop; 
		node = unary; 
	}

	void Imperative_unaryop(out DesignScript.Parser.Imperative.IDEHelpNode op) {
		op = new DesignScript.Parser.Imperative.IDEHelpNode(DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode);  
		if (la.kind == 11) {
			Get();
			op.setValue(t.col, t.line, t.val);  
		} else if (la.kind == 65) {
			Get();
			op.setValue(t.col, t.line, t.val);  
			#if ENABLE_INC_DEC_FIX 
		} else if (la.kind == 66 || la.kind == 67) {
			Imperative_PostFixOp(ref op);
			#endif 
		} else SynErr(121);
		#if ENABLE_INC_DEC_FIX
		#else
		if (la.val == "++" || la.val == "--") Get();
		#endif
		
	}

	void Imperative_factor(out Node node) {
		node = null; 
		if (IsNumber()) {
			Imperative_num(out node);
		} else if (isFunctionCall()) {
			Imperative_functioncall(out node);
		} else if (la.kind == 47) {
			Get();
			node = new DesignScript.Parser.Imperative.BooleanNode()  {Col = t.col, Line = t.line, value = ProtoCore.DSASM.Literal.True }; 
		} else if (la.kind == 48) {
			Get();
			node = new DesignScript.Parser.Imperative.BooleanNode() { Col = t.col, Line = t.line, value = ProtoCore.DSASM.Literal.False }; 
		} else if (la.kind == 49) {
			Get();
			node = new DesignScript.Parser.Imperative.NullNode() { Col = t.col, Line = t.line }; 
		} else if (la.kind == 5) {
			Imperative_Char(out node);
		} else if (la.kind == 4) {
			Imperative_String(out node);
		} else if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
			Imperative_identifierList(out node);
		} else if (StartOf(17)) {
			Imperative_unaryexpr(out node);
		} else if (la.kind == 7) {
			Imperative_languageblock(out node);
		} else SynErr(122);
	}

	void Imperative_negop(out DesignScript.Parser.Imperative.IDEHelpNode op) {
		op = new DesignScript.Parser.Imperative.IDEHelpNode(DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode); 
		if (la.kind == 1 || la.kind == 9 || la.kind == 50) {
		} else if (la.kind == 12) {
			Get();
			op.setValue(t.col, t.line, t.val); 
		} else SynErr(123);
	}

	void Imperative_logicalexpr(out Node node) {
		node = null;
		Imperative_RangeExpr(out node);
		while (StartOf(14)) {
			Operator op; 
			Imperative_relop(out op);
			DesignScript.Parser.Imperative.IDEHelpNode operatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Node rhsNode = null; 
			Imperative_RangeExpr(out rhsNode);
			DesignScript.Parser.Imperative.BinaryExpressionNode bNode = new DesignScript.Parser.Imperative.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			bNode.Col = node.Col;
			bNode.Line = node.Line;
			bNode.OperatorPos = operatorPos;
			node = bNode;
			
		}
	}

	void Imperative_logicalop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 63) {
			Get();
			op = Operator.and; 
		} else if (la.kind == 64) {
			Get();
			op = Operator.or; 
		} else SynErr(124);
	}

	void Imperative_RangeExpr(out Node node) {
		Imperative_rel(out node);
		if (la.kind == 21) {
			DesignScript.Parser.Imperative.RangeExprNode rnode = new DesignScript.Parser.Imperative.RangeExprNode();										
			rnode.FromNode = node;
			rnode.Col = node.Col;
			rnode.Line = node.Line;
			
			Get();
			rnode.FirstRangeOpPos.setValue(t.col, t.line, t.val); 
			Imperative_rel(out node);
			rnode.ToNode = node; 
			if (!(t.val == "..")&&(la.val == "..")) { 
			if (la.kind == 21) {
				RangeStepOperator op; 
				Get();
				rnode.SecondRangeOpPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
				Imperative_rangeStepOperator(out op);
				if (op != RangeStepOperator.stepsize)
				{
				 rnode.StepOpPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode };
				} 
				rnode.stepoperator = op; 
				Imperative_rel(out node);
				rnode.StepNode = node; 
			}
			} 
			node = rnode; 
		}
	}

	void Imperative_relop(out Operator op) {
		op = Operator.none; 
		switch (la.kind) {
		case 15: {
			Get();
			op = Operator.gt; 
			break;
		}
		case 14: {
			Get();
			op = Operator.lt; 
			break;
		}
		case 17: {
			Get();
			op = Operator.ge; 
			break;
		}
		case 16: {
			Get();
			op = Operator.le; 
			break;
		}
		case 18: {
			Get();
			op = Operator.eq; 
			break;
		}
		case 19: {
			Get();
			op = Operator.nq; 
			break;
		}
		default: SynErr(125); break;
		}
	}

	void Imperative_rel(out Node node) {
		node = null;
		Imperative_term(out node);
		while (la.kind == 12 || la.kind == 57) {
			Operator op; 
			Imperative_addop(out op);
			DesignScript.Parser.Imperative.IDEHelpNode operatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Node rhsNode = null; 
			Imperative_term(out rhsNode);
			DesignScript.Parser.Imperative.BinaryExpressionNode bNode = new DesignScript.Parser.Imperative.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			bNode.Col = node.Col;
			bNode.Line = node.Line;
			bNode.OperatorPos = operatorPos;
			node = bNode;
			
		}
	}

	void Imperative_rangeStepOperator(out RangeStepOperator op) {
		op = RangeStepOperator.stepsize; 
		if (la.kind == 65 || la.kind == 68) {
			if (la.kind == 68) {
				Get();
				op = RangeStepOperator.num; 
			} else {
				Get();
				op = RangeStepOperator.approxsize; 
			}
		}
	}

	void Imperative_term(out Node node) {
		node = null;
		Imperative_interimfactor(out node);
		while (la.kind == 58 || la.kind == 59 || la.kind == 60) {
			Operator op; 
			Imperative_mulop(out op);
			DesignScript.Parser.Imperative.IDEHelpNode operatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Node rhsNode = null; 
			Imperative_interimfactor(out rhsNode);
			DesignScript.Parser.Imperative.BinaryExpressionNode bNode = new DesignScript.Parser.Imperative.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			bNode.Col = node.Col;
			bNode.Line = node.Line;
			bNode.OperatorPos = operatorPos;
			node = bNode;
			
		}
	}

	void Imperative_addop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 57) {
			Get();
			op = Operator.add; 
		} else if (la.kind == 12) {
			Get();
			op = Operator.sub; 
		} else SynErr(126);
	}

	void Imperative_interimfactor(out Node node) {
		node = null;
		Imperative_factor(out node);
		while (la.kind == 13 || la.kind == 61 || la.kind == 62) {
			Operator op; 
			Imperative_bitop(out op);
			DesignScript.Parser.Imperative.IDEHelpNode operatorPos = new DesignScript.Parser.Imperative.IDEHelpNode() { Col = t.col, Line = t.line, Value = t.val, Type = DesignScript.Parser.Imperative.IDEHelpNode.HelpNodeType.PunctuationNode }; 
			Node rhsNode = null; 
			Imperative_factor(out rhsNode);
			DesignScript.Parser.Imperative.BinaryExpressionNode bNode = new DesignScript.Parser.Imperative.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			bNode.Col = node.Col;
			bNode.Line = node.Line;
			bNode.OperatorPos = operatorPos;
			node = bNode;
			
		}
	}

	void Imperative_mulop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 58) {
			Get();
			op = Operator.mul; 
		} else if (la.kind == 59) {
			Get();
			op = Operator.div; 
		} else if (la.kind == 60) {
			Get();
			op = Operator.mod; 
		} else SynErr(127);
	}

	void Imperative_bitop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 61) {
			Get();
			op = Operator.bitwiseand; 
		} else if (la.kind == 13) {
			Get();
			op = Operator.bitwiseor; 
		} else if (la.kind == 62) {
			Get();
			op = Operator.bitwisexor; 
		} else SynErr(128);
	}

	void Imperative_Char(out Node node) {
		node = null; 
		Expect(5);
		DesignScript.Parser.Imperative.CharNode _node = null;
		if (t.val.Length <= 2) {
		errors.SemErr(t.line, t.col, "Empty character literal.");
		}
		if (t.kind == _char)
		{
		_node = new DesignScript.Parser.Imperative.CharNode() { value = t.val};
		_node.IDEValue.setValue(t.line, t.col, t.val);
		_node.Line = t.line;
		_node.Col = t.col;
		}
		node = _node;
		
	}

	void Imperative_String(out Node node) {
		node = null; 
		Expect(4);
		DesignScript.Parser.Imperative.StringNode _node = null;
		node = new DesignScript.Parser.Imperative.StringNode(); 
		if (t.kind == _textstring)
		{
		_node = new DesignScript.Parser.Imperative.StringNode() { value = t.val };
		_node.IDEValue.setValue(t.line, t.col, t.val);
		_node.Line = t.line;
		_node.Col = t.col;
		}
		node = _node;
		
	}

	void Imperative_num(out Node node) {
		node = null; DesignScript.Parser.Imperative.IntNode INode = new DesignScript.Parser.Imperative.IntNode(); 
		if (la.kind == 12 || la.kind == 57) {
			if (la.kind == 12) {
				Get();
				if (t.val == "-") INode.SignPos.setValue(t.col, t.line, t.val); 
			} else {
				Get();
			}
			if (t.val == "+") INode.SignPos.setValue(t.col, t.line, t.val); 
		}
		if (la.kind == 2) {
			Get();
			if (t.kind == _number) INode.IDEValue.setValue(t.col, t.line, t.val); node = INode;
		} else if (la.kind == 3) {
			Get();
			if (t.kind == _float) { DesignScript.Parser.Imperative.DoubleNode DNode = new DesignScript.Parser.Imperative.DoubleNode(); DNode.SignPos = INode.SignPos; DNode.IDEValue.setValue(t.col, t.line, t.val); node = DNode; }  
		} else SynErr(129);
	}

	void Imperative_PostFixOp(ref DesignScript.Parser.Imperative.IDEHelpNode op) {
		if (la.kind == 66) {
			Get();
			op.setValue(t.col, t.line, t.val); 
		} else if (la.kind == 67) {
			Get();
			op.setValue(t.col, t.line, t.val); 
		} else SynErr(130);
	}

	void Imperative_TypeRestriction(out DesignScript.Parser.Imperative.TypeNode IDEType) {
		IDEType = new DesignScript.Parser.Imperative.TypeNode(); 
		ExpectWeak(54, 11);
		if (t.val == ":") IDEType.colon.setValue(t.col, t.line, t.val); 
		if (StartOf(12)) {
			if (la.kind == 37) {
				Get();
			} else if (la.kind == 38) {
				Get();
			} else if (la.kind == 39) {
				Get();
			} else {
				Get();
			}
			IDEType.BuildInTypeSetValue(t.val, t.line, t.col); 
		} else if (la.kind == 1) {
			Get();
			IDEType.UserDefinedTypeSetValue(t.val, t.line, t.col); 
		} else SynErr(131);
		while (la.kind == 7) {
			string openBracket = null; string closeBracket = null; 
			if (la.val == "[")  openBracket = la.val; int bracket_line = la.line; int bracket_col = la.col; 
			Get();
			if (la.val == "]") closeBracket = la.val; 
			Expect(8);
			if (openBracket != null && closeBracket != null) IDEType.AddBracket(openBracket, bracket_line, bracket_col, closeBracket, t.line, t.col); 
			if (la.kind == 21) {
				Get();
				IDEType.op.setValue(t.col, t.line, t.val); 
				string multiDimOpenBracket = null; string multiDimCloseBracket = null; 
				if (la.val == "[")  multiDimOpenBracket = la.val; int bracket2_line = la.line; int bracket2_col = la.col; 
				Expect(7);
				if (la.val == "]")  multiDimCloseBracket = la.val; 
				Expect(8);
				if (multiDimOpenBracket != null && multiDimCloseBracket != null) 
				IDEType.AddMultiDimNodes(multiDimOpenBracket, bracket2_line, bracket2_col, multiDimCloseBracket, t.line, t.col); 
			}
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		DesignScriptParser();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,T,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,T,T,x, T,T,T,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,x,T, x,T,x,T, T,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,T,T,T, T,x,x,T, T,T,T,T, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,T, T,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,x,x,T, T,x,x,x, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,x,T, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,T,T,x, T,T,T,x, x,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,x},
		{x,T,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,x},
		{x,T,T,T, T,T,x,T, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,T,T, x,x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = null;          // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
        if (null != errorStream)
        {
		    string s;
		    switch (n)
            {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "float expected"; break;
			case 4: s = "textstring expected"; break;
			case 5: s = "char expected"; break;
			case 6: s = "period expected"; break;
			case 7: s = "openbracket expected"; break;
			case 8: s = "closebracket expected"; break;
			case 9: s = "openparen expected"; break;
			case 10: s = "closeparen expected"; break;
			case 11: s = "not expected"; break;
			case 12: s = "neg expected"; break;
			case 13: s = "pipe expected"; break;
			case 14: s = "lessthan expected"; break;
			case 15: s = "greaterthan expected"; break;
			case 16: s = "lessequal expected"; break;
			case 17: s = "greaterequal expected"; break;
			case 18: s = "equal expected"; break;
			case 19: s = "notequal expected"; break;
			case 20: s = "endline expected"; break;
			case 21: s = "rangeop expected"; break;
			case 22: s = "kw_native expected"; break;
			case 23: s = "kw_class expected"; break;
			case 24: s = "kw_constructor expected"; break;
			case 25: s = "kw_def expected"; break;
			case 26: s = "kw_external expected"; break;
			case 27: s = "kw_extend expected"; break;
			case 28: s = "kw_public expected"; break;
			case 29: s = "kw_private expected"; break;
			case 30: s = "kw_protected expected"; break;
			case 31: s = "kw_heap expected"; break;
			case 32: s = "kw_if expected"; break;
			case 33: s = "kw_elseif expected"; break;
			case 34: s = "kw_else expected"; break;
			case 35: s = "kw_while expected"; break;
			case 36: s = "kw_for expected"; break;
			case 37: s = "Kw_double expected"; break;
			case 38: s = "Kw_int expected"; break;
			case 39: s = "Kw_var expected"; break;
			case 40: s = "Kw_function expected"; break;
			case 41: s = "Kw_import expected"; break;
			case 42: s = "Kw_from expected"; break;
			case 43: s = "Kw_prefix expected"; break;
			case 44: s = "Kw_static expected"; break;
			case 45: s = "Kw_break expected"; break;
			case 46: s = "Kw_continue expected"; break;
			case 47: s = "literal_true expected"; break;
			case 48: s = "literal_false expected"; break;
			case 49: s = "literal_null expected"; break;
			case 50: s = "\"{\" expected"; break;
			case 51: s = "\"}\" expected"; break;
			case 52: s = "\",\" expected"; break;
			case 53: s = "\"=\" expected"; break;
			case 54: s = "\":\" expected"; break;
			case 55: s = "\"=>\" expected"; break;
			case 56: s = "\"?\" expected"; break;
			case 57: s = "\"+\" expected"; break;
			case 58: s = "\"*\" expected"; break;
			case 59: s = "\"/\" expected"; break;
			case 60: s = "\"%\" expected"; break;
			case 61: s = "\"&\" expected"; break;
			case 62: s = "\"^\" expected"; break;
			case 63: s = "\"&&\" expected"; break;
			case 64: s = "\"||\" expected"; break;
			case 65: s = "\"~\" expected"; break;
			case 66: s = "\"++\" expected"; break;
			case 67: s = "\"--\" expected"; break;
			case 68: s = "\"#\" expected"; break;
			case 69: s = "\"in\" expected"; break;
			case 70: s = "??? expected"; break;
			case 71: s = "invalid DesignScriptParser"; break;
			case 72: s = "this symbol not expected in Import_Statement"; break;
			case 73: s = "invalid Import_Statement"; break;
			case 74: s = "this symbol not expected in Assoc_Statement"; break;
			case 75: s = "invalid Assoc_Statement"; break;
			case 76: s = "this symbol not expected in Assoc_FunctionDecl"; break;
			case 77: s = "invalid Assoc_FunctionDecl"; break;
			case 78: s = "this symbol not expected in Assoc_ClassDecl"; break;
			case 79: s = "this symbol not expected in Assoc_ClassDecl"; break;
			case 80: s = "invalid Assoc_ClassDecl"; break;
			case 81: s = "invalid Assoc_LanguageBlock"; break;
			case 82: s = "invalid Assoc_LanguageBlock"; break;
			case 83: s = "invalid Assoc_LanguageBlock"; break;
			case 84: s = "invalid Hydrogen"; break;
			case 85: s = "this symbol not expected in Assoc_FunctionalStatement"; break;
			case 86: s = "this symbol not expected in Assoc_FunctionalStatement"; break;
			case 87: s = "this symbol not expected in Assoc_FunctionalStatement"; break;
			case 88: s = "invalid Assoc_FunctionalStatement"; break;
			case 89: s = "invalid Imperative"; break;
			case 90: s = "invalid Assoc_TypeRestriction"; break;
			case 91: s = "this symbol not expected in Assoc_ArgumentSignatureDefinition"; break;
			case 92: s = "invalid Assoc_BinaryOps"; break;
			case 93: s = "invalid Assoc_AddOp"; break;
			case 94: s = "invalid Assoc_MulOp"; break;
			case 95: s = "invalid Assoc_ComparisonOp"; break;
			case 96: s = "invalid Assoc_LogicalOp"; break;
			case 97: s = "invalid Assoc_DecoratedIdentifier"; break;
			case 98: s = "invalid Assoc_UnaryExpression"; break;
			case 99: s = "invalid Assoc_NegExpression"; break;
			case 100: s = "invalid Assoc_unaryop"; break;
			case 101: s = "invalid Assoc_Factor"; break;
			case 102: s = "invalid Assoc_negop"; break;
			case 103: s = "invalid Assoc_BitOp"; break;
			case 104: s = "invalid Associative_PostFixOp"; break;
			case 105: s = "invalid Assoc_Number"; break;
			case 106: s = "invalid Assoc_NameReference"; break;
			case 107: s = "invalid Imperative_stmt"; break;
			case 108: s = "invalid Imperative_functiondecl"; break;
			case 109: s = "invalid Imperative_languageblock"; break;
			case 110: s = "invalid Imperative_languageblock"; break;
			case 111: s = "invalid Imperative_languageblock"; break;
			case 112: s = "invalid Imperative_assignstmt"; break;
			case 113: s = "invalid Imperative_ifstmt"; break;
			case 114: s = "invalid Imperative_ifstmt"; break;
			case 115: s = "invalid Imperative_ifstmt"; break;
			case 116: s = "invalid Imperative_forloop"; break;
			case 117: s = "invalid Imperative_DecoratedIdentifier"; break;
			case 118: s = "invalid Imperative_NameReference"; break;
			case 119: s = "invalid Imperative_unaryexpr"; break;
			case 120: s = "invalid Imperative_NegExpression"; break;
			case 121: s = "invalid Imperative_unaryop"; break;
			case 122: s = "invalid Imperative_factor"; break;
			case 123: s = "invalid Imperative_negop"; break;
			case 124: s = "invalid Imperative_logicalop"; break;
			case 125: s = "invalid Imperative_relop"; break;
			case 126: s = "invalid Imperative_addop"; break;
			case 127: s = "invalid Imperative_mulop"; break;
			case 128: s = "invalid Imperative_bitop"; break;
			case 129: s = "invalid Imperative_num"; break;
			case 130: s = "invalid Imperative_PostFixOp"; break;
			case 131: s = "invalid Imperative_TypeRestriction"; break;

            default: s = "error " + n; break;
            }

            errorStream.WriteLine(errMsgFormat, line, col, s);
        }

		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
        if (null != errorStream)
            errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
        if (null != errorStream)
            errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
        if (null != errorStream)
            errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
        if (null != errorStream)
            errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}