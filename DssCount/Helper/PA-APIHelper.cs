using Nager.AmazonProductAdvertising;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DssCount.Helper
{
   public static class PA_APIHelper
    {
        public static Nager.AmazonProductAdvertising.Model.Item RequestAmazonAPI(Item itemAmazon)
        {
            try
            {
                var authentication = new AmazonAuthentication
                {
                    AccessKey = "AKIAI2PLS24TS5MED6FQ",
                    SecretKey = "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"
                };

                var wrapper = new AmazonWrapper(authentication, AmazonEndpoint.DE, "dsscount-21");

                var amazonResult = wrapper.Lookup(itemAmazon.AsinId);

                if (amazonResult == null)
                {
                    Console.WriteLine("Could not get results from amazon, item.AsinId: " + itemAmazon.AsinId);
                    return null;
                }

                var items = amazonResult.Items;

                if (items == null)
                {
                    Console.WriteLine("Could not find items from amazon, item.AsinId: " + itemAmazon.AsinId);
                    return null;
                }

                var item = items.Item;

                if (item == null || item.Length < 1)
                {
                    Console.WriteLine("Could not find item list from amazon, item.AsinId: " + itemAmazon.AsinId);
                    return null;
                }
                Thread.Sleep(1000);
                return item[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
                return null;
            }
        }
    }
}
