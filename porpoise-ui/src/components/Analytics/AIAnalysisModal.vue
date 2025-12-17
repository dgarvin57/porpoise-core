<template>
  <div v-if="show" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="$emit('close')">
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
      <!-- Header -->
      <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
        <div class="relative flex items-center justify-between">
          <div class="flex items-center gap-2">
            <svg class="w-5 h-5 text-yellow-600 fill-yellow-600 dark:text-yellow-400 dark:fill-transparent" fill="currentColor" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
            </svg>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">{{ context || 'AI Analysis' }}</h3>
          </div>
          <div class="absolute left-1/2 transform -translate-x-1/2">
            <span class="text-base font-semibold text-blue-600 dark:text-blue-400 whitespace-nowrap">
              {{ questionLabel }}
            </span>
          </div>
          <button @click="$emit('close')" class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="px-6 py-4">
        <!-- Loading State -->
        <div v-if="loading" class="flex flex-col items-center justify-center py-8">
          <svg class="animate-spin h-10 w-10 text-blue-600 dark:text-blue-400 mb-4" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <p class="text-sm text-gray-600 dark:text-gray-400">Generating AI analysis...</p>
        </div>

        <!-- Analysis Content -->
        <div v-else-if="analysis" class="space-y-4">
          <div v-for="(section, idx) in parsedAnalysis" :key="idx">
            <h4 v-if="section.heading" class="font-semibold text-gray-900 dark:text-white mb-2 flex items-center gap-2">
              <span class="w-1 h-5 bg-blue-500 rounded"></span>
              {{ section.heading }}
            </h4>
            <p class="text-sm text-gray-700 dark:text-gray-300 leading-relaxed" :class="section.heading ? 'ml-3' : ''">
              {{ section.content }}
            </p>
          </div>

          <!-- Regenerate Option -->
          <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
            <button 
              @click="$emit('regenerate')"
              :disabled="loading"
              class="text-sm text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium"
            >
              <svg class="w-4 h-4 inline-block mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Regenerate Analysis
            </button>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="text-center py-12">
          <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-blue-100 dark:bg-blue-900/30 mb-4">
            <svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
          </div>
          <h4 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">Generate AI Analysis</h4>
          <p class="text-sm text-gray-600 dark:text-gray-400 max-w-md mx-auto mb-6">
            Our AI will analyze your question results and provide insights including an overview of the distribution, key findings, patterns, and actionable recommendations.
          </p>
          <button
            @click="$emit('generate')"
            :disabled="loading"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors inline-flex items-center gap-2"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            Generate Analysis
          </button>
        </div>
      </div>

      <!-- Footer -->
      <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
        <button 
          @click="$emit('close')"
          class="px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-900 dark:text-white text-sm font-medium rounded-lg transition-colors"
        >
          Close
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  show: Boolean,
  questionLabel: String,
  analysis: String,
  loading: Boolean,
  context: {
    type: String,
    default: ''
  }
})

defineEmits(['close', 'generate', 'regenerate'])

const parsedAnalysis = computed(() => {
  if (!props.analysis) return []
  
  const sections = []
  const lines = props.analysis.split('\n')
  let currentSection = null
  
  for (const line of lines) {
    const trimmedLine = line.trim()
    
    if (trimmedLine.startsWith('## ')) {
      if (currentSection) {
        sections.push(currentSection)
      }
      currentSection = {
        heading: trimmedLine.substring(3).trim(),
        content: ''
      }
    } else if (trimmedLine && currentSection) {
      if (currentSection.content) {
        currentSection.content += ' '
      }
      currentSection.content += trimmedLine
    } else if (trimmedLine && !currentSection) {
      sections.push({
        heading: null,
        content: trimmedLine
      })
    }
  }
  
  if (currentSection) {
    sections.push(currentSection)
  }
  
  return sections
})
</script>
