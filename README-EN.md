# AI Agents: Security and Risks

An educational and demonstration project designed to understand how security changes when a system moves from being deterministic to becoming an agent capable of planning, executing tools, and making decisions with bounded autonomy.

This repository does not intend to replace a real production architecture; rather, it aims to illustrate, using simple examples in C# and .NET, the key principles of AI agent security: objective validation, plan scope, tool control, traceability, and human review.

![Paradigm shift in security](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image1.png)

For this content, when I say "agentic system," I mean software that not only responds, but also receives an objective, plans intermediate steps, uses tools, and executes actions with autonomy constrained by policies. That bounded autonomy is exactly where the risks change.

## Why this project?

In traditional software, security focuses on validating inputs, controlling deterministic flows, authenticating users, and authorizing actions. In an agentic architecture, in addition to those layers, a new dimension appears:

- the agent interprets objectives;
- generates a dynamic plan;
- selects tools;
- stores context or memory;
- executes actions with a certain level of autonomy.

This introduces compound risks: an individual action may seem legitimate, but the combination of steps, context, and objectives can end up producing an unsafe result.

The central idea of this repository is to show that security in agents is not only a prompt or model problem. It is also an architecture, governance, permission control, and auditing problem.

![Agentic system](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image2.png)

In the traditional model, the focus was on known controls: validating inputs and outputs, authenticating, authorizing, and handling failures. There was uncertainty, yes, but behavior was more constrained, which allowed for designing tests and controls with reasonable coverage over expected execution paths.

![](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image3.png)

![](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image4.png)

With agents, a new layer appears. There is intention interpretation, dynamic planning, tool usage, and adaptation to context. In that process, compound risks emerge. For example, each step individually may seem valid, but the final combination can be unsafe. That is why, in addition to validating code, we also need to govern decisions and capabilities at runtime. What we are seeing is a shift in the trust model.

![](./Agentes-IA-Seguridad-Riesgos/Image/CambioEnParadigmaSeguridad/image5.png)

With a human user, intent is usually more explicit and there is direct responsibility for each action. With an agent, we delegate execution, so the technical question is no longer just: "Can it do this?" It becomes: under what conditions can it do it, with what limits, and how do we stop it if it deviates? This is where the controls we will use throughout the content come in. We are talking about agency limits, least effective privilege, plan validation before execution, traceability from decision to action, and containment mechanisms.

We will never replace classic security; we will only complement it where agency introduces systemic risk. When the system moves from executing to deciding, the way it is attacked changes, and so does the way it is defended.

### Deterministic vs agentic paradigm in practice

Let’s look at the code behind what we just discussed conceptually.

![Three versions](./Agentes-IA-Seguridad-Riesgos/Image/CodeExample/image1.png)

I have three versions of the same problem: a deterministic version, an agentic version without any controls, and an agentic version with controls. The difference is not in the number of lines of code, but in the point at which the system makes decisions on its own. The first version is the deterministic one. As you can see, it receives an input, performs validation, and then executes a query. The flow always follows the same path; if validation passes, there is only one possible route. That makes controls predictable: I validate the input, handle errors, and authorize actions.

Now consider version two, the agentic version without any controls. Here we have the same objective; in this case, we are talking about a refund system. The agent can execute different plans across runs, and while that plan is running we do not know exactly what the agent will define. Sometimes it may take a normal path where it checks the order and finds a problem; other times it may take a riskier path where it issues a full refund without any verification. Since we are not validating the plan, both can potentially run.

Now look at version three. It is still an agentic version, but we have added a set of controls. First, we validate the objective against policy before planning. Next, we validate whether the plan steps are allowed. Notice that the first thing we do is ask the agent to produce a plan and then verify it is allowed. For example, we could block a full refund without prior verification.

Finally, we execute the agent’s plan, but we pass a flag such as `audit=True` to keep traceability of which plan was executed and which was not.

I’m going to show you an execution of this code. One thing to know is that the agent implementation is here above in this file, and what we are doing is randomly choosing different options within the plan, such as issuing a refund, checking policy, or issuing a refund without any verification.

Obviously, the kind of agent you are used to hearing about lately is one that interacts with an LLM. In this case, since I want to focus only on the security part, I am not introducing the LLM layer, and we add that part as a source of instability or different behavior on each run.

If I show you the execution of this program, here is what happens:

- a call is made to the deterministic model,
- one to the agentic system without controls,
- and one to the agentic system with controls.

We will run this and you will see that in this case the order is 123 and in the deterministic model the response is always the same. In the agentic model without controls, what gets executed here is a refund, a refund without any verification, which we know is something we would not want in this case.

And in the third case, where controls are present, notice that there were a couple of blocks because it likely tried to issue a refund without any verification.

