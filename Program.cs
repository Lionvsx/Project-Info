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
            var test2 = Functions.ReadImage(@"../../../images/coco.bmp");
            test2.DisplayImage();
            Console.WriteLine(test2);
        }
    }
}