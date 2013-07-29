
using System;

namespace DesignScript.Editor.Core {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _period = 5;
	public const int _openbracket = 6;
	public const int _closebracket = 7;
	public const int _openparen = 8;
	public const int _closeparen = 9;
	public const int _not = 10;
	public const int _neg = 11;
	public const int _pipe = 12;
	public const int _lessthan = 13;
	public const int _greaterthan = 14;
	public const int _lessequal = 15;
	public const int _greaterequal = 16;
	public const int _equal = 17;
	public const int _notequal = 18;
	public const int _endline = 19;
	public const int _rangeop = 20;
	public const int _and = 21;
	public const int _or = 22;
	public const int _comment1 = 23;
	public const int _comment2 = 24;
	public const int _comment3 = 25;
	public const int _newline = 26;
	public const int _kw_native = 27;
	public const int _kw_class = 28;
	public const int _kw_constructor = 29;
	public const int _kw_def = 30;
	public const int _kw_external = 31;
	public const int _kw_extend = 32;
	public const int _kw_heap = 33;
	public const int _kw_if = 34;
	public const int _kw_elseif = 35;
	public const int _kw_else = 36;
	public const int _kw_while = 37;
	public const int _kw_for = 38;
	public const int _kw_import = 39;
	public const int _kw_prefix = 40;
	public const int _kw_from = 41;
	public const int _kw_break = 42;
	public const int _kw_continue = 43;
	public const int _kw_static = 44;
	public const int _literal_true = 45;
	public const int _literal_false = 46;
	public const int _literal_null = 47;
	public const int maxT = 48;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
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

	
	void DesignScriptParser() {
		Expect(1);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		
		DesignScriptParser();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "float expected"; break;
			case 4: s = "textstring expected"; break;
			case 5: s = "period expected"; break;
			case 6: s = "openbracket expected"; break;
			case 7: s = "closebracket expected"; break;
			case 8: s = "openparen expected"; break;
			case 9: s = "closeparen expected"; break;
			case 10: s = "not expected"; break;
			case 11: s = "neg expected"; break;
			case 12: s = "pipe expected"; break;
			case 13: s = "lessthan expected"; break;
			case 14: s = "greaterthan expected"; break;
			case 15: s = "lessequal expected"; break;
			case 16: s = "greaterequal expected"; break;
			case 17: s = "equal expected"; break;
			case 18: s = "notequal expected"; break;
			case 19: s = "endline expected"; break;
			case 20: s = "rangeop expected"; break;
			case 21: s = "and expected"; break;
			case 22: s = "or expected"; break;
			case 23: s = "comment1 expected"; break;
			case 24: s = "comment2 expected"; break;
			case 25: s = "comment3 expected"; break;
			case 26: s = "newline expected"; break;
			case 27: s = "kw_native expected"; break;
			case 28: s = "kw_class expected"; break;
			case 29: s = "kw_constructor expected"; break;
			case 30: s = "kw_def expected"; break;
			case 31: s = "kw_external expected"; break;
			case 32: s = "kw_extend expected"; break;
			case 33: s = "kw_heap expected"; break;
			case 34: s = "kw_if expected"; break;
			case 35: s = "kw_elseif expected"; break;
			case 36: s = "kw_else expected"; break;
			case 37: s = "kw_while expected"; break;
			case 38: s = "kw_for expected"; break;
			case 39: s = "kw_import expected"; break;
			case 40: s = "kw_prefix expected"; break;
			case 41: s = "kw_from expected"; break;
			case 42: s = "kw_break expected"; break;
			case 43: s = "kw_continue expected"; break;
			case 44: s = "kw_static expected"; break;
			case 45: s = "literal_true expected"; break;
			case 46: s = "literal_false expected"; break;
			case 47: s = "literal_null expected"; break;
			case 48: s = "??? expected"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}