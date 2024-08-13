using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using universal2;

namespace erp_2
{

    public partial class student_main : Form
    {
        private bool manualChange = false;
        public Student currentstudent;
        private Dictionary<string, Button> buttonDict = new Dictionary<string, Button>();
        public string consult_a;
        private MySqlConnection conn;
        private static string server = "192.168.31.147";
        private static string database = "team3";
        private static string uid = "root";
        private static string Password_1 = "0000";
        string connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={Password_1};";
        public student_main(Student student)
        {
            InitializeComponent();

            currentstudent = student;
            CalculateAverageGrade(currentstudent.score);
            display_student_info();
            update_top("student_main", "메인화면");
            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;
            timer1.Start();
        }


        private void update_top(string baseName, string buttonText)
        {
            string buttonName = "button_" + baseName; //버튼뒤에 이름을 basename으로
            string buttonXName = buttonName + "_x"; //x버튼 이름정하기
            string panelName = "panel_" + baseName; //내가뜨게할 패널이름 basename으로똑같이정해놓기
            // 중복 생성 방지
            if (buttonDict.ContainsKey(buttonName))
            {
                // 해당하는 패널 찾기 및 맨 앞으로 가져오기
                Control[] targetPanels = this.Controls.Find(panelName, true); //해당하는 패널 찾기
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
            Button lastButton = buttonDict.Values.LastOrDefault();
            int nextXPosition = lastButton != null ? lastButton.Right + 30 : 10; // 마지막 버튼 다음 위치

            // 새로운 버튼 생성
            Button newButton = new Button //여기서 딕셔너리에 넣음
            {
                Name = buttonName,
                Text = buttonText,
                Location = new Point(nextXPosition, 5),
                AutoSize = true
            };
            panel_top.Controls.Add(newButton);
            buttonDict[buttonName] = newButton;

            newButton.Click += (sender, e) => //탭이름버튼에 이벤트추가하기
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
            Button newXButton = new Button
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
                rearrange_buttons(); //x버튼에 이벤트추가
            };
            panel_top.Controls.Add(newXButton);
            rearrange_buttons(); // 버튼 재정렬
        }

        private void rearrange_buttons() //버튼 추가되거나 제거될때 버튼정렬
        {
            int xPosition = 10; // 시작 위치
            foreach (Button btn in panel_top.Controls.OfType<Button>().Where(b => !b.Name.EndsWith("_x")).OrderBy(b => b.Location.X))
            //패널탑내에 모든컨트롤 가져와서 버튼만 포함시키고 _x로 끝나지않는 애들만 가져오기 걔네가 b임 b들을 설치되었던 x좌표 순서대로 왼족부터 처리되게함
            {
                btn.Location = new Point(xPosition, btn.Location.Y); 
                //있던 버튼들을 x좌표는 시작위치정해놓은데서시작 y좌표는 그대로
                Button xBtn = panel_top.Controls.OfType<Button>().FirstOrDefault(b => b.Name == btn.Name + "_x");
                //닫기버튼찾고 가장첫번째 버튼 반환
                if (xBtn != null) //닫기버튼이 있다면
                {
                    xBtn.Location = new Point(btn.Right, btn.Location.Y); //현재버튼의 오른쪽끝에 놓기
                    xPosition = xBtn.Right + 10; // 다음 버튼 위치 갱신
                }
            }
        }


        private void UpdateLayout()
        {
            int yOffset = button_general.Height; // button_general 아래로의 초기 오프셋
            Point baseLocation = button_general.Location; // 기준 위치 설정

            if (general_menu.Visible) //일반메뉴가 보인다면?
            {
                general_menu.Location = new Point(baseLocation.X, baseLocation.Y + yOffset); 
                //일반버튼X,일반버튼Y+일반버튼높이 -> 일반버튼아래에 메뉴생성하겠다
                yOffset += general_menu.Height; // general_menu가 표시되면 yOffset에 일반메뉴패널의 높이를 추가
            }

            // button_record 위치 업데이트 if문통과했으면 늘어난 yoffset에 맞춰서 나올테고 아니면 그대로있을테고
            button_record.Location = new Point(baseLocation.X, baseLocation.Y + yOffset);

            // 학적버튼을 눌렀을때 나올 패널이 학적버튼 높이만큼 아래에 나와야함
            yOffset += button_record.Height;

            // record_menu의 가시성에 따른 조정
            if (record_menu.Visible)
            {
                record_menu.Location = new Point(baseLocation.X, button_record.Location.Y + button_record.Height);
                yOffset += record_menu.Height; // record_menu가 표시되면 yOffset에 그 높이를 추가
            }

            // button_class 위치 업데이트, record_menu 가시성에 상관없이 yOffset을 기준으로 설정
            button_class.Location = new Point(baseLocation.X, baseLocation.Y + yOffset);

            // yOffset을 button_class의 높이만큼 늘립니다. class_menu가 보이지 않을 때도 적용됩니다.
            yOffset += button_class.Height;

            // class_menu 가시성에 따른 위치 조정
            if (class_menu.Visible)
            {
                class_menu.Location = new Point(baseLocation.X, button_class.Location.Y + button_class.Height);
                // yOffset += class_menu.Height; // class_menu의 추가적인 높이를 고려할 필요가 있다면 여기에 추가
            }
        }

        private void button_general_Click_1(object sender, EventArgs e)
        {
            general_menu.Visible = !general_menu.Visible;
            UpdateLayout();

        }

        private void button_record_Click_1(object sender, EventArgs e)
        {
            record_menu.Visible = !record_menu.Visible;
            UpdateLayout();
        }

        private void button_class_Click(object sender, EventArgs e)
        {
            class_menu.Visible = !class_menu.Visible;
            UpdateLayout();
        }

        private void display_student_info()
        {
            student_name_box.Text = currentstudent.name;
            student_id_box.Text = currentstudent.ID;
            student_major_box.Text = currentstudent.department;
            student_email_box.Text = currentstudent.e_mail;
            student_number_box.Text = currentstudent.phone_number;
            main_photo(currentstudent.Photo);
        }

        private void student_personal_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            DisplayStudentPhoto(currentstudent.ID);
            student_info_name_box.Text = currentstudent.name;
            DisplayStudentPhoto(currentstudent.ID);
            email_update(currentstudent.e_mail);
            number_update(currentstudent.phone_number);
        }

