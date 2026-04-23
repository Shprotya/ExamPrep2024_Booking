using ExamPrep2024_Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RestaurantData db = new RestaurantData();
            try
            {
                using (db)
                {
                    Customer c1 = new Customer { Name = "John Doe", ContactNumber = "1234567890" };
                    Customer c2 = new Customer { Name = "Jane Smith", ContactNumber = "0987654321" };
                    Customer c3 = new Customer { Name = "Alice Johnson", ContactNumber = "5555555555" };

                    Console.WriteLine("Created customers");

                    db.Customers.Add(c1);
                    db.Customers.Add(c2);
                    db.Customers.Add(c3);

                    db.SaveChanges();

                    Console.WriteLine("Added the Customers");

                    Booking b1 = new Booking { BookingsDate = DateTime.Now, NumberOfParticipants = 4, CustomerId = 1 };
                    Booking b2 = new Booking { BookingsDate= DateTime.Now, NumberOfParticipants = 2, CustomerId = 2 };

                    Console.WriteLine("Created Bookings");

                    db.Bookings.Add(b1);
                    db.Bookings.Add(b2);

                    db.SaveChanges();

                    Console.WriteLine("Added the Bookings");

                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
