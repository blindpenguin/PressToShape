# Spec workflow (spec-workflow MCP)

Non-trivial features go through the `spec-workflow` MCP server (configured in `.mcp.json`). Specs live in `.spec-workflow/specs/<kebab-case-name>/`.

## When to use a spec
- Use a spec for any new feature, multi-file change, or architectural change.
- Skip it for typo fixes, renames, dependency bumps, and single-file bug fixes.
- If unsure, ask the user whether they want a spec.

## Process
1. Call `spec-workflow-guide` first, every time, before any other spec tool. Follow it exactly.
2. Read the approved steering docs before writing anything: `.spec-workflow/steering/product.md` (scope and non-goals), `tech.md` (stack and open decisions) and `structure.md` (target layout and dependency direction). Specs must stay consistent with them. If a spec needs to contradict one, ask the user and update the steering doc through the approval flow.
3. Work one spec at a time, in order: Requirements, Design, Tasks, Implementation. Never skip or merge phases.
4. Check `.spec-workflow/user-templates/` before `.spec-workflow/templates/`.
5. After each document, request approval with the `approvals` tool (`filePath` only, never content), then poll `status`.
6. Verbal approval ("looks good", "approved") does not count. Only the dashboard or extension status counts. If `needs-revision`, update the document from the comments and request a new approval.
7. After approval, `approvals delete` must succeed before the next phase. If it fails, return to polling.

## Implementing tasks
- Run `spec-status`, then read `tasks.md` and follow each task's `_Prompt` and `_Leverage`.
- Set the task to `[-]` before starting. Set it to `[x]` only after the code is done and logged.
- Search `Implementation Logs/` (grep several terms) before writing code, to reuse what exists.
- Call `log-implementation` after each task, with `artifacts` filled in (classes, functions, components, integrations).
- Keep tasks atomic (1-3 files). Do not implement work that is not in an approved task. If scope changes, revise the spec and get it re-approved.

## Housekeeping
- Steering docs are created only when the user asks for them (`steering-guide`).
- Dashboard: `npx -y @pimzino/spec-workflow-mcp@latest --dashboard`. If it is not running, tell the user and wait. Do not work around approvals.
- Use `.spec-workflow/` as the source of truth for what was decided. Do not duplicate it in rule files.
