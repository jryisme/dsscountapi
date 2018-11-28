using DssCount;
using DssCount.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace LanguageAPI
{
    class LanguageAPIProgram
    {
        //const string connStr = "server=localhost;user=root;database=dsscount;port=3306;password=";
        const string connStr = "server=dsscount-paris.crmnj9nnruyg.eu-west-3.rds.amazonaws.com;user=dsscount;database=dsscount;port=3306;password=XWWZw3fRGo36QyoQ";

        static void Main(string[] args)
        {
            string path = @"C:\xampp\htdocs\dsscountAPI\Logs\LanguageAPI\log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            using (StreamWriter writer = new StreamWriter(path))
            {
                Console.SetOut(writer);

                Console.WriteLine("Connstr: " + connStr);
                Console.WriteLine();
                List<Item> items = new Item().ListItemsNoCN(connStr);

                foreach (var item in items)
                {
                    InitTranslate(item);
                }
            }
        }

        private static void InitTranslate(Item item)
        {
            try
            {
                item.Title.TitleCn = TranslationHelper.HandleRequest(item.Title.TitleDe);

                item.Description.DescriptionCn1 = TranslationHelper.HandleRequest(item.Description.DescriptionDe1);
                item.Description.DescriptionCn2 = TranslationHelper.HandleRequest(item.Description.DescriptionDe2);
                item.Description.DescriptionCn3 = TranslationHelper.HandleRequest(item.Description.DescriptionDe3);
                item.Description.DescriptionCn4 = TranslationHelper.HandleRequest(item.Description.DescriptionDe4);
                item.Description.DescriptionCn5 = TranslationHelper.HandleRequest(item.Description.DescriptionDe5);

                item.Title.UpdateTitleCn(connStr);
                item.Description.UpdateDescriptionCn(connStr);

                item.UpdateTranslationCnStatus(connStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        }
    }
}
