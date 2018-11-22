using MySql.Data.MySqlClient;
using System;

namespace dsscountAPI.Model
{
    class Description
    {
        public int ID { get; set; }

        public string DescriptionDe1 { get; set; }
        public string DescriptionDe2 { get; set; }
        public string DescriptionDe3 { get; set; }
        public string DescriptionDe4 { get; set; }
        public string DescriptionDe5 { get; set; }

        public string DescriptionCn1 { get; set; }
        public string DescriptionCn2 { get; set; }
        public string DescriptionCn3 { get; set; }
        public string DescriptionCn4 { get; set; }
        public string DescriptionCn5 { get; set; }

        public string DescriptionEn1 { get; set; }
        public string DescriptionEn2 { get; set; }
        public string DescriptionEn3 { get; set; }
        public string DescriptionEn4 { get; set; }
        public string DescriptionEn5 { get; set; }

        public int Save(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO description (id, descriptionde1, descriptionde2, descriptionde3, descriptionde4, descriptionde5" +
                    ", descriptioncn1, descriptioncn2, descriptioncn3, descriptioncn4, descriptioncn5" +
                    ", descriptionen1, descriptionen2, descriptionen3, descriptionen4, descriptionen5) " +
                             "VALUES(NULL, @descriptionde1, @descriptionde2, @descriptionde3, @descriptionde4, @descriptionde5" +
                             ", null, null, null, null, null" +
                             ", null, null, null, null, null);", conn))
                    {
                        cmd.Parameters.AddWithValue("@descriptionde1", DescriptionDe1 ?? null);
                        cmd.Parameters.AddWithValue("@descriptionde2", DescriptionDe2 ?? null);
                        cmd.Parameters.AddWithValue("@descriptionde3", DescriptionDe3 ?? null);
                        cmd.Parameters.AddWithValue("@descriptionde4", DescriptionDe4 ?? null);
                        cmd.Parameters.AddWithValue("@descriptionde5", DescriptionDe5 ?? null);

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
