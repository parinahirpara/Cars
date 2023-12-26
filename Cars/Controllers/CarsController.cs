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
                var carsEnumerable = await _unitOfWork.GetRepository<Car>().GetAll();
                var carsQueryable = carsEnumerable.AsQueryable();
                var cars = await carsQueryable.Where(c => c.UserId == userId).ToListAsync();

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
                // Use the request to perform data filtering, paging, and sorting
                var queryableData = _unitOfWork.GetRepository<Car>().GetAll(); // Get your IQueryable<Car> data here

                // Apply filters, paging, and sorting using request parameters

                var data = queryableData.Result.ToList(); // Get the paged and filtered data

                // Return the paged and filtered data along with total records and filtered records count
                return Ok(new
                {
                    draw = request.Draw,
                    recordsTotal = data.Count, // Total records count
                    recordsFiltered = data.Count, // Filtered records count (if applying filters)
                    data = data // Paged and filtered data
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return StatusCode(500, "An error occurred while fetching data.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarById(int id)
        {
            try
            {
                var car = _unitOfWork.GetRepository<Car>().GetById(id);

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
        public async Task<IActionResult> AddCar([FromBody] Car car, int userId)
        {
            try
            {
                if (car == null)
                {
                    return BadRequest("Car object is null");
                }
                car.UserId = userId;
                await _unitOfWork.GetRepository<Car>().Add(car);
                await _unitOfWork.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCarById), new { id = car.Id }, car);
            }
            catch(Exception ex) { _logger.Error(ex.Message); throw; }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, int userId, [FromBody] Car car)
        {
            try
            {
                if (car == null || id != car.Id)
                {
                    return BadRequest("Invalid car data or ID mismatch");
                }

                var existingCar = await _unitOfWork.GetRepository<Car>().GetById(id);
                if (existingCar == null || existingCar.UserId != userId)
                {
                    return NotFound($"Car with ID {id} not found for user with ID {userId}");
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
        public async Task<IActionResult> DeleteCar(int id,int userId)
        {
            try
            {
                var existingCar = await _unitOfWork.GetRepository<Car>().GetById(id);
                if (existingCar == null || existingCar.UserId != userId)
                {
                    return NotFound($"Car with ID {id} not found for user with ID {userId}");
                }

                await _unitOfWork.GetRepository<Car>().Delete(id);
                return NoContent();
            }
            catch (Exception ex) { _logger.Error(ex.Message);  throw; }
           
        }


    }

}
