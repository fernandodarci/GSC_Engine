
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

    public static GSC_ScriptTokenType[][] Formats = new GSC_ScriptTokenType[4][]
    {
        new[] { GSC_ScriptTokenType.KEYWORD }, //GSC_Message
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING }, //GSC_Message<string>
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.INTEGER }, //GSC_Message<string,int>
        new[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING }, //GSC_Message<string,string>
    };

    public static List<GSC_GrammarSymbols> Lexicon = new()
    {
        new GSC_GrammarSymbols("@from", Formats[1]),
        new GSC_GrammarSymbols("@to", Formats[1]),
        new GSC_GrammarSymbols("@getall", Formats[0]),
        new GSC_GrammarSymbols("@get", Formats[2]),
        new GSC_GrammarSymbols("@erase", Formats[1]),
        // Quantitative methods
        // Validators
        new GSC_GrammarSymbols("@name", Formats[1]),
        new GSC_GrammarSymbols("@keyexist", Formats[1]),
        new GSC_GrammarSymbols("@keynotexist", Formats[1]),
        new GSC_GrammarSymbols("@equals", Formats[2]),
        new GSC_GrammarSymbols("@notequals", Formats[2]),
        new GSC_GrammarSymbols("@greater", Formats[2]),
        new GSC_GrammarSymbols("@greaterequals", Formats[2]),
        new GSC_GrammarSymbols("@less", Formats[2]),
        new GSC_GrammarSymbols("@lessequals", Formats[2]),
        new GSC_GrammarSymbols("@higher", Formats[1]),
        new GSC_GrammarSymbols("@lowest", Formats[1]),
        new GSC_GrammarSymbols("@mostcommon", Formats[1]),
        new GSC_GrammarSymbols("@lesscommon", Formats[1]),
        new GSC_GrammarSymbols("@distinct", Formats[1]),
        // Operators
        new GSC_GrammarSymbols("@set", Formats[2]),
        new GSC_GrammarSymbols("@remove", Formats[1]),
        new GSC_GrammarSymbols("@add", Formats[2]),
        new GSC_GrammarSymbols("@subtract", Formats[2]),
        new GSC_GrammarSymbols("@multiply", Formats[2]),
        new GSC_GrammarSymbols("@divide", Formats[2]),
        new GSC_GrammarSymbols("@dividerounded", Formats[2]),
        new GSC_GrammarSymbols("@remainder", Formats[2]),
        new GSC_GrammarSymbols("@min", Formats[2]),
        new GSC_GrammarSymbols("@max", Formats[2]),
        // Qualitative Methods
        // Validators
        new GSC_GrammarSymbols("@isgroup", Formats[1]),
        new GSC_GrammarSymbols("@notgroup", Formats[1]),
        new GSC_GrammarSymbols("@isqly", Formats[2]),
        new GSC_GrammarSymbols("@notqly", Formats[2]),
        // Operators
        new GSC_GrammarSymbols("@setqly", Formats[2]),
        new GSC_GrammarSymbols("@removeqly", Formats[2]),
    };

    public static List<GSC_Message> Parse(List<GSC_ScriptToken> tokens)
    {
        if (tokens == null || tokens.Count == 0 ||
            tokens.Any(x => x.TokenType == GSC_ScriptTokenType.UNDEFINED)) return new List<GSC_Message>();
        
        List<GSC_Message> messages = new();
        List<GSC_ScriptToken> currentMessageTokens = new();

        foreach (GSC_ScriptToken token in tokens)
        {
            if (token.TokenType == GSC_ScriptTokenType.KEYWORD)
            {
                if (currentMessageTokens.Count > 0)
                {
                    GSC_Message message = BuildMessageFromTokens(currentMessageTokens);
                    if (message != null) messages.Add(message);
                    currentMessageTokens.Clear();
                }
            }

            currentMessageTokens.Add(token);
        }

        // Check if there are remaining tokens after the last keyword
        if (currentMessageTokens.Count > 0)
        {
            GSC_Message message = BuildMessageFromTokens(currentMessageTokens);
            if (message != null) messages.Add(message);
        }

        return messages;
    }

    private static GSC_Message BuildMessageFromTokens(List<GSC_ScriptToken> messageTokens)
    {
        if (messageTokens == null || messageTokens.Count == 0)
            return new GSC_Message("Unrecognized");
      
        // Verifica se a keyword está presente no léxico
        GSC_GrammarSymbols symbol = Lexicon.FirstOrDefault(s => s.Keyword == messageTokens[0].Token);

        if (symbol == null) return new GSC_Message("Unrecognized");
        
        //Para cada formato, se o formato correspondente for igual a lista, construa a mensagem e retorne.
        for(int i = 0; i < Formats.Length; i++)
        {
            if (messageTokens.Select(x => x.TokenType).ToList().SequenceEqual(Formats[i]))
            {
                return i switch
                {
                    0 => new GSC_Message(messageTokens[0].Token),
                    1 => new GSC_Message<string>(messageTokens[0].Token,messageTokens[1].Token),
                    2 => new GSC_Message<string,int>(messageTokens[0].Token,messageTokens[1].Token,int.Parse(messageTokens[2].Token)),
                    3 => new GSC_Message<string,string>(messageTokens[0].Token,messageTokens[1].Token,messageTokens[2].Token),
                    _ => new GSC_Message("Unrecognized"),
                };
            }
        }

        return new GSC_Message("Unrecognized");
    }


}