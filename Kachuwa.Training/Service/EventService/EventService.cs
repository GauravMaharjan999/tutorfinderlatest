using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kachuwa.Training.Service
{
   public class EventService : IEventService
    {
        public CrudService<Event> EventCrudService { get; set; } = new CrudService<Event>();
        public CrudService<EventRegister> EventRegisterCrudService { get; set; } = new CrudService<EventRegister>();
    }
}
