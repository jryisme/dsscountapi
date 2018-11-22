﻿using dsscountAPI.Model;
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
                GetCccItemByCategory_De(category.KeyDe, category.ID, 1);
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
                        string reqUrl = "https://de.camelcamelcamel.com/top_drops/feed?bn=" + category + "&t=recent&i=7&s=relative&d=0&p=" + pageNo;

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
                                string guid = item.ChildNodes[4].InnerText;

                                Console.WriteLine("Found one item from CCC, Guid: " + guid);

                                var dbItem = new Item(guid).FindByGuid(connStr);

                                if (dbItem != null) // The item is already there, check the price from amazon
                                {
                                    Console.WriteLine("The item is already existed, check its price to update. Item.id: " + dbItem.ID + " . Item.ASINID: " + dbItem.AsinId);
                                    UpdateItemPrice(dbItem);
                                    Console.WriteLine();
                                    continue;
                                }

                                string title = item.FirstChild.InnerText;
                                Console.WriteLine("Raw title from CCC: " + title);

                                Regex regex = new Regex(@"([0-9]{1,3}(.|,)){1,4}[0-9]{1,2}(%|€)");
                                MatchCollection matches = regex.Matches(title);
                                if (matches.Count < 4)
                                {
                                    GetCccItemByCategory_De(keyDe, categoryId, pageNo);
                                }

                                decimal discount = Math.Round(Convert.ToDecimal(matches[matches.Count - 4].Value.Split('%')[0]));
                                decimal changePrice = Convert.ToDecimal(matches[matches.Count - 3].Value.Split('€')[0].Replace(',', '.'));
                                decimal newPrice = Convert.ToDecimal(matches[matches.Count - 2].Value.Split('€')[0].Replace(',', '.'));
                                decimal oldPrice = Convert.ToDecimal(matches[matches.Count - 1].Value.Split('€')[0].Replace(',', '.'));

                                DateTime timeStamp = Convert.ToDateTime(item.ChildNodes[3].InnerText.Replace(" PST", ""));

                                string des = item.ChildNodes[2].InnerText;
                                var descriptions = des.Split('?')[0].Split(new string[] { "product/" }, StringSplitOptions.None);

                                string asinId = descriptions[descriptions.Length - 1];

                                Item itemAmazon = new Item
                                {
                                    AsinId = asinId,
                                    CccGuid = guid,
                                    CategoryID = categoryId,
                                    TimeStamp = timeStamp.ToString()
                                };
                                GetAndSaveAmazonItem(itemAmazon, title, discount, changePrice, newPrice, oldPrice);
                            }
                        }
                        GetCccItemByCategory_De(keyDe, categoryId, pageNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void UpdateItemPrice(Item itemAmazon)
        {
            try
            {
                var rawItem = RequestAmazonAPI(itemAmazon);

                if (rawItem == null)
                {
                    Console.WriteLine();
                    return;
                }

                var rawItemAttributes = rawItem.ItemAttributes;

                Console.WriteLine("Found the item from Amazon API, ASIN ID: " + rawItem.ASIN);

                if (rawItemAttributes.ListPrice == null)
                {
                    Console.WriteLine("Unable to get the item price from Amazon API");
                    return;
                }

                decimal newPrice = 0;

                if (rawItem.Offers != null && rawItem.Offers.Offer != null && rawItem.Offers.Offer[0] != null && rawItem.Offers.Offer[0].OfferAttributes != null && rawItem.Offers.Offer[0].OfferAttributes.Condition != null && rawItem.Offers.Offer[0].OfferAttributes.Condition == "New" && rawItem.Offers.Offer[0].OfferListing != null && rawItem.Offers.Offer[0].OfferListing[0] != null && rawItem.Offers.Offer[0].OfferListing[0].Price != null) // Take the price from offers
                {
                    newPrice = Convert.ToDecimal(rawItem.Offers.Offer[0].OfferListing[0].Price.Amount) / 100;
                }
                else
                {
                    newPrice = Convert.ToDecimal(rawItemAttributes.ListPrice.Amount) / 100;
                }

                decimal latestPrice = new Price().GetLatestPrice(connStr, itemAmazon.ID);

                Console.WriteLine("New price: " + newPrice + " and latest price: " + latestPrice);

                if (newPrice.ToString() != latestPrice.ToString())
                {
                    Console.WriteLine("Update the price for item, item.id: " + itemAmazon.ID);
                    Price price = new Price
                    {
                        ItemID = itemAmazon.ID,
                        Value = newPrice,
                        Timestamp = DateTime.Now.ToString()
                    };

                    price.Save(connStr);

                    if (newPrice < latestPrice) // Discount applied
                    {
                        Console.WriteLine("Discount happened for item, item.id: " + itemAmazon.ID);
                        DiscountItem dsItem = new DiscountItem().FindByItemID(connStr, itemAmazon.ID);

                        dsItem.NewPrice = newPrice;
                        dsItem.OldPrice = latestPrice;
                        dsItem.ChangePrice = latestPrice - newPrice;
                        dsItem.Discount = Math.Round((latestPrice - newPrice) / latestPrice);
                        dsItem.TimeStamp = DateTime.Now.ToString();

                        if (dsItem != null)
                        {
                            Console.WriteLine("Update existing discount item.");
                            dsItem.Update(connStr);
                        }
                        else // The item might be removed from discount list
                        {
                            Console.WriteLine("Add new discount item.");
                            dsItem.Save(connStr);
                        }
                        Console.WriteLine();
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void GetAndSaveAmazonItem(Item itemAmazon, string cccTitle, decimal discount, decimal changePrice, decimal newPrice, decimal oldPrice)
        {
            try
            {
                var rawItem = RequestAmazonAPI(itemAmazon);

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
                }

                newPrice = 0;

                if (rawItem.Offers != null && rawItem.Offers.Offer != null && rawItem.Offers.Offer[0] != null && rawItem.Offers.Offer[0].OfferAttributes != null && rawItem.Offers.Offer[0].OfferAttributes.Condition != null && rawItem.Offers.Offer[0].OfferAttributes.Condition == "New" && rawItem.Offers.Offer[0].OfferListing != null && rawItem.Offers.Offer[0].OfferListing[0] != null && rawItem.Offers.Offer[0].OfferListing[0].Price != null) // Take the price from offers
                {
                    newPrice = Convert.ToDecimal(rawItem.Offers.Offer[0].OfferListing[0].Price.Amount) / 100;
                }
                else if (rawItemAttributes.ListPrice != null & rawItemAttributes.ListPrice.Amount != null)
                {
                    newPrice = Convert.ToDecimal(rawItemAttributes.ListPrice.Amount) / 100;
                }

                string titleDe = rawItemAttributes.Title ?? cccTitle;

                var descriptionsDe = rawItemAttributes.Feature;

                string url = rawItem.DetailPageURL;

                string reviewUrl = rawItem.CustomerReviews?.IFrameURL;

                itemAmazon.Url = url;
                itemAmazon.Review = reviewUrl;
                itemAmazon.Image = rawItem.LargeImage != null ? rawItem.LargeImage.URL : rawItem.ImageSets[0].LargeImage != null ? rawItem.ImageSets[0].LargeImage.URL : null;

                //todo: update title, description and item
                //if (itemAmazon.FindByGuid(connStr)) // The item is already there, update just in case of any price change lately
                //{
                //    return;
                //}

                Title title = new Title
                {
                    TitleDe = titleDe
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
                    DescriptionDe5 = descriptionsDe != null ? (descriptionsDe.Length >= 5 ? descriptionsDe[4] : null) : null
                };

                int descriptionId = description.Save(connStr);

                itemAmazon.DescriptionID = descriptionId;
                itemAmazon.TitleID = titleId;

                int itemId = itemAmazon.Save(connStr);
                Console.WriteLine("Add new item, item.id: " + itemId);

                DiscountItem dsItem = new DiscountItem
                {
                    NewPrice = newPrice,
                    OldPrice = oldPrice,
                    ChangePrice = changePrice,
                    Discount = discount,
                    ItemID = itemId,
                    TimeStamp = DateTime.Now.ToString()
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

                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Nager.AmazonProductAdvertising.Model.Item RequestAmazonAPI(Item itemAmazon)
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

                return item[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private static void AlterExistingItem()
        {
            List<Item> items = new Item().GetAllAsinId(connStr);

            foreach (var item in items)
            {
                var amazonItem = RequestAmazonAPI(item);

                item.Image = amazonItem.LargeImage != null ? amazonItem.LargeImage.URL : amazonItem.ImageSets[0].LargeImage.URL;

                Console.WriteLine(item.AsinId);

                Thread.Sleep(1000);
            }
        }
    }
}