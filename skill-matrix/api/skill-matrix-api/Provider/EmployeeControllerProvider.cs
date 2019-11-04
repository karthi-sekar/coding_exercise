using System.Linq;
using System.Collections.Generic;
using skill_matrix_api.Models;
using skill_matrix_api.Repositories;

namespace skill_matrix_api.Provider
{
    public class EmployeeControllerProvider
    {
        internal IEnumerable<EmployeeModel> GetAll()
        {
            return Repository.Instance.GetAllEmployees().Select(emp => MapEmployeeModel(emp));
        }

        internal EmployeeModel Get(string employeeId)
        {
            var emp = Repository.Instance.GetEmployee(employeeId);
            return MapEmployeeModel(emp);
        }

        private static EmployeeModel MapEmployeeModel(Employee emp)
        {
            return new EmployeeModel
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