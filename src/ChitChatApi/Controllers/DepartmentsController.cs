using ChitChatApi.Context;
using ChitChatApi.Dtos;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ChitChatApi.Controllers;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments(
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var departments = await connection.QueryAsync<DepartmentDto>(
            """
            select id, name
            from department
            order by name
            """);

        return Ok(departments);
    }
}
