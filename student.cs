using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Windows.Forms.LinkLabel;
using System.Threading;
using MySql.Data.MySqlClient;

namespace erp_2
{
    public class Student
    {
        public string name { get; set; } //이름
        public string department { get; set; } //학과
        public int year { get; set; } // 입학년도
        public string dormitory { get; set; } //기숙사여부
        public string gender { get; set; } //성별
        public string birth { get; set; } //생년월일
        public string status { get; set; } // 제적상태
        public string ID { get; set; } // 학번
        public string Password { get; set; }
        public string Photo { get; set; }
        public string e_mail { get; set; }
        public string phone_number { get; set; }
        public string advisor { get; set; }
        public string livingroom { get; set; }
        public int grade { get; set; }
        public int semester {  get; set; }
        public string parent {  get; set; }
        public string bank { get; set; }
        public string account { get; set; }
        public string roommate { get; set; }
        public List<string> basket { get; set; }
        public List<List<string>> score { get; set; }
        public string avr_score { get; set; }
        public List<string> apply {  get; set; }
        public string back_year { get; set; }
        public string back_semester { get; set; }
        public string start_year { get; set; }
        public string start_semester { get; set; }
        public string start_date { get; set; }
        public string start_type { get; set; }

        private static string server = "192.168.31.147";
        private static string database = "team3";
        private static string uid = "root";
        private static string Password_1 = "0000";
        string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={Password_1};";
        Dictionary<string, string> departmentCodes = new Dictionary<string, string>
        {
            { "경영학과", "01" },
            { "체육학과", "02" },
            { "컴퓨터공학과", "03" }
        };

        public void update_student_id()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // 학과별로 가장 높은 일련번호 조회
                var maxIdQuery = @"
                SELECT department, IFNULL(MAX(CAST(SUBSTRING(ID, 7, 2) AS UNSIGNED)), 0) AS maxId 
                FROM stu_personal 
                GROUP BY department;
            ";

                var command = new MySqlCommand(maxIdQuery, connection);
                var reader = command.ExecuteReader();

                var maxIdsByDepartment = new Dictionary<string, int>();
                while (reader.Read())
                {
                    string department = reader["department"].ToString();
                    int maxId = Convert.ToInt32(reader["maxId"]);
                    maxIdsByDepartment[department] = maxId;
                }
                reader.Close();

                // '-'로 설정된 학번을 가진 학생 조회
                var studentsQuery = "SELECT name, department, year FROM stu_personal WHERE ID = '-';";
                command = new MySqlCommand(studentsQuery, connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string department = reader["department"].ToString();
                    int year = Convert.ToInt32(reader["year"]);
                    string departmentCode = departmentCodes[department];
                    int newIdNumber = maxIdsByDepartment.ContainsKey(department) ? ++maxIdsByDepartment[department] : 1;

                    // 새 학번 생성
                    string newId = $"{year}{departmentCode}{newIdNumber:D2}";

                    // 학번 업데이트
                    reader.Close();
                    var updateQuery = "UPDATE stu_personal SET ID = @NewId WHERE name = @Name;";
                    var updateCommand = new MySqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@NewId", newId);
                    updateCommand.Parameters.AddWithValue("@Name", name);
                    updateCommand.ExecuteNonQuery();

                    // 다음 학생을 위해 reader 다시 열기
                    reader = command.ExecuteReader();
                }
                reader.Close();
            }

        }
        public void update_advisor()
        { 

            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                // 학과별 교수 목록 조회
                var professorQuery = "SELECT name, department FROM pro_personal;";
                MySqlCommand professorCommand = new MySqlCommand(professorQuery, connection);
                MySqlDataReader professorReader = professorCommand.ExecuteReader();

                var professorsByDepartment = new Dictionary<string, List<string>>();
                while (professorReader.Read())
                {
                    string name = professorReader["name"].ToString();
                    string department = professorReader["department"].ToString();
                    if (!professorsByDepartment.ContainsKey(department))
                    {
                        professorsByDepartment[department] = new List<string>();
                    }
                    professorsByDepartment[department].Add(name);
                }
                professorReader.Close();

                // 학과별 학생 목록 조회 및 지도 교수 배정 (advisor가 '-' 인 경우에만)
                foreach (var department in professorsByDepartment.Keys)
                {
                    var studentsQuery = $"SELECT ID FROM stu_personal WHERE department = '{department}' AND advisor = '-';";
                    MySqlCommand studentsCommand = new MySqlCommand(studentsQuery, connection);
                    MySqlDataReader studentsReader = studentsCommand.ExecuteReader();

                    var studentIds = new List<string>();
                    while (studentsReader.Read())
                    {
                        studentIds.Add(studentsReader["ID"].ToString());
                    }
                    studentsReader.Close();

                    var professors = professorsByDepartment[department];
                    for (int i = 0; i < studentIds.Count; i++)
                    {
                        string advisor = professors[i % professors.Count];
                        string studentId = studentIds[i];

                        // 학생 정보에 지도 교수 업데이트 (advisor가 '-' 인 경우에만)
                        var updateQuery = $"UPDATE stu_personal SET advisor = '{advisor}' WHERE ID = '{studentId}' AND advisor = '-';";
                        MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}





