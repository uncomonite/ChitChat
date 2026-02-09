using ChitChatApi.Context;
using ChitChatApi.Dtos;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace ChitChatApi.Controllers;

[ApiController]
[Route("api/chatrooms/{chatRoomId:int}/messages")]
public sealed class MessagesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatMessageDto>> CreateMessage(
        [FromRoute] int chatRoomId,
        [FromBody] CreateMessageRequest request,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var message = await connection.QuerySingleAsync<ChatMessageDto>(
            """
            insert into chat_message (sender_id, chatroom_id, message)
            values (@SenderId, @ChatRoomId, @Message)
            returning id,
                      sender_id as SenderId,
                      (select name from employee where id = sender_id) as SenderName,
                      chatroom_id as ChatRoomId,
                      created_at as CreatedAt,
                      message
            """,
            new { ChatRoomId = chatRoomId, request.SenderId, request.Message });

        return Ok(message);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessages(
        [FromRoute] int chatRoomId,
        [FromQuery] int? limit,
        [FromServices] AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var effectiveLimit = limit is > 0 and <= 200 ? limit.Value : 100;

        await using var connection = await dbContext.OpenConnectionAsync(cancellationToken);
        var messages = (await connection.QueryAsync<ChatMessageDto>(
            """
            select m.id,
                   m.sender_id as SenderId,
                   e.name as SenderName,
                   m.chatroom_id as ChatRoomId,
                   m.created_at as CreatedAt,
                   m.message
            from chat_message m
            join employee e on e.id = m.sender_id
            where m.chatroom_id = @ChatRoomId
            order by m.created_at desc
            limit @Limit
            """,
            new { ChatRoomId = chatRoomId, Limit = effectiveLimit })).ToList();

        messages.Reverse();

        return Ok(messages);
    }
}
