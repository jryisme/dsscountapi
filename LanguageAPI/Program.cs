using dsscountAPI.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace LanguageAPI
{
    class Program
    {
        const string connStr = "server=localhost;user=root;database=dsscount;port=3306;password=";
        //const string connStr = "server=dsscount-paris.crmnj9nnruyg.eu-west-3.rds.amazonaws.com;user=dsscount;database=dsscount;port=3306;password=XWWZw3fRGo36QyoQ";
        static void Main(string[] args)
        {
            List<Item> items = new Item().ListItemsNoCN(connStr);

            foreach (var item in items)
            {
                InitTranslate(item);
            }

        }

        private static void InitTranslate(Item item)
        {
            item.Title.TitleCn = HandleRequest(item.Title.TitleDe);

            item.Description.DescriptionCn1 = HandleRequest(item.Description.DescriptionDe1);
            item.Description.DescriptionCn1 = HandleRequest(item.Description.DescriptionDe2);
            item.Description.DescriptionCn1 = HandleRequest(item.Description.DescriptionDe3);
            item.Description.DescriptionCn4 = HandleRequest(item.Description.DescriptionDe4);
            item.Description.DescriptionCn5 = HandleRequest(item.Description.DescriptionDe5);

            item.Title.UpdateTitleCn(connStr);
            item.Description.UpdateDescriptionCn(connStr);
        }

        private static string HandleRequest(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }
            using (var client = new HttpClient())
            {
                dynamic requestBody = new JObject();
                requestBody.id = "a60092a9-c22c-42ab-8281-811a2d3d7f0d";
                requestBody.domainType = 10;
                requestBody.srcContent = raw;
                requestBody.srcLanguageType = 70;
                requestBody.tgtLanguageType = 0;

                // Get the latest 1 days items order by date desc per category
                string reqUrl = "https://www.ctc666.com:443/api/Translate/TranslateText";

                StringContent stringContent = new StringContent(requestBody.ToString());

                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var res = client.PostAsync(reqUrl, stringContent).Result;

                if (res.IsSuccessStatusCode)
                {
                    // by calling .Result you are performing a synchronous call
                    string rawContent = res.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(rawContent))
                    {
                        JObject content = JObject.Parse(rawContent);

                        string result = content["data"]["tgtContent"].ToString();

                        byte[] bytes = Encoding.Default.GetBytes(result);
                        string utf8result = Encoding.Default.GetString(bytes);

                        Console.WriteLine("Raw content: " + raw);
                        Console.WriteLine("Translated content: " + utf8result);
                        Console.WriteLine();

                        return result;
                    }
                }
            }
            return null;
        }
    }
}
