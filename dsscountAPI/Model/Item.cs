using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace dsscountAPI.Model
{
    class Item
    {
        public int ID { get; set; }

        public string AsinId { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string Review { get; set; }
        public string TimeStamp { get; set; }

        public string CccGuid { get; set; }

        public int CategoryID { get; set; }
        public int TitleID { get; set; }
        public int DescriptionID { get; set; }

        public Item(string cccGuid)
        {
            CccGuid = cccGuid;
        }

        public Item()
        {
        }

        public int Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO item_amazon_camel_de(id, asinId, url, image, review, timestamp, categoryid, istranslatedcn, istranslateden, cccguid, titleid, descriptionid) " +
                             "VALUES(NULL, @asinId, @url, @image, @review, @timestamp, @categoryid, 0, 0, @cccguid, @titleid, @descriptionid);", conn))
                    {
                        cmd.Parameters.AddWithValue("@asinId", AsinId);
                        cmd.Parameters.AddWithValue("@url", Url);
                        cmd.Parameters.AddWithValue("@image", Image);
                        cmd.Parameters.AddWithValue("@review", Review);
                        cmd.Parameters.AddWithValue("@categoryid", CategoryID);
                        cmd.Parameters.AddWithValue("@cccguid", CccGuid);
                        cmd.Parameters.AddWithValue("@titleid", TitleID);
                        cmd.Parameters.AddWithValue("@descriptionid", DescriptionID);

                        cmd.Parameters.AddWithValue("@timestamp", TimeStamp);

                        cmd.ExecuteNonQuery();
                        int id = (int)cmd.LastInsertedId;

                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return -1;
        }

        public Item FindByGuid(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select id, asinId from item_amazon_camel_de where cccguid = @cccguid;", conn))
                    {
                        cmd.Parameters.AddWithValue("@cccguid", CccGuid);

                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    return new Item
                                    {
                                        ID = (int)dataReader[0],
                                        AsinId = (string)dataReader[1],
                                        CccGuid = CccGuid
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public List<Item> GetAllAsinId(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select Id, asinId from item_amazon_camel_de;", conn))
                    {
                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {

                            List<Item> items = new List<Item>();
                            while (dataReader.Read())
                            {
                                Item item = new Item()
                                {
                                    ID = (int)dataReader[0],
                                    AsinId = (string)dataReader[1]
                                };
                                items.Add(item);
                            }
                            return items;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public void UpdateImage(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("Update item_amazon_camel_de set image=@image where id=@id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@image", Image);
                        cmd.Parameters.AddWithValue("@id", ID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
