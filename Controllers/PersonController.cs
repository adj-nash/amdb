using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieDatabase.Data;
using MovieDatabase.Entities;
using MovieDatabase.Models;

namespace MovieDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly MovieDbContext _dbContext;

        public PersonController(MovieDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new();

            try
            {
                var actorCount = _dbContext.Person.Count();
                var actorList = _dbContext.Person.Skip(pageIndex * pageSize).Take(pageSize).
                    Select(x => new ActorViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DateOfBirth = x.DateOfBirth
                    }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Person = actorList, Count = actorCount };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong.";

                return BadRequest(response);

            }
        }

        [HttpGet("{id}")]
        public IActionResult GetPersonById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var person = _dbContext.Person.Where(x => x.Id == id).FirstOrDefault();

                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Record not found!";
                    return BadRequest(response);
                }

                var personData = new ActorDetailsViewModel
                {
                    Id = person.Id,
                    DateOfBirth = person.DateOfBirth,
                    Name = person.Name,
                    Movies = _dbContext.Movie.Where(x => x.Actors.Contains(person)).Select(x => x.Title).ToArray()
                };

                response.Status = true;
                response.Message = "Success";
                response.Data = personData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong.";

                return BadRequest(response);


            }
        }

        [HttpPost]
        public IActionResult Post(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {

                    var postedModel = new Person()
                    {
                        Name = model.Name,
                        DateOfBirth = model.DateOfBirth

                    };

                    _dbContext.Person.Add(postedModel);
                    _dbContext.SaveChanges();

                    model.Id = postedModel.Id;

                    response.Status = true;
                    response.Message = "New movie created successfully!";
                    response.Data = model   ;

                    return Ok(response);

                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong.";
                return BadRequest(response);
            }
        }

    }
}
