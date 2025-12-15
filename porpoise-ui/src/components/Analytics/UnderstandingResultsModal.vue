<template>
  <div v-if="show" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="$emit('close')">
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
      <!-- Header -->
      <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Understanding Results</h3>
          </div>
          <button @click="$emit('close')" class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="px-6 py-4 space-y-6">
        <!-- Quick Tip -->
        <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4 border border-blue-200 dark:border-blue-800">
          <h4 class="font-semibold text-blue-900 dark:text-blue-300 mb-2 flex items-center gap-2">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Quick Tip
          </h4>
          <p class="text-sm text-blue-900 dark:text-blue-300 leading-relaxed">
            Click on any question in the sidebar to view its complete response distribution, statistics, and chart. Use the metrics below the chart to quickly understand response patterns.
          </p>
        </div>

        <!-- What is the Results View -->
        <div>
          <h4 class="font-semibold text-gray-900 dark:text-white mb-2">What is the Results View?</h4>
          <p class="text-sm text-gray-700 dark:text-gray-300 leading-relaxed">
            The Results View displays the complete distribution of responses for a single survey question. It shows how many respondents selected each answer choice, along with statistical measures that summarize the overall response pattern.
          </p>
        </div>

        <!-- Key Metrics -->
        <div>
          <h4 class="font-semibold text-gray-900 dark:text-white mb-3">Key Metrics</h4>
          
          <!-- Total N -->
          <div class="mb-4">
            <div class="flex items-center gap-2 mb-1.5">
              <div class="w-2 h-2 bg-purple-500 rounded-full"></div>
              <h5 class="font-semibold text-gray-900 dark:text-white text-sm">Total N (Total Responses): <span class="text-purple-600 dark:text-purple-400">{{ totalN }}</span></h5>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
              The number of valid responses for this question. 
              <span class="text-gray-500 dark:text-gray-400 italic">This question: {{ totalN }} responses.</span> 
              Larger samples provide more reliable results and lower margins of error.
            </p>
          </div>

          <!-- Top Response -->
          <div class="mb-4">
            <div class="flex items-center gap-2 mb-1.5">
              <div class="w-2 h-2 bg-orange-500 rounded-full"></div>
              <h5 class="font-semibold text-gray-900 dark:text-white text-sm">Top Response: <span class="text-orange-600 dark:text-orange-400">{{ topResponsePct }}%</span></h5>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
              The percentage for the most popular answer choice. 
              <span class="text-gray-500 dark:text-gray-400 italic">This question: {{ topResponsePct }}%.</span> 
              Helps you quickly identify the dominant response.
            </p>
          </div>

          <!-- Answer Options -->
          <div class="mb-4">
            <div class="flex items-center gap-2 mb-1.5">
              <div class="w-2 h-2 bg-pink-500 rounded-full"></div>
              <h5 class="font-semibold text-gray-900 dark:text-white text-sm">Answer Options: <span class="text-pink-600 dark:text-pink-400">{{ answerOptions }}</span></h5>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
              The number of different answer choices available for this question. 
              <span class="text-gray-500 dark:text-gray-400 italic">This question: {{ answerOptions }} options.</span>
            </p>
          </div>

          <!-- Index -->
          <div class="mb-4">
            <div class="flex items-center gap-2 mb-1.5">
              <div class="w-2 h-2 bg-blue-500 rounded-full"></div>
              <h5 class="font-semibold text-gray-900 dark:text-white text-sm">Index: <span class="text-blue-600 dark:text-blue-400">{{ index }}</span></h5>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
              Our proprietary sentiment metric that instantly reveals response favorability. 
              Calculated as <span class="font-mono bg-gray-100 dark:bg-gray-700 px-1.5 py-0.5 rounded text-xs">(Positive% - Negative% + 100)</span>. 
              Centered at 100—higher values show positive sentiment, lower show negative. 
              <span class="text-gray-500 dark:text-gray-400 italic">For this question: {{ index }} {{ index > 100 ? `= ${index - 100}% net positive` : index < 100 ? `= ${100 - index}% net negative` : '= neutral' }}</span>
            </p>
          </div>

          <!-- CI -->
          <div class="mb-4">
            <div class="flex items-center gap-2 mb-1.5">
              <div class="w-2 h-2 bg-green-500 rounded-full"></div>
              <h5 class="font-semibold text-gray-900 dark:text-white text-sm">CI (Confidence Interval): <span class="text-green-600 dark:text-green-400">±{{ ci }}</span></h5>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
              The margin of error for the entire question at 95% confidence. 
              Shows the range where the true population value likely falls. 
              <span class="text-gray-500 dark:text-gray-400 italic">For this question: ±{{ ci }} means results are accurate within {{ ci }} points</span>
            </p>
          </div>
        </div>

        <!-- Understanding the Chart -->
        <div>
          <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Understanding the Chart</h4>
          <ul class="text-sm text-gray-700 dark:text-gray-300 space-y-2 ml-4">
            <li>• <strong>Frequency Distribution:</strong> Shows the count and percentage for each response option</li>
            <li>• <strong>Visual Comparison:</strong> Bar lengths make it easy to compare popularity of different responses</li>
            <li>• <strong>Percentages:</strong> Always add up to 100% across all answer choices</li>
          </ul>
        </div>

        <!-- Tips -->
        <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4 border border-blue-200 dark:border-blue-800">
          <h4 class="font-semibold text-blue-900 dark:text-blue-300 mb-2 flex items-center gap-2">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
            </svg>
            Pro Tips
          </h4>
          <ul class="text-sm text-blue-900 dark:text-blue-300 space-y-2 ml-3">
            <li>• Use the "Analyze in Crosstab" button to see how responses differ across demographic groups</li>
            <li>• Look for unexpected patterns or outliers that might warrant further investigation</li>
            <li>• Compare results to previous surveys or benchmarks to track changes over time</li>
            <li>• Consider the margin of error when interpreting close percentages</li>
          </ul>
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
  question: Object
})

defineEmits(['close'])

// Extract actual values from question
const totalN = computed(() => props.question?.responses?.reduce((sum, r) => sum + r.n, 0) || 497)
const topResponsePct = computed(() => {
  if (!props.question?.responses?.length) return 42.7
  const sorted = [...props.question.responses].sort((a, b) => b.percent - a.percent)
  return sorted[0]?.percent || 42.7
})
const answerOptions = computed(() => props.question?.responses?.length || 3)
const index = computed(() => props.question?.index || props.question?.idx || 85)
const ci = computed(() => props.question?.samplingError || props.question?.confidenceInterval || props.question?.ci || 4.4)
</script>
