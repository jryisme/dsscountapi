using dsscountAPI.Model;
using Nager.AmazonProductAdvertising;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace dsscountAPI
{
    class Program
    {
        const string connStr = "server=localhost;user=root;database=dsscount;port=3306;password=";

        static void Main(string[] args)
        {
            List<Category> categories = new Category().GetCategories(connStr);

            foreach (var category in categories)
            {
                GetCccItemByCategory_De(category.KeyDe, category.ID);
            }
        }

        private static async void GetCccItemByCategory_De(string keyDe, int categoryId)
        {
            try
            {
                int pageNo = 1;

                string category = keyDe;

                while (pageNo < 10)
                {
                    using (var client = new HttpClient())
                    {
                        // Get the latest 7 days items order by date desc per category
                        string reqUrl = "https://de.camelcamelcamel.com/top_drops/feed?bn=" + category + "&t=recent&i=7&s=relative&d=0&p=" + pageNo;

                        var res = client.GetAsync(reqUrl).Result;

                        if (res.IsSuccessStatusCode)
                        {
                            // by calling .Result you are performing a synchronous call
                            string content = res.Content.ReadAsStringAsync().Result;

                            if (string.IsNullOrEmpty(content))
                            {
                                break;
                            }

                            XmlDocument xDoc = new XmlDocument();

                            xDoc.LoadXml(content);

                            XmlNode root = xDoc.DocumentElement;

                            XmlNodeList items = root.SelectNodes("/rss/channel/item");

                            if (items.Count == 0)
                            {
                                break;
                            }

                            foreach (XmlNode item in items)
                            {
                                string title = item.FirstChild.InnerText;

                                Regex regex = new Regex(@"[0-9]{1,2}(.|,)[0-9]{1,2}(%|€)");
                                MatchCollection matches = regex.Matches(title);
                                if (matches.Count < 1)
                                {
                                    break;
                                }

                                decimal dropPercent = Math.Round(Convert.ToDecimal(matches[0].Value.Split('%')[0]));
                                decimal dropValue = Convert.ToDecimal(matches[1].Value.Split('€')[0].Replace(',', '.'));
                                decimal newPrice = Convert.ToDecimal(matches[2].Value.Split('€')[0].Replace(',', '.'));
                                decimal oldPrice = Convert.ToDecimal(matches[3].Value.Split('€')[0].Replace(',', '.'));


                                DateTime timeStamp = Convert.ToDateTime(item.ChildNodes[3].InnerText.Replace(" PST", ""));

                                //decimal dropPercent = Math.Round(Convert.ToDecimal(matches[0].Value.Split('%')[0]));
                                //string dropValue = matches[1].Value.Split('€')[0];
                                //string newPrice = matches[2].Value.Split('€')[0];
                                //string oldPrice = matches[3].Value.Split('€')[0];

                                //decimal dropPercent = Math.Round(Convert.ToDecimal(matches[0].Value.Split('%')[0]));
                                //decimal dropValue = Convert.ToDecimal(matches[1].Value.Split('€')[0]);
                                //decimal newPrice = Convert.ToDecimal(matches[2].Value.Split('€')[0]);
                                //decimal oldPrice = Convert.ToDecimal(matches[3].Value.Split('€')[0]);

                                string des = item.ChildNodes[2].InnerText;
                                var descriptions = des.Split('?')[0].Split(new string[] { "product/" }, StringSplitOptions.None);

                                string asinId = descriptions[descriptions.Length - 1];

                                string guid = item.ChildNodes[4].InnerText;

                                Item itemAmazon = new Item
                                {
                                    AsinId = asinId,
                                    Discount = dropPercent,
                                    ChangePrice = dropValue,
                                    NewPrice = newPrice,
                                    OldPrice = oldPrice,
                                    Guid = guid,
                                    CategoryID = categoryId,
                                    TimeStamp= timeStamp.ToShortDateString()
                                };

                                GetAndSaveAmazonItem(itemAmazon, title);
                            }
                        }
                    }

                    pageNo++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void GetAndSaveAmazonItem(Item itemAmazon,string cccTitle)
        {
            try
            {
                var authentication = new AmazonAuthentication
                {
                    AccessKey = "AKIAI2PLS24TS5MED6FQ",
                    SecretKey = "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"
                };

                var wrapper = new AmazonWrapper(authentication, AmazonEndpoint.DE, "dsscount-21");

                Console.WriteLine(itemAmazon.AsinId);

                var item = wrapper.Lookup(itemAmazon.AsinId).Items.Item[0];

                var attributes = item.ItemAttributes;

                string titleDe = attributes.Title?? cccTitle;

                var descriptionsDe = attributes.Feature;

                string url = item.DetailPageURL;

                //UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer()
                //{
                //    ApiKey = "AIzaSyCpHclA-Rgq3sN9KCLGaoVrCJze3MIy-qw"
                //});

                //var m = new Google.Apis.Urlshortener.v1.Data.Url
                //{
                //    LongUrl = item.MediumImage.URL
                //};

                //Console.WriteLine(service.Url.Insert(m).Execute().Id);


                string imageUrl = item.MediumImage.URL;

                string reviewUrl = item.CustomerReviews.IFrameURL;

                itemAmazon.Url = url;
                itemAmazon.Review = reviewUrl;
                itemAmazon.Image = item.LargeImage != null ? item.LargeImage.URL : item.ImageSets[0].LargeImage.URL;

                int itemId = itemAmazon.Save(connStr);

                Title title = new Title
                {
                    TitleDe = titleDe,
                    ItemID = itemId
                };

                Console.WriteLine("Title Object: " + title);

                title.Save(connStr);

                if(descriptionsDe == null)
                {
                    Console.WriteLine("asdasdasd");
                }

                Description description = new Description
                {
                    DescriptionDe1 = descriptionsDe != null ? (descriptionsDe.Length >= 1 ? descriptionsDe[0] : null) : null,
                    DescriptionDe2 = descriptionsDe != null ? (descriptionsDe.Length >= 2 ? descriptionsDe[1] : null) : null,
                    DescriptionDe3 = descriptionsDe != null ? (descriptionsDe.Length >= 3 ? descriptionsDe[2] : null) : null,
                    DescriptionDe4 = descriptionsDe != null ? (descriptionsDe.Length >= 4 ? descriptionsDe[3] : null) : null,
                    DescriptionDe5 = descriptionsDe != null ? (descriptionsDe.Length >= 5 ? descriptionsDe[4] : null) : null,
                    ItemID = itemId
                };

                Console.WriteLine("Description Object: " + description);

                description.Save(connStr);

                Thread.Sleep(5000);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}