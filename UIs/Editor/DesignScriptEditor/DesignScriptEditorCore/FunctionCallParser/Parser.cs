
//#define ENABLE_INC_DEC_FIX
using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.AST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using DesignScript.Editor.Core;

namespace FunctionCallParser {



// Since the parser is generated, we don't want the 
// compiler to make noise about it not being CLS-compliant
#pragma warning disable 3008

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
	public const int _kw_heap = 28;
	public const int _kw_if = 29;
	public const int _kw_elseif = 30;
	public const int _kw_else = 31;
	public const int _kw_while = 32;
	public const int _kw_for = 33;
	public const int _kw_import = 34;
	public const int _kw_prefix = 35;
	public const int _kw_from = 36;
	public const int _kw_break = 37;
	public const int _kw_continue = 38;
	public const int _kw_static = 39;
	public const int _literal_true = 40;
	public const int _literal_false = 41;
	public const int _literal_null = 42;
	public const int maxT = 56;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

    FunctionCallPart rootFunctionCallPart = null;

    public FunctionCallPart RootFunctionCallPart
    {
        get { return this.rootFunctionCallPart; }
    }


	
	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

    System.Drawing.Point PointFromToken(Token token, bool includeToken)
    {
        if (false != includeToken)
            return (new System.Drawing.Point(token.col - 1, token.line - 1));

        int x = token.col + token.val.Length - 1;
        return (new System.Drawing.Point(x, token.line - 1));
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

	
	void FunctionCallParser() {
		rootFunctionCallPart = new FunctionCallPart(); 
		CommonExpression(rootFunctionCallPart);
	}

	void CommonExpression(FunctionCallPart part) {
		if (StartOf(1)) {
			CommonLogicalExpression(part);
		} else if (la.kind == 51) {
			CommonTernaryOperation(part);
		} else SynErr(57);
	}

	void CommonLogicalExpression(FunctionCallPart part) {
		CommonComparisonExpression(part);
		while (la.kind == 49 || la.kind == 50) {
			CommonLogicalOperator(part);
			CommonComparisonExpression(part);
		}
	}

	void CommonTernaryOperation(FunctionCallPart part) {
		Expect(51);
		CommonExpression(part);
		Expect(52);
		CommonExpression(part);
	}

	void CommonComparisonExpression(FunctionCallPart part) {
		CommonRangeExpression(part);
		while (StartOf(2)) {
			CommonComparisonOperator(part);
			CommonRangeExpression(part);
		}
	}

	void CommonLogicalOperator(FunctionCallPart part) {
		if (la.kind == 49) {
			Get();
		} else if (la.kind == 50) {
			Get();
		} else SynErr(58);
	}

	void CommonRangeExpression(FunctionCallPart part) {
		CommonArithmeticExpression(part);
		if (la.kind == 21) {
			Get();
			CommonArithmeticExpression(part);
			if (la.kind == 21) {
				Get();
				if (la.kind == 43 || la.kind == 44) {
					if (la.kind == 43) {
						Get();
					} else {
						Get();
					}
				}
				CommonArithmeticExpression(part);
			}
		}
	}

	void CommonComparisonOperator(FunctionCallPart part) {
		switch (la.kind) {
		case 15: {
			Get();
			break;
		}
		case 17: {
			Get();
			break;
		}
		case 14: {
			Get();
			break;
		}
		case 16: {
			Get();
			break;
		}
		case 18: {
			Get();
			break;
		}
		case 19: {
			Get();
			break;
		}
		default: SynErr(59); break;
		}
	}

	void CommonArithmeticExpression(FunctionCallPart part) {
		CommonTerm(part);
		while (StartOf(3)) {
			CommonMathOperator(part);
			CommonTerm(part);
		}
	}

	void CommonTerm(FunctionCallPart part) {
		switch (la.kind) {
		case 40: {
			Get();
			break;
		}
		case 41: {
			Get();
			break;
		}
		case 42: {
			Get();
			break;
		}
		case 5: {
			CommonCharacter(part);
			break;
		}
		case 4: {
			CommonString(part);
			break;
		}
		case 1: case 2: case 3: case 12: case 53: {
			CommonNegativeExpression(part);
			break;
		}
		case 11: {
			Get();
			CommonTerm(part);
			break;
		}
		default: SynErr(60); break;
		}
	}

	void CommonMathOperator(FunctionCallPart part) {
		if (la.kind == 45) {
			Get();
		} else if (la.kind == 12) {
			Get();
		} else if (la.kind == 46) {
			Get();
		} else if (la.kind == 47) {
			Get();
		} else if (la.kind == 48) {
			Get();
		} else SynErr(61);
	}

	void CommonCharacter(FunctionCallPart part) {
		Expect(5);
	}

	void CommonString(FunctionCallPart part) {
		Expect(4);
	}

	void CommonNegativeExpression(FunctionCallPart part) {
		if (la.kind == 12) {
			Get();
			part.AppendIdentifier(t); 
		}
		if (la.kind == 2 || la.kind == 3) {
			if (la.kind == 2) {
				Get();
			} else {
				Get();
			}
			part.AppendIdentifier(t); 
		} else if (la.kind == 1 || la.kind == 53) {
			CommonIdentifierList(part);
		} else SynErr(62);
	}

	void CommonIdentifierList(FunctionCallPart part) {
		string partName = string.Empty;  
		CommonNameReference(part);
		partName = part.Identifier;      
		while (la.kind == 6) {
			Get();
			CommonNameReference(part);
			string newPartName = part.Identifier;
			part.Identifier = partName + "." + newPartName;
			partName = part.Identifier;
			
		}
	}

	void CommonNameReference(FunctionCallPart part) {
		if (la.kind == 1) {
			CommonFunctionCall(part);
		} else if (la.kind == 53) {
			CommonArrayExpression(part);
		} else SynErr(63);
		if (la.kind == 7) {
			Get();
			part.AppendIdentifier(t); part.SetEndPoint(t, false); 
			if (StartOf(4)) {
				CommonExpression(part);
				part.SetEndPoint(t, false); 
			}
			Expect(8);
			part.AppendIdentifier(t); part.SetEndPoint(t, false); 
			while (la.kind == 7) {
				Get();
				part.AppendIdentifier(t); part.SetEndPoint(t, false); 
				if (StartOf(4)) {
					CommonExpression(part);
					part.SetEndPoint(t, false); 
				}
				Expect(8);
				part.AppendIdentifier(t); part.SetEndPoint(t, false); 
			}
		}
	}

	void CommonFunctionCall(FunctionCallPart part) {
		CommonIdentifier(part);
		while (la.kind == 9) {
			CommonArguments(part);
		}
	}

	void CommonArrayExpression(FunctionCallPart part) {
		Expect(53);
		part.SetStartPoint(t, false);                           
		if (StartOf(4)) {
			FunctionCallPart elementPart = new FunctionCallPart();  
			CommonExpression(elementPart);
			part.AddArgumentPart(elementPart);                      
			while (la.kind == 54) {
				Get();
				elementPart = new FunctionCallPart();                   
				CommonExpression(elementPart);
				part.AddArgumentPart(elementPart);                      
			}
		}
		Expect(55);
		part.SetEndPoint(t, true);                              
	}

	void CommonIdentifier(FunctionCallPart part) {
		Expect(1);
		part.Identifier = t.val; 
	}

	void CommonArguments(FunctionCallPart part) {
		Expect(9);
		part.SetStartPoint(t, false);
		part.SetEndPoint(la, true);
		System.Drawing.Point openBracket = PointFromToken(t, false);
		
		if (StartOf(4)) {
			FunctionCallPart parentCallPart = part;
			part = new FunctionCallPart();
			part.SetStartPoint(t, false);
			
			CommonExpression(part);
			part.SetEndPoint(t, false);
			parentCallPart.AddArgumentPart(part);
			part = parentCallPart;
			
			while (WeakSeparator(54,4,5) ) {
				parentCallPart = part;
				part = new FunctionCallPart();
				part.SetStartPoint(t, false);
				
				CommonExpression(part);
				part.SetEndPoint(la, true);
				parentCallPart.AddArgumentPart(part);
				part = parentCallPart;
				
			}
		}
		if (part.HasArgument == false) {
		   // See "AddDefaultArgument" for details.
		   System.Drawing.Point closeBracket = PointFromToken(la, true);
		   part.AddDefaultArgument(openBracket, closeBracket);
		}
		
		Expect(10);
		part.SetEndPoint(t, true); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		FunctionCallParser();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x,x,x,x, x,x,x,T, x,T,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
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
			case 28: s = "kw_heap expected"; break;
			case 29: s = "kw_if expected"; break;
			case 30: s = "kw_elseif expected"; break;
			case 31: s = "kw_else expected"; break;
			case 32: s = "kw_while expected"; break;
			case 33: s = "kw_for expected"; break;
			case 34: s = "kw_import expected"; break;
			case 35: s = "kw_prefix expected"; break;
			case 36: s = "kw_from expected"; break;
			case 37: s = "kw_break expected"; break;
			case 38: s = "kw_continue expected"; break;
			case 39: s = "kw_static expected"; break;
			case 40: s = "literal_true expected"; break;
			case 41: s = "literal_false expected"; break;
			case 42: s = "literal_null expected"; break;
			case 43: s = "\"#\" expected"; break;
			case 44: s = "\"~\" expected"; break;
			case 45: s = "\"+\" expected"; break;
			case 46: s = "\"*\" expected"; break;
			case 47: s = "\"/\" expected"; break;
			case 48: s = "\"%\" expected"; break;
			case 49: s = "\"&&\" expected"; break;
			case 50: s = "\"||\" expected"; break;
			case 51: s = "\"?\" expected"; break;
			case 52: s = "\":\" expected"; break;
			case 53: s = "\"{\" expected"; break;
			case 54: s = "\",\" expected"; break;
			case 55: s = "\"}\" expected"; break;
			case 56: s = "??? expected"; break;
			case 57: s = "invalid CommonExpression"; break;
			case 58: s = "invalid CommonLogicalOperator"; break;
			case 59: s = "invalid CommonComparisonOperator"; break;
			case 60: s = "invalid CommonTerm"; break;
			case 61: s = "invalid CommonMathOperator"; break;
			case 62: s = "invalid CommonNegativeExpression"; break;
			case 63: s = "invalid CommonNameReference"; break;

			default: s = "error " + n; break;
		}
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		count++;
	}
	
	public virtual void SemErr (string s) {
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
	}
	
	public virtual void Warning(string s) {
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}

#pragma warning restore 3008
}