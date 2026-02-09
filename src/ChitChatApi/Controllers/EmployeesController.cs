using ChitChatApi.Context;
using ChitChatApi.Dtos;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ChitChatApi.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var employees = await connection.QueryAsync<EmployeeDto>(
            """
            select id, name, department_id as DepartmentId, username
            from employee
            order by name
            """);

        return Ok(employees);
    }
}
