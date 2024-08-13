using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using universal2;

namespace erp_2
{

    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Student student = new Student();
            Professor professor = new Professor();
            student.update_student_id();
            professor.update_professor_id();
            student.update_advisor();
            Application.Run(new login_window());
        }
    }
}
