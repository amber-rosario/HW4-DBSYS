using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Asn1;

namespace mysqlex1
{
    internal class Program
    {
        // MySQL connection string

        private static string connectionString = "server=localhost;user=new_username1;database=new_database1;port=3306;password=new_password1";
        static void Main(string[] args)
        {
            EnsureTableExists();
            bool keepRunning = true;
            while (keepRunning)
            {
                Console.WriteLine("Choose an operation: ");
                Console.WriteLine("1. Create a record ");
                Console.WriteLine("2. Read Records ");
                Console.WriteLine("3. Update a record ");
                Console.WriteLine("4. Delete a record ");
                Console.WriteLine("5. Exit ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        CreateRecord();
                        break;
                    case "2":
                        ReadRecords();
                        break;
                    case "3":
                        UpdateRecord();
                        break;
                    case "4":
                        DeleteRecord();
                        break;
                    case "5":
                        keepRunning = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }

        static void CreateRecord()
        {
            string name;
            string email;
            string phone;
            string address;

            // Validate name (non-empty)
            do
            {
                Console.Write("Enter Name: ");
                name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("Name cannot be empty.");
                }
            } while (string.IsNullOrEmpty(name));

            // Validate email (use regular expression)
            do
            {
                Console.Write("Enter Email: ");
                email = Console.ReadLine();
                if (!ValidateEmail(email))
                {
                    Console.WriteLine("Invalid email format.");
                }
            } while (!ValidateEmail(email));

            // Validate phone (only digits and at least 10 characters)
            do
            {
                Console.Write("Enter Phone (only digits, at least 10 digits): ");
                phone = Console.ReadLine();
                if (!ValidatePhone(phone))
                {
                    Console.WriteLine("Invalid phone number.");
                }
            } while (!ValidatePhone(phone));

            // Validate address (non-empty)
            do
            {
                Console.Write("Enter Address: ");
                address = Console.ReadLine();
                if (string.IsNullOrEmpty(address))
                {
                    Console.WriteLine("Address cannot be empty.");
                }
            } while (string.IsNullOrEmpty(address));

            // Insert into the database
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO USER (name, email, phone, address) VALUES (@name, @email, @phone, @address)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@address", address);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Record inserted successfully.");
            }
        }

        // Validation methods
        static bool ValidateEmail(string email)
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@]+@[^@]+\.[^@]+$");
            return emailRegex.IsMatch(email);
        }

        static bool ValidatePhone(string phone)
        {
            return phone.All(char.IsDigit) && phone.Length >= 10;
        }



        static void ReadRecords()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Connection established successfully!");

                    // Ensure the query is correct and matches your table schema
                    string query = "SELECT name, email, phone, address, created_at FROM USER";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Debugging the query
                    Console.WriteLine("Executing query: " + query);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Check if the reader has rows before attempting to read
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("No records found.");
                        return;
                    }

                    // Create a DataTable to display records
                    DataTable table = new DataTable();
                    table.Columns.Add("Name");
                    table.Columns.Add("Email");
                    table.Columns.Add("Phone");
                    table.Columns.Add("Address");
                    table.Columns.Add("Created At");

                    // Read the records from the reader
                    while (reader.Read())
                    {
                        DataRow row = table.NewRow();
                        row["Name"] = reader["name"].ToString();
                        row["Email"] = reader["email"].ToString();
                        row["Phone"] = reader["phone"].ToString();
                        row["Address"] = reader["address"].ToString();
                        row["Created At"] = reader["created_at"].ToString();
                        table.Rows.Add(row);
                    }

                    reader.Close();  // Always close the reader when done

                    // Print the DataTable to the console in a formatted manner
                    PrintFormattedDataTable(table);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("An error occurred while reading records from the database:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred:");
                Console.WriteLine(ex.Message);
            }
        }


        static void PrintFormattedDataTable(DataTable table)
        {
            // Calculate column widths based on the longest value in each column
            int[] columnWidths = new int[table.Columns.Count];

            // Find the maximum length of each column's data (including headers)
            for (int i = 0; i < table.Columns.Count; i++)
            {
                int maxLength = table.Columns[i].ColumnName.Length; // Start with the header length
                foreach (DataRow row in table.Rows)
                {
                    int cellLength = row[i].ToString().Length;
                    if (cellLength > maxLength)
                    {
                        maxLength = cellLength;
                    }
                }
                columnWidths[i] = maxLength;
            }

            // Print the headers with padding
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Console.Write(table.Columns[i].ColumnName.PadRight(columnWidths[i] + 2));
            }
            Console.WriteLine();

            // Print the rows with padding
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Console.Write(row[i].ToString().PadRight(columnWidths[i] + 2));
                }
                Console.WriteLine();
            }
        }


        static void UpdateRecord()
        {
            Console.Write("Enter the ID of the record to update: ");
            int id = int.Parse(Console.ReadLine());

            string name, email, phone, address;

            // Validate and get new values
            Console.WriteLine("Enter new Name (leave empty to keep current): ");
            name = Console.ReadLine();
            Console.WriteLine("Enter new Email (leave empty to keep current): ");
            email = Console.ReadLine();
            Console.WriteLine("Enter new Phone (leave empty to keep current): ");
            phone = Console.ReadLine();
            Console.WriteLine("Enter new Address (leave empty to keep current): ");
            address = Console.ReadLine();

            // SQL Update query
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE USER SET name = @name, email = @email, phone = @phone, address = @address WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                // Add parameters, using original values if the user left them empty
                cmd.Parameters.AddWithValue("@name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                cmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                Console.WriteLine("Record updated successfully.");
            }
        }


        static void DeleteRecord()
        {
            int id;

            // Prompt the user to enter an ID to delete
            Console.Write("Enter the ID of the person to delete: ");

            // Use TryParse to safely convert the input to an integer
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid input. Please enter a valid integer for the ID.");
                Console.Write("Enter the ID of the person to delete: ");
            }

            // Proceed with the deletion if we have a valid ID
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Define the delete query using the ID
                    string query = "DELETE FROM USER WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    // Execute the delete operation
                    int result = cmd.ExecuteNonQuery();

                    // Check if any rows were affected (i.e., if a record was deleted)
                    if (result > 0)
                    {
                        Console.WriteLine($"Record with ID {id} was deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"No record found with ID {id}.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }


    static void EnsureTableExists()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS `USER` (
                `id` INT AUTO_INCREMENT PRIMARY KEY,
                `name` VARCHAR(100) NOT NULL,
                `email` VARCHAR(100) NOT NULL,
                `phone` VARCHAR(15) NOT NULL,
                `address` TEXT NOT NULL,
                `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );";
                MySqlCommand cmd = new MySqlCommand(createTableQuery, conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Table 'USER' is ensured to exist.");
            }
        }

    }
}


