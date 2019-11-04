using System.Collections.Generic;
using System.Web.Http;
using skill_matrix_api.Provider;
using skill_matrix_api.ViewModel;

namespace skill_matrix_api.Controllers
{
    public class EmployeeController : ApiController
    {
        readonly EmployeeControllerProvider _provider;
        public EmployeeController()
        {
            _provider = new EmployeeControllerProvider();
        }

        /// <summary>
        /// Get all employees as list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<EmployeeViewModel> GetAll()
        {
            return _provider.GetAll();
        }

        /// <summary>
        /// Get employee by employeeId
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("api/Employee/{employeeId}")]
        public EmployeeViewModel Get(string employeeId)
        {
            return _provider.Get(employeeId);
        }

        [HttpPost]
        public EmployeeViewModel Add([FromBody]EmployeeViewModel employee)
        {
            return employee;
        }
    }
}
