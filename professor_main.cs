using erp_2;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using universal2;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace professor
{
    public partial class professor_main : Form
    {
        private Professor currentprofessor;
        public newclass newclass;
        private bool manualChange = false; // 사용자에 의한 수동 변경 여부를 나타내는 플래그
        private Dictionary<string, System.Windows.Forms.Button> buttonDict = new Dictionary<string, System.Windows.Forms.Button>();
        private MySqlConnection conn;
        private static string server = "192.168.31.147"; //ip 주소
        private static string database = "team3"; //ex) market
        private static string uid = "root"; //root
        private static string password = "0000"; //0000
        string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};";
        public professor_main(Professor professor)
        {
            newclass = new newclass();
            InitializeComponent();
            currentprofessor = professor;
            display_professor_info();
            update_top("professor_main", "메인화면");
            newclass.InitializeSubjectCodeCounts();
            
            // 초기 설정
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dateTimePicker1.ShowUpDown = true;
            // 이벤트 핸들러 등록
            dateTimePicker1.ValueChanged += DateTimePicker1_ValueChanged;
            dataGridView_submit_stu.RowPostPaint += dataGridView_submit_stu_RowPostPaint;
            menti_table.RowPostPaint += menti_RowPostPaint;
            // 타이머 설정
            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            DataTable row = new DataTable();
            DataGridViewRow row1 = new DataGridViewRow();
        }
        private void update_top(string baseName, string buttonText)
        {
            string buttonName = "button_" + baseName;
            string buttonXName = buttonName + "_x";
            string panelName = "panel_" + baseName;
            // 중복 생성 방지
            if (buttonDict.ContainsKey(buttonName))
            {
                // 해당하는 패널 찾기 및 맨 앞으로 가져오기
                Control[] targetPanels = this.Controls.Find(panelName, true);
                if (targetPanels.Length > 0)
                {
                    Panel targetPanel = targetPanels[0] as Panel;
                    if (targetPanel != null)
                    {
                        targetPanel.Visible = true; // 패널을 보이게 설정
                        targetPanel.BringToFront(); // 패널을 맨 앞으로 가져옴
                    }
                }
                return; // 이미 존재하는 버튼을 클릭한 경우, 여기서 함수 종료
            }
            // 버튼 위치 결정
            System.Windows.Forms.Button lastButton = buttonDict.Values.LastOrDefault();
            int nextXPosition = lastButton != null ? lastButton.Right + 30 : 10; // 마지막 버튼 다음 위치

            // 새로운 버튼 생성
            System.Windows.Forms.Button newButton = new System.Windows.Forms.Button
            {
                Name = buttonName,
                Text = buttonText,
                Location = new Point(nextXPosition, 5),
                AutoSize = true
            };
            panel_top.Controls.Add(newButton);
            buttonDict[buttonName] = newButton;

            newButton.Click += (sender, e) =>
            {
                // 해당하는 패널 찾기
                Control[] targetPanels = this.Controls.Find(panelName, true);
                if (targetPanels.Length > 0)
                {
                    Panel targetPanel = targetPanels[0] as Panel;
                    if (targetPanel != null)
                    {
                        targetPanel.Visible = true; // 패널을 보이게 설정
                        targetPanel.BringToFront(); // 패널을 맨 앞으로 가져옴
                        targetPanel.Location = new Point(250, 140); // 패널 위치 조정
                    }
                }
            };
            // 'X' 버튼 생성
            System.Windows.Forms.Button newXButton = new System.Windows.Forms.Button
            {
                Name = buttonXName,
                Text = "X",
                Location = new Point(newButton.Right, 5),
                Size = new Size(20, newButton.Height)
            };

            Control[] panels = this.Controls.Find(panelName, true);

            if (panels.Length > 0)
            {
                Panel panel = panels[0] as Panel;
                if (panel != null)
                {
                    panel.Location = new Point(250, 140);
                    panel.Visible = true; // 패널을 보이게 설정
                    panel.BringToFront(); // 패널을 맨 앞으로 가져옴
                }
            }
            newXButton.Click += (sender, e) =>
            {
                // 클릭된 'X' 버튼과 관련된 버튼 제거
                panel_top.Controls.Remove(newButton);
                panel_top.Controls.Remove(newXButton);
                buttonDict.Remove(buttonName);

                // 관련 패널 비활성화
                Control[] relatedpanels = this.Controls.Find(panelName, true);

                if (relatedpanels.Length > 0)
                {
                    Panel panel = panels[0] as Panel;
                    if (panel != null)
                    {
                        panel.Visible = false;
                    }
                }
                // 버튼 재정렬
                rearrange_buttons();
            };
            panel_top.Controls.Add(newXButton);
            rearrange_buttons(); // 버튼 재정렬
        }
        private void rearrange_buttons()
        {
            int xPosition = 10; // 시작 위치
            foreach (System.Windows.Forms.Button btn in panel_top.Controls.OfType<System.Windows.Forms.Button>().Where(b => !b.Name.EndsWith("_x")).OrderBy(b => b.Location.X))
            {
                btn.Location = new Point(xPosition, btn.Location.Y);
                System.Windows.Forms.Button xBtn = panel_top.Controls.OfType<System.Windows.Forms.Button>().FirstOrDefault(b => b.Name == btn.Name + "_x");
                if (xBtn != null)
                {
                    xBtn.Location = new Point(btn.Right, btn.Location.Y);
                    xPosition = xBtn.Right + 10; // 다음 버튼 위치 갱신
                }
            }
        }
        private void email_update(string email)
        {
            // 이메일 정보가 "-"가 아니라면, 해당 정보를 텍스트 필드에 분할하여 적용
            if (!string.IsNullOrEmpty(email) && email != "-")
            {
                var parts = email.Split('@');
                if (parts.Length == 2) // 정상적인 이메일 형식인지 확인
                {
                    email_id.Text = parts[0];
                    email_list.Text = parts[1];
                }
            }
        }
        private void number_update(string number)
        {
            // 전화번호 정보가 "-"가 아니라면, 해당 정보를 텍스트 필드에 분할하여 적용
            if (!string.IsNullOrEmpty(number) && number != "-")
            {
                var parts = number.Split('-');
                if (parts.Length == 3) // 정상적인 전화번호 형식인지 확인
                {
                    number_list.Text = parts[0];
                    middle_number.Text = parts[1];
                    last_number.Text = parts[2];
                }
            }
        }
        private void display_professor_info()
        {
            professor_name_box.Text = currentprofessor.name;
            professor_id_box.Text = currentprofessor.ID;
            professor_major_box.Text = currentprofessor.department;
            professor_email_box.Text = currentprofessor.e_mail;
            professor_number_box.Text = currentprofessor.phone_number;
            main_photo(currentprofessor.Photo);
        }
        private void main_photo(string photoFileName)
        {
            string filePath = Path.Combine(Application.StartupPath, "profile", photoFileName);
            if (File.Exists(filePath))
            {
                professor_main_profile.Image = Image.FromFile(filePath);
            }
            else
            {
                // default.jpg 표시 또는 다른 처리
                professor_main_profile.Image = erp_2.Properties.Resources.defaultt;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            listView_output.View = View.Details;
            listView_output.GridLines = true;
            listView_output.FullRowSelect = true;

            listView_output.Columns.Add("담당교수", 100, HorizontalAlignment.Center);
            listView_output.Columns.Add("학과", 130, HorizontalAlignment.Center);
            listView_output.Columns.Add("학년", 73, HorizontalAlignment.Center);
            listView_output.Columns.Add("과목명", 150, HorizontalAlignment.Center);
            listView_output.Columns.Add("이수구분", 100, HorizontalAlignment.Center);
            listView_output.Columns.Add("학점", 60, HorizontalAlignment.Center);
            listView_output.Columns.Add("강의시간", 330, HorizontalAlignment.Center);
            listView_output.Columns.Add("수강정원", 73, HorizontalAlignment.Center);
        }
        private void label1_Click(object sender, EventArgs e)
        {
            //b-1 구역 로그아웃 버튼
            MessageBox.Show("로그아웃", "알림");
            login_window login_Window = new login_window();
            this.Hide();
            login_Window.ShowDialog();
            this.Close();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!manualChange)
            {
                dateTimePicker1.Value = dateTimePicker1.Value.AddSeconds(0.5); // 현재 시간에 +1초
            }
            UpdateTime();
        }

        private void UpdateTime()
        {
            string formattedDateTime = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");
            DateTimeLabel.Text = formattedDateTime;
        }

        private void UpdateDayOfWeek()
        {
            string dayOfWeek = dateTimePicker1.Value.DayOfWeek.ToString();
            DayOfWeek.Text = dayOfWeek;
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            UpdateTime();
            UpdateDayOfWeek();
        }
        //개설버튼
        private void button_newclass2_Click(object sender, EventArgs e)
        {
            newclass classInstance = new newclass();
            if (!int.TryParse(textBox_score.Text, out int score) || !int.TryParse(textBox_personnel.Text, out int personnel))
            {
                MessageBox.Show("학점과 수강정원은 숫자로 입력해주세요.");
                return;
            }

            if (string.IsNullOrEmpty(textBox_subjectname.Text) || string.IsNullOrEmpty(textBox_personnel.Text) || string.IsNullOrEmpty(textBox_score.Text) ||
                GetSelectedRadioButtonText_Opened(groupBox_course) == "" || GetSelectedRadioButtonText_Opened(groupBox_grade) == "" ||
                GetCheckedListBoxSelectedItems_Opened(groupBox_classtime) == "")
            {
                MessageBox.Show("모든 항목을 입력 및 선택하세요.");
                return;
            }

            string department = currentprofessor.department;
            string grade = GetSelectedRadioButtonText_Opened(groupBox_grade);
            string subjectName = textBox_subjectname.Text;
            string credits = textBox_score.Text;
            string fixedNumber= textBox_personnel.Text;
            string course = GetSelectedRadioButtonText_Opened(groupBox_course);
            List<string> selectedDates = checkedListBox_date.CheckedItems.OfType<string>().ToList();
            List<string> selectedTimes = checkedListBox_time.CheckedItems.OfType<string>().ToList();
            string lectureTime = string.Join(", ", selectedDates.Concat(selectedTimes));
            string code = classInstance.GenerateSubjectCode(department, course); // 이 메서드는 과목 코드를 생성하는 방법에 따라 다릅니다.
            if (IsLectureAlreadyExists(code, subjectName))
            {
                MessageBox.Show("이미 같은 내용의 강의가 개설되어 있습니다.");
                return;
            }

            // 시간대 충돌 검사
            if (!IsTimeSlotAvailable(selectedDates, selectedTimes))
            {
                MessageBox.Show("선택하신 시간대에 이미 개설된 강의가 있습니다.");
                return;
            }

            // 데이터베이스에 강의 추가
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = @"INSERT INTO class (code, name, department, grade, subject_name, completion, credits, time, fixed_number)
                          VALUES (@Code, @Name, @Department, @Grade, @SubjectName, @Completion, @Credits, @Time, @FixedNumber)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Code", code);
                        command.Parameters.AddWithValue("@Name", currentprofessor.name);
                        command.Parameters.AddWithValue("@Department", department);
                        command.Parameters.AddWithValue("@Grade", grade);
                        command.Parameters.AddWithValue("@SubjectName", subjectName);
                        command.Parameters.AddWithValue("@Completion", course);
                        command.Parameters.AddWithValue("@Credits", credits);
                        command.Parameters.AddWithValue("@Time", lectureTime);
                        command.Parameters.AddWithValue("@FixedNumber", fixedNumber);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("강의가 성공적으로 추가되었습니다.");
                    LoadDataFromFile(); // 데이터베이스에서 데이터를 다시 불러오는 메서드입니다.
                    clearPanelFields(); // 입력 필드를 초기화하는 메서드입니다.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new class: {ex.Message}");
            }
        }
        private void button_delete_Click(object sender, EventArgs e)
        {
            if (listView_output.SelectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 항목을 선택하세요.");
                return;
            }

            // 사용자에게 삭제 확인 요청
            var confirmResult = MessageBox.Show("선택한 항목을 정말 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                DeleteSelectedItemsFromFile();
            }

        }
        private void DeleteSelectedItemsFromFile()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (ListViewItem item in listView_output.SelectedItems)
                    {
                        var subjectname = item.SubItems[3].Text; // ListView의 첫 번째 서브 아이템이 과목 코드임을 가정

                        var query = @"DELETE FROM class WHERE subject_name = @SubjectName";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@SubjectName", subjectname);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show($"{subjectname} 강의가 성공적으로 삭제되었습니다.");
                            }
                            else
                            {
                                MessageBox.Show($"{subjectname} 강의를 찾을 수 없습니다.");
                            }
                        }
                    }

                    // 데이터베이스에서 삭제 후 ListView 업데이트
                    LoadDataFromFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"강의 삭제 중 오류 발생: {ex.Message}");
            }
        }
        private bool IsTimeSlotAvailable(List<string> selectedDates, List<string> selectedTimes)
        {
            string grade = GetSelectedRadioButtonText_Opened(groupBox_grade);
            string course = GetSelectedRadioButtonText_Opened(groupBox_course);

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var day in selectedDates)
                    {
                        foreach (var time in selectedTimes)
                        {
                            // `name` 컬럼을 사용하여 현재 교수의 강의만 필터링합니다.
                            // 시간 문자열이 데이터베이스 내의 저장 형식과 정확히 일치해야 합니다.
                            // 예: "화요일, 10:00~10:50" 형태의 시간 문자열을 검사합니다.
                            var query = $@"SELECT COUNT(*) FROM class 
                                   WHERE name = '{currentprofessor.name}'
                                   AND time LIKE '%{day}%{time.Trim()}%'";

                            using (var cmd = new MySqlCommand(query, connection))
                            {
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                if (count > 0)
                                {
                                    return false; // 선택된 시간대 중 하나라도 기존 강의와 겹치면 false를 반환
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
                return false;
            }

            return true; // 모든 검사를 통과하면 겹치는 강의가 없음을 의미합니다.
        }

        private bool IsLectureAlreadyExists(string code, string subjectname)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = @"SELECT COUNT(*) FROM class 
                          WHERE subject_name = @SubjectName";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        // 파라미터 값 설정
                        cmd.Parameters.AddWithValue("@SubjectName", subjectname);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0; // 같은 과목 코드와 과목 이름이 이미 존재하면 true 반환
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
                // 예외가 발생한 경우, 보통의 애플리케이션에서는 로깅을 하고 false를 반환하거나 사용자에게 알립니다.
                return false;
            }
        }
        private void LoadDataFromFile()
        {
            listView_output.Items.Clear();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = $@"SELECT code, name, department, grade, subject_name, completion, credits, time, fixed_number 
                           FROM class
                           WHERE name = '{currentprofessor.name}';";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ListViewItem(reader["name"].ToString()); // 과목 코드
                                item.SubItems.Add(reader["department"].ToString()); // 교수 이름
                                item.SubItems.Add(reader["grade"].ToString()); // 학과
                                item.SubItems.Add(reader["subject_name"].ToString()); // 과목 이름
                                item.SubItems.Add(reader["completion"].ToString()); // 이수 구분
                                item.SubItems.Add(reader["credits"].ToString()); // 학점
                                item.SubItems.Add(reader["time"].ToString()); // 시간
                                item.SubItems.Add(reader["fixed_number"].ToString()); // 정원

                                listView_output.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터베이스에서 데이터 로딩 중 오류 발생: {ex.Message}");
            }
        }
        private void clearPanelFields()
        {
            textBox_subjectname.Text = "";
            textBox_personnel.Text = "";
            textBox_score.Text = "";

            foreach (Control control in groupBox_course.Controls)
            {
                if (control is RadioButton radioButton)
                {
                    radioButton.Checked = false;
                }
            }

            foreach (Control control in groupBox_grade.Controls)
            {
                if (control is RadioButton radioButton)
                {
                    radioButton.Checked = false;
                }
            }

            foreach (Control control in groupBox_classtime.Controls)
            {
                if (control is CheckedListBox checkedListBox)
                {
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                    {
                        checkedListBox.SetItemChecked(i, false);
                    }
                }
            }
        }
        


        // groupBox 내에서 선택된 라디오버튼의 텍스트를 가져오는 메서드
        private string GetSelectedRadioButtonText_Opened(GroupBox groupBox)
        {
            foreach (Control control in groupBox.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    return radioButton.Text;
                }
            }
            return ""; // 선택된 라디오버튼이 없을 경우 빈 문자열 반환
        }

        // groupBox 내에서 선택된 체크리스트박스 항목들을 가져오는 메서드
        private string GetCheckedListBoxSelectedItems_Opened(GroupBox groupBox)
        {
            List<string> selectedItems = new List<string>();
            foreach (Control control in groupBox.Controls)
            {
                if (control is CheckedListBox checkedListBox)
                {
                    foreach (object item in checkedListBox.CheckedItems)
                    {
                        selectedItems.Add(item.ToString());
                    }
                }
            }
            return string.Join(", ", selectedItems); // 선택된 항목들을 쉼표로 구분하여 문자열로 반환
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SetSelectedRadioButtonText(groupBox_grade, listView_output.FocusedItem.SubItems[2].Text);
                textBox_subjectname.Text = listView_output.FocusedItem.SubItems[3].Text;
                SetSelectedRadioButtonText(groupBox_course, listView_output.FocusedItem.SubItems[4].Text);
                textBox_score.Text = listView_output.FocusedItem.SubItems[5].Text;
                SetCheckedListBoxSelectedItems(groupBox_classtime, listView_output.FocusedItem.SubItems[6].Text);
                textBox_personnel.Text = listView_output.FocusedItem.SubItems[7].Text;
            }
            catch { }
        }

        // groupBox 내에서 특정 텍스트를 가진 라디오버튼을 선택하는 메서드
        private void SetSelectedRadioButtonText(GroupBox groupBox, string text)
        {
            foreach (Control control in groupBox.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Text == text)
                {
                    radioButton.Checked = true;
                    return;
                }
            }
        }
        private void SetCheckedListBoxSelectedItems(GroupBox groupBox, string selectedItems)
        {
            List<string> selectedItemList = selectedItems.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();
            foreach (Control control in groupBox.Controls)
            {
                if (control is CheckedListBox checkedListBox)
                {
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                    {
                        checkedListBox.SetItemChecked(i, selectedItemList.Contains(checkedListBox.Items[i].ToString()));
                    }
                }
            }
        }

        private void button_professor_gradeinput_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void button_professor_stu_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            menti();
        }

        private void button_professor_personal_Click(object sender, EventArgs e)
        {
           

            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            DisplayProfessorPhoto(currentprofessor.ID);
            professor_info_name_box.Text = currentprofessor.name;
            DisplayProfessorPhoto(currentprofessor.ID);
            email_update(currentprofessor.e_mail);
            number_update(currentprofessor.phone_number);
        }

        private void button_professor_salary_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void button_professor_lecture_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void button_professor_schedule_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            tablePanel_timetable_show();
        }

        private void button_professor_class_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void button_professor_newclass_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            LoadDataFromFile();


        }

        private void button_professor_meeting_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            get_file_gridview_output();
        }
        private void dataGridView_submit_stu_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            dataGridView_submit_stu.Rows[e.RowIndex].Cells[0].Value = (e.RowIndex + 1).ToString();
        }
        private void menti_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            menti_table.Rows[e.RowIndex].Cells[0].Value = (e.RowIndex + 1).ToString();
        }
        private void get_file_gridview_output()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT name, ID, advisor, meeting_type, meeting_topic, first_date, meeting_state " +
                        "FROM consulting WHERE pro_name = @pro_name;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@pro_name", currentprofessor.name);

                        using (var reader = cmd.ExecuteReader())
                        {
                            dataGridView_submit_stu.Rows.Clear();

                            while (reader.Read())
                            {
                                string name = reader["name"].ToString();
                                string ID = reader["ID"].ToString();
                                string advisor = reader["advisor"].ToString();
                                string meeting_type = reader["meeting_type"].ToString();
                                string meeting_topic = reader["meeting_topic"].ToString();
                                string first_date = reader["first_date"].ToString();
                                string meeting_state = reader["meeting_state"].ToString();

                                if (advisor == "일반교수")
                                {
                                    string display_Stu_Type = "일반학생";
                                    dataGridView_submit_stu.Rows.Add(' ', name, ID, display_Stu_Type, meeting_type, meeting_topic, first_date, meeting_state);
                                }
                                else if (advisor == "지도교수")
                                {
                                    string display_Stu_Type = "담당학생";
                                    dataGridView_submit_stu.Rows.Add(' ', name, ID, display_Stu_Type, meeting_type, meeting_topic, first_date, meeting_state);
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }
        }
        private void button_professor_main_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            display_professor_info();
        }

        private void radioButton_side_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_side.Checked)
            {
                radioButton_first.Enabled = false;
                radioButton_second.Enabled = false;
                radioButton_third.Enabled = false;
                radioButton_fourth.Enabled = false;
                radioButton_total.Checked = true;
            }
            else if (!radioButton_side.Checked)
            {
                radioButton_first.Enabled = true;
                radioButton_second.Enabled = true;
                radioButton_third.Enabled = true;
                radioButton_fourth.Enabled = true;
            }
        }

        private void radioButton_sideselect_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_sideselect.Checked)
            {
                radioButton_first.Enabled = false;
                radioButton_second.Enabled = false;
                radioButton_third.Enabled = false;
                radioButton_fourth.Enabled = false;
                radioButton_total.Checked = true;
            }
            else if (!radioButton_sideselect.Checked)
            {
                radioButton_first.Enabled = true;
                radioButton_second.Enabled = true;
                radioButton_third.Enabled = true;
                radioButton_fourth.Enabled = true;
            }
        }
        private void dataGridView_submit_stu_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridView_submit_stu.CurrentCell.ColumnIndex == 8)
            {
                System.Windows.Forms.ComboBox combo = e.Control as System.Windows.Forms.ComboBox;

                if (combo != null)
                {
                    // Tag 속성을 사용하여 현재 행 인덱스 저장
                    combo.Tag = dataGridView_submit_stu.CurrentCell.RowIndex;

                    // 기존 이벤트 핸들러를 제거하여 이벤트가 중복으로 등록되지 않도록
                    combo.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                    // 새 이벤트 핸들러 추가
                    combo.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                }
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.ComboBox combo && combo.Tag is int rowIndex)
            {
                // ComboBox의 선택된 값을 가져옵니다.
                string selectedValue = combo.SelectedItem.ToString();

                // 선택된 값에 따라 cells[7]의 값을 변경합니다.
                string newValue = selectedValue == "O" ? "상담 전(승인)" : "상담 취소";
                dataGridView_submit_stu.Rows[rowIndex].Cells[7].Value = newValue;

                ComboBox_UpdateFile(rowIndex, newValue);


                UpdateProfessorConsultationDetails(rowIndex);
            }
        }
        private void UpdateProfessorConsultationDetails(int rowIndex) //콤보박스 내용 바꾸면 바로 업데이트되게하는 함수
        {
            professor_consult_name.Text = string.Empty;
            professor_consult_number.Text = string.Empty;
            professor_consult_type.Text = string.Empty;
            professor_consult_meeting_type.Text = string.Empty;
            professor_consult_title.Text = string.Empty;
            professor_consult_date.Text = string.Empty;
            professor_consult_condition.Text = string.Empty;
            professor_consult_titlebox.Text = string.Empty;
            professor_consult_detail.Text = string.Empty;
            textBox1professor_apply_detail.Text = string.Empty; // 추가된 내용
            // rowIndex를 사용하여 dataGridView_submit_stu에서 데이터를 가져옵니다.
            DataGridViewRow dr = dataGridView_submit_stu.Rows[rowIndex];
            professor_consult_name.Text = dr.Cells[1].Value.ToString();
            professor_consult_number.Text = dr.Cells[2].Value.ToString();
            professor_consult_type.Text = dr.Cells[3].Value.ToString();
            professor_consult_meeting_type.Text = dr.Cells[4].Value.ToString();
            professor_consult_title.Text = dr.Cells[5].Value.ToString();
            professor_consult_date.Text = dr.Cells[6].Value.ToString();
            professor_consult_condition.Text = dr.Cells[7].Value.ToString();

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM consulting WHERE ID = @ID AND pro_name = @pro_name AND first_date = @first_date;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", professor_consult_number.Text);
                        cmd.Parameters.AddWithValue("@pro_name", currentprofessor.name);
                        cmd.Parameters.AddWithValue("@first_date", professor_consult_date.Text);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                professor_consult_titlebox.Text = reader["title"].ToString();
                                professor_consult_detail.Text = reader["detail"].ToString();

                                if (reader["meeting_state"].ToString() == "상담 취소")
                                {
                                    textBox1professor_apply_detail.ReadOnly = true;
                                    button_save12.Visible = false;
                                }

                                else
                                {
                                    textBox1professor_apply_detail.ReadOnly = false;
                                    button_save12.Visible = true;
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }
        }
        private void ComboBox_UpdateFile(int rowIndex, string newValue)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    DataGridViewRow dr = dataGridView_submit_stu.Rows[rowIndex];
                    professor_consult_name.Text = dr.Cells[1].Value.ToString();
                    conn.Open();
                    string query = "UPDATE consulting SET meeting_state = @newValue WHERE name = @name AND pro_name = @pro_name;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", professor_consult_name.Text);
                        cmd.Parameters.AddWithValue("@newValue", newValue);
                        cmd.Parameters.AddWithValue("@pro_name", currentprofessor.name);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }
        }
        private void dataGridView_submit_stu_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView_submit_stu.SelectedRows.Count > 0)
            {
                DataGridViewRow dr = dataGridView_submit_stu.SelectedRows[0];
                UpdateProfessorConsultationDetailsFromSelectedRow(dr);
            }
        }
        private void UpdateProfessorConsultationDetailsFromSelectedRow(DataGridViewRow dr)
        {
            professor_consult_name.Text = dr.Cells[1].Value.ToString();
            professor_consult_number.Text = dr.Cells[2].Value.ToString();
            professor_consult_type.Text = dr.Cells[3].Value.ToString();
            professor_consult_meeting_type.Text = dr.Cells[4].Value.ToString();
            professor_consult_title.Text = dr.Cells[5].Value.ToString();
            professor_consult_date.Text = dr.Cells[6].Value.ToString();
            professor_consult_condition.Text = dr.Cells[7].Value.ToString();
            // 다른 필드들도 마찬가지로 업데이트...
            // 파일에서 추가 정보를 찾아 갱신하는 로직 포함
            string filePath = "consulting.txt";
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (dr.Cells[2].Value.ToString() == parts[2] && currentprofessor.name == parts[1] && dr.Cells[6].Value.ToString() == parts[10])
                {//학번 현재교수같은지비교
                    professor_consult_titlebox.Text = parts[7];
                    professor_consult_detail.Text = parts[8];
                    if (parts[12] == "상담 취소")
                    {
                        textBox1professor_apply_detail.ReadOnly = true;
                        button_save12.Visible = false;
                    }
                    else if (parts[12] == "상담 완료")
                    {
                        textBox1professor_apply_detail.Text = parts[13];
                        textBox1professor_apply_detail.ReadOnly = true;
                        button_save12.Visible = false;
                        int rowIndex = dataGridView_submit_stu.SelectedRows[0].Index;
                        dataGridView_submit_stu.Rows[rowIndex].Cells[8].ReadOnly = true;
                    }
                    else
                    {
                        textBox1professor_apply_detail.ReadOnly = false;
                        button_save12.Visible = true;
                    }
                }
            }
        }
        private void ComboBox_UpdateFile_2(int rowIndex, string newValue)
        {
            string filePath = "consulting.txt";
            var lines = File.ReadAllLines(filePath).ToList();

            if (rowIndex < lines.Count)
            {
                var cells = lines[rowIndex].Split(',');

                if (cells[1] == currentprofessor.name)
                {
                    // 12번째 요소를 "상담 완료"로 설정

                    // 13번째 요소에 textBox1professor_apply_detail.Text 내용 설정
                    if (string.IsNullOrWhiteSpace(textBox1professor_apply_detail.Text))
                    {
                        // 텍스트 내용이 비어 있으면 메시지를 표시합니다.
                        MessageBox.Show("상담 내용을 입력하지 않았습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // 메서드 실행을 여기서 중단합니다.
                    }
                    cells[12] = newValue;
                    cells[13] = textBox1professor_apply_detail.Text;

                    lines[rowIndex] = string.Join(",", cells);
                }

                File.WriteAllLines(filePath, lines);
            }
        }
        private void button_save12_Click(object sender, EventArgs e)
        {
            DataGridViewRow dr = dataGridView_submit_stu.SelectedRows[0];
            if (dataGridView_submit_stu.SelectedRows.Count > 0)
            {
                int rowIndex = dataGridView_submit_stu.SelectedRows[0].Index;

                // "상담 완료"로 상태 변경
                string newValue = "상담 완료";
                dataGridView_submit_stu.Rows[rowIndex].Cells[7].Value = newValue;

                // 파일 내용도 업데이트
                ComboBox_UpdateFile_2(rowIndex, newValue);

                UpdateProfessorConsultationDetailsFromSelectedRow(dr);
            }
        }

        private void tablePanel_timetable_show()
        {
            // 요일에 해당하는 코드로의 매핑
            var dayCodeMapping = new Dictionary<string, string>
    {
        {"월요일", "mon"},
        {"화요일", "tues"},
        {"수요일", "wed"},
        {"목요일", "thurs"},
        {"금요일", "fri"},
    };

            // 시간에 해당하는 row 인덱스 매핑
            var timeRowIndex = new Dictionary<string, int>
    {
        { "09:00~09:50", 1 },
        { "10:00~10:50", 2 },
        { "11:00~11:50", 3 },
        { "12:00~12:50", 4 },
        { "13:00~13:50", 5 },
        { "14:00~14:50", 6 },
        { "15:00~15:50", 7 },
        { "16:00~16:50", 8 },
        { "17:00~17:50", 9 },
    };

            TableLayoutPanel tablePanel_timetable = this.Controls.Find("tablePanel_timetable", true).FirstOrDefault() as TableLayoutPanel;
            if (tablePanel_timetable == null)
            {
                MessageBox.Show("TableLayoutPanel을 찾을 수 없습니다.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM class WHERE name = @ProfessorName";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProfessorName", currentprofessor.name);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var day = reader["time"].ToString().Split(',')[0];
                                var times = reader["time"].ToString().Split(',').Skip(1).ToArray();
                                var subject = reader["subject_name"].ToString();
                                var grade = reader["grade"].ToString();

                                if (dayCodeMapping.TryGetValue(day, out string dayCode))
                                {
                                    foreach (var time in times)
                                    {
                                        if (timeRowIndex.TryGetValue(time.Trim(), out int rowIndex))
                                        {
                                            string panelName = $"{dayCode}{rowIndex}";
                                            var panel = tablePanel_timetable.Controls.Find(panelName, true).FirstOrDefault() as Panel;
                                            if (panel != null)
                                            {
                                                panel.BackColor = grade_color(grade); // Example color, adjust as needed
                                                panel.Controls.Clear();
                                                panel.Controls.Add(new Label { Text = $"{subject}/{grade}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
                                            }
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
                MessageBox.Show($"Error loading timetable: {ex.Message}");
            }
        }
        private Color grade_color(string grade)
        {
            Color cr = Color.LightSlateGray; //기본값 설정

            switch (grade)
            {
                case "1학년":
                    cr = Color.LightGreen;
                    break;
                case "2학년":
                    cr = Color.LightPink;
                    break;
                case "3학년":
                    cr = Color.LightSalmon;
                    break;
                case "4학년":
                    cr = Color.LightSkyBlue;
                    break;
            }

            return cr;
        }

        private void menti()
        {
            menti_table.Rows.Clear();
            string path = "student.txt";
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (parts[12] == currentprofessor.name)
                {
                    menti_table.Rows.Add(' ', parts[14], parts[7], parts[0]);
                }
            }

        }

        private void DisplayProfessorPhoto(string userId)
        {
            conn = new MySqlConnection(connectionString);
            string photoFileName = ""; // 사진 파일 이름 초기화

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Photo FROM pro_personal WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", userId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                photoFileName = reader["Photo"].ToString(); //사진 파일 이름 가져오기
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(photoFileName))
            {
                string filePath = Path.Combine(Application.StartupPath, "profile", photoFileName);
                if (File.Exists(filePath))
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var img = Image.FromStream(fs);
                        professor_picture.Image?.Dispose(); // 기존 이미지 리소스 해제
                        professor_picture.Image = img;
                    }
                }
                else
                {
                    professor_picture.Image?.Dispose(); // 기존 이미지 리소스 해제
                    professor_picture.Image = erp_2.Properties.Resources.defaultt; // 기본 이미지 설정
                }
            }
            else
            {
                professor_picture.Image?.Dispose(); // 기존 이미지 리소스 해제
                professor_picture.Image = erp_2.Properties.Resources.defaultt; // 기본 이미지 설정
            }
        }
        private void info_save_Click(object sender, EventArgs e)
        {
            bool isPhoneSaved = save_phone_number();
            bool isEmailSaved = save_email();
            bool isPasswordChanged = true; // 초기값은 true로 설정, 비밀번호 변경이 요구되지 않았거나 성공적으로 변경되었다고 가정

            if (!(string.IsNullOrEmpty(previous_pw.Text) && string.IsNullOrEmpty(change_pw.Text) && string.IsNullOrEmpty(change_pw2.Text)))
            {
                isPasswordChanged = save_pw(); // 이 함수가 bool을 반환하도록 수정 필요
            }

            // 모든 작업이 성공적으로 완료되었는지 확인
            if (isPhoneSaved && isEmailSaved && isPasswordChanged)
            {
                MessageBox.Show("저장되었습니다.", "저장 완료");
            }
        }
        private bool save_email()
        {
            if (string.IsNullOrEmpty(email_id.Text) || string.IsNullOrEmpty(email_text.Text))
            {
                MessageBox.Show("이메일 ID와 도메인을 모두 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string email = email_id.Text + "@" + email_text.Text;
            if (email == currentprofessor.e_mail) return true;

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE pro_personal SET e_mail = @Email WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentprofessor.ID);
                        cmd.Parameters.AddWithValue("@Email", email);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentprofessor.e_mail = email;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }
            return false;
        }
        private bool save_phone_number()
        {
            if (string.IsNullOrEmpty(number_list.Text) || string.IsNullOrEmpty(middle_number.Text) || string.IsNullOrEmpty(last_number.Text))
            {
                MessageBox.Show("핸드폰 번호를 모두 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // 입력 실패 반환
            }
            if (middle_number.Text.Length != 4 || last_number.Text.Length != 4)
            {
                MessageBox.Show("핸드폰 번호를 4자리 모두 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // 입력 실패 반환
            }
            string number = number_list.Text + '-' + middle_number.Text + '-' + last_number.Text;
            if (number == currentprofessor.phone_number) return true; // 변경사항 없으면 성공 반환

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE pro_personal SET phone_number = @phone_number WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentprofessor.ID);
                        cmd.Parameters.AddWithValue("@phone_number", number);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentprofessor.phone_number = number;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
                return false;
            }
        }
        private bool save_pw()
        {
            if (string.IsNullOrEmpty(previous_pw.Text) || string.IsNullOrEmpty(change_pw.Text) || string.IsNullOrEmpty(change_pw2.Text))
            {
                MessageBox.Show("비밀번호를 모두 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // 입력 실패 반환
            }

            if (previous_pw.Text != currentprofessor.Password)
            {
                MessageBox.Show("현재 비밀번호가 일치하지 않습니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // 현재 비밀번호 불일치
            }

            if (change_pw.Text != change_pw2.Text)
            {
                MessageBox.Show("새 비밀번호가 일치하지 않습니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // 새 비밀번호 불일치
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE pro_personal SET Password = @NewPassword WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentprofessor.ID);
                        cmd.Parameters.AddWithValue("@OldPassword", previous_pw.Text);
                        cmd.Parameters.AddWithValue("@NewPassword", change_pw.Text);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentprofessor.Password = change_pw.Text;
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("비밀번호 변경 실패. 현재 비밀번호를 확인해주세요.", "비밀번호 변경 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }

                return false;
            }
        }
        private void picture_add_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "이미지 파일 (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fullPath = openFileDialog.FileName;
                string fileName = Path.GetFileName(fullPath);
                string destPath = Path.Combine(Application.StartupPath, "profile", fileName);

                // 파일을 profile 폴더로 복사
                File.Copy(fullPath, destPath, true);

                // student.txt에 사진 파일 이름(또는 경로) 업데이트
                SavePhotoExtension(fileName);

                // PictureBox에 사진 표시
                DisplayProfessorPhoto(currentprofessor.ID);
            }
        }
        private void email_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            string a = email_list.SelectedItem.ToString();
            if (a != "직접 입력")
            {
                email_text.Text = email_list.SelectedItem.ToString();
            }
        }
        private void SavePhotoExtension(string photoExtension)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE pro_personal SET Photo = @Photo WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentprofessor.ID);
                        cmd.Parameters.AddWithValue("@Photo", photoExtension);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentprofessor.Photo = photoExtension;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }
        }
        private void picture_del_Click(object sender, EventArgs e)
        {
            professor_picture.Image = erp_2.Properties.Resources.defaultt;
            conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();
                string query = "UPDATE pro_personal SET Photo = '-' WHERE ID = @ID;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", currentprofessor.ID);
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0) // 성공적으로 업데이트 되었다면
                    {
                        string photoPath = Path.Combine(Application.StartupPath, "profile", currentprofessor.Photo);
                        if (File.Exists(photoPath))
                        {
                            File.Delete(photoPath); // 기존 사진 파일 삭제
                        }
                        currentprofessor.Photo = "-"; // 현재 학생 객체의 사진 정보 업데이트
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void middle_number_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리             
            {
                e.Handled = true;
            }
            if (middle_number.TextLength >= 4 && e.KeyChar != Convert.ToChar(Keys.Back))
            {
                e.Handled = true;
            }
        }

        private void last_number_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리             
            {
                e.Handled = true;
            }
            if (last_number.TextLength >= 4 && e.KeyChar != Convert.ToChar(Keys.Back))
            {
                e.Handled = true;
            }
        }
    }
}