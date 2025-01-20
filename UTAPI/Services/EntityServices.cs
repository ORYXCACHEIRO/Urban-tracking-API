using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.Entity;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class EntityServices : IEntityServices
    {
        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<EntityServices> _logger; // facilita logging
        private readonly IMapper _mapper;

        public EntityServices(DataContext context, ILogger<EntityServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Criação de uma nova entidade
        public async Task<Entity> CreateEntityAsync(PostEntity request)
        {
            try
            {
                var checkIfNamePresent = await _context.Entity.FirstOrDefaultAsync(x => x.Name == request.Name);

                if (checkIfNamePresent != null) throw new Exception("An entity with that name already exists");

                var regionExists = await _context.Region.AnyAsync(r => r.Id == request.RegionId);
                if (!regionExists) throw new Exception("The specified region does not exist");

                var entity = _mapper.Map<Entity>(request);
                entity.Active = true; // Define como ativo, caso não seja enviado na request
                _context.Entity.Add(entity);
                await _context.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the entity");
                throw new Exception("An error occurred while creating the entity");
            }
        }

        // Exclusão de uma entidade
        public async Task DeleteEntityAsync(Guid id)
        {
            try
            {
                // Find the entity by its ID
                var entity = await _context.Entity
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (entity == null)
                {
                    throw new Exception("Entity not found");
                }

                // Check if there are any associated drivers
                var entityDrivers = await _context.EntityDriver
                    .AnyAsync(ed => ed.EntityId == id);
                if (entityDrivers)
                {
                    throw new Exception("Cannot delete entity because there are associated drivers");
                }

                // Check if there are any associated routes
                var routes = await _context.Route
                    .AnyAsync(r => r.EntityId == id);
                if (routes)
                {
                    throw new Exception("Cannot delete entity because there are associated routes");
                }

                var priceTables = await _context.PriceTable
                    .AnyAsync(r => r.EntityId == id);
                if (priceTables)
                {
                    throw new Exception("Cannot delete entity because there are price tables");
                }

                // If no associated records were found, proceed to delete the entity
                _context.Entity.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the entity");
                throw new Exception("An error occurred while deleting the entity", ex);
            }
        }


        // Buscar uma entidade por Id
        public async Task<OneEntity> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _context.Entity
                    .Include(e => e.Region) // Carregar a região associada, caso seja necessário
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) throw new Exception("Entity not found");

                var entityResponse = _mapper.Map<OneEntity>(entity);
                return entityResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding the entity");
                throw new Exception("An error occurred while finding the entity");
            }
        }

        public async Task<OneEntity> GetByUserIdAsync(Guid userId)
        {
            try
            {
                // Fetch the EntityId associated with the user from the EntityDriver table
                var entityDriver = await _context.EntityDriver
                    .FirstOrDefaultAsync(ed => ed.UserId == userId);

                if (entityDriver == null)
                {
                    throw new Exception("No associated entity found for the user");
                }

                // Use the EntityId from the EntityDriver table to fetch the Entity
                var entity = await _context.Entity
                    .Include(e => e.Region) // Load the associated region, if needed
                    .FirstOrDefaultAsync(e => e.Id == entityDriver.EntityId);

                if (entity == null)
                {
                    throw new Exception("Entity not found");
                }

                // Map the entity to the response model
                var entityResponse = _mapper.Map<OneEntity>(entity);
                return entityResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the entity");
                throw new Exception("An error occurred while retrieving the entity", ex);
            }
        }


        // Listar todas as entidades com filtros
        public async Task<List<ListEntity>> GetEntitiesAsync(FilterQuery? filter)
        {
            var prepEntities = _context.Entity
                .Include(e => e.Region) // Pode incluir a região para retornar o nome da região
                .ProjectTo<ListEntity>(_mapper.ConfigurationProvider);

            if (prepEntities == null) throw new Exception("Error getting entities");

            QueryHelper.ApplyListFilters(ref prepEntities, filter); // Aplica os filtros de query, caso existam

            var entities = await prepEntities.ToListAsync();

            if (entities == null || entities.Count == 0) return new List<ListEntity>();

            return entities;
        }

        // Atualizar informações de uma entidade
        public async Task UpdateEntityAsync(Guid id, PatchEntity request, Guid loggedUserId, string userRole)
        {
            try
            {
                var existingEntity = await _context.Entity.FindAsync(id);
                if (existingEntity == null) throw new Exception("Entity not found");

                // Restrict access for Entity Admins
                if (userRole == "2")
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId && ed.EntityId == id);

                    if (isEntityAdmin == null)
                    {
                        throw new Exception("You do not have permission to update this entity.");
                    }
                }

                // Map and update the fields
                existingEntity.Name = request.Name;
                existingEntity.Email = request.Email;
                existingEntity.Phone = request.Phone;
                existingEntity.Active = request.Active;
                existingEntity.About = request.About;
                existingEntity.WorkHours = request.WorkHours;

                // Update RegionId if provided and different
                if (request.RegionId != Guid.Empty && existingEntity.RegionId != request.RegionId)
                {
                    existingEntity.RegionId = request.RegionId;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the entity");
                throw new Exception("An error occurred while updating the entity", ex);
            }
        }

    }
}
