using Kachuwa.Core.DI;
using Kachuwa.Training.Service;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Kachuwa.Training
{
   public  class TrainingServiceRegistrar :IServiceRegistrar
    {
        public void Update(IServiceCollection serviceCollection)
        {

        }

        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<ITrainingTutorService, TrainingTutorService>();
            serviceCollection.AddSingleton<ITrainingCourseCategoryService, TrainingCourseCategoryService>();
            serviceCollection.AddSingleton<ICourseService, CourseService>();
            serviceCollection.AddSingleton<ICourseTutorMappingService, CourseTutorMappingService>();
            serviceCollection.AddSingleton<ICourseSyllabusService, CourseSyllabusService>();
            serviceCollection.AddSingleton<IEventService, EventService>();
            serviceCollection.AddSingleton<ICourseTimingService, CourseTimingService>();
            serviceCollection.AddSingleton<IEnrollService , EnrollService>();
            serviceCollection.AddSingleton<IPaymentLogService, PaymentLogService>();
            serviceCollection.AddSingleton<ICourseTimingStreamMapping, CourseTimingStreamMappingService>();

            var assp = new EmbeddedFileProvider(typeof(TrainingServiceRegistrar).GetTypeInfo().Assembly);
            serviceCollection.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(assp);
            });
        }
    }
}
