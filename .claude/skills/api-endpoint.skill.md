# Skill: ApiEndpoint

## Purpose

Generate thin API controller endpoints.

## File Location

`src/PolicyManagement.API/Controllers/[EntityName]Controller.cs`

## Class Rules

- Inherits ControllerBase
- [ApiController], [Route("api/v1/[controller]")], [Produces("application/json")]
- Constructor injects: I[EntityName]Service, ILogger<[ControllerName]>

## GET Endpoints

- [HttpGet] or [HttpGet("{id:guid}")]
- [ProducesResponseType] for 200, 400, 500
- [FromQuery] for query params, [FromRoute] for route params
- Call \_service directly
- Return Ok(result)
- Include XML doc comments (///)

## PATCH/POST Endpoints

- [HttpPatch] or [HttpPost]
- [FromBody] for request body
- ProducesResponseType decorations
- Call \_service, return Ok(result)
