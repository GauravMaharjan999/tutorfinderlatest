using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace CourseRecommendation.DataStructures
{
    class Course
    {
        public int courseId;

        public String courseTitle;

        private static String coursesdatasetRelativepath = $"{CourseRecom.DatasetsRelativePath}/courses.csv";
        private static string coursesdatasetpath = CourseRecom.GetAbsolutePath(coursesdatasetRelativepath);

        public static Lazy<List<Course>> _courses = new Lazy<List<Course>>(() => LoadCourseData(coursesdatasetpath));

        public Course()
        {
        }

        public  static Course Get(int id)
        {
            return _courses.Value.Single(m => m.courseId == id);
        }

        private static List<Course> LoadCourseData(String coursesdatasetpath)
        {
            var result = new List<Course>();
            Stream fileReader = File.OpenRead(coursesdatasetpath);
            StreamReader reader = new StreamReader(fileReader);
            try
            {
                bool header = true;
                int index = 0;
                var line = "";
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        line = reader.ReadLine();
                        header = false;
                    }
                    line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    int courseId = Int32.Parse(fields[0].ToString().TrimStart(new char[] { '0' }));
                    string courseTitle = fields[1].ToString();
                    result.Add(new Course() { courseId = courseId, courseTitle = courseTitle });
                    index++;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }
    }
}
