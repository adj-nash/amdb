using System.Linq;
using System.Net.Http.Headers;
using Azure;
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
        private readonly AutoMapper.IMapper _mapper;

        public MovieController(MovieDbContext context, AutoMapper.IMapper mapper)
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
                var movieCount = _dbContext.Movie.Count();
                var movieList = _mapper.Map<List<MovieListViewModel>>(_dbContext.Movie.Include(x => x.Actors).Skip(pageIndex * pageSize).Take(pageSize)).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Movies = movieList, Count = movieCount };

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
        public IActionResult GetById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _dbContext.Movie.Include(x => x.Actors).Where(x => x.Id == id).FirstOrDefault();

                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Record not found!";
                    return BadRequest(response);
                }

                var movieData = _mapper.Map<MovieDetailsViewModel>(movie);

                response.Status = true;
                response.Message = "Success";
                response.Data = movie;

                return Ok(response);
            }
            catch (Exception)
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

                    var postedModel = _mapper.Map<Movie>(model);
                    postedModel.Actors = actors;

                    _dbContext.Movie.Add(postedModel);
                    _dbContext.SaveChanges();

                    var responseData = _mapper.Map<MovieDetailsViewModel>(postedModel);
              

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
            catch (Exception)
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
            catch (Exception)
            {

                response.Status = false;
                response.Message = "Something went wrong.";
                return BadRequest(response);
            }
        }

        [HttpDelete]
         public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _dbContext.Movie.Where(x => x.Id == id).FirstOrDefault();
                if (movie == null)
                {
                    response.Status = false;
                    response.Message = "Validation failed";

                    return BadRequest(response);
                }

                _dbContext.Movie.Remove(movie);
                _dbContext.SaveChanges();

                response.Status = true;
                response.Message = "Movie deleted successfully!";

                return Ok(response);

            }
            catch (Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong.";
                return BadRequest(response);

            }
        }
        [HttpPost]
        [Route("upload-movie-poster")]

        public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile)
        {
            try
            {
                var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
                // For ease of saving down movie posters I'm using a local folder 'Images', however this is not best practice, especially in bigger applications where security is more of a concern.
                string newPath = @"C:\\Users\\alex_\\OneDrive\\Desktop\\dotnet\\MovieDatabase\\Images\\";

                if(!Directory.Exists(newPath))
                    {
                    Directory.CreateDirectory(newPath);
                    }

                string[] allowedImageExtensions = new string[]
                {
                    ".jpg", ".jpeg", ".png" 
                };

                if (!allowedImageExtensions.Contains(Path.GetExtension(filename)))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        Message = "Only .JPG, .JPEG, .PNG types files can be uploaded."
                    });
                }

                string newFileName = Guid.NewGuid() + Path.GetExtension(filename);
                string fullFilePath = Path.Combine(newPath, newFileName);

                using (var Stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(Stream);
                }
                return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}" });
                

            }
            catch (Exception)
            {
                return BadRequest(new BaseResponseModel
                {
                    Status = false,
                    Message = "Error occured!"
                });
            }

        }
    }
}
