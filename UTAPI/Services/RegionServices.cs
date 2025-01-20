using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.Region;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class RegionServices : IRegionServices
    {
        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<RegionServices> _logger; // facilita o logging
        private readonly IMapper _mapper;

        public RegionServices(DataContext context, ILogger<RegionServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Criação de uma nova região
        public async Task<OneRegion> CreateRegionAsync(PostRegion request)
        {
            try
            {
                var checkIfNamePresent = await _context.Region
                    .Where(x => EF.Functions.ILike(x.Name, request.Name)) // Comparação insensível a maiúsculas/minúsculas
                    .FirstOrDefaultAsync();

                if (checkIfNamePresent != null) throw new Exception("A region with that name already exists");

                var region = _mapper.Map<Region>(request);
                _context.Region.Add(region);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<OneRegion>(region);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the region");
                throw new Exception("An error occurred while creating the region");
            }
        }

        // Exclusão de uma região
        public async Task DeleteRegionAsync(Guid id)
        {
            try
            {
                // Check if the region exists
                var region = await _context.Region.FindAsync(id);
                if (region == null) throw new Exception("Region not found");

                // Check for references in related tables
                var isReferencedInRoutes = await _context.Route.AnyAsync(r => r.RegionId == id);
                if (isReferencedInRoutes)
                {
                    throw new Exception("Region cannot be deleted as it is referenced by one or more routes.");
                }

                var isReferencedInEntities = await _context.Entity.AnyAsync(e => e.RegionId == id);
                if (isReferencedInEntities)
                {
                    throw new Exception("Region cannot be deleted as it is referenced by one or more entities.");
                }

                // Proceed with deletion if no references exist
                _context.Region.Remove(region);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the region");
                throw new Exception("An error occurred while deleting the region", ex);
            }
        }


        // Encontrar uma região por Id
        public async Task<OneRegion> GetByIdAsync(Guid id)
        {
            try
            {
                var region = await _context.Region
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (region == null) throw new Exception("Region not found");

                var regionResponse = _mapper.Map<OneRegion>(region);
                return regionResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding the region");
                throw new Exception("An error occurred while finding the region");
            }
        }

        // Listar todas as regiões
        public async Task<List<ListRegion>> GetRegionsAsync(FilterQuery? filter)
        {
            var prepRegions = _context.Region
                .ProjectTo<ListRegion>(_mapper.ConfigurationProvider); // Projeta os dados para a estrutura desejada

            if (prepRegions == null) throw new Exception("Error getting regions");

            QueryHelper.ApplyListFilters(ref prepRegions, filter); // Aplica filtros, se necessário

            var regions = await prepRegions.ToListAsync();

            if (regions == null || regions.Count == 0) return new List<ListRegion>();

            return regions;
        }

        // Atualizar informações de uma região
        public async Task UpdateRegionAsync(Guid id, PatchRegion request)
        {
            try
            {
                var updatedRegion = _mapper.Map<PatchRegion>(request);
                var existingRegion = await _context.Region.FindAsync(id);

                if (existingRegion == null) throw new Exception("Region not found");

                existingRegion.Name = updatedRegion.Name;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing the region");
                throw new Exception("An error occurred while editing the region");
            }
        }
    }
}
