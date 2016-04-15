using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAnnealingGraph
{
   public class Link
    {
         
        public int id;
        public int start = 1;
        public int end = 2;
        public double cost = 3;
        public double capacity;
        public bool isUsed = false;
        public double consumpiton = 0;
        public int howManyTimesUsed = 0;

       public Link()
        {

        }
        public Link(int id1, int p, int k, double poj, double kosz)
        {
            id = id1;
            start = p;
            end = k;
            capacity = poj;
            cost = kosz;
        }
        public void add(int id1, int p, int k, double poj, double kosz)
        {
            id = id1;
            start = p;
            end = k;
            capacity = poj;
           cost = kosz;
        }
        public void add(int id1, int p, int k, double poj, double kosz, bool uzywane, int times)
        {
            id = id1;
            start = p;
           end = k;
            capacity= poj;
            cost = kosz;
            isUsed = uzywane;
            howManyTimesUsed = times;
        }
      
    }
    
}
