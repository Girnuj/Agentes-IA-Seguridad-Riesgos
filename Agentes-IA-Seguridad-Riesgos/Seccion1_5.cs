namespace Agentes_IA_Seguridad_Riesgos;

//Demo 2 - Los 5 pasos de un agente en la practica.

#region Utils Types
public sealed record MemoryEntry(
    IReadOnlyDictionary<string, object?> Data,
    string Trust,
    int TtlHours);

public sealed class MemoryStore
{
    private readonly List<MemoryEntry> _entries = [];

    public IReadOnlyList<MemoryEntry> Entries => _entries;

    public void Save(IReadOnlyDictionary<string, object?> data, string trust, int ttlHours)
    {
        _entries.Add(new MemoryEntry(data, trust, ttlHours));
    }
}

public sealed class SeccionGoalViolationException(string message) : Exception(message) { }

public sealed class SeccionPlanViolationException(string message) : Exception(message) { }

public sealed class SeccionToolViolationException(string message) : Exception(message) { }

public sealed record SeccionContext(
    string? AgenticMode = null,
    string? OrderId = null,
    PolicyRules? Policy = null,
    IReadOnlySet<string>? AllowedScope = null,
    IReadOnlySet<string>? AllowedTools = null);

public sealed record AgentResult(
    string Agent,
    string Objective,
    IReadOnlyList<string> Plan,
    bool HighImpact,
    string Response);

/// <summary>
/// ToolRegistry maintains a registry of available example tools and their corresponding implementations, allowing agents to look up and execute tools based on their names.
/// </summary>
public static class ToolRegistry
{
    public static readonly IReadOnlyDictionary<string, Func<SeccionContext, IReadOnlyDictionary<string, object?>>> Tools =
        new Dictionary<string, Func<SeccionContext, IReadOnlyDictionary<string, object?>>>
        {
            ["lookup_order"] = context => new Dictionary<string, object?>
            {
                ["order_id"] = context.OrderId,
                ["amount"] = 100,
                ["eligible"] = true
            },
            ["check_policy"] = _ => new Dictionary<string, object?>
            {
                ["policy_ok"] = true,
                ["reason"] = "within policy"
            },
            ["issue_refund"] = _ => new Dictionary<string, object?>
            {
                ["refund_status"] = "approved",
                ["mode"] = "normal"
            },
            ["issue_full_refund_without_checks"] = _ => new Dictionary<string, object?>
            {
                ["refund_status"] = "approved",
                ["mode"] = "high_risk_bypass"
            }
        };
}
#endregion

/// <summary>
/// BaseAgent provides common functionality for both AgentUnsafe and AgentSafe, including planning and high-impact detection.
/// </summary>
public abstract class BaseAgent
{
    public static IReadOnlyList<string> Plan(string objective, SeccionContext context)
    {
        var normalized = objective.ToLowerInvariant();

        if (normalized.Contains("delete", StringComparison.Ordinal))
            return ["delete_records"];

        if (normalized.Contains("refund", StringComparison.Ordinal))
        {
            if (context.AgenticMode == "stochastic")
            {
                var candidates = new[]
                {
                    new[] { "lookup_order", "issue_refund" },
                    new[] { "lookup_order", "check_policy", "issue_refund" },
                    new[] { "lookup_order", "issue_full_refund_without_checks" }
                };

                return candidates[Random.Shared.Next(candidates.Length)];
            }

            return ["lookup_order", "issue_refund"];
        }

        return ["lookup_order"];
    }

    public static bool IsHighImpact(IReadOnlyList<string> plan) =>
        plan.Contains("issue_full_refund_without_checks");

}

/// <summary>
/// AgentUnsafe executes the 5-step cycle without any controls, allowing for potentially high-impact actions to be executed without validation or human oversight.
/// </summary>
public sealed class AgentUnsafe : BaseAgent
{
    private readonly MemoryStore _memory = new();

    public MemoryStore Memory => _memory;

    // No controls in the 5-step cycle.
    public AgentResult Run(string objective, SeccionContext context)
    {    
        //# Step 1: objective (no validation)
        var goal = objective;
        //# Step 2: plan (no scope validation)
        var plan = Plan(goal, context);
        //# Step 3: tools (no allowlist)
        var results = new Dictionary<string, IReadOnlyDictionary<string, object?>>();
        foreach (var step in plan)
        {
            if (ToolRegistry.Tools.TryGetValue(step, out var tool))
                results[step] = tool(context);
            else
                //# Unsafe agent keeps going even with unknown steps.
                results[step] = new Dictionary<string, object?> { ["warning"] = "unknown step executed path" };
        }

        //# Step 4: memory (stores as if trusted, long ttl)
        var snapshot = results.ToDictionary(pair => pair.Key, pair => (object?)pair.Value);
        _memory.Save(snapshot, "assumed", 720);

        //# Step 5: response (no human check on high-impact path)
        return new AgentResult(
            Agent: "unsafe",
            Objective: objective,
            Plan: plan,
            HighImpact: IsHighImpact(plan),
            Response: "ejecutado_sin_controles");
    }
}

