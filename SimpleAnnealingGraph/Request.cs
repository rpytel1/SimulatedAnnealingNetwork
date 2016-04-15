using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAnnealingGraph
{
   public class Request
    {
        public int requestId;
        public int startNode;
        public int endNode;
        public double size;
        public bool executed = false;
        public Request()
        {
            requestId= 0;
            size = 0;
            startNode = 0;
            endNode = 0;
        }
        public Request(int id1, int wp, int wk, double size1)
        {
            requestId = id1;
            size = size1;
            startNode = wp;
            endNode = wk;
        }
    }
}