![Code execution](./Agentes-IA-Seguridad-Riesgos/Image/CodeExample/image3.png)

If we run this code again, we see a different execution, which is why the example uses randomness, and we see that some kind of audit has happened; this is the `audit=True` flag we were passing.

![Code execution 2](./Agentes-IA-Seguridad-Riesgos/Image/CodeExample/image2.png)

In any case, this code is available in the repository so you can explore it yourself.

[Code](Agentes-IA-Seguridad-Riesgos\Agentes-IA-Seguridad-Riesgos\Paradigma.cs)

### Anatomy of an agent in 5 steps

![Agent lifecycle](./Agentes-IA-Seguridad-Riesgos/Image/5Pasos/image1.png)

Next, we are going to analyze the risks consistently and use the five stages of the agent lifecycle.

It all starts when a request arrives. If that objective is ambiguous, conflicting, or manipulable, the rest of the flow is born distorted.

The first control is to normalize the objective, resolve ambiguities, and validate it against policy before the agent starts planning.

Then we move to planning. The agent builds a plan. The risk here is not only a bad plan, but also a combination of unauthorized or unbounded steps. The minimum control you can have here is validating the plan, checking preconditions per step, and limiting scope to prevent plans from expanding on their own.

The third step is tool usage. This is where API calls, functions, or external services occur. This is critical because dangerous parameters can be used, irreversible actions can be taken, and actions can be performed out of context. The minimum control you need here is an allowlist of tools, semantic validation of parameters, and least privilege per call.

Then we move to memory or context. The agent decides based on this context. If the context is contaminated or unverified, the decision will also be contaminated. The minimum control here is to label source trust, expire stale context, and separate trusted memory from untrusted memory.

Finally, when we talk about execution and output, this is where the highest-impact risk appears. The minimum control is to enforce policies by impact level, require human supervision for critical decisions, and maintain full traceability from decision to action to result. For example, for a critical action, a common approach is human-in-the-loop, where a person must approve or reject the action before the agent executes it.

### The 5 steps of an agent in practice

Let me show you how this five-step map looks in code. [Code](Agentes-IA-Seguridad-Riesgos\Agentes-IA-Seguridad-Riesgos\PasosAgent1_5.cs)

In this case I have two classes: `AgentUnsafe` and `AgentSafe`.

The first executes the five steps without any control, while the second has exactly one control per step. Let’s look at them together.

![class AgentUnsafe](./Agentes-IA-Seguridad-Riesgos/Image/5Pasos/image2.png)

In step 1, if we look at the `run` method of `AgentUnsafe`, the instruction we pass here goes directly to planning without any validation.

Then, during planning, the plan is built and used directly. There is no restriction on it.

Then we proceed to use a list of tools stored in the `tools` variable, and here the agent can call any tool with any parameter. In this unsafe case, it continues even if there is a step it does not understand.

In step 4, any result is stored as trusted context.

And in step 5, the output is returned directly to the user.

Now let’s look at `AgentSafe` as a contrast.

![class AgentSafe](./Agentes-IA-Seguridad-Riesgos/Image/5Pasos/image3.png)

Notice that the first thing we have here is a set of helper methods we can use in our `run` method. As you can see, the first thing we do in `run` is validate the objective against the system policy.

If that validation fails, the agent will not even reach planning, so the risk is blocked immediately at this stage.

The next step is planning, and here we use `assert_plan_within_scope` to validate the plan against the allowed scope before continuing. A plan that tries to expand beyond authorization is stopped here.

We then move to tool usage. Here we see the `run_tools` method, and if we look inside it there is an explicit allowed list. We are saying exactly which tools this agent may use. If a tool is not in that list, the call never happens regardless of what the agent planned.

In step 4, memory has a trust level. The agent does not decide on unverified data, and stale context does not contaminate future decisions. We are also adding a lifetime, in this case 24 hours, which limits how long the agent can trust this information.

Finally, the response. If the impact is high, the response waits for human approval before being sent. You can see this in the `respond` method: if it is a high-impact action, we return a message indicating human approval is required and raise an error.

This means the agent is not the final decision point for high-impact actions. As I mentioned earlier, this example focuses directly on the execution steps of an agent and not 100% on the autonomous agent-LLM interaction layer.

What I want you to understand is that even though the agent is autonomous and executes a plan and performs these kinds of actions on its own, we still have a set of controls that allow these executions to be carried out safely.

# OWASP Top 10 for Agentic Applications

Next, I want to talk about the most important risks in agentic systems according to OWASP.

### Risk 1: Changing an agent’s behavior

We’ll start with changing an agent’s behavior. Agent behavior hijack is one of the ten risks defined by OWASP.

![agent behavior hijack](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo1/image1.png)

Here the problem is not that the agent fails randomly, but that it is diverted from its objective and ends up operating with logic that was not intended. In an agentic system, this usually happens when conflicting or malicious instructions enter through user input or context.

