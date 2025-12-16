<template>
  <div class="h-full flex flex-col bg-gray-50 dark:bg-gray-900">
      <!-- Loading State - Skeleton -->
      <div 
        v-if="loading"
        class="h-full overflow-auto"
      >
        <div class="pt-3 px-6 pb-6 flex justify-center">
          <div class="w-full max-w-[848px]">
            <!-- Header Skeleton -->
            <div class="flex items-end justify-between mb-2 pb-2">
              <div class="space-y-2">
                <div class="h-5 bg-gray-200 dark:bg-gray-700 rounded w-64 animate-pulse"></div>
                <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-24 animate-pulse"></div>
              </div>
              <div class="flex gap-3">
                <div class="h-8 w-8 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                <div class="h-8 w-32 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
              </div>
            </div>

            <!-- Table Skeleton -->
            <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm">
              <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                  <thead class="bg-blue-50 dark:bg-gray-700">
                    <tr>
                      <th class="px-6 py-1.5 text-left">
                        <div class="h-3 bg-gray-300 dark:bg-gray-600 rounded w-24 animate-pulse"></div>
                      </th>
                      <th class="px-6 py-1.5 text-center" v-for="i in 4" :key="i">
                        <div class="h-3 bg-gray-300 dark:bg-gray-600 rounded w-16 mx-auto animate-pulse"></div>
                      </th>
                    </tr>
                  </thead>
                  <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                    <tr v-for="i in 5" :key="i" :class="i % 2 === 0 ? 'bg-white dark:bg-gray-800' : 'bg-gray-50 dark:bg-gray-700/30'">
                      <td class="px-6 py-1">
                        <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-32 animate-pulse"></div>
                      </td>
                      <td class="px-6 py-1 text-center" v-for="j in 4" :key="j">
                        <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-12 mx-auto animate-pulse"></div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div 
        v-else-if="!crosstabData && !props.firstQuestion"
        class="flex items-center justify-center h-full"
      >
        <div class="text-center p-6">
          <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
          <h3 class="mt-4 text-base font-medium text-gray-900 dark:text-white">Crosstab Analysis</h3>
          <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
            Select two questions from the list to create a crosstab analysis
          </p>
          <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">
            Tip: Click to select the first question (dependent variable), then click another to select the second question (independent variable)
          </p>
        </div>
      </div>

      <!-- Partial State - Only DV Selected -->
      <div 
        v-else-if="!crosstabData && props.firstQuestion && !props.secondQuestion"
        class="h-full overflow-auto"
      >
        <div class="pt-3 px-6 pb-6 flex justify-center">
          <div class="w-full max-w-[848px]">
            <!-- Header -->
            <div class="flex items-end justify-between mb-2 pb-2">
              <div>
                <h3 class="text-base font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                  <span>{{ props.firstQuestion.label }}</span>
                  <span class="inline-flex items-center justify-center w-4 h-4 rounded bg-blue-600 text-white text-xs font-semibold flex-shrink-0">1</span>
                  <span class="text-gray-500 dark:text-gray-400 font-normal">by</span>
                </h3>
                <div class="text-[10px] font-medium text-blue-600 dark:text-blue-400 uppercase tracking-wide">
                  CROSSTAB
                </div>
              </div>
            </div>
            
            <p class="mb-4 text-sm text-gray-600 dark:text-gray-400">
              Click a question label to select an independent variable
            </p>
            
            <!-- Partial Table - DV rows only -->
            <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm">
              <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                  <thead class="bg-blue-50 dark:bg-gray-700">
                    <tr>
                      <th class="px-6 py-1.5 text-left text-xs font-semibold text-gray-800 dark:text-gray-400 uppercase tracking-wider">
                        {{ props.firstQuestion.label }}
                      </th>
                      <th class="px-6 py-1.5 text-center text-xs font-semibold text-gray-500 dark:text-gray-500 uppercase tracking-wider italic">
                        Select IV...
                      </th>
                    </tr>
                  </thead>
                  <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                    <tr
                      v-for="(response, idx) in dvResponses"
                      :key="idx"
                      :class="[
                        idx % 2 === 0 ? 'bg-white dark:bg-gray-800' : 'bg-gray-50 dark:bg-gray-700/30'
                      ]"
                    >
                      <td class="px-6 py-1 text-xs font-medium text-gray-900 dark:text-white whitespace-nowrap">
                        {{ response }}
                      </td>
                      <td class="px-6 py-1 text-xs text-gray-400 dark:text-gray-600 text-center">
                        —
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="flex items-center justify-center h-full">
        <div class="text-center p-6">
          <svg class="mx-auto h-12 w-12 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Error</h3>
          <p class="mt-1 text-sm text-red-600 dark:text-red-400">{{ error }}</p>
          <Button
            @click="generateCrosstab"
            class="mt-4"
            size="sm"
          >
            Try Again
          </Button>
        </div>
      </div>

      <!-- Crosstab Results -->
      <div v-else-if="crosstabData" class="h-full overflow-auto">
        <div class="pt-3 px-6 pb-6 flex justify-center">
          <div class="w-full max-w-[833px] mt-[10px]">
            <!-- Header with question label and buttons -->
            <div class="flex items-end justify-between mb-2 pb-2">
              <div>
                <h3 class="text-base font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                  <span>{{ crosstabData.firstQuestion.label }}</span>
                  <span class="inline-flex items-center justify-center w-4 h-4 rounded bg-blue-600 text-white text-xs font-semibold flex-shrink-0">1</span>
                  <span class="text-gray-500 dark:text-gray-400 font-normal">by</span>
                  <span>{{ crosstabData.secondQuestion.label }}</span>
                  <span class="inline-flex items-center justify-center w-4 h-4 rounded bg-green-600 text-white text-xs font-semibold flex-shrink-0">2</span>
                </h3>
                <div class="text-[10px] font-medium text-blue-600 dark:text-blue-400 uppercase tracking-wide">
                  CROSSTAB
                </div>
              </div>
              <div class="flex gap-0">
                <!-- Total N Display -->
                <div class="text-sm text-gray-600 dark:text-gray-400 mt-[5px]">
                  <span class="font-medium">Total N:</span> {{ crosstabData.totalN }}
                </div>
                <!-- Info Button -->
                <button 
                  @click="showExplanation = true"
                  class="inline-flex gap-2 pl-3 py-1 px-3 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700"
                  title="Understanding crosstabs and statistical measures"
                >
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
                  </svg>
                </button>
                <!-- AI Analysis Button -->
                <button 
                  @click="showAIModal = true"
                  class="inline-flex justify-center gap-2 px-3 py-1 text-gray-700 dark:text-gray-200 bg-transparent hover:bg-gray-200/50 dark:hover:bg-gray-700/50 rounded-lg transition-colors"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                  </svg>
                  AI Analysis
                </button>
              </div>
            </div>

        <!-- Table Card -->
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-blue-50 dark:bg-gray-700">
                <tr>
                  <th 
                    v-for="(col, idx) in tableColumns"
                    :key="idx"
                    class="px-6 py-1.5 text-left text-xs font-semibold text-gray-800 dark:text-gray-400 uppercase tracking-wider"
                  >
                    {{ col.trim() === '' ? '' : col }}
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                <tr
                  v-for="(row, rowIdx) in crosstabData.table"
                  :key="rowIdx"
                  :class="[
                    'hover:bg-blue-50 dark:hover:bg-gray-700',
                    rowIdx % 2 === 0 ? 'bg-white dark:bg-gray-800' : 'bg-gray-50 dark:bg-gray-700/30'
                  ]"
                >
                  <td
                    v-for="(col, colIdx) in tableColumns"
                    :key="colIdx"
                    class="px-6 py-1 text-xs whitespace-nowrap"
                    :class="colIdx === 0 ? 'font-medium text-gray-900 dark:text-white' : 'text-gray-600 dark:text-gray-300'"
                  >
                    {{ formatCellValue(row[col]) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Chart Card -->
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm mt-4">
          <div class="px-6 py-2 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
            <h3 class="text-sm font-medium text-gray-900 dark:text-white">
              {{ crosstabData.firstQuestion.label }}
            </h3>
            <div class="flex items-center gap-3">
              <label class="flex items-center cursor-pointer">
                <input
                  type="radio"
                  v-model="graphMode"
                  value="index"
                  class="w-3 h-3 text-blue-600 border-gray-300 focus:ring-blue-500"
                />
                <span class="ml-1.5 text-xs text-gray-700 dark:text-gray-300">Graph Index</span>
              </label>
              <label class="flex items-center cursor-pointer">
                <input
                  type="radio"
                  v-model="graphMode"
                  value="posneg"
                  class="w-3 h-3 text-blue-600 border-gray-300 focus:ring-blue-500"
                />
                <span class="ml-1.5 text-xs text-gray-700 dark:text-gray-300">Graph Pos/Neg Percent</span>
              </label>
            </div>
          </div>
          <div class="p-4">
          
          <!-- Index Mode Chart -->
          <div v-if="graphMode === 'index'" class="space-y-2">
            <div
              v-for="(bar, idx) in chartData"
              :key="idx"
              class="flex items-center gap-2"
            >
              <div class="w-24 text-xs text-gray-700 dark:text-gray-300 text-right truncate">
                {{ bar.label }}
              </div>
              <div class="flex-1 flex items-center gap-2">
                <div class="flex-1 bg-gray-200 dark:bg-gray-700 rounded-full h-6 overflow-hidden">
                  <div
                    :style="{ width: Math.min((bar.value / 200 * 100), 100) + '%', backgroundColor: bar.color }"
                    class="h-full transition-all"
                  ></div>
                </div>
                <div class="w-12 text-xs font-medium text-gray-900 dark:text-white text-right">
                  {{ bar.value }}
                </div>
              </div>
            </div>
            <!-- X-axis labels for index mode (0-200 scale) -->
            <div class="flex items-center gap-2 mt-3">
              <div class="w-24"></div>
              <div class="flex-1 flex justify-between text-xs text-gray-500 dark:text-gray-400 px-0.5">
                <span>0</span>
                <span>50</span>
                <span>100</span>
                <span>150</span>
                <span>200</span>
              </div>
              <div class="w-12"></div>
            </div>
          </div>
          
          <!-- Pos/Neg Mode Chart -->
          <div v-else class="space-y-3">
            <div
              v-for="(bar, idx) in chartData"
              :key="idx"
              class="flex items-start gap-2"
            >
              <div class="w-24 text-xs text-gray-700 dark:text-gray-300 text-right pt-1">
                {{ bar.label }}
              </div>
              <div class="flex-1 space-y-1">
                <div
                  v-for="(segment, segIdx) in bar.segments"
                  :key="segIdx"
                  class="flex items-center gap-2"
                >
                  <div class="flex-1 bg-gray-200 dark:bg-gray-700 rounded h-4 overflow-hidden max-w-md">
                    <div
                      :style="{ width: Math.min(segment.value, 100) + '%', backgroundColor: segment.color }"
                      class="h-full transition-all"
                      :title="`${segment.label}: ${segment.value.toFixed(1)}%`"
                    ></div>
                  </div>
                  <div class="w-10 text-xs font-medium text-gray-700 dark:text-gray-300 text-right">
                    {{ segment.value.toFixed(1) }}%
                  </div>
                </div>
              </div>
            </div>
            <!-- X-axis labels for pos/neg mode (0-100% scale) -->
            <div class="flex items-start gap-2 mt-3">
              <div class="w-24"></div>
              <div class="flex-1">
                <div class="flex justify-between text-xs text-gray-500 dark:text-gray-400 max-w-md px-0.5">
                  <span>0</span>
                  <span>25</span>
                  <span>50</span>
                  <span>75</span>
                  <span>100</span>
                </div>
              </div>
              <div class="w-10"></div>
            </div>
          </div>

          <!-- Legend -->
          <div v-if="graphMode === 'posneg'" class="mt-4 pt-3 border-t border-gray-200 dark:border-gray-700 flex flex-wrap gap-3">
            <div class="flex items-center gap-1.5">
              <div class="w-3 h-3 rounded bg-blue-500"></div>
              <span class="text-xs text-gray-700 dark:text-gray-300">Positive Indexes</span>
            </div>
            <div class="flex items-center gap-1.5">
              <div class="w-3 h-3 rounded bg-red-500"></div>
              <span class="text-xs text-gray-700 dark:text-gray-300">Negative Indexes</span>
            </div>
          </div>
          </div>
        </div>
          </div>
        </div>
      </div>
    
    <!-- Combined Explanation & Statistics Modal -->
    <div v-if="showExplanation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showExplanation = false">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
        <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Understanding Crosstabs & Statistics</h3>
            <CloseButton @click="showExplanation = false" />
          </div>
        </div>
        <div class="px-6 py-4 space-y-4">
          <!-- Statistical Measures Section -->
          <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
            <h4 class="font-semibold text-gray-900 dark:text-white mb-3 text-sm">Current Analysis Statistics</h4>
            <div class="grid grid-cols-2 gap-3">
              <!-- Chi-Square -->
              <div class="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Chi-Square (χ²)</div>
                <div class="text-xl font-bold text-gray-900 dark:text-white">{{ formatNumber(crosstabData?.chiSquare) }}</div>
                <div v-if="crosstabData?.significant" class="text-xs mt-1" :class="crosstabData.pValue < 0.05 ? 'text-green-600 dark:text-green-400 font-medium' : 'text-gray-500 dark:text-gray-400'">
                  {{ crosstabData.significant }}
                </div>
              </div>
              <!-- Phi -->
              <div class="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Phi (φ)</div>
                <div class="text-xl font-bold text-gray-900 dark:text-white">{{ formatNumber(crosstabData?.phi) }}</div>
              </div>
              <!-- Cramér's V -->
              <div class="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Cramér's V</div>
                <div class="text-xl font-bold text-gray-900 dark:text-white">{{ formatNumber(crosstabData?.cramersV) }}</div>
              </div>
              <!-- Total N -->
              <div class="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Total N</div>
                <div class="text-xl font-bold text-gray-900 dark:text-white">{{ crosstabData?.totalN || 0 }}</div>
              </div>
            </div>
          </div>
          <!-- Quick Tip -->
          <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
            <div class="flex gap-3">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
              </svg>
              <div>
                <h4 class="font-semibold text-gray-900 dark:text-white mb-1 text-sm">Quick Tip</h4>
                <p class="text-sm text-gray-700 dark:text-gray-300">{{ quickTipText }}</p>
              </div>
            </div>
          </div>
          
          <!-- What is Crosstab Analysis -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">What is Crosstab Analysis?</h4>
            <p class="text-sm text-gray-700 dark:text-gray-300">Crosstab (cross-tabulation) analysis examines the relationship between two categorical variables by displaying their frequencies in a table format. This helps identify patterns and correlations between different survey questions.</p>
          </div>
          
          <!-- Statistical Measures -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Statistical Measures</h4>
            <ul class="list-disc list-inside space-y-2 ml-2 text-sm text-gray-700 dark:text-gray-300">
              <li><strong>Chi-Square (χ²):</strong> Tests whether a relationship exists (statistically significant vs random chance). If p&lt;0.05, the relationship is real, not due to chance.</li>
              <li><strong>Cramér's V:</strong> Measures the <em>strength</em> of the relationship (0=no association, 0.1=weak, 0.3=moderate, 0.5+=strong). A relationship can be statistically significant but weak.</li>
              <li><strong>Phi (φ):</strong> Similar to Cramér's V but specifically for 2×2 tables</li>
              <li><strong>Total N:</strong> Total number of valid responses analyzed</li>
            </ul>
            <div class="mt-3 p-3 bg-gray-50 dark:bg-gray-700/50 rounded border border-gray-200 dark:border-gray-600">
              <p class="text-sm text-gray-700 dark:text-gray-300"><strong>Understanding Both Together:</strong></p>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">Think of Chi-Square as asking "Is there smoke?" (Is the relationship real?) and Cramér's V as asking "How big is the fire?" (How strong is it?). A relationship can be real (significant Chi-Square) but weak (low Cramér's V), meaning other factors likely have stronger effects.</p>
            </div>
          </div>
          
          <!-- Graph Modes -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Graph Modes</h4>
            <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
              <li><strong>Graph Index:</strong> Shows a single overall sentiment score (0-200 scale) for each group, where 100 is neutral, above 100 is positive, and below 100 is negative</li>
              <li><strong>Graph Pos/Neg Percent:</strong> Shows the actual percentage of people who responded positively (blue) vs. negatively (red) in each group. This makes it easy to see exactly how many people in each category were satisfied/dissatisfied</li>
            </ul>
          </div>
          
          <!-- Index Values Explained -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Index Values Explained</h4>
            <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
              <li><strong>Positive Indexes:</strong> Percentage of favorable/desirable responses (e.g., "Strongly Agree", "Very Satisfied")</li>
              <li><strong>Negative Indexes:</strong> Percentage of unfavorable/undesirable responses (e.g., "Strongly Disagree", "Very Dissatisfied")</li>
              <li><strong>Overall Index:</strong> Combined measure where higher values indicate more positive sentiment</li>
            </ul>
          </div>
        </div>
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
          <Button @click="showExplanation = false">
            Close
          </Button>
        </div>
      </div>
    </div>
    
    <!-- AI Analysis Modal -->
    <div v-if="showAIModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showAIModal = false">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
        <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
          <div class="relative flex items-center justify-between">
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
              </svg>
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">AI Analysis</h3>
            </div>
            <div class="absolute left-1/2 transform -translate-x-1/2">
              <span class="text-base font-semibold text-blue-600 dark:text-blue-400 whitespace-nowrap">
                {{ crosstabData.firstQuestion.label }} <span class="font-normal">by</span> {{ crosstabData.secondQuestion.label }}
              </span>
            </div>
            <CloseButton @click="showAIModal = false" />
          </div>
        </div>
        <div class="px-6 py-4">
          <!-- Loading State -->
          <div v-if="loadingAnalysis" class="flex flex-col items-center justify-center py-8">
            <svg class="animate-spin h-10 w-10 text-blue-600 dark:text-blue-400 mb-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <p class="text-sm text-gray-600 dark:text-gray-400">Generating AI analysis...</p>
          </div>
          
          <!-- Analysis Content -->
          <div v-else-if="aiAnalysis" class="space-y-4">
            <div v-for="(section, idx) in parseAIAnalysis(aiAnalysis)" :key="idx">
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
              <Button
                @click="generateAIAnalysis"
                variant="ghost"
                size="sm"
                :loading="loadingAnalysis"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                Regenerate Analysis
              </Button>
            </div>
          </div>
          
          <!-- Generate Prompt -->
          <div v-else class="py-8">
            <div class="text-center mb-6">
              <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-blue-100 dark:bg-blue-900/30 mb-4">
                <svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h4 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">Generate AI Analysis</h4>
              <p class="text-sm text-gray-600 dark:text-gray-400 max-w-md mx-auto">
                Our AI will analyze your crosstab data and provide insights including a summary, statistical interpretation, category comparisons, and actionable recommendations.
              </p>
            </div>
            <div class="flex justify-center">
              <Button
                @click="generateAIAnalysis"
                :loading="loadingAnalysis"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
                Generate Analysis
              </Button>
            </div>
          </div>
        </div>
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
          <Button @click="showAIModal = false">
            Close
          </Button>
        </div>
      </div>
    </div>
    
    <!-- Statistics Modal -->
    <!-- Removed: Now merged with Explanation modal above -->
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import Button from '../common/Button.vue'
import CloseButton from '../common/CloseButton.vue'

const router = useRouter()
const route = useRoute()

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  firstQuestion: {
    type: Object,
    default: null
  },
  secondQuestion: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['selections-changed'])

// Check if navigated from Stat Sig view
const isFromStatSig = computed(() => route.query.fromStatSig === 'true')

// State
const loading = ref(false)
const error = ref(null)
const crosstabData = ref(null)
const graphMode = ref('index') // 'index' or 'posneg'
const showExplanation = ref(false)
const showAIModal = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)

