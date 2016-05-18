using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using AirVinyl.DataAccessLayer;
using AirVinyl.API.Helpers;
using AirVinyl.Model;

namespace AirVinyl.API.Controllers
{
    public class PeopleController : ODataController
    {
        private readonly AirVinylDbContext _context = new AirVinylDbContext();

        public IHttpActionResult Get()
        {
            return Ok(_context.People);
        }

        public IHttpActionResult Get([FromODataUri] int key)
        {
            var person = _context.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        [HttpGet]
        [ODataRoute("People({key})/Email")]
        [ODataRoute("People({key})/FirstName")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/DateOfBirth")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonProperty([FromODataUri] int key)
        {
            var person = _context.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri.Segments.Last();
            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [ODataRoute("People({key})/Friends")]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetPersonCollectionProperty([FromODataUri] int key)
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();

            var person = _context.People.Include(collectionPropertyToGet)
                .FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [ODataRoute("People({key})/Email/$value")]
        [ODataRoute("People({key})/FirstName/$value")]
        [ODataRoute("People({key})/LastName/$value")]
        [ODataRoute("People({key})/DateOfBirth/$value")]
        [ODataRoute("People({key})/Gender/$value")]
        public IHttpActionResult GetPersonPropertyRawValue([FromODataUri] int key)
        {
            var person = _context.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri
                .Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue.ToString());
        }

        public IHttpActionResult Post(Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.People.Add(person);
            _context.SaveChanges();

            return Created(person);
        }

        public IHttpActionResult Put([FromODataUri] int key, Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _context.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            person.PersonId = currentPerson.PersonId;
            _context.Entry(currentPerson).CurrentValues.SetValues(person);
            _context.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _context.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            var id = currentPerson.PersonId;
            patch.Patch(currentPerson);
            // You can't update the id - so "reset it"
            currentPerson.PersonId = id;
            _context.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var currentPerson = _context.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            var peopleWithCurrentPersonAsFriend = _context.People.Include("Friends")
                .Where(p => p.Friends.Select(f => f.PersonId).AsQueryable().Contains(key));
            foreach (var person in peopleWithCurrentPersonAsFriend.ToList())
            {
                person.Friends.Remove(currentPerson);
            }

            _context.People.Remove(currentPerson);
            _context.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}