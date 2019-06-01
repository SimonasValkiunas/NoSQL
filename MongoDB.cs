using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {

            AsyncContext.Run(() => MainAsync());
            Console.ReadLine();
        }

        static async Task MainAsync()
        {
            List<User> luser = new List<User>();

            var client = new MongoClient();

            IMongoDatabase db = client.GetDatabase("db");

            var collection = db.GetCollection<BsonDocument>("Users");

            using (IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    
                   
                    foreach (var document in batch)
                    {
                        var user = new User
                        {
                            id = (ObjectId)document[0],
                            name = (string)document[1],
                            surname = (string)document[2],
                            address = (string)document[3],
                            phone = (string)document[4],
                            CC_Info =document[5],
                            purchases = document[6],
   
                        };
  
                        luser.Add(user);
                    }
                }
            }

            //EMBBEDED

            Console.WriteLine("VISU KLIENTU KREDITINIU INFORMACIJA ");
            
            foreach (var user in luser) {
                BsonValue dimVal = user.CC_Info;
                List<CC> d = BsonSerializer.Deserialize<List<CC>>(dimVal.ToJson());
                Console.WriteLine("Klientas: " + user.name);
                foreach (var b in d)
                {
                    Console.WriteLine("Card Number: " +b.Card_Number);
                    Console.WriteLine("CVV: " + b.CVV);
                    Console.WriteLine("Expiration Date: " + b.Expires + '\n');
                }
            }

            //AGGREGATE

            Console.WriteLine("BENDRA VISU KLIENTU PIRKINIU SUMA ");
            double sum = 0;
            foreach (var user in luser)
            {
                BsonValue dimVal = user.purchases;
                List<purchases> d = BsonSerializer.Deserialize<List<purchases>>(dimVal.ToJson());
                foreach (var b in d)
                {
                    sum += b.total_price;
                }


            }

            Console.WriteLine("SUMA: " + sum);


            var coll = db.GetCollection<User>("Users");
            string map = @"function(){
                for (var i = 0; i< this.purchases.length; i++){
                        var value = this.purchases[i].total_price;
                        emit(this.purchases[i].product, value);
                }
            }";
            string reduce = @"function(product, total_price){
                   return Array.sum(total_price);
            }       
            ";
            var options = new MapReduceOptions<User, BsonDocument>
            {
                OutputOptions = MapReduceOutputOptions.Inline
            };

            var results = coll.MapReduce(map, reduce, options).ToList();
            var rr = results.ToJson();

            foreach(var r in results)
            {
                Console.WriteLine(r);
            }
            
            

        }

        public class User
        {
            public ObjectId id { get; set; }
            public String name { get; set; }
            public String surname { get; set; }
            public String address { get; set; }
            public String phone { get; set; }
            public BsonValue CC_Info { get; set; }
            public BsonValue purchases { get; set; }
        }

        class CC
        {

            public String Card_Number { get; set; }
            public String CVV { get; set; }
            public String Expires { get; set; }
        }
        class purchases
        {
            public String product { get; set; }
            public int quantity { get; set; }
            public double total_price { get; set; }
            public string data_of_purchase { get; set; }
        }


    }
}
