using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class AuditServices : IAuditServices
    {

        private readonly DataContext _context;
        private readonly ILogger<AuditServices> _logger; // facilita logging
        private readonly IMapper _mapper;

        public AuditServices(DataContext context, ILogger<AuditServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task AddAuditsAsync(IEnumerable<Audit> audits)
        {
            if (audits == null || !audits.Any())
                return;

            // Add the audits to the DbContext's Audit DbSet (assuming it's properly configured in the DbContext)
            await _context.Set<Audit>().AddRangeAsync(audits);

            // Save the changes to persist the audits to the database
            await _context.SaveChangesAsync();
        }

        public async Task<List<Audit>> GetAuditsAsync(FilterQuery? filter)
        {
            var prepAudits = _context.Audit.ProjectTo<Audit>(_mapper.ConfigurationProvider);

            if (prepAudits == null) throw new Exception("Error getting users");

            QueryHelper.ApplyListFilters(ref prepAudits, filter);

            var audits = await prepAudits.ToListAsync();

            if (audits == null || audits.Count == 0) return new List<Audit>();

            return audits;
        }
    }
}
