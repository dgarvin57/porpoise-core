<template>
  <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center pl-72">
    <!-- Backdrop -->
    <div 
      class="absolute inset-0 bg-black/30" 
      @click="$emit('dismiss', false)"
    ></div>
    
    <!-- Tooltip Content -->
    <div class="relative bg-white dark:bg-gray-800 rounded-lg shadow-2xl p-6 max-w-lg mx-4 border border-gray-200 dark:border-gray-700 animate-fade-in">
      <!-- Arrow pointing left (on left side of dialog) -->
      <div class="absolute left-[-8px] top-1/2 -translate-y-1/2 w-4 h-4 bg-white dark:bg-gray-800 border-l border-t border-gray-200 dark:border-gray-700 transform rotate-[-45deg]"></div>
      
      <!-- Close button -->
      <button
        @click="$emit('dismiss', false)"
        class="absolute top-4 right-4 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
      
      <!-- Content -->
      <div class="pr-6">
        <div class="flex items-start gap-3 mb-3">
          <!-- Step badge matching question list style -->
          <div 
            class="flex-shrink-0 inline-flex items-center justify-center w-6 h-6 rounded flex-shrink-0"
            :class="badgeColor === 'blue' ? 'bg-blue-600 text-white' : 'bg-green-600 text-white'"
          >
            <span class="text-sm font-semibold">
              {{ stepNumber }}
            </span>
          </div>
          <div class="flex-1">
            <h3 class="text-base font-semibold text-gray-900 dark:text-white mb-1.5">{{ title }}</h3>
            <p class="text-xs text-gray-600 dark:text-gray-300 mb-3 leading-relaxed">{{ message }}</p>
            
            <!-- Visual Guide -->
            <div v-if="showVisualGuide" class="bg-gray-50 dark:bg-gray-900 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
              <!-- DV Selection Guide (with toggle) - looks like actual hovered question row -->
              <div v-if="guideType === 'dv'" class="relative max-w-xs">
                <!-- Simple triangle pointer rotated 30° counterclockwise -->
                <div class="absolute top-5 left-3 z-20 pointer-events-none">
                  <svg width="14" height="18" viewBox="0 0 14 18" fill="none" xmlns="http://www.w3.org/2000/svg" class="drop-shadow-lg" style="transform: rotate(-30deg);">
                    <path d="M8 2L14 18H2L8 2Z" fill="white" stroke="black" stroke-width="1.2"/>
                  </svg>
                </div>
                
                <!-- Simulated question row with blue left border (hovered state) -->
                <div class="flex items-center px-2 py-1 rounded bg-gray-300/80 dark:bg-gray-600/50 border-l-2 border-l-blue-500 animate-pulse-soft">
                  <!-- Radio button (toggle) with ping animation -->
                  <div class="relative mr-2">
                    <input
                      type="radio"
                      class="w-3.5 h-3.5 text-blue-600 border-2 border-gray-600 dark:border-gray-400 focus:ring-0 cursor-pointer"
                      checked
                      disabled
                    />
                    <div class="absolute -top-0.5 -left-0.5 w-4.5 h-4.5 rounded-full border-2 border-blue-400 animate-ping-slow pointer-events-none"></div>
                  </div>
                  
                  <!-- Blue solid document icon (no lines, matching actual question list) -->
                  <svg class="w-3 h-3 text-blue-400 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                  </svg>
                  
                  <!-- Question text -->
                  <div class="flex-1 min-w-0">
                    <span class="text-[11px] text-gray-700 dark:text-gray-300">Question Label</span>
                  </div>
                </div>
                
                <div class="mt-1 text-[9px] font-medium text-gray-500 dark:text-gray-400 text-center">
                  Click button to the <span class="text-blue-500 dark:text-blue-400 font-semibold">LEFT</span> of question
                </div>
              </div>
              
              <!-- IV Selection Guide (question label only, with variable icon) -->
              <div v-else-if="guideType === 'iv'" class="relative max-w-xs">
                <!-- Simple triangle pointer rotated 30° counterclockwise -->
                <div class="absolute top-5 z-20 pointer-events-none" style="left: 70px;">
                  <svg width="14" height="18" viewBox="0 0 14 18" fill="none" xmlns="http://www.w3.org/2000/svg" class="drop-shadow-lg" style="transform: rotate(-30deg);">
                    <path d="M8 2L14 18H2L8 2Z" fill="white" stroke="black" stroke-width="1.2"/>
                  </svg>
                </div>
                
                <!-- Simulated question row (no toggle, just the question) -->
                <div class="relative">
                  <div class="flex items-center px-2 py-1 rounded bg-gray-300/80 dark:bg-gray-600/50 border-l-2 border-l-transparent animate-pulse-soft">
                    <!-- Radio button placeholder (hidden/not shown for IV) -->
                    <div class="w-3.5 h-3.5 mr-2 opacity-0"></div>
                    
                    <!-- Red document icon (matching DV icon but red color) -->
                    <svg class="w-3 h-3 text-red-400 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                      <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                    </svg>
                    
                    <!-- Question text -->
                    <div class="flex-1 min-w-0">
                      <span class="text-[11px] text-gray-700 dark:text-gray-300">Age</span>
                    </div>
                  </div>
                  
                  <!-- Green pulsing border to indicate selection target -->
                  <div class="absolute inset-0 rounded border-2 border-green-500 dark:border-green-400 pointer-events-none"></div>
                  <div class="absolute -inset-0.5 rounded border-2 border-green-300 dark:border-green-600 animate-ping-slow pointer-events-none"></div>
                </div>
                
                <div class="mt-1 text-[9px] font-medium text-gray-500 dark:text-gray-400 text-center">
                  Click question <span class="text-blue-500 dark:text-blue-400 font-semibold">label</span>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="flex items-center justify-between mt-3 pt-3 border-t border-gray-200 dark:border-gray-700">
          <label class="flex items-center cursor-pointer group">
            <div class="relative">
              <input
                type="checkbox"
                v-model="dontShowAgain"
                class="w-3.5 h-3.5 rounded border-2 border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-0 dark:bg-gray-700 dark:focus:ring-offset-gray-800 cursor-pointer transition-all"
              />
            </div>
            <span class="ml-2 text-xs text-gray-600 dark:text-gray-400 group-hover:text-gray-900 dark:group-hover:text-gray-200 transition-colors">Don't show this again</span>
          </label>
          
          <button
            @click="$emit('dismiss', dontShowAgain)"
            class="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white text-xs font-medium rounded transition-colors shadow-sm hover:shadow"
          >
            Got it!
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  show: {
    type: Boolean,
    required: true
  },
  title: {
    type: String,
    required: true
  },
  message: {
    type: String,
    required: true
  },
  position: {
    type: String,
    default: 'left', // 'top', 'bottom', 'left', 'right'
    validator: (value) => ['top', 'bottom', 'left', 'right'].includes(value)
  },
  targetSelector: {
    type: String,
    default: null
  },
  showVisualGuide: {
    type: Boolean,
    default: false
  },
  visualGuideLabel: {
    type: String,
    default: 'Click here'
  },
  stepNumber: {
    type: [Number, String],
    default: '1'
  },
  badgeColor: {
    type: String,
    default: 'blue', // 'blue' or 'green'
    validator: (value) => ['blue', 'green'].includes(value)
  },
  guideType: {
    type: String,
    default: 'dv', // 'dv' or 'iv'
    validator: (value) => ['dv', 'iv'].includes(value)
  }
})