        private void button_student_main_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            display_student_info();
        }
        private void boy()
        {
            btn_1.Visible = true;
            btn_3.Visible = true;
            btn_5.Visible = true;
            btn_7.Visible = true;
        }
        private void girl()
        {
            btn_2.Visible = true;
            btn_4.Visible = true;
            btn_6.Visible = true;
            btn_8.Visible = true;
        }
        private void button_dor_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            if (currentstudent.dormitory == "O")
            {
                if (currentstudent.livingroom == "용주학사(남자)")
                {
                    boy();
                    button_revise1.Visible = true;
                    button_revise1.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "용주학사(여자)")
                {
                    girl();
                    button_revise2.Visible = true;
                    button_revise2.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "챌린지하우스1인실(남자)")
                {
                    boy();
                    button_revise3.Visible = true;
                    button_revise3.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "챌린지하우스1인실(여자)")
                {
                    girl();
                    button_revise4.Visible = true;
                    button_revise4.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "챌린지하우스2인실(남자)")
                {
                    boy();
                    button_revise5.Visible = true;
                    button_revise5.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "챌린지하우스2인실(여자)")
                {
                    girl();
                    button_revise6.Visible = true;
                    button_revise6.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "도솔학사(남자)")
                {
                    boy();
                    button_revise7.Visible = true;
                    button_revise7.Size = new Size(71, 33);
                    count = 2;
                }
                else if (currentstudent.livingroom == "도솔학사(여자)")
                {
                    girl();
                    button_revise8.Visible = true;
                    button_revise8.Size = new Size(71, 33);
                    count = 2;
                }
            }
            else if (currentstudent.dormitory == "X")
            {
                if (currentstudent.gender == "남")
                {
                    boy();
                }
                else if (currentstudent.gender == "여")
                {
                    girl();
                }
            }
            if (currentstudent.roommate == "O")
            {
                tablelayout_present_roommate.Visible = true;
                roommate_count = 1;
            }
        }

        private void button_apply_money_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void button_scholar_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void button_graduation_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
        }

        private void student_info_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);

        }
        private void button_student_enrolment_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
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
                DisplayStudentPhoto(currentstudent.ID);
            }
        }
        private void SavePhotoExtension(string photoExtension)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE stu_personal SET Photo = @Photo WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        cmd.Parameters.AddWithValue("@Photo", photoExtension);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentstudent.Photo = photoExtension;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }
            }
        }
        private void DisplayStudentPhoto(string userId)
        {
            conn = new MySqlConnection(connectionString);
            string photoFileName = ""; // 사진 파일 이름 초기화

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Photo FROM stu_personal WHERE ID = @ID;";

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
                        student_picture.Image?.Dispose(); // 기존 이미지 리소스 해제
                        student_picture.Image = img;
                    }
                }
                else
                {
                    student_picture.Image?.Dispose(); // 기존 이미지 리소스 해제
                    student_picture.Image = Properties.Resources.defaultt; // 기본 이미지 설정
                }
            }
            else
            {
                student_picture.Image?.Dispose(); // 기존 이미지 리소스 해제
                student_picture.Image = Properties.Resources.defaultt; // 기본 이미지 설정
            }
        }
    
        private void main_photo(string photoFileName)
        {
            string filePath = Path.Combine(Application.StartupPath, "profile", photoFileName);
            if (File.Exists(filePath))
            {
                student_main_profile.Image = Image.FromFile(filePath);
            }
            else
            {
                // default.jpg 표시 또는 다른 처리
                student_main_profile.Image = Properties.Resources.defaultt;
            }
        }
        private void picture_add_Click_1(object sender, EventArgs e)
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
                DisplayStudentPhoto(currentstudent.ID);
            }
        }
        private void picture_del_Click(object sender, EventArgs e)
        {
            conn = new MySqlConnection(connectionString);
            student_picture.Image = Properties.Resources.defaultt;

            try
            {
                conn.Open();
                string query = "UPDATE stu_personal SET Photo = '-' WHERE ID = @ID;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0) // 성공적으로 업데이트 되었다면
                    {
                        string photoPath = Path.Combine(Application.StartupPath, "profile", currentstudent.Photo);
                        if (File.Exists(photoPath))
                        {
                            File.Delete(photoPath); // 기존 사진 파일 삭제
                        }
                        currentstudent.Photo = "-"; // 현재 학생 객체의 사진 정보 업데이트
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

        private void email_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            string a = email_list.SelectedItem.ToString();
            if (a != "직접 입력")
            {
                email_text.Text = email_list.SelectedItem.ToString();
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
            if (email == currentstudent.e_mail) return true;

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE stu_personal SET e_mail = @Email WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        cmd.Parameters.AddWithValue("@Email", email);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentstudent.e_mail = email;
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
                return false;
            }
            if (middle_number.Text.Length != 4 || last_number.Text.Length != 4)
            {
                MessageBox.Show("핸드폰 번호를 4자리 모두 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // 입력 실패 반환
            }
            string number = number_list.Text + '-' + middle_number.Text + '-' + last_number.Text;
            if (number == currentstudent.phone_number) return true;

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE stu_personal SET phone_number = @phone_number WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        cmd.Parameters.AddWithValue("@phone_number", number);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentstudent.phone_number = number;
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
        private bool save_pw()
        {
            if (string.IsNullOrEmpty(previous_pw.Text) || string.IsNullOrEmpty(change_pw.Text) || string.IsNullOrEmpty(change_pw2.Text))
            {
                MessageBox.Show("비밀번호를 모두 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (previous_pw.Text != currentstudent.Password)
            {
                MessageBox.Show("현재 비밀번호가 일치하지 않습니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (change_pw.Text != change_pw2.Text)
            {
                MessageBox.Show("새 비밀번호가 일치하지 않습니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE stu_personal SET Password = @NewPassword WHERE ID = @ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        cmd.Parameters.AddWithValue("@OldPassword", previous_pw.Text);
                        cmd.Parameters.AddWithValue("@NewPassword", change_pw.Text);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            currentstudent.Password = change_pw.Text;
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

        private void button_student_fill_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            consult_name.Text = currentstudent.name;
            consult_studentid.Text = currentstudent.ID;
            consult_number.Text = currentstudent.phone_number;
            consult_email.Text = currentstudent.e_mail;

        }
        private bool IsAnyCheckBoxChecked()
        {
            // tableLayoutPanel4 안의 모든 체크박스를 순회하며 체크된 것이 있는지 확인
            foreach (Control control in tableLayoutPanel4.Controls)
            {
                if (control is System.Windows.Forms.CheckBox checkBox && checkBox.Checked)
                {
                    return true; // 적어도 하나의 체크박스가 체크되었다면 true 반환
                }
            }
            return false; // 체크된 체크박스가 없다면 false 반환
        }
        private void consult_apply_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(consult_name.Text) ||
                string.IsNullOrWhiteSpace(consult_professorname.Text) ||
                string.IsNullOrWhiteSpace(consult_number.Text) ||
                string.IsNullOrWhiteSpace(consult_email.Text) ||
                string.IsNullOrWhiteSpace(consult_professor.Text) ||
                string.IsNullOrWhiteSpace(consult_meetingtype.Text) ||
                string.IsNullOrWhiteSpace(consult_title.Text) ||
                string.IsNullOrWhiteSpace(apply_detail.Text) ||
                !IsAnyCheckBoxChecked())
            {
                MessageBox.Show("모든 내용을 입력해주세요.", "상담신청", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 시작 및 종료 시간 설정
            string start_time = consult_meetingtype.Text == "방문" ? consult_time_first.Value.ToString("M월 d일 H시 m분") : "-";
            string end_time = consult_meetingtype.Text == "방문" ? consult_time_second.Value.ToString("M월 d일 H시 m분") : "-";

            string apply = apply_detail.Text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");


            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var query = @"
                INSERT INTO consulting
                (name, pro_name, ID, phone_number, e_mail, advisor, meeting_type, title, detail, meeting_topic, first_date, second_date, meeting_state, completion_notes)
                VALUES
                (@Name, @ProName, @ID, @phone_number, @Email, @Advisor, @MeetingType, @Title, @Detail, @MeetingTopic, @FirstDate, @SecondDate, '승인대기', '');
            ";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", currentstudent.name);
                        cmd.Parameters.AddWithValue("@ProName", consult_professorname.Text);
                        cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        cmd.Parameters.AddWithValue("@phone_number", currentstudent.phone_number);
                        cmd.Parameters.AddWithValue("@Email", currentstudent.e_mail);
                        cmd.Parameters.AddWithValue("@Advisor", consult_professor.Text);
                        cmd.Parameters.AddWithValue("@MeetingType", consult_meetingtype.Text);
                        cmd.Parameters.AddWithValue("@Title", consult_title.Text);
                        cmd.Parameters.AddWithValue("@Detail", apply);
                        cmd.Parameters.AddWithValue("@MeetingTopic", apply_detail.Text); // 'consult_a'의 값이 여기에 들어가야 하는지 확인 필요
                        cmd.Parameters.AddWithValue("@FirstDate", start_time);
                        cmd.Parameters.AddWithValue("@SecondDate", end_time);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("상담 신청이 완료되었습니다.", "상담신청", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("상담 신청에 실패했습니다.", "상담신청 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("데이터베이스 연결 실패: " + ex.Message);
                }

            }

        }
        private void find_professor_Click(object sender, EventArgs e)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var query = "SELECT name, department FROM pro_personal WHERE department = @department;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@department", currentstudent.department);

                        using (var reader = cmd.ExecuteReader())
                        {
                            search_professor.Rows.Clear();

                            while (reader.Read())
                            {
                                string name = reader.GetString("name");
                                string department = reader.GetString("department");
                                string role = name == currentstudent.advisor ? "지도교수" : "일반교수";

                                search_professor.Rows.Add(name, role, department);
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

        private void search_professor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dataGridView = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                if (dataGridView.Columns[e.ColumnIndex].Name == "방문")
                {
                    consult_professorname.Text = dataGridView.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    consult_professord.Text = dataGridView.Rows[e.RowIndex].Cells["department"].Value.ToString();
                    consult_professor.Text = dataGridView.Rows[e.RowIndex].Cells["roll"].Value.ToString();
                    consult_meetingtype.Text = dataGridView.Rows[e.RowIndex].Cells["방문"].Value.ToString();
                    consult_time_first.Visible = true;
                    consult_time_second.Visible = true;

                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "온라인")
                {
                    consult_professorname.Text = dataGridView.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    consult_professord.Text = dataGridView.Rows[e.RowIndex].Cells["department"].Value.ToString();
                    consult_professor.Text = dataGridView.Rows[e.RowIndex].Cells["roll"].Value.ToString();
                    consult_meetingtype.Text = dataGridView.Rows[e.RowIndex].Cells["온라인"].Value.ToString();
                    consult_time_first.Visible = false;
                    consult_time_second.Visible = false;
                }
            }
        }
        private void UncheckOtherCheckBoxes(System.Windows.Forms.CheckBox checkedBox)
        {
            foreach (Control control in tableLayoutPanel4.Controls)
            {
                // 컨트롤이 CheckBox이고, 현재 체크된 박스가 아니면 체크 해제
                if (control is System.Windows.Forms.CheckBox checkBox && checkBox != checkedBox)
                {
                    checkBox.Checked = false;
                }
            }
        }

        private void checkBox_01_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked)
            {
                string label = label_01.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }

        }

        private void checkBox_02_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_02.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_03_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_03.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_04_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_04.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_05_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_05.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_06_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_06.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_07_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_07.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_08_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_08.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_09_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_09.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_10_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_10.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_11_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_11.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_12_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_12.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_13_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_13.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_14_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_14.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_15_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_15.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_16_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_16.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_17_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_17.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_18_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_18.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_19_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_19.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_20_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_20.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_21_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_21.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_22_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_22.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_23_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_23.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_24_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_24.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_25_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_25.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_26_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_26.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void checkBox_27_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox currentCheckBox = sender as System.Windows.Forms.CheckBox;
            if (currentCheckBox != null && currentCheckBox.Checked) // 수정된 조건
            {
                string label = label_27.Text;
                consult_a = label;
                UncheckOtherCheckBoxes(currentCheckBox);
            }
        }

        private void search_consult_Click(object sender, EventArgs e)
        {
            consult_searchbox.Rows.Clear();
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = $"SELECT pro_name, first_date, meeting_state FROM consulting WHERE ID = {currentstudent.ID};";

                    using (var cmd = new MySqlCommand(query, conn))
                    {

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string pro_name = reader["pro_name"].ToString();
                                string first_date = reader["first_date"].ToString();
                                string meeting_state = reader["meeting_state"].ToString();

                                switch (meeting_state)
                                {
                                    case "승인대기":
                                        meeting_state = "승인대기";
                                        break;
                                    case "상담 취소":
                                        meeting_state = "상담취소";
                                        break;
                                    case "상담 전(승인)":
                                        meeting_state = "승인완료";
                                        break;
                                    case "상담 완료":
                                        meeting_state = "상담완료";
                                        break;
                                };
                                consult_searchbox.Rows.Add(pro_name, first_date, meeting_state);
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

        private void logout_btn_stu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("로그아웃", "알림");
            login_window login_Window = new login_window();
            this.Hide();
            login_Window.ShowDialog();
            this.Close();
        }
        private void UpdateTime()
        {
            string formatteddatetime = time_machine_str.Value.ToString("yyyy-MM-dd HH:mm:ss");
            date_time_label_str.Text = formatteddatetime;

            // formatteddatetime 값을 DateTime 객체로 파싱
            DateTime currentDateTime = DateTime.Parse(formatteddatetime);

            // 비교를 위한 시작 시간과 종료 시간을 정의
            DateTime startTime_1 = new DateTime(2024, 3, 5, 9, 0, 0);
            DateTime endTime_1 = new DateTime(2024, 3, 5, 18, 0, 0);

            DateTime startTime_2 = new DateTime(2024, 2, 27, 9, 0, 0);
            DateTime endTime_2 = new DateTime(2024, 2, 27, 18, 0, 0);

            DateTime startTime_3 = new DateTime(2024, 2, 25, 9, 0, 0);
            DateTime endTime_3 = new DateTime(2024, 2, 25, 18, 0, 0);



            //기숙사를 신청했었으면 시간 관계없이 수정버튼이 떠야함 currentstudent.dormitory == O, currentstudent.livingroom == 예)용주학사 이용하기
            // 현재 시간이 2024-03-05 09:00 ~ 2024-03-05 18:00 사이인지 확인
            if (currentDateTime >= startTime_1 && currentDateTime <= endTime_1)
            {
                if (currentstudent.gender == "남")
                {
                    btn_1.Visible = true;
                }
                else if (currentstudent.gender == "여")
                {
                    btn_2.Visible = true;
                }
                btn_3.Visible = false;
                btn_4.Visible = false;
                btn_5.Visible = false;
                btn_6.Visible = false;
                btn_7.Visible = false;
                btn_8.Visible = false;
            }
            else if (currentDateTime >= startTime_2 && currentDateTime <= endTime_2)
            {
                if (currentstudent.gender == "남")
                {
                    btn_3.Visible = true;
                }
                else if (currentstudent.gender == "여")
                {
                    btn_4.Visible = true;
                }
                btn_1.Visible = false;
                btn_2.Visible = false;
                btn_5.Visible = false;
                btn_6.Visible = false;
                btn_7.Visible = false;
                btn_8.Visible = false;
            }
            else if (currentDateTime >= startTime_3 && currentDateTime <= endTime_3)
            {
                if (currentstudent.gender == "남")
                {
                    btn_5.Visible = true;
                }
                else if (currentstudent.gender == "여")
                {
                    btn_6.Visible = true;
                }
                btn_1.Visible = false;
                btn_2.Visible = false;
                btn_3.Visible = false;
                btn_4.Visible = false;
                btn_7.Visible = false;
                btn_8.Visible = false;
            }
            else
            {
                btn_1.Visible = false;
                btn_2.Visible = false;
                btn_3.Visible = false;
                btn_4.Visible = false;
                btn_5.Visible = false;
                btn_6.Visible = false;
                btn_7.Visible = false;
                btn_8.Visible = false;
            }
        }

        private void UpdateDayOfWeek()
        {
            string dayOfWeek = time_machine_str.Value.DayOfWeek.ToString();
            DayOfWeek_str.Text = dayOfWeek;
        }
        private void time_machine_str_ValueChanged(object sender, EventArgs e)
        {
            UpdateTime();
            UpdateDayOfWeek();
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!manualChange)
            {
                time_machine_str.Value = time_machine_str.Value.AddSeconds(0.5); // 현재 시간에 +1초
            }
            UpdateTime();
        }

        private void enrolment_search_Click(object sender, EventArgs e)
        {
            string completion = enrolment_completion.Text; //전체,전공선택,전공필수,교양필수,교양선택
            string department = enrolment_department.Text; // 전체,컴공,경영,체육
            string grade = enrolment_grade.Text; //전체,1,2,3,4

            enrollment_searchtable.Rows.Clear();
            if (completion == "이수구분" || department == "학과" || grade == "학년")
            {
                MessageBox.Show("분류를 선택해주세요");
                return;
            }
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM class;" ; // 'class' 테이블에서 모든 강의 정보 조회
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string currentDepartment = reader["department"].ToString();
                                string currentCompletion = reader["completion"].ToString(); // 현재 줄의 이수 구분
                                string currentGrade = reader["grade"].ToString(); // 현재 줄의 학년, "학년" 문자 제거

                                bool ignoreGradeCondition = completion == "교양선택" || completion == "교양필수";

                                if ((completion == "전체" || currentCompletion == completion) &&
                                    (department == "전체" || currentDepartment == department) &&
                                    (ignoreGradeCondition || grade == "전체" || currentGrade == grade))
                                {
                                    enrollment_searchtable.Rows.Add(reader["code"].ToString(), reader["completion"].ToString(), reader["subject_name"].ToString(), reader["credits"].ToString(), reader["name"].ToString(), reader["grade"].ToString(), reader["fixed_number"].ToString(), reader["time"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void button_inquiry_Click(object sender, EventArgs e)
        {
            string inquiry_major = combobox_roommate_major.Text;
            string inquiry_year = combobox_roommate_year.Text;
            string inquiry_gender = combobox_roommate_gender.Text;

            datagridview_dormitory.Rows.Clear();
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT p.department, p.ID, p.year, p.gender, p.status, p.name, d.livingroom FROM stu_personal p INNER JOIN dormitory d ON p.ID = d.ID;";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader["name"].ToString();
                                string department = reader["department"].ToString();
                                string year = reader["year"].ToString();
                                string gender = reader["gender"].ToString();
                                if ((inquiry_major == "전체" || department == inquiry_major) &&
                                    (inquiry_year == "전체" || year == inquiry_year) &&
                                    (inquiry_gender == "전체" || gender == inquiry_gender) &&
                                        (currentstudent.name != name))
                                {
                                    datagridview_dormitory.Rows.Add(new object[]
                                {
                                department,
                                reader["ID"],
                                year,
                                gender,
                                reader["status"],
                                reader["name"],
                                reader["livingroom"]
                                });
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
        private void apply_dormitory()
        {
            panel_apply.Visible = true;
            panel_apply.BringToFront();
            txt_name.Text = currentstudent.name;
            txt_gender.Text = currentstudent.gender;
            txt_major.Text = currentstudent.department;
            txt_email.Text = currentstudent.e_mail;
            txt_hp.Text = currentstudent.phone_number;
            txt_bankname.Text = currentstudent.name;
            txt_parent.Text = currentstudent.parent;
            txt_account.Text = currentstudent.account;
        }
        private void btn_1_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '용주학사(남자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("용주학사(남자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '용주학사(남자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "용주학사(남자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }

                txt_apply_livingroom.Text = label_apply1.Text;
                apply_dormitory();
                button_revise1.Visible = true;
                button_revise1.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }

        }
        private void btn_2_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '용주학사(여자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("용주학사(여자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '용주학사(여자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "용주학사(여자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply2.Text;
                apply_dormitory();
                button_revise2.Visible = true;
                button_revise2.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }
        private void btn_3_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '챌린지하우스1인실(남자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("챌린지하우스1인실(남자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '챌린지하우스1인실(남자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "챌린지하우스1인실(남자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply3.Text;
                apply_dormitory();
                button_revise3.Visible = true;
                button_revise3.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }
        private void btn_4_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '챌린지하우스1인실(여자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("챌린지하우스1인실(여자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '챌린지하우스1인실(여자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "챌린지하우스1인실(여자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply4.Text;
                apply_dormitory();
                button_revise4.Visible = true;
                button_revise4.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }

        private void btn_5_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '챌린지하우스2인실(남자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("챌린지하우스2인실(남자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '챌린지하우스2인실(남자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "챌린지하우스2인실(남자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply5.Text;
                apply_dormitory();
                button_revise5.Visible = true;
                button_revise5.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }
        private void btn_6_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '챌린지하우스1인실(여자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("챌린지하우스1인실(여자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '챌린지하우스1인실(여자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "챌린지하우스1인실(여자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply3.Text;
                apply_dormitory();
                button_revise6.Visible = true;
                button_revise6.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }

        private void btn_7_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '도솔학사(남자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("도솔학사(남자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '도솔학사(남자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "도솔학사(남자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply7.Text;
                apply_dormitory();
                button_revise7.Visible = true;
                button_revise7.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }

        private void btn_8_Click(object sender, EventArgs e)
        {
            if (currentstudent.status == "휴학")
            {
                MessageBox.Show("휴학한 학생은 신청할 수 없습니다.");
                return;
            }
            if (count == 0)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string countQuery = @"SELECT COUNT(*) FROM dormitory WHERE livingroom = '도솔학사(여자)';";
                        using (var countCmd = new MySqlCommand(countQuery, conn))
                        {
                            int dormitoryCount = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (dormitoryCount >= 5)
                            {
                                MessageBox.Show("도솔학사(여자) 신청이 마감되었습니다.");
                                return;
                            }
                            else
                            {
                                string updateQuery = @"UPDATE dormitory SET livingroom = '도솔학사(여자)' WHERE ID = @ID;";
                                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                                    updateCmd.ExecuteNonQuery();
                                    currentstudent.livingroom = "도솔학사(여자)";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                    }
                }
                txt_apply_livingroom.Text = label_apply8.Text;
                apply_dormitory();
                button_revise8.Visible = true;
                button_revise8.Size = new Size(71, 33);
                count++;
            }
            else if (count == 2)
            {
                MessageBox.Show("이미 신청하셨습니다");
            }
        }
        private int count = 0;

        private void button_back_Click(object sender, EventArgs e)
        {
            if (count == 1)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"UPDATE dormitory SET  livingroom = '-' WHERE ID = @ID;";
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                            cmd.ExecuteNonQuery();
                            currentstudent.livingroom = "-";
                        }
                    }
                    catch
                    {

                    }
                }
            }
            count--;
            panel_apply.Visible = false;
            button_revise1.Visible = false;
            button_revise2.Visible = false;
            button_revise3.Visible = false;
            button_revise4.Visible = false;
            button_revise5.Visible = false;
            button_revise6.Visible = false;
            button_revise7.Visible = false;
            button_revise8.Visible = false;
        }

        private void button_submit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_hp.Text) || txt_hp.Text == "-")
            {
                MessageBox.Show("연락처를 입력해주세요");
            }
            else if (!long.TryParse(txt_hp.Text.Trim(), out _) || txt_hp.Text.Trim().Length != 11)
            {
                MessageBox.Show("연락처가 잘못 입력되었습니다.");
            }
            else if (string.IsNullOrWhiteSpace(txt_parent.Text.Trim()) || txt_parent.Text.Trim() == "-")
            {
                MessageBox.Show("학부모 연락처를 입력해주세요.");
            }
            else if (!long.TryParse(txt_parent.Text.Trim(), out _) || txt_parent.Text.Trim().Length != 11)
            {
                MessageBox.Show("학부모 연락처가 잘못 입력되었습니다.");
            }
            else if (txt_hp == txt_parent)
            {
                MessageBox.Show("연락처와 학부모 연락처가 같습니다.");
            }
            else if (string.IsNullOrWhiteSpace(combobox_bank.Text))
            {
                MessageBox.Show("은행을 선택해주세요");
            }
            else if (string.IsNullOrWhiteSpace(txt_account.Text) || txt_account.Text == "-")
            {
                MessageBox.Show("계좌번호를 입력해주세요");
            }
            else if (!long.TryParse(txt_account.Text.Trim(), out _))
            {
                MessageBox.Show("계좌번호가 잘못 입력되었습니다");
            }
            else if (string.IsNullOrWhiteSpace(combobox_submit_email.Text))
            {
                MessageBox.Show("이메일을 선택해주세요");
            }
            else if (count == 1)
            {
                MessageBox.Show("신청되었습니다");

                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        // 불필요한 콤마 제거
                        string query = @"UPDATE dormitory SET dormitory = 'O', email = @email, hp = @hp, parent = @Parent, bank = @Bank, account = @Account WHERE ID = @ID;";
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", currentstudent.ID); // id
                            cmd.Parameters.AddWithValue("@email", txt_email.Text); // email
                            cmd.Parameters.AddWithValue("@hp", txt_hp.Text); // hp
                            cmd.Parameters.AddWithValue("@Parent", txt_parent.Text); // 사용자 인터페이스에서 입력된 parent 값
                            cmd.Parameters.AddWithValue("@Bank", combobox_bank.Text); // 사용자 인터페이스에서 선택된 bank 값
                            cmd.Parameters.AddWithValue("@Account", txt_account.Text); // 사용자 인터페이스에서 입력된 account 값

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        // 오류 처리
                        Console.WriteLine(ex.Message);
                    }
                }
                panel_apply.Visible = false;
                count++;
            }
        }
        private void revise_dormitory()
        {
            txt_revise_livingroom.Text = currentstudent.livingroom;
            txt_revise_name.Text = currentstudent.name;
            txt_revise_gender.Text = currentstudent.gender;
            txt_revise_major.Text = currentstudent.department;
            txt_revise_email.Text = currentstudent.e_mail;
            txt_revise_hp.Text = currentstudent.phone_number;
            txt_revise_bankname.Text = currentstudent.name;
            txt_revise_parent.Text = currentstudent.parent;
            txt_revise_account.Text = currentstudent.account;
            combobox_revise_bank.Text = currentstudent.bank;
            panel_revise.Visible = true;
            panel_revise.BringToFront();
        }
        private void button_revise1_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }
        private void button_revise2_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }
        private void button_revise3_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }

        private void button_revise4_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }

        private void button_revise5_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }

        private void button_revise6_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }

        private void button_revise7_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }

        private void button_revise8_Click(object sender, EventArgs e)
        {
            revise_dormitory();
        }
        private void enrollment_basket_search_Click(object sender, EventArgs e)
        {
            string completion = enrolment_completion_2.Text; //전체,전공선택,전공필수,교양필수,교양선택
            string department = enrolment_department_2.Text; // 전체,컴공,경영,체육
            string grade = enrolment_grade_2.Text; //전체,1,2,3,4

            enrollment_baskettable.Rows.Clear();
            if (completion == "이수구분" || department == "학과" || grade == "학년")
            {
                MessageBox.Show("분류를 선택해주세요");
                return;
            }
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM class;"; // 'class' 테이블에서 모든 강의 정보 조회
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string currentDepartment = reader["department"].ToString();
                                string currentCompletion = reader["completion"].ToString(); // 현재 줄의 이수 구분
                                string currentGrade = reader["grade"].ToString(); // 현재 줄의 학년, "학년" 문자 제거

                                bool ignoreGradeCondition = completion == "교양선택" || completion == "교양필수";

                                if ((completion == "전체" || currentCompletion == completion) &&
                                    (department == "전체" || currentDepartment == department) &&
                                    (ignoreGradeCondition || grade == "전체" || currentGrade == grade))
                                {
                                    enrollment_baskettable.Rows.Add("담기", "취소",reader["code"].ToString(), reader["completion"].ToString(), reader["subject_name"].ToString(), reader["department"].ToString(), reader["credits"].ToString(), reader["name"].ToString(), reader["grade"].ToString(), reader["fixed_number"].ToString(), reader["time"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void button_revise_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_revise_hp.Text.Trim()) || txt_revise_hp.Text.Trim() == "-")
            {
                MessageBox.Show("연락처를 입력해주세요.");
            }
            else if (!long.TryParse(txt_revise_hp.Text.Trim(), out _) || txt_revise_hp.Text.Trim().Length != 11)
            {
                MessageBox.Show("연락처가 잘못 입력되었습니다.");
            }
            else if (string.IsNullOrWhiteSpace(txt_revise_parent.Text.Trim()) || txt_revise_parent.Text.Trim() == "-")
            {
                MessageBox.Show("학부모 연락처를 입력해주세요.");
            }
            else if (!long.TryParse(txt_revise_parent.Text.Trim(), out _) || txt_revise_parent.Text.Trim().Length != 11)
            {
                MessageBox.Show("학부모 연락처가 잘못 입력되었습니다.");
            }
            else if (txt_revise_hp.Text == txt_revise_parent.Text)
            {
                MessageBox.Show("연락처와 학부모 연락처가 같습니다.");
            }
            else if (string.IsNullOrWhiteSpace(combobox_revise_bank.Text))
            {
                MessageBox.Show("은행을 선택해주세요");
            }
            else if (string.IsNullOrWhiteSpace(txt_revise_account.Text) || txt_revise_account.Text == "-")
            {
                MessageBox.Show("계좌번호를 입력해주세요");
            }
            else if (!long.TryParse(txt_revise_account.Text.Trim(), out _))
            {
                MessageBox.Show("계좌번호가 잘못 입력되었습니다");
            }
            else if (string.IsNullOrWhiteSpace(combobox_revise_email.Text))
            {
                MessageBox.Show("이메일을 선택해주세요");
            }
            else
            {
                MessageBox.Show("수정되었습니다");

                using (var conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"UPDATE dormitory SET hp = @hp, parent = @parent, bank = @bank, account = @account WHERE ID = @ID;";
                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                            cmd.Parameters.AddWithValue("@hp", txt_revise_hp.Text);
                            cmd.Parameters.AddWithValue("@parent", txt_revise_parent.Text);
                            cmd.Parameters.AddWithValue("@bank", combobox_revise_bank.Text);
                            cmd.Parameters.AddWithValue("@account", txt_revise_account.Text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch
                    {

                    }
                }
                panel_revise.Visible = false;
            }
        }

        private void button_revise_back_Click(object sender, EventArgs e)
        {
            panel_revise.Visible = false;
        }

        private void button_revise_delete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("삭제되었습니다");
            button_revise1.Visible = false;
            button_revise2.Visible = false;
            button_revise3.Visible = false;
            button_revise4.Visible = false;
            button_revise5.Visible = false;
            button_revise6.Visible = false;
            button_revise7.Visible = false;
            button_revise8.Visible = false;
            panel_revise.Visible = false;
            tablelayout_present_roommate.Visible = false;
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string getRoommateQuery = @"SELECT roommate from dormitory WHERE ID = @currentstudentID;";
                    string roommatename = "";

                    using (var getRoommateCmd = new MySqlCommand(getRoommateQuery, conn))
                    {
                        getRoommateCmd.Parameters.AddWithValue("currentstudentID", currentstudent.ID);
                        using (var reader = getRoommateCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                roommatename = reader["roommate"].ToString();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(roommatename))
                    {
                        string updateRoommateQuery = @"UPDATE dormitory SET roommate = '-' WHERE name = @RoommateName;";
                        using (var updateRoommateCmd = new MySqlCommand(updateRoommateQuery, conn))
                        {
                            updateRoommateCmd.Parameters.AddWithValue("@RoommateName", roommatename);
                            updateRoommateCmd.ExecuteNonQuery();
                        }
                    }

                    string query = @"UPDATE dormitory SET eamil = '-', hp = '-', dormitory = '-', livingroom = '-', parent = '-' ,bank = '-', account = '-', roommate = '-' WHERE ID = @ID;";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {

                }
            }

            count = 0;
        }
        private int roommate_count = 0;
        private void datagridview_dormitory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dataGridView = sender as DataGridView;
            if (e.RowIndex >= 0 && dataGridView.Columns[e.ColumnIndex].Name == "choice")
            {
                string selectedName = dataGridView.Rows[e.RowIndex].Cells["roommate_name"].Value.ToString();
                string selectedGender = dataGridView.Rows[e.RowIndex].Cells["gender"].Value.ToString();
                string selectedLivingroom = dataGridView.Rows[e.RowIndex].Cells["livingroom"].Value.ToString();
                string selectedStatus = dataGridView.Rows[e.RowIndex].Cells["status"].Value.ToString();


                if (currentstudent.gender != selectedGender)
                {
                    MessageBox.Show("성별이 같지 않습니다.");
                }
                else if (currentstudent.livingroom == "챌린지하우스1인실(남자)" || currentstudent.livingroom == "챌린지하우스1인실(여자)")
                {
                    MessageBox.Show("1인실 생활관 신청해 룸메이트 신청이 불가합니다.");
                }
                else if (currentstudent.livingroom != selectedLivingroom)
                {
                    MessageBox.Show("생활관이 같지 않습니다.");
                }
                else if (currentstudent.status != selectedStatus)
                {
                    MessageBox.Show("휴학한 학생입니다.");
                }
                else if (roommate_count == 0)
                {
                    tablelayout_present_roommate.Visible = true;
                    txt_present_roommate_major.Text = dataGridView.Rows[e.RowIndex].Cells["major"].Value.ToString();
                    txt_present_roommate_num.Text = dataGridView.Rows[e.RowIndex].Cells["num"].Value.ToString();
                    txt_present_roommate_year.Text = dataGridView.Rows[e.RowIndex].Cells["year"].Value.ToString();
                    txt_present_roommate_gender.Text = dataGridView.Rows[e.RowIndex].Cells["gender"].Value.ToString();
                    txt_present_roommate_status.Text = dataGridView.Rows[e.RowIndex].Cells["status"].Value.ToString();
                    txt_present_roommate_name.Text = dataGridView.Rows[e.RowIndex].Cells["roommate_name"].Value.ToString();
                    MessageBox.Show("신청되었습니다");

                    using (var conn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();

                            // 김동민의 roommate를 김동현으로 업데이트
                            string updateCurrentStudentQuery = @"UPDATE dormitory SET roommate = @RoommateName WHERE name = @CurrentStudentName;";
                            using (var cmd1 = new MySqlCommand(updateCurrentStudentQuery, conn))
                            {
                                cmd1.Parameters.AddWithValue("@RoommateName", selectedName);
                                cmd1.Parameters.AddWithValue("@CurrentStudentName", currentstudent.name);
                                cmd1.ExecuteNonQuery();
                            }

                            // 김동현의 roommate를 김동민으로 업데이트
                            string updateSelectedRoommateQuery = @"UPDATE dormitory SET roommate = @CurrentStudentName WHERE name = @RoommateName;";
                            using (var cmd2 = new MySqlCommand(updateSelectedRoommateQuery, conn))
                            {
                                cmd2.Parameters.AddWithValue("@CurrentStudentName", currentstudent.name);
                                cmd2.Parameters.AddWithValue("@RoommateName", selectedName);
                                cmd2.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"오류가 발생했습니다: {ex.Message}");
                        }
                    }
                    roommate_count++;
                }
                else if (roommate_count == 1)
                {
                    MessageBox.Show("이미 신청하셨습니다.");
                }
            }
        }

        private double CalculateAverageGrade(List<List<string>> score)
        {
            var gradeValues = new Dictionary<string, double>
            {
                {"A+", 4.5}, {"A0", 4.0}, {"B+", 3.5}, {"B0", 3.0}, {"C+", 2.5}, {"C0", 2.0}, {"D+", 1.5}, {"D0", 1.0}, {"F", 0.0}
            };

            double totalGradeValue = 0;
            int count = 0;
            foreach (var parts in score) // parts=(회계기초,전공필수,b+)
            {
                for (var i=0; i<parts.Count; i++) //i=회계기초 / i = 전공필수
                {
                    var grade = parts[2]; // 성적을 가져옵니다. 예: "A0"
                    if (gradeValues.TryGetValue(grade, out double value) && parts[2]!="-")
                    {
                        totalGradeValue += value;
                        count++; // 유효한 성적 데이터의 개수를 카운트
                    }
                }
            }
            if (count == 0) return 0; // 유효한 성적 데이터가 없는 경우, 평균값은 0입니다.
            currentstudent.avr_score = Math.Round(totalGradeValue / count, 2).ToString();
            return Math.Round(totalGradeValue / count, 2);  // 평균 성적 반환
        }
        private int DetermineMaxCredits()
        {
            // 1학년 1학기의 경우
            if (currentstudent.year == 1 && currentstudent.semester == 1)
            {
                return 18;
            }

            // 평균 성적에 따른 학점 계산
            if (Double.Parse(currentstudent.avr_score) >= 3.0)
            {
                return 21; // 3.0 이상은 21학점
            }
            else if (Double.Parse(currentstudent.avr_score) >= 2.0)
            {
                return 18; // 2.0 이상 3.0 미만은 18학점
            }
            else if (Double.Parse(currentstudent.avr_score) >= 1.0)
            {
                return 15; // 1.0 이상 2.0 미만은 15학점
            }
            else
            {
                return 12; // 그 외 경우는 12학점으로 가정 (조정 가능)
            }
        }
        private int DetermineMaxCredits_basket()
        {
            // 1학년 1학기의 경우
            if (currentstudent.year == 1 && currentstudent.semester == 1)
            {
                return 24;
            }

            // 평균 성적에 따른 학점 계산
            if (Double.Parse(currentstudent.avr_score) >= 3.0)
            {
                return 27; // 3.0 이상은 21학점
            }
            else if (Double.Parse(currentstudent.avr_score) >= 2.0)
            {
                return 24; // 2.0 이상 3.0 미만은 18학점
            }
            else if (Double.Parse(currentstudent.avr_score) >= 1.0)
            {
                return 21; // 1.0 이상 2.0 미만은 15학점
            }
            else
            {
                return 18; // 그 외 경우는 12학점으로 가정 (조정 가능)
            }
        }
        

        // 장바구니에 담긴 과목의 총 학점을 계산하는 함수
        
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            var dataGridView = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                if (dataGridView.Columns[e.ColumnIndex].Name == "basket")
                {
                    var subjectCode = dataGridView.Rows[e.RowIndex].Cells[2].Value.ToString(); // 과목 코드
                    string subjectDepartment = "";
                    string subjectType = "";

                    try
                    {
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            // 과목 정보 조회
                            var checkQuery = $"SELECT COUNT(*) FROM basket_code WHERE name = '{currentstudent.ID}' AND basket_code = '{subjectCode}';";
                            using (var checkCommand = new MySqlCommand(checkQuery, connection))
                            {
                                int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                                if (count > 0)
                                {
                                    MessageBox.Show("이미 장바구니에 추가된 과목입니다.");
                                    return; // 추가 작업 중단
                                }
                            }
                            var query = $"SELECT department, completion FROM class WHERE code='{subjectCode}';";
                            using (var command = new MySqlCommand(query, connection))
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        subjectType = reader["completion"].ToString(); // 과목 유형
                                        subjectDepartment = reader["department"].ToString(); // 개설 학과
                                    }
                                }
                            }

                            // 전공 과목이며 학생의 학과와 과목의 학과가 일치하는지 확인
                            if ((subjectType == "전공필수" || subjectType == "전공선택" && subjectDepartment == currentstudent.department) || subjectType.StartsWith("교양"))
                            {
                                // 장바구니에 과목 추가
                                var insertQuery = $"INSERT INTO basket_code (name, basket_code) VALUES ('{currentstudent.ID}', '{subjectCode}')";
                                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                                {
                                    insertCommand.ExecuteNonQuery();
                                    MessageBox.Show("성공적으로 장바구니에 담아졌습니다.");
                                    UpdateCreditsDisplay();
                                }
                            }
                            
                            else if ((subjectType == "전공필수" || subjectType == "전공선택") && subjectDepartment != currentstudent.department)
                            {
                                MessageBox.Show("다른 학과의 전공 과목은 담을 수 없습니다.", "장바구니 추가 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }

                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "basket_cancel")
                {
                    // 선택된 행에서 과목 코드 가져오기
                    var subjectCode = dataGridView.Rows[e.RowIndex].Cells[2].Value.ToString();

                    try
                    {
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            // 장바구니에서 해당 과목 삭제
                            var deleteQuery = $"DELETE FROM basket_code WHERE name='{currentstudent.ID}' AND basket_code='{subjectCode}'";
                            using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                            {
                                int rowsAffected = deleteCommand.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    // 데이터베이스에서 삭제 성공 시, DataGridView에서도 행 제거
                                    //dataGridView.Rows.RemoveAt(e.RowIndex);
                                    MessageBox.Show("장바구니에서 과목이 제거되었습니다.");
                                    UpdateCreditsDisplay();
                                }
                                else
                                {
                                    MessageBox.Show("삭제할 과목을 찾지 못했습니다.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void enrollment_apply_button_Click(object sender, EventArgs e)
        {
            string completion = enrolment_completion3.Text;
            string department = enrolment_department3.Text;
            string grade = enrolment_grade3.Text;

            enrollment_apply_table.Rows.Clear();

            if (completion == "이수구분" || department == "학과" || grade == "학년")
            {
                MessageBox.Show("분류를 선택해주세요");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // 현재 신청 인원을 포함한 과목 정보 조회 쿼리
                    var query = @"
                SELECT 
                    class.code, 
                    class.subject_name, 
                    class.credits, 
                    class.name, 
                    class.grade, 
                    class.fixed_number, 
                    class.time, 
                    class.completion,
                    class.department,
                    (SELECT COUNT(*) FROM stu_results WHERE subject = class.subject_name AND score = '-') AS current_enrollment
                FROM 
                    class;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string currentDepartment = reader["department"].ToString();
                                string currentCompletion = reader["completion"].ToString(); // 이수 구분
                                string currentGrade = reader["grade"].ToString(); // 학년
                                string currentVsMax = $"{reader["current_enrollment"].ToString()}/{reader["fixed_number"].ToString()}";

                                bool ignoreGradeCondition = completion == "교양선택" || completion == "교양필수";
                                string buttonLabel = "신청";

                                if ((completion == "전체" || currentCompletion == completion) &&
                                    (department == "전체" || currentDepartment == department) &&
                                    (ignoreGradeCondition || grade == "전체" || currentGrade == grade))
                                {
                                    // 강의 정보와 현재/최대 신청 인원 정보를 테이블에 추가
                                    enrollment_apply_table.Rows.Add(
                                        buttonLabel,
                                        "취소",
                                        reader["code"].ToString(),
                                        currentCompletion,
                                        reader["subject_name"].ToString(),
                                        reader["credits"].ToString(),
                                        reader["name"].ToString(),
                                        currentGrade,
                                        currentVsMax,  // 현재 신청 인원/최대 신청 인원
                                        reader["time"].ToString()
                                    );
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
       
        private int CalculateBasketCredits(string studentId)
        {
            int totalCredits = 0;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // 현재 학생이 장바구니에 담은 과목들의 코드와 해당 과목의 학점 정보 조회
                    var query = $@"
                SELECT SUM(class.credits) AS totalCredits
                FROM basket_code
                INNER JOIN class ON basket_code.basket_code = class.code
                WHERE basket_code.name = '{studentId}';";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            totalCredits = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating basket credits: {ex.Message}");
            }

            return totalCredits;
        }
        private int CalculateCredits()
        {
            int totalCredits = 0;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // 현재 학생이 이번 학기에 신청한 과목들의 총 학점을 계산
                    var query = $@"
                SELECT SUM(class.credits) AS TotalCredits
                FROM stu_results
                INNER JOIN class ON stu_results.subject = class.subject_name
                WHERE stu_results.ID = '{currentstudent.ID}' AND stu_results.score = '-';";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            totalCredits = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating credits: {ex.Message}");
            }

            return totalCredits;
        }
        private void enrolment_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (enrolment_main.SelectedIndex == 1)
            {
                max_score.Text = DetermineMaxCredits_basket().ToString();
                present_score.Text = CalculateBasketCredits(currentstudent.ID).ToString();
            }
            if (enrolment_main.SelectedIndex == 2)
            {
                FillDataGridViewInInnerTabControl();
                max_apply_score.Text = DetermineMaxCredits().ToString();
                present_apply_score.Text = CalculateCredits().ToString();
            }
        }
        private void UpdateCreditsDisplay()
        {
            max_score.Text = DetermineMaxCredits_basket().ToString();
            present_score.Text = CalculateBasketCredits(currentstudent.ID).ToString();
        }
        private void UpdateCreditsDisplay_2()
        {
            max_apply_score.Text = DetermineMaxCredits().ToString();
            present_apply_score.Text = CalculateCredits().ToString();
        }
        private void FillDataGridViewInInnerTabControl()
        {
            DataGridView dgv = enrolment_submain.TabPages[0].Controls["enrollment_basket_apply_table"] as DataGridView;
            dgv.Rows.Clear();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = @"
                SELECT 
                    class.code, 
                    class.department, 
                    class.grade, 
                    class.subject_name, 
                    class.completion, 
                    class.credits, 
                    class.time, 
                    class.fixed_number,
                    class.semester,
                    (SELECT COUNT(*) FROM stu_results WHERE subject = class.subject_name AND score = '-') AS current_enrollment
                FROM 
                    class 
                INNER JOIN 
                    basket_code ON class.code = basket_code.basket_code
                WHERE 
                    basket_code.name = @StudentID;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentID", currentstudent.ID);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string currentVsMax = $"{reader["current_enrollment"].ToString()}/{reader["fixed_number"].ToString()}";
                                dgv.Rows.Add(
                                    "신청",
                                    "취소",
                                    reader["code"].ToString(),
                                    reader["completion"].ToString(),
                                    reader["subject_name"].ToString(),
                                    reader["credits"].ToString(),
                                    reader["department"].ToString(),
                                    reader["grade"].ToString(),
                                    currentVsMax, // 현재 신청 인원/최대 신청 인원
                                    reader["time"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void enrollment_basket_apply_table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dataGridView = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                if (dataGridView.Columns[e.ColumnIndex].Name == "apply")
                {
                    var completion = dataGridView.Rows[e.RowIndex].Cells[3].Value.ToString();
                    var subject = dataGridView.Rows[e.RowIndex].Cells[4].Value.ToString();
                    var newClassTime = dataGridView.Rows[e.RowIndex].Cells[9].Value.ToString().Split(',');
                    var newClassDay = newClassTime[0];
                    var newClassTimes = newClassTime.Skip(1).ToArray();
                    var creditsToApply = int.Parse(dataGridView.Rows[e.RowIndex].Cells[5].Value.ToString());
                    int currentAppliedCredits = CalculateCredits();
                    var enrollmentData = dataGridView.Rows[e.RowIndex].Cells[8].Value.ToString().Split('/');
                    int currentEnrollment = int.Parse(enrollmentData[0]);
                    int maxEnrollment = int.Parse(enrollmentData[1]);

                    // 정원 초과 검사
                    if (currentEnrollment >= maxEnrollment)
                    {
                        MessageBox.Show("이 과목은 정원이 초과되어 더 이상 신청할 수 없습니다.");
                        return; // 신청 작업 중단
                    }
                    if (currentAppliedCredits + creditsToApply > DetermineMaxCredits())
                    {
                        MessageBox.Show("최대 신청 가능 학점을 초과합니다.");
                        return; // 추가 작업 중단
                    }
                    try
                    {
                        var checkQuery = $"SELECT COUNT(*) FROM stu_results WHERE ID = '{currentstudent.ID}' AND score = '-' and subject = '{subject}';";
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var checkCommand = new MySqlCommand(checkQuery, connection))
                            {
                                int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                                if (count > 0)
                                {
                                    MessageBox.Show("이미 신청된 과목입니다.");
                                    return; // 추가 작업 중단
                                }
                            }
                            var query = $@"SELECT class.time FROM stu_results
                                   INNER JOIN class ON stu_results.subject = class.code
                                   WHERE stu_results.ID = '{currentstudent.ID}' AND stu_results.score = '-';";
                            using (var command = new MySqlCommand(query, connection))
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var classTimeInfo = reader["time"].ToString().Split(',');
                                        var classDay = classTimeInfo[0];
                                        var classTimes = classTimeInfo.Skip(1).ToArray();

                                        if (newClassDay == classDay)
                                        {
                                            foreach (var time in classTimes)
                                            {
                                                if (newClassTimes.Any(nt => nt.Trim() == time.Trim()))
                                                {
                                                    MessageBox.Show("선택하신 시간대에 이미 신청한 강의가 있습니다.");
                                                    return; // 시간 충돌이 있으므로 추가 작업 중단
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var insertQuery = $@"INSERT INTO stu_results (ID, subject, completion, grade, semester, score) 
                     VALUES ('{currentstudent.ID}', '{subject}', '{completion}', {currentstudent.grade}, {currentstudent.semester}, '-')";

                            using (var insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show("과목 신청이 완료되었습니다.");
                                UpdateCreditsDisplay_2();
                                FillDataGridViewInInnerTabControl();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "cancel")
                {
                    var subject = dataGridView.Rows[e.RowIndex].Cells[4].Value.ToString();
                    try
                    {
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            var deleteQuery = $@"DELETE FROM stu_results WHERE ID = '{currentstudent.ID}' AND subject = '{subject}' AND score = '-';";
                            using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                            {
                                int rowsAffected = deleteCommand.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("수강 취소되었습니다.");
                                    UpdateCreditsDisplay_2();
                                    FillDataGridViewInInnerTabControl();
                                }
                                else
                                {
                                    MessageBox.Show("해당 과목이 수강 신청 목록에 없습니다.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void enrollment_apply_table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dataGridView = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                if (dataGridView.Columns[e.ColumnIndex].Name == "search_apply")
                {
                    var completion = dataGridView.Rows[e.RowIndex].Cells[3].Value.ToString();
                    var subject = dataGridView.Rows[e.RowIndex].Cells[4].Value.ToString();
                    var newClassTime = dataGridView.Rows[e.RowIndex].Cells[9].Value.ToString().Split(',');
                    var newClassDay = newClassTime[0];
                    var newClassTimes = newClassTime.Skip(1).ToArray();
                    var creditsToApply = int.Parse(dataGridView.Rows[e.RowIndex].Cells[5].Value.ToString());
                    int currentAppliedCredits = CalculateCredits();
                    var enrollmentData = dataGridView.Rows[e.RowIndex].Cells[8].Value.ToString().Split('/');
                    int currentEnrollment = int.Parse(enrollmentData[0]);
                    int maxEnrollment = int.Parse(enrollmentData[1]);

                    // 정원 초과 검사
                    if (currentEnrollment >= maxEnrollment)
                    {
                        MessageBox.Show("이 과목은 정원이 초과되어 더 이상 신청할 수 없습니다.");
                        return; // 신청 작업 중단
                    }
                    if (currentAppliedCredits + creditsToApply > DetermineMaxCredits())
                    {
                        MessageBox.Show("최대 신청 가능 학점을 초과합니다.");
                        return; // 추가 작업 중단
                    }
                    try
                    {
                        var checkQuery = $"SELECT COUNT(*) FROM stu_results WHERE ID = '{currentstudent.ID}' AND score = '-' and subject = '{subject}';";
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var checkCommand = new MySqlCommand(checkQuery, connection))
                            {
                                int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                                if (count > 0)
                                {
                                    MessageBox.Show("이미 신청된 과목입니다.");
                                    return; // 추가 작업 중단
                                }
                            }
                            var query = $@"SELECT class.time FROM stu_results
                                   INNER JOIN class ON stu_results.subject = class.code
                                   WHERE stu_results.ID = '{currentstudent.ID}' AND stu_results.score = '-';";
                            using (var command = new MySqlCommand(query, connection))
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var classTimeInfo = reader["time"].ToString().Split(',');
                                        var classDay = classTimeInfo[0];
                                        var classTimes = classTimeInfo.Skip(1).ToArray();

                                        if (newClassDay == classDay)
                                        {
                                            foreach (var time in classTimes)
                                            {
                                                if (newClassTimes.Any(nt => nt.Trim() == time.Trim()))
                                                {
                                                    MessageBox.Show("선택하신 시간대에 이미 신청한 강의가 있습니다.");
                                                    return; // 시간 충돌이 있으므로 추가 작업 중단
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var insertQuery = $@"INSERT INTO stu_results (ID, subject, completion, grade, semester, score) 
                     VALUES ('{currentstudent.ID}', '{subject}', '{completion}', {currentstudent.grade}, {currentstudent.semester}, '-')";

                            using (var insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show("과목 신청이 완료되었습니다.");
                                UpdateCreditsDisplay_2();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
                else if (dataGridView.Columns[e.ColumnIndex].Name == "cancel_apply")
                {
                    var subject = dataGridView.Rows[e.RowIndex].Cells[4].Value.ToString();
                    try
                    {
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            var deleteQuery = $@"DELETE FROM stu_results WHERE ID = '{currentstudent.ID}' AND subject = '{subject}' AND score = '-';";
                            using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                            {
                                int rowsAffected = deleteCommand.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("수강 취소되었습니다.");
                                    UpdateCreditsDisplay_2();
                                }
                                else
                                {
                                    MessageBox.Show("해당 과목이 수강 신청 목록에 없습니다.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void button_stu_loa_Click(object sender, EventArgs e)
        {
            string path = "student.txt";
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ');
                if (parts[0] == currentstudent.name)
                {
                    if (parts[6].Contains("휴학"))
                    {
                        MessageBox.Show("이미 휴학신청이 완료되었습니다.");
                        break;
                    }
                    if (comboBox_loa.SelectedIndex == -1 || comboBox_backsemester.SelectedIndex == -1)
                    {
                        MessageBox.Show("모든 항목을 입력 및 선택하세요.");
                        return;
                    }
                    else
                    {
                        string selectValue = comboBox_loa.SelectedItem.ToString();
                        parts[6] = selectValue;
                        lines[i] = string.Join(" ", parts);
                        currentstudent.back_semester = comboBox_backsemester.SelectedItem.ToString();

                        File.WriteAllLines("student.txt", lines);

                        MessageBox.Show("휴학신청이 완료되었습니다.");
                    }
                    break;
                }
            }
        }

        private void button_stu_loa_cancel_Click(object sender, EventArgs e)
        {
            string cancleText = "재학";
            string path = "student.txt";
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ');
                if (parts[0] == currentstudent.name)
                {
                    parts[6] = cancleText;
                    lines[i] = string.Join(" ", parts);
                    break;
                }
            }
            File.WriteAllLines("student.txt", lines);

            MessageBox.Show("휴학신청이 취소되었습니다.");
        }
        private void loa_show()
        {
            string filePath = "student.txt";
            var lines = File.ReadAllLines(filePath);

            dataGridView_loa.Rows.Clear();

            foreach (var line in lines)
            {
                var cells = line.Split(' ');

                if (cells[0] == currentstudent.name)
                {
                    dataGridView_loa.Rows.Add(cells[1], cells[7], cells[0]);
                    textBox_startstate.Text = cells[6];
                    textBox_startSemester.Text = $"{cells[14]}학년 {cells[15]}학기";

                    textBox_startYear.Text = DateTime.Now.ToString("yyyy");
                    textBox_date2.Text = DateTime.Now.ToString("yyyy.MM.dd.dddd");
                    currentstudent.back_year = dateTimePicker_backyear.Value.ToString("yyyy"); //복학예정년도

                    if (comboBox_backsemester.SelectedItem != null)
                    {
                        currentstudent.back_semester = comboBox_backsemester.SelectedItem.ToString(); ; //복학예정학기
                    }
                    else
                    {
                        currentstudent.back_semester = " ";
                    }
                    currentstudent.start_year = textBox_startYear.Text; //휴학년도
                    currentstudent.start_semester = textBox_startSemester.Text; //휴학학기
                    currentstudent.start_date = textBox_date2.Text; //휴학날짜
                    currentstudent.start_type = textBox_startstate.Text; //상태
                }
            }
        }
        private void button_student_loa_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            loa_show();
        }

        private void button_student_gbts_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            string tag = pictureBox.Tag.ToString(); // 예: "기숙사 신청"
            string baseName = pictureBox.Name.Replace("button_", ""); // 예: "dor"
            update_top(baseName, tag);
            stu_back();
        }
        private void stu_back()
        {
            string filePath = "student.txt";
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var cells = line.Split(' ');

                if (cells[0] == currentstudent.name)
                {
                    dataGridView_back.Rows.Add(cells[1], cells[7], cells[0]);

                    if (cells[6].Contains("휴학"))
                    {
                        textBox_year.Text = currentstudent.start_year; //휴학년도
                        textBox_semester.Text = currentstudent.start_semester; //휴학학기
                        textBox_datee.Text = currentstudent.start_date; //휴학일자
                        textBox_type.Text = currentstudent.start_type; //휴학구분
                        textBox_backyear.Text = currentstudent.back_year.ToString(); //복학예정년도
                        textBox_backdate.Text = DateTime.Now.ToString("yyyy.MM.dd.dddd");//휴학신청일자
                        textBox_backyearsubmit.Text = DateTime.Now.ToString("yyyy");//복학신청년도
                        textBox_backsemester.Text = currentstudent.back_semester.ToString(); //복학예정학기
                    }
                    else
                    {
                        textBox_year.Text = " ";
                        textBox_semester.Text = " ";
                        textBox_datee.Text = " ";
                        textBox_type.Text = " ";
                        textBox_backyear.Text = " ";
                        textBox_backdate.Text = " ";
                        textBox_backyearsubmit.Text = " ";
                        textBox_backsemester.Text = " ";
                    }
                }
            }
        }

        private void button_backsubmit_Click(object sender, EventArgs e)
        {
            string filePath = "student.txt";
            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                var cells = lines[i].Split(' ');

                if (cells[0] == currentstudent.name)
                {
                    if (cells[6].Contains("재학"))
                    {

                        MessageBox.Show("휴학 상태가 아닙니다.");
                        break;
                    }

                    if (comboBox_backsemestersubmit.SelectedIndex == -1 || comboBox_backtype.SelectedIndex == -1)
                    {
                        MessageBox.Show("모든 항목을 입력 및 선택하세요.");
                        return;
                    }

                    else
                    {
                        string Value = "재학";
                        cells[6] = Value;
                        lines[i] = string.Join(" ", cells);

                        //textBox_type.Text = " "; //유지되게 수정
                        File.WriteAllLines(filePath, lines);

                        MessageBox.Show("복학신청이 완료되었습니다.");
                    }
                    break;
                }
            }
        }

        private void button_backcancle_Click(object sender, EventArgs e)
        {
            string filePath = "student.txt";
            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                var cells = lines[i].Split(' ');

                if (cells[0] == currentstudent.name)
                {
                    cells[6] = currentstudent.start_type;
                    lines[i] = string.Join(" ", cells);
                    break;
                }
            }
            File.WriteAllLines(filePath, lines);

            MessageBox.Show("복학신청이 취소되었습니다.");
        }

        private void btn_roommate_cancel_Click(object sender, EventArgs e)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string getRoommateQuery = "SELECT roommate FROM dormitory WHERE ID = @CurrentStudentID;";
                    string roommateID = "";
                    using (var getRoommateCmd = new MySqlCommand(getRoommateQuery, conn))
                    {
                        getRoommateCmd.Parameters.AddWithValue("@CurrentStudentID", currentstudent.ID);
                        using (var reader = getRoommateCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                roommateID = reader["roommate"].ToString();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(roommateID))
                    {
                        string updateRoommateQuery = "UPDATE dormitory SET roommate = '-' WHERE ID = @RoommateID;";
                        using (var updateRoommateCmd = new MySqlCommand(updateRoommateQuery, conn))
                        {
                            updateRoommateCmd.Parameters.AddWithValue("@RoommateID", roommateID);
                            updateRoommateCmd.ExecuteNonQuery();
                        }
                    }
                    string updateQuery = "UPDATE dormitory SET roommate = '-' WHERE ID = @ID;";
                    using (var updateCmd = new MySqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@ID", currentstudent.ID);
                        updateCmd.ExecuteNonQuery();
                        MessageBox.Show("취소되었습니다.");
                        tablelayout_present_roommate.Visible = false;
                        roommate_count--;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("룸메이트 취소 중 오류가 발생했습니다: " + ex.Message);
                }
            }
           
        }

    }
}
      