// Track last request to prevent duplicates
const lastRequestKey = ref('')

// Watch for prop changes
// Auto-generate crosstab when both questions are selected
watch([() => props.firstQuestion, () => props.secondQuestion], ([first, second]) => {
  if (first && second) {
    // Create a unique key for this request
    const requestKey = `${first.id}-${second.id}`
    
    // Only generate if it's a different request than last time
    if (requestKey !== lastRequestKey.value) {
      lastRequestKey.value = requestKey
      generateCrosstab()
    }
  } else {
    crosstabData.value = null
    lastRequestKey.value = ''
  }
}, { immediate: true })

// Computed
const dvResponses = computed(() => {
  // Get response labels from firstQuestion for partial crosstab display
  if (!props.firstQuestion?.responses) return []
  return props.firstQuestion.responses.map(r => r.label)
})

const tableColumns = computed(() => {
  if (!crosstabData.value || !crosstabData.value.table || crosstabData.value.table.length === 0) {
    return []
  }
  // Include all columns, even those with space as name (first column)
  return Object.keys(crosstabData.value.table[0])
})

const quickTipText = computed(() => {
  if (!crosstabData.value) return ''
  
  const firstVar = props.firstQuestion?.label || 'first variable'
  const secondVar = props.secondQuestion?.label || 'second variable'
  
  if (graphMode.value === 'index') {
    return `The Graph Index shows a single overall sentiment score (0-200 scale) for each category of "${secondVar}". A score of 100 is neutral, above 100 is positive, and below 100 is negative. This helps you quickly compare how different groups feel about "${firstVar}".`
  } else {
    return `The Positive/Negative graph shows the percentage of positive (blue) vs. negative (red) responses for each category of "${secondVar}". Use this to quickly identify which groups are most positive or negative about "${firstVar}".`
  }
})