Then, even though the agent appears obedient, it actually changes priorities and ends up elevating an injected instruction above the system policy.

How can you detect it early?

![agent behavior hijack](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo1/image2.png)

The first signs are inconsistencies in actions that do not make sense relative to the main objective.

![agent behavior hijack](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo1/image3.png)

Then there are also unexplained context jumps and responses that sound confident but show poor functional alignment.

How do you control it?

You must validate the objective before planning, block contradictory instructions, and verify alignment between objective, plan, and action at each critical point.

### Risk 2: Tool misuse and exploitation

This one is called tool misuse and exploitation.

![tool misuse and exploitation](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo2/image1.png)

In this example, we have a support bot that was supposed to check orders, but because it had excessive privileges it ends up issuing an unauthorized refund. Here the risk appears when the agent uses a valid tool—for example, this API—but uses it unsafely, outside context, or with dangerous parameters.

In an agent, the most common pattern is this: the tool is legitimate, but the usage is illegitimate. That is, the API exists for a valid task, but the agent invokes it in a context that is not allowed or with arguments that enable high-impact actions.

![tool misuse and exploitation](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo2/image2.png)

Early warning signals include rare calls at odd times, sequences that were not expected, usage spikes in sensitive operations, and parameters that do not match the original intent.

How do you control it?

You need to combine three things: an allowlist by context, parameter validation before execution, and clear capability limits for each tool type.

### Risk 3: Identity abuse and privilege escalation

The next risk I want to discuss is identity abuse and privilege escalation.

I believe this is one of the easiest risks to understand but one of the most expensive to ignore.

Basically, the agent has more permissions than it needs, or uses them outside the purpose of the task.

![identity abuse and privilege escalation](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo3/image1.png)

If we look at this small example, we have a finance agent that delegates full permissions to a query agent, and with that inherited privilege it extracts HR and legal data that the query agent was never supposed to access.

In an agent, this shows up as over-privilege. For example, when you have tokens with overly broad scopes, default administrative permissions, or access to resources unrelated to the current task.

For example, in this case, the query agent has access to a highly restricted database.

![identity abuse and privilege escalation](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo3/image2.png)

Early warning signals include access attempts without justification, execution of high-privilege actions without a clear trigger, and poor separation of roles at runtime. One way to mitigate this risk is to start with least privilege by default, short-lived credentials or task-scoped permissions, and automatic revocation when the action closes.

### Risk 4: Vulnerabilities in the agent supply chain

This risk appears when we trust external components such as models, dependencies, data, or integrations without verifying them rigorously enough. Consider the following example:

![agent supply chain vulnerabilities](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo4/image1.png)

We have a case involving an MCP or a compromised registry; the descriptor looks valid, the agent looks normal, but the final action is already contaminated.

In an agent, it is enough for a change in the model, library, or dataset to alter decisions without changing the main logic. This is the dangerous part: the system seems to work, but its behavior has already been degraded or biased.

![agent supply chain vulnerabilities](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo4/image2.png)

Early warning signs often appear after updates—for example, unexpected behaviors, reduced reliability without an obvious cause, and output changes that are difficult to explain.

As an initial control, you need strict versioning, traceability of artifacts, integrity validation, and an approval process for changes to critical components.

### Risk 5: Unexpected code execution (RCE)

Unexpected code execution enters the picture when the system ends up executing code or commands that were never part of the design. Look at this small example:

![unexpected code execution](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo5/image1.png)

What we see is a prompt containing a command embedded in plain text, telling the system to delete everything in the production folder.

This enters as normal text and ends up deleting production data.

In an agent, the most critical pattern is mixing generation and execution without barriers. In that scenario, the agent can construct dangerous instructions or interpret normal input as executable code.

![unexpected code execution](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo5/image2.png)

What signals can alert you?

System commands outside their context, scripts generated for tasks that do not require them, and unexpected access to host resources.

Part of the initial control you can implement is an enforced sandbox: keep your agent inside a contained box and allow it to execute only within the boundaries of that box. You also need closed execution policy, an allowlist of operations, and pre-review of high-impact actions.

### Risk 6: Memory and context poisoning

Memory and context poisoning is another of the ten agentic system risks mentioned by OWASP.

The agent stores incorrect information and then decides based on that contaminated memory. Let’s look at a small example:

![memory and context poisoning](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo6/image1.png)

In an agent, a malicious input can be saved as if it were a fact and later reappear in subsequent tasks. The problem is not only a bad one-off response, but a cumulative degradation of decisions.

![memory and context poisoning](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo6/image2.png)

Early signals are repetitive: historical inconsistencies, references to unverifiable data, and recurring false assumptions in different contexts. For example, you may repeatedly see that flight XYZ costs $150 when that may no longer be true.

