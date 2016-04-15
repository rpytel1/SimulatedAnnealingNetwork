using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimpleAnnealingGraph
{
    public class Network
    { 
            int NodeNumber;
            int LinkNumber;
            int RequestNumber;



            double[,] flowTable;

            int[] pathTable;
            Request[] requestTable;
            bool[] visited;
            double[,] edgeTable;
            Link[] links = new Link[1];
            Link[] usedLinks;
            Link[] tmp_links;
            double[,] tmp;


            Random rnd = new Random();
            double infinity = Double.PositiveInfinity;

            double optimalCost;
            int[,] optimalPathTable;
            Link[] optimalLinkTable;



            int[,] requestPathTable;
            int[,] currRequestPathTable;







            public void LoadDataFromFile()
            {
                #region loading data
                string filePath;
                StreamReader read;
                string[] words;
                string[] words2;

                Console.WriteLine("Drag file here and press ENTER...");
                filePath = Console.ReadLine();
                if (filePath[0] == '\"') filePath = filePath.Substring(1, filePath.Length - 2);
                Console.WriteLine(" ");
                read = new StreamReader(filePath);
                String line = "";
                while (line.Length < 2 || line[0] == '#')
                {
                    line = read.ReadLine();
                }
                words = line.Split(' ');
                if (words[0] == "NODES") NodeNumber = int.Parse(words[2]);


                line = read.ReadLine();
                while (line.Length < 2 || line[0] == '#')
                {
                    line = read.ReadLine();
                }
                words = line.Split(' ');
                if (words[0] == "LINKS") LinkNumber = int.Parse(words[2]);
                links = new Link[LinkNumber + 1]; //ZERO LINK IS EMPTY
                links[0] = null;
                line = read.ReadLine(); //READ ONE LINE WITH #
                for (int i = 1; i <= LinkNumber; i++)
                {
                    line = read.ReadLine();
                    words = line.Split(' ');
                    words2 = words[3].Split('.');
                    words[3] = (words2[0] + "," + words2[1]);
                    words2 = words[4].Split('.');
                    words[4] = (words2[0] + "," + words2[1]);
                    links[i] = new Link(int.Parse(words[0]), int.Parse(words[1]), int.Parse(words[2]), double.Parse(words[3]), double.Parse(words[4]));
                }

                line = read.ReadLine();
                while (line.Length < 2 || line[0] == '#')
                {
                    line = read.ReadLine();
                }
                words = line.Split(' ');
                if (words[0] == "REQUESTS") RequestNumber = int.Parse(words[2]);
                line = read.ReadLine();
                requestTable = new Request[RequestNumber + 1];
                for (int i = 1; i <= RequestNumber; i++)
                {
                    line = read.ReadLine();
                    words = line.Split(' ');
                    words2 = words[3].Split('.');
                    words[3] = (words2[0] + "," + words2[1]);
                    requestTable[i] = new Request(int.Parse(words[0]), int.Parse(words[1]), int.Parse(words[2]), double.Parse(words[3]));
                }
                read.Close();
                #endregion
                #region network building
                //filling data about network
                int a, b;
                edgeTable = new double[NodeNumber + 1, NodeNumber + 1];

                for (int j = 1; j <= NodeNumber; j++)
                    for (int k = 1; k <= NodeNumber; k++)
                    {

                        edgeTable[j, k] = infinity;
                    }

                for (int i = 1; i <= LinkNumber; i++)
                {
                    a = links[i].start;
                    b = links[i].end;

                    edgeTable[a, b] = links[i].capacity;

                #endregion

                }

            }
            public void saveDataToFile()
            {
                #region Results and saving data to file

                    
                Console.WriteLine("Drag output file and press ENTER:");
                string nazwa_pliku = Console.ReadLine();

                Console.WriteLine(" ");
                StreamWriter zapis = new StreamWriter(nazwa_pliku);
                zapis.WriteLine("# cost of solution");
                zapis.WriteLine("cost = " + optimalCost);
                zapis.WriteLine(" ");
                zapis.WriteLine("# number of requests");
                zapis.WriteLine("REQUESTS = " + RequestNumber);
                zapis.WriteLine("# every Request is id. of request and set of used links");
                Console.WriteLine("cost: " + optimalCost);
                for (int i = 1; i <= RequestNumber; i++)
                {
                    String write_tekst = (i.ToString() + " ");
                    String Request_tekst = ("Request" + i + ": ");
                    for (int a = 1; a <= NodeNumber; a++)
                    {
                        if (optimalPathTable[i, a] != 0)
                        {
                            Request_tekst += (optimalPathTable[i, a] + " ");
                            write_tekst += (optimalPathTable[i, a] + " ");
                        }
                    }

                    Console.WriteLine(Request_tekst);
                    zapis.WriteLine(write_tekst);
                }
                zapis.WriteLine(" ");
                zapis.WriteLine("# number of links");
                zapis.WriteLine("links = " + LinkNumber);
                zapis.WriteLine("# every Link is: id, number of used modules");
                Console.WriteLine("links i number of used modules: ");
                for (int i = 1; i <= LinkNumber; i++)
                {
                    if (optimalLinkTable[i].isUsed)
                    {
                        Console.WriteLine(links[i].id + " " + optimalLinkTable[i].howManyTimesUsed);
                        zapis.WriteLine(links[i].id + " " + optimalLinkTable[i].howManyTimesUsed);
                    }
                }
                zapis.WriteLine("# links that are not used are not seen");
                zapis.Close();
                Console.WriteLine(" ");
                #endregion
                Console.WriteLine("Press ENTER to finish");
                Console.ReadLine();
            }

            public void annealing()
            {

                double cost;
                int T = 1000;

                BasicSolution();

                currRequestPathTable = new int[RequestNumber + 1, NodeNumber + 1];
                for (int i = 1; i <= RequestNumber; i++)
                {
                    for (int j = 1; j <= NodeNumber; j++)
                    {
                        currRequestPathTable[i, j] = requestPathTable[i, j];
                    }
                }
                cost = costOfExecution();

                while (T > 1)
                {

                    nearbySolution();

                    #region Comparing the costs
                    double actualCost = costOfExecution();
                    if (cost > actualCost)
                    {
                        cost = actualCost;
                        for (int m = 1; m <= RequestNumber; m++)
                        {
                            for (int n = 1; n <= NodeNumber; n++)
                            {
                                optimalPathTable[m, n] = requestPathTable[m, n];
                            }
                        }
                        Link[] wsk = usedLinks;
                        for (int m = 1; m <= LinkNumber; m++)
                        {
                            optimalLinkTable[m].add(wsk[m].id, wsk[m].start, wsk[m].end, wsk[m].capacity, wsk[m].cost, wsk[m].isUsed, wsk[m].howManyTimesUsed);
                        }
                    }
                    #endregion

                    #region going to next possible solution

                    double delta_f = actualCost - cost;
                    double dystryb = Math.Exp(-delta_f / T);
                    double x = rnd.NextDouble();

                    if (x < dystryb)
                    {
                        for (int i = 1; i <= RequestNumber; i++)
                        {
                            for (int j = 1; j <= NodeNumber; j++)
                            {
                                currRequestPathTable[i, j] = requestPathTable[i, j];
                            }
                        }
                    }




                    #endregion
                    T--;


                }

                optimalCost = cost;

            }
            void BasicSolution()
            {
                #region preapering data

                usedLinks = new Link[LinkNumber + 1];
                tmp_links = new Link[LinkNumber + 1];

                for (int i = 1; i <= LinkNumber; i++)
                {
                    usedLinks[i] = new Link();
                    usedLinks[i].add(links[i].id, links[i].start, links[i].end, links[i].capacity, links[i].cost, links[i].isUsed, links[i].howManyTimesUsed);
                    tmp_links[i] = new Link();
                }

                flowTable = new double[NodeNumber + 1, NodeNumber + 1];

                for (int m = 1; m <= NodeNumber; m++)
                    for (int j = 1; j <= NodeNumber; j++)
                    {
                        flowTable[m, j] = edgeTable[m, j];
                    }

                requestPathTable = new int[RequestNumber + 1, NodeNumber + 1];
                for (int i = 1; i <= RequestNumber; i++)
                {
                    for (int j = 1; j <= NodeNumber; j++)
                    {
                        requestPathTable[i, j] = 0;
                    }
                }
                tmp = flowTable;

                #endregion

                #region finding shortest paths
                for (int i = 1; i <= RequestNumber; i++)
                {
                    int start = requestTable[i].startNode;
                    int meta = requestTable[i].endNode;
                    double size = requestTable[i].size;
                    Dijkstra(start, meta, size, i);
                }
                #endregion

                #region setting optimal execution

                //optymalny_cost =  costOfExecution();

                optimalLinkTable = new Link[LinkNumber + 1];

                for (int i = 1; i <= LinkNumber; i++)
                {
                    optimalLinkTable[i] = new Link();
                    optimalLinkTable[i] = usedLinks[i];
                }

                optimalPathTable = new int[RequestNumber + 1, NodeNumber + 1];

                for (int i = 1; i <= RequestNumber; i++)
                    for (int j = 1; j <= NodeNumber; j++)
                    {
                        optimalPathTable[i, j] = requestPathTable[i, j];
                    }
                #endregion
            }

            void Dijkstra(int start, int meta, double size, int nr)
            {
                #region finding shortest path
                Node[] Prelength = new Node[NodeNumber + 1];
                for (int i = 1; i <= NodeNumber; i++)
                {
                    Prelength[i] = new Node();
                }

                PirorityQueue<double, int> queue = new PirorityQueue<double, int>();
                //queue of Nodes to see and their prelength

                for (int i = 1; i <= NodeNumber; i++)
                {
                    if (i != start)
                    {

                        if (edgeTable[start, i] != infinity)
                        {
                            Prelength[i].setPrelength(1);
                            Prelength[i].setBefore(start);
                        }
                        //adding fresh Nodes

                        if (Prelength[i].getPrelength() != infinity)
                        {
                            Element<double, int> newElem = new Element<double, int>(Prelength[i].getPrelength(), i);
                            queue.add(newElem);
                        }
                    }
                }

                int currNode = 0;
                while ((queue.getSize() != 0) && (currNode != meta))
                {
                    Element<double, int> curr = queue.getMin();
                    currNode = curr.getData();
                    for (int i = 1; i <= NodeNumber; i++)
                    {
                        if ((i != currNode) && (edgeTable[currNode, i] != infinity))
                        {
                            double newLength = Prelength[currNode].getPrelength() + 1;

                            double oldPreLength = Prelength[i].getPrelength();
                            bool isBetter = false;
                            if (oldPreLength < newLength)
                                isBetter = true;
                            if ((oldPreLength == infinity) && (newLength != infinity))
                                isBetter = true;

                            if (isBetter)
                            {
                                Prelength[i].setPrelength(newLength);
                                Prelength[i].setBefore(currNode);                  
                                Element<double, int> newElem = new Element<double, int>(Prelength[i].getPrelength(), i);
                                queue.add(newElem);
                            }
                        }
                    }
                }

                #endregion

                #region read data
                currNode = meta;
                Link[] wsk = usedLinks;
                for (int i = 1; i <= LinkNumber; i++)
                {
                    tmp_links[i].add(wsk[i].id, wsk[i].start, wsk[i].end, wsk[i].capacity, wsk[i].cost, wsk[i].isUsed, wsk[i].howManyTimesUsed);
                }

                while (currNode != start)
                {

                    int n = Prelength[currNode].getBefore();
                    if (flowTable[n, currNode] < size)
                    {
                        getModules(n, currNode, size);
                    }
                    else
                    {
                        flowTable[n, currNode] -= size;
                    }
                    bool condition = true;
                    int g = 1;
                    while (condition)
                    {

                        if ((tmp_links[g].start == n) && (tmp_links[g].end == currNode))
                        {
                            condition = false;
                        }

                        else
                            g++;
                    }

                    if (tmp_links[g].isUsed == false)
                    {
                        tmp_links[g].isUsed = true;
                        tmp_links[g].howManyTimesUsed++;
                    }


                    currNode = Prelength[currNode].getBefore();
                }


                currNode = meta;

                while (currNode != start)         
                {
                    requestPathTable[nr, currNode] = Prelength[currNode].getBefore();
                    currNode = Prelength[currNode].getBefore();
                }

                for (int i = 1; i <= LinkNumber; i++)
                {

                    usedLinks[i].add(tmp_links[i].id, tmp_links[i].start, tmp_links[i].end, tmp_links[i].capacity, tmp_links[i].cost, tmp_links[i].isUsed, tmp_links[i].howManyTimesUsed);

                }


                #endregion

            }
            void nearbySolution()
            {



                bool isExecuted = false;

                while (isExecuted == false)
                {

                    #region data preperation
                    flowTable = new double[NodeNumber + 1, NodeNumber + 1];
                    for (int m = 1; m <= NodeNumber; m++)
                        for (int j = 1; j <= NodeNumber; j++)
                        {
                            flowTable[m, j] = edgeTable[m, j];
                        }
                    requestPathTable = new int[RequestNumber + 1, NodeNumber + 1];
                    for (int k = 1; k <= RequestNumber; k++)
                    {
                        for (int j = 1; j <= NodeNumber; j++)
                        {
                            requestPathTable[k, j] = 0;
                            requestTable[k].executed = false;

                        }
                    }

                    usedLinks = new Link[LinkNumber + 1];
                    for (int h = 1; h <= LinkNumber; h++)
                    {
                        usedLinks[h] = new Link();
                        usedLinks[h].add(links[h].id, links[h].start, links[h].end, links[h].capacity, links[h].cost, links[h].isUsed, links[h].howManyTimesUsed);
                        tmp_links[h].add(links[h].id, links[h].start, links[h].end, links[h].capacity, links[h].cost, links[h].isUsed, links[h].howManyTimesUsed);

                    }

                    #endregion

                    for (int i = 0; i < RequestNumber; i++)
                    {
                        int k = getRandomRequest(RequestNumber - i);
                        if (i != 0)
                        {
                            executeLikeBefore(k);
                            requestTable[k].executed = true;
                            isExecuted = true;
                        }
                        else
                        {
                            isExecuted = buildPath(k);

                        }

                        if (isExecuted == false)
                        {
                            break;
                        }
                        else
                            requestTable[k].executed = true;

                    }
                }

            }
            void executeLikeBefore(int n)
            {

                double size = requestTable[n].size;
                int currNode = requestTable[n].endNode;

                while (currNode != requestTable[n].startNode)
                {
                    int m = currRequestPathTable[n, currNode];
                    if (flowTable[m, currNode] < size)
                    {
                        getModules(n, currNode, size);
                    }
                    else
                    {
                        flowTable[n, currNode] -= size;
                    }
                    bool condition = true;
                    int g = 1;
                    while (condition)
                    {

                        if ((tmp_links[g].start == m) && (tmp_links[g].end == currNode))
                        {
                            condition = false;
                        }

                        else
                            g++;
                    }

                    if (tmp_links[g].isUsed == false)
                    {
                        tmp_links[g].isUsed = true;
                        tmp_links[g].howManyTimesUsed++;
                    }
                    currNode = m;

                }
                for (int i = 1; i <= LinkNumber; i++)
                {

                    usedLinks[i].add(tmp_links[i].id, tmp_links[i].start, tmp_links[i].end, tmp_links[i].capacity, tmp_links[i].cost, tmp_links[i].isUsed, tmp_links[i].howManyTimesUsed);

                }
                for (int i = 1; i <= NodeNumber; i++)
                {
                    requestPathTable[n, i] = currRequestPathTable[n, i];
                }

            }

            bool buildPath(int n)
            {
                #region data preperation


                tmp_links = new Link[LinkNumber + 1];
                Link[] wsk = usedLinks;
                for (int i = 1; i <= LinkNumber; i++)
                {
                    tmp_links[i] = new Link();
                    tmp_links[i].add(wsk[i].id, wsk[i].start, wsk[i].end, wsk[i].capacity, wsk[i].cost, wsk[i].isUsed, wsk[i].howManyTimesUsed);
                }

                int start, meta, currNode;
                Request currRequest = requestTable[n];

                start = currRequest.startNode;
                meta = currRequest.endNode;
                currNode = start;

                tmp = new double[NodeNumber + 1, NodeNumber + 1];

                for (int i = 1; i <= NodeNumber; i++)
                    for (int j = 1; j <= NodeNumber; j++)
                    {
                        tmp[i, j] = flowTable[i, j];
                    }

                visited = new bool[NodeNumber + 1];
                pathTable = new int[NodeNumber + 1];

                for (int i = 1; i <= NodeNumber; i++)
                {
                    visited[i] = false;
                    pathTable[i] = 0;

                }

                #endregion

                #region randoming the path

                while (currNode != meta)
                {
                    int randomSet = howManyLinksFromNode(currNode);

                    if (randomSet == 0)
                    {          
                        //if no way from Node then total break and start all one more time 
                        return false;

                    }
                    int randomNum = rnd.Next(1, randomSet + 1);

                    int m = 0, i = 0;

                    while (m != randomNum)
                    {
                        i++;
                        if (i <= NodeNumber)
                        {
                            if ((edgeTable[currNode, i] != infinity) && (edgeTable[currNode, i] != 0))
                            //chosingpaths that exist and are possible to use
                            {
                                m++;
                            }
                        }
                        else
                            return false;


                    }
                    if (tmp[currNode, i] < requestTable[n].size)
                    {
                        bool bought = getModules(currNode, i, requestTable[n].size);
                    }
                    else
                    {

                        tmp[currNode, i] -= requestTable[n].size;
                        int g = 1;
                        bool condition = true;

                        while (condition)
                        {

                            if ((tmp_links[g].start == currNode) && (tmp_links[g].end == i))
                            {
                                condition = false;
                            }

                            else
                                g++;
                        }

                        if (tmp_links[g].isUsed == false)
                        {
                            tmp_links[g].isUsed = true;
                            tmp_links[g].howManyTimesUsed++;
                        }



                    }

                    pathTable[i] = currNode;
                    currNode = i;

                    if ((visited[currNode] == true))
                    {
                        //total break!=>starting finding nearbySoltion one more time

                        return false;
                    }
                    visited[currNode] = true;

                }

                #endregion

                #region update flowTable and requestPathTable

                currNode = meta;
                while (currNode != start)
      
                {
                    flowTable[pathTable[currNode], currNode] -= currRequest.size;
                    currNode = pathTable[currNode];

                }

                for (int i = 1; i <= NodeNumber; i++)
                
                {
                    requestPathTable[n, i] = pathTable[i];
                }

                for (int i = 1; i <= LinkNumber; i++)
 
                {

                    usedLinks[i].add(tmp_links[i].id, tmp_links[i].start, tmp_links[i].end, tmp_links[i].capacity, tmp_links[i].cost, tmp_links[i].isUsed, tmp_links[i].howManyTimesUsed);

                }
                #endregion
                return true;
            }

            bool getModules(int start, int end, double size)
            {

                for (int i = 1; i <= LinkNumber; i++)
                {
                    if ((links[i].start == start) && (links[i].end == end))
                    {
                        double sizeNeeded = size - tmp[start, end];
                        int moduleNumbers = Convert.ToInt32(Math.Ceiling(sizeNeeded / links[i].capacity));

                        tmp_links[i].isUsed = true;
                        tmp_links[i].howManyTimesUsed += moduleNumbers;

                        tmp[start, end] = tmp[start, end] + moduleNumbers * links[i].capacity - size;
                        return true;
                    }
                }
                return false;
            }

            double costOfExecution()
            {
                double cost = 0;
                for (int i = 1; i <= LinkNumber; i++)
                {
                    if (usedLinks[i].isUsed)
                    {
                        cost += usedLinks[i].howManyTimesUsed * usedLinks[i].cost;
                    }
                }
                return cost;
            }
            int howManyLinksFromNode(int curr)
            {
                int zRet = 0;
                for (int i = 1; i <= NodeNumber; i++)
                {
                    if ((edgeTable[curr, i] != infinity))
                    {
                        zRet++;
                    }
                }
                return zRet;
            }

            int NumberOfLinksFromNode(int n, double size)
            {
                int zRet = 0;
                for (int i = 1; i <= NodeNumber; i++)
                {
                    if ((edgeTable[n, i] != infinity) && (flowTable[n, i] >= size))
                    {
                        zRet++;
                    }
                }
                return zRet;
            }
            int getRandomRequest(int j)
           {
                int randomNum;

                randomNum = rnd.Next(1, j + 1);
                int i = 0, k = 0;//k to indeks zapotrzebowania ktore zostalo wylosowane
                while (i != randomNum)
                {
                    k++;
                    if (requestTable[k].executed == false)
                    {
                        i++;
                    }

                }
                return k;
            }

        }
}

