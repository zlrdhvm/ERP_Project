using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace erp_2
{
    public class Professor
    {
        public List<Professor> Professors { get; private set; }
        public string name { get; set; } //이름
        public string department { get; set; } //학과
        public int year { get; set; } // 취임년도
        public string gender { get; set; } //성별
        public string ID { get; set; } // 교번
        public string Password { get; set; }
        public string birth { get; set; }
        public string Photo { get; set; }
        public string e_mail { get; set; }
        public string phone_number { get; set; }
        private static string server = "192.168.31.147";
        private static string database = "team3";
        private static string uid = "root";
        private static string Password_1 = "0000";
        string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={Password_1};";

        public void update_professor_id()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 학과별 교수님 ID의 최대값을 찾음
                    var query = @"
                    SELECT department, MAX(CAST(SUBSTRING(ID, 5, 4) AS UNSIGNED)) AS maxId
                    FROM pro_personal
                    GROUP BY department;";

                    var maxIdByDepartment = new Dictionary<string, int>();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var department = reader["department"].ToString();
                                var maxId = reader.IsDBNull(reader.GetOrdinal("maxId")) ? 0 : reader.GetInt32("maxId");
                                maxIdByDepartment[department] = maxId;
                            }
                        }
                    }

                    // 새 교수님 ID 생성 및 업데이트
                    foreach (var department in maxIdByDepartment.Keys)
                    {
                        var newId = maxIdByDepartment[department] + 1; // 새 ID는 현재 최대값 + 1
                        var newProfessorId = $"{DateTime.Now.Year}{department}{newId:D4}"; // 예: 2023010001

                        // ID가 '-'인 교수님에게 새 ID 부여
                        var updateQuery = $@"
                        UPDATE pro_personal
                        SET ID = '{newProfessorId}'
                        WHERE department = '{department}' AND ID = '-';";

                        using (var updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }

        }
    }
}
