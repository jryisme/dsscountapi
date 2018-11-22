﻿using MySql.Data.MySqlClient;
using System;

namespace dsscountAPI.Model
{
    class Price
    {
        public int ID { get; set; }

        public decimal Value { get; set; }
        public string Timestamp { get; set; }
        public int ItemID { get; set; }

        public void Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO price_item_amazon_camel_de(id, price, timestamp, itemid) " +
                             "VALUES(NULL, @price, @timestamp, @itemid);", conn))
                    {
                        cmd.Parameters.AddWithValue("@price", Value);
                        cmd.Parameters.AddWithValue("@timestamp", Timestamp);
                        cmd.Parameters.AddWithValue("@itemid", ItemID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public decimal GetLatestPrice(string connStr, int itemId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select price from price_item_amazon_camel_de where itemid=@itemid order by timestamp DESC;", conn))
                    {
                        cmd.Parameters.AddWithValue("@itemid", itemId);
                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                return (decimal)dataReader[0];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return 0;
        }
    }
}