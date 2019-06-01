using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisUžduotis
{
    class Program
    {
        static void Main(string[] args)


        {

            // Creating connection with Redis server
                
            var conn = ConnectionMultiplexer.Connect("localhost");

            //Connects to db
            IDatabase redis = conn.GetDatabase();

            //Clearing old data


            //Creates an array of (new) users

            string[] userArray = { "Jonas" , "Petras", "Antanas", "Lukas"};

            //Adds user to db

            foreach (var user in userArray)
            {
                redis.ListRemove("user_name", user);
                redis.ListRightPush("users_name", user);
            }
            
            //Gets lenght of the new list

            var listLength = redis.ListLength("users_name");

            //Adds accounts for users

            Random rnd = new Random();

            Console.WriteLine("Before Transactions");

            for (int i = 0; i<listLength; i++)
            {
                var hashKey = userArray[i];

                //Generating credit card numbers and balances

                
                long cc_numb = rnd.Next(100000000, 1000000000);
                int balance = rnd.Next(10, 1000);

                //Adding credit card numbers and balances to db

                HashEntry[] userInfo = {
                new HashEntry("CC_number", "LT" + cc_numb.ToString()),
                new HashEntry("CC_Balance", balance.ToString()),
              };

                redis.HashSet(hashKey, userInfo);
                Console.WriteLine(hashKey);
                foreach (var item in userInfo)
                {
                    Console.WriteLine(String.Format("{0}: {1}", item.Name, item.Value));
                }

            }


            //Transakcijos
            //1) Jonas Perveda 20Euru Petrui

            redis.HashIncrement("Petras", "CC_Balance", 20);
            redis.HashDecrement("Jonas", "CC_Balance", 20);

            //2)Antanas gauna 500 Euru alga

            redis.HashIncrement("Antanas", "CC_Balance", 500);

            //3)Lukas sumokėjo 15 Euru parduotuvėje
            redis.HashDecrement("Jonas", "CC_Balance", 15);

            HashEntry[] result;

            Console.WriteLine("After Transactions");

            foreach (var user in userArray)
            {
                Console.WriteLine(user);
                result = redis.HashGetAll(user);
                foreach(var item in result)
                {
                    Console.WriteLine(String.Format("{0}: {1}", item.Name, item.Value));
                }
                

            }
            Console.ReadLine();











        }
    }
}
