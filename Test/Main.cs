// 
//  Main.cs
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
	class MainClass
	{	
		public static void Main (string[] args)
		{
			//Console.WriteLine(Build.ParsingTreeFromFile("../../grammar.cs", "../../grammar.cs"));
			//Console.WriteLine(Build.ParsingTreeFromFile("./string.txt", "../../SimulinkGrammar.cs"));

			DateTime start = new DateTime();
			start = DateTime.Now;
			ParsingTree pt = Build.ParsingTreeFromFile("./string.txt", "../../SimulinkGrammar.cs");
			DateTime end = new DateTime();
			end = DateTime.Now;
			Console.WriteLine("Parsing time:");
			Console.WriteLine((end - start).Seconds + " Seconds");
			Console.WriteLine();

            StreamWriter sw = new StreamWriter("Result.cs");
            sw.Write(pt);
            sw.Close();
            sw.Dispose();

            List<ParsingTree> blocks = GetParsingTreeByRootSymbolValue(pt, "BlockParameterDefaults");
            Console.WriteLine(blocks.Count);
            if (blocks.Count > 0)
            {
                Console.WriteLine(blocks[0]);
                
            }


		

			

			Console.ReadLine();
		}

		public static List<ParsingTree> GetParsingTreeByRootSymbolValue(ParsingTree SourceParsingTree, string Value)
		{
			List<ParsingTree> Result = new List<ParsingTree>();
            
            GetParsingTreeByRootSymbolValue(SourceParsingTree, Value, ref Result);
            
            return Result;
		}

		public static void GetParsingTreeByRootSymbolValue(ParsingTree SourceParsingTree, string Value, ref List<ParsingTree> Result)
		{
			// If the first child root symbol name is equal to value
			// The parsing tree is picked
            if (SourceParsingTree.Children.Count > 0)
                if (SourceParsingTree.Children[0].RootSymbol.Value.Equals(Value))
                    Result.Add(SourceParsingTree);
                
			for (int i = 0; i < SourceParsingTree.Children.Count; i++)
                GetParsingTreeByRootSymbolValue(SourceParsingTree.Children[i], Value, ref Result);
		}
		
		
	}	
		
}
