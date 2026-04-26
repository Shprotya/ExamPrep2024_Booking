using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamPrep2024_Booking
{
    /// <summary>
    /// Second window: shows customers matching the entered name,
    /// allows selecting an existing one or creating a new customer,
    /// then saves the booking.
    /// </summary>
    public partial class CustomerSearchResults : Window
    {
        // Booking details passed in from the main window
        private readonly string _searchName;
        private readonly string _contactNumber;
        private readonly DateTime _bookingDate;
        private readonly int _numberOfCustomers;

        // The list of matched customers displayed in the listbox
        private List<Customer> _matchedCustomers;

        public CustomerSearchResults(string searchName, string contactNumber,
                                     DateTime bookingDate, int numberOfCustomers)
        {
            InitializeComponent();

            _searchName = searchName;
            _contactNumber = contactNumber;
            _bookingDate = bookingDate;
            _numberOfCustomers = numberOfCustomers;

            // Pre-fill the "new customer" fields with what was typed on the main screen
            tbx_Name.Text = _searchName;
            tbx_Contact.Text = _contactNumber;

            // Show the booking info as read-only labels
            tbl_BookingDate.Text = $"Date of booking - {_bookingDate:dd/MM/yyyy}";
            tbl_NumCustomers.Text = $"Number of customers - {_numberOfCustomers}";

            // Wire up selection change so clicking a match fills the fields
            lbx_Matches.SelectionChanged += Lbx_Matches_SelectionChanged;

            // Perform the customer search
            SearchCustomers();
        }

        /// <summary>
        /// Searches the Customers table for names containing the search term.
        /// </summary>
        private void SearchCustomers()
        {
            using (RestaurantData db = new RestaurantData())
            {
                _matchedCustomers = db.Customers
                    .Where(c => c.Name.Contains(_searchName))
                    .ToList();

                lbx_Matches.Items.Clear();
                foreach (var customer in _matchedCustomers)
                    lbx_Matches.Items.Add($"{customer.Name} {customer.ContactNumber}");
            }
        }

        /// <summary>
        /// When the user selects a match, populate the fields with that customer's details.
        /// </summary>
        private void Lbx_Matches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbx_Matches.SelectedIndex >= 0)
            {
                Customer selected = _matchedCustomers[lbx_Matches.SelectedIndex];
                tbx_Name.Text = selected.Name;
                tbx_Contact.Text = selected.ContactNumber;
            }
        }

        /// <summary>
        /// Creates the booking. Uses an existing customer if one was selected,
        /// otherwise creates a new customer with the details in the text boxes.
        /// </summary>
        private void Btn_CreateBooking_Click(object sender, RoutedEventArgs e)
        {
            string name = tbx_Name.Text.Trim();
            string contact = tbx_Contact.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show("Please enter both a name and contact number.", "Validation");
                return;
            }

            using (RestaurantData db = new RestaurantData())
            {
                Customer bookingCustomer;

                // If the user selected a match AND hasn't edited the fields, reuse that customer
                if (lbx_Matches.SelectedIndex >= 0)
                {
                    Customer selectedMatch = _matchedCustomers[lbx_Matches.SelectedIndex];
                    // Re-attach to this context
                    bookingCustomer = db.Customers.Find(selectedMatch.CustomerId);
                }
                else
                {
                    // Create a brand-new customer
                    bookingCustomer = new Customer
                    {
                        Name = name,
                        ContactNumber = contact
                    };
                    db.Customers.Add(bookingCustomer);
                    db.SaveChanges(); // get the new CustomerId
                }

                // Create the booking linked to this customer
                Booking newBooking = new Booking
                {
                    BookingsDate = _bookingDate,
                    NumberOfParticipants = _numberOfCustomers,
                    CustomerId = bookingCustomer.CustomerId
                };

                db.Bookings.Add(newBooking);
                db.SaveChanges();
            }

            MessageBox.Show("Booking created successfully!", "Booking Confirmed");
            this.Close(); // Return to main window
        }
    }
}