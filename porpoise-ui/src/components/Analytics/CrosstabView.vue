<template>
  <div class="h-full flex flex-col bg-gray-50 dark:bg-gray-900">
      <!-- Loading State - Skeleton (only show if no data exists yet) -->
      <div 
        v-if="loading && !crosstabData"
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
          <h3 class="mt-4 text-base font-medium text-gray-900 dark:text-white">No Variables Selected</h3>
          <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
            Click the <span class="font-semibold text-blue-600 dark:text-blue-400">toggle button</span> to the left of a question to select a dependent variable, then click a <span class="font-semibold text-blue-600 dark:text-blue-400">question label</span> to select an independent variable
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
              Click a <span class="font-semibold text-blue-600 dark:text-blue-400">question label</span> to select an independent variable
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
      <div v-else-if="crosstabData" class="h-full overflow-auto relative">
        <!-- Loading overlay when refreshing data -->
        <div 
          v-if="loading"
          class="absolute inset-0 bg-gray-50/80 dark:bg-gray-900/80 backdrop-blur-sm z-10 flex items-center justify-center"
        >
          <div class="flex items-center gap-3 bg-white dark:bg-gray-800 px-4 py-3 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700">
            <svg class="animate-spin h-5 w-5 text-blue-500" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span class="text-sm text-gray-700 dark:text-gray-300">Updating crosstab...</span>
          </div>
        </div>
        
        <div class="pt-3 px-6 pb-6 flex justify-center">
          <div class="w-full max-w-[833px]">
            <!-- Header with question label and buttons (matching StatSig tab) -->
            <div class="flex items-end justify-between mb-2 pb-2">
              <div>
                <h3 class="text-base font-semibold text-gray-900 dark:text-white flex items-center gap-2 flex-wrap">
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
              
              <div class="flex items-center gap-3">
                <div 
                  class="text-xs"
                  :class="displayMode === 'n' ? 'text-red-800 dark:text-red-400 font-semibold' : 'text-gray-600 dark:text-gray-400'"
                  :title="displayMode === 'n' ? 'Total number of survey responses used in this crosstab analysis' : 'Total sample size for this analysis'"
                >
                  <span class="font-medium">Total N:</span> {{ crosstabData.totalN }}
                </div>
                
                <!-- Display Mode Toggle (% vs N) -->
                <div 
                  class="flex items-center bg-gray-100 dark:bg-gray-700 rounded-md p-0.5"
                  title="Toggle between percentages and counts. When N is selected (red), the table shows actual response counts instead of percentages."
                >
                  <button
                    @click="displayMode = 'percent'"
                    :class="[
                      'px-2 py-0.5 text-xs font-medium rounded transition-colors',
                      displayMode === 'percent'
                        ? 'bg-white dark:bg-gray-600 text-gray-900 dark:text-white shadow-sm'
                        : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200'
                    ]"
                  >
                    %
                  </button>
                  <button
                    @click="displayMode = 'n'"
                    :class="[
                      'px-2 py-0.5 text-xs font-medium rounded transition-colors',
                      displayMode === 'n'
                        ? 'bg-red-600 dark:bg-red-600 text-white shadow-sm'
                        : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200'
                    ]"
                  >
                    N
                  </button>
                </div>
                
                <!-- Info Button -->
                <button 
                  @click="showExplanation = true"
                  class="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700"
                  title="Understanding crosstabs and statistical measures"
                >
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
                  </svg>
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
                    :class="[
                      'px-6 py-1.5 text-xs font-semibold uppercase tracking-wider',
                      idx === 0 ? 'text-left bg-blue-100 dark:bg-gray-600 text-gray-900 dark:text-white' : 'text-center text-gray-800 dark:text-gray-400'
                    ]"
                  >
                    {{ idx === 0 ? 'DV Response' : (col.trim() === '' ? '' : col) }}
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                <tr
                  v-for="(row, rowIdx) in displayTable"
                  :key="rowIdx"
                  :class="[
                    'hover:bg-blue-50 dark:hover:bg-gray-700 transition-colors',
                    getRowBackgroundClass(row, rowIdx)
                  ]"
                >
                  <td
                    v-for="(col, colIdx) in tableColumns"
                    :key="colIdx"
                    :class="[
                      'px-6 py-1 text-xs whitespace-nowrap',
                      colIdx === 0 ? 'font-semibold text-gray-900 dark:text-white bg-blue-50/50 dark:bg-gray-700/50' : 'text-center',
                      getCellColorClass(row, colIdx)
                    ]"
                  >
                    {{ formatCellValue(row[col], colIdx === 0 ? null : row[tableColumns[0]]) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Chart Card -->
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm mt-4">
          <!-- Graph Mode Tabs -->
          <div class="border-b border-gray-200 dark:border-gray-700">
            <div class="flex">
              <button
                @click="graphMode = 'index'"
                :class="[
                  'px-4 py-1.5 text-xs font-medium transition-colors border-b-2',
                  graphMode === 'index'
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20'
                    : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700/50'
                ]"
              >
                Graph Index
              </button>
              <button
                @click="graphMode = 'posneg'"
                :class="[
                  'px-4 py-1.5 text-xs font-medium transition-colors border-b-2',
                  graphMode === 'posneg'
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20'
                    : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700/50'
                ]"
              >
                Graph Pos/Neg Percent
              </button>
              <div class="flex-1 border-b-2 border-transparent"></div>
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
                    class="h-full rounded-full transition-all"
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
          
          <!-- How to Create a Crosstab -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">How to Create a Crosstab</h4>
            <ul class="text-sm text-gray-700 dark:text-gray-300 space-y-1.5 ml-2">
              <li class="flex gap-2"><span class="text-blue-600 dark:text-blue-400 font-semibold">1.</span> <span>Click the <strong>toggle button</strong> next to your first question (Dependent Variable - what you're measuring)</span></li>
              <li class="flex gap-2"><span class="text-green-600 dark:text-green-400 font-semibold">2.</span> <span>Click the <strong>question label</strong> for your second question (Independent Variable - how you're grouping)</span></li>
              <li class="flex gap-2"><span class="text-gray-500 dark:text-gray-400">→</span> <span>The crosstab will generate automatically showing relationships between the two variables</span></li>
            </ul>
          </div>
          
          <!-- Display Mode Toggle -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">% / N Toggle</h4>
            <ul class="text-sm text-gray-700 dark:text-gray-300 space-y-1.5 ml-2">
              <li><strong>% (Percentages):</strong> Shows what percentage of each group chose each response</li>
              <li><strong class="text-red-800 dark:text-red-400">N (Counts):</strong> Shows the actual number of respondents in each cell. When active, the N button turns red and all numbers appear in dark red.</li>
              <li class="text-xs italic text-gray-600 dark:text-gray-400 ml-4">Note: Index and Marginal Percentage rows remain unchanged in both modes</li>
            </ul>
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
    <AIAnalysisModal
      :show="showAIModal"
      :questionLabel="crosstabData ? `${crosstabData.firstQuestion.label} by ${crosstabData.secondQuestion.label}` : ''"
      :analysis="aiAnalysis"
      :loading="loadingAnalysis"
      context="Crosstab"
      @close="showAIModal = false"
      @generate="generateAIAnalysis"
      @regenerate="generateAIAnalysis"
    />
    
    <!-- Onboarding Tooltips -->
    <!-- DV Selection Tooltip (shown when no variables selected) -->
    <OnboardingTooltip
      :show="showDVTooltip"
      position="left"
      stepNumber="1"
      badgeColor="blue"
      title="Select a Dependent Variable"
      message="To start your crosstab analysis, hover to the LEFT of any question in the list (over the left margin area). A toggle button will appear - click it to select your dependent variable (what you want to measure)."
      :showVisualGuide="true"
      guideType="dv"
      visualGuideLabel="Click button to the LEFT of question"
      @dismiss="handleDVTooltipDismiss"
    />
    
    <!-- IV Selection Tooltip (shown when DV selected but no IV) -->
    <OnboardingTooltip
      :show="showIVTooltip"
      position="left"
      stepNumber="2"
      badgeColor="green"
      title="Now Select an Independent Variable"
      message="Great! Now click directly on any question label in the list to select your independent variable (how you want to group your data). The crosstab will generate automatically."
      :showVisualGuide="true"
      guideType="iv"
      visualGuideLabel="Click question label"
      @dismiss="handleIVTooltipDismiss"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import Button from '../common/Button.vue'
import CloseButton from '../common/CloseButton.vue'
import OnboardingTooltip from '../common/OnboardingTooltip.vue'
import AIAnalysisModal from './AIAnalysisModal.vue'

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
  },
  triggerAIModal: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['selections-changed', 'ai-modal-shown'])

