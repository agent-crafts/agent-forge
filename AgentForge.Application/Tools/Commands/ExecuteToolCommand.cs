using System.Text.Json;
using AgentForge.Application.Tools.Execution;
using AgentForge.Core.Repositories;
using Mediator;

namespace AgentForge.Application.Tools.Commands;

public sealed record ExecuteToolCommand(Guid Id, JsonElement Input) : ICommand<ToolExecutionResult>;

public sealed class ExecuteToolCommandHandler(IToolRepository tools, IToolExecutorRegistry registry)
    : ICommandHandler<ExecuteToolCommand, ToolExecutionResult>
{
    public async ValueTask<ToolExecutionResult> Handle(ExecuteToolCommand command, CancellationToken cancellationToken)
    {
        var tool = await tools.GetByIdAsync(command.Id, cancellationToken);
        if (tool is null || !tool.IsActive)
            return ToolExecutionResult.Fail("tool_not_found", "Tool not found.");
        var executor = registry.Resolve(tool.Name);
        if (executor is null)
            return ToolExecutionResult.Fail("executor_unavailable", "This tool has no approved execution adapter.");
        if (command.Input.ValueKind != JsonValueKind.Object)
            return ToolExecutionResult.Fail("invalid_input", "Input must be a JSON object.");

        using var document = JsonDocument.Parse(tool.JsonSchema);
        var schema = document.RootElement;
        if (schema.TryGetProperty("required", out var required) && required.ValueKind == JsonValueKind.Array)
        {
            foreach (var field in required.EnumerateArray())
            {
                var name = field.GetString();
                if (name is not null && (!command.Input.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null))
                    return ToolExecutionResult.Fail("invalid_input", $"Missing required input: {name}.");
            }
        }
        if (schema.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in command.Input.EnumerateObject())
            {
                if (!properties.TryGetProperty(property.Name, out var definition))
                {
                    if (schema.TryGetProperty("additionalProperties", out var additional) && additional.ValueKind == JsonValueKind.False)
                        return ToolExecutionResult.Fail("invalid_input", $"Unknown input: {property.Name}.");
                    continue;
                }
                if (definition.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String)
                {
                    var valid = type.GetString() switch
                    {
                        "string" => property.Value.ValueKind == JsonValueKind.String,
                        "integer" => property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt64(out _),
                        "number" => property.Value.ValueKind == JsonValueKind.Number,
                        "boolean" => property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False,
                        "array" => property.Value.ValueKind == JsonValueKind.Array,
                        "object" => property.Value.ValueKind == JsonValueKind.Object,
                        _ => false
                    };
                    if (!valid) return ToolExecutionResult.Fail("invalid_input", $"Invalid type for {property.Name}.");
                }
                if (definition.TryGetProperty("minimum", out var minimum) &&
                    minimum.ValueKind == JsonValueKind.Number &&
                    property.Value.ValueKind == JsonValueKind.Number &&
                    property.Value.GetDouble() < minimum.GetDouble())
                    return ToolExecutionResult.Fail("invalid_input", $"{property.Name} is below the minimum.");
            }
        }
        return await executor.ExecuteAsync(command.Input, cancellationToken);
    }
}
