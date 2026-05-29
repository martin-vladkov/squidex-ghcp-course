![Squidex Logo](https://raw.githubusercontent.com/Squidex/squidex/master/media/logo-wide.png "Squidex")

# What is Squidex??

Squidex is an open source headless CMS and content management hub. In contrast to a traditional CMS Squidex provides a rich API with OData filter and Swagger definitions. It is up to you to build your UI on top of it. It can be website, a native app or just another server. We build it with ASP.NET Core and CQRS and is tested for Windows and Linux on modern browsers.

[![Discourse topics](https://img.shields.io/discourse/https/support.squidex.io/topics.svg)](https://support.squidex.io) 
[![Dev](https://github.com/Squidex/squidex/actions/workflows/dev.yml/badge.svg)](https://github.com/Squidex/squidex/actions/workflows/dev.yml)
![Docker Pulls](https://img.shields.io/docker/pulls/squidex/squidex)

Read the docs at [https://docs.squidex.io/](https://docs.squidex.io/) (work in progress) or just check out the code and play around.

## How to make feature requests, get help or report bugs? 

Please join our community forum: https://support.squidex.io

## Status

Current Version ![GitHub release](https://img.shields.io/github/release/squidex/squidex) Roadmap: https://trello.com/b/KakM4F3S/squidex-roadmap

## Prerequisites

* [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
* [Node.js](https://nodejs.org/en/) (development only)
* [MongoDB](https://www.mongodb.com/), PostgreSQL, MySQL or SQLServer
* [.NET 8 SDK](https://dotnet.microsoft.com/download#/current) (Already part of Visual Studio 2022 v17.8)

## Deployment Options

| Platform | Documentation | Quicklink | 
| -------- | ------------- | ---- |
| Azure    | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-azure) | [![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fsangramrath%2Fsquidex-docs2%2Fmaster%2Fscripts%2Fsquidex-minimal-azure-arm.json)
| AWS      | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-on-aws) |
| Docker   | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-docker) | [![Docker Compose](https://img.shields.io/badge/-docker--compose.yml-2496ED?style=for-the-badge&logo=docker&logoColor=ffffff)](https://github.com/Squidex/squidex-hosting/blob/master/docker-compose/docker-compose.yml) |
| GCP      | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-on-gcp) |
| Heroku   | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-on-heroku) | [![Deploy to Heroku](https://img.shields.io/badge/-Deploy%20to%20Heroku-430098?style=for-the-badge&logo=heroku&logoColor=ffffff)](https://heroku.com/deploy?template=https://github.com/Squidex/squidex) |
| IIS      | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-iis) |
| K8S      | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-on-kubernetes) |
| Render   | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-on-render) | [![Deploy to Render](https://img.shields.io/badge/-Deploy%20to%20Render-44E4B4?style=for-the-badge&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAADyklEQVR4Xu1aT0gUURh/k5qV2y5pEbpqHiqqy4IIQUGghbNOK53WkwQFdekqRHXNDkHXIKhuXdZTZu6MRkbkJRFJKAKNUnQPEir+IXdr98XbdXbfzrznjLvvObM7s5dld7553/f93u/78/4IwOEfweH+AxcAlwEOR8ANAYcTwE2CfEIAAqEzpjSmQPLobhj2zi9N4fLiotxKet87vvZloKcnqT6jyW2sx2fGz1xb38kGJgCEv0b2h/yBvuu+0/0kZRAAAtX0/wqCkGcPhBAJ6T7SbNQXPSWtqQ9ocsFFpUNpDI5xBeBnYn2+pcrTtJuZpsnSANBCRQNAK8cVgM7F4W6lvmuQZRYxy4De2ajvlSkGvO1QGkPsGXB/aeJF/7G2myxmHR/DGIBMREizsnUhcO/3xMtHdW03WDuPxjMGIKPVshwgLshXZb84lO88OcUVApDtAYAwBUn5XHV2Kbn1tzc2FhptlkYKAcCKd0yXwbnExnxzVQ012z9emb5ztzbw1AonitFpCgBU5yPnwnGaouCCcl5pCn4uxhCr3jUFwLPVbw9v+84+0BkJAXiyMn2rry7w3CoHkN7u2JtDJP0HPv2J4x0jScYUALROaym5lTheebDaSueRbu6dIE2BOBe9PNIivS9vACAQIIApIn00vbtVQOgnKFOambTCV2KDzaP13XOlBUDGWiYAoKVmtEGcJCULbeNiHwYwBkBuECdLiwEMQwAxwO4AFMM8wzLoAuAyYDsECIs+uyRBx4dACkJIimVmZTAvCWJMsAsDuLbCpZAE9xYACADiG6KcAxmQ4b8aBQ4EIJNvHQ+AWnbswgDuZdDuiyHuAOTKYH435BgGuIuhklsO89wP2I4C9LXPjltiWJTyaYWxjGOXHMC3E/wlt8onSm1HiGcIZNsgG7XCKZjeu9aeWzoqBEhn1EwACMNIxcaypyZNqmUAQG0uCUTrcvd0imlGin23a2bYSxrDc7Jrc0AQspepCj4aK9ZAO79vuClqZ+NZ2OYCYISi2R0hdEQ9WB/aNHOAQqvb1Csyaobb/t7TO0Kirg/I5VvcYBUAUjY2e/fHSC49NgRA+kG5KIkrhwAEYwwuSuIM0DpHAqAQBtA2WEzfFE33AXrNTMpgFgCDcwGuIaDxbW9DwOTJkAsAzyRoJQMufn992OOpbsvZ8A8AUJn+id/EDkciFWsXvJdwW5EUktbe2BYX5HZSrtDLDbWrunB5r1/8iHd4tPHiNdVTH460r+5U6dw+wKgPKPfnLgPKfYaN/HMZYIRQuT93GVDuM2zk338nAalfI74e1QAAAABJRU5ErkJggg==&logoColor=ffffff)](https://render.com/deploy?repo=https://github.com/Squidex/squidex) |
| Vultr    | [Docs](https://docs.squidex.io/01-getting-started/installation/platforms/install-on-vultr) | [![Deploy to Vultr](https://img.shields.io/badge/-Deploy%20to%20Vultr-007BFC?style=for-the-badge&logo=vultr&logoColor=ffffff)](https://www.vultr.com/marketplace/apps/squidex) |

## Contributors

### Core Team and Founders

* [Sebastian Stehle](https://github.com/SebastianStehle) Software Engineer, Germany

### Contributors

* [civicplus](https://www.civicplus.com/) ([Avd6977](https://github.com/Avd6977), [dsbegnoce](https://github.com/dsbegnoche)): Google Maps support, custom regex patterns and a lot of small improvements.
* [cpmstars](https://www.cpmstars.com): Asset support for rich editor.
* [guohai](https://github.com/seamys): FTP asset store support, Email rule support, custom editors and bug fixes.
* [pushrbx](https://pushrbx.net/): Azure Store support.
* [razims](https://github.com/razims): GridFS support.
* [sauravvijay](https://github.com/sauravvijay): Kafka Rule action.

## Running Tests and Coverage

### Quick test run (GHCP exercises)

```bash
# From repo root
./test-crawl.sh
```

### Coverage report (Cobertura XML + HTML)

Coverlet is pre-configured via `backend/tests/coverlet.runsettings.xml`.

**macOS / Linux — individual suite:**

```bash
cd backend/tests

# Core model + operations
dotnet test Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
  --filter "Category!=Dependencies&Category!=TestContainer" \
  --collect "XPlat Code Coverage" \
  --results-directory ./_coverage-out \
  --settings coverlet.runsettings.xml

# Domain entities (AppDomainObject etc.)
dotnet test Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
  --filter "Category!=Dependencies&Category!=TestContainer" \
  --collect "XPlat Code Coverage" \
  --results-directory ./_coverage-out \
  --settings coverlet.runsettings.xml
```

The Cobertura XML is written to `backend/tests/_coverage-out/<guid>/coverage.cobertura.xml`.

**Generate an HTML report** (requires [ReportGenerator](https://github.com/danielpalme/ReportGenerator)):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator \
  -reports:"backend/tests/_coverage-out/**/coverage.cobertura.xml" \
  -targetdir:"backend/tests/_coverage-out/report" \
  -reporttypes:Html

open backend/tests/_coverage-out/report/index.html
```

**Windows (PowerShell):** use `backend/tests/RunCoverage.ps1 -testAll` — see the script header for individual suite flags.

### Baseline coverage (as of Walk Ex2)

| Suite | Packages covered | Line % | Branch % |
|---|---|---|---|
| `Squidex.Domain.Apps.Core.Tests` | Core.Model · Core.Operations · Events · Infrastructure · Shared | **57.2 %** | **57.2 %** |
| `Squidex.Domain.Apps.Entities.Tests` | + Entities · Users · Web | **56.9 %** | **48.4 %** |

> These numbers reflect the two suites relevant to the GHCP exercises. Full project coverage (all suites) will be higher once Infrastructure, Users, and Web test suites are also collected.

### Including coverage in a PR description

Copy this snippet into your PR body, replacing the placeholders:

```
## Evidence
- Tests: `dotnet test … --collect "XPlat Code Coverage"`
  - Suite: Squidex.Domain.Apps.Core.Tests — Line: XX.X %  Branch: XX.X %
  - Suite: Squidex.Domain.Apps.Entities.Tests — Line: XX.X %  Branch: XX.X %
- Coverage delta vs baseline: +/- X.X pp (line)
```

## Contributing

Please create issues to report bugs, suggest new functionalities, ask questions or just share your thoughts about the project. We will really appreciate your contribution, thanks.

For the **GHCP Walk track** — plan-first workflow, PR expectations, branching strategy, and Copilot usage guidelines — see [CONTRIBUTING.md](CONTRIBUTING.md).

New to the Walk track? Paste [docs/onboarding-walk.md](docs/onboarding-walk.md) into Copilot Chat to get oriented in one step.

## Cloud Version

Although Squidex is free it is also available as a Saas version on [https://cloud.squidex.io](https://cloud.squidex.io).