// Check if navigated from Stat Sig view
const isFromStatSig = computed(() => route.query.fromStatSig === 'true')

// State
const loading = ref(false)
const error = ref(null)
const crosstabData = ref(null)
const graphMode = ref('index') // 'index' or 'posneg'
const displayMode = ref('percent') // 'percent' or 'n' - for table display (always starts as percent)
const showExplanation = ref(false)
const showAIModal = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)

// Onboarding tooltips state
const showIVTooltip = ref(false)
const showDVTooltip = ref(false)

// Track last request to prevent duplicates
const lastRequestKey = ref('')

// Check localStorage for tooltip preferences
const hideIVTooltip = ref(localStorage.getItem('hideIVTooltip') === 'true')
const hideDVTooltip = ref(localStorage.getItem('hideDVTooltip') === 'true')

// Listen for storage changes (when Reset Tips is clicked in same window)
const checkTooltipPreferences = () => {
  const newHideIV = localStorage.getItem('hideIVTooltip') === 'true'
  const newHideDV = localStorage.getItem('hideDVTooltip') === 'true'
  
  // If preferences changed from hidden to not hidden, update and potentially show tooltips
  if (hideDVTooltip.value && !newHideDV) {
    hideDVTooltip.value = false
    // Show DV tooltip if appropriate
    if (!props.firstQuestion && !props.secondQuestion) {
      setTimeout(() => {
        if (!props.firstQuestion && !props.secondQuestion) {
          showDVTooltip.value = true
        }
      }, 500)
    }
  }
  
  if (hideIVTooltip.value && !newHideIV) {
    hideIVTooltip.value = false
    // Show IV tooltip if appropriate
    if (props.firstQuestion && !props.secondQuestion && !crosstabData.value) {
      setTimeout(() => {
        if (props.firstQuestion && !props.secondQuestion) {
          showIVTooltip.value = true
        }
      }, 500)
    }
  }
}

