import { ref } from 'vue'
import Shepherd from 'shepherd.js'
import 'shepherd.js/dist/css/shepherd.css'
import { useTourManager } from './useTourManager'

// Helper function to check if element is in collapsed block and expand it
function expandIfCollapsed(element) {
  if (!element) return
  
  // Look for parent block that might be collapsed
  let current = element.parentElement
  while (current) {
    // Check if this element has a sibling that's a block header (clickable collapse toggle)
    const blockHeader = current.previousElementSibling
    if (blockHeader && blockHeader.tagName === 'BUTTON') {
      // Check if block content is hidden (collapsed)
      const blockContent = blockHeader.nextElementSibling
      if (blockContent && blockContent.offsetHeight === 0) {
        // Block is collapsed, click to expand
        blockHeader.click()
        return
      }
    }
    current = current.parentElement
  }
}

export function useStatSigTour() {
  const tourInstance = ref(null)
  const { hasTourBeenCompleted, markTourCompleted, resetTour } = useTourManager()

  function createTour() {
    if (tourInstance.value) {
      tourInstance.value.complete()
    }

    const tour = new Shepherd.Tour({
      useModalOverlay: true,
      modalContainer: document.body,
      defaultStepOptions: {
        classes: 'shepherd-theme-custom',
        scrollTo: false,
        cancelIcon: {
          enabled: true
        },
        modalOverlayOpeningPadding: 8,
        modalOverlayOpeningRadius: 8,
        canClickTarget: true,
        popperOptions: {
          modifiers: [
            {
              name: 'offset',
              options: {
                offset: [0, 12],
              },
            },
          ],
        },
      }
    })

    // Apply custom overlay styling
    tour.on('show', () => {
      const overlay = document.querySelector('.shepherd-modal-overlay-container')
      if (overlay) {
        overlay.setAttribute('fill', 'rgb(0, 0, 0)')
        overlay.setAttribute('fill-opacity', '0.8')
        overlay.setAttribute('opacity', '0.8')  
        overlay.style.setProperty('fill-opacity', '0.8', 'important')
        overlay.style.setProperty('opacity', '0.8', 'important')
        
        const rects = overlay.querySelectorAll('rect')
        rects.forEach(rect => {
          rect.setAttribute('fill', 'rgb(0, 0, 0)')
          rect.setAttribute('fill-opacity', '0.8')
        })
      }
    })

    // Step 1: Explain selecting a question for statistical significance
    tour.addStep({
      id: 'statsig-select',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">
              <span class="inline-flex items-center justify-center w-6 h-6 rounded bg-blue-600 text-white text-sm font-semibold mr-2">1</span>
              Select Your Question
            </h3>
          </div>
          <div class="shepherd-text">
            <p>Click the <strong>toggle button</strong> (○) next to any question to analyze its statistical significance.</p>
            <div class="mt-3 p-2.5 bg-blue-50 dark:bg-blue-900/30 rounded-lg border-l-4 border-blue-600 border border-blue-300 dark:border-blue-700">
              <div class="flex items-center space-x-2">
                <div class="relative w-5 h-5 flex-shrink-0">
                  <input type="radio" checked class="w-5 h-5 accent-blue-600 appearance-none rounded-full border-2 border-blue-600 bg-white" disabled />
                  <div class="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <div class="w-2.5 h-2.5 rounded-full bg-blue-600"></div>
                  </div>
                </div>
                <svg class="w-3.5 h-3.5 text-blue-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                </svg>
                <span class="flex-1 text-sm font-medium text-gray-900 dark:text-white">1st Ballot</span>
              </div>
            </div>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Statistical significance helps determine if differences between response groups are meaningful or just due to chance.</p>
          </div>
        </div>
      `,
      attachTo: {
        element: () => {
          // Priority 1: Find currently selected question (has blue badge #1)
          const selectedBadge = Array.from(document.querySelectorAll('span[class*="bg-blue-600"]'))
            .find(badge => badge.textContent.trim() === '1')
          if (selectedBadge) {
            const radio = selectedBadge.closest('[class*="flex items-center"]')?.querySelector('input[type="radio"]')
            if (radio) {
              expandIfCollapsed(radio)
              return radio
            }
          }
          
          // Priority 2: Find checked radio button
          let radio = document.querySelector('.group\\/radio input[type="radio"]:checked')
          if (radio && !radio.disabled) {
            expandIfCollapsed(radio)
            return radio
          }
          
          // Priority 3: Find first enabled radio button
          const allRadios = Array.from(document.querySelectorAll('.group\\/radio input[type="radio"]'))
          radio = allRadios.find(r => !r.disabled)
          
          if (radio) {
            expandIfCollapsed(radio)
            return radio
          }
          
          // Fallback
          return allRadios[0] || null
        },
        on: 'right'
      },
      buttons: [
        {
          text: 'Skip Tour',
          action: tour.complete,
          secondary: true
        },
        {
          text: 'Next',
          action: tour.next
        }
      ]
    })

    // Step 2: Explain the results table and clickable questions
    tour.addStep({
      id: 'statsig-results',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">
              <span class="inline-flex items-center justify-center w-6 h-6 rounded bg-green-600 text-white text-sm font-semibold mr-2">2</span>
              Understanding the Results
            </h3>
          </div>
          <div class="shepherd-text">
            <p>The table shows which response differences are <strong>statistically significant</strong> (unlikely to be random chance).</p>
            <p class="mt-3"><strong>Quick Tip:</strong> You can click on any question name in the <strong>3rd column</strong> to automatically generate a crosstab analysis!</p>
            <div class="mt-3 p-2.5 bg-blue-50 dark:bg-blue-900/30 rounded-lg border-l-4 border-blue-600 border border-blue-300 dark:border-blue-700">
              <div class="flex items-center space-x-2">
                <svg class="w-4 h-4 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
                </svg>
                <span class="text-sm text-gray-700 dark:text-gray-300">This will switch to the Crosstab tab for detailed comparison</span>
              </div>
            </div>
          </div>
        </div>
      `,
      buttons: [
        {
          text: 'Back',
          action: tour.back,
          secondary: true
        },
        {
          text: 'Got it!',
          action: () => {
            markTourCompleted('statsig')
            tour.complete()
          }
        }
      ]
    })

    // Handle tour completion/cancellation
    tour.on('complete', () => {
      markTourCompleted('statsig')
      // Clean up modal overlay
      const overlay = document.querySelector('.shepherd-modal-overlay-container')
      if (overlay) overlay.remove()
    })
    tour.on('cancel', () => {
      markTourCompleted('statsig')
      // Clean up modal overlay
      const overlay = document.querySelector('.shepherd-modal-overlay-container')
      if (overlay) overlay.remove()
    })

    tourInstance.value = tour
    return tour
  }

  function startTour() {
    const tour = createTour()
    tour.start()
  }

  return {
    hasTourBeenCompleted: () => hasTourBeenCompleted('statsig'),
    markTourCompleted: () => markTourCompleted('statsig'),
    resetTour: () => resetTour('statsig'),
    startTour,
    createTour
  }
}
