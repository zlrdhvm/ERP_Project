using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace erp_2
{

    public class newclass
    {
        private static string server = "192.168.31.147"; //ip 주소
        private static string database = "team3"; //ex) market
        private static string uid = "root"; //root
        private static string password = "0000"; //0000
        string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};";
        private static Dictionary<string, int> subjectCodeCounts = new Dictionary<string, int>();
        private Dictionary<string, string> departmentCodes = new Dictionary<string, string>
        {
            { "경영학과", "1" },
            { "체육학과", "2" },
            { "컴퓨터공학과", "3" }
        };

        private Dictionary<string, string> courseTypeCodes = new Dictionary<string, string>
        {
            { "전공필수", "05" },
            { "전공선택", "06" },
            { "교양필수", "07" },
            { "교양선택", "08" }
        };
        public void InitializeSubjectCodeCounts()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT department, completion, code FROM class";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var department = reader["department"].ToString();
                                var courseType = reader["completion"].ToString();
                                var code = reader["code"].ToString();

                                if (departmentCodes.TryGetValue(department, out string departmentCode) &&
                                    courseTypeCodes.TryGetValue(courseType, out string courseTypeCode))
                                {
                                    string key = $"{department}-{courseType}";
                                    string codePrefix = $"{departmentCode}{courseTypeCode}";

                                    if (code.StartsWith(codePrefix))
                                    {
                                        int sequenceNumber = int.Parse(code.Substring(codePrefix.Length));
                                        if (!subjectCodeCounts.ContainsKey(key) || subjectCodeCounts[key] < sequenceNumber)
                                        {
                                            subjectCodeCounts[key] = sequenceNumber;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing subject code counts: {ex.Message}");
            }
        }
        // 과목 코드 생성 함수
        public string GenerateSubjectCode(string department, string courseType)
        {
            string key = $"{department}-{courseType}"; // 학과와 이수구분의 조합을 키로 사용

            if (!subjectCodeCounts.ContainsKey(key))
            {
                subjectCodeCounts[key] = 0; // 해당 조합이 처음으로 등장하는 경우 초기화
            }

            if (!departmentCodes.TryGetValue(department, out string departmentCode))
            {
                // 해당 학과의 코드가 없는 경우
                throw new ArgumentException($"학과 코드를 찾을 수 없습니다: {department}");
            }

            if (!courseTypeCodes.TryGetValue(courseType, out string courseTypeCode))
            {
                // 해당 이수구분의 코드가 없는 경우
                throw new ArgumentException($"이수구분 코드를 찾을 수 없습니다: {courseType}");
            }

            // 해당 이수구분의 일련번호만 증가하도록 수정
            int count = ++subjectCodeCounts[key]; // 키에 대한 카운트를 증가
            string subjectCode = $"{departmentCode}{courseTypeCode}{count:D2}";

            return subjectCode;
        }
    }
}