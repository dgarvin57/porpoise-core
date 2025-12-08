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

# Copy built files and necessary config
COPY --from=build /app/dist ./dist
COPY --from=build /app/package*.json ./
COPY --from=build /app/vite.config.js ./

# Install only production dependencies
RUN npm install vite

# Railway will set PORT, default to 3000
ENV PORT=3000

# Serve the built app
CMD ["sh", "-c", "npx vite preview --host 0.0.0.0 --port $PORT"]
