using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCSV;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Need to input csv file.....");
                return;
            }


            
            CSV csv = new CSV();
            csv.ReadFromFile(args[0]);

            Console.WriteLine("csv rows:" + csv.GetRows() + "  " );
            csv.WriteToFile("OUT_TEST.csv"); 
        }
    }
}
