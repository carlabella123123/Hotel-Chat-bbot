using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;

namespace HotelChatBot
{
    class Program
    {
        static void Main(string[] args)
        {
            HotelChatBot bot = new HotelChatBot();
            bot.MainMenu();
        }
    }

    // Base class for User with common properties
    public abstract class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // Admin class inherits from User
    public class Admin : User
    {
        public Admin()
        {
            Username = "admin";
            Password = "admin123";
        }
    }

    // Customer class inherits from User
    public class Customer : User
    {
        public List<Booking> Bookings { get; set; } = new();
    }

    // Room class holds details about each room type
    public class Room
    {
        public int Price { get; set; }
        public int Count { get; set; }
        public string[] Amenities { get; set; }
    }

    // Booking class stores reservation details including timestamps
    public class Booking
    {
        public string RoomType { get; set; }
        public int Nights { get; set; }
        public int TotalCost { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime CheckoutTime { get; set; }
        public string RoomNumber { get; set; }  // Add RoomNumber property
    }


    // Main HotelChatBot class that manages the chatbot flow
    public class HotelChatBot
    {
        private const string CustomersFile = "customers.txt";
        private const string RoomsFile = "rooms.txt";
        private const string ReservationsFile = "reservations.txt";

        private readonly Admin admin = new Admin();
        private Dictionary<string, Customer> customers = new();
        private Dictionary<string, Room> rooms = new();

        public HotelChatBot()
        {
            LoadCustomers();
            LoadRooms();
            LoadReservations();
        }

        public void MainMenu()
        {
            Console.WriteLine("\nHello! I'm your friendly Hotel Chat Bot 🤖. How can I assist you today?");
            while (true)
            {
                Console.WriteLine("\n[Main Menu]");
                Console.WriteLine("1. Admin Login");
                Console.WriteLine("2. Customer Login");
                Console.WriteLine("3. Exit");
                Console.Write("\nPlease type your choice (e.g., 1, 2, or 3): ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Taking you to the admin login...");
                        AdminLogin();
                        break;
                    case "2":
                        Console.WriteLine("Redirecting you to the customer menu...");
                        CustomerMenu();
                        break;
                    case "3":
                        Console.WriteLine("Thank you for visiting. Have a great day! 👋");
                        SaveAll(); // Save data before quitting
                        return;
                    default:
                        Console.WriteLine("Oops! I didn't catch that. Please enter a valid choice.");
                        break;
                }
            }
        }



        private void AdminLogin()
        {
            Console.Write("Enter admin username: ");
            string username = Console.ReadLine();

            Console.Write("Enter password: ");
            string password = AnsiConsole.Prompt(
            new TextPrompt<string>("")
                .Secret()); // Hides the password input

            if (username == admin.Username && password == admin.Password)
            {
                Console.WriteLine("Admin login successful!");
                AdminMenu();
            }
            else
            {
                Console.WriteLine("Invalid admin credentials.");
            }

        }

