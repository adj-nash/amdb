using Azure;
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
        private readonly AutoMapper.IMapper _mapper;

        public PersonController(MovieDbContext context, AutoMapper.IMapper mapper)
        {
            _dbContext = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new();

            try
            {
                var actorCount = _dbContext.Person.Count();
                var actorList = _mapper.Map<List<ActorViewModel>>(_dbContext.Person.Skip(pageIndex * pageSize).Take(pageSize).ToList());

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Person = actorList, Count = actorCount };

                return Ok(response);
            }
            catch (Exception)
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
                    Movies = _dbContext.Movie.Where(x => x.Actors.Contains(person)).Select(x => x.Title).ToArray(),
                    CoverImage = _dbContext.Movie.Where(x => x.Actors.Contains(person)).Select(x => x.CoverImage).ToArray()
            };

                response.Status = true;
                response.Message = "Success";
                response.Data = personData;

                return Ok(response);
            }
            catch (Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong.";

                return BadRequest(response);


            }
        }

        [HttpGet]
        [Route("Search/{searchText}")]

        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var searchedPerson = _dbContext.Person.Where(x => x.Name.Contains(searchText)).Select(x => new
                {
                    x.Id,
                    x.Name 
                }).ToList();


                response.Status = true;
                response.Message = "Success";
                response.Data = searchedPerson;

                return Ok(response);
            }


            catch(Exception)
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
            catch (Exception)
            {

                response.Status = false;
                response.Message = "Something went wrong.";
                return BadRequest(response);
            }
        }
        [HttpPut]

        public IActionResult Put(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    var postedModel = _mapper.Map<Person>(model);

                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid data entered.";

                        return BadRequest(response);
                    }

                    var personDetails = _dbContext.Person.Where(x => x.Id == model.Id).AsNoTracking().FirstOrDefault();
                    if (personDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Actor doesn't exist.";

                        return BadRequest(response);
                    }
                    _dbContext.Person.Update(postedModel);
                    _dbContext.SaveChanges();

                    response.Status = true;
                    response.Message = "Actor updated successfully!";
                    response.Data = postedModel;

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
            catch (Exception)
            {
                response.Status = false;
                response.Message = "An error occured.";

                return BadRequest(response);
            }
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var person = _dbContext.Person.Where(x => x.Id == id).FirstOrDefault();
                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Validation failed";

                    return BadRequest(response);
                }

                _dbContext.Person.Remove(person);
                _dbContext.SaveChanges();

                response.Status = true;
                response.Message = "Actor deleted successfully!";

                return Ok(response);

            }
            catch (Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong.";
                return BadRequest(response);

            }
        }
    }
}
