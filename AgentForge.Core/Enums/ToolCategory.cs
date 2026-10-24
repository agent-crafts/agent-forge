using System.ComponentModel;

namespace AgentForge.Core.Enums;

public enum ToolCategory
{
    [Description("General")]
    General = 1,

    [Description("Development")]
    Development = 2,

    [Description("Integration")]
    Integration = 3,

    [Description("Data")]
    Data = 4,

    [Description("Automation")]
    Automation = 5
}
