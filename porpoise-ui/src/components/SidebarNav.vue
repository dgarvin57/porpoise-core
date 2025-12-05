<template>
  <div :class="sidebarWidth">
    <nav class="space-y-1">
      <button
        v-for="item in items"
        :key="item.value"
        @click="$emit('update:modelValue', item.value)"
        :class="[
          'w-full text-left px-4 py-3 rounded-lg text-base font-medium transition-colors flex items-center gap-3',
          modelValue === item.value
            ? 'bg-blue-600 text-white'
            : 'text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 border border-gray-200 dark:border-gray-700'
        ]"
      >
        <svg v-if="item.icon" class="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="item.icon" />
        </svg>
        {{ item.label }}
      </button>
    </nav>
  </div>
</template>

<script setup>
defineProps({
  items: {
    type: Array,
    required: true,
    // Array of { label: string, value: string }
  },
  modelValue: {
    type: String,
    required: true
  },
  sidebarWidth: {
    type: String,
    default: 'w-56 flex-shrink-0'
  }
})

defineEmits(['update:modelValue'])
</script>
