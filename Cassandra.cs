using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cassandraApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Cluster cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();
            ISession session = cluster.Connect("eshop");

            //UI menu
            string selection = selectionMenu();

            if (selection == "1")
            {

                //Register user

                string email;
                string name;
                string surname;
                string address;
                string phone_number;

                Console.WriteLine("Enter email");
                email = Console.ReadLine();
                Console.WriteLine("Enter name");
                name = Console.ReadLine();
                Console.WriteLine("Enter surname");
                surname = Console.ReadLine();
                Console.WriteLine("Enter address");
                address = Console.ReadLine();
                Console.WriteLine("Enter phone number");
                phone_number = Console.ReadLine();

                //Prepare users query
                var ps = session.Prepare("INSERT INTO users (email, creation_date , name, surname, address, phone_number) " +
                    " VALUES (?,?,?,?,?,?) IF NOT EXISTS");
           
                    var send = ps.Bind(email, DateTime.Now, name, surname, address, phone_number);
                //Execute query
                session.Execute(send);

                //Check new user list
                var result = session.Execute("SELECT * FROM users");
                foreach (var row in result)
                {
                    email = row.GetValue<string>("email");
                    name = row.GetValue<string>("name");
                    surname = row.GetValue<string>("surname");
                    Console.WriteLine(email + " " + name + " " + surname);
                }

            }
            else if (selection == "2")
            {

                Console.WriteLine("Enter user email" + '\n');
                string email = Console.ReadLine();
                //Prepare query
                var prep = session.Prepare("INSERT INTO purchases(email, date, products, total_price) VALUES(?,?,?,?)");


                string title = "";
                double price = 0;
                List<string> products = new List<string>();

                //Calculate price + make list of products
                while (title != "stop")
                {

                    Console.WriteLine("Enter product title (enter stop when finished)");
                    title = Console.ReadLine();
                    var ps_product = session.Prepare("SELECT title, price,quantity FROM  products_title WHERE title = ? AND quantity != 0 ");
                    var send = ps_product.Bind(title);
                    var result = session.Execute(send);
                    
                    foreach (var row in result)
                    {
                        //Add to product list
                        title = row.GetValue<string>("title");
                        products.Add(title);
                        //Calculate total price
                        price += row.GetValue<Double>("price");

                        //Decrement quantity
                        int quantity = row.GetValue<int>("quantity");
                        quantity--;

                        //Prep the magic
                        var prep_quant_title = session.Prepare("UPDATE purchase_title SET quantity = ? WHERE title = ?");
                        var prep_quant = session.Prepare("UPDATE purchase SET quantity = ? WHERE title = ?");
                        //Make sure magic get interupted
                        var batch = new BatchStatement()
                            .Add(prep_quant_title.Bind(quantity, title))
                            .Add(prep_quant.Bind(quantity, title));
                        //Execute the magic
                        session.Execute(batch);
                    }
                }

                //Execute purchase query
                var send_purch = prep.Bind(email, DateTime.Now, products, price);
                session.Execute(send_purch);

            }else if (selection == "3")
            {
                //Get email
                Console.Write("Enter user email" + '\n');
                string email = Console.ReadLine();

                var prep = session.Prepare("SELECT * FROM users WHERE email =?");
                var send = prep.Bind(email);

                var result = session.Execute(send);

                foreach(var row in result)
                {
                    email = row.GetValue<string>("email");
                    DateTime date = row.GetValue<DateTime>("creation_date");
                    string name = row.GetValue<string>("name");
                    string surname = row.GetValue<string>("surname");
                    string address = row.GetValue<string>("address");
                    string phone = row.GetValue<string>("phone_number");

                    Console.WriteLine("Email:" + email + '\n' + "Name:" + name + '\n'+"Surname:" + surname + '\n'+
                        "Registration_Date:" + date + '\n' + "Phone:" + phone + '\n' + "Address:" + address + '\n');

                }

            }else if (selection == "4")
            {
                //Get title
                Console.Write("Enter product title" + '\n');
                string title = Console.ReadLine();

                var prep = session.Prepare("SELECT * FROM products_title WHERE title =?");
                var send = prep.Bind(title);

                var result = session.Execute(send);

                foreach (var row in result)
                {
                    title = row.GetValue<string>("title");
                    DateTime date = row.GetValue<DateTime>("creation_date");
                    string desc = row.GetValue<string>("description");
                    string url = row.GetValue<string>("slug_url");
                    Double price = row.GetValue<Double>("price");
                    int quantity = row.GetValue<int>("quantity");

                    Console.WriteLine("Title:" + title + '\n' + "Description:" + desc + '\n' + "Url:" + url + '\n' +
                        "Upload_Date:" + date + '\n' + "Price:" + price + '\n' + "quantity:" + quantity + '\n');

                }
            }else if (selection == "5")
            {
                //Get url
                Console.Write("Enter product url" + '\n');
                string url = Console.ReadLine();

                var prep = session.Prepare("SELECT * FROM products WHERE slug_url =?");
                var send = prep.Bind(url);

                var result = session.Execute(send);

                foreach (var row in result)
                {
                    string title = row.GetValue<string>("title");
                    DateTime date = row.GetValue<DateTime>("creation_date");
                    string desc = row.GetValue<string>("description");
                    url = row.GetValue<string>("slug_url");
                    Double price = row.GetValue<Double>("price");
                    int quantity = row.GetValue<int>("quantity");

                    Console.WriteLine("Title:" + title + '\n' + "Description:" + desc + '\n' + "Url:" + url + '\n' +
                        "Upload_Date:" + date + '\n' + "Price:" + price + '\n' + "quantity:" + quantity + '\n');

                }
            }

                Console.ReadLine();

        }

        static string selectionMenu()
        {
            Console.WriteLine("Select operation");
            Console.WriteLine("1.Register new user" + '\n' + "2.Purchase" + '\n' + "3.Select user by email" + '\n' + "4.Select product by title" + '\n' + "5.Select product by url");
            string selection = Console.ReadLine();
            return selection;
        }

    }
}
