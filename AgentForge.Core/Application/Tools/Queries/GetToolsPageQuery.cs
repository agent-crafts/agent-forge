using AgentForge.Core.Domains;
using AgentForge.Core.Utilities;

namespace AgentForge.Core.Application.Tools.Queries;

public sealed record GetToolsPageQuery(PagedQuery<Tool> Page);