// Poll localStorage every second to check for changes (since storage events don't fire in same window)
const tooltipCheckInterval = setInterval(checkTooltipPreferences, 1000)

// Cleanup on unmount
onUnmounted(() => {
  clearInterval(tooltipCheckInterval)
})

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
    
    // Show appropriate tooltip based on state
    if (!first && !second && !hideDVTooltip.value) {
      // No variables selected - show DV tooltip after a short delay
      setTimeout(() => {
        if (!props.firstQuestion && !props.secondQuestion) {
          showDVTooltip.value = true
        }
      }, 500)
    }
  }
}, { immediate: true })

// Watch for when DV is selected but IV is not - show IV tooltip
watch(() => props.firstQuestion, (newVal, oldVal) => {
  if (newVal && !props.secondQuestion && !hideIVTooltip.value && !crosstabData.value) {
    setTimeout(() => {
      if (props.firstQuestion && !props.secondQuestion) {
        showIVTooltip.value = true
      }
    }, 500)
  }
  
  // Auto-dismiss DV tooltip when DV is selected
  if (newVal && !oldVal && showDVTooltip.value) {
    showDVTooltip.value = false
  }
})

// Auto-dismiss IV tooltip when IV is selected
watch(() => props.secondQuestion, (newVal, oldVal) => {
  if (newVal && !oldVal && showIVTooltip.value) {
    showIVTooltip.value = false
  }
})

// Watch for parent triggering AI modal
watch(() => props.triggerAIModal, (newVal) => {
  if (newVal) {
    showAIModal.value = true
    emit('ai-modal-shown')
  }
})

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

