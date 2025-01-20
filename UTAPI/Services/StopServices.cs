using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.Stop;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class StopServices : IStopServices
    {
        private readonly DataContext _context;
        private readonly ILogger<StopServices> _logger;
        private readonly IMapper _mapper;

        public StopServices(DataContext context, ILogger<StopServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Stop> CreateStopAsync(PostStop request)
        {
            try
            {
                var stop = _mapper.Map<Stop>(request);
                stop.Active = true;
                _context.Stop.Add(stop);
                await _context.SaveChangesAsync();

                return stop;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the stop");
                throw new Exception("An error occurred while creating the stop");
            }
        }

        public async Task DeleteStopAsync(Guid id)
        {
            try
            {
                var stop = await _context.Stop.FindAsync(id);
                _context.Stop.Remove(stop);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the stop");
                throw new Exception("An error occurred while deleting the stop");
            }
        }

        public async Task<OneStop> GetByIdAsync(Guid id)
        {
            try
            {
                var stop = await _context.Stop.FindAsync(id);
                return _mapper.Map<OneStop>(stop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding the stop");
                throw new Exception("An error occurred while finding the stop");
            }
        }

        public async Task<List<ListStop>> GetStopsAsync(FilterQuery? filter)
        {
            var prepStops = _context.Stop.ProjectTo<ListStop>(_mapper.ConfigurationProvider);

            if (prepStops == null) throw new Exception("Error getting stops");

            QueryHelper.ApplyListFilters(ref prepStops, filter);

            var stops = await prepStops.ToListAsync();

            if (stops == null || stops.Count == 0) return new List<ListStop>();

            return stops;
        }

        public async Task UpdateStopAsync(Guid id, PatchStop request)
        {
            try
            {
                var updatedStop = _mapper.Map<PatchStop>(request);
                var existingStop = await _context.Stop.FindAsync(id);

                if (existingStop == null) throw new Exception("An error occurred while editing the stop");

                existingStop.Name = updatedStop.Name;
                if (updatedStop.Latitude.HasValue)
                {
                    existingStop.Latitude = updatedStop.Latitude.Value;
                }

                if (updatedStop.Longitude.HasValue)
                {
                    existingStop.Longitude = updatedStop.Longitude.Value;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing the stop");
                throw new Exception("An error occurred while editing the stop");
            }
        }
    }
}
