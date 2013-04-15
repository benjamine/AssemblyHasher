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
            bool ignoreVersions = false;
            var arguments = args.ToList();
            if (arguments.Any(arg => arg == "--ignore-versions"))
            {
                arguments.RemoveAll(arg => arg == "--ignore-versions");
                ignoreVersions = true;
            }

            if (args.Length < 1)
            {
                Console.WriteLine("Specify assembly filenames to hash");
                Console.WriteLine("   --ignore-versions: ignore assembly version and assembly file version attributes");
                return;
            }
            var hash = FileHasher.Hash(ignoreVersions, arguments.ToArray());
            Console.Write(hash);
        }
    }
}
