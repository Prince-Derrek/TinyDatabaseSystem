namespace TinyDB.Core.Parsing
{
    public enum TokenType
    {
        // Keywords
        CREATE,
        TABLE,
        INSERT,
        INTO,
        VALUES,
        SELECT,
        FROM,
        WHERE,   // Reserved for later

        // Data Types
        INT_TYPE,
        STRING_TYPE,
        BOOL_TYPE,

        // Symbols / Punctuation
        OPEN_PAREN,   // (
        CLOSE_PAREN,  // )
        COMMA,        // ,
        STAR,         // * (Wildcard)
        SEMICOLON,    // ;

        // Literals (The actual data)
        INTEGER_LITERAL, // 123
        STRING_LITERAL,  // 'Derrek'
        BOOLEAN_LITERAL, // true/false

        // Identifiers
        IDENTIFIER,      // Table or Column names (e.g., Users, Id)

        // End of File
        EOF
    }
}