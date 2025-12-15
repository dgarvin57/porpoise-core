<template>
  <!-- Horizontal Splitter: Top = Results Table, Bottom = Analysis Tabs + Charts -->
  <Splitter layout="horizontal" class="h-full">
    <!-- Top Panel: Results Table + Question/Block Stem -->
    <SplitterPanel :size="35" :minSize="20" :maxSize="60">
      <div class="h-full overflow-y-auto bg-white dark:bg-gray-800 [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb:hover]:bg-gray-400 dark:[&::-webkit-scrollbar-thumb:hover]:bg-gray-500">
        <!-- Show ResultsTable with data when question is selected -->
        <div v-if="selectedQuestionWithResponses" 
             class="border-b border-gray-200 dark:border-gray-700">
          <ResultsTable 
            :key="selectedQuestionWithResponses.id"
            :question="selectedQuestionWithResponses"
            :columnMode="columnMode"
            :surveyId="surveyId"
            :activeSection="activeSection"
            @column-mode-changed="$emit('column-mode-changed', $event)"
            @analyze-crosstab="$emit('analyze-crosstab', $event)"
          />
        </div>
        
        <!-- Show skeleton/placeholder when no question is selected -->
        <div v-else class="border-b border-gray-200 dark:border-gray-700">
          <div class="sticky top-0 z-30 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 py-1">
            <div class="relative flex items-center justify-between px-4">
              <div class="flex items-center space-x-3 min-w-0">
                <div class="w-5 h-5 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                <div class="w-5 h-5 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                <div class="h-4 w-48 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
              </div>
              <div class="absolute left-[45%]">
                <span class="text-base font-semibold text-blue-600 dark:text-blue-400 text-left uppercase tracking-wider">
                  RESULTS
                </span>
              </div>
              <div class="flex items-center space-x-3 flex-shrink-0">
                <div class="flex items-center space-x-4">
                  <div class="h-3 w-24 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                  <div class="h-3 w-24 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                  <div class="h-3 w-24 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                </div>
              </div>
            </div>
          </div>
          
          <div style="height: 250px;">
            <Splitter layout="horizontal" class="h-full">
              <SplitterPanel :size="45" :minSize="30" :maxSize="60">
                <div class="h-full p-4">
                  <div class="space-y-3">
                    <div class="h-8 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                    <div class="h-8 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                    <div class="h-8 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                    <div class="h-8 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                  </div>
                </div>
              </SplitterPanel>
              
              <SplitterPanel :size="55" :minSize="40" :maxSize="70">
                <div class="h-full p-4">
                  <div class="space-y-2">
                    <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded animate-pulse w-3/4"></div>
                    <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                    <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded animate-pulse w-5/6"></div>
                  </div>
                </div>
              </SplitterPanel>
            </Splitter>
          </div>
        </div>
      </div>
    </SplitterPanel>

    <!-- Bottom Panel: Tab Navigation + Analysis Area -->
    <SplitterPanel :size="65" :minSize="40" :maxSize="80">
      <div class="h-full flex flex-col bg-gray-50 dark:bg-gray-900">
        <!-- Tabs Header (Fixed) -->
        <div class="flex-shrink-0 primevue-tabs-wrapper">
          <Tabs 
            :value="activeSection" 
            @update:value="$emit('tab-changed', $event)"
          >
            <TabList>
              <Tab v-for="tab in analysisTabs" :key="tab.id" :value="tab.id">
                <div class="flex items-center gap-1.5">
                  <svg v-if="tab.iconType === 'text'" class="w-3.5 h-3.5" viewBox="0 0 24 24">
                    <text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle" font-size="18" font-weight="600" fill="currentColor">{{ tab.icon }}</text>
                  </svg>
                  <svg v-else class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" :d="tab.icon" />
                  </svg>
                  <span>{{ tab.label }}</span>
                  <span v-if="tab.badge" class="ml-1 text-[10px] opacity-60">
                    ({{ tab.badge }})
                  </span>
                </div>
              </Tab>
            </TabList>
          </Tabs>
        </div>

        <!-- Analysis Content (Scrollable) -->
        <div class="flex-1 overflow-y-auto overflow-x-hidden min-h-0 [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb:hover]:bg-gray-400 dark:[&::-webkit-scrollbar-thumb:hover]:bg-gray-500">
          <!-- Split View: Results Panel + Crosstab side-by-side -->
          <Splitter v-show="splitViewEnabled && activeSection === 'crosstab'" layout="horizontal" class="h-full p-4">
            <SplitterPanel :size="40" :minSize="20" :maxSize="60">
              <div class="h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden flex flex-col">
                <div class="bg-gray-100 dark:bg-gray-900 px-3 py-1.5 border-b border-gray-200 dark:border-gray-700">
                  <h3 class="text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wide">Results</h3>
                </div>
                <div class="flex-1 overflow-hidden">
                  <ResultsView 
                    :surveyId="surveyId"
                    :preselectedQuestionId="selectedQuestionForSplit?.id"
                    :hideSidebar="true"
                  />
                </div>
              </div>
            </SplitterPanel>
            
            <SplitterPanel :size="60" :minSize="40" :maxSize="80">
              <div class="h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden flex flex-col">
                <div class="bg-gray-100 dark:bg-gray-900 px-3 py-1.5 border-b border-gray-200 dark:border-gray-700">
                  <h3 class="text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wide">Crosstab Analysis</h3>
                </div>
                <div class="flex-1 overflow-hidden">
                  <CrosstabView 
                    v-show="true"
                    :surveyId="surveyId"
                    :initialFirstQuestion="crosstabFirstQuestion"
                    :initialSecondQuestion="crosstabSecondQuestion"
                    @selections-changed="$emit('crosstab-selections-changed', $event)"
                  />
                </div>
              </div>
            </SplitterPanel>
          </Splitter>

          <!-- Regular Single View -->
          <div v-if="!splitViewEnabled || activeSection !== 'crosstab'" class="h-full min-h-0">
            <ResultsView 
              v-show="activeSection === 'results'" 
              :surveyId="surveyId"
              :preselectedQuestionId="selectedQuestionId"
              :hideSidebar="true"
              :hideTable="true"
              :initialQuestionId="selectedQuestionId"
              :initialExpandedBlocks="expandedBlocks"
              :initialColumnMode="columnMode"
              :initialInfoExpanded="infoExpanded"
              :initialInfoTab="infoTab"
              @question-selected="$emit('question-selected', $event)"
              @expanded-blocks-changed="$emit('expanded-blocks-changed', $event)"
              @column-mode-changed="$emit('column-mode-changed', $event)"
              @info-expanded-changed="$emit('info-expanded-changed', $event)"
              @info-tab-changed="$emit('info-tab-changed', $event)"
              @analyze-crosstab="$emit('analyze-crosstab', $event)"
            />

            <CrosstabView 
              v-show="activeSection === 'crosstab'" 
              :surveyId="surveyId"
              :hideSidebar="true"
              :initialFirstQuestion="crosstabFirstQuestion"
              :initialSecondQuestion="crosstabSecondQuestion"
              @selections-changed="$emit('crosstab-selections-changed', $event)"
            />

            <StatSigView 
              v-show="activeSection === 'statsig'" 
              :surveyId="surveyId"
              :selectedQuestion="selectedQuestionWithResponses"
              @question-selected="$emit('statsig-question-selected', $event)"
            />

            <!-- Placeholder Views -->
            <div v-show="activeSection === 'index'" class="h-full flex items-center justify-center">
              <div class="text-center">
                <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Index</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Quickest way to select target groups</p>
                <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★★ Priority - Coming soon...</p>
              </div>
            </div>

            <div v-show="activeSection === 'indexplus'" class="h-full flex items-center justify-center">
              <div class="text-center">
                <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Index +</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Compare two questions within a block</p>
                <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★ Priority - Coming soon...</p>
              </div>
            </div>

            <div v-show="activeSection === 'profile'" class="h-full flex items-center justify-center">
              <div class="text-center">
                <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Profile</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Quick targeting and media strategy advice</p>
                <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★★ Priority - Coming soon...</p>
              </div>
            </div>

            <div v-show="activeSection === 'oneresponse'" class="h-full flex items-center justify-center">
              <div class="text-center">
                <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">One Response</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Compare specific responses across a block</p>
                <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★ Priority - Coming soon...</p>
              </div>
            </div>

            <div v-show="activeSection === 'fullblock'" class="h-full flex items-center justify-center">
              <div class="text-center">
                <svg class="mx-auto h-12 w-12 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Full Block</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">See patterns when graphically displayed</p>
                <p class="mt-1 text-xs text-yellow-600 dark:text-yellow-400">★★★★★ Critical Priority - Coming soon...</p>
              </div>
            </div>

            <div v-show="activeSection === 'matchingblocks'" class="h-full flex items-center justify-center">
              <div class="text-center">
                <svg class="mx-auto h-12 w-12 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Matching Blocks</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Spot how responses differ across different blocks</p>
                <p class="mt-1 text-xs text-yellow-600 dark:text-yellow-400">★★★★★ Critical Priority - Coming soon...</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </SplitterPanel>
  </Splitter>
</template>

<script setup lang="ts">
import Splitter from 'primevue/splitter'
import SplitterPanel from 'primevue/splitterpanel'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import ResultsTable from './ResultsTable.vue'
import ResultsView from './ResultsView.vue'
import CrosstabView from './CrosstabView.vue'
import StatSigView from './StatSigView.vue'

// Props
defineProps<{
  surveyId: string
  activeSection: string
  selectedQuestionWithResponses: any
  selectedQuestionId: string | null
  selectedQuestionForSplit: any
  columnMode: string
  splitViewEnabled: boolean
  crosstabFirstQuestion: any
  crosstabSecondQuestion: any
  expandedBlocks: string[]
  infoExpanded: boolean
  infoTab: string
  analysisTabs: any[]
}>()

// Emits
defineEmits([
  'column-mode-changed',
  'analyze-crosstab',
  'tab-changed',
  'question-selected',
  'expanded-blocks-changed',
  'info-expanded-changed',
  'info-tab-changed',
  'crosstab-selections-changed',
  'statsig-question-selected'
])
</script>

<style scoped>
/* Custom scrollbar styling is already in the template with Tailwind classes */
</style>
