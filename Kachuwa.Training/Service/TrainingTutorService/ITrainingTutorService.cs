using Kachuwa.Data;
using Kachuwa.Identity.Models;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
    public interface ITrainingTutorService
    {
        CrudService<TrainingTutor> TrainingTutorCrudService { get; set; }
        Task<IEnumerable<IdentityUser>> TutorIdentityListNotMappedOnTutor();
        Task<IEnumerable<IdentityUser>> TutorIdentityList(int identityUserId);
    }
}
