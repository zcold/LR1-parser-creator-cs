// 
//  tests.cs
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
using System.Linq;
using System.IO;
using ParserBase;


namespace Test
{
	public static class tests
	{
		public static void testParserAll ()
		{
			Grammar g = new Grammar();
			g.AddProduction("A", "B", "C", "D", "$");
			g.AddProduction("B", "b");
			g.AddProduction("C", "c");
			g.AddProduction("D", "d");
			
			ParsingTable table = Build.Table(g);
			
			Console.WriteLine(Build.StatePool(g));
			Console.WriteLine(table);
			
			LRParser parser = new LRParser(g, table);
			List<Symbol> sl = new List<Symbol>();
			sl.Add(new Symbol("b", SymbolType.Terminal, "WTFB"));
			sl.Add(new Symbol("c", SymbolType.Terminal, "WTFC"));
			sl.Add(new Symbol("d", SymbolType.Terminal, "WTFD"));
			sl.Add(new Symbol("$", SymbolType.Terminal, "EOF"));
			
			Console.WriteLine(parser.GetParsingTree(sl).ToString());
			TestSourceFormatter ();		
		}
		
		public static void testParserWithTable ()
		{
			
			Grammar g = new Grammar();
			g.AddProduction("A", "B", "C", "D", "$");
			g.AddProduction("B", "b");
			g.AddProduction("C", "c");
			g.AddProduction("D", "d");
			
			ParsingTable table = Build.Table(g);
			
			Console.WriteLine(Build.StatePool(g));
			Console.WriteLine(table);
			
			string t = string.Empty;
			
			LRParser parser = new LRParser(g, table);
			
			while(true)
			{
				Console.Write ("input next terminal (exit to quit): ");
				t = Console.ReadLine();
				
				if (t.StartsWith("exit")) Environment.Exit(0);
				else if (t.StartsWith("stack")) 
				{
					Console.WriteLine(parser.GetCurrentStack().ToString());
					foreach (StackEntry se in parser.GetCurrentStack())
						Console.WriteLine(se.ParsingTreeNode.ToString());
				}
				else if (t.StartsWith("status"))
					Console.WriteLine("Done? " +parser.IsDone);
				else
				{		
					Console.WriteLine(parser.Step(ToSymbol(t)));
					
					if(parser.IsDone)
					{
						Console.WriteLine(parser.GetParsingTree().ToString());
						Environment.Exit(0);
					}
				}
				
				
				
				
			}
			
		
		}
	
		public static void testStatePool ()
		{
			Grammar g = new Grammar();
			g.AddProduction("A", "B", "C", "D", "$");
			g.AddProduction("B", "b", "c");
			g.AddProduction("C", "c");
			g.AddProduction("D", "d");
			g.AddProduction("B", "b");
			g.AddProduction("B", "b", "d");
			g.AddProduction("c", "");
			
			ParsingTableState pts = new ParsingTableState();
			pts.Index = 0;
			pts.Add(new DotProduction(g[0], 0));
			pts.Complete(g);
			
			ParsingTableStatePool p = Build.StatePool(g);
			Console.WriteLine(p);
			p.AcceptStateIndices(g[0]).ForEach(idx => Console.WriteLine("State " + idx + " accepts."));
			
			//ParsingTableStatePool ss = p.Clean();
			//Console.WriteLine(ss);
		}
		
		public static void testNextState ()
		{
			Grammar g = new Grammar();
			g.AddProduction("A", "B", "C", "D");
			g.AddProduction("B", "b", "c");
			g.AddProduction("C", "c");
			g.AddProduction("D", "d");
			g.AddProduction("B", "b");
			g.AddProduction("c", "");
			
			ParsingTableState pts = new ParsingTableState();
			
			pts.Add(new DotProduction(new Production("A", "B", "C", "D"), 0));
			pts.Complete(g);
						
			ParsingTableState pp = new ParsingTableState();
			
			pp.Add(new DotProduction(new Production("A", "B", "C", "D"), 0));
			pp.Complete(g);
			
			Console.WriteLine(pp.NextState("b", g).NextState("c", g));
			Console.WriteLine(pp);
			
			Console.WriteLine(pts.Equals(pp));
		}
		