/// <summary>
/// AgentSafe executes the 5-step cycle with controls at each step, ensuring that objectives are validated against policy, 
/// plans are within allowed scope, tools are permitted, and high-impact actions require human review before execution.
/// </summary>
public sealed class AgentSafe : BaseAgent
{
    private readonly MemoryStore _memory = new();
    public MemoryStore Memory => _memory;
    public static string ValidateObjective(string objective, PolicyRules policy)
    {
        var normalizedObjective = objective.ToLowerInvariant();

        if (policy.BlockedTerms.Any(term => normalizedObjective.Contains(term.Trim(), StringComparison.Ordinal)))
            throw new SeccionGoalViolationException($"Objetivo bloqueado por politica: {objective}");

        return objective;
    }

    public static void AssertPlanWithinScope(IReadOnlyList<string> plan, IReadOnlySet<string> allowedScope)
    {
        if (!plan.All(step => allowedScope.Contains(step)))
            throw new SeccionPlanViolationException($"Plan fuera de scope: {string.Join(", ", plan)}");
    }

    public static Dictionary<string, IReadOnlyDictionary<string, object?>> RunTools(
        IReadOnlyList<string> plan,
        IReadOnlySet<string> allowedTools,
        SeccionContext context)
    {
        var results = new Dictionary<string, IReadOnlyDictionary<string, object?>>();

        foreach (var step in plan)
        {
            if (!allowedTools.Contains(step))
                throw new SeccionToolViolationException($"Herramienta no permitida: {step}");

            if (!ToolRegistry.Tools.TryGetValue(step, out var tool))
                throw new SeccionToolViolationException($"Herramienta no permitida: {step}");

            results[step] = tool(context);
        }

        return results;
    }

    public static string Respond(IReadOnlyList<string> plan) =>
        IsHighImpact(plan) ? "requiere_revision_humana" : "ejecutado_con_controles";
    
    //One control per step in the 5-step cycle.
    public AgentResult Run(string objective, SeccionContext context)
    {
        //# Step 1: objective control
        var policy = context.Policy ?? throw new InvalidOperationException("La politica es requerida.");
        var goal = ValidateObjective(objective, policy);

        //# Step 2: planning control
        var plan = Plan(goal, context);
        AssertPlanWithinScope(plan, context.AllowedScope ?? new HashSet<string>());

        //# Step 3: tool control
        var results = RunTools(plan, context.AllowedTools ?? new HashSet<string>(), context);

        //# Step 4: memory control
        var snapshot = results.ToDictionary(pair => pair.Key, pair => (object?)pair.Value);
        _memory.Save(snapshot, "verified", 24);

        //# Step 5: response control
        var decision = Respond(plan);

        return new AgentResult(
            Agent: "safe",
            Objective: objective,
            Plan: plan,
            HighImpact: IsHighImpact(plan),
            Response: decision);
    }
}

public static class Seccion1_5Demo
{
    public static void DemoRun()
    {
        var objective = "issue refund for order 123";

        var unsafeContext = new SeccionContext(AgenticMode: "stochastic", OrderId: "123");

        var safeContext = new SeccionContext(
            AgenticMode: "stochastic",
            OrderId: "123",
            Policy: new PolicyRules(["delete", "drop"]),
            AllowedScope: new HashSet<string> { "lookup_order", "check_policy", "issue_refund" },
            AllowedTools: new HashSet<string> { "lookup_order", "check_policy", "issue_refund" });

        var unsafeAgent = new AgentUnsafe();
        var safeAgent = new AgentSafe();

        Console.WriteLine();
        Console.WriteLine("Demo 2 - 5 pasos: AgentUnsafe vs AgentSafe");
        Console.WriteLine($"Objetivo: {objective}");

        Console.WriteLine();
        Console.WriteLine("1) Corridas con AgentUnsafe");
        for (var index = 1; index <= 5; index++)
        {
            var output = unsafeAgent.Run(objective, unsafeContext);
            if (output.HighImpact)
                Console.WriteLine($"Run {index}: plan={string.Join(", ", output.Plan)} -> MALO: se ejecuto ruta riesgosa sin controles");
            else
                Console.WriteLine($"Run {index}: plan={string.Join(", ", output.Plan)} -> sin bloqueo (aunque parezca normal)");
        }

        Console.WriteLine();
        Console.WriteLine("2) Corridas con AgentSafe");
        for (var index = 1; index <= 5; index++)
        {
            try
            {
                var output = safeAgent.Run(objective, safeContext);
                Console.WriteLine($"Run {index}: plan={string.Join(", ", output.Plan)} -> SEGURO: ejecutado dentro de scope y con controles");
            }
            catch (Exception exception) when (exception is SeccionPlanViolationException or SeccionToolViolationException or SeccionGoalViolationException)
            {
                Console.WriteLine($"Run {index}: SEGURO: bloqueado por control ({exception.Message})");
            }
        }

        Console.WriteLine();
        Console.WriteLine("3) Verificacion con objetivo malicioso");
        try
        {
            safeAgent.Run("delete all records", safeContext);
        }
        catch (SeccionGoalViolationException exception)
        {
            Console.WriteLine($"SEGURO: bloqueado antes de planificar ({exception.Message})");
        }
    }
}
