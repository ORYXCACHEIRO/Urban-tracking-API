using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.User;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;
using UTAPI.Security;

namespace UTAPI.Services
{
    public class UserServices : IUserServices    
    {
        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<UserServices> _logger; // facilita logging
        private readonly IMapper _mapper;
        private readonly IPasswordHelper _passwordHelper;

        public UserServices(DataContext context, ILogger<UserServices> logger, IMapper mapper, IPasswordHelper passwordHelper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _passwordHelper = passwordHelper;
        }

        //Semnpre que a func tem driver no nome quer dizer que é uma função a ser usada pelo admin de entidade
        //Restantes funcs são usadas pelo o admin

        public async Task<OneUser> CreateDriverAsync(PostUser request, Guid loggedInUserId)
        {
            try
            {
                // Check if the email is already registered
                var checkIfEmailPresent = await _context.User.FirstOrDefaultAsync(x => x.Email == request.Email);

                if (checkIfEmailPresent != null)
                    throw new Exception("A user with that email already exists");

                // Fetch the entity associated with the logged-in admin
                var entityDriver = await _context.EntityDriver
                    .FirstOrDefaultAsync(ed => ed.UserId == loggedInUserId);

                if (entityDriver == null)
                    throw new Exception("Logged-in user is not associated with any entity");

                // Map the incoming request to the User entity
                var user = _mapper.Map<User>(request);

                // Generate a hashed password and populate other user details
                user.Password = _passwordHelper.HashPassword(user.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.Active = true;
                user.Role = "1"; // Role '1' indicates a driver
                _context.User.Add(user);

                // Save the new user to generate their UUID
                await _context.SaveChangesAsync();

                // Associate the newly created driver with the same entity
                var newEntityDriver = new EntityDriver
                {
                    UserId = user.Id,
                    EntityId = entityDriver.EntityId // Use the entity ID of the logged-in admin
                };
                _context.EntityDriver.Add(newEntityDriver);

                // Save changes for the association
                await _context.SaveChangesAsync();

                // Map the user back to the response DTO
                var response = _mapper.Map<OneUser>(user);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the driver");
                throw new Exception("An error occurred while creating the driver");
            }
        }


        public async Task<OneUser> CreateUserAsync(PostUser request)
        {
            try
            {

                var checkIfEmailPresent = await _context.User.FirstOrDefaultAsync(x => x.Email == request.Email);

                if(checkIfEmailPresent != null) throw new Exception("An user with that email already exisits");

                var user = _mapper.Map<User>(request);

                user.Password = _passwordHelper.HashPassword(user.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.Active = true;
                _context.User.Add(user);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<OneUser>(user);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while creating the user");
                throw new Exception("An error occured while creating the user");
            }
        }

        public async Task DeleteUserAsync(Guid id, Guid loggedInUserId)
        {
            try
            {
                // Ensure the user to delete exists
                var user = await _context.User.FindAsync(id);
                if (user == null) throw new Exception("The user does not exist");

                // Prevent deletion of an admin (Role 3)
                if (user.Role == "3") throw new Exception("Admin users cannot be deleted");

                // Prevent a user from deleting themselves
                if (id == loggedInUserId) throw new Exception("You cannot delete your own account");

                // Check if the user has the Driver role and is associated with an entity
                if (user.Role == "1")
                {
                    var driverAssociation = await _context.EntityDriver.AnyAsync(ed => ed.UserId == id);
                    if (driverAssociation) throw new Exception("Drivers associated with an entity cannot be deleted");
                }

                // Remove associations in other tables
                var entityDrivers = _context.EntityDriver.Where(ed => ed.UserId == id);
                var favouriteRoutes = _context.FavRoute.Where(fr => fr.UserId == id);
                var routeHistories = _context.RouteHistory.Where(rh => rh.UserId == id);
                var sessions = _context.Session.Where(s => s.UserId == id);

                // Remove associations
                _context.EntityDriver.RemoveRange(entityDrivers);
                _context.FavRoute.RemoveRange(favouriteRoutes);
                _context.RouteHistory.RemoveRange(routeHistories);
                _context.Session.RemoveRange(sessions);

                // Remove the user
                _context.User.Remove(user);

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user");
                throw new Exception("An error occurred while deleting the user");
            }
        }

        public async Task DeleteDriverAsync(Guid id, Guid loggedInUserId)
        {
            try
            {
                // Ensure the user to delete exists
                var user = await _context.User.FindAsync(id);
                if (user == null) throw new Exception("The user does not exist");

                // Ensure the logged-in user is an entity admin (role = 2)
                var admin = await _context.User.FindAsync(loggedInUserId);
                if (admin == null || admin.Role != "2") throw new Exception("Only an entity admin can delete a user");

                // Prevent deletion of an admin (Role 3)
                if (user.Role == "3") throw new Exception("Admin users cannot be deleted");

                // Prevent a user from deleting themselves
                if (id == loggedInUserId) throw new Exception("You cannot delete your own account");

                // Ensure the user to be deleted is a driver
                if (user.Role == "1") // Assuming "1" is the Driver role
                {
                    // Check if the driver belongs to the same entity as the admin
                    var driverEntity = await _context.EntityDriver
                        .Where(ed => ed.UserId == id)
                        .Select(ed => ed.EntityId)
                        .FirstOrDefaultAsync();

                    if (driverEntity == Guid.Empty)
                        throw new Exception("Driver is not associated with any entity");

                    var adminEntity = await _context.EntityDriver
                        .Where(ea => ea.UserId == loggedInUserId)
                        .Select(ea => ea.EntityId)
                        .FirstOrDefaultAsync();

                    if (driverEntity != adminEntity)
                        throw new Exception("You cannot delete a driver who does not belong to your entity");
                }

                // Remove associations in other tables
                var entityDrivers = _context.EntityDriver.Where(ed => ed.UserId == id);
                var favouriteRoutes = _context.FavRoute.Where(fr => fr.UserId == id);
                var routeHistories = _context.RouteHistory.Where(rh => rh.UserId == id);
                var sessions = _context.Session.Where(s => s.UserId == id);

                // Remove associations
                _context.EntityDriver.RemoveRange(entityDrivers);
                _context.FavRoute.RemoveRange(favouriteRoutes);
                _context.RouteHistory.RemoveRange(routeHistories);
                _context.Session.RemoveRange(sessions);

                // Remove the user
                _context.User.Remove(user);

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user");
                throw new Exception("An error occurred while deleting the user");
            }
        }


        public async Task<OneUser> GetByIdAsync(Guid id, Guid loggedInUserId, string role)
        {
            try
            {
                // Check if the logged-in user is an admin (role = 3) or the user is requesting their own information
                if (role != "3" && id != loggedInUserId)
                {
                    throw new Exception("User not found");
                }

                // Fetch the user by ID
                var res = await _context.User.FindAsync(id);
                if (res == null)
                {
                    throw new Exception("User not found");
                }

                var user = _mapper.Map<OneUser>(res);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user.");
                throw new Exception("An error occurred while retrieving the user.");
            }
        }


        public async Task<OneUser> GetDriverByIdAsync(Guid id, Guid loggedInUserId)
        {
            try
            {
                if (id == loggedInUserId)
                {
                    // Fetch the user and map to the OneUser DTO
                    var own = await _context.User.FindAsync(id);
                    if (own == null) throw new Exception("The user does not exist");

                    var ownDet = _mapper.Map<OneUser>(own);
                    return ownDet;
                }
                else
                {
                    // Fetch the entity of the logged-in user (Entity Admin)
                    var adminEntity = await _context.EntityDriver
                        .Where(ed => ed.UserId == loggedInUserId)
                        .Select(ed => ed.EntityId)
                        .FirstOrDefaultAsync();

                    if (adminEntity == Guid.Empty)
                        throw new Exception("You are not associated with any entity");

                    // Fetch the entity of the requested user
                    var driverEntity = await _context.EntityDriver
                        .Where(ed => ed.UserId == id)
                        .Select(ed => ed.EntityId)
                        .FirstOrDefaultAsync();

                    if (driverEntity == Guid.Empty || driverEntity != adminEntity)
                        throw new Exception("You are not authorized to view this user");

                    // Fetch the user and map to the OneUser DTO
                    var res = await _context.User.FindAsync(id);
                    if (res == null) throw new Exception("The user does not exist");

                    var user = _mapper.Map<OneUser>(res);
                    return user;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding the user");
                throw new Exception("An error occurred while finding the user");
            }
        }


        public async Task<List<ListUser>> GetUsersAsync(FilterQuery? filter)
        {
            var prepUsers = _context.User.ProjectTo<ListUser>(_mapper.ConfigurationProvider);

            if (prepUsers == null) throw new Exception("Error getting users");

            QueryHelper.ApplyListFilters(ref prepUsers, filter);

            var users = await prepUsers.ToListAsync();

            if (users == null || users.Count == 0) return new List<ListUser>();

            return users;
        }

        public async Task<List<ListUser>> GetDriversAsync(Guid loggedInUserId, FilterQuery? filter)
        {
            try
            {
                // Fetch the entity of the logged-in admin
                var adminEntity = await _context.EntityDriver
                    .Where(ed => ed.UserId == loggedInUserId)
                    .Select(ed => ed.EntityId)
                    .FirstOrDefaultAsync();

                if (adminEntity == Guid.Empty)
                    throw new Exception("You are not associated with any entity");

                // Filter users: only drivers (role = "1") from the same entity
                var prepUsers = _context.User
                    .Where(u => u.Role == "1" && _context.EntityDriver
                        .Any(ed => ed.UserId == u.Id && ed.EntityId == adminEntity))
                    .ProjectTo<ListUser>(_mapper.ConfigurationProvider);

                if (prepUsers == null) throw new Exception("Error getting users");

                // Apply additional filters (e.g., role, active status, etc.) from the query
                QueryHelper.ApplyListFilters(ref prepUsers, filter);

                // Convert to a list
                var users = await prepUsers.ToListAsync();

                // Return results or an empty list if none found
                if (users == null || users.Count == 0) return new List<ListUser>();

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users");
                throw new Exception("An error occurred while fetching users");
            }
        }

        public async Task<List<ListUser>> GetUnassociatedUsersAsync(Guid loggedUserId, string userRole, FilterQuery? filter)
        {
            try
            {
                // Verify that the logged-in user exists in the EntityDriver table
                var isLoggedUserAssociated = await _context.EntityDriver
                    .AnyAsync(ed => ed.UserId == loggedUserId);

                if (userRole == "2" && !isLoggedUserAssociated)
                {
                    throw new Utils.UnauthorizedAccessException("Logged-in user is not associated with any entity.");
                }

                // Query users with roles "1" (Driver) or "2" (Entity Admin)
                // who are not present in the EntityDriver table
                var prepUsers = _context.User
                    .Where(u => (u.Role == "1" || u.Role == "2") &&
                                !_context.EntityDriver.Any(ed => ed.UserId == u.Id))
                    .ProjectTo<ListUser>(_mapper.ConfigurationProvider);

                if (prepUsers == null) throw new Exception("Error fetching unassociated users");

                // Apply additional filters if provided
                QueryHelper.ApplyListFilters(ref prepUsers, filter);

                // Convert to a list
                var users = await prepUsers.ToListAsync();

                // Return results or an empty list if none found
                return users ?? new List<ListUser>();
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt while fetching unassociated users");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching unassociated users");
                throw new Exception("An error occurred while fetching unassociated users", ex);
            }
        }



        public async Task UpdateDriverAsync(Guid id, Guid loggedInAdminId, PatchUser request)
        {
            try
            {
                // Fetch the driver to update
                var oldUser = await _context.User.FindAsync(id);
                if (oldUser == null) throw new Exception("The user does not exist");

                // Ensure the logged-in user is an Entity Admin
                var adminEntity = await _context.EntityDriver
                    .Where(ed => ed.UserId == loggedInAdminId)
                    .Select(ed => ed.EntityId)
                    .FirstOrDefaultAsync();

                if (adminEntity == Guid.Empty)
                    throw new Exception("You are not associated with any entity");

                // Ensure the user being updated is associated with the same entity
                var driverEntity = await _context.EntityDriver
                    .Where(ed => ed.UserId == id)
                    .Select(ed => ed.EntityId)
                    .FirstOrDefaultAsync();

                if (driverEntity == Guid.Empty || driverEntity != adminEntity)
                    throw new Exception("You can only update drivers associated with your entity");

                // Map the incoming request to a new user object
                var newUser = _mapper.Map<PatchUser>(request);

                // Check if email is being updated and already exists
                if (!string.IsNullOrEmpty(newUser.Email) && newUser.Email != oldUser.Email)
                {
                    var checkIfEmailPresent = await _context.User
                        .Where(x => x.Email == newUser.Email)
                        .FirstOrDefaultAsync();

                    if (checkIfEmailPresent != null)
                        throw new Exception("A user with that email already exists");
                }

                // Update user details
                oldUser.Name = newUser.Name ?? oldUser.Name;
                oldUser.Email = newUser.Email ?? oldUser.Email;
                oldUser.Active = newUser.Active;

                if (!string.IsNullOrEmpty(request.Password) &&
                    !_passwordHelper.VerifyPassword(oldUser.Password, newUser.Password))
                {
                    // Only hash and update if the new password differs from the old one
                    oldUser.Password = _passwordHelper.HashPassword(newUser.Password);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing the user");
                throw new Exception("An error occurred while editing the user");
            }
        }


        public async Task UpdateUserAsync(Guid id,Guid logUser,string userRole, PatchUser request)
        {
            try
            {
                if (logUser != id && userRole != "3") throw new Exception("An error occured while editing the user");

                var newUser = _mapper.Map<PatchUser>(request);
                var oldUser = await _context.User.FindAsync(id);

                if (oldUser == null) throw new Exception("An error occured while editing the user");

                var checkIfEmailPresent = await _context.User.Where(x => x.Email == newUser.Email && x.Id != id).FirstOrDefaultAsync();

                if (checkIfEmailPresent != null) throw new Exception("An user with that email already exisits");

                if (logUser != id && userRole == "3"){
                    oldUser.Active = newUser.Active;
                    oldUser.Role = newUser.Role;
                }
                oldUser.Name = newUser.Name;
                oldUser.Email = newUser.Email;


                if (!string.IsNullOrEmpty(request.Password) &&
                    !_passwordHelper.VerifyPassword(oldUser.Password, newUser.Password)
                )
                {
                    // Only hash and update if the new password differs from the old one
                    oldUser.Password = _passwordHelper.HashPassword(newUser.Password);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while editing the user");
                throw new Exception("An error occured while editing the user");
            }
        }

    }
}
