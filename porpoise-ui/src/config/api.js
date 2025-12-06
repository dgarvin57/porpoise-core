/**
 * API Configuration
 * Central configuration for API endpoints
 * 
 * The API base URL is read from environment variables:
 * - Development: VITE_API_BASE_URL from .env.development
 * - Production: VITE_API_BASE_URL from .env.production (Railway deployment)
 * 
 * Falls back to localhost if not configured.
 */

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5107'

/**
 * Helper function to construct full API URLs
 * @param {string} path - API path (should start with /)
 * @returns {string} Full API URL
 */
export function getApiUrl(path) {
  // Ensure path starts with /
  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  return `${API_BASE_URL}${normalizedPath}`
}
