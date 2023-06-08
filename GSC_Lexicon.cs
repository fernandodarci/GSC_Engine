
using System;
using System.Collections.Generic;
using System.Linq;
namespace GSC_Engine
{
    public static class GSC_ScriptEntries
    {
        #region KEYWORDS WITH NO ARGUMENTS
        public const string K01 = "@dump";
        public const string K02 = "@get_all";
        public const string K03 = "@from";
        public const string K04 = "@summary";
        #endregion

        #region KEYWORDS WITH STRING ARGUMENT

        public const string KS01 = "@feed";
        public const string KS02 = "@create";
        public const string KS03 = "@get_all";
        public const string KS04 = "@get";
        public const string KS05 = "@get_dictionary";
        public const string KS06 = "@from";
        public const string KS07 = "@get_dictionary_byname";
        public const string KS08 = "@to";
        public const string KS09 = "@remove_dictionary";
        public const string KS10 = "@remove_from";
        public const string KS11 = "@get_all_byname";
        public const string KS12 = "@remove";
        public const string KS13 = "@remove_byname";
        public const string KS14 = "@with_quantitative_key";
        public const string KS15 = "@withot_quantitative_key";
        public const string KS16 = "@remove_quantitative_key";
        public const string KS17 = "@select_from_max";
        public const string KS18 = "@select_notfrom_max";
        public const string KS19 = "@select_from_min";
        public const string KS20 = "@select_notfrom_min";
        public const string KS21 = "@select_from_minmax";
        public const string KS22 = "@select_notfrom_minmax";
        public const string KS23 = "@select_from_average";
        public const string KS24 = "@select_notfrom_average";
        public const string KS25 = "@select_from_range";
        public const string KS26 = "@select_notfrom_range";
        public const string KS27 = "@select_byname";

        #endregion

        #region KEYWORDS WITH STRING AND INTEGER ARGUMENTS

        public const string KSI01 = " ";
        public const string KSI02 = " ";
        public const string KSI03 = " ";
        public const string KSI04 = " ";
        public const string KSI05 = " ";
        public const string KSI06 = " ";
        public const string KSI07 = " ";
        public const string KSI08 = " ";
        public const string KSI09 = " ";
        public const string KSI10 = " ";
        public const string KSI11 = " ";
        public const string KSI12 = " ";
        public const string KSI13 = " ";
        public const string KSI14 = " ";
        public const string KSI15 = " ";

        #endregion

        #region KEYWORDS WITH STRING AND STRING ARGUMENTS

        public const string KSS01 = "@get_byname";
        public const string KSS02 = "@get_byid";
        public const string KSS03 = "@remove_byname";
        public const string KSS04 = "@remove_byid";
        public const string KSS05 = "@select_onboth";
        public const string KSS06 = "@select_onlyfirst";
        public const string KSS07 = "@select_not_onboth";
        public const string KSS08 = "@select_quantitative_key";
        public const string KSS09 = "@select_not_quantitative_key";
        public const string KSS10 = "@select_qualitative_key";
        public const string KSS11 = "@select_not_qualitative_key";
        public const string KSS12 = " ";
        public const string KSS13 = " ";
        public const string KSS14 = " ";
        public const string KSS15 = " ";
        public const string KSS16 = " ";
        public const string KSS17 = " ";
        public const string KSS18 = " ";
        public const string KSS19 = " ";
        public const string KSS20 = " ";
        public const string KSS21 = " ";
        public const string KSS22 = " ";
        public const string KSS23 = " ";

        #endregion

        #region KEYWORDS WITH STRING, STRING AND INTEGER ARGUMENTS

        public const string KSSI01 = "@select_equals";
        public const string KSSI02 = "@select_notequals";
        public const string KSSI03 = "@select_greater";
        public const string KSSI04 = "@select_greater_equals";
        public const string KSSI05 = "@select_less";
        public const string KSSI06 = "@select_less_equals";

        #endregion

        #region KEYWORDS WITH STRING, STRING AND STRING ARGUMENTS

