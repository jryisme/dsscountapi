using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayAPI
{
    class EbayAPIProgram
    {
        //const string connStr = "server=localhost;user=root;database=dsscount;port=3306;password=";
        const string connStr = "server=dsscount-paris.crmnj9nnruyg.eu-west-3.rds.amazonaws.com;user=dsscount;database=dsscount;port=3306;password=XWWZw3fRGo36QyoQ";

        static void Main(string[] args)
        {
            string amazonPath = @"C:\xampp\htdocs\dsscountAPI\Logs\EbayAPI\log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            using (StreamWriter writer = new StreamWriter(amazonPath))
            {
                Console.SetOut(writer);

                Console.WriteLine("Connstr: " + connStr);
                Console.WriteLine();

                EbayAPI.StartEbayAPI(connStr);
            }
        }
    }
}
