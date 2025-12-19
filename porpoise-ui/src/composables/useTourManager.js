/**
 * Shared tour state management across all application tours
 * Manages localStorage keys and provides unified interface for tour completion tracking
 */

const TOUR_KEYS = {
  results: 'porpoise_results_tour_completed',
  crosstab: 'porpoise_crosstab_tour_completed',
  statsig: 'porpoise_statsig_tour_completed',
  start: 'porpoise_start_tour_completed'
}

export function useTourManager() {
  /**
   * Check if a specific tour has been completed
   * @param {string} tourName - One of: 'results', 'crosstab', 'statsig', 'start'
   * @returns {boolean}
   */
  function hasTourBeenCompleted(tourName) {
    const key = TOUR_KEYS[tourName]
    if (!key) {
      console.warn(`Unknown tour name: ${tourName}`)
      return false
    }
    return localStorage.getItem(key) === 'true'
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
   * @param {string} tourName - One of: 'results', 'crosstab', 'statsig', 'start'
   */
  function resetTour(tourName) {
    const key = TOUR_KEYS[tourName]
    if (!key) {
      console.warn(`Unknown tour name: ${tourName}`)
      return
    }
    localStorage.removeItem(key)
  }

  /**
   * Reset all tours
   */
  function resetAllTours() {
    Object.values(TOUR_KEYS).forEach(key => localStorage.removeItem(key))
  }

  return {
    hasTourBeenCompleted,
    hasAnyTourBeenCompleted,
    markTourCompleted,
    resetTour,
    resetAllTours
  }
}
