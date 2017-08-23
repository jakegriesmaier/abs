using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace abs {
    class Program {
        static void Main(string[] args) {
            string connectionString = "Server=localhost;Port=5432;Username=postgres;Password=biggears;Database=postgres";
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            Console.WriteLine(connection.State);
            printAll(connection, "users");
            
            Console.ReadKey();
        }

        static void executeCommand(NpgsqlConnection connection, string query) {
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        static void printAll(NpgsqlConnection connection, string table) {
            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM " + table, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) Console.WriteLine(reader.GetString(0) + "," + reader.GetString(1));
            reader.Close();
            command.Dispose();
        }
    }
}
