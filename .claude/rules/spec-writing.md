# Writing specs

Applies on top of `spec-workflow.md`. Its core principle: **do not assume. If something is unclear, ask.**

## Ask, don't assume
- Ask with the `AskUserQuestion` tool (load it via ToolSearch if needed), not as plain text in the reply, and wait for the answers before drafting. Offer concrete options with a recommended one first.
- Before writing a requirements document, ask the user about anything ambiguous: scope, target users, platforms, edge cases, data formats, performance or accessibility expectations, and what is explicitly out of scope.
- Ask at every phase, not only the first. A design or task decision that the approved requirements don't settle is a question for the user.
- Never fill a gap with a guess, and never use an "Assumptions" list as a substitute for asking. Every open point (behavior of an edge case, which objects are allowed, where a command sits, a limit or tolerance) is asked with `AskUserQuestion` before it goes into a document. Several rounds of questions are fine, and the questions come before the draft, not at approval time.
- Only if the user is not available at all, write a working assumption under an "Assumptions" heading, say so in the reply, and ask about it as soon as the user is back. A question being minor is no reason to assume.
- Ask a few focused questions in one batch, with a recommended default for each where you have one. Do not ask what the code, steering docs, or existing specs already answer. Read them first.
- When the user's request is vague, restate your understanding in a sentence or two and confirm before drafting.
- Conflicting sources (the request versus an approved spec, or the spec versus the code) are a question for the user. Do not silently pick one.

## Requirements
- Write user stories with EARS-style acceptance criteria. Each criterion must be testable and unambiguous.
- Include non-goals. Keep one concern per requirement and give each a stable ID (`R1`, `R2`...) that design and tasks can cite.
- Do not describe implementation in requirements.

## Design
- Read the existing code first. Reuse existing patterns, base classes, and utilities, and name them in the document.
- Follow the project rules (`avalonia-mvvm.md`, `csharp.md`). Record any deviation and the reason.
- Cover the data flow, the affected files and folders, error handling, and how it will be tested.
- When there are real alternatives, state the trade-offs and recommend one. Ask the user if the choice is significant.
- Do not introduce new dependencies without asking.

## Tasks
- Each task is atomic (1-3 files), ordered by dependency, and references the requirement IDs it satisfies.
- Fill in every `_Prompt` field (role, task, restrictions, `_Leverage`, `_Requirements`, success criteria) as the guide requires.
- Include test tasks alongside the code they cover.
- Each task has a verifiable "done" condition. No vague tasks such as "polish UI".

## Revision and scope
- Treat approval comments as instructions. If a comment is unclear, ask for clarification instead of interpreting it.
- Change only what the comments cover. Do not rewrite approved sections unprompted.
- If implementation reveals a flaw or gap in an approved document, stop, tell the user, and revise the spec and get it re-approved. Do not deviate silently.
- Keep documents concise. Prefer short sections and lists over prose, and do not pad.

## Implementation
- If a task is unclear or conflicts with the code, ask before coding.
- Report what was done and what was not. Never mark a task `[x]` that is partial, untested, or unlogged.
