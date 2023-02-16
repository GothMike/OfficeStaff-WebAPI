﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OfficeStaff.Data.Dto;
using OfficeStaff.Data.Interfaces;
using OfficeStaff.Data.Models;
using OfficeStaff.Data.Repository;
using OfficeStaff.Persistence;

namespace OfficeStaff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IMapper _mapper;


        public EmployeeController(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, IPositionRepository positionRepository , IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateEmployee([FromQuery] int departmentId, [FromQuery] int positionId, [FromBody] EmployeeDto employeeCreated)
        {
            if (employeeCreated == null)
                return BadRequest(ModelState);

            var employees = _employeeRepository.GetEmployees().Where(e => e.LastName.Trim().ToUpper() == employeeCreated.LastName.TrimEnd().ToUpper()).FirstOrDefault();

            if(employees != null) 
            {
                ModelState.AddModelError("", "Сотрудник уже создан");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeMap = _mapper.Map<Employee>(employeeCreated);

            employeeMap.Department = _departmentRepository.GetDepartment(departmentId);
            employeeMap.Position = _positionRepository.GetPosition(positionId);


            if (!_employeeRepository.CreateEmployee(employeeMap, departmentId, positionId))
            {
                ModelState.AddModelError("", "Something went wrong while savin");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully created");
        }

        [HttpGet("{employeeId}")]
        [ProducesResponseType(200, Type = typeof(Employee))]
        [ProducesResponseType(400)]
        public IActionResult ReadEmployee(int employeeId)
        {
            if(!_employeeRepository.EmployeeExists(employeeId))
                return NotFound();

            var employee = _mapper.Map<EmployeeDto>(_employeeRepository.GetEmployee(employeeId));


                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(employee);
            }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Employee>))]
        [ProducesResponseType(400)]
        public IActionResult ReadEmployees()
        {
            var employees = _mapper.Map<List<EmployeeDto>>(_employeeRepository.GetEmployees());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(employees);
        }


        [HttpPut("{employeeId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateEmployee(int employeeId, [FromQuery] int departmentId, [FromBody] EmployeeDto updateEmployee)
        {
            if (updateEmployee == null)
                return BadRequest();

            if (employeeId != updateEmployee.Id)
                return BadRequest();

            if (!_employeeRepository.EmployeeExists(employeeId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeMap = _mapper.Map<Employee>(updateEmployee);

            employeeMap.Department = _departmentRepository.GetDepartment(departmentId);

            if (!_employeeRepository.UpdateEmployee(employeeMap))
            {
                ModelState.AddModelError("", "Что-то пошло не так при редактировании сотрудника");
                return StatusCode(500, ModelState);
            }

            return Ok($"Сотрудник {employeeMap.FirstName } {employeeMap.LastName} - отредактирована в базе данных");
        }

        [HttpDelete("{employeeId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteLocation(int employeeId)
        {
            if (!_employeeRepository.EmployeeExists(employeeId))
                return NotFound();

            var employeeToDelete = _employeeRepository.GetEmployee(employeeId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_employeeRepository.DeleteEmployee(employeeToDelete))
            {
                ModelState.AddModelError("", "Что-то пошло не так при удалении сотрудника");
                return StatusCode(500, ModelState);
            }

            return Ok($"Сотрудник {employeeToDelete.FirstName} {employeeToDelete.LastName} - удален из базы данных");
        }
    }
}