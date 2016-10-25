using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Spliter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Path：");
            string file = Console.ReadLine();
            Console.Write("Output Path：");
            string output = Console.ReadLine();
            Console.Write("Output Name：");
            string name = Console.ReadLine();
            Console.Write("Output Extension：");
            string extension = Console.ReadLine();
            Console.Write("Line Limit：");
            int limit = int.Parse(Console.ReadLine());
            using (StreamReader reader = new StreamReader(file))
            {
                int count = 1;
                while (reader.Peek() != -1)
                {
                    string outputname = (output.Last() == '\\' || output.Last() == '/' ? output : output + '\\') + name + count++ + extension;
                    using (StreamWriter writer = new StreamWriter(outputname))
                    {
                        Console.WriteLine("Creating {0}...", outputname);
                        for (int i = 0; i < limit && reader.Peek() != -1; i++)
                            writer.WriteLine(reader.ReadLine());
                    }
                }
            }
            Console.WriteLine("######  Finish!!  ######");
            Console.ReadLine();
        }
    }
}
