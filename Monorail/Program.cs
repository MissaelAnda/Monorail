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
                try
                {
                    game.Run();
                } catch(Exception e)
                {
                    Log.Core.Error(e.ToString());
                }
            }
        }
    }
}
