using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;
using UTAPI.Requests.PriceTable;
using UTAPI.Requests.PriceTableContent;

namespace UTAPI.Services
{
    public class PriceTableContentService : IPriceTableContentServices
    {
        private readonly DataContext _context; // interage com a bd
        private readonly ILogger<EntityServices> _logger; // facilita logging
        private readonly IMapper _mapper;

        public PriceTableContentService(DataContext context, ILogger<EntityServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Criação de uma nova Price Table
        public async Task<ListPriceTableContent> CreatePriceTableContentAsync(PostPriceTableContent request, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the price table
                var priceTable = await _context.PriceTable.FirstOrDefaultAsync(x => x.Id == request.PriceTableId);
                if (priceTable == null) throw new Exception("The specified price table does not exist");

                // Check if the user is an entity admin and restrict the price table entity
                if (userRole == "2") // Entity Admin
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to create price table content for this entity.");

                    // Ensure the price table belongs to the same entity as the entity admin
                    if (priceTable.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to create price table content for this price table.");
                }
                // If the user is a system admin (role == "3"), they can create price table content for any entity

                // Check if line and column already exist in the price table content
                var checkIfLineColExist = await _context.PriceTableContent
                    .FirstOrDefaultAsync(x => x.Line == request.Line && x.Col == request.Col && x.PriceTableId == request.PriceTableId);
                if (checkIfLineColExist != null)
                    throw new Exception("A price table content with that line and column already exists");

                // Check if the line and column are within the bounds of the price table
                if (request.Line > priceTable.NLine || request.Col > priceTable.NColumn)
                    throw new Exception("Line or column exceeds the bounds of the price table");

                // Map the request to PriceTableContent and add it to the context
                var priceTableContent = _mapper.Map<PriceTableContent>(request);
                _context.PriceTableContent.Add(priceTableContent);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Map the created price table content to ListPriceTableContent and return it
                var listPriceTableContent = _mapper.Map<ListPriceTableContent>(priceTableContent);

                return listPriceTableContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the price table content");
                throw new Exception("An error occurred while creating the price table content", ex);
            }
        }


        // Exclusão de uma entidade
        public async Task DeletePriceTableContentAsync(Guid id, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the price table content to delete
                var priceTableContent = await _context.PriceTableContent.FindAsync(id);
                if (priceTableContent == null)
                    throw new Exception("Price table content not found");

                // Fetch the associated price table
                var priceTable = await _context.PriceTable
                    .FirstOrDefaultAsync(pt => pt.Id == priceTableContent.PriceTableId);

                if (priceTable == null)
                    throw new Exception("Price table not found");

                // Apply role-based restriction logic
                if (userRole == "2") // Entity Admin
                {
                    // Ensure the price table belongs to the same entity as the entity admin
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to delete price table content for this entity.");

                    // Check if the price table belongs to the entity the admin manages
                    if (priceTable.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to delete price table content from this price table.");
                }

                // If the user is a system admin (role == "3"), they can delete content from any price table

                // Proceed to delete the price table content
                _context.PriceTableContent.Remove(priceTableContent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the price table content");
                throw new Exception("An error occurred while deleting the price table content", ex);
            }
        }


        // Listar todas as entidades com filtros
        public async Task<List<ListPriceTableContent>> GetPriceTablesContentAsync(FilterQuery? filter)
        {
            var prepPriceTablesContent = _context.PriceTableContent
            .Include(t => t.PriceTable)
            .ProjectTo<ListPriceTableContent>(_mapper.ConfigurationProvider);

            if (prepPriceTablesContent == null) throw new Exception("Error getting price tables");

            QueryHelper.ApplyListFilters(ref prepPriceTablesContent, filter); // Aplica os filtros de query, caso existam

            var priceTablesContent = await prepPriceTablesContent.ToListAsync();

            if (priceTablesContent == null || priceTablesContent.Count == 0) return new List<ListPriceTableContent>();

            return priceTablesContent;
        }

        public async Task<List<ListPriceTableContent>> GetPriceTableContentByPriceTableIdAsync(Guid priceTableId, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the price table to check entity ownership
                var priceTable = await _context.PriceTable
                    .FirstOrDefaultAsync(pt => pt.Id == priceTableId);

                if (priceTable == null)
                    throw new Exception("Price table not found");

                // Role-based restrictions
                if (userRole == "2") // Entity Admin
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to view price table content for this entity.");

                    // Check if the price table belongs to the same entity as the entity admin
                    if (priceTable.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to view price table content for this price table.");
                }

                // Fetch the price table content for the given price table ID
                var priceTableContentList = await _context.PriceTableContent
                    .Where(ptc => ptc.PriceTableId == priceTableId)
                    .ToListAsync();

                if (priceTableContentList == null || !priceTableContentList.Any()) return new List<ListPriceTableContent>();

                // Map the result to a ListPriceTableContent (DTO or ViewModel)
                var result = _mapper.Map<List<ListPriceTableContent>>(priceTableContentList);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the price table content");
                throw new Exception("An error occurred while retrieving the price table content", ex);
            }
        }

        // Atualizar informações de uma entidade
        public async Task UpdatePriceTableContentAsync(Guid id, PatchPriceTableContent request, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the price table content to update
                var existingPriceTableContent = await _context.PriceTableContent.FindAsync(id);
                if (existingPriceTableContent == null)
                    throw new Exception("Price table content not found");

                // Fetch the associated price table to check its entity and bounds
                var priceTable = await _context.PriceTable
                    .FirstOrDefaultAsync(pt => pt.Id == existingPriceTableContent.PriceTableId);

                if (priceTable == null)
                    throw new Exception("Price table not found");

                // Apply role-based restriction logic
                if (userRole == "2") // Entity Admin
                {
                    var isEntityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isEntityAdmin == null)
                        throw new Exception("You are not authorized to update price table content for this entity.");

                    // Check if the price table belongs to the same entity as the entity admin
                    if (priceTable.EntityId != isEntityAdmin.EntityId)
                        throw new Exception("You are not authorized to update price table content from this price table.");
                }

                // Validate that the line and column are within the bounds of the price table
                if (request.Line > priceTable.NLine || request.Col > priceTable.NColumn)
                    throw new Exception("Line or column exceeds the bounds of the price table");

                // Check if the specified line and column are already occupied
                var existingContent = await _context.PriceTableContent
                    .FirstOrDefaultAsync(x => x.PriceTableId == priceTable.Id && x.Line == request.Line && x.Col == request.Col);

                if (existingContent != null && existingContent.Id != id)
                    throw new Exception("The specified line and column are already occupied");

                // Update the price table content
                existingPriceTableContent.Content = request.Content;
                existingPriceTableContent.Line = request.Line;
                existingPriceTableContent.Col = request.Col;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the price table content");
                throw new Exception("An error occurred while updating the price table content", ex);
            }
        }

    }
}
