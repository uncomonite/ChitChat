using ChitChatApi.Context;
using ChitChatApi.Dtos;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ChitChatApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<EmployeeDto>> Login(
        [FromBody] LoginRequest request,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var employee = await connection.QuerySingleOrDefaultAsync<EmployeeDto>(
            """
            select id, name, department_id as DepartmentId, username
            from employee
            where username = @Username and password = crypt(@Password, password)
            """,
            request);

        return employee is null ? Unauthorized() : Ok(employee);
    }
}
