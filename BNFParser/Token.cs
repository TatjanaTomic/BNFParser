using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNFParser
{
    class Token
    {
        public Token(string n, string d)
        {
            Name = n;
            Definition = d;
        }
        public string Name { get; }
        public string Definition { get; }
    }
}
