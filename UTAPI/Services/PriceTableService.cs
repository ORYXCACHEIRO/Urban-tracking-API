using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;
using UTAPI.Requests.PriceTable;

namespace UTAPI.Services
{
    public class PriceTableService : IPriceTableServices
    {
        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<EntityServices> _logger; // facilita logging
        private readonly IMapper _mapper;

        public PriceTableService(DataContext context, ILogger<EntityServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Criação de uma nova Price Table
        public async Task<ListPriceTable> CreatePriceTableAsync(PostPriceTable request, Guid loggedUserId, string userRole)
        {
            try
            {

                // If the user is an entity admin, check if they belong to the same entity
                if (userRole == "2") // Entity Admin
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to create a price table for this entity.");

                    // Ensure the price table belongs to the same entity as the entity admin
                    if (request.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to create a price table for this entity.");
                }
                // If the user is a system admin (role == "3"), they can create the price table for any entity

                // Check if the entity exists (applicable for both roles)
                var entityExists = await _context.Entity
                    .AnyAsync(e => e.Id == request.EntityId);

                if (!entityExists)
                    throw new Exception("The specified entity does not exist");

                // Map the request to a PriceTable object and set it as inactive by default
                var priceTable = _mapper.Map<PriceTable>(request);
                priceTable.Active = false;

                // Add the price table to the context and save changes
                _context.PriceTable.Add(priceTable);
                await _context.SaveChangesAsync();

                // Return the created price table
                return _mapper.Map<ListPriceTable>(priceTable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the price table");
                throw new Exception("An error occurred while creating the price table", ex);
            }
        }


        // Exclusão de uma entidade
        public async Task DeletePriceTableAsync(Guid id, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the price table to be deleted
                var priceTable = await _context.PriceTable.FindAsync(id);

                if (priceTable == null)
                    throw new Exception("Price table not found");

                // If the user is an entity admin (role == "2"), check if the price table belongs to their entity
                if (userRole == "2")
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to delete this price table.");

                    // Check if the price table belongs to the same entity as the entity admin
                    if (priceTable.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to delete this price table for this entity.");
                }

                // Delete all related PriceTableContent entries
                var relatedPriceTableContents = _context.PriceTableContent
                    .Where(ptc => ptc.PriceTableId == id);

                _context.PriceTableContent.RemoveRange(relatedPriceTableContents);

                // Delete the price table
                _context.PriceTable.Remove(priceTable);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the price table");
                throw new Exception("An error occurred while deleting the price table", ex);
            }
        }


        // Listar todas as entidades com filtros
        public async Task<List<ListPriceTable>> GetPriceTablesAsync(FilterQuery? filter)
        {
            var prepPriceTables = _context.PriceTable
            .Include(pt => pt.Entity) // Pode incluir a entidade para retornar o nome da entidade
            .ProjectTo<ListPriceTable>(_mapper.ConfigurationProvider);

            if (prepPriceTables == null) throw new Exception("Error getting price tables");

            QueryHelper.ApplyListFilters(ref prepPriceTables, filter); // Aplica os filtros de query, caso existam

            var priceTables = await prepPriceTables.ToListAsync();

            if (priceTables == null || priceTables.Count == 0) return new List<ListPriceTable>();

            return priceTables;
        }

        public async Task<List<ListPriceTable>> GetPriceTableByEntityIdAsync(Guid entityId, FilterQuery? filter, string userRole)
        {
            try
            {
                // Prepare the query to fetch price tables associated with the provided entityId
                var prepPriceTables = _context.PriceTable
                    .Where(pt => pt.EntityId == entityId);

                // Filter for active price tables if the user role is "0" or "1"
                if (userRole == "0" || userRole == "1")
                {
                    prepPriceTables = prepPriceTables.Where(pt => pt.Active == true);
                }

                // Project the query to the desired output model
                var projectedPriceTables = prepPriceTables.ProjectTo<ListPriceTable>(_mapper.ConfigurationProvider);

                // Apply query filters if provided
                if (filter != null)
                {
                    QueryHelper.ApplyListFiltersWithoutVars(ref projectedPriceTables, filter, new[] { "EntityId", "Active" });
                }

                // Execute the query and get the results
                var priceTables = await projectedPriceTables.ToListAsync();

                // Return an empty list if no records are found
                return priceTables ?? new List<ListPriceTable>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the price tables");
                throw new Exception("An error occurred while retrieving the price tables", ex);
            }
        }


        public async Task<ListPriceTable> GetPriceTableByIdAsync(Guid id, Guid loggedUserId, string userRole)
        {
            try
            {
                // Prep Fetch the price table
                var prepPriceTable = _context.PriceTable
                    .Where(pt => pt.Id == id);

                // Check if the user is an entity admin
                if (userRole == "2")
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                    {
                        throw new Exception("User is not an entity admin");
                    }

                    // Only return the price table if it belongs to the same entity
                    prepPriceTable = prepPriceTable.Where(pt => pt.EntityId == isEntityAdmin.EntityId);
                }

                // Fetch the price table, ensuring it exists
                var priceTable = await prepPriceTable.FirstOrDefaultAsync();

                if (priceTable == null)
                {
                    throw new Exception("Price table not found");
                }

                // Map to ListPriceTable and return
                var mappedPriceTable = _mapper.Map<ListPriceTable>(priceTable);

                return mappedPriceTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the price table");
                throw new Exception("An error occurred while retrieving the price table", ex);
            }
        }



        // Atualizar informações de uma entidade
        public async Task UpdatePriceTableAsync(Guid id, PatchPriceTable request, Guid loggedUserId, string userRole)
        {
            try
            {
                // Find the existing price table
                var existingPriceTable = await _context.PriceTable.FindAsync(id);

                if (existingPriceTable == null)
                    throw new Exception("Price table not found");

                // If the user is an entity admin (role == "2"), check if the price table belongs to the same entity
                if (userRole == "2")
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to edit this price table.");

                    // If the price table doesn't belong to the user's entity, prevent editing
                    if (existingPriceTable.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to edit this price table for this entity.");
                }

                // Update the price table properties
                existingPriceTable.Name = request.Name;
                existingPriceTable.NLine = request.NLine;
                existingPriceTable.NColumn = request.NColumn;
                existingPriceTable.Active = request.Active;

                // If the request contains a new EntityId, validate and update it
                if (request.EntityId != Guid.Empty && existingPriceTable.EntityId != request.EntityId)
                {
                    var entityExists = await _context.Entity.AnyAsync(e => e.Id == request.EntityId);
                    if (!entityExists) throw new Exception("The specified entity does not exist");

                    existingPriceTable.EntityId = request.EntityId;
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing the price table");
                throw new Exception("An error occurred while editing the price table", ex);
            }
        }

    }
}
