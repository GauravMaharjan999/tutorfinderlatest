using Microsoft.ML.Data;

namespace CourseRecommendationConsoleApp.DataStructures
{
    public class CourseRating
    {
        [LoadColumn(0)]
        public float userId;

        [LoadColumn(1)]
        public float courseId;

        [LoadColumn(2)]
        public float Label;
    }
}
