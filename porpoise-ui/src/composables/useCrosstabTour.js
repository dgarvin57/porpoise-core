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

export function useCrosstabTour() {
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
        scrollTo: false, // Disable auto-scrolling to prevent jumping
        cancelIcon: {
          enabled: true
        },
        modalOverlayOpeningPadding: 8,
        modalOverlayOpeningRadius: 8,
        canClickTarget: true, // Allow clicking on highlighted elements
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

    // Apply custom overlay styling after tour is created
    tour.on('show', () => {
      const overlay = document.querySelector('.shepherd-modal-overlay-container')
      if (overlay) {
        // Try multiple approaches to force overlay darkness
        overlay.setAttribute('fill', 'rgb(0, 0, 0)')
        overlay.setAttribute('fill-opacity', '0.8')
        overlay.setAttribute('opacity', '0.8')  
        overlay.style.setProperty('fill-opacity', '0.8', 'important')
        overlay.style.setProperty('opacity', '0.8', 'important')
        
        // Also try setting on the SVG's rect children
        const rects = overlay.querySelectorAll('rect')
        rects.forEach(rect => {
          rect.setAttribute('fill', 'rgb(0, 0, 0)')
          rect.setAttribute('fill-opacity', '0.8')
        })
      }
    })

    // Step 1: Highlight the toggle buttons and explain selection
    tour.addStep({
      id: 'toggle-buttons',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">
              <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-blue-600 text-white text-sm font-semibold mr-2">1</span>
              Select Your First Variable (DV)
            </h3>
          </div>
          <div class="shepherd-text">
            <p>Click a <strong>toggle button</strong> (○) to the LEFT of any question to select it as your <strong>Dependent Variable</strong>.</p>
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
                <span class="inline-flex items-center justify-center w-6 h-6 rounded bg-blue-600 text-white text-sm font-semibold">1</span>
              </div>
            </div>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">This is what you want to analyze (e.g., "Voter Choice", "Approval Rating")</p>
          </div>
        </div>
      `,
      attachTo: {
        element: () => {
          // Priority 1: Find currently selected DV (has blue badge #1)
          const selectedBadge = Array.from(document.querySelectorAll('span[class*="bg-blue-600"]'))
            .find(badge => badge.textContent.trim() === '1')
          if (selectedBadge) {
            const radio = selectedBadge.closest('[class*="flex items-center"]')?.querySelector('input[type="radio"]')
            if (radio) {
              // Check if hidden in collapsed block and expand
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
          
          // Priority 4: Fallback
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

    // Step 2: Explain clicking the question label for IV
    tour.addStep({
      id: 'question-label',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">
              <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-green-600 text-white text-sm font-semibold mr-2">2</span>
              Select Your Second Variable (IV)
            </h3>
          </div>
          <div class="shepherd-text">
            <p>Now click directly on the <strong>question name/label</strong> to select it as your <strong>Independent Variable</strong>.</p>
            <div class="mt-3 p-2.5 bg-green-50 dark:bg-green-900/30 rounded-lg border-l-4 border-green-600 border border-green-300 dark:border-green-700">
              <div class="flex items-center space-x-2">
                <input type="radio" class="w-5 h-5 opacity-40" disabled />
                <svg class="w-3.5 h-3.5 text-red-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                </svg>
                <span class="flex-1 text-sm font-medium text-gray-900 dark:text-white">Age</span>
                <span class="inline-flex items-center justify-center w-6 h-6 rounded bg-green-600 text-white text-sm font-semibold">2</span>
              </div>
            </div>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">This is how you want to group/compare data (e.g., "Age", "Gender", "Party Registration")</p>
          </div>
        </div>
      `,
      attachTo: {
        element: () => {
          // Priority 1: Find currently selected IV (has green badge #2)
          const selectedBadge = Array.from(document.querySelectorAll('span[class*="bg-green-600"]'))
            .find(badge => badge.textContent.trim() === '2')
          if (selectedBadge) {
            // Go up to the parent row, then find the icon+label container
            let parentRow = selectedBadge.parentElement
            // Keep going up until we find the row with all children
            while (parentRow && !parentRow.querySelector('input[type="radio"]')) {
              parentRow = parentRow.parentElement
            }
            const container = parentRow?.querySelector('div.flex.items-center.space-x-2.flex-1')
            if (container) {
              expandIfCollapsed(container)
              return container
            }
          }
          
          // Priority 2: Find first enabled IV question with red icon
          const allRedIcons = Array.from(document.querySelectorAll('svg.text-red-400'))
          
          for (const icon of allRedIcons) {
            const container = icon.closest('div.flex.items-center.space-x-2.flex-1')
            const radioButton = icon.closest('[class*="flex items-center"]')?.querySelector('input[type="radio"]')
            
            // Check if enabled (radio not disabled)
            if (!radioButton || !radioButton.disabled) {
              expandIfCollapsed(container)
              return container
            }
          }
          
          // Priority 3: Fallback to first red icon container
          const firstRedIcon = document.querySelector('svg.text-red-400')
          return firstRedIcon?.closest('div.flex.items-center.space-x-2.flex-1') || null
        },
        on: 'left'
      },
      buttons: [
        {
          text: 'Back',
          action: tour.back,
          secondary: true
        },
        {
          text: 'Next',
          action: tour.next
        }
      ]
    })

    // Step 3: Finish with automatic crosstab generation
    tour.addStep({
      id: 'auto-generate',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">That's It! ✨</h3>
          </div>
          <div class="shepherd-text">
            <p><strong>Your crosstab will generate automatically</strong> as soon as you select both variables!</p>
            <p class="mt-3"><strong>Quick Summary:</strong></p>
            <div class="mt-2 space-y-2 text-xs">
              <div class="flex items-start space-x-2">
                <span class="text-blue-600 font-bold">○</span>
                <span>Toggle button = Dependent Variable (What to measure)</span>
              </div>
              <div class="flex items-start space-x-2">
                <svg class="w-4 h-4 text-green-600 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                </svg>
                <span>Question name = Independent Variable (How to group)</span>
              </div>
            </div>
            <p class="mt-3 text-xs text-gray-600 dark:text-gray-400">
              💡 Tip: You can access this tour anytime from the Help menu
            </p>
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
            markTourCompleted('crosstab')
            tour.complete()
          }
        }
      ]
    })

    // Handle tour completion/cancellation
    tour.on('complete', () => {
      markTourCompleted('crosstab')
      // Clean up modal overlay
      const overlay = document.querySelector('.shepherd-modal-overlay-container')
      if (overlay) overlay.remove()
    })
    tour.on('cancel', () => {
      markTourCompleted('crosstab')
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
    hasTourBeenCompleted: () => hasTourBeenCompleted('crosstab'),
    markTourCompleted: () => markTourCompleted('crosstab'),
    resetTour: () => resetTour('crosstab'),
    startTour,
    createTour
  }
}
