import { ref, onMounted, nextTick } from 'vue'

const HINT_KEY = 'hint-results-label-click-shown'
const MAX_SHOW_COUNT = 1 // Only show once

// Module-level reactive ref for active hint (shared across all instances)
const activeHint = ref(null)

export function useResultsLabelHint() {
  const hintElement = ref(null)
  const showHint = ref(false)
  const isAnimating = ref(false)

  function shouldShowHint() {
    try {
      const shownCount = parseInt(localStorage.getItem(HINT_KEY) || '0', 10)
      return shownCount < MAX_SHOW_COUNT
    } catch (e) {
      return false
    }
  }

  function incrementHintCount() {
    try {
      const currentCount = parseInt(localStorage.getItem(HINT_KEY) || '0', 10)
      localStorage.setItem(HINT_KEY, (currentCount + 1).toString())
    } catch (e) {
      console.error('Failed to save hint count:', e)
    }
  }

  async function showHintIfNeeded(targetElementSelector) {
    // If any hint is already active, don't show another one
    if (activeHint.value) {
      return
    }

    if (!shouldShowHint()) {
      return
    }

    await nextTick()
    
    const targetElement = document.querySelector(targetElementSelector)
    if (!targetElement) {
      return
    }

    // Mark this hint as active
    activeHint.value = 'results-label'
    showHint.value = true
    isAnimating.value = true
    
    // Position hint near target element
    const rect = targetElement.getBoundingClientRect()
    if (hintElement.value) {
      // Position to the right of the target element
      hintElement.value.style.position = 'fixed'
      hintElement.value.style.top = `${rect.top}px`
      hintElement.value.style.left = `${rect.right + 12}px`
      hintElement.value.style.zIndex = '9999'
    }

    // Animation complete after delay
    setTimeout(() => {
      isAnimating.value = false
    }, 600)
  }

  function dismissHint() {
    showHint.value = false
    activeHint.value = null
    incrementHintCount()
  }

  return {
    hintElement,
    showHint,
    isAnimating,
    showHintIfNeeded,
    dismissHint
  }
}
