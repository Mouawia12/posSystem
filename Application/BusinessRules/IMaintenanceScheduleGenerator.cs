using Core.Entities;
using System.Collections.Generic;

namespace Application.BusinessRules
{
    public interface IMaintenanceScheduleGenerator
    {
        IReadOnlyList<MaintenanceSchedule> Generate(DateTime startDateUtc, DateTime endDateUtc);
    }
}
