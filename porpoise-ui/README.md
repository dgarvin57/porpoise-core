# Porpoise UI

Modern Vue 3 web interface for the Porpoise political polling analytics engine.

## Technology Stack

- **Vue 3** with Composition API and `<script setup>`
- **Vite** for fast development and optimized builds
- **Axios** for API communication
- **Vue Router** for navigation

## Development Setup

```bash
# Install dependencies
npm install

# Start development server (http://localhost:5173)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Project Structure

```
porpoise-ui/
├── src/
│   ├── components/     # Reusable Vue components
│   ├── views/          # Page-level components
│   ├── services/       # API service layer
│   ├── router/         # Vue Router configuration
│   ├── App.vue         # Root component
│   └── main.js         # Application entry point
├── public/             # Static assets
└── vite.config.js      # Vite configuration
```

## API Integration

The UI connects to the Porpoise API at `http://localhost:5000` (configurable in services).

Requires:
- Porpoise.Api running
- Valid X-Tenant-Id header for multi-tenancy

## Features Implemented

- Survey import from .porps files
- Project and survey listing
- Basic navigation structure

## IDE Recommendations

- [VSCode](https://code.visualstudio.com/)
- [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) extension for Vue 3 support

For more about Vue 3, see the [Vue Docs](https://vuejs.org/).
