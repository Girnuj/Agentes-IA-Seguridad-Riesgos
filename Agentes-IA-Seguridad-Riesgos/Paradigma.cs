namespace Agentes_IA_Seguridad_Riesgos;

//Demo 1 - Paradigma determinista vs agentico en la practica.

//Self-contained script for instructor use.It shows:
//1) Deterministic flow
//2) Agentic flow without controls
//3) Agentic flow with controls

#region Utils Types
public sealed record PolicyRules(IReadOnlyList<string> BlockedTerms);

public sealed record AgentContext(
    string? AgenticMode = null,
    string? OrderId = null,
    PolicyRules? Policy = null,
    IReadOnlySet<string>? AllowedScope = null,
    IReadOnlySet<string>? AllowedTools = null);

public sealed record AgentExecutionResult(
    string Agent,
    string Objective,
    IReadOnlyList<string> Plan,
    bool HighImpact,
    string Response);

public sealed class ParadigmaGoalViolationException(string message) : Exception(message) { }

public sealed class ParadigmaPlanViolationException(string message) : Exception(message) { }
#endregion

/// <summary>
/// Represents an agent that can plan and execute actions based on a given objective and context.
/// </summary>
public sealed class Agent
{
    public static IReadOnlyList<string> Plan(string objective, AgentContext? context = null)
    {
        var normalized = objective.ToLowerInvariant();

        if (normalized.Contains("delete", StringComparison.Ordinal))
            return ["delete_records"];

        if (normalized.Contains("refund", StringComparison.Ordinal))
        {
            if (context?.AgenticMode == "stochastic")
            {
                var candidates = new[]
                {
                    new[] { "lookup_order", "issue_refund" },
                    new[] { "lookup_order", "check_policy", "issue_refund" },
                    new[] { "lookup_order", "issue_full_refund_without_checks" }
                };

                var selected = candidates[Random.Shared.Next(candidates.Length)];
                return selected;
            }

            return ["lookup_order", "issue_refund"];
        }

        return ["lookup_order"];
    }

    public static string Execute(IReadOnlyList<string> plan, bool audit = false)
    {
        if (audit)
            Console.WriteLine($"AUDITORIA: ejecutando plan={string.Join(", ", plan)}");

        return $"Ejecutado: {string.Join(", ", plan)}";
    }
}

/// <summary>
/// Demonstrates different paradigms of processing requests, including deterministic and agentic approaches, with and without controls.
/// </summary>
public static class ParadigmaDemo
{
    private static readonly Agent Agent = new();

    public static bool IsValidInput(string userInput) =>
        !string.IsNullOrWhiteSpace(userInput);

    public static string ExecuteQuery(string userInput) =>
        $"Consulta OK para input: {userInput}";


    public static bool ValidateObjective(string objective, PolicyRules? policy)
    {
        var blockedTerms = policy?.BlockedTerms ?? [];
        var normalizedObjective = objective.ToLowerInvariant();

        return blockedTerms.All(term => !normalizedObjective.Contains(term.Trim(), StringComparison.Ordinal));
    }

    public static bool IsPlanWithinScope(IReadOnlyList<string> plan, IReadOnlySet<string> allowedScope) =>
        plan.All(step => allowedScope.Contains(step));

 //-- VERSION 1: Deterministic software ------------------
    public static string ProcessRequest(string userInput)
    {
        if (!IsValidInput(userInput))
            throw new ArgumentException("Entrada invalida");

        return ExecuteQuery(userInput);
    }
 //-- VERSION 2: Agentic system without controls ---------
    public static string ProcessAgenticUnsafe(string objective, AgentContext context)
    {
        var plan = Agent.Plan(objective, context);
        return Agent.Execute(plan);
    }
 //-- VERSION 3: Agentic system with controls -------------
    public static string ProcessAgenticSafe(string objective, AgentContext context)
    {
        if (!ValidateObjective(objective, context.Policy))
            throw new ParadigmaGoalViolationException($"Objetivo viola politica: {objective}");

        var plan = Agent.Plan(objective, context);
        if (context.AllowedScope is not null && !IsPlanWithinScope(plan, context.AllowedScope))
            throw new ParadigmaPlanViolationException("Plan fuera de alcance permitido");

        return Agent.Execute(plan, audit: true);
    }

    public static void DemoRun()
    {
        var agenticContext = new AgentContext(AgenticMode: "stochastic");

        var safeContext = new AgentContext(
            AgenticMode: "stochastic",
            Policy: new PolicyRules(["delete", "drop"]),
            AllowedScope: new HashSet<string> { "lookup_order", "check_policy", "issue_refund" });

        Console.WriteLine();
        Console.WriteLine("1) Determinista (mismo input, mismo resultado)");
        Console.WriteLine(ProcessRequest("order=123"));
        Console.WriteLine(ProcessRequest("order=123"));

        Console.WriteLine();
        Console.WriteLine("2) Agentico sin controles (mismo objetivo, plan variable)");
        var objective = "issue refund for order 123";
        for (var index = 1; index <= 5; index++)
            Console.WriteLine($"Run {index}: {ProcessAgenticUnsafe(objective, agenticContext)}");

        Console.WriteLine();
        Console.WriteLine("3) Agentico con controles (validacion de plan)");
        for (var index = 1; index <= 5; index++)
        {
            try
            {
                Console.WriteLine($"Run {index}: {ProcessAgenticSafe(objective, safeContext)}");
            }
            catch (ParadigmaPlanViolationException exception)
            {
                Console.WriteLine($"Run {index}: Bloqueado por control: {exception.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("4) Agentico con controles (objetivo malicioso)");
        try
        {
            Console.WriteLine(ProcessAgenticSafe("delete all records", safeContext));
        }
        catch (ParadigmaGoalViolationException exception)
        {
            Console.WriteLine($"Bloqueado por control: {exception.Message}");
        }
    }
}
