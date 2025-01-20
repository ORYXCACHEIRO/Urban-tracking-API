using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Interfaces;
using AutoMapper.QueryableExtensions;
using UTAPI.Requests.EntityDriver;
using UTAPI.Models;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class EntityDriverServices : IEntityDriverServices
    {
        private readonly DataContext _context;
        private readonly ILogger<EntityDriverServices> _logger;
        private readonly IMapper _mapper;

        public EntityDriverServices(DataContext context, ILogger<EntityDriverServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ListEntityDriver> CreateEntityDriverAsync(PostEntityDriver request, Guid loggedUserId, string userRole)
        {
            try
            {
                // Check if the user to be associated is already linked to another entity
                var existingEntityDriver = await _context.EntityDriver.FirstOrDefaultAsync(ed => ed.UserId == request.UserId);
                if (existingEntityDriver != null)
                {
                    throw new Exception("The user is already associated with another entity");
                }

                // Check if the user being inserted has role = 1 (Driver) or role = 2 (Entity Admin)
                var user = await _context.User.FirstOrDefaultAsync(u => u.Id == request.UserId);
                if (user == null || (user.Role != "1" && user.Role != "2"))
                {
                    throw new Exception("The user does not have the required 'Driver' role");
                }

                // Verify the entity is active
                var entity = await _context.Entity.FirstOrDefaultAsync(e => e.Id == request.EntityId);
                if (entity == null || !entity.Active)
                {
                    throw new Exception("The specified entity does not exist or is not active");
                }

                // Restrict actions based on the role of the logged-in user
                if (userRole == "2") // Entity Admin
                {
                    // Verify if the logged user is an entity admin and get the associated entity
                    var entityAdmin = await _context.EntityDriver.FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);
                    if (entityAdmin == null)
                    {
                        throw new Exception("The logged-in user is not authorized to create entity drivers");
                    }

                    // Ensure the entity admin can only associate users to their entity
                    if (entityAdmin.EntityId != request.EntityId)
                    {
                        throw new Exception("You can only associate drivers with your own entity");
                    }

                    if (user.Role == "2")
                    {
                        throw new Exception("Only system admins can add this user to this entity");
                    }
                }
                else if (userRole != "3") // Sys Admin
                {
                    throw new Exception("Unauthorized action. Only Entity Admins or Sys Admins can create entity drivers");
                }

                // Proceed to create the EntityDriver record
                var entityDriver = _mapper.Map<EntityDriver>(request);
                _context.EntityDriver.Add(entityDriver);
                await _context.SaveChangesAsync();

                // Fetch and return the created record
                var createdEntityDriver = await _context.EntityDriver
                    .Include(ed => ed.Entity) // Include related entity data, if necessary
                    .FirstOrDefaultAsync(ed => ed.Id == entityDriver.Id);

                if (createdEntityDriver == null)
                {
                    throw new Exception("Failed to retrieve the created entity driver record");
                }

                return _mapper.Map<ListEntityDriver>(createdEntityDriver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the entity driver");
                throw new Exception("An error occurred while creating the entity driver", ex);
            }
        }


        public async Task DeleteEntityDriverAsync(Guid id, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the entity driver record to be deleted
                var entityDriver = await _context.EntityDriver
                    .Include(ed => ed.Entity) // Include entity data for role validation
                    .FirstOrDefaultAsync(ed => ed.Id == id);

                if (entityDriver == null) throw new Exception("Entity driver not found");

                // Check if the user being inserted has role = 1 (Driver) or role = 2 (Entity Admin)
                var targetUser = await _context.User.FirstOrDefaultAsync(u => u.Id == entityDriver.UserId);
                if (targetUser == null)
                {
                    throw new Exception("The user is not valid");
                }

                // Restrict actions based on the role of the logged-in user
                if (userRole == "2") // Entity Admin
                {
                    // Verify if the logged-in user is an entity admin
                    var entityAdmin = await _context.EntityDriver.FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);
                    if (entityAdmin == null)
                    {
                        throw new Exception("The logged-in user is not authorized to delete entity drivers");
                    }

                    // Ensure the entity admin can only delete drivers from their own entity
                    if (entityAdmin.EntityId != entityDriver.EntityId)
                    {
                        throw new Exception("You can only delete drivers associated with your own entity");
                    }

                    if (targetUser.Role == "2")
                    {
                        throw new Exception("Only system admins can delete this user to this entity");
                    }
                }

                // Proceed to delete the EntityDriver record
                _context.EntityDriver.Remove(entityDriver);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the entity driver");
                throw new Exception("An error occurred while deleting the entity driver", ex);
            }
        }


        public async Task<List<ListEntityDriver>> GetEntityDriverByUserIdAsync(Guid userId, Guid loggedUserId, string userRole)
        {
            try
            {
                IQueryable<EntityDriver> prepEntityDrivers;

                if (userRole == "2") // Entity Admin
                {
                    // Verify if the logged-in user is associated with an entity
                    var entityAdmin = await _context.EntityDriver.FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);
                    if (entityAdmin == null)
                    {
                        throw new Exception("The logged-in user is not authorized to view entity drivers");
                    }

                    // Restrict to drivers within the admin's entity
                    prepEntityDrivers = _context.EntityDriver
                        .Where(ed => ed.EntityId == entityAdmin.EntityId && ed.UserId == userId);
                }
                else if (userRole == "3") // Sys Admin
                {
                    // Sys Admin can fetch any driver associated with the given userId
                    prepEntityDrivers = _context.EntityDriver
                        .Where(ed => ed.UserId == userId);
                }
                else
                {
                    throw new Exception("Unauthorized action. Only Entity Admins or Sys Admins can view entity drivers");
                }

                // Map to DTO
                var mappedEntityDrivers = prepEntityDrivers.ProjectTo<ListEntityDriver>(_mapper.ConfigurationProvider);

                // Execute query and retrieve results
                var entityDrivers = await mappedEntityDrivers.ToListAsync();

                // Return an empty list if no drivers are found
                if (entityDrivers == null || entityDrivers.Count == 0)
                {
                    return new List<ListEntityDriver>();
                }

                return entityDrivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the entity drivers");
                throw new Exception("An error occurred while retrieving the entity drivers", ex);
            }
        }


        public async Task<List<ListEntityDriver>> GetEntityDriversByEntityIdAsync(Guid entityId, Guid loggedUserId, string userRole)
        {
            try
            {
                IQueryable<EntityDriver> prepEntityDrivers;

                if (userRole == "2") // Entity Admin
                {
                    // Verify if the logged-in user is associated with the specified entity
                    var entityAdmin = await _context.EntityDriver.FirstOrDefaultAsync(ed => ed.UserId == loggedUserId && ed.EntityId == entityId);
                    if (entityAdmin == null)
                    {
                        throw new Exception("The logged-in user is not authorized to view drivers for this entity");
                    }

                    // Restrict to drivers within the specified entity
                    prepEntityDrivers = _context.EntityDriver
                        .Where(ed => ed.EntityId == entityId)
                        .Where(ed => ed.UserId != loggedUserId)
                        .Include(ed => ed.User);
                }
                else if (userRole == "3") // Sys Admin
                {
                    // Sys Admin can fetch any drivers for the given entityId
                    prepEntityDrivers = _context.EntityDriver
                        .Where(ed => ed.EntityId == entityId)
                        .Include(ed => ed.User);
                }
                else
                {
                    throw new Exception("Unauthorized action. Only Entity Admins or Sys Admins can view entity drivers");
                }

                // Map to DTO
                var mappedEntityDrivers = prepEntityDrivers.ProjectTo<ListEntityDriver>(_mapper.ConfigurationProvider);

                // Execute query and retrieve results
                var entityDrivers = await mappedEntityDrivers.ToListAsync();

                // Return an empty list if no drivers are found
                if (entityDrivers == null || entityDrivers.Count == 0)
                {
                    return new List<ListEntityDriver>();
                }

                return entityDrivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving entity drivers");
                throw new Exception("An error occurred while retrieving entity drivers", ex);
            }
        }

        public async Task<List<ListEntityDriver>> GetEntityDriversAsync(Guid loggedUserId, string userRole, FilterQuery? filter)
        {
            try
            {
                IQueryable<EntityDriver> prepEntityDrivers;
                
                if(userRole != "3") throw new Exception("Unauthorized action. Sys Admins can use service");
                
                prepEntityDrivers = _context.EntityDriver
                        .Include(ed => ed.User);

                QueryHelper.ApplyListFilters(ref prepEntityDrivers, filter);

                // Map to DTO
                var mappedEntityDrivers = prepEntityDrivers.ProjectTo<ListEntityDriver>(_mapper.ConfigurationProvider);

                // Execute query and retrieve results
                var entityDrivers = await mappedEntityDrivers.ToListAsync();

                // Return an empty list if no drivers are found
                if (entityDrivers == null || entityDrivers.Count == 0)
                {
                    return new List<ListEntityDriver>();
                }

                return entityDrivers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving entity drivers");
                throw new Exception("An error occurred while retrieving entity drivers", ex);
            }
        }

    }
}
