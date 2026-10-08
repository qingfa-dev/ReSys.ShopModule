# Re.Shop

**An online shop, built as a monorepo.**

> **Status:** early scaffold. Features listed below are planned, not yet implemented.

## Features (planned)

- Product catalog with search and categories
- Shopping cart and checkout
- Order history
- Admin panel for managing products and orders

## Tech stack

- **Backend:** ASP.NET Core Web API
- **Frontend:** React single-page application
- **Database:** PostgreSQL with Entity Framework Core

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)
- PostgreSQL

### Initial setup

The solution has not been scaffolded yet. Once project files exist, expect commands along these lines:

```bash
# API
cd apps/api
dotnet restore
dotnet run

# Web
cd apps/web
npm install
npm run dev
```

## Repository layout (planned)

```
Re.Shop/
├── apps/
│   ├── api/          # ASP.NET Core Web API
│   └── web/          # React SPA
├── packages/
│   └── shared/       # shared types and contracts
└── README.md
```

## Contributing

Issues and pull requests are welcome once the scaffold is in place.

## License

MIT
