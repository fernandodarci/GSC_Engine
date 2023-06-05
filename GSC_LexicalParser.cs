using System;
using System.Collections.Generic;

namespace GSC_Engine
{
    public enum GSC_ScriptTokenType
    {
        UNDEFINED,
        KEYWORD,
        STRING,
        INTEGER,
        HANDLER,
        END,
    }

    public class GSC_ScriptToken
    {
        public readonly GSC_ScriptTokenType TokenType;
        public readonly string Token;

        public GSC_ScriptToken(GSC_ScriptTokenType tokenType, string token)
        {
            TokenType = tokenType;
            Token = token;
        }
    }



    public static class GSC_LexicalParser
    {
        private static GSC_ScriptToken Undefined(string val)
            => new GSC_ScriptToken(GSC_ScriptTokenType.UNDEFINED, val);

        private static GSC_ScriptToken Keyword(string val)
            => new GSC_ScriptToken(GSC_ScriptTokenType.KEYWORD, val);

        private static GSC_ScriptToken String(string val)
            => new GSC_ScriptToken(GSC_ScriptTokenType.STRING, val);

        private static GSC_ScriptToken Integer(string val)
            => new GSC_ScriptToken(GSC_ScriptTokenType.INTEGER, val);

        private static GSC_ScriptToken End()
            => new GSC_ScriptToken(GSC_ScriptTokenType.END, ";");

        private static GSC_ScriptToken Handler(string val)
         => new GSC_ScriptToken(GSC_ScriptTokenType.HANDLER, val);

        public static List<GSC_ScriptToken> Parse(string script)
        {
            if (string.IsNullOrEmpty(script))
                return new List<GSC_ScriptToken>(new[] { Undefined(null) });

            List<GSC_ScriptToken> tokens = new List<GSC_ScriptToken>();

            string[] splitted = script.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);

            bool OpenBracket = false;
            bool UndefinedBracketed = false;
            string Bracketed = "";

            foreach (string parts in splitted)
            {
                string part = parts.Trim();
                
                if (part == "@" || part == "$" || part == "[" || part == "]" || part == ";")
                {
                    tokens.Add(Undefined(part));
                }
                else
                {
                    if (OpenBracket == false)
                    {
                        if (part.StartsWith("@") && !part.EndsWith(";"))
                        {
                            tokens.Add(IsValidString(part.Substring(1)) ? Keyword(part) : Undefined(part));
                        }
                        else if (part.StartsWith("["))
                        {
                            if (part.EndsWith("]"))
                            {
                                tokens.Add(Undefined(part));
                            }
                            else
                            {
                                if (!IsValidString(part.Substring(1))) UndefinedBracketed = true;

                                Bracketed = part.Substring(1);
                                OpenBracket = true;
                            }

                        }
                        else if (part.EndsWith("]") || part.EndsWith("];"))
                        {
                            foreach (string uPart in splitted)
                            {
                                if (uPart == part) break;

                                if (Bracketed == "") Bracketed = uPart;
                                else Bracketed += $" {uPart}";
                            }
                            
                            tokens.Clear();
                            tokens.Add(Undefined(Bracketed));
                            Bracketed = "";
                            if (part.EndsWith(";"))
                            {
                                tokens.Add(End());
                                return tokens;
                            }
                        }
                        else if (part.EndsWith(";"))
                        {
                            string endPart = part.TrimEnd(';');
                            if (endPart.StartsWith("@")) tokens.Add(Keyword(endPart));
                            else if (endPart.StartsWith("$")) tokens.Add(Handler(endPart));
                            else if (IsValidString(endPart)) tokens.Add(String(endPart));
                            else if (int.TryParse(endPart, out _)) tokens.Add(Integer(endPart));
                            else tokens.Add(Undefined(endPart));
                            
                            tokens.Add(End());
                            return tokens;
                        }
                        else if (IsValidString(part)) tokens.Add(String(part));
                        else if (int.TryParse(part, out _)) tokens.Add(Integer(part));
                        else tokens.Add(Undefined(part));
                    }
                    else //Open bracket is true
                    {
                        if (IsValidString(part))
                        {
                            Bracketed += $" {part}";
                        }
                        else if (part.EndsWith("]"))
                        {
                            string uPart = part.TrimEnd(']');
                            UndefinedBracketed = !IsValidString(uPart);
                            Bracketed += $" {uPart}";

                            tokens.Add(UndefinedBracketed ? Undefined(Bracketed) : String(Bracketed));
                            UndefinedBracketed = false;
                            OpenBracket = false;
                            Bracketed = "";
                        }
                        else if (part.EndsWith("];"))
                        {
                            string uPart = part.TrimEnd(';', ']');
                            if (UndefinedBracketed is false) UndefinedBracketed = !IsValidString(uPart);
                            Bracketed += $" {uPart}";

                            tokens.Add(UndefinedBracketed ? Undefined(Bracketed) : String(Bracketed));
                            UndefinedBracketed = false;
                            Bracketed = "";
                            tokens.Add(End());
                            return tokens;
                        }
                        else if (part.StartsWith("@") || part.StartsWith("[") || part.StartsWith("$") ||int.TryParse(part, out _) || part.EndsWith(";"))
                        {
                            UndefinedBracketed = true;
                            Bracketed += $" {part}";

                            if (part.EndsWith(";"))
                            {
                                tokens.Add(Undefined(Bracketed));
                                tokens.Add(End());
                            }
                        }
                    }
                }
            }

            return tokens;
        }

        private static bool IsValidString(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsLetter(c)) return false;
            }

            return true;
        }
    }
}
