using System.ComponentModel;

namespace AgentForge.Core.Enums;

public enum ExternalToolSourceType
{
    [Description("OpenAPI / Swagger")]
    OpenApi = 1,

    [Description("MCP Server")]
    McpServer = 2,

    [Description("Existing Integration")]
    Integration = 3
}
