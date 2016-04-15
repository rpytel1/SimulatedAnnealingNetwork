using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAnnealingGraph
{
    class Program
    {
        static void Main(string[] args)
        {

           Network net = new Network();
           net.LoadDataFromFile();
           net.annealing();
           net.saveDataToFile();
           Console.ReadLine();
        }
    }
}
