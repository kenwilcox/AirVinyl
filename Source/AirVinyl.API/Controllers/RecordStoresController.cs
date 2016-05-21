﻿using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class RecordStoresController: ODataController
    {
        private AirVinylDbContext _context = new AirVinylDbContext();

        // GET odata/RecordStores
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_context.RecordStores);
        }

        // GET odata/RecordStores(key)
        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            var recordStores = _context.RecordStores.Where(r => r.RecordStoreId == key);
            if (!recordStores.Any())
            {
                return NotFound();
            }
            return Ok(SingleResult.Create(recordStores));
        }

        [HttpGet]
        [ODataRoute("RecordStores({key})/Tags")]
        [EnableQuery]
        public IHttpActionResult GetRecordStoreTagsProperty([FromODataUri] int key)
        {
            // no include necessary for EF - Tags ins't a navigation property
            // in the entity model
            var recordStore = _context.RecordStores.FirstOrDefault(r => r.RecordStoreId == key);
            if (recordStore == null)
            {
                return NotFound();
            }
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var collectionPropertyValue = recordStore.GetValue(collectionPropertyToGet);

            // return the collection of tags
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [ODataRoute("RecordStores({key})/AirVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
        public bool IsHighRated([FromODataUri] int key, int minimumRating)
        {
            // get the RecordStore
            var recordStore = _context.RecordStores
                .FirstOrDefault(p => p.RecordStoreId == key
                    && p.Ratings.Any()
                    && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating);

            return (recordStore != null);
        }

        [HttpGet]
        [ODataRoute("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={personIds})")]
        public IHttpActionResult AreRatedBy([FromODataUri] IEnumerable<int> personIds)
        {
            // get the RecordStores
            var recordStores = _context.RecordStores
                .Where(p => p.Ratings.Any(r => personIds.Contains(r.RatedBy.PersonId)));

            return this.CreateOKHttpActionResult(recordStores);
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}