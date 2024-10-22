﻿using MySql.Data.MySqlClient;
using System;

namespace DssCount
{
    public class DiscountItem
    {
        public int ID { get; set; }

        public decimal Discount { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public decimal ChangePrice { get; set; }
        public string TimeStamp { get; set; }
        public string EndTime { get; set; }
        public int Dealer { get; set; }

        public int ItemID { get; set; }
        public int CategoryID { get; set; }

        public void Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO discount_item_amazon_camel_de(id, discount, newprice, oldprice, changeprice, timestamp, itemid, categoryid, endTime, dealer) " +
                             "VALUES(NULL, @discount, @newprice, @oldprice, @changeprice, @timestamp, @itemid, @categoryid, @endTime, @dealer);", conn))
                    {
                        cmd.Parameters.AddWithValue("@discount", Discount);
                        cmd.Parameters.AddWithValue("@newprice", NewPrice);
                        cmd.Parameters.AddWithValue("@oldprice", OldPrice);
                        cmd.Parameters.AddWithValue("@changeprice", ChangePrice);
                        cmd.Parameters.AddWithValue("@timestamp", TimeStamp);
                        cmd.Parameters.AddWithValue("@itemid", ItemID);
                        cmd.Parameters.AddWithValue("@categoryid", CategoryID);
                        cmd.Parameters.AddWithValue("@endTime", string.IsNullOrEmpty(EndTime) ? null : EndTime);
                        cmd.Parameters.AddWithValue("@dealer", Dealer);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public DiscountItem FindByItemID(string connStr, int itemId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select id, newprice from discount_item_amazon_camel_de where itemid = @itemid;", conn))
                    {
                        cmd.Parameters.AddWithValue("@itemid", itemId);

                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    return new DiscountItem
                                    {
                                        ID = (int)dataReader[0],
                                        NewPrice = (decimal)dataReader[1]
                                    };
                                }
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
            return null;
        }

        public void Update(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("Update discount_item_amazon_camel_de set discount=@discount, newprice=@newprice, oldprice=@oldprice, changeprice=@changeprice, timestamp=@timestamp, endTime=@endTime where id=@id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@discount", Discount);
                        cmd.Parameters.AddWithValue("@newprice", NewPrice);
                        cmd.Parameters.AddWithValue("@oldprice", OldPrice);
                        cmd.Parameters.AddWithValue("@changeprice", ChangePrice);
                        cmd.Parameters.AddWithValue("@timestamp", TimeStamp);
                        cmd.Parameters.AddWithValue("@endTime", string.IsNullOrEmpty(EndTime) ? null : EndTime);
                        cmd.Parameters.AddWithValue("@id", ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }

        public void Delete(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("delete from discount_item_amazon_camel_de where id=@id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }

        public void RemoveOldAndBadItems(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("delete from discount_item_amazon_camel_de where (dealer=0 and (oldprice-newprice!=changeprice or changeprice/oldprice*100>discount+1 or changeprice/oldprice*100<discount-1)) or (dealer=1 and endTime<@time);", conn))//Unnecessary code:timestamp<@time or 
                    {
                        cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString());

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }

        //public List<Item> GetAllAsinId(string connStr)
        //{
        //    try
        //    {
        //        using (MySqlConnection conn = new MySqlConnection(connStr))
        //        {
        //            conn.Open();
        //            using (MySqlCommand cmd = new MySqlCommand("select Id, asinId from item_amazon_camel_de;", conn))
        //            {
        //                using (MySqlDataReader dataReader = cmd.ExecuteReader())
        //                {

        //                    List<Item> items = new List<Item>();
        //                    while (dataReader.Read())
        //                    {
        //                        Item item = new Item()
        //                        {
        //                            ID = (int)dataReader[0],
        //                            AsinId = (string)dataReader[1]
        //                        };
        //                        items.Add(item);
        //                    }
        //                    return items;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //    return null;
        //}
    }
}
