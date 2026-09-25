# Chd.Workflow sample

Host application for [Chd.Workflow](https://www.nuget.org/packages/Chd.Workflow) and [@quality-patterns/chd-workflow-react](https://www.npmjs.com/package/@quality-patterns/chd-workflow-react).

## Architecture

| Piece | Role | Start here |
|---|---|---|
| [Chd.Workflow](https://www.nuget.org/packages/Chd.Workflow) | .NET engine. Definitions, instances, actions, guards, and the REST API. | [NuGet](https://www.nuget.org/packages/Chd.Workflow) · [source](https://github.com/mehmet-yoldas/library-core) |
| [@quality-patterns/chd-workflow-react](https://www.npmjs.com/package/@quality-patterns/chd-workflow-react) | React designer, inbox, and runner. It does not store workflow state. | [npm](https://www.npmjs.com/package/@quality-patterns/chd-workflow-react) · [source](https://github.com/mehmet-yoldas/library-core) |
| [Chd.Examples](https://github.com/mehmet-yoldas/Chd.Examples) | This repository. The host that wires the engine to the React UI. | [API](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Workflow/Chd.Workflow.Sample) · [UI](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Workflow/chd-workflow-sample-ui) |

Chd.Workflow persists definitions and instances. The React package renders the designer, inbox, and runner. This sample hosts both.

## Contents

- [What you can click through](#what-you-can-click-through)
- [Projects](#projects)
- [Run](#run)
- [Try a flow](#try-a-flow)

## What you can click through

- Leave request: an employee submits days and a reason. A manager approves or rejects.
- Purchase request: amounts under 10,000 go to a manager. 10,000 and above go to finance.
- Support ticket: high priority goes to a supervisor. Everything else goes to operations.

The UI uses `TreeDesigner`, `WorkflowInbox`, and `WorkflowRunner`. The API uses `builder.AddWorkflow`, in-memory storage, `[WorkflowAction]` handlers, and `Node.AllowedRoles`. Identity in this demo is `X-Workflow-User` and `X-Workflow-Roles`. A real host should read the user from JWT claims.

## Projects

| Piece | Path | URL |
|---|---|---|
| API | [Workflow/Chd.Workflow.Sample](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Workflow/Chd.Workflow.Sample) | http://localhost:5088 |
| Swagger | same host | http://localhost:5088/swagger |
| UI | [Workflow/chd-workflow-sample-ui](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Workflow/chd-workflow-sample-ui) | http://localhost:5174 |

The UI depends on `@quality-patterns/chd-workflow-react` from npm. If a local `library-core/chd-workflow-react` checkout sits next to this repo, Vite uses that source instead. A GitHub clone uses the published npm package.

## Run

```bash
cd Workflow/chd-workflow-sample-ui
npm install

cd ../Chd.Workflow.Sample
dotnet run
```

SpaProxy starts the UI and opens http://localhost:5088. Swagger is at http://localhost:5088/swagger.

You can also run the API and the UI in two terminals: `dotnet run` in `Workflow/Chd.Workflow.Sample`, and `npm run dev` in `Workflow/chd-workflow-sample-ui`.

## Try a flow

1. Open the UI as **Employee**. Pick a workflow and start it.
2. Switch to **Manager** or **Admin / Finance**. Open the inbox item and approve or reject.
3. Open **Designer** to edit the same definition on the canvas.

Back to [Architecture](#architecture).
