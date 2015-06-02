// 
//  ParsingTableBuilder.cs
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
	/// State edge.
	/// </summary>
	public class StateEdge
	{
		public string SymbolName;
		public int Source;
		public int Destination;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.StateEdge"/> class.
		/// </summary>
		/// <param name='symbolName'>
		/// Symbol name.
		/// </param>
		/// <param name='src'>
		/// Source.
		/// </param>
		/// <param name='dest'>
		/// Destination.
		/// </param>
		public StateEdge(string symbolName, int src, int dest)
		{
			SymbolName = symbolName;
			Source = src;
			Destination = dest;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.StateEdge"/> class.
		/// </summary>
		/// <param name='anotherEdge'>
		/// Another edge.
		/// </param>
		public StateEdge(StateEdge anotherEdge)
		{
			SymbolName = string.Copy(anotherEdge.SymbolName);
			Source = anotherEdge.Source;
			Destination = anotherEdge.Destination;
		}
		
		/// <summary>
		/// Determines whether this instance is equal to the specified anotherEdge.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is equal to the specified anotherEdge; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='anotherEdge'>
		/// Another state edge
		/// </param>
		public bool IsEqualTo(StateEdge anotherEdge)
		{
			if (SymbolName.Equals(anotherEdge.SymbolName))
				if (Source == anotherEdge.Source)
					if (Destination == anotherEdge.Destination)
						return true;
			return false;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.StateEdge"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.StateEdge"/>.
		/// </returns>
		public override string ToString ()
		{ return string.Format ("[StateEdge] {0} to {1} via {2}", Source, Destination, SymbolName); }
	}
	
	/// <summary>
	/// Parsing table state pool.
	/// </summary>
	public class ParsingTableStatePool : List<ParsingTableState>
	{
		public List<Dictionary<string, int>> Links;
		public List<StateEdge> Edges;
		
		private bool stable;
		
		/// <summary>
		/// Gets a value indicating whether this instance is stable.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is stable; otherwise, <c>false</c>.
		/// </value>
		public bool IsStable
		{ get { return stable; } }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTableStatePool"/> class.
		/// </summary>
		public ParsingTableStatePool () : base() { stable = false; Edges = new List<StateEdge>();}	
		
		/// <summary>
		/// Clean this instance.
		/// </summary>
		public ParsingTableStatePool Clean()
		{
			ParsingTableStatePool metaResult = new ParsingTableStatePool();
			metaResult.AddRange(this);
			
			ParsingTableStatePool result = new ParsingTableStatePool();
			
			while(metaResult.Count > 0)
			{
				result.Add(metaResult[0]);
				metaResult.RemoveAll(r => r.Equals(result.Last()));
			}
			
			stable = this.Count == result.Count;
			
			
			for (int i = 0; i < result.Count; i++)
				result[i].Index = i;
			
			return result;			
		}
		
		/// <summary>
		/// Updates the edges.
		/// </summary>
		/// <param name='orignalStateIndex'>
		/// Orignal state index.
		/// </param>
		/// <param name='newStateIndex'>
		/// New state index.
		/// </param>
		public void UpdateEdges(int orignalStateIndex, int newStateIndex)
		{
			for(int k = 0; k < this.Edges.Count; k++)
			{
				if (this.Edges[k].Source == orignalStateIndex)
					this.Edges[k].Source = newStateIndex;
				if (this.Edges[k].Destination == orignalStateIndex)
					this.Edges[k].Destination = newStateIndex;
			}
		}
		
		/// <summary>
		/// Cleans up.
		/// </summary>
		public void CleanUp()
		{
			for(int i = 0; i < this.Count; i++)
				for(int j = i + 1; j < this.Count; j++)
					if (this[i].IsEqualTo(this[j]))
					{
						UpdateEdges(this[j].Index, this[i].Index);
						this.RemoveAt(j);
					}		
			
			for(int i = 0; i < this.Edges.Count; i++)
				for(int j = i + 1; j < this.Edges.Count; j++)
					if (this.Edges[j].IsEqualTo(this.Edges[i]))
						this.Edges.RemoveAt(j--);
			
			for(int i = 0; i < this.Count; i++)
			{
				UpdateEdges(this[i].Index, i);
				this[i].Index = i;
			}
			
		}
		
		/// <summary>
		/// Accept state indices.
		/// </summary>
		/// <returns>
		/// The state indices.
		/// </returns>
		/// <param name='p'>
		/// Accepting production.
		/// </param>
		public List<int> AcceptStateIndices(Production p)
		{
			List<int> result = new List<int>();
			this.ForEach(s => {if (s.IsAccept(p)) result.Add(s.Index);});
			
			return result;
		}
		
		/// <summary>
		/// Evolve the current state pool based on the specified grammar.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		public bool Evolve(Grammar grammar)
		{
			ParsingTableStatePool temp = new ParsingTableStatePool();
			temp.AddRange(this);
			
			temp.ForEach(state => state.TransationSymbolNames.ForEach(s => {
				this.Add(state.NextState(s, grammar, this.Count));
				this.Edges.Add(new StateEdge(s, state.Index, this.Count - 1));
				}));			
			
			this.CleanUp();	
			
			return stable;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTableStatePool"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTableStatePool"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = string.Empty;
			this.ForEach(s => result += s.ToString() + "\n");
			
			this.Edges.ForEach(e => result += e.ToString() + "\n");
			return string.Format ("[ParsingTableStatePool]\n\n{0}", result);
		}
	}
	
	/// <summary>
	/// Parsing table state.
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public class ParsingTableState : List<DotProduction>
	{
		/// <summary>
		/// Determines whether this instance accepts the specified acceptProduction.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance accepts the specified acceptProduction; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='acceptProduction'>
		/// Accepting production.
		/// </param>
		public bool IsAccept(Production acceptProduction)
		{ return this.FindAll(dp => (dp.DotLocation == dp.Derivation.Count) && dp.Equals(new DotProduction(acceptProduction, acceptProduction.Derivation.Count))).Count > 0; }
		
		public int Index;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTableState"/> class.
		/// </summary>
		public ParsingTableState() : base() { Index = -1; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.ParsingTableState"/> class.
		/// </summary>
		/// <param name='index'>
		/// Index.
		/// </param>
		public ParsingTableState(int index) : base() { Index = index; }
		
		/// <summary>
		/// Determines whether this instance is equal to the specified anotherState.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is equal to the specified anotherState; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='anotherState'>
		/// Another state.
		/// </param>
		public bool IsEqualTo(ParsingTableState anotherState)
		{
			if (this.Count != anotherState.Count) return false;
			
			for (int i = 0; i < this.Count; i++)
			{
				if (anotherState.FindAll(dp => dp.Equals(this[i])).Count == 1)
					continue;
				
				return false;
			}
			
			return true;
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="ParserBase.ParsingTableState"/> is empty.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty
		{ get { return this.Count == 0; } }
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="ParserBase.ParsingTableState"/> is complete.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is complete; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{ get { return this.FindAll(dp => !dp.IsComplete).Count == 0; } }
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="ParserBase.ParsingTableState"/> is valid.
		/// </summary>
		/// <value>
		/// <c>true</c> if valid; otherwise, <c>false</c>.
		/// </value>
		public bool Valid
		{ get { return (!this.IsEmpty) || (this.FindAll(dp => dp.IsComplete).Count == 1); } }
		
		/// <summary>
		/// Gets the transation symbol names.
		/// </summary>
		/// <value>
		/// The transation symbol names.
		/// </value>
		public List<string> TransationSymbolNames
		{
			get
			{
				List<string> metaResult = new List<string>();
				this.IncomplatePart.ForEach(icdp => metaResult.Add(icdp.NextSymbolName));
				List<string> result = new List<string>();
				
				while(metaResult.Count > 0)
				{
					result.Add(metaResult[0]);
					metaResult.RemoveAll(s => s.Equals(result.Last()));
				}
				
				return result;
			}
		}
			
		/// <summary>
		/// Gets the incomplate part.
		/// </summary>
		/// <value>
		/// The incomplate part.
		/// </value>
		public ParsingTableState IncomplatePart
		{
			get
			{
				ParsingTableState result = new ParsingTableState();
				if (this.IsComplete) return result;
				
				result.AddRange(this.FindAll(dp => !dp.IsComplete));
								
				return result;
			}
		}
		
		/// <summary>
		/// Gets the complate part.
		/// </summary>
		/// <value>
		/// The complate part.
		/// </value>
		public ParsingTableState CompletePart
		{
			get
			{
				ParsingTableState result = new ParsingTableState();
				if (this.IsComplete) return result;
				
				result.AddRange(this.FindAll(dp => dp.IsComplete));
								
				return result;
			}
		}
		
		/// <summary>
		/// Complete the state based on the specified grammar.
		/// </summary>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		public void Complete(Grammar grammar)
		{
			List<DotProduction> derivedDotProduction = new List<DotProduction>();
			
			int currentStateCount = this.Count;
			int lastStateCount = 0;
			
			while(currentStateCount != lastStateCount)
			{
				lastStateCount = this.Count;
				this.TransationSymbolNames.ForEach(n => grammar.FindAll(p => p.From == n).ForEach(p => derivedDotProduction.Add(new DotProduction(p))));
				this.AddRange(derivedDotProduction);
				CleanUp();
				currentStateCount = this.Count;
			}
		}
		
		public void CleanUp()
		{
			for(int i = 0; i < this.Count; i++)
			{
				for(int j = i + 1; j < this.Count; j++)
				{
					if (this[i].Equals(this[j]))
						this.RemoveAt(j--);
				}
			}
		}
		
		/// <summary>
		/// Nexts the state.
		/// </summary>
		/// <returns>
		/// The state.
		/// </returns>
		/// <param name='symbolName'>
		/// Symbol name.
		/// </param>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		/// <param name='presetIndex'>
		/// Preset index.
		/// </param>
		public ParsingTableState NextState(string symbolName, Grammar grammar, int presetIndex)
		{
			ParsingTableState result = new ParsingTableState();
			result = NextState(symbolName, grammar);
			result.Index = presetIndex;
			
			return result;
		}
		
		/// <summary>
		/// Nexts the state.
		/// </summary>
		/// <returns>
		/// The state.
		/// </returns>
		/// <param name='symbolName'>
		/// Symbol name.
		/// </param>
		/// <param name='grammar'>
		/// Grammar.
		/// </param>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public ParsingTableState NextState(string symbolName, Grammar grammar)
		{
			if (!TransationSymbolNames.Contains(symbolName))
				throw new Exception("Unexpected symbol " + symbolName);
			
			ParsingTableState result = new ParsingTableState();
			
			ParsingTableState possibleDotProductions = new ParsingTableState();
			possibleDotProductions.AddRange(this.IncomplatePart.FindAll(dp => dp.NextSymbolName == symbolName));
			
			result = possibleDotProductions.Clone;
			
			for(int i = 0; i < result.Count; i++)
				result[i].Forward();
			
			result.Complete(grammar);	
			result.Index = this.Index + this.TransationSymbolNames.IndexOf(symbolName) + 1;
			
			return result;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTableState"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.ParsingTableState"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = "State " + Index.ToString() + "\n";
			this.ForEach(dp => result += dp.ToString() + "\n");
			
			return result;
		}
		
		/// <summary>
		/// Gets the clone.
		/// </summary>
		/// <value>
		/// The clone.
		/// </value>
		public ParsingTableState Clone
		{
			get
			{
				ParsingTableState result = new ParsingTableState();
				
				this.ForEach(dp => result.Add(new DotProduction(dp, dp.DotLocation)));
								
				return result;
			}
		}
	}
	
	/// <summary>
	/// Dot production.
	/// </summary>
	public class DotProduction : Production
	{
		/// <summary>
		/// Determines whether this <see cref="ParserBase.DotProduction"/> is from the specified production.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this <see cref="ParserBase.DotProduction"/> is from the specified production; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='production'>
		/// If set to <c>true</c> production.
		/// </param>
		public bool IsFrom(Production production)
		{
			if (this.Count != production.Count)
				return false;
			
			if (!this.From.Equals(production.From))
				return false;
			
			for (int i = 0; i < this.Derivation.Count; i++)
				if (!this.Derivation[i].Equals(production.Derivation[i]))
					return false;
		
			return true;
		}
		
		/// <summary>
		/// Determines whether the specified <see cref="DotProduction"/> is equal to the current <see cref="ParserBase.DotProduction"/>.
		/// </summary>
		/// <param name='anotherDotProduction'>
		/// The <see cref="DotProduction"/> to compare with the current <see cref="ParserBase.DotProduction"/>.
		/// </param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="DotProduction"/> is equal to the current
		/// <see cref="ParserBase.DotProduction"/>; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(DotProduction anotherDotProduction)
		{
			if (this.DotLocation != anotherDotProduction.DotLocation)
				return false;
			
			if (this.Count != anotherDotProduction.Count)
				return false;
			
			if (!this.From.Equals(anotherDotProduction.From))
				return false;
			
			for (int i = 0; i < this.Derivation.Count; i++)
				if (!this.Derivation[i].Equals(anotherDotProduction.Derivation[i]))
					return false;
		
			return true;
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="ParserBase.DotProduction"/> is complete.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is complete; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{get { return DotLocation == this.Count - 1; } }
		
		/// <summary>
		/// Gets the name of the next symbol.
		/// </summary>
		/// <value>
		/// The name of the next symbol.
		/// </value>
		public string NextSymbolName
		{ get { return (this.IsComplete) ? string.Empty : Derivation[DotLocation]; } }
		
		public int DotLocation;
		
		/// <summary>
		/// Forward this instance.
		/// </summary>
		public void Forward()
		{ DotLocation++; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.DotProduction"/> class.
		/// </summary>
		public DotProduction() : base()
		{ DotLocation = 0; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.DotProduction"/> class.
		/// </summary>
		/// <param name='product'>
		/// Product.
		/// </param>
		public DotProduction(Production product)
		{
			product.ForEach(s => this.Add(s));
			DotLocation = 0;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.DotProduction"/> class.
		/// </summary>
		/// <param name='dotProduct'>
		/// Dot product.
		/// </param>
		public DotProduction(DotProduction dotProduct)
		{
			dotProduct.ForEach(s => this.Add(s));
			DotLocation = dotProduct.DotLocation;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.DotProduction"/> class.
		/// </summary>
		/// <param name='product'>
		/// Product.
		/// </param>
		/// <param name='dotLocation'>
		/// Dot location.
		/// </param>
		public DotProduction(Production product, int dotLocation)
		{
			product.ForEach(s => this.Add(s));
			DotLocation = dotLocation;
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.DotProduction"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.DotProduction"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = string.Empty;
			
			for (int i = 0; i < Derivation.Count; i++)
				result += " " + ((i == DotLocation) ? "." : string.Empty) + Derivation[i];
			
			if (DotLocation == Derivation.Count) result += ".";
			
			return string.Format ("{0} →{1}", From, result);
		}	
	}
}

