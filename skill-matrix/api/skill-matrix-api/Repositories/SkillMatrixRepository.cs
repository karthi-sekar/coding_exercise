using skill_matrix_api.Models;
using System.Linq;

namespace skill_matrix_api.Repositories
{
    public class SkillMatrixRepository
    {
        SkillMatrixEntities _skillMatrix;

        public SkillMatrixRepository()
        {
            _skillMatrix = new SkillMatrixEntities();
        }

        public void GetEmployee(string employeeId)
        {
            _skillMatrix.Employees.FirstOrDefault(x => x.EmployeeId == employeeId);
        }
    }
}