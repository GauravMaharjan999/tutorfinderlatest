using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kachuwa.Training.Service
{
  public   class CourseTimingStreamMappingService : ICourseTimingStreamMapping
    {
        public CrudService<CourseTimingStreamMapping> CourseTimingStreamMappingCrudService { get; set; } = new CrudService<CourseTimingStreamMapping>();
    }
}
