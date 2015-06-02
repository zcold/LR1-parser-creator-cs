// 
//  Stack.cs
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
	/// Stack entry.
	/// </summary>
	public class StackEntry
	{
		/// <summary>
		/// The index of the state.
		/// </summary>
		public int StateIndex;
		
		/// <summary>
		/// The symbol.
		/// </summary>
		public ParsingTree ParsingTreeNode;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.StackEntry"/> class.
		/// </summary>
		public StackEntry ()
		{
			StateIndex = -1;
			ParsingTreeNode = new ParsingTree();			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.StackEntry"/> class.
		/// </summary>
		/// <param name='stateIndex'>
		/// State index.
		/// </param>
		public StackEntry (int stateIndex)
		{
			StateIndex = stateIndex;
			ParsingTreeNode = new ParsingTree();			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.StackEntry"/> class.
		/// </summary>
		/// <param name='stateIndex'>
		/// State index.
		/// </param>
		/// <param name='parseTreeNode'>
		/// Parse tree node.
		/// </param>
		public StackEntry (int stateIndex, ParsingTree parseTreeNode)
		{
			StateIndex = stateIndex;
			ParsingTreeNode= parseTreeNode;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.StackEntry"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.StackEntry"/>.
		/// </returns>
		public override string ToString ()
		{
			 return string.Format("[Stack entry] state: {0}, symbol: {1}", StateIndex, ParsingTreeNode.RootSymbol.Name);
		}
		
	}
	
	/// <summary>
	/// Parse tree stack.
	/// </summary>
	public class ParsingTreeStack : List<StackEntry>
	{
		/// <summary>
		/// Gets the current state;
		/// </summary>
		/// <value>
		/// The state of the current.
		/// </value>
		public int CurrentState
		{ get { return this[this.Count - 1].StateIndex; } }
		
		/// <summary>
		/// Gets the name of the current symbol.
		/// </summary>
		/// <value>
		/// The name of the current symbol.
		/// </value>
		public string CurrentSymbolName
		{ get { return this[this.Count - 1].ParsingTreeNode.RootSymbol.Name; } }
		
	
		/// <summary>
		/// Shift the specified entry.
		/// </summary>
		/// <param name='entry'>
		/// Entry.
		/// </param>
		public void Shift (StackEntry entry)
		{ this.Add(entry); }
		
		/// <summary>
		/// Reduce the stack by applying the specified rule.
		/// </summary>
		/// <param name='rule'>
		/// The production rule.
		/// </param>
		public bool Reduce (Production rule)
		{
			List<StackEntry> ChildEntries = this.GetRange(this.Count - rule.Derivation.Count, rule.Derivation.Count);
			
			List<ParsingTree> Children = new List<ParsingTree>();
			
			for (int i = 0; i < ChildEntries.Count; i++)
				Children.Add(ChildEntries[i].ParsingTreeNode);				

			this.RemoveRange(this.Count - rule.Derivation.Count, rule.Derivation.Count);

			ParsingTree reducedTree = new ParsingTree();
			reducedTree.RootSymbol =  new Symbol(rule.From, SymbolType.NonTerminal);
			reducedTree.Children.AddRange(Children);
						
			this.Add(new StackEntry(this[this.Count - 1].StateIndex, reducedTree));
						
			return true;
		}
		
		/// <summary>
		/// Goto the specified stateIndex (Optional: in the specified parsing table).
		/// </summary>
		/// <param name='stateIndex'>
		/// State index.
		/// </param>
		/// <param name='table'>
		/// Optional parsing table.
		/// </param>
		public bool Goto (int stateIndex, params ParsingTable[] table)
		{
			if (table.Length > 0)
				if (!(table[0].StateCount > stateIndex)) return false;
			
			this[this.Count - 1].StateIndex = stateIndex;
			return true;
		}
		
		public bool Accept ()
		{
			List<StackEntry> ChildEntries = this.GetRange(1, this.Count - 1);
			
			List<ParsingTree> Children = new List<ParsingTree>();
			
			for (int i = 0; i < ChildEntries.Count; i++)
				Children.Add(ChildEntries[i].ParsingTreeNode);				

			this.RemoveRange(1, this.Count - 1);
						
			this[0].ParsingTreeNode.Children.AddRange(Children);
						
			return true;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTreeStack"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTreeStack"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = "[Stack top]\n";
			
			this.ForEach(s => result += s.ToString() + "\n");
			
			result += "[Stack end]\n";
			
			return result;
		}
	}
}

























