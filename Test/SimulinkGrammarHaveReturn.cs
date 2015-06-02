// Complete Simulink grammer
// Too much for Sylva

// Configurations
// Now only end of file marker and skip is allowed
configurations
{
	endOfFileMarker = EndOfFileMarker;
	skip = Whitespace CommentLine;
}

// Prioritized symbol regular expressions (.NET style)
production
{
	// Non-terminals
	MODELFILE = MODEL Return STATEFLOW EndOfFileMarker;
	
	MODEL = Keyword_Model BLOCK Return;
	STATEFLOW = Keyword_Stateflow BLOCK Return;
	
	BLOCK = OpenB Return STATEMENTS CloseB;
	
	STATEMENTS = STATEMENTS STATEMENT;
	STATEMENTS = STATEMENT;
	STATEMENT = BlockType Identifier Return;
	STATEMENT = ListType Identifier Return;
	STATEMENT = Identifier INNERPART Return;
	STATEMENT = FormattedString Return;	
	
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