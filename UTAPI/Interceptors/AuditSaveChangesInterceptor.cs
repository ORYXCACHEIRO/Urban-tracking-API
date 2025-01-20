using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UTAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using UTAPI.Data;

namespace UTAPI.Interceptors
{
    /// <summary>
    /// Interceptor for auditing changes to entities during SaveChanges operations.
    /// Tracks modified or deleted entities and records an audit log for each change.
    /// </summary>
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private static bool _isSavingChanges = false; // Static flag to prevent recursion during SaveChanges calls

        /// <summary>
        /// Constructor to initialize the interceptor with HTTP context and service provider.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the HTTP context (for user claims).</param>
        /// <param name="serviceProvider">Service provider for resolving dependencies, including DataContext.</param>
        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Retrieves the logged-in user's ID from the HTTP context claims.
        /// Returns an empty Guid if no user is logged in or if the claim is missing/invalid.
        /// </summary>
        /// <returns>The user's ID as a Guid, or an empty Guid if no valid ID is found.</returns>
        private Guid GetLoggedInUserId()
        {
            var context = _httpContextAccessor.HttpContext;
            var user = context?.User;

            if (user == null)
            {
                return Guid.Empty; // No user logged in
            }

            var userIdString = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Asynchronously intercepts the saving of changes in the DbContext.
        /// Tracks modified or deleted entities and records audit logs after successful entity changes.
        /// </summary>
        /// <param name="eventData">Event data related to the save changes operation.</param>
        /// <param name="result">The result of the interception.</param>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
        /// <returns>Returns the result of the SaveChanges operation after performing audit logging.</returns>
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken)
        {
            // Prevent recursion if saving changes is already in progress
            if (_isSavingChanges)
            {
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            try
            {
                // Mark saving changes as in progress
                _isSavingChanges = true;

                var dbContext = eventData.Context;
                if (dbContext == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

                var userId = GetLoggedInUserId(); // Get the logged-in user's ID
                var audits = new List<Audit>(); // List to collect the generated audit logs

                // Loop through all entries in the ChangeTracker to find modified or deleted entities
                foreach (var entry in dbContext.ChangeTracker.Entries())
                {
                    // Only track entities that are modified or deleted
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        var audit = CreateAudit(entry, userId); // Create audit log for the entity
                        if (audit != null)
                        {
                            audits.Add(audit); // Add the audit log to the list
                        }
                    }
                }

                // Save the changes to the entities first
                var entityChangesSaved = await base.SavingChangesAsync(eventData, result, cancellationToken);

                // Resolve the DataContext dynamically from the service provider
                var context = _serviceProvider.GetRequiredService<DataContext>();

                // Save audit logs after entity changes are saved
                if (audits.Any())
                {
                    await context.Set<Audit>().AddRangeAsync(audits); // Add audit entries to the context
                    await context.SaveChangesAsync(cancellationToken); // Commit the audit changes to the database
                }

                return entityChangesSaved; // Return the result of the entity changes save operation
            }
            finally
            {
                // Reset the flag after the save operation is completed
                _isSavingChanges = false;
            }
        }

        /// <summary>
        /// Creates an audit log entry for a modified or deleted entity.
        /// </summary>
        /// <param name="entry">The entry representing the entity in the change tracker.</param>
        /// <param name="userId">The ID of the user performing the change.</param>
        /// <returns>The created Audit log entry, or null if the entity doesn't have a valid primary key or user ID.</returns>
        private Audit CreateAudit(EntityEntry entry, Guid userId)
        {
            if (userId == Guid.Empty) return null; // Skip audit if user ID is not valid

            var entity = entry.Entity;

            // Retrieve the primary key using EF Core metadata
            var keyProperty = entry.Metadata.FindPrimaryKey();

            if (keyProperty == null || keyProperty.Properties.Count == 0)
            {
                Console.WriteLine($"No primary key found for entity: {entity.GetType().Name}");
                return null; // Return null if no primary key is found for the entity
            }

            // We assume that the primary key is of type Guid (adjust as necessary)
            var keyValue = entry.Property(keyProperty.Properties[0].Name).CurrentValue;

            // Ensure the key value is a Guid
            if (keyValue == null || !(keyValue is Guid resourceId))
            {
                Console.WriteLine($"Primary key is not a Guid for entity: {entity.GetType().Name}");
                return null; // Return null if the primary key is not a Guid
            }

            // Create and return the audit log entry
            return new Audit
            {
                UserId = userId,
                Action = entry.State == EntityState.Modified ? "Update" : "Delete", // Set action type based on entity state
                Resource = entity.GetType().Name, // Store entity type name as resource
                ResourceId = resourceId, // Set resource ID (primary key)
                LogDate = DateTime.UtcNow // Set the log date to the current UTC time
            };
        }
    }
}
