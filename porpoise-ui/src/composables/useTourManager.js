/**
 * Shared tour state management across all application tours
 * Manages localStorage keys and provides unified interface for tour completion tracking
 */

// Each tour has its own completion key, but they share a skip-all flag
const TOUR_KEYS = {
  results: 'porpoise_results_tour_completed',
  crosstab: 'porpoise_crosstab_tour_completed',
  statsig: 'porpoise_statsig_tour_completed',
  start: 'porpoise_start_tour_completed'
}

// Shared flag for skipping all analytics tours
const SKIP_ALL_KEY = 'porpoise_analytics_tours_skipped'

export function useTourManager() {
  /**
   * Check if a specific tour has been completed OR if all tours were skipped
   * @param {string} tourName - One of: 'results', 'crosstab', 'statsig', 'start'
   * @returns {boolean}
   */
  function hasTourBeenCompleted(tourName) {
    const key = TOUR_KEYS[tourName]
    if (!key) {
      console.warn(`Unknown tour name: ${tourName}`)
      return false
    }
    // Check if this specific tour was completed OR if user skipped all tours
    const isSkippedAll = localStorage.getItem(SKIP_ALL_KEY) === 'true'
    const isCompleted = localStorage.getItem(key) === 'true'
    return isCompleted || isSkippedAll
  }

  /**
   * Check if any tour has been completed
   * Useful for showing "Restart Tour" buttons
   * @returns {boolean}
   */
  function hasAnyTourBeenCompleted() {
    return Object.values(TOUR_KEYS).some(key => localStorage.getItem(key) === 'true')
  }

  /**
   * Mark a specific tour as completed
   * @param {string} tourName - One of: 'results', 'crosstab', 'statsig', 'start'
   */
  function markTourCompleted(tourName) {
    const key = TOUR_KEYS[tourName]
    if (!key) {
      console.warn(`Unknown tour name: ${tourName}`)
      return
    }
    localStorage.setItem(key, 'true')
  }

  /**
   * Reset a specific tour (mark as not completed)
   * Also clears the skip-all flag to allow tours to run again
   * @param {string} tourName - One of: 'results', 'crosstab', 'statsig', 'start'
   */
  function resetTour(tourName) {
    const key = TOUR_KEYS[tourName]
    if (!key) {
      console.warn(`Unknown tour name: ${tourName}`)
      return
    }
    localStorage.removeItem(key)
    // Also clear skip-all flag when resetting any individual tour
    localStorage.removeItem(SKIP_ALL_KEY)
  }

  /**
   * Mark all tours as skipped (when user cancels/skips any tour)
   */
  function skipAllTours() {
    localStorage.setItem(SKIP_ALL_KEY, 'true')
  }

  /**
   * Reset all tours
   */
  function resetAllTours() {
    Object.values(TOUR_KEYS).forEach(key => localStorage.removeItem(key))
    localStorage.removeItem(SKIP_ALL_KEY)
  }

  return {
    hasTourBeenCompleted,
    hasAnyTourBeenCompleted,
    markTourCompleted,
    resetTour,
    resetAllTours,
    skipAllTours
  }
}
