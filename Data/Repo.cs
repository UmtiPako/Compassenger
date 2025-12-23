using Compassenger.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compassenger.Data
{
    public class Repo
    {
        private readonly AppDbContext _context;
        
        // Basit in-memory cache (Repo Transient oldugu icin static olmali)
        private static List<Waypoint>? _memoryCache;

        public Repo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddWaypointAsync(Waypoint waypoint)
        {
            try
            {
                await _context.savedWaypoints.AddAsync(waypoint);
                await _context.SaveChangesAsync();

                // Cache guncelle
                _memoryCache?.Add(waypoint);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Message: " + ex.Message);
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return (false, message);
            }
        }

        public async Task<List<Waypoint>> GetAllWaypointsAsync(bool forceRefresh = false)
        {
            try
            {
                if (_memoryCache != null && !forceRefresh)
                {
                    return _memoryCache;
                }

                var list = await _context.savedWaypoints.ToListAsync();
                _memoryCache = list;

                return list;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Message: " + ex.Message);
                return new List<Waypoint>();
            }
        }

        public async Task<List<Waypoint>> GetWaypointsPagedAsync(int skip, int take)
        {
            try
            {
                return await _context.savedWaypoints
                                     .OrderByDescending(x => x.Id) // En son eklenenler ustte olsun
                                     .Skip(skip)
                                     .Take(take)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Pagination Error: " + ex.Message);
                return new List<Waypoint>();
            }
        }

        public async Task<bool> RemoveWaypointAsync(string Name)
        {
            try
            {
                var waypointToRemove = await _context.savedWaypoints.FirstOrDefaultAsync(w => w.Name == Name);

                if (waypointToRemove != null)
                {
                    _context.savedWaypoints.Remove(waypointToRemove);
                    await _context.SaveChangesAsync();

                    // Cache'den de sil
                    if (_memoryCache != null)
                    {
                        var cachedItem = _memoryCache.FirstOrDefault(w => w.Name == Name);
                        if (cachedItem != null) _memoryCache.Remove(cachedItem);
                    }

                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Message: " + ex.Message);
                return false;
            }
        }
    }
}
