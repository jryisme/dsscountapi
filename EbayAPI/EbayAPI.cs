using DssCount;
using DssCount.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace EbayAPI
{
    public static class EbayAPI
    {
        public static void StartEbayAPI(string connStr)
        {
            Dictionary<string, int> queries = new Dictionary<string, int>
            {
                { "31786,180345", 27  },
                { "58058", 8 },
                { "10682,14324,258031", 34 },
                { "10290,4196,49750", 28 },
                { "10968,15124", 28 },
                { "15032", 11 }
            };

            foreach (var query in queries)
            {
                Console.WriteLine("Start a new category, category: " + query.Value + "; query key: " + query.Key);
                Console.WriteLine();

                ListCat(connStr, query.Value, query.Key);
            }
        }

        private static void ListCat(string connStr, int category, string catQuery, int pageNo = 1)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    Console.WriteLine("Page number: " + pageNo);

                    // Get the core item make-up and perfumes
                    string reqUrl = "https://epndeals.api.ebay.com/epndeals/v1?marketplace=de&campaignid=5338441456&categoryid=" + catQuery + "&count=200&offset=" + pageNo + "&toolid=100034&rotationId=707-53477-19255-0&type=CORE&format=json";

                    var res = client.GetAsync(reqUrl).Result;

                    if (res.IsSuccessStatusCode)
                    {
                        // by calling .Result you are performing a synchronous call
                        string content = res.Content.ReadAsStringAsync().Result;

                        if (string.IsNullOrEmpty(content))
                        {
                            return;
                        }
                        JObject result = JObject.Parse(content);

                        JToken items = result["items"];

                        if (!items.HasValues)
                        {
                            return;
                        }

                        foreach (var item in items)
                        {
                            Console.WriteLine("Ebay ID: " + item["itemId"].ToString());

                            decimal newPrice = Convert.ToDecimal(item["price"].ToString()) * 100 / 100;

                            decimal oldPrice = string.IsNullOrEmpty(item["originPrice"].ToString()) ? 0 : Convert.ToDecimal(item["originPrice"].ToString().Replace("EUR ", "").Replace(".", "").Replace(",", "")) / 100;

                            decimal changePrice = (oldPrice > 0 && oldPrice > newPrice) ? oldPrice - newPrice : 0;

                            decimal discount = (changePrice > 0) ? changePrice / oldPrice * 100 : 0;

                            DateTime endTime = Convert.ToDateTime(item["endsAt"].ToString());

                            Item existItem = new Item(item["itemId"].ToString()).FindByASINID(connStr);

                            if (existItem != null) // The item is aleady there
                            {
                                Console.WriteLine("The item is already existed.");

                                DiscountItem existDsItem = new DiscountItem().FindByItemID(connStr, existItem.ID);

                                if (existDsItem == null) //The discount item is already removed
                                {
                                    DiscountItem dsItem = new DiscountItem
                                    {
                                        NewPrice = newPrice,
                                        OldPrice = oldPrice,
                                        ChangePrice = changePrice,
                                        Discount = discount,
                                        ItemID = existItem.ID,
                                        TimeStamp = DateTime.Now.ToString(),
                                        CategoryID = category,
                                        EndTime = endTime.ToString(),
                                        Dealer = 1
                                    };
                                    dsItem.Save(connStr);
                                    Console.WriteLine("Add new discount ebay item, item.id: " + existItem.ID);
                                }
                                else if (existDsItem.NewPrice != newPrice) // update existing dsItem
                                {
                                    Console.WriteLine("The latest saved price: " + existDsItem.NewPrice + " is not the same as the latest price: " + newPrice + ". Update the ds item.");
                                    existDsItem.NewPrice = newPrice;
                                    existDsItem.OldPrice = oldPrice;
                                    existDsItem.ChangePrice = changePrice;
                                    existDsItem.Discount = discount;
                                    existDsItem.EndTime = endTime.ToString();
                                    existDsItem.TimeStamp = DateTime.Now.ToString();
                                    existDsItem.Update(connStr);
                                }
                                Console.WriteLine();
                                continue;
                            }

                            string desUrl = "http://open.api.ebay.com/shopping?callname=GetSingleItem&responseencoding=JSON&appid=RuoyuJia-DssCount-PRD-760bef6c5-5a772b10&siteid=77&version=967&ItemID=" + item["itemId"].ToString() + "&IncludeSelector=Description";

                            var response = client.GetAsync(desUrl).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                string desContent = response.Content.ReadAsStringAsync().Result;

                                if (string.IsNullOrEmpty(desContent))
                                {
                                    continue;
                                }
                                JObject desResult = JObject.Parse(desContent);

                                string descriptionText = desResult["Item"]["Description"].ToString();

                                Description description = new Description
                                {
                                    DescriptionDe1 = !string.IsNullOrEmpty(descriptionText) ? descriptionText : null
                                };

                                int descriptionId = description.Save(connStr);

                                Title title = new Title
                                {
                                    TitleDe = item["title"].ToString(),
                                    TitleCn = TranslationHelper.HandleRequest(item["title"].ToString())
                                };

                                Thread.Sleep(300);

                                int titleId = title.Save(connStr);

                                Item ebayItem = new Item
                                {
                                    AsinId = item["itemId"].ToString(),
                                    CategoryID = category,
                                    TimeStamp = DateTime.Now.ToString(),
                                    Url = item["itemUrl"].ToString(),
                                    DealUrl = item["dealUrl"].ToString(),
                                    Dealer = 1,//means ebay
                                    ImageLarge = desResult["Item"]["PictureURL"] != null ? desResult["Item"]["PictureURL"][0].ToString() : item["imageUrl"].ToString(),
                                    TitleID = titleId,
                                    DescriptionID = descriptionId
                                };

                                int itemId = ebayItem.Save(connStr);
                                Console.WriteLine("Add new ebay item, item.id: " + itemId);

                                DiscountItem dsItem = new DiscountItem
                                {
                                    NewPrice = newPrice,
                                    OldPrice = oldPrice,
                                    ChangePrice = changePrice,
                                    Discount = discount,
                                    ItemID = itemId,
                                    TimeStamp = DateTime.Now.ToString(),
                                    CategoryID = category,
                                    EndTime = endTime.ToString(),
                                    Dealer = 1
                                };
                                Console.WriteLine("Add new discount ebay item, item.id: " + itemId);

                                dsItem.Save(connStr);

                                Price price = new Price
                                {
                                    ItemID = itemId,
                                    Value = newPrice,
                                    Timestamp = DateTime.Now.ToString()
                                };
                                Console.WriteLine("Add new ebay price, item.id: " + itemId);
                                Console.WriteLine();

                                price.Save(connStr);
                            }
                        }
                        pageNo++;
                        ListCat(connStr, category, catQuery, pageNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }
    }
}
