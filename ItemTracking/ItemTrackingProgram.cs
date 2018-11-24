using DssCount;
using dsscountAPI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ItemTracking
{
    class ItemTrackingProgram
    {
        const string connStr = "server=localhost;user=root;database=dsscount;port=3306;password=";
        //const string connStr = "server=dsscount-paris.crmnj9nnruyg.eu-west-3.rds.amazonaws.com;user=dsscount;database=dsscount;port=3306;password=XWWZw3fRGo36QyoQ";

        static void Main(string[] args)
        {
            Console.WriteLine("Connstr: " + connStr);
            Console.WriteLine();

            List<Item> items = new Item().GetAllAsinId(connStr);

            foreach (var item in items)
            {
                CrawlerProgram.UpdateItemPrice(item);
                Thread.Sleep(400);
            }
        }
    }
}