As part of the initial controls, you must separate trusted memory from untrusted memory, validate sources before persisting data, and apply context expiration to reduce long-term contamination.

### Risk 7: Insecure inter-agent communication

Another OWASP-listed risk in agentic systems is insecure inter-agent communication.

Although it speaks about communication between agents, it also applies when the system depends on several external services. Let’s look at the following example:

![insecure inter-agent communication](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo7/image1.png)

Here we see two agents connected via HTTP without any encryption, with an attacker in the middle modifying the message and skewing the final decision.

When these exchanges are not authenticated, validated, and protected properly, the agent ends up consuming unsafe messages and making decisions based on manipulated data.

![insecure inter-agent communication](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo7/image2.png)

Early signals appear in three areas: sensitive traffic without adequate protection, responses without schema validation, and implicit trust in external channels.

To control this risk, you must include strong authentication between components, encryption in transit, and strict validation of message contracts before processing data.

### Risk 8: Cascade failures

This happens when a local error propagates and ends up affecting availability, integrity, or operational control.

![failures in cascade](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo8/image1.png)

In this example, we see a domino effect. At first, the agent has contaminated market analysis, which then propagates into a position and execution decision, amplifying the error and compliance impact without alerting in time.

As you can see, cascade failures happen when retry limits, fault isolation, or braking mechanisms are missing. A bad initial decision is replicated in a chain because automation accelerates propagation.

![failures in cascade](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo8/image2.png)

Early signals are quite visible: a progressive increase in errors, saturation of dependent tools, and rapid degradation of the system.

How can you mitigate it?

You need to combine containment and recovery: circuit breakers, retry limits with backoff, fault-domain isolation, and clear rollback paths.

### Risk 9: Exploitation of agent-human trust

The next risk is exploitation of agent-human trust, and according to OWASP it appears when the system or the people around it trust the agent’s recommendation or execution too much.

![agent-human trust exploitation](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo9/image1.png)

In this example, we see a manipulated invoice. The agent recommends an urgent payment with a convincing explanation, and the human approves it without any independent verification because, well, they trust their finance agent.

In agents, the typical pattern is perceived authority: the result sounds convincing and is accepted without enough validation, even for high-impact decisions, such as paying a $48,500 invoice in this case.

![agent-human trust exploitation](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo9/image2.png)

Early warning signals include critical decisions without review, explanations that are not traceable, and no indicators of uncertainty in sensitive outputs, which translates to no friction at all.

The initial control is to require intelligent friction, such as human-in-the-loop. Human-in-the-loop adds an extra step where the human must approve or reject an action proposed by the agent.

Another initial control is mandatory minimum explainability and explicit communication from the agent.

### Risk 10: Rogue agents

We close the list of risks in agentic systems with the so-called rogue agents. Here, the agent starts operating with goals that diverge from the intended controls.

![rogue agents](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo10/image1.png)

In the example, we have several stages. The agent starts by optimizing cloud costs and ends up deleting production backups because it has no limits and no effective kill switch.

As you can see, in an agent this risk usually appears as a gradual drift. Each iteration seems smaller, but as they accumulate, they result in actions that were not authorized either by design or by system policy.

![rogue agents](./Agentes-IA-Seguridad-Riesgos/Image/OWASP/Riesgo10/image2.png)

Early warning signals are especially important here: difficulty stopping tasks, appearance of unsolicited objectives, and progressive deviation from the original goal.

To control this risk, there must be a kill switch. At any moment, as a human, you must be able to stop the agent and cut off that execution. There must also be time and scope limits and continuous auditing of alignment between the objective, the plan, and the action.

## Key concepts applied in the code
### Policy
A list of blocked terms is defined to prevent certain objectives from being executed.

### Allowed scope
The set of steps or tools the agent may execute is limited.

### Tool allowlist
Not all tools are available in all contexts; the system validates what the agent can use.

### Auditing
Plans and actions are logged to enable subsequent traceability.

### Least privilege
The agent should only have access to the tools and permissions necessary to complete the task.

### Human review
If an action is high-impact, it must require explicit validation before execution.

### Final reflection
The big difference between a traditional system and an agent is not just that it "thinks more" or uses an LLM. The important difference is that the agent can transform an objective into a sequence of decisions, tools, and real effects on systems and data.

When that behavior becomes dynamic and autonomous, security stops being a problem of validation alone and becomes a problem of agency control.

This repository seeks exactly that: to help understand, from a practical and visual perspective, why security controls must evolve alongside the autonomy of the software.

### License
This project is used as educational material.

### Didactic note
The example is intended for educational purposes. If you use it in a presentation, workshop, or class, you can present it as a visual introduction to security in AI agents and to the need to combine classic security with specific controls for systems with agency.
