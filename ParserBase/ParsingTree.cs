// 
//  ParsingTree.cs
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
	/// Parsing tree
	/// </summary>
	public class ParsingTree
	{
		/// <summary>
		/// The root symbol.
		/// </summary>
		public Symbol RootSymbol;
		
		/// <summary>
		/// The children.
		/// </summary>
		public List<ParsingTree> Children;
		
		/// <summary>
		/// Gets a value indicating whether this instance is unassigned.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is unassigned; otherwise, <c>false</c>.
		/// </value>
		public bool IsUnassigned { get { return RootSymbol.GetType().Equals(SymbolType.Unassigned); } }
		
		#region { public bool IsLeaf }
		
		/// <summary>
		/// Gets or sets a value indicating whether this instance is leaf.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is leaf; otherwise, <c>false</c>.
		/// </value>
		public bool IsLeaf
		{
			get { return GetIsLeaf(); }
			set { SetIsLeaf(value); }
		}
		
		/// <summary>
		/// Private method for getting IsLeaf.
		/// </summary>
		/// <returns>
		/// The is leaf.
		/// </returns>
		private bool GetIsLeaf()
		{
			return Children.Count == 0;
		}
		
		/// <summary>
		/// Private method for setting IsLeaf.
		/// </summary>
		/// <param name='setValue'>
		/// Set value.
		/// </param>
		private void SetIsLeaf(bool setValue)
		{
			if (setValue) Children = new List<ParsingTree>();
		}
		
		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTree"/> class.
		/// </summary>
		public ParsingTree()
		{
			RootSymbol = new Symbol();
			IsLeaf = true;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTree"/> class.
		/// </summary>
		/// <param name='rootSymbolName'>
		/// Root symbol name.
		/// </param>
		/// <param name='rootSymbolType'>
		/// Root symbol type.
		/// </param>
		/// <param name='symbolValue'>
		/// Symbol value.
		/// </param>
		public ParsingTree(string rootSymbolName, SymbolType rootSymbolType = SymbolType.Unassigned, params string[] symbolValue)
		{
			RootSymbol = new Symbol(rootSymbolName, rootSymbolType, symbolValue);
			IsLeaf = true;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTree"/> class.
		/// </summary>
		/// <param name='rootSymbol'>
		/// Root symbol.
		/// </param>
		public ParsingTree(Symbol rootSymbol)
		{
			RootSymbol = rootSymbol;
			IsLeaf = true;
		}
		
		/// <summary>
		/// Tos the string.
		/// </summary>
		/// <returns>
		/// The string.
		/// </returns>
		/// <param name='currentLevel'>
		/// Current level. Default vale is 0;
		/// </param>
		public string ToString (int currentLevel = 0)
		{
			string currentPrefixSpaceString = GetPrefixSpaceString(currentLevel);
			string result = string.Empty;
			
			result = currentPrefixSpaceString + RootSymbol.Name + "\n";
			
			if (!IsLeaf)
				Children.ForEach(c => result += c.ToString(currentLevel + 1));
					
			return result;
		}
		
		public override string ToString ()
		{
			return this.ToString(0);
		}
				
		/// <summary>
		/// Gets the prefix space string.
		/// </summary>
		/// <returns>
		/// The prefix space string.
		/// </returns>
		/// <param name='currentLevel'>
		/// Current level.
		/// </param>
		private string GetPrefixSpaceString(int currentLevel)
		{
			string result = string.Empty;
			
			if (currentLevel == 0) return result;
			
			for (int i = 0; i < currentLevel; i++)
				result += " ";
			
			return result;
		}
	}

	/// <summary>
	/// Parsing tree handeler
	/// </summary>
	public class ParsingTreeHandler
	{
		/// <summary>
		/// Source parsing tree should be initialized during class construction
		/// </summary>
		public ParsingTree SourceParsingTree { private set; get; }

		/// <summary>
		/// Only one constructing method
		/// </summary>
		/// <param name="SourceParsingTree">Source parsing tree to be handled</param>
		public ParsingTreeHandler(ParsingTree SourceParsingTree)
		{ this.SourceParsingTree = SourceParsingTree; }

		/// <summary>
		/// Get parsing tree by root symbol value
		/// </summary>
		/// <param name="Value">String value of the root symbol</param>
		/// <returns></returns>
		public List<ParsingTree> GetParsingTreeByRootSymbolValue(string Value)
		{
			List<ParsingTree> Result = new List<ParsingTree>();

			GetParsingTreeByRootSymbolValue(SourceParsingTree, Value, ref Result);

			return Result;
		}

		/// <summary>
		/// private method to get parsing tree by root symbol value
		/// </summary>
		/// <param name="SourceParsingTree">Source parsing tree</param>
		/// <param name="Value">String value</param>
		/// <param name="Result">Result parsing tree list</param>
		private void GetParsingTreeByRootSymbolValue(ParsingTree SourceParsingTree, string Value, ref List<ParsingTree> Result)
		{
			// If the first child root symbol name is equal to value
			// The parsing tree is picked
			if (SourceParsingTree.Children.Count > 0)
				if (SourceParsingTree.Children[0].RootSymbol.Value.Equals(Value))
					Result.Add(SourceParsingTree);

			for (int i = 0; i < SourceParsingTree.Children.Count; i++)
				GetParsingTreeByRootSymbolValue(SourceParsingTree.Children[i], Value, ref Result);
		}

		public ParsingTree GetFormattedParsingTree(ParsingTree SourceParsingTree, String DummyNonTerminalName)
		{
			ParsingTree Result = new ParsingTree();
			if (SourceParsingTree.Children[0].RootSymbol.Name.Equals(DummyNonTerminalName))
				return SourceParsingTree.Children[0];

			return Result;
		}
	}
}