const chartData = computed(() => {
  if (!crosstabData.value) return []

  const columns = tableColumns.value.filter(col => col !== '' && !col.toLowerCase().includes('total'))
  
  if (graphMode.value === 'index') {
    // Index mode: Show one bar per DV column with its index value
    return columns.map(col => {
      const indexInfo = crosstabData.value.ivIndexes?.find(idx => idx.label === col)
      return {
        label: col,
        value: indexInfo?.index || 0,
        color: '#3b82f6' // blue
      }
    }).filter(bar => bar.value > 0) // Don't show bars with zero value
  } else {
    // Pos/Neg mode: Show stacked bars with positive and negative index percentages
    return columns.map(col => {
      const indexInfo = crosstabData.value.ivIndexes?.find(idx => idx.label === col)
      const posIndex = indexInfo?.posIndex || 0
      const negIndex = indexInfo?.negIndex || 0
      
      return {
        label: col,
        segments: [
          { label: 'Positive', value: posIndex, color: '#3b82f6', width: posIndex },
          { label: 'Negative', value: negIndex, color: '#ef4444', width: negIndex }
        ].filter(s => s.value > 0)
      }
    }).filter(bar => bar.segments.length > 0) // Don't show bars with no segments
  }
})

// Methods
async function generateCrosstab() {
  if (!props.firstQuestion || !props.secondQuestion) {
    return
  }

  loading.value = true
  error.value = null
  aiAnalysis.value = '' // Reset AI analysis when generating new crosstab

  try {
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/crosstab`,
      {
        firstQuestionId: props.firstQuestion.id,
        secondQuestionId: props.secondQuestion.id
      }
    )
    crosstabData.value = response.data
    
    // Reset AI analysis when new crosstab is generated
    aiAnalysis.value = ''
  } catch (err) {
    console.error('Failed to generate crosstab:', err)
    console.error('Error details:', err.response?.data)
    error.value = err.response?.data?.title || err.response?.data || 'Failed to generate crosstab. Please try again.'
  } finally {
    loading.value = false
  }
}

async function generateAIAnalysis() {
  if (!crosstabData.value) return
  
  loadingAnalysis.value = true
  
  try {
    // Prepare context for AI
    // In crosstab "X by Y": X is dependent (measured outcome), Y is independent (grouping variable)
    const context = {
      dependentVariable: props.firstQuestion.label,  // Column variable - what we're measuring
      independentVariable: props.secondQuestion.label,  // Row variable - how we group/segment
      totalN: crosstabData.value.totalN,
      chiSquare: crosstabData.value.chiSquare,
      chiSquareSignificant: crosstabData.value.chiSquareSignificant,
      phi: crosstabData.value.phi,
      cramersV: crosstabData.value.cramersV,
      tableData: crosstabData.value.tableData,
      indexes: crosstabData.value.ivIndexes
    }
    
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/analyze-crosstab`,
      context
    )
    
    aiAnalysis.value = response.data.analysis
  } catch (err) {
    console.error('Error generating AI analysis:', err)
    aiAnalysis.value = 'Unable to generate analysis at this time. The statistical results above show the key findings from your crosstab.'
  } finally {
    loadingAnalysis.value = false
  }
}

