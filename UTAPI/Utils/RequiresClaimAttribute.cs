using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UTAPI.Utils
{
    /// <summary>
    /// This attribute is used to enforce claim-based authorization on controller actions or entire controllers.
    /// It ensures that a user is authenticated and has a specific claim with an allowed value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresClaimAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _claimName;   // The name of the claim to check for.
        private readonly string[] _claimValues; // The allowed values for the specified claim.

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresClaimAttribute"/> class.
        /// </summary>
        /// <param name="claimName">The name of the claim to check for in the user's claims.</param>
        /// <param name="claimValues">The allowed values for the specified claim.</param>
        public RequiresClaimAttribute(string claimName, params string[] claimValues)
        {
            _claimName = claimName;
            _claimValues = claimValues;
        }

        /// <summary>
        /// The method that gets called to authorize the request.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Return Unauthorized if not authenticated
                context.Result = new UnauthorizedResult();
                return;
            }

            // Retrieve the specified claim from the user's claims
            var claim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == _claimName);

            // If the claim doesn't exist, forbid the request
            if (claim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // If the claim value doesn't match one of the allowed values, forbid the request
            if (!_claimValues.Contains(claim.Value))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