        private void AdminMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\nAdmin Menu:");
                Console.WriteLine("1. View All Customers");
                Console.WriteLine("2. Search Customer");
                Console.WriteLine("3. Update Room Details");
                Console.WriteLine("4. View Room Availability");
                Console.WriteLine("5. Exit");
                Console.Write("Enter choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewAllCustomers();  // Show all customers
                        break;
                    case "2":
                        SearchCustomerForAdmin();  // Search a specific customer
                        break;
                    case "3":
                        ChangeRoomDetails();  // Update room details
                        break;
                    case "4":
                        ViewRoomAvailability();  // View available rooms
                        break;
                    case "5":
                        return;  // Exit the admin menu
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void ViewAllCustomers()
        {
            Console.Clear();
            if (customers.Count == 0)
            {
                Console.WriteLine("No customers found.");
            }
            else
            {
                Console.WriteLine("\nAll Registered Customers:");
                foreach (var customer in customers.Values)
                {
                    Console.WriteLine($"Username: {customer.Username}");
                    Console.WriteLine($"Bookings: {customer.Bookings.Count}");
                    Console.WriteLine("----------------------------------");
                }
            }

            Console.WriteLine("\nPress Enter to return to the Admin Menu...");
            Console.ReadLine();  // Wait for user input to return to the admin menu
        }




        private void ChangeCustomerPasswordForAdmin(string username)
        {
            Console.Write("Enter new password for customer: ");
            string newPassword = Console.ReadLine();

            if (customers.ContainsKey(username))
            {
                customers[username].Password = newPassword;
                SaveCustomers();  // Save changes to file
                Console.WriteLine("Password changed successfully.");
            }
            else
            {
                Console.WriteLine("Customer not found.");
            }
        }

        private void DeleteCustomerAccount(string username)
        {
            if (customers.ContainsKey(username))
            {
                customers.Remove(username);
                SaveCustomers();  // Save changes to file
                Console.WriteLine("Customer account deleted successfully.");
            }
            else
            {
                Console.WriteLine("Customer not found.");
            }
        }

        private void SearchCustomerForAdmin()
        {
            Console.Clear();
            Console.Write("Enter username to search: ");
            string username = Console.ReadLine();

            if (customers.ContainsKey(username))
            {
                var customer = customers[username];
                Console.WriteLine($"\nCustomer found: {customer.Username}");
                Console.WriteLine("1. View Booking History");
                Console.WriteLine("2. Change Customer Password");
                Console.WriteLine("3. Delete Customer Account");
                Console.WriteLine("4. Exit");
                Console.Write("Select an action: ");
                string action = Console.ReadLine();

                switch (action)
                {
                    case "1":
                        ViewBookingHistory(customer.Username);  // Pass only the username (string)
                        break;
                    case "2":
                        ChangeCustomerPassword(customer);  // Change password of the found customer
                        break;
                    case "3":
                        DeleteCustomerAccount(customer);  // Delete the found customer account
                        break;
                    case "4":
                        return;  // Exit the search operation
                    default:
                        Console.WriteLine("Invalid choice. Returning to admin menu...");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Customer not found.");
            }

            Console.WriteLine("\nPress Enter to return to the Admin Menu...");
            Console.ReadLine();  // Wait for user input to return to the admin menu
        }


        private void ViewBookingHistory(string username)
        {
            Console.Clear();
            var customer = customers[username];  

            if (customer.Bookings.Count == 0)
            {
                Console.WriteLine("No bookings found for this customer.");
                Console.WriteLine("\nPress Enter to return...");
                Console.ReadLine();
                return;
            }

         
            Console.WriteLine($"Booking History for {username}:");
            for (int i = 0; i < customer.Bookings.Count; i++)
            {
                var booking = customer.Bookings[i];
                Console.WriteLine($"{i + 1}. Room Number: {booking.RoomNumber} | {booking.RoomType} - {booking.Nights} nights (Check-in: {booking.ReservationTime}, Total: ${booking.TotalCost})");
            }

            Console.WriteLine("\nEnter the number of the booking to cancel (or 0 to return): ");
            int choice;
            if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= customer.Bookings.Count)
            {
                var bookingToCancel = customer.Bookings[choice - 1];

              
                DateTime currentDate = DateTime.Now;
                int nightsStayed = (currentDate - bookingToCancel.ReservationTime).Days;
                int refundAmount = nightsStayed * bookingToCancel.TotalCost / bookingToCancel.Nights;

            
                Console.WriteLine($"Are you sure you want to cancel this booking? Refund: ${refundAmount}");
                Console.Write("Enter 'yes' to confirm cancellation: ");
                string cancelConfirmation = Console.ReadLine().ToLower();

                if (cancelConfirmation == "yes")
                {
                    
                    customer.Bookings.RemoveAt(choice - 1);

                  
                    SaveReservations();
                    SaveRooms();

                    Console.WriteLine("Booking successfully canceled.");
                    Console.WriteLine($"Refund Amount: ${refundAmount}");

                  
                    string cancelFile = "cancellations.txt";
                    using (StreamWriter writer = new StreamWriter(cancelFile, append: true))
                    {
                        writer.WriteLine($"{username}|{bookingToCancel.RoomNumber}|{bookingToCancel.RoomType}|{bookingToCancel.Nights}|{refundAmount}|{bookingToCancel.ReservationTime}|{currentDate}");
                    }
                }
                else
                {
                    Console.WriteLine("Booking cancellation aborted.");
                }
            }
            else
            {
                Console.WriteLine("Invalid selection. Returning to menu...");
            }

            Console.WriteLine("\nPress Enter to return...");
            Console.ReadLine();
        }


        private void ChangeCustomerPassword(Customer customer)
        {
            Console.Clear();
            Console.Write("Enter new password for " + customer.Username + ": ");
            string newPassword = Console.ReadLine();
            customer.Password = newPassword;
            Console.WriteLine("Password updated successfully.");
            SaveCustomers();  // Save the updated customer details
            Console.WriteLine("\nPress Enter to return to Search Menu...");
            Console.ReadLine();  // Wait for user input to return to the search menu
        }

        private void DeleteCustomerAccount(Customer customer)
        {
            Console.Clear();
            Console.Write("Are you sure you want to delete this customer account? (yes/no): ");
            string confirmation = Console.ReadLine().ToLower();

            if (confirmation == "yes")
            {
                customers.Remove(customer.Username);
                SaveCustomers();  // Save after removing customer
                Console.WriteLine("Customer account deleted successfully.");
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }

            Console.WriteLine("\nPress Enter to return to Search Menu...");
            Console.ReadLine();  // Wait for user input to return to the search menu
        }


        private void CancelSpecificReservation(string username, int bookingIndex)
        {
            var customer = customers[username];
            var bookingToCancel = customer.Bookings[bookingIndex];
            DateTime cancellationTime = DateTime.Now;

            // Calculate days stayed
            int daysStayed = (cancellationTime - bookingToCancel.ReservationTime).Days;
            if (daysStayed < 0) daysStayed = 0; // Ensure no negative values

            // Adjust the total cost for the days stayed
            int costPerNight = bookingToCancel.TotalCost / bookingToCancel.Nights;
            int adjustedTotalCost = daysStayed * costPerNight;

            // Refund calculation for unused days
            int refundAmount = bookingToCancel.TotalCost - adjustedTotalCost;

            Console.WriteLine($"\nReservation Details:");
            Console.WriteLine($"Room Type: {bookingToCancel.RoomType}");
            Console.WriteLine($"Check-in Time: {bookingToCancel.ReservationTime}");
            Console.WriteLine($"Cancellation Time: {cancellationTime}");
            Console.WriteLine($"Days Stayed: {daysStayed}");
            Console.WriteLine($"Refund Amount: ${refundAmount}");

            // Log the cancellation
            const string CancellationsFile = "cancellations.txt";
            using StreamWriter writer = new StreamWriter(CancellationsFile, append: true);
            writer.WriteLine($"{username}|{bookingToCancel.RoomType}|{bookingToCancel.ReservationTime}|{cancellationTime}|{daysStayed}|{adjustedTotalCost}|{refundAmount}");

            // Restore room availability for unused nights
            if (rooms.ContainsKey(bookingToCancel.RoomType))
            {
                rooms[bookingToCancel.RoomType].Count += bookingToCancel.Nights - daysStayed;
            }

            // Remove the booking and update the data
            customer.Bookings.RemoveAt(bookingIndex);
            SaveReservations();
            SaveRooms();

            Console.WriteLine("\nReservation canceled successfully. Details logged in cancellations.txt.");
        }




        private void CustomerMenu()
        {
            Console.Clear();
            Console.WriteLine("\nHello! I'm your friendly Hotel Chat Bot 🤖. How can I assist you today?");
            Console.WriteLine("Whether you're new here or a returning guest, I'm here to help you book your perfect stay! 🏨");

            while (true)
            {
                Console.WriteLine("\n[Customer Menu]");
                Console.WriteLine("1. Sign Up - Let's get you all set up with an account! 📝");
                Console.WriteLine("2. Login - Ready to check your bookings or make a new one? 🔑");
                Console.WriteLine("3. Exit - Heading out? No worries, come back anytime! 👋");
                Console.Write("\nPlease type 1, 2, or 3 to let me know what you'd like to do: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nAwesome! Let's create your account so you can start booking your dream stay! 🛌");
                        CustomerSignUp();
                        break;
                    case "2":
                        Console.WriteLine("\nWelcome back! Let me log you in so we can get started. 🔑");
                        CustomerLogin();
                        break;
                    case "3":
                        Console.WriteLine("\nAlright, I’ll miss you! Have a great day, and come back soon! 👋");
                        return;
                    default:
                        Console.WriteLine("\nHmm, I didn't quite catch that. Could you please type 1, 2, or 3? 🤔");
                        break;
                }
            }
        }


