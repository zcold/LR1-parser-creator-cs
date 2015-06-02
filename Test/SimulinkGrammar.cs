// Complete Simulink grammer
// Too much for Sylva

// Configurations
// Now only end of file marker and skip is allowed
configurations
{
	endOfFileMarker = EndOfFileMarker;
	skip = Whitespace CommentLine Return;
}

// Prioritized symbol regular expressions (.NET style)
production
{
	// Non-terminals
	MODELFILE = MODEL STATEFLOW EndOfFileMarker;
	
	MODEL = Keyword_Model BLOCK;
	STATEFLOW = Keyword_Stateflow BLOCK;
	
	BLOCK = OpenB STATEMENTS CloseB;
	
	STATEMENTS = STATEMENTS STATEMENT;
	STATEMENTS = STATEMENT;
	STATEMENT = BlockType Identifier;
	STATEMENT = ListType Identifier;
	STATEMENT = Identifier INNERPART;
	STATEMENT = FormattedString;	

	INNERPART = FormattedString;
	INNERPART = Number;
	INNERPART = On;
	INNERPART = Off; 
	INNERPART = BLOCK;
	INNERPART = Array;
	
	// Regular expressions for terminals
	BlockType = @"BlockType";
	ListType = @"ListType";
	On = @"on";
	Off = @"off";
	
	Whitespace = @"[^\n\S]+";
	CommentLine = @"#[^\n]*\n";
	OpenB = @"{";
	CloseB = @"}";
	Keyword_Model = @"Model";
	Keyword_Stateflow = @"Stateflow";
	Identifier = @"[$_a-zA-Z\.]+[$_a-zA-Z\.]*\d*[$_a-zA-Z\.]*";
	FormattedString = @"\042[^\042]*\042";
	Return = @"\n+";
	Number = @"\-?\d+(\.\d+)?";
	Array = @"\[[^\n]*\]";
	
	
	
	
	
	
	
	
}