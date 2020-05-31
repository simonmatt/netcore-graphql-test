using HotChocolate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netcore_graphql_test
{
    public class LocationQueries
    {
        /// <summary>
        /// Return a list of all locations
        /// Notice the [Service]. It's an auto look-up from HotChocolate
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public async Task<List<Location>> GetLocations([Service] MyDbContext dbContext) =>
            await dbContext.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync();

        
        public async Task<List<Location>> GetLocation([Service] MyDbContext dbContext, string code) =>
            await dbContext.Locations
            .AsNoTracking()
            .Where(l => l.Code == code)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }
}