﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using TABP.Application.Commands.Cities.CreateCity;
using TABP.Application.Commands.Cities.DeleteCity;
using TABP.Application.Commands.Cities.UpdateCity;
using TABP.Application.Queries.Cities.GetTrendingCities;
using TABP.Domain.Enums;
using TABP.Web.DTOs.Cities;

namespace TABP.Web.Controllers;

[ApiController]
[Route("api/cities")]
[Authorize(Roles = nameof(Role.Admin))]
public class CitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CitiesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrendingCities()
    {
        var trendingCities = await _mediator.Send(
            new GetTrendingCitiesQuery());

        return Ok(trendingCities);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCity([FromBody] CreateCityDto cityCreateDto)
    {
        var command = _mapper.Map<CreateCityCommand>(cityCreateDto);

        await _mediator.Send(command);

        return Created();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCity(Guid id)
    {
        await _mediator.Send(new DeleteCityCommand { Id = id });

        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateCity(Guid id,
        [FromBody] JsonPatchDocument<UpdateCityDto> cityUpdateDto)
    {
        var cityDocument = _mapper.Map<JsonPatchDocument<CityUpdate>>(cityUpdateDto);

        await _mediator.Send(new UpdateCityCommand { Id = id, CityDocument = cityDocument });

        return Ok();
    }
}