function formatNumber(value) {
  if (typeof value !== 'number') return value
  return value.toFixed(3)
}

function backToStatSig() {
  // Navigate back to Stat Sig view with the DV question (firstQuestion) selected
  router.push({
    name: 'analytics',
    params: { id: props.surveyId },
    query: {
      section: 'statsig',
      questionId: props.firstQuestion.id
    }
  })
}

function parseAIAnalysis(text) {
  if (!text) return []
  
  const sections = []
  const lines = text.split('\n')
  let currentSection = null
  
  for (const line of lines) {
    const trimmedLine = line.trim()
    
    // Check if line is a heading (starts with ##)
    if (trimmedLine.startsWith('## ')) {
      // Save previous section if it exists
      if (currentSection) {
        sections.push(currentSection)
      }
      // Start new section
      currentSection = {
        heading: trimmedLine.substring(3).trim(),
        content: ''
      }
    } else if (trimmedLine && currentSection) {
      // Add content to current section
      if (currentSection.content) {
        currentSection.content += ' '
      }
      currentSection.content += trimmedLine
    } else if (trimmedLine && !currentSection) {
      // Content without a heading (shouldn't happen with new format, but handle gracefully)
      sections.push({
        heading: null,
        content: trimmedLine
      })
    }
  }
  
  // Don't forget the last section
  if (currentSection) {
    sections.push(currentSection)
  }
  
  return sections
}

function formatPValue(value) {
  if (value == null) return '—'
  if (value < 0.001) return '<.001'
  if (value < 0.01) return '<.01'
  if (value < 0.05) return '<.05'
  return value.toFixed(3)
}

function formatCellValue(value) {
  if (value === null || value === undefined) return '-'
  if (typeof value === 'number') {
    return value % 1 === 0 ? value : value.toFixed(1)
  }
  return value
}

// Watch for surveyId changes to clear state
watch(() => props.surveyId, (newId, oldId) => {
  if (newId && oldId && newId !== oldId) {
    crosstabData.value = null
    aiAnalysis.value = ''
    error.value = null
    loading.value = false
    lastRequestKey.value = ''
  }
}, { immediate: false })
</script>

<style scoped>
.glow-subtle {
  @apply drop-shadow-[0_0_20px_rgba(255,255,255,0.6)] 
         drop-shadow-[0_0_50px_rgba(147,197,253,0.4)];
}

.glow-intense {
  @apply drop-shadow-[0_0_30px_rgba(255,255,255,0.8)] 
         drop-shadow-[0_0_70px_rgba(147,197,253,0.6)];
}
</style>
