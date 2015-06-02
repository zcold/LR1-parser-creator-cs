# LR1-parser-creator-cs
LR1 parser creator in CSharp, course project level quality

It is a Visual Studio 2010/2012 solution folder. ParserBase folder is the basic
objects/methods/properties for building a parser. Test folder contains
a set of methods (Test.cs) for testing the parser and small piece code
(Main.cs) for test a parser which is specified by a grammar definition
file (I use .cs as extension name for grammar highlight).

In the Main.cs in Test folder, I use a file named SimulinkGrammar.cs
in Test folder to configure the parser to parse Simulink model file
(.mdl). An example .mdl code piece is copied into string.txt in
Test/bin/Debug folder.

The parser can also be used to handle the parser spec file. The
grammar definition is in the file Grammar.cs in Test folder.
