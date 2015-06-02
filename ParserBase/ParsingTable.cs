// 
//  ParsingAction.cs
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

namespace ParserBase
{
	/// <summary>
	/// Parsing action type. Reduce, Shift, Goto, Unassigned, Accept.
	/// </summary>
	public enum ParsingActionType {Reduce, Shift, Goto, Unassigned, Accept}
	
	/// <summary>
	/// Parsing action.
	/// </summary>
	public class ParsingAction
	{
		public ParsingActionType ActionType;
		public int Index;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingAction"/> class.
		/// </summary>
		public ParsingAction()
		{
			ActionType = ParsingActionType.Unassigned;
			Index = -1;			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingAction"/> class.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='index'>
		/// Index.
		/// </param>
		public ParsingAction(ParsingActionType type, int index)
		{
			ActionType = type;
			Index = index;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingAction"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingAction"/>.
		/// </returns>
		public override string ToString ()
		{
			switch (ActionType)
			{
				case ParsingActionType.Accept: return "Accept";
				case ParsingActionType.Unassigned: return string.Empty;
				default: return string.Format ("{0} {1}", ActionType.ToString(), Index);
			}			
		}
	}
	
	/// <summary>
	/// Parsing table.
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public class ParsingTable
	{
		/// <summary>
		/// Expecteds the symbol names.
		/// </summary>
		/// <returns>
		/// The symbol names.
		/// </returns>
		/// <param name='currentState'>
		/// Current state.
		/// </param>
		public List<string> ExpectedSymbolNames(int currentState)
		{
			if (table.Count <= currentState)
				return new List<string>();
			
			List<string> result = new List<string>();
			
			foreach (string s in  table[currentState].Keys)
				if (!this[currentState, s].ActionType.Equals(ParsingActionType.Unassigned)) result.Add(s);
			
			return result;
		}
		
		List<Dictionary<string, ParsingAction>> table;
		
		public bool StrictSymbolList;
		
		/// <summary>
		/// Gets or sets the state count.
		/// </summary>
		/// <value>
		/// The state count.
		/// </value>
		public int StateCount
		{
			get { return table.Count; }
			set
			{
				table = new List<Dictionary<string, ParsingAction>>(); 
				for (int i = 0; i < value; i++) 
					table.Add(new Dictionary<string, ParsingAction>());
			}
		}
		
		private List<string> _SymbolList;
		
		/// <summary>
		/// Gets or sets the symbol list.
		/// </summary>
		/// <value>
		/// The symbol list.
		/// </value>
		public List<string> SymbolList
		{
			get { return _SymbolList; }
			set
			{
				_SymbolList = new List<string>();
				_SymbolList.AddRange(value);
				
				for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
				{
					foreach(string s in _SymbolList)
						if (!table[stateIndex].ContainsKey(s))					
							table[stateIndex].Add(s, new ParsingAction());
				}
			}
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTable"/> class.
		/// </summary>
		public ParsingTable()
		{
			StateCount = 0;
			SymbolList = new List<string>();
			StrictSymbolList = false;
		}
		
		/// <summary>
		/// Gets or sets the <see cref="ParserBase.ParsingTable"/> with the specified stateIndex currentSymbol.
		/// </summary>
		/// <param name='stateIndex'>
		/// State index.
		/// </param>
		/// <param name='currentSymbol'>
		/// Current symbol.
		/// </param>
		public ParsingAction this[int stateIndex, string currentSymbol]
		{
			get { return table[stateIndex][currentSymbol]; }
			set
			{
				while(table.Count < stateIndex + 1)
					table.Add(new Dictionary<string, ParsingAction>());
				
				for(int i = 0; i < table.Count; i++)
					SymbolList.ForEach(s => { if (!table[i].ContainsKey(s)) table[i].Add(s, new ParsingAction(ParsingActionType.Unassigned, 0)); } );
				
				if (table[stateIndex].ContainsKey(currentSymbol)) 
					table[stateIndex].Remove(currentSymbol);
				if (_SymbolList.Contains(currentSymbol))
					table[stateIndex].Add(currentSymbol, value);
				else
				{
					if (StrictSymbolList) throw new Exception("Symbol is not contained in the symbol list.");
					
					List<string> temp = _SymbolList;
					temp.Add(currentSymbol);
					SymbolList = temp;
					table[stateIndex].Remove(currentSymbol);
					table[stateIndex].Add(currentSymbol, value);
				}
			}
		}
		
		/// <summary>
		/// To string.
		/// </summary>
		/// <returns>
		/// The string.
		/// </returns>
		/// <param name='Separator'>
		/// Separator.
		/// </param>
		public string ToString (string Separator)
		{
			string result = "[Parsing table]\n";
			List<List<string>> content = new List<List<string>>();
			content.Add(new List<string>(){"Symbol"});
			
			foreach (string key in _SymbolList)
				content[0].Add(key);
						
			for (int stateIndex = 0; stateIndex < table.Count; stateIndex++)
			{
				content.Add(new List<string>(){"State " + stateIndex});
				for (int symbolIndex = 1; symbolIndex < content[0].Count; symbolIndex++)
					if (table[stateIndex].ContainsKey(content[0][symbolIndex]))
						content[stateIndex + 1].Add(this[stateIndex, content[0][symbolIndex]].ToString());
					else
						content[stateIndex + 1].Add(new ParsingAction().ToString());
			}
						
			return result + Build.Table(content, Separator) + "[End of parsing table]\n";
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTable"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTable"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = "[Parsing table]\n";
			List<List<string>> content = new List<List<string>>();
			content.Add(new List<string>(){"Symbol"});
			
			foreach (string key in _SymbolList)
				content[0].Add(key);
						
			for (int stateIndex = 0; stateIndex < table.Count; stateIndex++)
			{
				content.Add(new List<string>(){"State " + stateIndex});
				for (int symbolIndex = 1; symbolIndex < content[0].Count; symbolIndex++)
					if (table[stateIndex].ContainsKey(content[0][symbolIndex]))
						content[stateIndex + 1].Add(this[stateIndex, content[0][symbolIndex]].ToString());
					else
						content[stateIndex + 1].Add(new ParsingAction().ToString());
			}
						
			return result + Build.Table(content) + "[End of parsing table]\n";
		}
	}
	
	/// <summary>
	/// Detailed parsing table.
	/// </summary>
	public class DetailedParsingTable
	{
		public ParsingTable Table;
		public ParsingTableStatePool StatePool;
		
		public DetailedParsingTable() {}
		
		public DetailedParsingTable(Grammar grammar)
		{
			this.StatePool = Build.StatePool(grammar);
			this.Table = Build.Table(grammar, this.StatePool);
		}
	}

}

