using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNFParser
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Grammar grammar = BNFAnalyzer.CheckBNF();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            Console.ReadKey();
        }
    }
}
