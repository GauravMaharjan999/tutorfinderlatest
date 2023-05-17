using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kachuwa.Training.Service
{
    public interface IEventService
    {
        CrudService<Event> EventCrudService { get; set; }
        CrudService<EventRegister> EventRegisterCrudService { get; set; }
    }
}
