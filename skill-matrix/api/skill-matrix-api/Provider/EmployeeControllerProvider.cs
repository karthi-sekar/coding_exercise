using System.Linq;
using System.Collections.Generic;
using skill_matrix_api.ViewModel;
using skill_matrix_api.Repositories;

namespace skill_matrix_api.Provider
{
    public class EmployeeControllerProvider
    {
        internal IEnumerable<EmployeeViewModel> GetAll()
        {
            return Repository.Instance.GetAllEmployees().Select(emp => MapEmployeeModel(emp));
        }

        internal EmployeeViewModel Get(string employeeId)
        {
            var emp = Repository.Instance.GetEmployee(employeeId);
            return MapEmployeeModel(emp);
        }

        private static EmployeeViewModel MapEmployeeModel(Employee emp)
        {
            return new EmployeeViewModel
            {
                Id = emp.Id,
                EmployeeId = emp.EmployeeId,
                Name = emp.Name,
                DateOfJoining = emp.DateOfJoining,
                DesignationId = emp.DesignationId,
                Experience = emp.Experience,
                IsActive = emp.IsActive
            };
        }
    }
}