        private void CustomerSignUp()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            // Check if username already exists
            if (customers.ContainsKey(username))
            {
                Console.WriteLine("Username already exists. Please choose a different username.");
            }
            else
            {
                Console.Write("Enter password: ");
                string password = AnsiConsole.Prompt(
                new TextPrompt<string>("")
                    .Secret()); // Hides the password input

                // Create a new customer and add to the dictionary
                customers[username] = new Customer { Username = username, Password = password };

                Console.WriteLine("Sign-up successful!");
                SaveCustomers(); // Save immediately after sign-up
            }
        }



        private void CustomerLogin()
        {
            try
            {
                Console.Write("Enter username: ");
                string username = Console.ReadLine()?.Trim();
                Console.Write("Enter password: ");
                string password = AnsiConsole.Prompt(
                new TextPrompt<string>("")
                    .Secret()); // Hides the password input

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Username and password cannot be empty. Please try again.");
                    return;
                }

                if (customers.ContainsKey(username) && customers[username].Password == password)
                {
                    Console.WriteLine("Login successful!");
                    CustomerDashboard(username);
                }
                else
                {
                    Console.WriteLine("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }




        private void CustomerDashboard(string username)
        {
            while (true)
            {
                try
                {
                    
                    Console.WriteLine($"\nHello, {username}! Welcome back to your Customer Dashboard!");
                    Console.WriteLine("\nWhat would you like to do today?");
                    Console.WriteLine("1. View Room Availability");
                    Console.WriteLine("2. Book a Room");
                    Console.WriteLine("3. Checkout/View Reservation");
                    Console.WriteLine("4. Exit");
                    Console.Write("Please enter your choice: ");

                    string choice = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(choice))
                    {
                        Console.WriteLine("Choice cannot be empty. Please try again.");
                        continue;
                    }

                    switch (choice)
                    {
                        case "1":
                            ViewRoomAvailability();
                            break;
                        case "2":
                            BookRoom(username);
                            break;
                        case "3":
                            Checkout(username);
                            break;
                        case "4":
                            Console.WriteLine("Goodbye! Thank you for choosing our hotel.");
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please enter a number between 1 and 4.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }






        // Save all data to files
        private void SaveAll()
        {
            SaveCustomers();
            SaveRooms();
            SaveReservations();
        }

        private void SaveCustomers()
        {
            try
            {
                using StreamWriter writer = new StreamWriter(CustomersFile);
                foreach (var customer in customers.Values)
                {
                    writer.WriteLine($"{customer.Username}|{customer.Password}");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error saving customer data: {ex.Message}");
            }
        }





        private void LoadCustomers()
        {
            try
            {
                if (File.Exists(CustomersFile))
                {
                    foreach (var line in File.ReadAllLines(CustomersFile))
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 2)
                        {
                            var customer = new Customer
                            {
                                Username = parts[0],
                                Password = parts[1]
                            };
                            customers[customer.Username] = customer;
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error loading customer data: {ex.Message}");
            }
        }




        private void LoadReservations()
        {
            if (File.Exists(ReservationsFile))
            {
                foreach (var line in File.ReadAllLines(ReservationsFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 7) 
                    {
                        var booking = new Booking
                        {
                            RoomType = parts[1],
                            Nights = int.Parse(parts[2]),
                            TotalCost = int.Parse(parts[3]),
                            ReservationTime = DateTime.Parse(parts[4]),
                            CheckoutTime = DateTime.Parse(parts[5]),
                            RoomNumber = parts[6] 
                        };

                        string username = parts[0];
                        if (customers.ContainsKey(username))
                        {
                            customers[username].Bookings.Add(booking);
                        }
                    }
                }
            }
        }



        private void SaveReservations()
        {
            using StreamWriter writer = new StreamWriter(ReservationsFile);
            foreach (var customer in customers.Values)
            {
                foreach (var booking in customer.Bookings)
                {
                    writer.WriteLine($"{customer.Username}|{booking.RoomType}|{booking.Nights}|{booking.TotalCost}|{booking.ReservationTime}|{booking.CheckoutTime}|{booking.RoomNumber}");
                }
            }
        }


        private void Checkout(string username)
        {
            Console.Clear();
            var customer = customers[username];

            if (customer.Bookings.Count == 0)
            {
                Console.WriteLine("You have no active bookings to check out.");
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            
            Console.WriteLine("\nYour Reservations:");
            for (int i = 0; i < customer.Bookings.Count; i++)
            {
                var booking = customer.Bookings[i];
                Console.WriteLine($"{i + 1}. Room Number: {booking.RoomNumber} | {booking.RoomType} - {booking.Nights} nights (Check-in: {booking.ReservationTime}, Total: ${booking.TotalCost})");
            }

            Console.Write("\nSelect a reservation to check out (enter number)/Leave Blank if you dont want to checkout: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= customer.Bookings.Count)
            {
                var selectedBooking = customer.Bookings[choice - 1];

               
                selectedBooking.CheckoutTime = selectedBooking.ReservationTime.AddDays(selectedBooking.Nights);

                
                Console.WriteLine("\n==========================================");
                Console.WriteLine("               HOTEL RECEIPT              ");
                Console.WriteLine("==========================================");
                Console.WriteLine($"Customer Name: {username}");
                Console.WriteLine($"Date: {DateTime.Now}");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine($"Room Number    : {selectedBooking.RoomNumber}");  
                Console.WriteLine($"Room Type      : {selectedBooking.RoomType}");
                Console.WriteLine($"Check-in Time  : {selectedBooking.ReservationTime}");
                Console.WriteLine($"Checkout Time  : {selectedBooking.CheckoutTime}");
                Console.WriteLine($"Nights Stayed  : {selectedBooking.Nights}");
                Console.WriteLine($"Total Cost     : ${selectedBooking.TotalCost}");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("Thank you for staying with us!");
                Console.WriteLine("==========================================\n");

             
                const string CheckoutFile = "checkouts.txt";
                using (StreamWriter writer = new StreamWriter(CheckoutFile, append: true))
                {
                    writer.WriteLine($"{username}|{selectedBooking.RoomNumber}|{selectedBooking.RoomType}|{selectedBooking.Nights}|{selectedBooking.TotalCost}|{selectedBooking.ReservationTime}|{selectedBooking.CheckoutTime}");
                }

               
                customer.Bookings.RemoveAt(choice - 1);

                
                var reservations = File.Exists(ReservationsFile)
                    ? new List<string>(File.ReadAllLines(ReservationsFile))
                    : new List<string>();

                reservations.RemoveAll(line => line.StartsWith($"{username}|"));
                File.WriteAllLines(ReservationsFile, reservations);

                
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                Console.Clear();
            }
            else
            {
                Console.WriteLine("Invalid selection. Please try again.");
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private void SaveRooms()
        {
            using StreamWriter writer = new StreamWriter(RoomsFile);
            foreach (var room in rooms)
            {
                writer.WriteLine($"{room.Key}|{room.Value.Price}|{room.Value.Count}|{string.Join(",", room.Value.Amenities)}");
            }
        }

        private void ChangeRoomDetails()
        {
            Console.Clear();
            Console.Write("Enter room type (Standard/Deluxe/Suite): ");
            string roomType = Console.ReadLine();

            if (rooms.ContainsKey(roomType))
            {
                try
                {
                    Console.Write($"Enter new price for {roomType}: ");
                    rooms[roomType].Price = int.Parse(Console.ReadLine());

                    Console.Write($"Enter new room count for {roomType}: ");
                    rooms[roomType].Count = int.Parse(Console.ReadLine());

                    Console.WriteLine($"{roomType} updated successfully!");
                    SaveRooms(); 
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter numeric values for price and count.");
                }
            }
            else
            {
                Console.WriteLine("Room type not found.");
            }
        }



        private void LoadRooms()
        {
            rooms = new Dictionary<string, Room>();
            if (File.Exists(RoomsFile))
            {
                foreach (var line in File.ReadAllLines(RoomsFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 4)
                    {
                        rooms[parts[0]] = new Room
                        {
                            Price = int.Parse(parts[1]),
                            Count = int.Parse(parts[2]),
                            Amenities = parts[3].Split(',')
                        };
                    }
                }
            }
            else
            {
                Console.WriteLine("Room file not found. Creating default rooms...");
                rooms["STANDARD"] = new Room { Price = 100, Count = 10, Amenities = new[] { "WiFi", "TV" } };
                rooms["DELUXE"] = new Room { Price = 200, Count = 5, Amenities = new[] { "WiFi", "TV", "Minibar" } };
                rooms["SUITE"] = new Room { Price = 300, Count = 2, Amenities = new[] { "WiFi", "TV", "Minibar", "Jacuzzi" } };
                SaveRooms();
            }
        }
        private void BookRoom(string username)
        {
            Console.Write("Enter room type to book (Standard/Deluxe/Suite): ");
            string roomType=Console.ReadLine().ToUpper();
            
            if (rooms.ContainsKey(roomType  ) && rooms[roomType].Count > 0)
            {
                try
                {
                    Console.Write("Enter number of nights: ");
                    int nights = int.Parse(Console.ReadLine());

                  
                    int totalCost = rooms[roomType].Price * nights;

                    
                    string roomNumberPrefix = roomType switch
                    {
                        "STANDARD" => "S",
                        "DELUXE" => "D",
                        "SUITE" => "R", 
                        _ => "U" 
                    };
                    int roomIndex = rooms[roomType].Count; 
                    string roomNumber = $"{roomNumberPrefix}{roomIndex:D2}"; 

                    Console.WriteLine($"Total cost: ${totalCost}");
                    Console.Write("Confirm booking? (yes/no): ");
                    if (Console.ReadLine().ToLower() == "yes")
                    {
                        
                        rooms[roomType].Count--;

                        
                        var booking = new Booking
                        {
                            RoomType = roomType,
                            Nights = nights,
                            TotalCost = totalCost,
                            ReservationTime = DateTime.Now,
                            RoomNumber = roomNumber, 
                            CheckoutTime = DateTime.Now.AddDays(nights) 
                        };

                        
                        customers[username].Bookings.Add(booking);

                        
                        SaveReservations();
                        SaveRooms();

                        Console.WriteLine($"Room booked successfully! Room Number: {roomNumber}");
                    }
                    else
                    {
                        Console.WriteLine("Booking canceled.");
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter a valid number of nights.");
                }
            }
            else
            {
                Console.WriteLine("Room not available.");
            }
        }



        private void ViewRoomAvailability()
        {
            Console.Clear();
            Console.WriteLine("\n🤖 Hello! I'm your friendly Hotel Assistant here to help you with room availability. ");
            Console.WriteLine("\nHere's the current status of our rooms:");

            foreach (var room in rooms)
            {
                Console.WriteLine($"\n{room.Key}: {room.Value.Count} rooms available at ${room.Value.Price}/night");
                Console.WriteLine($"Amenities: {string.Join(", ", room.Value.Amenities)}");

                // Adding chatbot comments based on room availability
                if (room.Value.Count > 10)
                {
                    Console.WriteLine(" Plenty of rooms available! It's the perfect time to book one. ");
                }
                else if (room.Value.Count > 0 && room.Value.Count <= 10)
                {
                    Console.WriteLine(" Rooms are filling up fast! Grab one before they're all gone! ");
                }
                else
                {
                    Console.WriteLine(" Oh no! This room type is fully booked at the moment.  Check back later!");
                }
            }

            Console.WriteLine("\n Is there anything else I can assist you with? Press Enter to return to the main menu.");
            Console.ReadLine(); // Pause for user interaction before returning
        }



    }
}
