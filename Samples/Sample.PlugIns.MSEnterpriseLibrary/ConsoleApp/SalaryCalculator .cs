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

            //Program.ExceptionManager.Process(() => {
            //    string connString = ConfigurationManager.ConnectionStrings
            //                                      ["EmployeeDatabase"].ConnectionString;
            //    // Access database to get salary for employee.
            //    // In this example, just assume it's some large number.
            //    employeeName = "John Smith";
            //    salary = 1000000;
            //    weeklySalary = salary / weeks;
            //}, Program.ExceptionShieldingPolicyID);

            //string connString = ConfigurationManager.ConnectionStrings
            //                                  ["EmployeeDatabase"].ConnectionString;
            // Access database to get salary for employee.
            // In this example, just assume it's some large number.
            employeeName = "John Smith";
            salary = 1000000;
            weeklySalary = salary / weeks;


            return weeklySalary;
        }

        //public Decimal GetWeeklySalary(string employeeId, int weeks)
        //{
        //    String connString = string.Empty;
        //    String employeeName = String.Empty;
        //    Decimal salary = 0;
        //    try
        //    {
        //        connString = ConfigurationManager
        //            .ConnectionStrings["EmployeeDatabase"]
        //            .ConnectionString;
        //        // Access database to get salary for employee here...
        //        // In this example, just assume it's some large number.
        //        employeeName = "John Smith";
        //        salary = 1000000;
        //        return salary / weeks;
        //    }
        //    catch (Exception ex)
        //    {
        //        // provide error information for debugging
        //        string template = "Error calculating salary for {0}."
        //                        + " Salary: {1}. Weeks: {2}\n"
        //                        + "Data connection: {3}\n{4}";
        //        Exception informationException = new Exception(
        //            string.Format(template, employeeName, salary, weeks,
        //                          connString, ex.Message));
        //        throw informationException;
        //    }
        //}
    }
}
