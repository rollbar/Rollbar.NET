using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class SalaryCalculator
    {
        public Decimal GetWeeklySalary(string employeeId, int weeks)
        {
            string employeeName = String.Empty;
            Decimal salary = 0;
            Decimal weeklySalary = 0;

            //string connString = ConfigurationManager.ConnectionStrings
            //                                  ["EmployeeDatabase"].ConnectionString;
            // Access database to get salary for employee.
            // In this example, just assume it's some large number.
            employeeName = "John Smith";
            salary = 1000000;
            weeklySalary = salary / weeks;


            return weeklySalary;
        }
    }
}
