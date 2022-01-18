using System;
using System.Drawing;

namespace Project_Info
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var test = new byte[] {255, 0, 0, 0};
            Console.WriteLine(Functions.ConvertToInt(test));

        }
    }
}