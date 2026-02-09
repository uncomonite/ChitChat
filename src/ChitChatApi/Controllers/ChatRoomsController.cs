using ChitChatApi.Context;
using ChitChatApi.Dtos;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ChitChatApi.Controllers;

[ApiController]
[Route("api/chatrooms")]
public sealed class ChatRoomsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatRoomDto>>> GetChatRooms(
        [FromQuery] int employeeId,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
        {
            return BadRequest("Employee id is required.");
        }

        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var rooms = await connection.QueryAsync<ChatRoomDto>(
            """
            select c.id,
                   c.topic,
                   (
                       select created_at
                       from chat_message
                       where chatroom_id = c.id
                       order by created_at desc
                       limit 1
                   ) as LastMessageAt,
                   (
                       select message
                       from chat_message
                       where chatroom_id = c.id
                       order by created_at desc
                       limit 1
                   ) as LastMessagePreview
            from chatroom c
            join chatroom_members m on m.chatroom_id = c.id
            where m.employee_id = @EmployeeId
            order by c.id
            """,
            new { EmployeeId = employeeId });

        return Ok(rooms);
    }

    [HttpPost]
    public async Task<ActionResult<ChatRoomDto>> CreateChatRoom(
        [FromBody] CreateChatRoomRequest request,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var roomId = await connection.ExecuteScalarAsync<int>(
            """
            insert into chatroom (topic)
            values (@Topic)
            returning id
            """,
            new { request.Topic },
            transaction);

        await connection.ExecuteAsync(
            """
            insert into chatroom_members (chatroom_id, employee_id)
            values (@ChatRoomId, @EmployeeId)
            on conflict do nothing
            """,
            new { ChatRoomId = roomId, EmployeeId = request.CreatorEmployeeId },
            transaction);

        await transaction.CommitAsync(cancellationToken);

        return Ok(new ChatRoomDto(roomId, request.Topic, null, null));
    }

    [HttpPatch("{chatRoomId:int}/topic")]
    public async Task<IActionResult> UpdateTopic(
        [FromRoute] int chatRoomId,
        [FromBody] UpdateChatRoomTopicRequest request,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(
            """
            update chatroom
            set topic = @Topic
            where id = @ChatRoomId
            """,
            new { ChatRoomId = chatRoomId, request.Topic });

        return updated == 0 ? NotFound() : NoContent();
    }

    [HttpPost("{chatRoomId:int}/members")]
    public async Task<IActionResult> AddMember(
        [FromRoute] int chatRoomId,
        [FromBody] AddChatRoomMemberRequest request,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            """
            insert into chatroom_members (chatroom_id, employee_id)
            values (@ChatRoomId, @EmployeeId)
            on conflict do nothing
            """,
            new { ChatRoomId = chatRoomId, request.EmployeeId });

        return NoContent();
    }

    [HttpDelete("{chatRoomId:int}/members/{employeeId:int}")]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] int chatRoomId,
        [FromRoute] int employeeId,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            """
            delete from chatroom_members
            where chatroom_id = @ChatRoomId and employee_id = @EmployeeId
            """,
            new { ChatRoomId = chatRoomId, EmployeeId = employeeId },
            transaction);

        var memberCount = await connection.ExecuteScalarAsync<int>(
            """
            select count(*)
            from chatroom_members
            where chatroom_id = @ChatRoomId
            """,
            new { ChatRoomId = chatRoomId },
            transaction);

        if (memberCount <= 1)
        {
            await connection.ExecuteAsync(
                """
                delete from chatroom
                where id = @ChatRoomId
                """,
                new { ChatRoomId = chatRoomId },
                transaction);
        }

        await transaction.CommitAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("{chatRoomId:int}/members")]
    public async Task<ActionResult<IEnumerable<ChatRoomMemberDto>>> GetMembers(
        [FromRoute] int chatRoomId,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var members = await connection.QueryAsync<ChatRoomMemberDto>(
            """
            select e.id, e.name, e.department_id as DepartmentId, e.username, m.joined_at as JoinedAt
            from chatroom_members m
            join employee e on e.id = m.employee_id
            where m.chatroom_id = @ChatRoomId
            order by e.name
            """,
            new { ChatRoomId = chatRoomId });

        return Ok(members);
    }
}
