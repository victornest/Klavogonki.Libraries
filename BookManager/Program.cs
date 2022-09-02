using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookManager.Configuration;
using Klavogonki;
using Ninject;

namespace BookManager
{
    internal class Program
    {
        private static StandardKernel kernel;
        public static void Main(string[] args)
        {
            kernel = new StandardKernel(new NinjectRegistrations());
            
            Console.WriteLine("Press 1 to retrieve book parts");

            // var key = Console.ReadLine();
            // if (key == "1")
            {
                _= RetrieveBookParts();

            }
            Console.ReadLine();
        }
        
        private static async Task RetrieveBookParts()
        {
            var bookPartsToSave = new StringBuilder();
            
            for (var i = 1; i <= 1119; i++)
            {
                bookPartsToSave.AppendLine("============");
                bookPartsToSave.AppendLine($"отрывок {i}");
                bookPartsToSave.AppendLine("============");
                // bookPartsToSave.AppendLine();
                string part = await NetworkClient.PostFormAsync("http://klavogonki.ru/ajax/edit-vocbook-part", 
                    new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("voc", "217766"),
                        new KeyValuePair<string, string>("part", i.ToString()),
                    }));
                bookPartsToSave.AppendLine(part);
                Thread.Sleep(2000);
            }
            
            File.WriteAllText("outputBookParts.txt", bookPartsToSave.ToString());
        }
    }
}