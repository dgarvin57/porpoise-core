<template>
  <div class="flex items-center">
    <button 
      @click="$emit('ai-analysis')"
      class="ai-button ai-button-emphasized"
    >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
        </svg>
        AI Analysis
      </button>
  </div>
</template>

<script setup>
defineEmits(['ai-analysis'])
</script>

<style scoped>
.ai-button {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0 0.375rem 0.75rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: rgb(229 231 235);
  background-color: transparent;
  border: none;
  border-radius: 0.5rem;
  transition: background-color 0.15s ease-in-out;
  cursor: pointer;
}

.ai-button-emphasized {
  /* Emphasis styles if needed in future */
}

.ai-button:hover {
  background-color: rgba(55, 65, 81, 0.5);
}

.ai-button:focus {
  outline: none;
}

</style>
