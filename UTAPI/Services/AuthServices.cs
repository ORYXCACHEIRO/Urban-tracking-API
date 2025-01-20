using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.Auth;
using UTAPI.Requests.User;

namespace UTAPI.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<AuthServices> _logger; // facilita logging
        private readonly IMapper _mapper;
        private readonly IPasswordHelper _passwordHelper;

        public AuthServices(DataContext context, ILogger<AuthServices> logger, IMapper mapper, IPasswordHelper passwordHelper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _passwordHelper = passwordHelper;
        }

        public async Task<User> CheckUserAsync(Login request)
        {
            try
            {
                var checkUser = await _context.User.Where(x => x.Email == request.email).FirstAsync();

                if (checkUser == null) throw new Exception("Email or password are incorrect");

                var checkPassword = _passwordHelper.VerifyPassword(checkUser.Password, request.password);

                if(!checkPassword) throw new Exception("Email or password are incorrect");

                return checkUser;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while trying to login");
                throw new Exception("An error occured while trying to login");
            }
        }

        public async Task RegisterAsync(Register request)
        {
            try
            {

                var checkIfEmailPresent = await _context.User.FirstOrDefaultAsync(x => x.Email == request.Email);

                if (checkIfEmailPresent != null) throw new Exception("An user with that email already exisits");

                var user = _mapper.Map<User>(request);

                user.Password = _passwordHelper.HashPassword(user.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.Active = true;
                user.Role = "0";
                _context.User.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while creating the user");
                throw new Exception("An error occured while creating the user");
            }
        }

        public async Task<OneUser> GetMeInfoAsync(Guid userId)
        {
            try
            {

                // Fetch the user by ID
                var res = await _context.User.FindAsync(userId);
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
    }
}