defineEmits(['dismiss'])

const dontShowAgain = ref(false)

const arrowClasses = computed(() => {
  switch (props.position) {
    case 'top':
      return 'bottom-[-8px] left-1/2 -translate-x-1/2 border-b border-r'
    case 'bottom':
      return 'top-[-8px] left-1/2 -translate-x-1/2 border-t border-l'
    case 'left':
      return 'right-[-8px] top-1/2 -translate-y-1/2 border-t border-r'
    case 'right':
      return 'left-[-8px] top-1/2 -translate-y-1/2 border-b border-l'
    default:
      return 'right-[-8px] top-1/2 -translate-y-1/2 border-t border-r'
  }
})
</script>

<style scoped>
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes pulseSoft {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.6;
  }
}

@keyframes pingSlow {
  0% {
    transform: scale(1);
    opacity: 0.8;
  }
  50% {
    transform: scale(1.1);
    opacity: 0.4;
  }
  100% {
    transform: scale(1.2);
    opacity: 0;
  }
}

.animate-fade-in {
  animation: fadeIn 0.2s ease-out;
}

.animate-pulse-soft {
  animation: pulseSoft 2s ease-in-out infinite;
}

.animate-ping-slow {
  animation: pingSlow 2s cubic-bezier(0, 0, 0.2, 1) infinite;
}
</style>