		public static void TestSourceFormatter ()
		{
			Regex Keyword = new Regex(@"keyword");			
			Regex Identifier = new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*");
			Regex Number = new Regex(@"[+-]?[0-9]*\.?[0-9]+E?[+-]?\d+");
						
			SymbolRegEx keyword = new SymbolRegEx("keyword", Keyword.ToString());
			SymbolRegEx identifier = new SymbolRegEx("identifier", Identifier.ToString());
			//SymbolRegEx number = new SymbolRegEx("number", Number.ToString());
						
			List<SymbolRegEx> l = new List<SymbolRegEx>();
			l.Add(keyword);
			l.Add(identifier);
			//l.Add(number);
			
			SourceFormatter sf = new SourceFormatter();
			sf.AddNewSymbol("number", Number.ToString());
			sf.SourceFileName = "./testSource.x";
			
			
			List<Symbol> sl = sf.SourceToSymbolListEntireFile(sf.SourceFileName, new List<SymbolRegEx>(){new SymbolRegEx("number", Number.ToString())});
			
			string result = string.Empty;
			
			sl.ForEach(sy => result += "#" + sy.Name + " " + sy.Value + "#\n");
			Console.WriteLine(result);
		}
		
		public static void testParser ()
		{
			ParsingTable wtf = new ParsingTable();
			
			wtf[0, "B"] = new ParsingAction(ParsingActionType.Goto, 1);
			wtf[0, "b"] = new ParsingAction(ParsingActionType.Shift, 4);
			
			wtf[4, "b"] = new ParsingAction(ParsingActionType.Reduce, 1);
			wtf[4, "c"] = new ParsingAction(ParsingActionType.Reduce, 1);
			wtf[4, "d"] = new ParsingAction(ParsingActionType.Reduce, 1);
			wtf[4, "$"] = new ParsingAction(ParsingActionType.Reduce, 1);
			
			wtf[1, "C"] = new ParsingAction(ParsingActionType.Goto, 2);
			wtf[1, "c"] = new ParsingAction(ParsingActionType.Shift, 5);
			
			wtf[5, "b"] = new ParsingAction(ParsingActionType.Reduce, 2);
			wtf[5, "c"] = new ParsingAction(ParsingActionType.Reduce, 2);
			wtf[5, "d"] = new ParsingAction(ParsingActionType.Reduce, 2);
			wtf[5, "$"] = new ParsingAction(ParsingActionType.Reduce, 2);
			
			wtf[2, "D"] = new ParsingAction(ParsingActionType.Goto, 3);
			wtf[2, "d"] = new ParsingAction(ParsingActionType.Shift, 6);
			
			wtf[6, "b"] = new ParsingAction(ParsingActionType.Reduce, 3);
			wtf[6, "c"] = new ParsingAction(ParsingActionType.Reduce, 3);
			wtf[6, "d"] = new ParsingAction(ParsingActionType.Reduce, 3);
			wtf[6, "$"] = new ParsingAction(ParsingActionType.Reduce, 3);
			
			wtf[3, "$"] = new ParsingAction(ParsingActionType.Accept, 0);
			
			// sort symbols
			wtf.SymbolList = new List<string>(){"b", "c", "d", "$", "B", "C", "D"};
			Console.WriteLine(wtf.ToString());
						
			Grammar g = new Grammar();
			g.AddProduction("A", "B", "C", "D");
			//g.Add(new Production(){"A", "B", "C", "D"});
			g.AddProduction("B", "b");
			g.AddProduction("C", "c");
			g.AddProduction("D", "d");
			
			Console.WriteLine(g.ToString());
			
			string t = string.Empty;
			
			LRParser parser = new LRParser(g, wtf);
			
			while(true)
			{
				Console.Write ("input next terminal (exit to quit): ");
				t = Console.ReadLine();
				
				if (t.StartsWith("exit")) Environment.Exit(0);
				else
				{		
					Console.WriteLine(parser.Step(ToSymbol(t)));
				}
				if (t.StartsWith("stack")) 
				{
					Console.WriteLine(parser.GetCurrentStack().ToString());
					foreach (StackEntry se in parser.GetCurrentStack())
						Console.WriteLine(se.ParsingTreeNode.ToString());
				}
				if (t.StartsWith("status"))
					Console.WriteLine("Done? " +parser.IsDone); 
			}
		}
		
		public static Symbol ToSymbol(string s)
		{
			return new Symbol(s, SymbolType.Terminal, s);
		}
	}
}

