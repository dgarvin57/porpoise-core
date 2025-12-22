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

export function useResultsTour() {
  const tourInstance = ref(null)
  const { hasTourBeenCompleted, markTourCompleted, resetTour, skipAllTours } = useTourManager()

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
          enabled: true,
          label: 'Skip tour'
        },
        when: {
          cancel: () => {
            skipAllTours()  // Set skip-all flag when X button clicked
          }
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

    // Step 1: Welcome and explain the Results tab
    tour.addStep({
      id: 'results-welcome',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">Welcome to Survey Analysis! 👋</h3>
          </div>
          <div class="shepherd-text">
            <p>Let's take a quick tour of how to analyze your survey data.</p>
            <p class="mt-3">The <strong>Results tab</strong> shows individual question responses with counts and percentages. This is perfect for viewing one question at a time.</p>
            <p class="mt-3">Click <strong>Next</strong> to learn how to select a question.</p>
          </div>
        </div>
      `,
      buttons: [
        {
          text: 'Skip Tour',
          action: () => {
            skipAllTours()
            tour.complete()
          },
          secondary: true
        },
        {
          text: 'Next',
          action: tour.next
        }
      ]
    })

    // Step 2: Explain selecting a question
    tour.addStep({
      id: 'results-select-question',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">
              <span class="inline-flex items-center justify-center w-6 h-6 rounded bg-blue-600 text-white text-sm font-semibold mr-2">1</span>
              Select a Question
            </h3>
          </div>
          <div class="shepherd-text">
            <p>Click a <strong>toggle button</strong> (○) or <strong>question name</strong> to view its detailed results.</p>
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
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">The selected question's responses will appear in the main panel with charts and statistics.</p>
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

    // Step 3: Explain crosstab generation
    tour.addStep({
      id: 'results-crosstab-hint',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">
              <span class="inline-flex items-center justify-center w-6 h-6 rounded bg-green-600 text-white text-sm font-semibold mr-2">2</span>
              Compare Questions
            </h3>
          </div>
          <div class="shepherd-text">
            <p>Want to compare two questions? Click the <strong>Analyze in Crosstab</strong> button above the chart.</p>
            <div class="mt-3 p-3 bg-gradient-to-r from-blue-50 to-blue-100 dark:from-blue-900/30 dark:to-blue-800/30 rounded-lg border border-blue-200 dark:border-blue-700">
              <button class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-md shadow-sm transition-colors">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
                Analyze in Crosstab
              </button>
            </div>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">This will take you to the Crosstab tab where you can select a second question to compare.</p>
          </div>
        </div>
      `,
      attachTo: {
        element: () => {
          // Find the "Analyze in Crosstab" button
          const buttons = Array.from(document.querySelectorAll('button'))
          const crosstabButton = buttons.find(btn => 
            btn.textContent.includes('Analyze in Crosstab') || 
            btn.textContent.includes('Crosstab')
          )
          return crosstabButton || null
        },
        on: 'bottom'
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

    // Step 4: Explore other tabs
    tour.addStep({
      id: 'results-explore',
      text: `
        <div class="shepherd-content-wrapper">
          <div class="shepherd-header">
            <h3 class="shepherd-title">Explore More Analysis Types ✨</h3>
          </div>
          <div class="shepherd-text">
            <p><strong>You're all set!</strong> Here's what else you can do:</p>
            <div class="mt-3 space-y-2 text-sm">
              <div class="flex items-start space-x-2">
                <svg class="w-4 h-4 text-blue-600 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clip-rule="evenodd" />
                </svg>
                <span><strong>Crosstab</strong> - Compare two questions side-by-side</span>
              </div>
              <div class="flex items-start space-x-2">
                <svg class="w-4 h-4 text-green-600 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd" />
                </svg>
                <span><strong>Statistical Significance</strong> - Find meaningful differences</span>
              </div>
              <div class="flex items-start space-x-2">
                <svg class="w-4 h-4 text-purple-600 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
                </svg>
                <span>Use the sidebar to explore Questions, Data View, and more</span>
              </div>
            </div>
            <p class="mt-3 text-xs text-gray-600 dark:text-gray-400">
              💡 Tip: Each tab has its own guided tour to help you get started
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
            markTourCompleted('results')
            tour.complete()
          }
        }
      ]
    })

    // Handle tour completion/cancellation
    tour.on('complete', () => {
      markTourCompleted('results')
      // Clean up modal overlay
      const overlay = document.querySelector('.shepherd-modal-overlay-container')
      if (overlay) overlay.remove()
    })
    tour.on('cancel', () => {
      skipAllTours()  // Mark all analytics tours as skipped
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
    hasTourBeenCompleted: () => hasTourBeenCompleted('results'),
    markTourCompleted: () => markTourCompleted('results'),
    resetTour: () => resetTour('results'),
    startTour,
    createTour
  }
}
