using System.Security.Cryptography.X509Certificates;

namespace Tutorial3Tests;

public class AdvancedEmpDeptTests
{
    // 11. MAX salary
    // SQL: SELECT MAX(Sal) FROM Emp;
    [Fact]
    public void ShouldReturnMaxSalary()
    {
        var emps = Database.GetEmps();

        decimal? maxSalary = emps.Max(p => p.Sal);
        Assert.Equal(5000, maxSalary);
    }

    // 12. MIN salary in department 30
    // SQL: SELECT MIN(Sal) FROM Emp WHERE DeptNo = 30;
    [Fact]
    public void ShouldReturnMinSalaryInDept30()
    {
        var emps = Database.GetEmps();

        decimal? minSalary  = emps.Where(p=>p.DeptNo==30).Min(p => p.Sal);
        Assert.Equal(1250, minSalary);
    }

    // 13. Take first 2 employees ordered by hire date
    // SQL: SELECT * FROM Emp ORDER BY HireDate ASC FETCH FIRST 2 ROWS ONLY;
    [Fact]
    public void ShouldReturnFirstTwoHiredEmployees()
    {
        var emps = Database.GetEmps();

        var firstTwo = emps.Select(p=>p).OrderBy(p=>p.HireDate).Take(2).ToList(); 
        
        Assert.Equal(2, firstTwo.Count);
        Assert.True(firstTwo[0].HireDate <= firstTwo[1].HireDate);
    }

    // 14. DISTINCT job titles
    // SQL: SELECT DISTINCT Job FROM Emp;
    [Fact]
    public void ShouldReturnDistinctJobTitles()
    {
        var emps = Database.GetEmps();

        var jobs = emps.Select(p=>p.Job).Distinct().ToList(); 
        
        Assert.Contains("PRESIDENT", jobs);
        Assert.Contains("SALESMAN", jobs);
    }

    // 15. Employees with managers (NOT NULL Mgr)
    // SQL: SELECT * FROM Emp WHERE Mgr IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithManagers()
    {
        var emps = Database.GetEmps();

        var withMgr = emps.Select(p=>p).Where(p=>p.Mgr.HasValue); 
        
        Assert.All(withMgr, e => Assert.NotNull(e.Mgr));
    }

    // 16. All employees earn more than 500
    // SQL: SELECT * FROM Emp WHERE Sal > 500; (simulate all check)
    [Fact]
    public void AllEmployeesShouldEarnMoreThan500()
    {
        var emps = Database.GetEmps();

        var result = emps.Select(p=>p).All(p=>p.Sal>500); 
        
        Assert.True(result);
    }

    // 17. Any employee with commission over 400
    // SQL: SELECT * FROM Emp WHERE Comm > 400;
    [Fact]
    public void ShouldFindAnyWithCommissionOver400()
    {
        var emps = Database.GetEmps();

        var result = emps.Select(p=>p).Any(p=>p.Comm>400); 
        
        Assert.True(result);
    }

    // 18. Self-join to get employee-manager pairs
    // SQL: SELECT E1.EName AS Emp, E2.EName AS Manager FROM Emp E1 JOIN Emp E2 ON E1.Mgr = E2.EmpNo;
    [Fact]
    public void ShouldReturnEmployeeManagerPairs()
    {
        var emps = Database.GetEmps();
        var result = emps.Join(emps,p1=>p1.Mgr,p2=>p2.EmpNo,(p1,p2)=>new{ Employee = p1.EName ,Manager=p2.EName });
        
        Assert.Contains(result, r => r.Employee == "SMITH" && r.Manager == "FORD");
    }

    // 19. Let clause usage (sal + comm)
    // SQL: SELECT EName, (Sal + COALESCE(Comm, 0)) AS TotalIncome FROM Emp;
    [Fact]
    public void ShouldReturnTotalIncomeIncludingCommission()
    {
        var emps = Database.GetEmps();

        var result = emps.Select(p => new { p.EName, Total = p.Sal + p.Comm });
        
        Assert.Contains(result, r => r.EName == "ALLEN" && r.Total == 1900);
    }

    // 20. Join all three: Emp → Dept → Salgrade
    // SQL: SELECT E.EName, D.DName, S.Grade FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldJoinEmpDeptSalgrade()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        var grades = Database.GetSalgrades();

        var result = emps.Join(depts, e => e.DeptNo, d => d.DeptNo, (emp, dept) => new { emp, dept })
            .SelectMany(group => grades, (group, grade) => new { group.emp, group.dept, grade })
            .Where(group => group.emp.Sal >= group.grade.Losal && group.emp.Sal <= group.grade.Hisal)
            .Select(group=> new { group.emp.EName, group.dept.DName,group.grade.Grade });
        
            
        Assert.Contains(result, r => r.EName == "ALLEN" && r.DName == "SALES" && r.Grade == 3);
    }
}
