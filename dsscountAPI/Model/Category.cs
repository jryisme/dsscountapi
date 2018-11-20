﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace dsscountAPI.Model
{
    class Category
    {
        public Int32 ID { get; set; }
        public string KeyDe { get; set; }
        public string NameDe { get; set; }
        public string NameCn { get; set; }

        public List<Category> GetCategories(string connStr)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select id, keyDe from category;", conn))
                    {
                        conn.Open();
                        using (MySqlDataReader dataReader = cmd.ExecuteReader())
                        {
                            List<Category> categories = new List<Category>();
                            while (dataReader.Read())
                            {
                                Category category = new Category()
                                {
                                    ID = (int)dataReader[0],
                                    KeyDe = (string)dataReader[1]
                                };
                                categories.Add(category);
                            }
                            return categories;
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
    }
}
