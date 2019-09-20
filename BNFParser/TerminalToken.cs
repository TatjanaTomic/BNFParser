using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BNFParser
{
    class TerminalToken
    {
        public string Name { get; set; }
        public string RegexPattern { get; set; }
        public Regex Definition { get; set; }

        public TerminalToken(string name, string regex)
        {
            Name = name;
            RegexPattern = regex;
            Definition = new Regex(RegexPattern);
        }
        
    }
}
