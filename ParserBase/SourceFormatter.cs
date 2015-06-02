// 
//  ParseTreeBuilder.cs
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
	public enum SourceFormatterType { LineByLine, EntireFile }
	
	public class SourceFormatter
	{
		public SourceFormatterType WorkingType;
		
		public string SourceFileName;
		public string EOFSymbolName;
				
		public int LineNumberStartsFrom;
		public int ColumnNumberStartsFrom;
				
		private List<SymbolRegEx> SymbolRegExList;
		private List<SymbolRegEx> SkipSymbolRegExList;
		
		public bool AddNewSymbol(string symbolName, string symbolPattern)
		{
			if (SymbolRegExList.FindAll(s => s.Name == symbolName).Count > 0) return false;
			SymbolRegExList.Add(new SymbolRegEx(symbolName, symbolPattern)); return true;
		}
		
		public bool AddNewSkipSymbol(string symbolName, string symbolPattern)
		{
			if (SkipSymbolRegExList.FindAll(s => s.Name == symbolName).Count > 0) return false;
			SkipSymbolRegExList.Add(new SymbolRegEx(symbolName, symbolPattern)); return true;
		}
			
		public void ToDefault()
		{
			WorkingType = SourceFormatterType.EntireFile;
			
			EOFSymbolName = "EndOfFileMarker";
			SourceFileName = "";
			
			LineNumberStartsFrom = 1;
			ColumnNumberStartsFrom = 1;
			SymbolRegExList = new List<SymbolRegEx>();
			SkipSymbolRegExList = new List<SymbolRegEx>();
		}
				
		public SourceFormatter()
		{ ToDefault(); }
		
		public SourceFormatter(List<SymbolRegEx> symbolRegExList, List<SymbolRegEx> skipSymbolRegExList)
		{
			ToDefault();
			SymbolRegExList.AddRange(symbolRegExList);
			SkipSymbolRegExList.AddRange(skipSymbolRegExList);
		}
		
		public SourceFormatter(string sourceFileName, List<SymbolRegEx> symbolRegExList, List<SymbolRegEx> skipSymbolRegExList)
		{
			ToDefault();
			SourceFileName = sourceFileName;
			SymbolRegExList.AddRange(symbolRegExList);
			SkipSymbolRegExList.AddRange(skipSymbolRegExList);
		}
						
		public List<Symbol> SourceToSymbolListEntireFile(string sourceFileName, List<SymbolRegEx> symbolRegExList, int LineNumberStartsFrom = 1, int ColumnNumberStartsFrom = 1)
		{
			StreamReader sr = new StreamReader(sourceFileName);
			string currentLine = sr.ReadToEnd();
			sr.Close();
			
			List<Symbol> result = new List<Symbol>();
			List<Symbol> matchedSymbols = new List<Symbol>();
			
			int currentLineNumber = LineNumberStartsFrom;
			int currentColumnNumber = ColumnNumberStartsFrom;
			
			while (currentLine.Length > 0)
			{
				matchedSymbols = new List<Symbol>();
				Symbol matchedSymbol = new Symbol();
				
				foreach (SymbolRegEx sre in symbolRegExList)
					matchedSymbols.Add(new Symbol(sre.Name, SymbolType.Terminal, new Regex(@"^(" + sre.Pattern + ")").Match(currentLine).Value));
				
				matchedSymbol = new Symbol();
				matchedSymbols.ForEach(ms => { if (matchedSymbol.Value.Length < ms.Value.Length) matchedSymbol = new Symbol(ms); } );
								
				if (matchedSymbol.Value.Length == 0)
					throw new Exception("Unable to handel\n#\n" + currentLine + "\n#");
				
				matchedSymbol.LineNumber = currentLineNumber;
				matchedSymbol.ColumnNumber = currentColumnNumber;
				
				result.Add(matchedSymbol);
				
				int startIndex = matchedSymbol.Value.IndexOf("\n", 0);
				
				if (startIndex == -1)
					currentColumnNumber += matchedSymbol.Value.Length;
				else
				{
					while (startIndex != -1)
					{
						currentLineNumber++;
						currentColumnNumber = LineNumberStartsFrom;
						startIndex = matchedSymbol.Value.IndexOf("\n", startIndex + 1);
					}
					
					currentColumnNumber = matchedSymbol.Value.Length - matchedSymbol.Value.LastIndexOf("\n") - 1 + ColumnNumberStartsFrom;
				}
					
				currentLine = currentLine.Remove(0, matchedSymbol.Value.Length);
			}
			
			result.Add(new Symbol(EOFSymbolName, SymbolType.Terminal, ""));
			result[result.Count - 1].LineNumber = currentLineNumber;
			result[result.Count - 1].ColumnNumber = currentColumnNumber;
			return result;
		}
		
						
		public List<Symbol> SourceToSymbolList(string sourceCodeString, List<SymbolRegEx> symbolRegExList, int LineNumberStartsFrom = 1, int ColumnNumberStartsFrom = 1)
		{
			
			string currentLine = sourceCodeString;
						
			List<Symbol> result = new List<Symbol>();
			List<Symbol> matchedSymbols = new List<Symbol>();
			
			int currentLineNumber = LineNumberStartsFrom;
			int currentColumnNumber = ColumnNumberStartsFrom;
			
			while (currentLine.Length > 0)
			{
				matchedSymbols = new List<Symbol>();
				Symbol matchedSymbol = new Symbol();
				
				foreach (SymbolRegEx sre in symbolRegExList)
					matchedSymbols.Add(new Symbol(sre.Name, SymbolType.Terminal, new Regex(@"^(" + sre.Pattern + ")").Match(currentLine).Value));
				
				matchedSymbol = new Symbol();
				matchedSymbols.ForEach(ms => { if (matchedSymbol.Value.Length < ms.Value.Length) matchedSymbol = new Symbol(ms); } );
								
				if (matchedSymbol.Value.Length == 0)
					throw new Exception("Unable to handel\n#\n" + currentLine + "\n#");
				
				matchedSymbol.LineNumber = currentLineNumber;
				matchedSymbol.ColumnNumber = currentColumnNumber;
				
				result.Add(matchedSymbol);
				
				int startIndex = matchedSymbol.Value.IndexOf("\n", 0);
				
				if (startIndex == -1)
					currentColumnNumber += matchedSymbol.Value.Length;
				else
				{
					while (startIndex != -1)
					{
						currentLineNumber++;
						currentColumnNumber = LineNumberStartsFrom;
						startIndex = matchedSymbol.Value.IndexOf("\n", startIndex + 1);
					}
					
					currentColumnNumber = matchedSymbol.Value.Length - matchedSymbol.Value.LastIndexOf("\n") - 1 + ColumnNumberStartsFrom;
				}
					
				currentLine = currentLine.Remove(0, matchedSymbol.Value.Length);
			}
			
			result.Add(new Symbol(EOFSymbolName, SymbolType.Terminal, ""));
			result[result.Count - 1].LineNumber = currentLineNumber;
			result[result.Count - 1].ColumnNumber = currentColumnNumber;
			return result;
		}
		
		public List<Symbol> SymbolList
		{
			get
			{
				List<Symbol> result = SourceToSymbolListEntireFile(SourceFileName, SymbolRegExList, LineNumberStartsFrom, ColumnNumberStartsFrom);
				
				result.RemoveAll(r => SkipSymbolRegExList.FindAll(s => s.Name.Equals(r.Name)).Count > 0);
				
				return result;
				
			}
		}
	}
	
	/// <summary>
	/// Symbol regular expression list.
	/// </summary>
	public class SymbolRegEx
	{
		public string Name;
		private string RegExPattern;
		
		/// <summary>
		/// Gets the pattern.
		/// </summary>
		/// <value>
		/// The pattern.
		/// </value>
		public string Pattern { get { return RegExPattern; } }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.SymbolRegEx"/> class.
		/// </summary>
		/// <param name='symbolName'>
		/// Symbol name.
		/// </param>
		/// <param name='pattern'>
		/// Pattern.
		/// </param>
		public SymbolRegEx (string symbolName, string pattern)
		{
			Name = symbolName;
			RegExPattern = pattern;
		}
		
		/// <summary>
		/// Match the specified symbol and source.
		/// </summary>
		/// <param name='symbol'>
		/// Empty symbol.
		/// </param>
		/// <param name='source'>
		/// Source.
		/// </param>
		public bool Match(ref Symbol symbol, string source)
		{
			Regex tempRegEx = new Regex(Pattern);
			
			if (tempRegEx.IsMatch(source))
			{
				symbol.Name = Name;
				symbol.Value = tempRegEx.Match(source).Value;
				
				if (symbol.Value.Equals(source)) return true;
				else symbol = new Symbol();
				
				return false;
			}
			
			return false;
		}
		
		/// <summary>
		/// Match the specified source.
		/// </summary>
		/// <param name='source'>
		/// Source.
		/// </param>
		public Match Match(string source)
		{ return new Regex(this.Pattern).Match(source); }
		
	}
	
}


























