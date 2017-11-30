using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicStore.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MusicStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppSettings _appSettings;

        public StoreController(MusicStoreContext dbContext, IOptions<AppSettings> options)
        {
            DbContext = dbContext;
            _appSettings = options.Value;
        }

        public MusicStoreContext DbContext { get; }

        //
        // GET: /Store/
        public async Task<IActionResult> Index()
        {
            var genres = await DbContext.Genres.ToListAsync();

            return View(genres);
        }


        public IActionResult CustomerPage()
        {
            //var genres = await DbContext.Genres.ToListAsync();

            //return View(genres);
            //var response = "";
            //using (var client = new System.Net.Http.HttpClient())
            //{
            //    response =
            //        await client.GetStringAsync("https://employee.cglean.com/api/values");
            //    // The response object is a string that looks like this:
            //    // "{ message: 'Hello world!' }"
            //}
            return View("~/Views/Home/Employee.cshtml");
        }
        public IActionResult JavaPage()
        {
            
            return View("~/Views/Home/Customer.cshtml");
        }
        public async Task<IActionResult> Customerdetails()
        {
            //var genres = await DbContext.Genres.ToListAsync();

            //return View(genres);
            var response = "";
            using (var client = new System.Net.Http.HttpClient())
            {
                response =
                    await client.GetStringAsync("https://employee.cglean.com/api/values");
                // The response object is a string that looks like this:
                // "{ message: 'Hello world!' }"
            }
            return View("~/Views/Home/Customer.cshtml",response);
        }

        public async Task<IActionResult> MicroServiceDemo(int id)
        {
            //return View(genres);
            var response = "";
            //string[] arr;
            Dictionary<string, string> _Data = new Dictionary<string, string>();
            using (var client = new System.Net.Http.HttpClient())
            {
                response =
                //   await client.GetStringAsync("https://employee.cglean.com/api/values/1");
                await client.GetStringAsync("https://employee.cglean.com/api/values/"+id.ToString());
               // await client.GetStringAsync("http://localhost:52419/api/values/"+id.ToString());
                // The response object is a string that looks like this:
                // "{ message: 'Hello world!' }"
               _Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(response.ToString());
                // arr = JsonConvert.DeserializeObject<string[]>(response);
            }
            return View("~/Views/Home/Employeedetails.cshtml",_Data);
        }
        //
        // GET: /Store/Browse?genre=Disco
        public async Task<IActionResult> Browse(string genre)
        {
            // Retrieve Genre genre and its Associated associated Albums albums from database
            var genreModel = await DbContext.Genres
                .Include(g => g.Albums)
                .Where(g => g.Name == genre)
                .FirstOrDefaultAsync();

            if (genreModel == null)
            {
                return NotFound();
            }

            return View(genreModel);
        }

        public async Task<IActionResult> Details(
            [FromServices] IMemoryCache cache,
            int id)
        {
            var cacheKey = string.Format("album_{0}", id);
            Album album;
            if (!cache.TryGetValue(cacheKey, out album))
            {
                album = await DbContext.Albums
                                .Where(a => a.AlbumId == id)
                                .Include(a => a.Artist)
                                .Include(a => a.Genre)
                                .FirstOrDefaultAsync();

                if (album != null)
                {
                    if (_appSettings.CacheDbResults)
                    {
                        //Remove it from cache if not retrieved in last 10 minutes
                        cache.Set(
                            cacheKey,
                            album,
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                    }
                }
            }

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }
    }
}