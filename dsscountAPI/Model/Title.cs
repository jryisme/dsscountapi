using MySql.Data.MySqlClient;
using System;

namespace dsscountAPI.Model
{
    class Title
    {
        public int ID { get; set; }

        public string TitleDe { get; set; }
        public string TitleCn { get; set; }
        public string TitleEn { get; set; }

        public int ItemID { get; set; }

        public void Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO title(id, titlede, titlecn, titleen, itemid) " +
                             "VALUES(NULL, @titlede, null, null, @itemid);", conn))
                    {

                        cmd.Parameters.AddWithValue("@titlede", TitleDe);
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
    }
}
