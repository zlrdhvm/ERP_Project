using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using erp_2;
using professor;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using Mysqlx.Notice;

namespace universal2
{
    public partial class login_window : Form
    {
        private MySqlConnection conn;
        private static string server = "192.168.31.147";
        private static string database = "team3";
        private static string uid = "root";
        private static string Password_1 = "0000";
        string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={Password_1};";
        public login_window()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private async Task FetchWeatherInfo()
        {
            string baseUrl = "https://api.openweathermap.org/data/2.5/weather?lat=36.3333&lon=127.4167&appid=35abe63960c29f65f12e6540e4f3a7b6&units=metric";
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(baseUrl);
                var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(response);

                int tempInt = Convert.ToInt32(Math.Round(weatherInfo.Main.Temp));
                label7.Text = $"{tempInt} ºC \n{weatherInfo.Weather[0].Main}";

                string iconCode = weatherInfo.Weather[0].Icon;
                string iconUrl = $"http://openweathermap.org/img/w/{iconCode}.png";
                pictureBox8.Load(iconUrl);
            }
        }
        public class WeatherInfo
        {
            public Weather[] Weather { get; set; }
            public Main Main { get; set; }
        }

        public class Weather
        {
            public string Main { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
        }

        public class Main
        {
            public double Temp { get; set; }
        }
        

        private async void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 100;
            timer1.Start();
            await FetchWeatherInfo();

            password.PasswordChar = '*';
            password2.PasswordChar = '*';

            pictureBox2.Cursor = Cursors.Hand;
            pictureBox3.Cursor = Cursors.Hand;
            pictureBox4.Cursor = Cursors.Hand;
            pictureBox5.Cursor = Cursors.Hand;
            pictureBox6.Cursor = Cursors.Hand;
            pictureBox7.Cursor = Cursors.Hand;
        }

        private void clock_block(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString("yyyy년 MM월 dd일");
            label2.Text = DateTime.Now.ToString("HH시 mm분");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                password.PasswordChar = '\0';
            }
            else
            {
                password.PasswordChar = '*';
            }

