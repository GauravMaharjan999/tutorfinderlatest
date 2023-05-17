using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.LiveTrainingApp.Components
{
    [ViewComponent(Name = "TrainingTutor")]
    public class TrainingTutorViewComponent : KachuwaViewComponent
    {

        private readonly ILogger _logger;
        private readonly ITrainingTutorService _trainingTutorService;

        public TrainingTutorViewComponent(ILogger logger , ITrainingTutorService trainingTutorService)
        {
            _logger = logger;
            _trainingTutorService = trainingTutorService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var data = await  _trainingTutorService.TrainingTutorCrudService.GetListAsync("Where IsActive = @isActive and  IsShowOnHomePage =@isShow ", new { @isActive = true, @isShow = true });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Training Tutor loading error.", e);
                throw e;
            }

        }
        public override string DisplayName { get; } = "Training Tutor";
        public override bool IsVisibleOnUI { get; } = true;

    }
}


