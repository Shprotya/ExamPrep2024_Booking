using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamPrep2024_Booking
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }

        //Customer can have multiple bookings
        //virtual keyword is used to enable lazy loading
        public virtual List<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public DateTime BookingsDate { get; set; }
        public int NumberOfParticipants { get; set; }

        //Foreign key to Customer
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
    }

    public class RestaurantData : DbContext
    {
        //the name of the database
        public RestaurantData() : base("OODExam_YelyzavetaKareieva") { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
    }
}
