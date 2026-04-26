using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExamPrep2024_Booking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Restaurant capacity constant
        private const int RestaurantCapacity = 40;

        public MainWindow()
        {
            InitializeComponent();

            // Set date pickers to today by default
            date_BookDetails.SelectedDate = DateTime.Today;
            date_NewBook.SelectedDate = DateTime.Today;

            // Wire up event handlers
            date_BookDetails.SelectedDateChanged += Date_BookDetails_SelectedDateChanged;
            btn_Delete.Click += Btn_Delete_Click;
            btn_Search.Click += Btn_Search_Click;

            // Load bookings for today on startup
            LoadBookingsForDate(DateTime.Today);
        }

        /// <summary>
        /// Fires when the user picks a date on the left-hand DatePicker.
        /// Queries the DB and refreshes the listbox and stats labels.
        /// </summary>
        private void Date_BookDetails_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (date_BookDetails.SelectedDate.HasValue)
                LoadBookingsForDate(date_BookDetails.SelectedDate.Value);
        }

        /// <summary>
        /// Queries the database for all bookings on the given date,
        /// populates the listbox, and updates the capacity/bookings/available labels.
        /// </summary>
        private void LoadBookingsForDate(DateTime selectedDate)
        {
            using (RestaurantData db = new RestaurantData())
            {
                // LINQ query: match on date only (ignore time component)
                var bookings = db.Bookings
                    .Include("Customer")
                    .Where(b => b.BookingsDate.Year == selectedDate.Year
                             && b.BookingsDate.Month == selectedDate.Month
                             && b.BookingsDate.Day == selectedDate.Day)
                    .ToList();

                // Format each entry as: Customer Name (Phone Number) – Party of X
                lbx_Bookings.Items.Clear();
                foreach (var booking in bookings)
                {
                    string display = $"{booking.Customer.Name} ({booking.Customer.ContactNumber}) – Party of {booking.NumberOfParticipants}";
                    lbx_Bookings.Items.Add(display);
                }

                // Store bookings as Tag so we can identify selected index for delete
                lbx_Bookings.Tag = bookings;

                // Calculate stats
                int totalBooked = bookings.Sum(b => b.NumberOfParticipants);
                int available = RestaurantCapacity - totalBooked;

                tbl_Capacity.Text = RestaurantCapacity.ToString();
                tbl_Booking.Text = totalBooked.ToString();
                tbl_Available.Text = available.ToString();
            }
        }

        /// <summary>
        /// Opens the CustomerSearchResults window, passing across the
        /// new booking details entered on the right-hand panel.
        /// </summary>
        private void Btn_Search_Click(object sender, RoutedEventArgs e)
        {
            // Basic validation
            if (!date_NewBook.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a booking date.", "Validation");
                return;
            }

            if (!int.TryParse(tbx_CustomerNo.Text, out int numCustomers) || numCustomers <= 0)
            {
                MessageBox.Show("Please enter a valid number of customers.", "Validation");
                return;
            }

            string customerName = tbx_CustomerName.Text.Trim();
            string contactNumber = tbx_ContactNo.Text.Trim();

            if (string.IsNullOrWhiteSpace(customerName))
            {
                MessageBox.Show("Please enter a customer name.", "Validation");
                return;
            }

            // Open the search/create window
            CustomerSearchResults searchWindow = new CustomerSearchResults(
                customerName,
                contactNumber,
                date_NewBook.SelectedDate.Value,
                numCustomers);

            // Refresh main screen after returning
            searchWindow.ShowDialog();
            LoadBookingsForDate(date_BookDetails.SelectedDate ?? DateTime.Today);

            // Clear the input fields
            tbx_CustomerName.Text = "Customer Name";
            tbx_ContactNo.Text = "Contact Number";
            tbx_CustomerNo.Text = "Number of Customers";
        }

        /// <summary>
        /// Deletes the booking that is currently selected in the listbox.
        /// </summary>
        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (lbx_Bookings.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a booking to delete.", "Delete Booking");
                return;
            }

            // Retrieve the matching Booking object from the Tag we stored
            var bookings = lbx_Bookings.Tag as System.Collections.Generic.List<Booking>;
            if (bookings == null) return;

            Booking selected = bookings[lbx_Bookings.SelectedIndex];

            MessageBoxResult confirm = MessageBox.Show(
                $"Are you sure you want to delete the booking for {selected.Customer?.Name}?",
                "Confirm Delete",
                MessageBoxButton.YesNo);

            if (confirm != MessageBoxResult.Yes) return;

            using (RestaurantData db = new RestaurantData())
            {
                // Attach and remove
                Booking toDelete = db.Bookings.Find(selected.BookingId);
                if (toDelete != null)
                {
                    db.Bookings.Remove(toDelete);
                    db.SaveChanges();
                }
            }

            // Refresh the UI
            LoadBookingsForDate(date_BookDetails.SelectedDate ?? DateTime.Today);
        }
    }
}