// Configurations
// Now only end of file marker and skip is allowed
configurations
{
	endOfFileMarker = EndOfFileMarker;
	skip = Return Whitespace Comment BlockComment;
}

// Prioritized symbol regular expressions (.NET style)
production
{
	// Non-terminals
	CFGFILE = CONFIGURATIONBODY PRODUCTIONBODY EndOfFileMarker;
	PRODUCTIONBODY = ProductionStart OpenB STATEMENT CloseB;
	PRODUCTIONBODY = ProductionStart OpenB CloseB;
	STATEMENT = STATEMENT PRODUCTION;
	STATEMENT = STATEMENT REGEX;
	STATEMENT = PRODUCTION;
	STATEMENT = REGEX;		
	PRODUCTION = Identifier Equal DERIVATION Semicolon;
	DERIVATION = Identifier;
	DERIVATION = DERIVATION Identifier;		
	REGEX = Identifier Equal RegularExpressionPattern Semicolon;
	CONFIGURATIONBODY = ConfigurationStart OpenB EOFSTATEMENT SKIPSTATEMENT CloseB;
	CONFIGURATIONBODY = ConfigurationStart OpenB SKIPSTATEMENT EOFSTATEMENT CloseB;
	EOFSTATEMENT = EOF Equal Identifier Semicolon;
	SKIPSTATEMENT = SKIP Equal DERIVATION Semicolon;
	
	
	// Regular expressions for terminals
	ConfigurationStart = @"configurations";
	ProductionStart = @"production";
	ProductionPrefix = @"Production";
	RegExPrefix = @"RegEx";
	EOF = @"endOfFileMarker";
	SKIP = @"skip";
	Regex = @"regex";
	Whitespace = @"[^\n\S]+";
	Return = @"\n";
	OpenB = @"{";
	CloseB = @"}";
	Equal = @"\=";
	Semicolon = @";";
	FormattedString = @"(?<!@)\042[^\042\n]*\042";
	RegularExpressionPattern = @"@\042[^\042]*\042(?=;)";			
	Identifier = @"[_a-zA-Z]+[_a-zA-Z0-9]*";
	Comment = @"//[^\n]*";
	BlockComment = @"/\*(((?!\*/).)*[\r\n]?)*\*/";
}