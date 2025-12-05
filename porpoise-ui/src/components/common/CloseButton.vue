<template>
  <button 
    @click="$emit('click')"
    :class="sizeClasses"
    class="rounded bg-transparent text-gray-500 hover:text-gray-900 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-white dark:hover:bg-gray-700 transition-colors border-0"
    :title="title"
  >
    <svg :class="iconSizeClasses" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
    </svg>
  </button>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  size: {
    type: String,
    default: 'md',
    validator: (value) => ['sm', 'md', 'lg'].includes(value)
  },
  title: {
    type: String,
    default: 'Close'
  }
})

defineEmits(['click'])

const sizeClasses = computed(() => {
  const sizes = {
    sm: 'p-0.5',
    md: 'p-1',
    lg: 'p-1.5'
  }
  return sizes[props.size]
})

const iconSizeClasses = computed(() => {
  const sizes = {
    sm: 'w-3.5 h-3.5',
    md: 'w-5 h-5',
    lg: 'w-6 h-6'
  }
  return sizes[props.size]
})
</script>
