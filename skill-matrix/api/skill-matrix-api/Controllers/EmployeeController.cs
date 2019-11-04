using System.Collections.Generic;
using System.Web.Http;
using skill_matrix_api.Provider;
using skill_matrix_api.Models;

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
        public IEnumerable<EmployeeModel> GetAll()
        {
            return _provider.GetAll();
        }

        /// <summary>
        /// Get employee by employeeId
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("api/Employee/{employeeId}")]
        public EmployeeModel Get(string employeeId)
        {
            return _provider.Get(employeeId);
        }

        [HttpPost]
        public EmployeeModel Add([FromBody]EmployeeModel employee)
        {
            return employee;
        }
    }
}
