using System.ComponentModel;

namespace AgentForge.Core.Enums;

public enum ToolKind
{
    [Description("Custom Tool")]
    Custom = 1,

    [Description("External Tool")]
    External = 2
}
