using System.Collections.Generic;
using System.Linq;
using skill_matrix_api.Models;

namespace skill_matrix_api.Repositories
{
    public class Repository
    {
        SkillMatrixEntities _skillMatrix;
        static readonly object lockObj = new object();
        private static Repository _repository = new Repository();

        public static Repository Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (_repository == null)
                    {
                        _repository = new Repository();
                    }
                }
                return _repository;
            }
        }

        private Repository()
        {
            _skillMatrix = new SkillMatrixEntities();
        }

        public Employee GetEmployee(string employeeId)
        {
            return _skillMatrix.Employees.FirstOrDefault(x => x.EmployeeId.Equals(employeeId));
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _skillMatrix.Employees.ToList();
        }

        public void AddEmployee(Employee employee)
        {
            _skillMatrix.Employees.Add(employee);
            _skillMatrix.SaveChanges();
        }
    }
}