// Transform table data based on display mode (% or N)
const displayTable = computed(() => {
  if (!crosstabData.value || !crosstabData.value.table) return []
  
  // If showing percentages, return original table (formatted percentages)
  if (displayMode.value === 'percent') {
    return crosstabData.value.table
  }
  
  // If showing N, calculate N from percentage strings
  const totalN = crosstabData.value.totalN || 0
  const firstColName = tableColumns.value[0] || ''
  
  // Get the marginal percentage row to calculate column totals
  const marginalRow = crosstabData.value.table.find(r => {
    const label = r[firstColName]
    return label && label.trim().toLowerCase().includes('marginal')
  })
  
  // Get the original table structure to know row order
  return crosstabData.value.table.map(row => {
    const newRow = { ...row }
    const rowLabel = row[firstColName]
    const trimmedLabel = rowLabel ? rowLabel.trim() : ''
    
    // Keep Index and Marginal Percentage rows as-is (they already have the right values)
    if (trimmedLabel.toLowerCase().includes('index') || trimmedLabel.toLowerCase().includes('marginal')) {
      return newRow
    }
    
    // For data rows, calculate N from percentage strings
    tableColumns.value.forEach((col, idx) => {
      if (idx === 0) return // Skip first column (row labels)
      
      const cellValue = row[col]
      
      // Check if it's a percentage string (e.g., "55.4%")
      if (typeof cellValue === 'string' && cellValue.includes('%')) {
        // Parse the percentage value
        const percentValue = parseFloat(cellValue.replace('%', ''))
        
        if (!isNaN(percentValue)) {
          // Get the column total from marginal percentage
          const colMarginalStr = marginalRow ? marginalRow[col] : '100%'
          const colMarginal = parseFloat(colMarginalStr.replace('%', ''))
          const columnTotal = Math.round((totalN * colMarginal) / 100)
          
          // Convert percentage to count: N = (percentage * columnTotal) / 100
          const countValue = Math.round((percentValue * columnTotal) / 100)
          
          newRow[col] = countValue
        }
      }
    })
    
    return newRow
  })
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

function formatPValue(value) {
  if (value == null) return '—'
  if (value < 0.001) return '<.001'
  if (value < 0.01) return '<.01'
  if (value < 0.05) return '<.05'
  return value.toFixed(3)
}

function formatCellValue(value, rowLabel = null) {
  if (value === null || value === undefined) return '-'
  if (typeof value === 'string') return value
  if (typeof value === 'number') {
    // In N mode, show whole numbers without decimals (except for Index and Marginal Percentage rows)
    if (displayMode.value === 'n' && rowLabel !== 'Index' && rowLabel !== 'Marginal Percentage') {
      return Math.round(value)
    }
    // In percent mode or for Index/Marginal Percentage, show decimals
    return value % 1 === 0 ? value : value.toFixed(1)
  }
  return value
}

// Helper function to determine row background styling
function getRowBackgroundClass(row, rowIdx) {
  const rowLabel = row[tableColumns.value[0]]?.toString().toLowerCase() || ''
  
  // Index row - subtle blue tint
  if (rowLabel.includes('index') && !rowLabel.includes('marginal')) {
    return 'bg-blue-50/70 dark:bg-gray-700/70'
  }
  
  // Marginal Percentage row - subtle gray tint
  if (rowLabel.includes('marginal')) {
    return 'bg-gray-100 dark:bg-gray-700'
  }
  
  // Regular rows - no alternating, just white/dark
  return 'bg-white dark:bg-gray-800'
}

// Helper function to determine cell text color
function getCellColorClass(row, colIdx) {
  if (colIdx === 0) return ''
  
  const rowLabel = row[tableColumns.value[0]]?.toString().toLowerCase() || ''
  const isSpecialRow = rowLabel.includes('index') || rowLabel.includes('marginal')
  
  // Special rows always use normal color
  if (isSpecialRow) {
    return 'text-gray-600 dark:text-gray-300 font-medium'
  }
  
  // Data rows: red in N mode, gray in percent mode
  return displayMode.value === 'n' 
    ? 'text-red-800 dark:text-red-400 font-semibold' 
    : 'text-gray-600 dark:text-gray-300'
}

// Tooltip handlers
function handleDVTooltipDismiss(dontShow) {
  showDVTooltip.value = false
  if (dontShow) {
    localStorage.setItem('hideDVTooltip', 'true')
    hideDVTooltip.value = true
  }
}

function handleIVTooltipDismiss(dontShow) {
  showIVTooltip.value = false
  if (dontShow) {
    localStorage.setItem('hideIVTooltip', 'true')
    hideIVTooltip.value = true
  }
}

// Watch for surveyId changes to clear state
watch(() => props.surveyId, (newId, oldId) => {
  if (newId && oldId && newId !== oldId) {
    crosstabData.value = null
    aiAnalysis.value = ''
    error.value = null
    loading.value = false
    lastRequestKey.value = ''
    displayMode.value = 'percent' // Reset to percentages when survey changes
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
