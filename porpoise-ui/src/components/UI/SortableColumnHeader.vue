<template>
  <button
    @click="handleSort"
    :class="[
      'text-xs font-semibold uppercase flex items-center space-x-1 px-2 py-1 rounded border transition-colors',
      textAlign === 'right' ? 'justify-end' : textAlign === 'center' ? 'justify-center' : '',
      isActive
        ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800'
        : 'bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-600 text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-600 hover:text-gray-900 dark:hover:text-gray-200 hover:border-gray-300 dark:hover:border-gray-500'
    ]"
  >
    <span>{{ label }}</span>
    <svg
      v-if="isActive"
      class="w-3 h-3"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
    >
      <path
        v-if="direction === 'asc'"
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M5 15l7-7 7 7"
      />
      <path
        v-else
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M19 9l-7 7-7-7"
      />
    </svg>
  </button>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  label: {
    type: String,
    required: true
  },
  sortKey: {
    type: String,
    required: true
  },
  currentSort: {
    type: String,
    required: true
  },
  direction: {
    type: String,
    required: true,
    validator: (value) => ['asc', 'desc'].includes(value)
  },
  textAlign: {
    type: String,
    default: 'left',
    validator: (value) => ['left', 'center', 'right'].includes(value)
  }
})

const emit = defineEmits(['sort'])

const isActive = computed(() => props.currentSort === props.sortKey)

const handleSort = () => {
  emit('sort', props.sortKey)
}
</script>
