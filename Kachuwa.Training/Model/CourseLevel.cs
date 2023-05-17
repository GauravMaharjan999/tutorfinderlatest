using System;
using System.Collections.Generic;
using System.Text;

namespace Kachuwa.Training.Model
{
  public   class CourseLevel
    {
        public CourseLevel() { }

        public CourseLevel(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public int Id
        {
            get; set;
        }
        public string Name { get; set; }

        public List<CourseLevel> List()
        {
            List<CourseLevel> lst = new List<CourseLevel> {
                new CourseLevel(1,"Beginner"),
                new CourseLevel(2,"Intermediate"),
                new CourseLevel(3,"Professional"),

            };
            return lst;
        }
    }
}
