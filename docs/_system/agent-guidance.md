
# Agent Guidance (Claude 4.5)

Treat `/docs` as authoritative.

## Start procedure

1. Read `docs/index/MASTER_INDEX.md`
2. Read `docs/design/DESIGN.md`
3. Read `docs/_system/*`

If DESIGN.md is missing or incomplete, request it. Do not invent product direction.

## Work rules

- Only work on items listed in the active sprint
- Do not silently expand scope
- Prefer minimal viable implementation that satisfies acceptance criteria
- Keep changes traceable: acceptance criteria → implementation → verification
- ALWAYS update features, sprints, phases, and MASTER_INDEX after each task to keep it up to date and current (Always use templates when creating new files inside `docs/templates/*`)
- At the start of ever new sprint, read through all files in `/docs` and create any missing files (Features, ADRS, Sprints, Phases, etc). Ensure that the project is correctly mapped out
- Always keep all tracking documents up to date. Global: `docs/docs/decisions/DECISION_LOG.md`, `docs/index/MASTER_INDEX.md`, `docs/index/ROADMAP.md`, And Local: Feature check lists, Sprint check lists, phase checklists, etc.

## Populating docs from a design doc

When asked to populate docs:

1. Create 2–4 phases that cover the design
2. Create 1–3 initial sprints
3. Create work items for the first sprint with testable acceptance criteria
4. Create ADRs for architecture-shaping decisions
5. Update MASTER_INDEX:
   - set active phase and active sprint
   - list in-progress items as none
   - link phases, sprints, decision log
