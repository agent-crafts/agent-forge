using System.ComponentModel;

namespace AgentForge.Core.Enums;

public enum ToolCategory
{
    [Description("Discovery")]
    Discovery = 1,

    [Description("Design")]
    Design = 2,

    [Description("Development")]
    Development = 3,

    [Description("Quality")]
    Quality = 4,

    [Description("Deployment")]
    Deployment = 5
}