        public const string KSSS01 = "@select_qualitative";
        public const string KSSS02 = "@select_not_qualitative";

        #endregion
    }

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

        public static GSC_ScriptTokenType[][] Patterns = new GSC_ScriptTokenType[][]
        {
            //K
            new GSC_ScriptTokenType[] { GSC_ScriptTokenType.KEYWORD }, 
            //KS
            new GSC_ScriptTokenType[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING }, 
            //KSS
            new GSC_ScriptTokenType[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING },
            //KSI
            new GSC_ScriptTokenType[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.INTEGER },
            //KSSS
            new GSC_ScriptTokenType[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING },
            //KSSI
            new GSC_ScriptTokenType[] { GSC_ScriptTokenType.KEYWORD, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.STRING, GSC_ScriptTokenType.INTEGER },
        };



        public static List<GSC_GrammarSymbols> Lexicon = new List<GSC_GrammarSymbols>()
        {
            new GSC_GrammarSymbols(GSC_ScriptEntries.K01,Patterns[0]),
            new GSC_GrammarSymbols(GSC_ScriptEntries.K02,Patterns[0]),
            new GSC_GrammarSymbols(GSC_ScriptEntries.K03,Patterns[0]),
            new GSC_GrammarSymbols(GSC_ScriptEntries.K04,Patterns[0]),

        };

        public static List<GSC_Message> Parse(this List<GSC_ScriptToken> tokens)
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
            List<GSC_ScriptToken> processedTokens = messageTokens.ProcessGrammarSymbols();
            foreach (GSC_ScriptToken token in processedTokens) Console.Write($"{token.TokenType} ");
            Console.WriteLine();
            foreach (GSC_ScriptToken token in processedTokens) Console.Write($"{token.Token} ");
            GSC_GrammarSymbols symbol = Lexicon.Find(x => x.Keyword == processedTokens[0].Token);

            if (symbol != null)
            {
                GSC_ScriptTokenType[] pattern = processedTokens.Select(x => x.TokenType).ToArray();
                int patternIndex = Patterns.FindPatternIndex(pattern);

                switch (patternIndex)
                {
                    case 0: return new GSC_Message(processedTokens[0].Token);
                    case 1: return new GSC_Message<string>(processedTokens[0].Token, processedTokens[1].Token);
                    case 2: return new GSC_Message<string, string>(processedTokens[0].Token, processedTokens[1].Token, processedTokens[2].Token);
                    case 3: return new GSC_Message<string, int>(processedTokens[0].Token, processedTokens[1].Token, int.Parse(processedTokens[2].Token));
                    case 4: return new GSC_Message<string, string, string>(processedTokens[0].Token, processedTokens[1].Token, processedTokens[2].Token, processedTokens[3].Token);
                    case 5: return new GSC_Message<string, string, int>(processedTokens[0].Token, processedTokens[1].Token, processedTokens[2].Token, int.Parse(processedTokens[3].Token));
                };
            }

            return new GSC_Message("Unrecognized");
        }

        public static List<GSC_ScriptToken> ProcessGrammarSymbols(this List<GSC_ScriptToken> tokens)
        {
            if (tokens.Count == 0) return new List<GSC_ScriptToken>();

            GSC_ScriptToken keywordToken = tokens[0];

            if (keywordToken.TokenType != GSC_ScriptTokenType.KEYWORD) return tokens;

            List<GSC_ScriptToken> processedTokens = new List<GSC_ScriptToken>();
            processedTokens.Add(tokens[0]);

            if (tokens.Count > 1)
            {
                tokens.RemoveAt(0);
                while (tokens[0].TokenType == GSC_ScriptTokenType.HANDLER)
                {
                    processedTokens[0] = new GSC_ScriptToken(keywordToken.TokenType, keywordToken.Token.ConcatenateStrings(tokens[0].Token));
                    tokens.RemoveAt(0);
                }

                processedTokens.AddRange(tokens);
            }

            return processedTokens;
        }
    }
}