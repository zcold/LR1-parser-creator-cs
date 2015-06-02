// 
//  Grammar.cs
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
using System.Text.RegularExpressions;

namespace ParserBase
{
	/// <summary>
	/// Grammar.
	/// </summary>
	public class Grammar : List<Production> 
	{
		/// <summary>
		/// Adds the production.
		/// </summary>
		/// <param name='production'>
		/// Production.
		/// </param>
		public void AddProduction(params string[] production)
		{
			Add(new Production(production));
			this.Clean();
		}
		
		/// <summary>
		/// Clean this instance.
		/// </summary>
		public void Clean()
		{
			for(int i = 0; i < this.Count; i++)
				for(int j = i + 1; j < this.Count; j++)
					if (this[i].IsEqualTo(this[j]))
					{
						this.RemoveAt(j);
						j--;
					}
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.Grammar"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.Grammar"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = "[Grammar]\n";
			
			for(int i = 0; i < this.Count; i++)
				result += string.Format("{0}:\t{1}\n", i, this[i].ToString());
			
			result += "[End of grammar]\n";
			
			return result;
		}
	}
	/// <summary>
	/// Production. Example: E → P X, E is string[0], P is string[1], X is string[2].
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	public class Production : List<string>
	{
		/// <summary>
		/// Determines whether this instance is equal to the specified anotherProduction.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is equal to the specified anotherProduction; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='anotherProduction'>
		/// If set to <c>true</c> another production.
		/// </param>
		public bool IsEqualTo(Production anotherProduction)
		{
			if (this.Count != anotherProduction.Count)
				return false;
			if (!this.From.Equals(anotherProduction.From))
				return false;
			for(int i = 0; i < this.Count - 1; i++)
				if (!this[i + 1].Equals(anotherProduction[i + 1]))
					return false;
			return true;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ParserBase.Production"/> class.
		/// </summary>
		/// <param name='production'>
		/// Production.
		/// </param>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public Production(params string[] production) : base()
		{
			if (production.Length > 1)
				for (int i = 0; i < production.Length; i++)
					this.Add(production[i]);
			
			if (production.Length == 1)
			{
				this.Add(production[0].Split('=')[0].Remove(@"\s*"));
				
				string DerivationString = production[0].Split('=')[1];
				
				while (DerivationString.StartsWith(" "))
					DerivationString = DerivationString.Remove(0, 1);
				while (DerivationString.StartsWith("\t"))
					DerivationString = DerivationString.Remove(0, 1);
				
				DerivationString = DerivationString.Replace(new Regex(@"\s+"), " ");
				
				foreach(string symbol in DerivationString.Split(' '))
					this.Add(symbol);
				
				this.RemoveAll(s => s.Length == 0);
			}
		}
		
		/// <summary>
		/// Gets or sets from symbol.
		/// </summary>
		/// <value>
		/// From.
		/// </value>
		public string From
		{
			get { return this[0]; }
			set { this[0] = value; }
		}
		
		/// <summary>
		/// Gets or sets the derivation symbols.
		/// </summary>
		/// <value>
		/// The derivation.
		/// </value>
		public List<string> Derivation
		{
			get
			{
				List<string > result = new List<string> (){}; 
				result.AddRange (this); 
				result.RemoveAt (0); 
				return result; 
			}
			set
			{
				this.RemoveRange (1, this.Count - 1);
				this.AddRange (value);				
			}			
		}
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="ParserBase.Production"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current <see cref="ParserBase.Production"/>.
		/// </returns>
		public override string ToString ()
		{
			string result = string.Empty;
			
			for (int i = 0; i < Derivation.Count; i++)
				result += Derivation[i] + " ";
			
			return string.Format ("{0} → {1}", From, result);
		}
	}
	
	/// <summary>
	/// Regex lib.
	/// </summary>
	public class RegexLib : Dictionary<string, Regex>
	{
		/// <summary>
		/// Add the specified key and pattern.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='pattern'>
		/// Pattern.
		/// </param>
		public void Add (string key, string pattern)
		{
			string patternInt = @"" + pattern;
			patternInt = patternInt.Replace(" ", "");
			
			foreach (string k in this.Keys)
				patternInt = patternInt.Replace(new Regex(@"(?<![a-zA-Z_0-9])" + k + "(?![a-zA-Z_0-9])"), "(" + this[k].ToString() + ")");
			
			if (this.ContainsKey(key)) this.Remove(key);				
			
			this.Add(key, new Regex(patternInt));
		}
		
		/// <summary>
		/// Tos the spring.
		/// </summary>
		/// <returns>
		/// The spring.
		/// </returns>
		public string ToSpring() { return Info;}
		
		/// <summary>
		/// Gets the info.
		/// </summary>
		/// <value>
		/// The info.
		/// </value>
		public string Info
		{
			get
			{
				string result = string.Empty;	
				
				foreach(string Key in this.Keys)
					result += "\n" + Key + " " + this[Key];					
								
				return result.Remove(0, 1);
			}
		}
	}
}
	
	
	
