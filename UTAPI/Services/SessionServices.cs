using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UTAPI.Data;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.Entity;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class SessionServices : ISessionServices
    {

        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<SessionServices> _logger; // facilita logging
        private readonly IMapper _mapper;

        public SessionServices(DataContext context, ILogger<SessionServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task CreateSessionAsync(string token, Guid userId)
        {
            try
            {

                // Deactivate any active sessions for this user
                var activeSessions = await _context.Session
                    .Where(s => s.UserId == userId && s.Active)
                    .ToListAsync();

                foreach (var activeSession in activeSessions)
                {
                    activeSession.Active = false;
                }

                // Save changes for deactivated sessions
                await _context.SaveChangesAsync();

                var session = new Session
                {
                    Active = true,
                    SessionDate = DateTime.UtcNow,
                    Token = token,
                    UserId = userId // Ensure that it's parsed correctly as a Guid
                };

                _context.Session.Add(session);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the session");
                throw new Exception("An error occurred while creating the session");
            }
        }

        public async Task DeactivateSessionAsync(Guid id)
        {
            try
            {
                // Find the session by ID
                var session = await _context.Session.FindAsync(id);

                // Check if the session exists and is active
                if (session != null && session.Active)
                {
                    // Set active to false
                    session.Active = false;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deactivating the session");
                throw new Exception("An error occurred while deactivating the session");
            }
        }


        public async Task<List<Session>> GetSessionsByUserAsync(FilterQuery? filter, Guid userId)
        {
            var prepSession = _context.Session.ProjectTo<Session>(_mapper.ConfigurationProvider);

            if (prepSession == null) throw new Exception("Error getting sessions");

            prepSession = prepSession.Where(x => x.UserId == userId);

            QueryHelper.ApplyListFiltersWithoutVars(ref prepSession, filter, ["userId"]); // Aplica os filtros de query, caso existam

            var session = await prepSession.ToListAsync();

            if (session == null || session.Count == 0) return new List<Session>();

            return session;
        }
    }
}
