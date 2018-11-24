using DssCount;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace LanguageAPI
{
    class LanguageAPIProgram
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
                //dynamic requestBody = new JObject();
                //requestBody.id = "a60092a9-c22c-42ab-8281-811a2d3d7f0d";
                //requestBody.domainType = 10;

                //byte[] rawBytes = Encoding.Default.GetBytes(raw);
                //string utf8Raw = Encoding.UTF8.GetString(rawBytes);

                //requestBody.srcContent = utf8Raw;
                //requestBody.srcLanguageType = 70;
                //requestBody.tgtLanguageType = 0;

                //// Get the latest 1 days items order by date desc per category
                //string reqUrl = "https://www.ctc666.com:443/api/Translate/TranslateText";

                //StringContent stringContent = new StringContent(requestBody.ToString());

                //stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //var res = client.PostAsync(reqUrl, stringContent).Result;

                //if (res.IsSuccessStatusCode)
                //{
                //    // by calling .Result you are performing a synchronous call
                //    string rawContent = res.Content.ReadAsStringAsync().Result;

                //    if (!string.IsNullOrEmpty(rawContent))
                //    {
                //        JObject content = JObject.Parse(rawContent);

                //        string result = content["data"]["tgtContent"].ToString();

                //        byte[] bytes = Encoding.UTF8.GetBytes(result);
                //        string utf8result = Encoding.BigEndianUnicode.GetString(bytes);



                //        Console.WriteLine("Raw content: " + raw);
                //        Console.WriteLine("Translated content: " + utf8result);
                //        Console.WriteLine();

                //        return result;
                //    }
                //}

                byte[] rawBytes = Encoding.Default.GetBytes(raw.Replace("&", "und"));

                string q = Encoding.UTF8.GetString(rawBytes);

                string from = "de";
                string to = "zh";
                string appid = "20181124000238727";
                string key = "aHXVOVDFmSMtpVJrVLuN";
                string salt = "68123345678";

                string sign = appid + q + salt + key;

                sign = MD5Hash(sign);

                q = Uri.EscapeUriString(q);

                string reqUrl = "https://fanyi-api.baidu.com/api/trans/vip/translate?" + "q=" + q + "&from=" + from + "&to=" + to + "&appid=" + appid + "&salt=" + salt + "&sign=" + sign;

                var res = client.GetAsync(reqUrl).Result;

                if (res.IsSuccessStatusCode)
                {
                    // by calling .Result you are performing a synchronous call
                    string rawContent = res.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(rawContent))
                    {
                        JObject content = JObject.Parse(rawContent);

                        string result = content["trans_result"][0]["dst"].ToString();

                        byte[] bytes = Encoding.Default.GetBytes(result);
                        string utf8result = Encoding.UTF8.GetString(bytes);

                        Console.WriteLine("Raw content: " + raw);
                        Console.WriteLine("Translated content: " + utf8result);
                        Console.WriteLine();

                        return result;
                    }
                }
            }
            return null;
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
