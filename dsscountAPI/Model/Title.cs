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

        public int Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO title(id, titlede, titlecn, titleen) " +
                             "VALUES(NULL, @titlede, null, null);", conn))
                    {
                        cmd.Parameters.AddWithValue("@titlede", TitleDe);

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
