<template>
  <Teleport to="body">
    <Transition name="hint-fade">
      <div
        v-if="activeHint"
        class="contextual-hint-overlay"
        @click="dismissHint"
      >
        <div
          class="contextual-hint"
          :class="`hint-${activeHint.position}`"
          :style="hintStyle"
          @click.stop
        >
          <!-- Arrow -->
          <div class="hint-arrow" :class="`arrow-${activeHint.position}`"></div>
          
          <!-- Content -->
          <div class="hint-content">
            <div class="hint-header">
              <h4 class="hint-title">{{ activeHint.title }}</h4>
              <button
                class="hint-close"
                @click="dismissHint"
                aria-label="Close hint"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <p class="hint-text">{{ activeHint.text }}</p>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed, watch, nextTick } from 'vue'
import { useContextualHints } from '../composables/useContextualHints'

const { activeHint, dismissHint } = useContextualHints()

const hintStyle = computed(() => {
  if (!activeHint.value?.targetElement) return {}

  const rect = activeHint.value.targetElement.getBoundingClientRect()
  const position = activeHint.value.position
  const offset = 20 // Distance from target

  let style = {}

  switch (position) {
    case 'right':
      style = {
        left: `${rect.right + offset}px`,
        top: `${rect.top + rect.height / 2}px`,
        transform: 'translateY(-50%)'
      }
      break
    case 'left':
      style = {
        right: `${window.innerWidth - rect.left + offset}px`,
        top: `${rect.top + rect.height / 2}px`,
        transform: 'translateY(-50%)'
      }
      break
    case 'top':
      style = {
        left: `${rect.left + rect.width / 2}px`,
        bottom: `${window.innerHeight - rect.top + offset}px`,
        transform: 'translateX(-50%)'
      }
      break
    case 'bottom':
      style = {
        left: `${rect.left + rect.width / 2}px`,
        top: `${rect.bottom + offset}px`,
        transform: 'translateX(-50%)'
      }
      break
  }

  return style
})

// Reposition on window resize
let resizeTimeout
watch(activeHint, (newHint) => {
  if (newHint) {
    window.addEventListener('resize', handleResize)
  } else {
    window.removeEventListener('resize', handleResize)
  }
})

function handleResize() {
  clearTimeout(resizeTimeout)
  resizeTimeout = setTimeout(() => {
    // Force recompute by triggering reactivity
    if (activeHint.value) {
      const temp = activeHint.value
      activeHint.value = null
      nextTick(() => {
        activeHint.value = temp
      })
    }
  }, 100)
}
</script>

<style scoped>
.contextual-hint-overlay {
  position: fixed;
  inset: 0;
  z-index: 9998;
  pointer-events: none;
}

.contextual-hint {
  position: fixed;
  max-width: 320px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15), 0 0 0 1px rgba(0, 0, 0, 0.05);
  pointer-events: auto;
  z-index: 9999;
}

.dark .contextual-hint {
  background: #1f2937;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3), 0 0 0 1px rgba(255, 255, 255, 0.1);
}

.hint-arrow {
  position: absolute;
  width: 0;
  height: 0;
  border-style: solid;
}

.arrow-right {
  left: -10px;
  top: 50%;
  transform: translateY(-50%);
  border-width: 10px 10px 10px 0;
  border-color: transparent white transparent transparent;
}

.dark .arrow-right {
  border-color: transparent #1f2937 transparent transparent;
}

.arrow-left {
  right: -10px;
  top: 50%;
  transform: translateY(-50%);
  border-width: 10px 0 10px 10px;
  border-color: transparent transparent transparent white;
}

.dark .arrow-left {
  border-color: transparent transparent transparent #1f2937;
}

.arrow-top {
  bottom: -10px;
  left: 50%;
  transform: translateX(-50%);
  border-width: 10px 10px 0 10px;
  border-color: white transparent transparent transparent;
}

.dark .arrow-top {
  border-color: #1f2937 transparent transparent transparent;
}

.arrow-bottom {
  top: -10px;
  left: 50%;
  transform: translateX(-50%);
  border-width: 0 10px 10px 10px;
  border-color: transparent transparent white transparent;
}

.dark .arrow-bottom {
  border-color: transparent transparent #1f2937 transparent;
}

.hint-content {
  padding: 16px;
}

.hint-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.hint-title {
  font-size: 0.9375rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.dark .hint-title {
  color: #f9fafb;
}

.hint-close {
  flex-shrink: 0;
  padding: 4px;
  margin: -4px -4px -4px 8px;
  color: #6b7280;
  background: none;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s;
}

.hint-close:hover {
  color: #111827;
  background: #f3f4f6;
}

.dark .hint-close {
  color: #9ca3af;
}

.dark .hint-close:hover {
  color: #f9fafb;
  background: #374151;
}

.hint-text {
  font-size: 0.875rem;
  line-height: 1.5;
  color: #4b5563;
  margin: 0;
}

.dark .hint-text {
  color: #d1d5db;
}

/* Fade transition - gentle scale draws attention */
.hint-fade-enter-active {
  transition: opacity 0.4s ease-out, transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.hint-fade-leave-active {
  transition: opacity 0.25s ease-in, transform 0.25s ease-in;
}

.hint-fade-enter-from {
  opacity: 0;
  transform: scale(0.9);
}

.hint-fade-leave-to {
  opacity: 0;
  transform: scale(0.98);
}
</style>