            password.MaxLength = 14;
        }

        private void lunchmenu_btn(object sender, EventArgs e)
        {
            Process.Start("https://www.ansan.ac.kr/www/meals/1");
        }

        private void homepage_btn(object sender, EventArgs e)
        {
            Process.Start("http://newhome.hannam.ac.kr/240214-sugang-Ub6ag/");
        }

        private void schedule_btn(object sender, EventArgs e)
        {
            Process.Start("http://hnu.kr/kor/guide/guide_01_2.html");
        }

        private void office365_btn(object sender, EventArgs e)
        {
            Process.Start("http://hnu.kr/kor/life/life_09_1.html");
        }

        private void certificate_btn(object sender, EventArgs e)
        {
            Process.Start("http://hnu.kr/kor/guide/guide_06_1.html");
        }

        private void campusmap_btn(object sender, EventArgs e)
        {
            Process.Start("https://www.hannam.ac.kr/data/building.html");
        }

        private void library_btn(object sender, EventArgs e)
        {
            Process.Start("https://hanul.hannam.ac.kr/");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                password2.PasswordChar = '\0';
            }
            else
            {
                password2.PasswordChar = '*';
            }

            password2.MaxLength = 14;
        }


        private void student_ID_Click(object sender, EventArgs e)
        {
            this.ID.Text = string.Empty;
        }

        private void student_password_Click(object sender, EventArgs e)
        {
            this.password.Text = string.Empty;
        }

        private void profeossor_ID_Click(object sender, EventArgs e)
        {
            this.ID2.Text = string.Empty;
            this.ID2.ReadOnly = false;
        }

        private void profeossor_password_Click(object sender, EventArgs e)
        {
            this.password2.Text = string.Empty;
            this.password2.ReadOnly = false;
        }

        private void Tb_Loginform_Pw_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                login_btn1(sender, e);
            }
        }

        private void Tb_Loginform_Pw_KeyDown2(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                login_btn2(sender, e);
            }
        }

        private void login_btn1(object sender, EventArgs e)
        {

            string userId = ID.Text;
            string userPassword = password.Text;

            Student loggedInStudent = CheckLogin(userId, userPassword);

            if (loggedInStudent != null)
            {
                MessageBox.Show("로그인에 성공했습니다11. ", "로그인");
                student_main aa = new student_main(loggedInStudent);
                this.Hide();
                aa.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("ID 또는 비밀번호가 틀립니다11.", "로그인");
            }
        }
        private void login_btn2(object sender, EventArgs e)
        {
            string userId2 = ID2.Text;
            string userPassword2 = password2.Text;

            Professor loggedInProfessor = CheckLogin2(userId2, userPassword2);

            if (loggedInProfessor != null)
            {
                MessageBox.Show("로그인에 성공했습니다11. ", "로그인");
                professor_main bb = new professor_main(loggedInProfessor);
                this.Hide();
                bb.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("ID 또는 비밀번호가 틀립니다22.", "로그인");
            }
        }
        private bool make_connection()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
        private Student CheckLogin(string userId, string userPassword)
        {
            // 데이터베이스 연결 설정
            server = "192.168.31.147";
            database = "team3"; // 여기서 사용할 데이터베이스 이름으로 변경하세요.
            uid = "root";
            Password_1 = "0000";
            string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={Password_1};";
            conn = new MySqlConnection(connectionString);

            try
            {
                // 데이터베이스 연결 시도
                if (make_connection())
                {
                    string query = $"SELECT * FROM stu_personal WHERE ID = '{userId}' AND Password = '{userPassword}';";
                    
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    
                    if (reader.HasRows) // 사용자가 존재한다면
                    {
                        reader.Close();
                        return GetStudentInfo(userId);
                    }
                    else
                    {
                        reader.Close();
                        return null; // 로그인 실패
                    }
                }
                else
                {
                    return null; // 데이터베이스 연결 실패
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null; // 예외 발생 시 로그인 실패
            }
            finally
            {
                conn.Close(); // 데이터베이스 연결이 열려 있으면 닫기
            }
        }
        public Student GetStudentInfo(string userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var query = $"SELECT \r\n    stu_personal.name, \r\n    stu_personal.department, \r\n    stu_personal.year, \r\n    stu_personal.gender, \r\n    stu_personal.birth, \r\n    stu_personal.status, \r\n    stu_personal.ID, \r\n    stu_personal.Password, \r\n    stu_personal.Photo, \r\n    stu_personal.e_mail, \r\n    stu_personal.phone_number, \r\n    stu_personal.advisor, \r\n    stu_personal.grade, \r\n    stu_personal.semester,\r\n    dormitory.dormitory, \r\n    dormitory.livingroom, \r\n    dormitory.parent, \r\n    dormitory.bank, \r\n    dormitory.account, \r\n    dormitory.roommate\r\nFROM \r\n    stu_personal\r\nINNER JOIN \r\n    dormitory ON stu_personal.name = dormitory.name\r\nWHERE \r\n    stu_personal.ID = '{userId}';\r\n";
                var command = new MySqlCommand(query, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Student
                        {
                            name = reader["name"].ToString(),
                            department = reader["department"].ToString(),
                            year = int.Parse(reader["year"].ToString()),
                            gender = reader["gender"].ToString(),
                            birth = reader["birth"].ToString(),
                            status = reader["status"].ToString(),
                            ID = reader["ID"].ToString(),
                            Password = reader["Password"].ToString(),
                            Photo = reader["Photo"].ToString(),
                            e_mail = reader["e_mail"].ToString(),
                            phone_number = reader["phone_number"].ToString(),
                            advisor = reader["advisor"].ToString(),
                            grade = int.Parse(reader["grade"].ToString()),
                            semester = int.Parse(reader["semester"].ToString()),
                            livingroom = reader["livingroom"].ToString(),
                            dormitory = reader["dormitory"].ToString(),
                            parent = reader["parent"].ToString(),
                            bank = reader["bank"].ToString(),
                            account = reader["account"].ToString(),
                            roommate = reader["roommate"].ToString(),
                            //basket = new List<string>(),
                            score = get_score(userId),
                            avr_score = "",
                            apply = new List<string>(),
                            back_year = "",
                            back_semester = "",
                            start_year = "",
                            start_semester = "",
                            start_date = "",
                            start_type = ""
                        };
                    }
                }
            }
            return null; // 사용자 정보 없음
        }

        public List<List<string>> get_score(string userId)
        {
            List<List<string>> scores = new List<List<string>>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var query = $"SELECT * FROM stu_results WHERE ID='{userId}'";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 각 성적 항목을 별도의 리스트로 생성
                            List<string> score = new List<string>
                            {
                                reader["subject"].ToString(), // 과목명
                                reader["completion"].ToString(), // 이수 구분
                                reader["score"].ToString() // 성적
                            };
                            // 이중 리스트에 추가
                            scores.Add(score);
                        }
                    }
                }
            }

            return scores;
        }
        public Professor CheckLogin2(string userId2, string userPassword2)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            try
            {
                // 데이터베이스 연결 시도
                conn.Open();

                string query = $"SELECT * FROM pro_personal WHERE ID = '{userId2}' AND Password = {userPassword2};";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) // 교수 정보가 존재한다면
                {
                    // 해당 교수 정보로 Professor 객체를 생성하여 반환합니다.
                    Professor professor = new Professor
                    {
                        name = reader["name"].ToString(),
                        department = reader["department"].ToString(),
                        year = int.Parse(reader["year"].ToString()),
                        gender = reader["gender"].ToString(),
                        birth = reader["birth"].ToString(),
                        ID = reader["ID"].ToString(),
                        Password = reader["Password"].ToString(),
                        Photo = reader["Photo"].ToString(),
                        e_mail = reader["e_mail"].ToString(),
                        phone_number = reader["phone_number"].ToString()
                    };
                    reader.Close();
                    return professor;
                }
                reader.Close();
                return null; // 로그인 실패 또는 교수 정보 없음
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null; // 예외 발생 시
            }
            finally
            {
                conn.Close(); // 데이터베이스 연결이 열려 있으면 닫기
            }
        }
    }
}
