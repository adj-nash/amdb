using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieDatabase.Data;
using MovieDatabase.Entities;
using MovieDatabase.Models;

namespace MovieDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _dbContext;

        public MovieController(MovieDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new();

            try
            {
                var movieCount = _dbContext.Movie.Count();
                var movieList = _dbContext.Movie.Include(x => x.Actors).Skip(pageIndex * pageSize).Take(pageSize).
                    Select(x => new MovieListViewModel {
                        Id = x.Id,
                        Title = x.Title,
                        Actors = x.Actors.Select(y => new ActorViewModel
                        { 
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,
                        }).ToList(),
                        CoverImage = x.CoverImage,
                        Language = x.Language,
                        ReleaseDate = x.ReleaseDate
                    }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Movies = movieList, Count = movieCount };

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
        public IActionResult GetById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _dbContext.Movie.Include(x => x.Actors).Where(x => x.Id == id).Select(x => new MovieDetailsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Actors = x.Actors.Select(y => new ActorViewModel
                    {
                        Id = y.Id,
                        Name = y.Name,
                        DateOfBirth = y.DateOfBirth,
                    }).ToList(),
                    CoverImage = x.CoverImage,
                    Language = x.Language,
                    ReleaseDate = x.ReleaseDate,
                    Description = x.Description
                }).FirstOrDefault();

                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Record not found!";
                    return BadRequest(response);
                }

                response.Status = true;
                response.Message = "Success";
                response.Data = movie;

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
        public IActionResult Post(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if(ModelState.IsValid)
                {
                    var actors = _dbContext.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if(actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid actor selected";
                        return BadRequest(response);
                    }

                    var postedModel = new Movie()
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Language = model.Language,
                        ReleaseDate = model.ReleaseDate,
                        CoverImage = model.CoverImage,
                        Actors = actors
                    };

                    _dbContext.Movie.Add(postedModel);
                    _dbContext.SaveChanges();

                    var responseData = new MovieDetailsViewModel
                    {
                        Id = postedModel.Id,
                        Title = postedModel.Title,
                        Actors = postedModel.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,
                        }).ToList(),
                        CoverImage = postedModel.CoverImage,
                        Language = postedModel.Language,
                        ReleaseDate = postedModel.ReleaseDate,
                        Description = postedModel.Description
                    };

                    response.Status = true;
                    response.Message = "New movie created successfully!";
                    response.Data = responseData;

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

        [HttpPut]
        public IActionResult Put(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {   
                    if(model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid movie record.";
                        return BadRequest(response);

                    }

                    var actors = _dbContext.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid actor selected";
                        return BadRequest(response);
                    }

                    var movieDetails = _dbContext.Movie.Include(x => x.Actors).Where(x => x.Id == model.Id).FirstOrDefault();

                    if(movieDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid movie record.";
                        return BadRequest(response);
                    }

                    movieDetails.CoverImage = model.CoverImage;
                    movieDetails.Description = model.Description;
                    movieDetails.Language = model.Language;
                    movieDetails.ReleaseDate = model.ReleaseDate;
                    movieDetails.Title = model.Title;

                    // Find removed actors

                    var removedActors = movieDetails.Actors.Where(x => !model.Actors.Contains(x.Id)).ToList();

                    foreach(var actor in removedActors)
                    {
                        movieDetails.Actors.Remove(actor);
                    }

                    // Find added actors

                    var addedActors = actors.Except(movieDetails.Actors).ToList();

                    foreach (var actor in addedActors)
                    {
                        movieDetails.Actors.Add(actor);
                    }

                    _dbContext.SaveChanges();


                    var responseData = new MovieDetailsViewModel
                    {
                        Id = movieDetails.Id,
                        Title = movieDetails.Title,
                        Actors = movieDetails.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth,
                        }).ToList(),
                        CoverImage = movieDetails.CoverImage,
                        Language = movieDetails.Language,
                        ReleaseDate = movieDetails.ReleaseDate,
                        Description = movieDetails.Description
                    };

                    response.Status = true;
                    response.Message = "Movie updated successfully!";
                    response.Data = responseData;

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
