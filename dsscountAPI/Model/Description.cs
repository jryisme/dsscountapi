using MySql.Data.MySqlClient;
using System;

namespace dsscountAPI.Model
{
    public class Description
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
                Console.WriteLine();
            }
            return -1;
        }

        public Description FindByID(string connStr, int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select descriptionde1, descriptionde2, descriptionde3, descriptionde4, descriptionde5 from description where id = @id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    return new Description
                                    {
                                        DescriptionDe1 = dataReader.IsDBNull(0) ? null : (string)dataReader[0],
                                        DescriptionDe2 = dataReader.IsDBNull(1) ? null : (string)dataReader[1],
                                        DescriptionDe3 = dataReader.IsDBNull(2) ? null : (string)dataReader[2],
                                        DescriptionDe4 = dataReader.IsDBNull(3) ? null : (string)dataReader[3],
                                        DescriptionDe5 = dataReader.IsDBNull(4) ? null : (string)dataReader[4],
                                        ID = id
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
                Console.WriteLine();
            }
            return null;
        }

        public void UpdateDescriptionCn(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("Update description set descriptioncn1=@descriptioncn1, descriptioncn2=@descriptioncn2, descriptioncn3=@descriptioncn3, descriptioncn4=@descriptioncn4, descriptioncn5=@descriptioncn5 where id=@id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@descriptioncn1", DescriptionCn1 ?? null);
                        cmd.Parameters.AddWithValue("@descriptioncn2", DescriptionCn2 ?? null);
                        cmd.Parameters.AddWithValue("@descriptioncn3", DescriptionCn3 ?? null);
                        cmd.Parameters.AddWithValue("@descriptioncn4", DescriptionCn4 ?? null);
                        cmd.Parameters.AddWithValue("@descriptioncn5", DescriptionCn5 ?? null);
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
    }
}
