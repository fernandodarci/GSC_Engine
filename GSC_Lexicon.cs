
using System;
using System.Collections.Generic;
using System.Linq;
using GSC_Engine;

/// <summary>
/// The lexicon will be used to define and parse the script, using some keys and a syntax to build messages.
/// </summary>
public static class GSC_Lexicon
{
    //The structure of the grammar used by define each part of the script meaning.
    public class GSC_GrammarSymbols
    {
        public readonly string Keyword;
        public GSC_ScriptTokenType[] Tokens;

        public GSC_GrammarSymbols(string keyword, params GSC_ScriptTokenType[] tokens)
        {
            Keyword = keyword;
            Tokens = tokens;
        }
    }

    public static GSC_ScriptTokenType[][] HandlerFormats = new GSC_ScriptTokenType[][]
    {
        new[] { GSC_ScriptTokenType.KEYWORD }, 
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.HANDLER },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.STRING },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.STRING },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.INTEGER },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.INTEGER },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING },
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.HANDLER, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING },

    };

    public static List<GSC_GrammarSymbols> Lexicon = new List<GSC_GrammarSymbols>()
    {
        
    };

    public static List<GSC_Message> Parse(List<GSC_ScriptToken> tokens)
    {
        if (tokens == null || tokens.Count == 0 || tokens[0].TokenType != GSC_ScriptTokenType.KEYWORD ||
            tokens[tokens.Count - 1].TokenType != GSC_ScriptTokenType.END)
        {
            return new List<GSC_Message> { new GSC_Message("Undefined") };
        }

        List<List<GSC_ScriptToken>> tokensplit = new List<List<GSC_ScriptToken>>();
        List<GSC_ScriptToken> scriptPart = new List<GSC_ScriptToken>();

        while (tokens.Count > 1)
        {
            if (tokens[0].TokenType == GSC_ScriptTokenType.KEYWORD && scriptPart.Count > 0)
            {
                tokensplit.Add(scriptPart);
                scriptPart = new List<GSC_ScriptToken>();
            }
            scriptPart.Add(tokens[0]);
            tokens.RemoveAt(0);
        }

        if (scriptPart.Count > 0)
        {
            tokensplit.Add(scriptPart);
        }

        List<GSC_Message> messages = new List<GSC_Message>();
        foreach (List<GSC_ScriptToken> part in tokensplit) messages.Add(BuildMessageFromTokens(part));
        return messages;
    }


    private static GSC_Message BuildMessageFromTokens(List<GSC_ScriptToken> messageTokens)
    {
        if (messageTokens == null || messageTokens.Count == 0) return new GSC_Message("Unrecognized");

        GSC_GrammarSymbols symbol = Lexicon.FirstOrDefault(s => s.Keyword == messageTokens[0].Token);

        if (symbol == null) return new GSC_Message("Unrecognized");

        //Insert the code to process the token sequence

        return new GSC_Message("Unrecognized");
    }


}