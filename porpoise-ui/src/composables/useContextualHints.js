import { ref, onMounted, onUnmounted } from 'vue'

// Shared state - single instance for all components
const activeHint = ref(null)
const hintTimeout = ref(null)

/**
 * Composable for managing contextual hints/tooltips
 * Shows one-time hints to guide users through specific workflows
 */
export function useContextualHints() {
  /**
   * Check if a hint has been shown before
   * @param {string} hintKey - The unique key for the hint
   * @param {number} maxShows - Maximum times to show (default: 3)
   */
  function hasHintBeenShown(hintKey, maxShows = 3) {
    const count = parseInt(localStorage.getItem(`hint-${hintKey}-count`) || '0')
    return count >= maxShows
  }

  /**
   * Mark a hint as shown (increment counter)
   */
  function markHintAsShown(hintKey) {
    const count = parseInt(localStorage.getItem(`hint-${hintKey}-count`) || '0')
    localStorage.setItem(`hint-${hintKey}-count`, (count + 1).toString())
  }

  /**
   * Show a contextual hint tooltip
   * @param {Object} options - Hint configuration
   * @param {string} options.key - Unique key for this hint (for localStorage)
   * @param {string|Function} options.target - CSS selector or function returning element
   * @param {string} options.title - Hint title
   * @param {string} options.text - Hint description
   * @param {string} options.position - 'top'|'bottom'|'left'|'right'
   * @param {number} options.autoDismiss - Auto-dismiss after milliseconds (0 = no auto-dismiss)
   * @param {Function} options.onDismiss - Callback when hint is dismissed
   */
  function showHint(options) {
    const {
      key,
      target,
      title,
      text,
      position = 'right',
      autoDismiss = 5000,
      onDismiss
    } = options

    console.log('🔔 showHint called:', { key, title, hasTarget: !!target })

    // Check if hint was already shown
    if (hasHintBeenShown(key)) {
      console.log('🔔 Hint already shown, skipping')
      return
    }

    // Get target element
    const targetElement = typeof target === 'function' 
      ? target() 
      : document.querySelector(target)

    if (!targetElement) {
      console.warn(`Hint target not found: ${target}`)
      return
    }

    // Create hint data
    activeHint.value = {
      key,
      targetElement,
      title,
      text,
      position,
      onDismiss
    }

    // Mark as shown
    markHintAsShown(key)

    // Auto-dismiss
    if (autoDismiss > 0) {
      hintTimeout.value = setTimeout(() => {
        dismissHint()
      }, autoDismiss)
    }
  }

  /**
   * Dismiss the active hint
   */
  function dismissHint() {
    if (hintTimeout.value) {
      clearTimeout(hintTimeout.value)
      hintTimeout.value = null
    }

    if (activeHint.value?.onDismiss) {
      activeHint.value.onDismiss()
    }

    activeHint.value = null
  }

  /**
   * Reset a specific hint (for testing/debugging)
   */
  function resetHint(hintKey) {
    localStorage.removeItem(`hint-${hintKey}-count`)
  }

  /**
   * Reset all hints (for testing/debugging)
   */
  function resetAllHints() {
    const keys = Object.keys(localStorage)
    keys.forEach(key => {
      if (key.startsWith('hint-') && key.endsWith('-count')) {
        localStorage.removeItem(key)
      }
    })
  }

  // Cleanup on unmount
  onUnmounted(() => {
    if (hintTimeout.value) {
      clearTimeout(hintTimeout.value)
    }
  })

  return {
    activeHint,
    showHint,
    dismissHint,
    hasHintBeenShown,
    resetHint,
    resetAllHints
  }
}
