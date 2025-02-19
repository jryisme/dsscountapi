﻿using DssCount;
using DssCount.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace dsscountAPI
{
    public class CrawlerProgram
    {
        //const string connStr = "server=localhost;user=root;database=dsscount;port=3306;password=";
        const string connStr = "server=dsscount-paris.crmnj9nnruyg.eu-west-3.rds.amazonaws.com;user=dsscount;database=dsscount;port=3306;password=XWWZw3fRGo36QyoQ";

        static void Main(string[] args)
        {
            string amazonPath = @"C:\xampp\htdocs\dsscountAPI\Logs\DssCountCrawler\log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            using (StreamWriter writer = new StreamWriter(amazonPath))
            {
                Console.SetOut(writer);

                Console.WriteLine("Connstr: " + connStr);
                Console.WriteLine();

                List<Category> categories = new Category().GetCategories(connStr);

                foreach (var category in categories)
                {
                    GetCccItemByCategory_De(category.KeyDe, category.ID, 1);
                }
            }
        }

        private static async void GetCccItemByCategory_De(string keyDe, int categoryId, int pageNo)
        {
            try
            {
                if (pageNo < 10)
                {
                    string category = keyDe;

                    using (var client = new HttpClient())
                    {
                        //string reqUrl = "https://de.camelcamelcamel.com/top_drops/feed?bn=" + category + "&t=recent&i=7&s=relative&d=0&p=" + pageNo;

                        // Get the latest 1 days items order by date desc per category
                        string reqUrl = "https://de.camelcamelcamel.com/top_drops/feed?bn=" + category + "&t=recent&i=2&s=relative&d=0&p=" + pageNo;

                        pageNo++;

                        var res = client.GetAsync(reqUrl).Result;

                        if (res.IsSuccessStatusCode)
                        {
                            // by calling .Result you are performing a synchronous call
                            string content = res.Content.ReadAsStringAsync().Result;

                            if (string.IsNullOrEmpty(content))
                            {
                                return;
                            }

                            XmlDocument xDoc = new XmlDocument();

                            xDoc.LoadXml(content);

                            XmlNode root = xDoc.DocumentElement;

                            XmlNodeList items = root.SelectNodes("/rss/channel/item");

                            if (items.Count == 0) // Run to the end of the pager
                            {
                                Console.WriteLine("Run to the end of page in CCC. Go to next category.");
                                return;
                            }

                            foreach (XmlNode item in items)
                            {
                                string des = item.ChildNodes[2].InnerText;
                                var descriptions = des.Split('?')[0].Split(new string[] { "product/" }, StringSplitOptions.None);

                                string asinId = descriptions[descriptions.Length - 1];

                                Console.WriteLine("Found one item from CCC, AsinID: " + asinId);

                                var dbItem = new Item(asinId).FindByASINID(connStr);

                                if (dbItem != null) // The item is already there, check the price from amazon
                                {
                                    Console.WriteLine("The item is already existed, check its price to update. Item.id: " + dbItem.ID + " . Item.ASINID: " + dbItem.AsinId);
                                    UpdateExistingAmazonItem(dbItem);
                                    Console.WriteLine();
                                }
                                else
                                {
                                    SaveNewAmazonItem(keyDe, categoryId, pageNo, item, asinId);
                                }
                            }
                        }
                        GetCccItemByCategory_De(keyDe, categoryId, pageNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }

        private static void SaveNewAmazonItem(string keyDe, int categoryId, int pageNo, XmlNode item, string asinId)
        {
            try
            {
                string rawTitle = item.FirstChild.InnerText;
                Console.WriteLine("Raw title from CCC: " + rawTitle);

                Regex regex = new Regex(@"([0-9]{1,3}(.|,)){1,4}[0-9]{1,2}(%|€)");
                MatchCollection matches = regex.Matches(rawTitle);
                if (matches.Count < 4)
                {
                    GetCccItemByCategory_De(keyDe, categoryId, pageNo);
                }

                decimal discount = Convert.ToDecimal(matches[matches.Count - 4].Value.Split('%')[0]);
                decimal changePrice = Convert.ToDecimal(matches[matches.Count - 3].Value.Split('€')[0].Replace(".", "").Replace(",", "")) / 100;
                decimal newPrice = Convert.ToDecimal(matches[matches.Count - 2].Value.Split('€')[0].Replace(".", "").Replace(",", "")) / 100;
                decimal oldPrice = Convert.ToDecimal(matches[matches.Count - 1].Value.Split('€')[0].Replace(".", "").Replace(",", "")) / 100;

                DateTime timeStamp = Convert.ToDateTime(item.ChildNodes[3].InnerText.Replace(" PST", ""));

                string guid = item.ChildNodes[4].InnerText;

                Item itemAmazon = new Item
                {
                    AsinId = asinId,
                    CccGuid = guid,
                    CategoryID = categoryId,
                    TimeStamp = DateTime.Now.ToString()
                };

                var rawItem = PA_APIHelper.RequestAmazonAPI(itemAmazon);

                if (rawItem == null)
                {
                    Console.WriteLine();
                    return;
                }

                Console.WriteLine("Found the item from Amazon API, ASIN ID:" + rawItem.ASIN);

                var rawItemAttributes = rawItem.ItemAttributes;

                if (rawItemAttributes == null)
                {
                    Console.WriteLine("Item attribute is null:" + rawItem.ASIN);
                    Console.WriteLine();
                    return;
                }

                if (rawItem.Offers != null && rawItem.Offers.Offer != null && rawItem.Offers.Offer[0] != null && rawItem.Offers.Offer[0].OfferAttributes != null && rawItem.Offers.Offer[0].OfferAttributes.Condition != null && rawItem.Offers.Offer[0].OfferAttributes.Condition == "New" && rawItem.Offers.Offer[0].OfferListing != null && rawItem.Offers.Offer[0].OfferListing[0] != null && rawItem.Offers.Offer[0].OfferListing[0].Price != null) // Take the price from offers
                {
                    newPrice = Convert.ToDecimal(rawItem.Offers.Offer[0].OfferListing[0].Price.Amount) / 100;
                }
                else
                {
                    if (rawItemAttributes.ListPrice == null)
                    {
                        Console.WriteLine("Unable to get the item price from Amazon API");
                        return;
                    }

                    newPrice = Convert.ToDecimal(rawItemAttributes.ListPrice.Amount) / 100;
                }

                if (newPrice >= oldPrice)
                {
                    Console.WriteLine("Item new price is larger than old price, so not include it in database");
                    Console.WriteLine();
                    return;
                }

                string titleDe = rawItemAttributes.Title ?? rawTitle;

                var descriptionsDe = rawItemAttributes.Feature;

                string url = rawItem.DetailPageURL;

                string reviewUrl = rawItem.CustomerReviews?.IFrameURL;

                itemAmazon.Url = url;
                itemAmazon.Review = reviewUrl;
                itemAmazon.Image = rawItem.MediumImage != null ? rawItem.MediumImage.URL : (rawItem.ImageSets != null ? (rawItem.ImageSets[0] != null ? (rawItem.ImageSets[0].MediumImage?.URL) : null) : null);
                itemAmazon.ImageLarge = rawItem.LargeImage != null ? rawItem.LargeImage.URL : (rawItem.ImageSets != null ? (rawItem.ImageSets[0] != null ? (rawItem.ImageSets[0].LargeImage?.URL) : null) : null);
                itemAmazon.ImageSmall = rawItem.SmallImage != null ? rawItem.SmallImage.URL : (rawItem.ImageSets != null ? (rawItem.ImageSets[0] != null ? (rawItem.ImageSets[0].SmallImage?.URL) : null) : null);

                string titleCn = TranslationHelper.HandleRequest(titleDe);

                Title title = new Title
                {
                    TitleDe = titleDe,
                    TitleCn = titleCn
                };

                int titleId = title.Save(connStr);

                if (descriptionsDe == null)
                {
                    Console.WriteLine("descriptionsDe is null");
                }

                Description description = new Description
                {
                    DescriptionDe1 = descriptionsDe != null ? (descriptionsDe.Length >= 1 ? descriptionsDe[0] : null) : null,
                    DescriptionDe2 = descriptionsDe != null ? (descriptionsDe.Length >= 2 ? descriptionsDe[1] : null) : null,
                    DescriptionDe3 = descriptionsDe != null ? (descriptionsDe.Length >= 3 ? descriptionsDe[2] : null) : null,
                    DescriptionDe4 = descriptionsDe != null ? (descriptionsDe.Length >= 4 ? descriptionsDe[3] : null) : null,
                    DescriptionDe5 = descriptionsDe != null ? (descriptionsDe.Length >= 5 ? descriptionsDe[4] : null) : null,
                    DescriptionCn1 = descriptionsDe != null ? (descriptionsDe.Length >= 1 ? (!string.IsNullOrEmpty(descriptionsDe[0]) ? TranslationHelper.HandleRequest(descriptionsDe[0]) : null) : null) : null,
                    DescriptionCn2 = descriptionsDe != null ? (descriptionsDe.Length >= 2 ? (!string.IsNullOrEmpty(descriptionsDe[1]) ? TranslationHelper.HandleRequest(descriptionsDe[1]) : null) : null) : null,
                    DescriptionCn3 = descriptionsDe != null ? (descriptionsDe.Length >= 3 ? (!string.IsNullOrEmpty(descriptionsDe[2]) ? TranslationHelper.HandleRequest(descriptionsDe[2]) : null) : null) : null,
                    DescriptionCn4 = descriptionsDe != null ? (descriptionsDe.Length >= 4 ? (!string.IsNullOrEmpty(descriptionsDe[3]) ? TranslationHelper.HandleRequest(descriptionsDe[3]) : null) : null) : null,
                    DescriptionCn5 = descriptionsDe != null ? (descriptionsDe.Length >= 5 ? (!string.IsNullOrEmpty(descriptionsDe[4]) ? TranslationHelper.HandleRequest(descriptionsDe[4]) : null) : null) : null
                };

                int descriptionId = description.Save(connStr);

                itemAmazon.DescriptionID = descriptionId;
                itemAmazon.TitleID = titleId;
                itemAmazon.IsTranslatedCn = 1;
                itemAmazon.Dealer = 0; //means amazon

                int itemId = itemAmazon.Save(connStr);
                Console.WriteLine("Add new item, item.id: " + itemId);

                DiscountItem dsItem = new DiscountItem
                {
                    NewPrice = newPrice,
                    OldPrice = oldPrice,
                    ChangePrice = changePrice,
                    Discount = discount,
                    ItemID = itemId,
                    TimeStamp = timeStamp.ToString(),
                    CategoryID = categoryId,
                    Dealer = 0
                };
                Console.WriteLine("Add new discount item, item.id: " + itemId);

                dsItem.Save(connStr);

                Price price = new Price
                {
                    ItemID = itemId,
                    Value = newPrice,
                    Timestamp = DateTime.Now.ToString()
                };
                Console.WriteLine("Add new price, item.id: " + itemId);
                Console.WriteLine();

                price.Save(connStr);

                Thread.Sleep(9000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }

        public static void UpdateExistingAmazonItem(Item itemAmazon)
        {
            try
            {
                var rawItem = PA_APIHelper.RequestAmazonAPI(itemAmazon);

                if (rawItem == null)
                {
                    Console.WriteLine();
                    return;
                }

                var rawItemAttributes = rawItem.ItemAttributes;

                Console.WriteLine("Found the item from Amazon API, ASIN ID: " + rawItem.ASIN);

                decimal newPrice = 0;

                if (rawItem.Offers != null && rawItem.Offers.Offer != null && rawItem.Offers.Offer[0] != null && rawItem.Offers.Offer[0].OfferAttributes != null && rawItem.Offers.Offer[0].OfferAttributes.Condition != null && rawItem.Offers.Offer[0].OfferAttributes.Condition == "New" && rawItem.Offers.Offer[0].OfferListing != null && rawItem.Offers.Offer[0].OfferListing[0] != null && rawItem.Offers.Offer[0].OfferListing[0].Price != null) // Take the price from offers
                {
                    newPrice = Convert.ToDecimal(rawItem.Offers.Offer[0].OfferListing[0].Price.Amount) / 100;
                }
                else
                {
                    if (rawItemAttributes.ListPrice == null)
                    {
                        Console.WriteLine("Unable to get the item price from Amazon API");
                        return;
                    }

                    newPrice = Convert.ToDecimal(rawItemAttributes.ListPrice.Amount) / 100;
                }

                decimal latestPrice = new Price().GetLatestPrice(connStr, itemAmazon.ID);

                Console.WriteLine("New price: " + newPrice + " and latest price: " + latestPrice);

                if (newPrice != latestPrice)
                {
                    Console.WriteLine("Update the price for item, item.id: " + itemAmazon.ID);
                    Price price = new Price
                    {
                        ItemID = itemAmazon.ID,
                        Value = newPrice,
                        Timestamp = DateTime.Now.ToString()
                    };

                    price.Save(connStr);

                    DiscountItem dsItem = new DiscountItem().FindByItemID(connStr, itemAmazon.ID);

                    if (newPrice < latestPrice) // Discount applied
                    {
                        Console.WriteLine("Discount happened for item, item.id: " + itemAmazon.ID);

                        if (dsItem != null)
                        {
                            dsItem.NewPrice = newPrice;
                            dsItem.OldPrice = latestPrice;
                            dsItem.ChangePrice = latestPrice - newPrice;
                            dsItem.Discount = (latestPrice - newPrice) / latestPrice*100;
                            dsItem.TimeStamp = DateTime.Now.ToString();
                            dsItem.ItemID = itemAmazon.ID;

                            Console.WriteLine("Update existing discount item.");
                            dsItem.Update(connStr);
                        }
                        else // The item might have been removed from discount list
                        {
                            DiscountItem newDsItem = new DiscountItem
                            {
                                NewPrice = newPrice,
                                OldPrice = latestPrice,
                                ChangePrice = latestPrice - newPrice,
                                Discount = (latestPrice - newPrice) / latestPrice*100,
                                TimeStamp = DateTime.Now.ToString(),
                                ItemID = itemAmazon.ID,
                                CategoryID = itemAmazon.CategoryID,
                                Dealer = 0
                            };

                            Console.WriteLine("Add new discount item.");
                            newDsItem.Save(connStr);
                        }
                        Console.WriteLine();
                    }
                    else if (newPrice > latestPrice) // Remove it from the discount list
                    {
                        if (dsItem != null)
                        {
                            Console.WriteLine("Remove the existing discount item.");
                            dsItem.Delete(connStr);
                        }
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }
    }
}