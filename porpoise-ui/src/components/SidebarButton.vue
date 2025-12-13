<template>
  <button
    @click="$emit('click')"
    :class="[
      'w-full flex items-center px-3 py-2.5 text-sm font-medium transition-all duration-200 rounded-lg group relative',
      collapsed ? 'justify-center' : 'space-x-3',
      isActive
        ? 'text-blue-600 dark:text-blue-400 bg-blue-50/50 dark:bg-blue-900/20'
        : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
    ]"
  >
    <!-- Icon -->
    <component 
      :is="'svg'" 
      :class="[
        'flex-shrink-0 transition-all duration-200',
        collapsed ? 'w-5 h-5 group-hover:scale-125' : 'w-5 h-5'
      ]" 
      fill="none" 
      stroke="currentColor" 
      viewBox="0 0 24 24"
    >
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="iconPath" />
    </component>

    <!-- Label (visible when expanded) -->
    <span v-if="!collapsed" class="flex-1 text-left">{{ label }}</span>

    <!-- Badge (visible when expanded) -->
    <span v-if="!collapsed && badge" class="text-xs italic text-yellow-500 dark:text-yellow-400 opacity-70">
      {{ badge }}
    </span>

    <!-- Hover Tooltip (visible when collapsed) -->
    <div 
      v-if="collapsed" 
      class="absolute left-full ml-4 px-3 py-2 bg-gray-900 dark:bg-gray-700 text-white text-sm rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 whitespace-nowrap pointer-events-none z-50"
    >
      {{ label }}
      <span v-if="badge" class="ml-2 text-xs italic text-yellow-300 opacity-80">({{ badge }})</span>
    </div>

    <!-- Active Indicator -->
    <span 
      v-if="isActive && !collapsed" 
      class="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-8 bg-blue-600 dark:bg-blue-400 rounded-r"
    ></span>
  </button>
</template>

<script setup>
defineProps({
  label: {
    type: String,
    required: true
  },
  iconPath: {
    type: String,
    required: true
  },
  isActive: {
    type: Boolean,
    default: false
  },
  collapsed: {
    type: Boolean,
    default: false
  },
  badge: {
    type: String,
    default: null
  }
})

defineEmits(['click'])
</script>
