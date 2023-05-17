using System;
using Microsoft.ML;
using CourseRecommendation.DataStructures;
using System.IO;
using Microsoft.ML.Trainers;
using CourseRecommendationConsoleApp.DataStructures;
using System.Linq;
using System.Collections.Generic;
using CourseRecommend.CourseRecommemdation.DataStructures;

namespace CourseRecommendation
{
    public class CourseRecom
    {

        public static string ModelsRelativePath = @"../../../../MLModels";
        public static string DatasetsRelativePath = @"E:\Projects\Github\New folder\TutorFinder\LiveTraining\LiveTraining\CourseRecommend\Data";
        
        public static string TrainingDataRelativePath = $"{DatasetsRelativePath}/recommendation-ratings-train.csv";
        public static string TestDataRelativePath = $"{DatasetsRelativePath}/recommendation-ratings-test.csv";
        public static string CoursesDataLocation = $"{DatasetsRelativePath}/courses.csv";
        
        public static string TrainingDataLocation = GetAbsolutePath(TrainingDataRelativePath);
        public static string TestDataLocation = GetAbsolutePath(TestDataRelativePath);
        
        public static string ModelPath = GetAbsolutePath(ModelsRelativePath);

        //private const float predictionuserId = 10;
        //private const int predictioncourseId = 10;

        public static Recom ClassRec(int predictionuserId)
        {
                Recom recom = new Recom();

            MLContext mlcontext = new MLContext();


            IDataView trainingDataView = mlcontext.Data.LoadFromTextFile<CourseRating>(TrainingDataLocation, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlcontext.Data.LoadFromTextFile<CourseRating>(TestDataLocation, hasHeader: true, separatorChar: ',');


            var dataProcessingPipeline = mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: nameof(CourseRating.userId))
                           .Append(mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: "courseIdEncoded", inputColumnName: nameof(CourseRating.courseId)));


            MatrixFactorizationTrainer.Options options = new MatrixFactorizationTrainer.Options();
            options.MatrixColumnIndexColumnName = "userIdEncoded";
            options.MatrixRowIndexColumnName = "courseIdEncoded";
            options.LabelColumnName = "Label";
            options.NumberOfIterations = 20;
            options.ApproximationRank = 100;


            var pipeline = mlcontext.Transforms.Conversion.MapValueToKey(
                            inputColumnName: "userId",
                            outputColumnName: "userIdEncoded")
            .Append(mlcontext.Transforms.Conversion.MapValueToKey(
                                inputColumnName: "courseId",
                                outputColumnName: "courseIdEncoded")

// step 2: find recommendations using matrix factorization
            .Append(mlcontext.Recommendation().Trainers.MatrixFactorization(options)));

            // train the model
            //Console.WriteLine("Training the model...");
            var model = pipeline.Fit(trainingDataView);
            //Console.WriteLine();




            //Console.WriteLine("Evaluating the model...");
            var predictions = model.Transform(testDataView);
            var metrics = mlcontext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
            //Console.WriteLine($"  RMSE: {metrics.RootMeanSquaredError:#.##}");
            //Console.WriteLine($"  L1:   {metrics.MeanAbsoluteError.ToString():#.##}");
            //Console.WriteLine($"  L2:   {metrics.MeanSquaredError:#.##}");
            //Console.WriteLine();





            //Console.WriteLine("Calculating the score for user 6 liking the movie 'GoldenEye'...");
            var predictionEngine = mlcontext.Model.CreatePredictionEngine<CourseRating, CourseRatingPrediction>(model);
            //var prediction = predictionEngine.Predict(
            //    new CourseRating()
            //    {
            //        userId = predictionuserId,
            //        courseId = predictioncourseId  // GoldenEye
            //    }
            //);
            //Console.WriteLine($"  Score: {prediction.Score}");
            //Console.WriteLine();




            //Console.WriteLine("Calculating the top 3 courses for user 6...");
            var top3 = (from m in Course._courses.Value
                        let p = predictionEngine.Predict(
                           new CourseRating()
                           {
                               userId =predictionuserId,
                               courseId = m.courseId
                           })
                        orderby p.Score descending
                        select (CourseId: m.courseId,Score:p.Score)).Take(3);
            try
            {

            foreach (var t in top3)
            {

                //Console.WriteLine($"  Score:{t.Score}\tCourse: {Course.Get(t.CourseId)?.courseTitle}\tCourseId:{Course.Get(t.CourseId)?.courseId} ");
                recom.courseId.Add(Course.Get(t.CourseId).courseId);
                
            }
            }catch(Exception e)
            {
                
            }
            


            return recom;
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(CourseRecom).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}