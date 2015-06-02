// 
//  LRParser.cs
//  
//  Author:
//       shuol <${AuthorEmail}>
//  
//  Copyright (c) 2011 shuol
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
	/// LR parser.
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public class LRParser
	{
		private ParsingTable parsingTable;
		
		/// <summary>
		/// Sets the parsing table.
		/// </summary>
		/// <param name='table'>
		/// Table.
		/// </param>
		public void SetParsingTable(ParsingTable table)
		{ this.parsingTable = table; }
		
		/// <summary>
		/// Gets the parsing table.
		/// </summary>
		/// <returns>
		/// The parsing table.
		/// </returns>
		public ParsingTable GetParsingTable()
		{ return this.parsingTable; }
		
		private Grammar grammar;
		
		/// <summary>
		/// Sets the grammar.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		public void SetGrammar(Grammar grammar)
		{ this.grammar = grammar; }
		
		/// <summary>
		/// Gets the grammar.
		/// </summary>
		/// <returns>
		/// The grammar.
		/// </returns>
		public Grammar GetGrammar()
		{ return grammar; }
				
		private ParsingTreeStack stack;
		
		/// <summary>
		/// Gets the current stack.
		/// </summary>
		/// <returns>
		/// The current stack.
		/// </returns>
		public ParsingTreeStack GetCurrentStack ()
		{ return stack; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.LRParser"/> class.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		/// <param name='table'>
		/// Table.
		/// </param>
		public LRParser (Grammar grammar, ParsingTable table)
		{
			SetParsingTable(table);
			SetGrammar(grammar);
			stack = new ParsingTreeStack();
			stack.Shift(new StackEntry(0, new ParsingTree(grammar[0].From, SymbolType.NonTerminal)));
			Done = false;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.LRParser"/> class.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		public LRParser (Grammar grammar)
		{
			statePool = Build.StatePool(grammar);
			SetParsingTable(Build.Table(grammar, statePool));
			SetGrammar(grammar);
			stack = new ParsingTreeStack();
			stack.Shift(new StackEntry(0, new ParsingTree(grammar[0].From, SymbolType.NonTerminal)));
			Done = false;
		}
		
		private ParsingTableStatePool statePool;
		
		/// <summary>
		/// Gets the state pool.
		/// </summary>
		/// <returns>
		/// The state pool.
		/// </returns>
		public ParsingTableStatePool GetStatePool()
		{ return statePool; }
		
		/// <summary>
		/// Nexts the action.
		/// </summary>
		/// <returns>
		/// The action.
		/// </returns>
		/// <param name='currentState'>
		/// Current state.
		/// </param>
		/// <param name='nextSymbolName'>
		/// Next symbol name.
		/// </param>
		private ParsingAction NextAction(int currentState, string nextSymbolName)
		{ return parsingTable[currentState, nextSymbolName]; }
		
		/// <summary>
		/// Step the specified nextSymbol.
		/// </summary>
		/// <param name='nextSymbol'>
		/// Next symbol.
		/// </param>
		public string Step(Symbol nextSymbol)
		{ return Step(nextSymbol, parsingTable, grammar, ref stack, ref Done); }
		
		/// <summary>
		/// Step the specified nextSymbol, table, grammar, stack and parsingDone.
		/// </summary>
		/// <param name='nextSymbol'>
		/// Next symbol.
		/// </param>
		/// <param name='table'>
		/// Table.
		/// </param>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		/// <param name='stack'>
		/// Stack.
		/// </param>
		/// <param name='parsingDone'>
		/// Parsing done.
		/// </param>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		private string Step(Symbol nextSymbol, ParsingTable table, Grammar grammar, ref ParsingTreeStack stack, ref bool parsingDone)
		{	
			if (!table.SymbolList.Contains(nextSymbol.Name))
				return string.Format("Unsuccess. Unvaild symbol name {0}.", nextSymbol.Name);	
			
			ParsingAction NextAction = table[stack.CurrentState, nextSymbol.Name];
			string result = string.Empty;
					
			result += NextAction.ToString();
					
			if (NextAction.ActionType.Equals(ParsingActionType.Unassigned))
			{
				string expectedSymbolNames = string.Empty;
				table.ExpectedSymbolNames(stack.CurrentState).ForEach(s => expectedSymbolNames += "\t" + s);
				
				return string.Format("Unsuccess.\n  Current state: {0}, unexpected next symbol: {1}.\nExpecting\n{2}", stack.CurrentState, nextSymbol.Name, expectedSymbolNames);
			}
			else
				switch (NextAction.ActionType)
				{
					case ParsingActionType.Shift:
						stack.Shift(new StackEntry(NextAction.Index, new ParsingTree(nextSymbol)));
						if (table[stack.CurrentState, nextSymbol.Name].ActionType == ParsingActionType.Reduce)
							result += "\n" + Step(nextSymbol, table, grammar, ref stack, ref parsingDone);
												
						break;
					case ParsingActionType.Reduce:
						stack.Reduce(grammar[NextAction.Index]);
						ParsingTree pt = stack[stack.Count - 1].ParsingTreeNode;
						Symbol s = pt.RootSymbol;
						if (table[stack.CurrentState, s.Name].ActionType == ParsingActionType.Reduce)
							result += "\n" + Step(s, table, grammar, ref stack, ref parsingDone);
						if (table[stack.CurrentState, s.Name].ActionType == ParsingActionType.Goto)
							result += "\n" + Step(s, table, grammar, ref stack, ref parsingDone);
						break;
					case ParsingActionType.Goto:
						stack.Goto(NextAction.Index, table);
						ParsingTree ptg = stack[stack.Count - 1].ParsingTreeNode;
						Symbol sg = ptg.RootSymbol;
						if (table[stack.CurrentState, sg.Name].ActionType == ParsingActionType.Reduce)
							result += "\n" + Step(sg, table, grammar, ref stack, ref parsingDone);
						break;
					case ParsingActionType.Accept:
						parsingDone = true;
						stack.Accept();
						return "Accept. Parsing is successfully done.";
					default:
						throw new Exception("Undifineded parsing action type.");
				}
			
			
			return result;
		}
		
		private bool Done;
		
		/// <summary>
		/// Gets a value indicating whether this instance is done.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is done; otherwise, <c>false</c>.
		/// </value>
		public bool IsDone
		{ get { return Done; } }
		
		/// <summary>
		/// Gets the parsing tree.
		/// </summary>
		/// <returns>
		/// The parsing tree.
		/// </returns>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public ParsingTree GetParsingTree()
		{
			if (IsDone)
				return stack[0].ParsingTreeNode;
			else
				throw new Exception("Unable to get parsing tree since parsing is not completed yet.");
		}
		
		/// <summary>
		/// Gets the parsing tree.
		/// </summary>
		/// <returns>
		/// The parsing tree.
		/// </returns>
		/// <param name='symbolList'>
		/// Symbol list.
		/// </param>
		public ParsingTree GetParsingTree(List<Symbol> symbolList)
		{
			int i = 0;
			string result = string.Empty;
			while (!Done)
			{
				result = this.Step(symbolList[i++]);
				if (result.StartsWith("U"))				
					throw new Exception(result + "\nParsing failed @ " + symbolList[i-1]);
			}
			return stack[0].ParsingTreeNode;				
		}
	}
}

