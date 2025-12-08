# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the app (Vite will use VITE_API_BASE_URL from environment)
RUN npm run build

# Production stage - serve with Node
FROM node:20-alpine
WORKDIR /app

# Copy built files
COPY --from=build /app/dist ./dist
COPY --from=build /app/package*.json ./

# Install only vite for preview server
RUN npm install vite

EXPOSE 3000

# Serve the built app
CMD ["npx", "vite", "preview", "--host", "0.0.0.0", "--port", "3000"]
