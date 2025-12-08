# Build stage - updated to force cache bust
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

# Production stage - use nginx to serve static files
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Remove default nginx static files
RUN rm -rf ./*

# Copy built files from build stage
COPY --from=build /app/dist .

# Copy nginx configuration template
COPY nginx.conf /etc/nginx/templates/default.conf.template

EXPOSE 80

# Set default PORT if not provided
ENV PORT=80

# Use ENTRYPOINT to prevent Railway from overriding the command
ENTRYPOINT ["nginx", "-g", "daemon off;"]
