using GameLoggd.Data;
using GameLoggd.Models;
using GameLoggd.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLoggd.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var otherUser = await _userManager.FindByNameAsync(username);

            if (otherUser == null) return NotFound("User not found");

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == otherUser.Id) ||
                            (m.SenderId == otherUser.Id && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id = m.Id,
                    content = m.Content,
                    sentAt = m.SentAt,
                    isSentByMe = m.SenderId == currentUser.Id,
                    senderName = m.Sender.UserName,
                    senderProfilePicture = m.Sender.ProfilePicturePath ?? "/images/default-profile.png"
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content)) return BadRequest("Message cannot be empty");

            var currentUser = await _userManager.GetUserAsync(User);
            var otherUser = await _userManager.FindByNameAsync(request.ReceiverUsername);

            if (otherUser == null) return NotFound("User not found");

            var message = new Message
            {
                SenderId = currentUser.Id,
                ReceiverId = otherUser.Id,
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            // Get all messages sent or received by current user
            var messages = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Group by the "other" user to get unique conversations
            var conversations = messages
                .Select(m => new
                {
                    UserId = m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId,
                    User = m.SenderId == currentUser.Id ? m.Receiver : m.Sender,
                    LastMessage = m.Content,
                    LastMessageTime = m.SentAt,
                    UnreadCount = m.ReceiverId == currentUser.Id && !m.IsRead ? 1 : 0
                })
                .GroupBy(x => x.UserId)
                .Select(g => g.First())
                .Select(x => new
                {
                    username = x.User.UserName,
                    profilePicture = x.User.ProfilePicturePath ?? "/images/default-profile.png",
                    lastMessage = x.LastMessage.Length > 20 ? x.LastMessage.Substring(0, 20) + "..." : x.LastMessage,
                    timeAgo = GetTimeAgo(x.LastMessageTime)
                })
                .ToList();

            return Json(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            // Get IDs of users we have conversations with
            var conversationUserIds = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            // Get friends (follows) excluding those we have conversations with
            var friends = await _context.UserFollows
                .Where(f => f.ObserverId == currentUser.Id && !conversationUserIds.Contains(f.TargetId))
                .Include(f => f.Target)
                .Select(f => new
                {
                    username = f.Target.UserName,
                    profilePicture = f.Target.ProfilePicturePath ?? "/images/default-profile.png"
                })
                .ToListAsync();

            return Json(friends);
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            if (span.TotalSeconds < 60) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h";
            return $"{(int)span.TotalDays}d";
        }

        public class SendMessageRequest
        {
            public string ReceiverUsername { get; set; }
            public string Content { get; set; }
        }
    }
}
