using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Specify assembly filenames to hash");
                return;
            }
            var hash = FileHasher.Hash(args);
            Console.Write(hash);
        }
    }
}
