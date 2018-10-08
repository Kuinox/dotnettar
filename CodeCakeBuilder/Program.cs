using System;
using System.Linq;
using CodeCake;

namespace CodeCakeBuilder
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CodeCakeApplication();
            bool interactive = !args.Contains( '-' + InteractiveAliases.NoInteractionArgument, StringComparer.OrdinalIgnoreCase );
            int result = app.Run( args );
            Console.WriteLine();
            if (!interactive) return result;
            Console.WriteLine( "Hit any key to exit. (Use -{0} parameter to exit immediately)", InteractiveAliases.NoInteractionArgument );
            Console.ReadKey();
            return result;
        }
    }
}
