using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using IceCastLibrary;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        // Replace these values with your actual database information
        string server = "4.227.176.108";
        string database = "nieappworld";
        string username = "kesara";
        string password = "Nie@12345678";
            
        // Build the connection string
        string connectionString = $"Server={server};Database={database};User ID={username};Password={password};";

        // Create a MySqlConnection object
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                // Open the connection
                connection.Open();

                // Connection is successful, you can perform database operations here

                string query = "SELECT automation_file FROM automations";

                // Create a MySqlCommand object
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Create a list to store the automation_file values
                    List<string> automationFiles = new List<string>();

                    // Execute the reader and read the results
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Assuming automation_file is of type string in the database
                            string automationFile = reader["automation_file"].ToString();


                            automationFile = Path.Combine("C:\\xampp\\htdocs\\public", automationFile);

                            automationFile = automationFile.Replace("/", "\\");
                            automationFiles.Add(automationFile);

                        }
                    }

                    // Specify the path for the text file
                    string filePath = "playlist.txt";

                    // Write the list of automation_file values to the text file
                    File.WriteAllLines(filePath, automationFiles);

                    Console.WriteLine($"Data written to {filePath}");
                }

                // Don't forget to close the connection when done
                connection.Close();
                stream();
            }
            

            

            
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        
    }
    private static void stream()
    {
        try
        {
            // Icecast server details
            var icecast = new Libshout();
            icecast.setProtocol(0);
            icecast.setHost("172.212.81.114");
            icecast.setPort(8000);
            icecast.setPassword("hackme");
            icecast.setFormat(Libshout.FORMAT_MP3);
            icecast.setPublic(true);
            icecast.setDescription("Icecast Broadcaster Example");
            icecast.setName("Test Radio Example");
            icecast.setMount("live");
            icecast.open();

            if (!icecast.isConnected())
            {
                Console.WriteLine(icecast.GetError());

                return;
            }

            Console.WriteLine("connected to" + icecast.getUrl() + icecast.getMount());

            // Read playlist and loop through files
            string playlistPath = Path.Combine(Environment.CurrentDirectory, "playlist.txt");
            string[] playlist = File.ReadAllLines(playlistPath);

            foreach (var filename in playlist)
            {
                Console.WriteLine($"Now streaming: {filename}" + "Now Streaming");

                // Read the MP3 file
                BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open));
                var length = reader.BaseStream.Length;
                int total = 0;

                byte[] buff = new byte[4096];
                int read;

                while (true)
                {
                    // Read buffer
                    read = reader.Read(buff, 0, buff.Length);
                    total = total + read;

                    Console.WriteLine($"Position: {(total / (double)length) * 100:00.00}%");

                    // If not the end, then send the buffer to Icecast
                    if (read > 0)
                    {
                        icecast.send(buff, read);  // Sync inside method
                    }
                    else break;
                }

                reader.Close(); // Close the current file
            }

            icecast.close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}
