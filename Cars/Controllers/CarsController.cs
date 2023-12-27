using Cars.Data;
using Cars.Models;
using DataTables.AspNet.Core;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Cars.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILog _logger;
        public CarsController(IUnitOfWork unitOfWork,ILog logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetCars()
        {
            try
            {
                _logger.Info("This is an information log message.");
                var cars = _unitOfWork.GetRepository<Car>().GetAll().Result;
                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }

        }
        [HttpGet("ByUserId/{userId}")]
        public async Task<IActionResult> GetCarsByUserId(int userId)
        {
            try
            {
                var cars =  _unitOfWork.GetRepository<Car>().GetAll().Result.Where(c => c.UserId == userId).ToList();

                if (cars == null || !cars.Any())
                {
                    return NotFound($"No cars found for user with ID {userId}");
                }

                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        [HttpPost("CarDatatableResult")]
        public async Task<IActionResult> CarDatatableResult([FromForm] MyDataTablesRequest request)
        {
            try
            {
                
                var queryableData = _unitOfWork.GetRepository<Car>().GetAll(); // Get your IQueryable<Car> data here

               

                var data = queryableData.Result.ToList(); // Get the paged and filtered data

                
                return Ok(new
                {
                    draw = request.Draw,
                    recordsTotal = data.Count, 
                    recordsFiltered = data.Count, 
                    data = data 
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return StatusCode(500, "An error occurred while fetching data.");
            }
        }
        [HttpPost("Search")]
        public async Task<IActionResult> SearchCarsByUserAndKeyword([FromBody] SearchRequest request)
        {

           try
            {
                if (request.UserId <= 0)
                {
                    return BadRequest("Invalid userId");
                }

                IEnumerable<Car> cars;

                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    cars = _unitOfWork.GetRepository<Car>().GetAll().Result
                        .Where(c => c.UserId == request.UserId &&
                                    (c.Brand.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) ||
                                        c.Model.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase))).ToList();
                            
                }
                else
                {
                    cars =  _unitOfWork.GetRepository<Car>()
                        .GetAll().Result
                        .Where(c => c.UserId == request.UserId)
                        .ToList();
                }

                if (cars == null || !cars.Any())
                {
                    if (string.IsNullOrWhiteSpace(request.Keyword))
                    {
                        return NotFound($"No cars found for User ID: {request.UserId}");
                    }
                    else
                    {
                        return NotFound($"No cars found for User ID: {request.UserId} and the provided keyword: {request.Keyword}");
                    }
                }

                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                    return StatusCode(500, "An error occurred while searching for cars.");
           }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarById(int id)
        {
            try
            {
                var car =await _unitOfWork.GetRepository<Car>().GetById(id);

                if (car == null || id != car.Id)
                {
                    return NotFound($"Car with ID {id} not found");
                }

                return Ok(car);
            }
             catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCar([FromBody] Car car)
        {
            try
            {
                if (car == null)
                {
                    return BadRequest("Car object is null");
                }
                car.UserId = car.UserId;
                await _unitOfWork.GetRepository<Car>().Add(car);
                await _unitOfWork.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCarById), new { id = car.Id }, car);
            }
            catch(Exception ex) { _logger.Error(ex.Message); throw; }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar([FromBody] Car car)
        {
            try
            {
                if (car == null)
                {
                    return BadRequest("Invalid car data or ID mismatch");
                }

                var existingCar = await _unitOfWork.GetRepository<Car>().GetById(car.Id);
                if (existingCar == null || existingCar.UserId != car.UserId)
                {
                    return NotFound($"Car with ID {car.Id} not found for user with ID {car.UserId}");
                }
                existingCar.Brand = car.Brand;
                existingCar.Model = car.Model;
                existingCar.Year = car.Year;
                existingCar.Price = car.Price;
                existingCar.New = car.New;

                await _unitOfWork.GetRepository<Car>().Update(existingCar);
                return NoContent();
            }
            catch (Exception ex) { _logger.Error(ex.Message); throw; }
            
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                var existingCar = await _unitOfWork.GetRepository<Car>().GetById(id);
                if (existingCar == null)
                {
                    return NotFound($"Car with ID {id} not found");
                }

                await _unitOfWork.GetRepository<Car>().Delete(id);
                return NoContent();
            }
            catch (Exception ex) { _logger.Error(ex.Message);  throw; }
           
        }


    }

}
