using System.ComponentModel;

namespace AgentForge.Core.Enums;

public enum ToolCategory
{
    // Legacy values retained to keep existing string-persisted tools readable.

    [Description("General")]
    General = 1,

    [Description("Development")]
    Development = 2,

    [Description("Integration")]
    Integration = 3,

    [Description("Data")]
    Data = 4,

    [Description("Automation")]
    Automation = 5,

    [Description("Discovery")]
    Discovery = 6,

    [Description("Design")]
    Design = 7,

    [Description("Quality")]
    Quality = 8,

    [Description("Deployment")]
    Deployment = 9
}
