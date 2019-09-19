using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BNFParser
{
    class TerminalSymbol
    {
        public TerminalSymbol() { }
        public TerminalSymbol(string name, string regex)
        {
            Name = name;
            RegexPattern = regex;
            Definition = new Regex(RegexPattern);
        }
        public string Name { get; set; }
        public string RegexPattern { get; set; }
        public Regex Definition { get; set; }
    }
}
