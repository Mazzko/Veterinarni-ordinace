using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinarniOrdinace.Models;

namespace VeterinarniOrdinace.Data
{
    public class VisitRepository
    {
        private readonly string _cs = DatabaseInitializer.ConnectionString;

        public List<Visit> GetAll()
        {
            var result = new List<Visit>();

            using var con = new SqliteConnection(this._cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            SELECT Id, AnimalId, Date, Reason, Diagnosis, Price, Status
            FROM Visits
            ORDER BY Date DESC, Id DESC;
            """;

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var dateText = r.GetString(2);
                var date = DateTime.TryParse(dateText, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.Parse(dateText);

                result.Add(new Visit
                {
                    Id = r.GetInt32(0),
                    AnimalId = r.GetInt32(1),
                    Date = date,
                    Reason = r.IsDBNull(3) ? null : r.GetString(3),
                    Diagnosis = r.IsDBNull(4) ? null : r.GetString(4),
                    Price = r.IsDBNull(5) ? null : r.GetDouble(5),
                    Status = r.IsDBNull(6) ? null : r.GetString(6)
                });
            }

            return result;
        }

        public int Insert(Visit v)
        {
            using var con = new SqliteConnection(_cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO Visits (AnimalId, Date, Reason, Diagnosis, Price, Status)
            VALUES ($animalId, $date, $reason, $diagnosis, $price, $status);
            SELECT last_insert_rowid();
            """;

            cmd.Parameters.AddWithValue("$animalId", v.AnimalId);
            cmd.Parameters.AddWithValue("$date", v.Date.ToString("o"));
            cmd.Parameters.AddWithValue("$reason", (object?)v.Reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$diagnosis", (object?)v.Diagnosis ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$price", v.Price.HasValue ? v.Price.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$status", (object?)v.Status ?? DBNull.Value);

            var id = (long)cmd.ExecuteScalar()!;
            return (int)id;
        }

        public void Update(Visit v)
        {
            using var con = new SqliteConnection(_cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText =
            """
            UPDATE Visits
            SET AnimalId=$animalId,
                Date=$date,
                Reason=$reason,
                Diagnosis=$diagnosis,
                Price=$price,
                Status=$status
            WHERE Id=$id;
            """;

            cmd.Parameters.AddWithValue("$id", v.Id);
            cmd.Parameters.AddWithValue("$animalId", v.AnimalId);
            cmd.Parameters.AddWithValue("$date", v.Date.ToString("o"));
            cmd.Parameters.AddWithValue("$reason", (object?)v.Reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$diagnosis", (object?)v.Diagnosis ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$price", v.Price.HasValue ? v.Price.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$status", (object?)v.Status ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
        public void Delete(int id)
        {
            using var con = new SqliteConnection(_cs);
            con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Visits WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
