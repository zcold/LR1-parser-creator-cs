// 
//  Symbol.cs
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

namespace ParserBase
{
	/// <summary>
	/// Symbol type. (Terminal, NonTerminal or Unassigned)
	/// </summary>
	public enum SymbolType { Terminal, NonTerminal, Unassigned }
	
	/// <summary>
	/// Symbol of terminal, non-terminal or unassigned type
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public class Symbol
	{   
		public string Name;
		
		public string Value;
		
		public int LineNumber;
		public int ColumnNumber;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.Symbol"/> class.
		/// </summary>
		/// <param name='anotherSymbol'>
		/// Another symbol.
		/// </param>
		public Symbol(Symbol anotherSymbol)
		{
			this.Name = anotherSymbol.Name;
			this.Value = anotherSymbol.Value;
			this.LineNumber = anotherSymbol.LineNumber;
			this.ColumnNumber = anotherSymbol.ColumnNumber;
		}
		
		private SymbolType symbolType;
		
		/// <summary>
		/// Gets the symbol type of the current symbol.
		/// </summary>
		/// <returns>
		/// The symbol type.
		/// </returns>
		public SymbolType GetSymbolType() { return symbolType; }
		
		/// <summary>
		/// Sets the symbol type of the current symbol.
		/// </summary>
		/// <param name='type'>
		/// Symbol type.
		/// </param>
		public void SetType(SymbolType type) { symbolType = type; if (!type.Equals(SymbolType.Terminal)) Value = string.Empty; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.Symbol"/> class.
		/// </summary>
		public Symbol()
		{
			Name = "Unassigned";
			SetType(SymbolType.Unassigned);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.Symbol"/> class.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='symbolValue'>
		/// Value. Terminal can only has one value string. 
		/// Non-terminal and unassigned symbol cannot have any value string.
		/// </param>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public Symbol(string name, SymbolType type, params string[] symbolValue)
		{
			Name = name;
			SetType(type);
			
			switch (type)
			{
				case SymbolType.Terminal:
					if (symbolValue.Length == 1) Value = symbolValue[0];
					else
						throw new Exception(string.Format("Only one value can be assigned with a Terminal instance instead of {0}.", symbolValue.Length));
					break;
				case SymbolType.NonTerminal:
					if (symbolValue.Length > 0)
						throw new Exception(string.Format("Non-terminal should have no value assigned instead of {0} etc.", symbolValue[0]));
					break;			
				default:
					if (symbolValue.Length > 0)
						throw new Exception(string.Format("Unassigned type Symbol instance cannot have value instead of {0} etc.", symbolValue[0]));
					break;
			}		
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.Symbol"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.Symbol"/>.
		/// </returns>
		public override string ToString ()
		{ return string.Format("[Symbol Line: {2} Colone: {3} Name: {0} Value: {1}]", Name, Value, LineNumber, ColumnNumber); }
	}
}

