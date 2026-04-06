# Chd.Pos.Web.Local.Npm – React Frontend for the POS Demo

A minimal React application for testing the [@mehmetyoldas/chd-auto-ui-react](https://www.npmjs.com/package/@mehmetyoldas/chd-auto-ui-react) npm package with the [Chd.Pos.Api](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Chd.Pos.Api) backend.

---

## 📝 Table of Contents

- [About](#about)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Testing Local Package Changes](#testing-local-package-changes)
- [Login Credentials](#login-credentials)
- [Project Structure](#project-structure)
- [Related Projects](#related-projects)

---

## About

This project exists so you can see `DynamicGrid`, `DynamicForm`, and `Login` changes in a real browser before publishing the npm package. It is not a production application — it is a test harness.

The app connects to `Chd.Pos.Api`, fetches all entity metadata on load, and renders a sidebar with entity links. Selecting an entity shows the `DynamicGrid`. Clicking Edit or Create opens the `DynamicForm`.

---

## ⚙️ Requirements

- Node.js 18+
- [Chd.Pos.Api](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Chd.Pos.Api) running on `http://localhost:5218`

---

## 🚀 Getting Started

```powershell
cd Chd.Pos.Web.Local.Npm
npm install
npm start
```

The app runs on `http://localhost:3000`. API requests to `/api/*` are proxied to `http://localhost:5218` (configured in `package.json` via the `proxy` field).

---

## 🔄 Testing Local Package Changes

The package is referenced from a local `.tgz` file, not the npm registry. When you change something in `chd-auto-ui-react/src`, you need to rebuild and reinstall:

```powershell
# Step 1 — rebuild the package
cd C:\Projects\library-core\chd-auto-ui-react
npm run build
npm pack

# Step 2 — reinstall in the test app
cd C:\Projects\library-core\Chd.Pos.Web.Local.Npm
Remove-Item package-lock.json -Force
npm install
```

Then restart `npm start`. The browser will reload with the updated components.

> **Why delete package-lock.json?**
> npm caches `.tgz` files by their hash. If the file content changes but the filename stays the same, npm may skip reinstalling. Deleting the lock file forces a clean resolution.

---

## 🔑 Login Credentials

| Username | Password | Roles |
|---|---|---|
| Admin | test | User, Admin |
| Manager | test | User, Manager |
| User | test | User |

Role-based permissions are set on the DTOs in `Chd.Pos.Core`. Logging in as different users will show or hide the Create, Edit, and Delete buttons accordingly.

---

## 🏗️ Project Structure

```
Chd.Pos.Web.Local.Npm/
├── public/
│   └── index.html
├── src/
│   ├── App.tsx          # Main layout, auth state, entity routing
│   ├── index.tsx
│   └── pages/
│       └── MetadataDemo.tsx   # Raw metadata viewer
└── package.json
```

`App.tsx` is the best place to test new component props. The current version demonstrates `renderActions`, `renderCell`, `logo`, and `children` on `Login` and `DynamicForm`.

---

## 🔗 Related Projects

| Project | Description |
|---|---|
| [Chd.AutoUI](https://www.nuget.org/packages/Chd.AutoUI) | .NET package behind the metadata API |
| [chd-auto-ui-react](https://www.npmjs.com/package/@mehmetyoldas/chd-auto-ui-react) | The npm package being tested here |
| [AutoUI Demo](https://github.com/mehmet-yoldas/Chd.Examples/tree/master/Chd.Pos.Api) | Backend API this app connects to |
| [All Demo Projects](https://github.com/mehmet-yoldas/Chd.Examples) | All CHD examples, benchmarks and test projects |

---

## 📄 License

MIT



