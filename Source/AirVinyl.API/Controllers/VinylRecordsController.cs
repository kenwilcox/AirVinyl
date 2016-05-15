using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using AirVinyl.DataAccessLayer;

namespace AirVinyl.API.Controllers
{
    public class VinylRecordsController: ODataController
    {
        private readonly AirVinylDbContext _context = new AirVinylDbContext();

        [HttpGet]
        [ODataRoute("VinylRecords")]
        public IHttpActionResult GetAllVinylRecords()
        {
            return Ok(_context.VinylRecords);
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}