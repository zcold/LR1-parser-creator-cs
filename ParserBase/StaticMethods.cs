// 
//  ParsingTableBuilder.cs
//  
//  Author:
//       Shuo Li <shuol@kth.se>
//  
//  Copyright (c) 2011 Shuo Li
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace ParserBase
{
	/// <summary>
	/// Static methods to build various of classes
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public static class Build
	{
		/// <summary>
		/// Build a parsing table state pool based on the specified grammar.
		/// </summary>
		/// <returns>
		/// The parsig table state pool.
		/// </returns>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public static ParsingTableStatePool StatePool(Grammar grammar)
		{
			if (grammar.Count == 0)
				throw new Exception("Given grammar is empty.");
			
			ParsingTableState rootState = new ParsingTableState();
			rootState.Index = 0;
			rootState.Add(new DotProduction(grammar.First(),0));
			rootState.Complete(grammar);
			
			ParsingTableStatePool result = new ParsingTableStatePool();
			result.Add(rootState);
			
			int LastStateCount = 0;
			int LastEdgeCount = 0;
			
			while ((LastEdgeCount != result.Edges.Count) || (LastStateCount != result.Count))
			{
				LastStateCount = result.Count;
				LastEdgeCount = result.Edges.Count;
				
				result.Evolve(grammar);
			}
			
			return result;
		}
			
		/// <summary>
		/// Build a parsing table based on the specified grammar.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		public static ParsingTable Table(Grammar grammar)
		{
			ParsingTableStatePool statePool = StatePool(grammar);
			return Table(grammar, statePool);
		}	
	
		/// <summary>
		/// Build a parsing table based on the specified grammar.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		public static ParsingTable Table(Grammar grammar, ParsingTableStatePool statePool)
		{
			ParsingTable result = new ParsingTable();
			List<int> AcceptStateIndices = statePool.AcceptStateIndices(grammar.First());
			
			List<string> NonTerminalNames = new List<string>();
			grammar.ForEach(p => NonTerminalNames.Add(p.From));
			
			for(int i = 0; i < NonTerminalNames.Count; i++)
				for(int j = i + 1; j < NonTerminalNames.Count; j++)
					if (NonTerminalNames[i].Equals(NonTerminalNames[j]))
						NonTerminalNames.RemoveAt(j--);
			
			NonTerminalNames.RemoveAll(s => s.Equals(grammar.First().From));
			
			List<string> TerminalNames = new List<string>();
			grammar.ForEach(p => TerminalNames.AddRange(p.Derivation));
			
			for(int i = 0; i < TerminalNames.Count; i++)
				for(int j = i + 1; j < TerminalNames.Count; j++)
					if (TerminalNames[i].Equals(TerminalNames[j]))
						TerminalNames.RemoveAt(j--);				
						
			for(int i = 0; i < TerminalNames.Count; i++)
				if (NonTerminalNames.Contains(TerminalNames[i]))
					TerminalNames.RemoveAt(i--);
			
			result.SymbolList = new List<string>();
			result.SymbolList.AddRange(TerminalNames);
			result.SymbolList.AddRange(NonTerminalNames);
			
			foreach (StateEdge e in statePool.Edges)
			{
				if (AcceptStateIndices.Contains(e.Destination))
					result[e.Source, e.SymbolName] = new ParsingAction(ParsingActionType.Accept, 0);
				else
					if (TerminalNames.Contains(e.SymbolName))
						result[e.Source, e.SymbolName] = new ParsingAction(ParsingActionType.Shift, e.Destination);
					else
						result[e.Source, e.SymbolName] = new ParsingAction(ParsingActionType.Goto, e.Destination);
								
				if (statePool[e.Destination].IsComplete)
					for(int i = 0; i < grammar.Count; i++)
						if (statePool[e.Destination][0].IsFrom(grammar[i]))
						{
							result[e.Destination, e.SymbolName] = new ParsingAction(ParsingActionType.Reduce, i);
							break;
						}				
			}
			
			return result;
		}	
	
		/// <summary>
		/// Build a table-like string based on the specified content (2D string list).
		/// </summary>
		/// <param name='content'>
		/// Content.
		/// </param>
		public static string Table (List<List<string>> content)
		{
			string result = string.Empty;
			int max = 0;
			
			content.ForEach(sl => sl.ForEach( s => max = s.Length > max ? s.Length : max));
			
			max++;
			
			foreach (List<string> sl in content)
			{
				foreach (string s in sl)
				{	
					result += s;
					for(int i = 0; i < max - s.Length; i++)
						result += " ";
				}
				
				result += "\n";
			}
			
			return result;
		}
		
		/// <summary>
		/// Build a table-like string based on the specified content (2D string list).
		/// </summary>
		/// <param name='content'>
		/// Content.
		/// </param>
		public static string Table (List<List<string>> content, string Separator)
		{
			string result = string.Empty;
			
			foreach (List<string> sl in content)
			{
				foreach (string s in sl)
				{	
					result += s + Separator;				
				}
				result.Remove(result.Length - 1 - Separator.Length, Separator.Length);
				result += "\n";
			}
			
			return result;
		}
		
		public static Grammar GrammarFromSpecTree (ParsingTree specTree)
		{
			// Get grammar for nonterminals
			List<ParsingTree> productionTreeList = GetSymbolByName(specTree, "PRODUCTION");
			Grammar result = new Grammar();
			productionTreeList.ForEach(t => result.Add(new Production(TreeString(t, " ").Remove(@";"))));
			return result;
		}
		
		public static ParsingTree ParingTreeFromSpecFile (string specFileName)
		{
			RegexLib TerminalRegexLib = new RegexLib();
			TerminalRegexLib.Add("ConfigurationStart", @"configurations");
			TerminalRegexLib.Add("ProductionStart", @"production");
			TerminalRegexLib.Add("ProductionPrefix", @"Production");
			TerminalRegexLib.Add("RegExPrefix", @"RegEx");
			TerminalRegexLib.Add("EOF", @"endOfFileMarker");
			TerminalRegexLib.Add("SKIP", @"skip");
			TerminalRegexLib.Add("Regex", @"regex");
			TerminalRegexLib.Add("Whitespace", @"[^\n\S]+");
			TerminalRegexLib.Add("Return", @"\n");
			TerminalRegexLib.Add("OpenB", @"{");
			TerminalRegexLib.Add("CloseB", @"}");
			TerminalRegexLib.Add("Equal", @"\=");
			TerminalRegexLib.Add("Semicolon", @";");
			TerminalRegexLib.Add("FormattedString", @"(?<!@)\042[^\042\n]*\042");
			TerminalRegexLib.Add("RegularExpressionPattern", @"@\042[^\042]*\042(?=;)");			
			TerminalRegexLib.Add("Identifier", @"[_a-zA-Z]+[_a-zA-Z0-9]*");
			TerminalRegexLib.Add("Comment", @"//[^\n]*");
			TerminalRegexLib.Add("BlockComment", @"/\*(((?!\*/).)*[\r\n]?)*\*/");
			
			List<SymbolRegEx> symbolRegExList = new List<SymbolRegEx>();
			List<SymbolRegEx> skipSymbolRegExList = new List<SymbolRegEx>();
			
			foreach (string k in TerminalRegexLib.Keys)
				symbolRegExList.Add(new SymbolRegEx(k, TerminalRegexLib[k].ToString()));
			
			skipSymbolRegExList.Add(new SymbolRegEx("Return", TerminalRegexLib["Return"].ToString()));
			skipSymbolRegExList.Add(new SymbolRegEx("Whitespace", TerminalRegexLib["Whitespace"].ToString()));
			skipSymbolRegExList.Add(new SymbolRegEx("Comment", TerminalRegexLib["Comment"].ToString()));
			skipSymbolRegExList.Add(new SymbolRegEx("BlockComment", TerminalRegexLib["BlockComment"].ToString()));
							
			Grammar g = new Grammar();
			g.Add(new Production("CFGFILE = CONFIGURATIONBODY PRODUCTIONBODY EndOfFileMarker"));
			g.Add(new Production("PRODUCTIONBODY = ProductionStart OpenB STATEMENT CloseB"));
			g.Add(new Production("PRODUCTIONBODY = ProductionStart OpenB CloseB"));
			g.Add(new Production("STATEMENT = STATEMENT PRODUCTION"));
			g.Add(new Production("STATEMENT = STATEMENT REGEX"));
			g.Add(new Production("STATEMENT = PRODUCTION"));
			g.Add(new Production("STATEMENT = REGEX"));		
			g.Add(new Production("PRODUCTION = Identifier Equal DERIVATION Semicolon"));
			g.Add(new Production("DERIVATION = Identifier"));
			g.Add(new Production("DERIVATION = DERIVATION Identifier"));		
			g.Add(new Production("REGEX = Identifier Equal RegularExpressionPattern Semicolon"));
			g.Add(new Production("CONFIGURATIONBODY = ConfigurationStart OpenB EOFSTATEMENT SKIPSTATEMENT CloseB"));
			g.Add(new Production("CONFIGURATIONBODY = ConfigurationStart OpenB SKIPSTATEMENT EOFSTATEMENT CloseB"));
			g.Add(new Production("EOFSTATEMENT = EOF Equal Identifier Semicolon"));
			g.Add(new Production("SKIPSTATEMENT = SKIP Equal DERIVATION Semicolon"));
						
			LRParser parser = new LRParser(g);
			StreamReader sr = new StreamReader(specFileName);
			string specString = sr.ReadToEnd();
			sr.Close();
			
			SourceFormatter sf = new SourceFormatter();
			sf.EOFSymbolName = "EndOfFileMarker";
			
			List<Symbol> sl = sf.SourceToSymbolList(specString, symbolRegExList);
			sl.RemoveAll(r => skipSymbolRegExList.FindAll(s => s.Name.Equals(r.Name)).Count > 0);
			
			return parser.GetParsingTree(sl);
		}
		
		public static RegexLib TerminalRegularExpressionListFromSpecTree(ParsingTree specTree)
		{
			// Get regular expression for terminals
			List<ParsingTree> resultRegexTreeList = GetSymbolByName(specTree, "REGEX");
			RegexLib result = new RegexLib();
			
			foreach (ParsingTree tree in resultRegexTreeList)
			{
				string stringValue = TreeString(tree, "");
				string keyName = stringValue.Split('=')[0];
				
				string regexPattern = stringValue.Substring(stringValue.IndexOf("=") + 1);
				regexPattern = regexPattern.Remove(0, 2);
				regexPattern = regexPattern.Remove(regexPattern.Length - 2, 2);
				result.Add(keyName, new Regex(regexPattern));
			}
			
			return result;	
		}
		
		public static string EOFSymbolNameFromSpecTree(ParsingTree specTree)
		{
			// Get resultEOFSymbolName			
			string result = TreeString(GetSymbolByName(specTree, "EOFSTATEMENT")[0], "").Substring(TreeString(GetSymbolByName(specTree, "EOFSTATEMENT")[0], "").IndexOf("=") + 1);
			result = result.Remove(result.Length - 1);
			return result;
		}
		
		public static List<string> SkipSymbolListFromSpecTree(ParsingTree specTree)
		{
			// Get SKIPSTATEMENT
			string resultSkipString = TreeString(GetSymbolByName(specTree, "SKIPSTATEMENT")[0], " ");
			resultSkipString = resultSkipString.Substring(resultSkipString.IndexOf("=") + 2);
			resultSkipString = resultSkipString.Remove(resultSkipString.Length - 4);
			List<string> result = resultSkipString.Split(' ').ToList();
			result.RemoveAll(s => s.Length == 0);
			return result;
		}
		
		public static ParsingTree ParsingTreeFromFile (string sourceFileName, string specFileName)
		{
			ParsingTree pt = ParingTreeFromSpecFile(specFileName);
			
			Grammar resultGrammar = GrammarFromSpecTree(pt);
						
			string resultEOFSymbolName = EOFSymbolNameFromSpecTree(pt);
			List<string> resultSkipSymbolNameList = SkipSymbolListFromSpecTree(pt);
			
			RegexLib resultTerminalRegExLib = TerminalRegularExpressionListFromSpecTree(pt);
			
			// Build symbol regex list			
			List<SymbolRegEx> resultSymbolRegExList = new List<SymbolRegEx>();
			foreach (string k in resultTerminalRegExLib.Keys)
				resultSymbolRegExList.Add(new SymbolRegEx(k, resultTerminalRegExLib[k].ToString()));
			
			List<SymbolRegEx> resultSkipSymbolRegExList = new List<SymbolRegEx>();
			foreach (string k in resultSkipSymbolNameList)
				resultSkipSymbolRegExList.Add(new SymbolRegEx(k, resultTerminalRegExLib[k].ToString()));				
		
			// Parse
			SourceFormatter resultSF = new SourceFormatter();
			resultSF.EOFSymbolName = resultEOFSymbolName;
			StreamReader sr = new StreamReader(sourceFileName);
			List<Symbol> resultSL = resultSF.SourceToSymbolList(sr.ReadToEnd(), resultSymbolRegExList);
			sr.Close();
			resultSL.RemoveAll(r => resultSkipSymbolRegExList.FindAll(s => s.Name.Equals(r.Name)).Count > 0);
			
			LRParser resultParser = new LRParser(resultGrammar);
			ParsingTree resultPT = resultParser.GetParsingTree(resultSL);
						
			return resultPT;
		}
				
		
		public static string GetValueOf(ParsingTree parsingTree, string outterName, string innerName, char Separator)
		{
			string result = string.Empty;
			
			if (parsingTree.RootSymbol.Name.Equals(outterName))
				parsingTree.Children.ForEach(c => {result += GetValueOf(c, outterName, innerName, Separator);});		
			
			if (parsingTree.RootSymbol.Name.Equals(innerName))
				result += parsingTree.RootSymbol.Value + Separator;
									
			return result;
		}
				
		public static List<ParsingTree> GetSymbolByName(ParsingTree parsingTree, string symbolName)
		{
			List<ParsingTree> result = new List<ParsingTree>();
			
			
			if (parsingTree.RootSymbol.Name.Equals(symbolName))
				result.Add(parsingTree);
			else
				if (!parsingTree.IsLeaf)
					parsingTree.Children.ForEach(c => result.AddRange(GetSymbolByName(c, symbolName)));
						
			return result;
		}
				
		public static string TreeString(ParsingTree tree, string Separator)
		{
			string result = string.Empty;
		
			if (tree.IsLeaf)	
				result = tree.RootSymbol.Value;
			else
				tree.Children.ForEach(ct => result += TreeString(ct, Separator) + Separator);
			
			return result;
		}
	}
	
	/// <summary>
	/// String ext method.
	/// </summary>
	public static class StringExtMethod
	{
		public static string Remove(this string s, Regex removeRegex)
		{ return s.Replace(removeRegex, string.Empty); }
		
		public static string Remove(this string s, string removePattern)
		{ return s.Replace(new Regex(removePattern), string.Empty); }
		
		public static string Remove(this string s, List<string> removePatternList)
		{
			string result = string.Empty + s;
			removePatternList.ForEach( r => result = result.Remove(r) );			
			return result;
		}
		
		public static string Replace(this string s, Regex replaceRegexp, string replacement)
		{ return replaceRegexp.Replace(s, replacement); }
	}
	
	
}

