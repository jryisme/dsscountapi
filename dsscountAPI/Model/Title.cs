using MySql.Data.MySqlClient;
using System;

namespace dsscountAPI.Model
{
    public class Title
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
                Console.WriteLine();
            }
            return -1;
        }
        
        public Title FindByID(string connStr, int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select titlede from title where id = @id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    return new Title
                                    {
                                        TitleDe = (string)dataReader[0],
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

        public void UpdateTitleCn(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("Update title set titlecn=@titlecn where id=@id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@titlecn", TitleCn);
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
