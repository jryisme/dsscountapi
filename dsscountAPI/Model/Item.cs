using MySql.Data.MySqlClient;
using System;

namespace dsscountAPI.Model
{
    class Item
    {
        public int ID { get; set; }

        public string AsinId { get; set; }
        public decimal Discount { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public decimal ChangePrice { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string Review { get; set; }
        public string TimeStamp { get; set; }

        public string Guid { get; set; }

        public int CategoryID { get; set; }

        public int Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO items_amazon_camel_de_de(id, asinId, discount, newprice, oldprice, changeprice, url, image, review, timestamp, categoryid, istranslatedcn, istranslateden, guid) " +
                             "VALUES(NULL, @asinId, @discount, @newprice, @oldprice, @changeprice, @url, @image, @review, @timestamp, @categoryid, 0, 0, @guid);", conn))
                    {
                        cmd.Parameters.AddWithValue("@asinId", AsinId);
                        cmd.Parameters.AddWithValue("@discount", Discount);
                        cmd.Parameters.AddWithValue("@newprice", NewPrice);
                        cmd.Parameters.AddWithValue("@oldprice", OldPrice);
                        cmd.Parameters.AddWithValue("@changeprice", ChangePrice);
                        cmd.Parameters.AddWithValue("@url", Url);
                        cmd.Parameters.AddWithValue("@image", Image);
                        cmd.Parameters.AddWithValue("@review", Review);
                        cmd.Parameters.AddWithValue("@categoryid", CategoryID);
                        cmd.Parameters.AddWithValue("@guid", Guid);

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
    }
}
