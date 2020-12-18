using Monorail.Debug;
using System;

namespace Monorail
{
    class Program
    {
        static void Main(string[] args)
        {
            using (App game = new App())
            {
                game.Run();
            }
        }
    }
}
