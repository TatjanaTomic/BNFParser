using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNFParser
{
    class Grammar
    {
        public string InitialToken { get; set; }
        public List<string> nonterminalTokens;
        public List<TerminalToken> terminalTokens;
        public List<(string, string)> productions;

        public static readonly string PHONE_NUMBER = "[-+]?[0-9]+(?:\\.?[0-9]+)*(?:[eE][-+]?[0-9]+)?";
        public static readonly string CONSTANT = "[-+]?[0-9]+(?:\\.?[0-9]+)*(?:[eE][-+]?[0-9]+)?";
        public static readonly string EMAIL = "[a-zA-Z0-9.!#$%&’*+\\/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*";
        public static readonly string CITY = "Istanbul";
        public static readonly string URL = "";

        public Grammar()
        {
            InitialToken = "";
            nonterminalTokens = new List<string>();
            terminalTokens = new List<TerminalToken>();
            productions = new List<(string, string)>();
        }

        internal void AddNonterminalToken(string token)
        {
            if (!nonterminalTokens.Contains(token))
                nonterminalTokens.Add(token);
        }

        internal void AddTerminalToken(string token)
        {
            string pattern = RemoveQuotemarks(token);
            terminalTokens.Add(new TerminalToken(token, pattern));
        }

        internal void AddSpecialTerminalToken(string rightSide)
        {
            if (rightSide == "broj_telefona")
                terminalTokens.Add(new TerminalToken("broj_telefona", PHONE_NUMBER));
            else if (rightSide == "brojevna_konstanta")
                terminalTokens.Add(new TerminalToken("bojevna_konstanta", CONSTANT));
            else if (rightSide == "mejl_adresa")
                terminalTokens.Add(new TerminalToken("mejl_adresa", EscapeSpecials(EMAIL)));
            else if (rightSide == "veliki_grad")
                terminalTokens.Add(new TerminalToken("veliki_grad", CITY));
            else if (rightSide == "web_link")
                terminalTokens.Add(new TerminalToken("web_link", URL));
            else if (rightSide.StartsWith("regex"))
                terminalTokens.Add(new TerminalToken(rightSide, GetRegexPattern(rightSide)));
        }
        
        internal void AddProduction(string rootToken, string definition)
        {
            productions.Add((rootToken, definition));
        }

        private string GetRegexPattern(string rightSide)
        {
            string pattern = rightSide.Substring(6); // da uklonim "regex("
            pattern = pattern.Substring(0, pattern.Length - 1); // da uklonim ")"
            pattern = EscapeSpecials(pattern);
            return pattern;
        }

        private string RemoveQuotemarks(string str)
        {
            if (str.StartsWith("\"") && str.EndsWith("\""))
                str = str.Substring(1, str.Length - 2);
            return str;
        }

        private string EscapeSpecials(string str)
        {
            string[] specials = { "\\", "[", "\"", "^", "$", ".", "|", "?", "*", "+", "(", ")", "{", "}", "<", ">" };
            foreach (string special in specials)
            {
                if (str.Contains(special))
                    str = str.Replace(special, "\\" + special);
            }
            return str;
        }

    }
}
