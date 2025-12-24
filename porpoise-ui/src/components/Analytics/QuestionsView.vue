<template>
  <div class="h-full flex flex-col">
    <!-- Loading State -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
        <p class="text-gray-500 dark:text-gray-400">Loading questions...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="text-center text-red-500">
        <p>{{ error }}</p>
        <button @click="loadQuestions" class="mt-4 px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded text-white">
          Retry
        </button>
      </div>
    </div>

    <!-- Main Content with Resizable Splitter -->
    <div v-else class="h-full flex bg-gray-50 dark:bg-gray-900">
      <!-- Left Panel: Question List (Resizable) -->
      <div :style="{ width: listWidth + '%' }" class="border-r border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 flex flex-col">
        <!-- Search -->
        <div class="p-2 border-b border-gray-200 dark:border-gray-700">
          <div class="relative">
            <input
              v-model="searchTerm"
              type="text"
              placeholder="Find question"
              autocomplete="off"
              data-1p-ignore
              data-lpignore="true"
              class="w-full px-3 py-1 pl-9 pr-9 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <svg class="absolute left-3 top-2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <button
              v-if="searchTerm"
              @click="searchTerm = ''"
              class="absolute right-2 top-2 p-0.5 rounded bg-transparent text-gray-500 hover:text-gray-900 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-white dark:hover:bg-gray-700 transition-colors"
              title="Clear search"
            >
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          <div class="flex items-center justify-between text-xs mt-0.5">
            <div class="flex items-center space-x-1.5">
              <a
                @click="expandAll"
                class="text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 cursor-pointer hover:underline"
              >
                Expand All
              </a>
              <span class="text-gray-400">|</span>
              <a
                @click="collapseAll"
                class="text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 cursor-pointer hover:underline"
              >
                Collapse All
              </a>
            </div>
          </div>
        </div>

        <!-- Question List -->
        <div class="flex-1 overflow-y-auto">
          <div v-for="item in filteredQuestions" :key="item.id || item.blockId">
            <!-- Block Group -->
            <div v-if="item.questions" class="mb-1">
              <button
                @click="toggleBlock(item.id)"
                class="w-full flex items-center space-x-2 px-2 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 rounded transition-colors text-left"
              >
                <span 
                  class="text-xs text-gray-700 dark:text-gray-300 transition-transform flex-shrink-0 font-bold"
                  :class="{ 'rotate-90': expandedBlocks.has(item.id) }"
                >▶</span>
                <svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
                </svg>
                <div class="flex-1 text-left min-w-0">
                  <span class="text-xs font-medium text-gray-800 dark:text-white truncate">
                    {{ item.label }}
                  </span>
                </div>
                <!-- Selected indicator - blue dot when a question in this block is selected -->
                <svg
                  v-if="item.questions.some(q => selectedQuestion?.id === q.id)"
                  class="w-4 h-4 text-blue-500 dark:text-blue-400 flex-shrink-0"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  title="Contains selected question"
                >
                  <circle cx="10" cy="10" r="4" />
                </svg>
                <span class="text-xs text-gray-400 dark:text-gray-500 flex-shrink-0">
                  {{ item.questions.length }}
                </span>
              </button>
              
              <div v-if="expandedBlocks.has(item.id)" class="mt-0.5 space-y-0">
                <button
                  v-for="question in item.questions"
                  :key="question.id"
                  @click="selectQuestion(question)"
                  :class="[
                    'w-full flex items-center space-x-2 px-2 py-1 rounded cursor-pointer transition-all border',
                    selectedQuestion?.id === question.id 
                      ? 'bg-blue-50 dark:bg-blue-900/30 border-blue-400/60 dark:border-blue-500/60' 
                      : 'hover:bg-gray-100 dark:hover:bg-gray-700 border-transparent'
                  ]"
                >
                  <div class="flex items-center space-x-2 flex-1 min-w-0 ml-6">
                    <svg :class="[getVariableTypeColor(question.variableType), 'w-3.5 h-3.5 flex-shrink-0']" fill="currentColor" viewBox="0 0 20 20">
                      <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                    </svg>
                    <span class="text-xs text-gray-700 dark:text-gray-300 flex-shrink-0">{{ question.qstNumber }}</span>
                    <span v-if="question.qstLabel" class="text-xs text-gray-400 dark:text-gray-500 truncate">{{ question.qstLabel }}</span>
                  </div>
                </button>
              </div>
            </div>
            
            <!-- Standalone Question -->
            <button
              v-else
              @click="selectQuestion(item)"
              :class="[
                'w-full flex items-center space-x-2 px-2 py-1 rounded cursor-pointer transition-all mb-1 border',
                selectedQuestion?.id === item.id 
                  ? 'bg-blue-50 dark:bg-blue-900/30 border-blue-400/60 dark:border-blue-500/60' 
                  : 'hover:bg-gray-100 dark:hover:bg-gray-700 border-transparent'
              ]"
            >
              <svg :class="[getVariableTypeColor(item.variableType), 'w-3.5 h-3.5 flex-shrink-0']" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
              </svg>
              <span class="text-xs text-gray-700 dark:text-gray-300 flex-shrink-0">{{ item.qstNumber }}</span>
              <span v-if="item.qstLabel" class="text-xs text-gray-400 dark:text-gray-500 truncate">{{ item.qstLabel }}</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Resizable Splitter -->
      <div 
        @mousedown="startDrag"
        class="w-1 bg-gray-200 dark:bg-gray-700 hover:bg-blue-500 dark:hover:bg-blue-600 cursor-col-resize transition-colors"
        :class="{ 'bg-blue-500 dark:bg-blue-600': isDragging }"
      ></div>

      <!-- Right Panel: Question Details (Max-width constrained) -->
      <div class="flex-1 overflow-y-auto p-3 bg-white dark:bg-gray-800">
        <div class="w-full max-w-[700px] mx-auto">
          <div v-if="selectedQuestion" class="space-y-2">
            
            <!-- Question Information Section (Collapsible) -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm">
              <div 
                @click="questionInfoExpanded = !questionInfoExpanded"
                class="flex items-center gap-2 p-3 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 rounded-t-lg"
              >
                <span 
                  class="text-xs text-gray-700 dark:text-gray-300 transition-transform flex-shrink-0 font-bold"
                  :class="{ 'rotate-90': questionInfoExpanded }"
                >▶</span>
                <h3 class="text-base font-semibold text-blue-600 dark:text-blue-400">Question Information</h3>
              </div>
              
              <div v-show="questionInfoExpanded" class="p-3 pt-0 space-y-2">
                <div class="grid grid-cols-2 gap-2">
                  <div>
                    <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Question Number</label>
                    <input
                      v-model="selectedQuestion.qstNumber"
                      @blur="e => saveQuestionField(selectedQuestion.id, 'qstNumber', e.target.value)"
                      type="text"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    />
                  </div>

                  <div>
                    <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Data File Column</label>
                    <input
                      v-model.number="selectedQuestion.dataFileCol"
                      @blur="e => saveQuestionField(selectedQuestion.id, 'dataFileCol', Number(e.target.value))"
                      type="number"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    />
                  </div>
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Question Label</label>
                  <input
                    v-model="selectedQuestion.qstLabel"
                    @blur="e => saveQuestionField(selectedQuestion.id, 'qstLabel', e.target.value)"
                    type="text"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  />
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Question Stem</label>
                  <textarea
                    v-model="selectedQuestion.qstStem"
                    @blur="e => saveQuestionField(selectedQuestion.id, 'qstStem', e.target.value)"
                    @input="autoResize($event.target)"
                    rows="2"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100 resize-none overflow-hidden"
                  ></textarea>
                </div>

                <div class="grid grid-cols-2 gap-2">
                  <div>
                    <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1 flex items-center gap-1.5">
                      <svg :class="[getVariableTypeColor(selectedQuestion.variableType), 'w-3.5 h-3.5 flex-shrink-0']" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                      </svg>
                      Variable Type
                      <button
                        type="button"
                        @click="showVariableTypeInfo = !showVariableTypeInfo"
                        class="text-blue-500 dark:text-blue-400 hover:text-blue-600 dark:hover:text-blue-300 cursor-pointer text-base leading-none"
                        title="Click for more info"
                      >ⓘ</button>
                    </label>
                    <div v-if="showVariableTypeInfo" class="mb-2 p-2 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded text-xs text-gray-700 dark:text-gray-300">
                      <strong>Independent variables</strong> (who people are — demographics, backgrounds like age, gender, location) appear across the top in crosstabs.<br>
                      <strong>Dependent variables</strong> (what people think — opinions, attitudes, behaviors being measured) appear down the side.
                    </div>
                    <select
                      v-model.number="selectedQuestion.variableType"
                      @change="e => saveQuestionField(selectedQuestion.id, 'variableType', Number(e.target.value))"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    >
                      <option :value="0">None</option>
                      <option :value="1">Independent (Who They Are)</option>
                      <option :value="2">Dependent (What They Think)</option>
                    </select>
                  </div>

                  <div>
                    <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1 flex items-center gap-1.5">
                      Data Type
                      <button
                        type="button"
                        @click="showDataTypeInfo = !showDataTypeInfo"
                        class="text-blue-500 dark:text-blue-400 hover:text-blue-600 dark:hover:text-blue-300 cursor-pointer text-base leading-none"
                        title="Click for more info"
                      >ⓘ</button>
                    </label>
                    <div v-if="showDataTypeInfo" class="mb-2 p-2 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded text-xs text-gray-700 dark:text-gray-300">
                      Auto-set during import. <strong>Nominal</strong> = unordered categories (≤12 options). <strong>Interval</strong> = ordered or many options (>12).
                    </div>
                    <select
                      v-model.number="selectedQuestion.dataType"
                      @change="e => saveQuestionField(selectedQuestion.id, 'dataType', Number(e.target.value))"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    >
                      <option :value="0">None</option>
                      <option :value="1">Nominal</option>
                      <option :value="2">Interval</option>
                      <option :value="3">Both</option>
                    </select>
                  </div>
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Missing Values</label>
                  <div class="grid grid-cols-3 gap-2">
                    <input
                      v-model.number="selectedQuestion.missValue1"
                      @blur="e => saveQuestionField(selectedQuestion.id, 'missValue1', e.target.value ? Number(e.target.value) : null)"
                      type="number"
                      placeholder="Value 1"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    />
                    <input
                      v-model.number="selectedQuestion.missValue2"
                      @blur="e => saveQuestionField(selectedQuestion.id, 'missValue2', e.target.value ? Number(e.target.value) : null)"
                      type="number"
                      placeholder="Value 2"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    />
                    <input
                      v-model.number="selectedQuestion.missValue3"
                      @blur="e => saveQuestionField(selectedQuestion.id, 'missValue3', e.target.value ? Number(e.target.value) : null)"
                      type="number"
                      placeholder="Value 3"
                      class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Block Information Section (Collapsible, only if in block) -->
            <div v-if="selectedQuestion.blockId" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm">
              <div 
                @click="blockInfoExpanded = !blockInfoExpanded"
                class="flex items-center gap-2 p-3 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 rounded-t-lg"
              >
                <span 
                  class="text-xs text-gray-700 dark:text-gray-300 transition-transform flex-shrink-0 font-bold"
                  :class="{ 'rotate-90': blockInfoExpanded }"
                >▶</span>
                <h3 class="text-base font-semibold text-blue-600 dark:text-blue-400">Block Information</h3>
              </div>
              
              <div v-show="blockInfoExpanded" class="p-3 pt-0 space-y-2">
                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Block Label</label>
                  <input
                    v-model="selectedQuestion.blkLabel"
                    @blur="e => saveBlockField(selectedQuestion.blockId, 'blkLabel', e.target.value)"
                    type="text"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  />
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Block Stem</label>
                  <textarea
                    v-model="selectedQuestion.blkStem"
                    @blur="e => saveBlockField(selectedQuestion.blockId, 'blkStem', e.target.value)"
                    @input="autoResize($event.target)"
                    rows="2"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100 resize-none overflow-hidden"
                  ></textarea>
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Block Status</label>
                  <select
                    v-model.number="selectedQuestion.blkQstStatus"
                    @change="e => saveQuestionField(selectedQuestion.id, 'blkQstStatus', Number(e.target.value))"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  >
                    <option :value="0">None</option>
                    <option :value="1">Block Header</option>
                    <option :value="2">Block Member</option>
                  </select>
                </div>

                <div v-if="blockQuestionCount > 1" class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded p-2">
                  <p class="text-xs text-yellow-800 dark:text-yellow-200">
                    ⚠ Changing this will update all {{ blockQuestionCount }} questions in this block
                  </p>
                </div>
              </div>
            </div>

            <!-- Responses Section (Collapsible) -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm">
              <div 
                @click="responsesExpanded = !responsesExpanded"
                class="flex items-center gap-2 p-3 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 rounded-t-lg"
              >
                <span 
                  class="text-xs text-gray-700 dark:text-gray-300 transition-transform flex-shrink-0 font-bold"
                  :class="{ 'rotate-90': responsesExpanded }"
                >▶</span>
                <h3 class="text-base font-semibold text-blue-600 dark:text-blue-400">Responses</h3>
              </div>
              
              <div v-show="responsesExpanded" class="p-3 pt-0">
                <div class="overflow-x-auto">
                  <table class="w-full text-sm">
                    <thead>
                      <tr class="border-b border-gray-200 dark:border-gray-700">
                        <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-16 text-xs">Value</th>
                        <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium text-xs">Label</th>
                        <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-40 text-xs">
                          <div class="flex items-center gap-1">
                            Favorability Index
                            <button
                              type="button"
                              @click="showIndexTypeInfo = !showIndexTypeInfo"
                              class="text-blue-500 dark:text-blue-400 hover:text-blue-600 dark:hover:text-blue-300 cursor-pointer text-base leading-none"
                              title="Click for more info"
                            >ⓘ</button>
                          </div>
                        </th>
                        <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-20 text-xs">Weight</th>
                        <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-16 text-xs">N</th>
                        <th class="w-8"></th>
                      </tr>
                      <tr v-if="showIndexTypeInfo">
                        <td colspan="6" class="py-2 px-2">
                          <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded p-2 text-xs text-gray-700 dark:text-gray-300">
                            <strong>Favorability Index</strong> marks responses as favorable (+), unfavorable (-), or neutral (/) to calculate net favorability scores.<br>
                            <strong>Example:</strong> Mark "Strongly Approve" and "Approve" as <strong>+ Positive</strong> and "Disapprove" and "Strongly Disapprove" as <strong>- Negative</strong> to get a net favorability score like +45 (68% favorable - 23% unfavorable).<br>
                            This creates comparable metrics across different questions and is standard in political polling and market research.
                          </div>
                        </td>
                      </tr>
                    </thead>
                    <tbody>
                      <tr
                        v-for="response in sortedResponses"
                        :key="response.id"
                        class="border-b border-gray-100 dark:border-gray-700 hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors"
                      >
                        <td class="py-1.5 px-2 text-gray-600 dark:text-gray-500 text-xs">{{ response.respValue }}</td>
                        <td class="py-1.5 px-2">
                          <input
                            v-model="response.label"
                            @blur="e => handleResponseFieldBlur(selectedQuestion.id, response.id, 'label', e)"
                            type="text"
                            class="w-full px-2 py-1 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-xs focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                          />
                        </td>
                        <td class="py-1.5 px-2">
                          <select
                            v-model="response.indexType"
                            @change="e => handleResponseFieldBlur(selectedQuestion.id, response.id, 'indexType', e)"
                            class="w-full px-2 py-1 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-xs focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                          >
                            <option value="None">None</option>
                            <option value="Neutral">/ Neutral</option>
                            <option value="Positive">+ Positive</option>
                            <option value="Negative">- Negative</option>
                          </select>
                        </td>
                        <td class="py-1.5 px-2">
                          <input
                            v-model.number="response.weight"
                            @blur="e => handleResponseFieldBlur(selectedQuestion.id, response.id, 'weight', e)"
                            type="number"
                            step="0.1"
                            class="w-full px-2 py-1 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-xs focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                          />
                        </td>
                        <td class="py-1.5 px-2 text-gray-700 dark:text-gray-400 text-xs">
                          {{ getFrequency(response.respValue) }}
                        </td>
                        <td class="py-1.5 px-2">
                          <button
                            @click="confirmDeleteResponse(response)"
                            class="text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 text-xs"
                            title="Delete response"
                          >
                            ✕
                          </button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>

                <div class="mt-2 flex items-center justify-between text-xs">
                  <div class="text-gray-700 dark:text-gray-500">
                    Total N: {{ totalN }} | Missing: {{ missingN }} | Valid: {{ validN }}
                    <span v-if="netFavorability !== null" class="ml-3 font-medium" :class="netFavorability >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'">
                      | Net Favorability: {{ netFavorability > 0 ? '+' : '' }}{{ netFavorability }}
                    </span>
                  </div>
                  <button
                    @click="addNewResponse"
                    class="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-xs font-medium"
                  >
                    + Add Response
                  </button>
                </div>
              </div>
            </div>

          </div>

          <!-- No Selection State -->
          <div v-else class="flex items-center justify-center h-full">
            <p class="text-gray-500 dark:text-gray-400">Select a question to view details</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { useQuestionEditor } from '@/composables/useQuestionEditor'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  }
})

// Get composable functionality
const {
  questions,
  selectedQuestion,
  loading,
  saving,
  error,
  responseFrequencies,
  questionsByBlock,
  loadQuestions,
  selectQuestion: selectQuestionFn,
  updateQuestion,
  updateResponse,
  addResponse,
  deleteResponse,
  updateBlock
} = useQuestionEditor(props.surveyId)

// Local state
const searchTerm = ref('')
const expandedBlocks = ref(new Set())
const questionInfoExpanded = ref(true)
const blockInfoExpanded = ref(false)
const responsesExpanded = ref(true)
const listWidth = ref(25) // percentage (start at 25%)
const isDragging = ref(false)
const showVariableTypeInfo = ref(false)
const showDataTypeInfo = ref(false)
const showIndexTypeInfo = ref(false)

// Load questions on mount
loadQuestions()

// Watch for selected question changes to auto-resize textareas
watch(selectedQuestion, async () => {
  if (selectedQuestion.value) {
    await nextTick()
    const textareas = document.querySelectorAll('textarea')
    textareas.forEach(autoResize)
  }
})

// Watch for question info expansion to auto-resize question stem textarea
watch(questionInfoExpanded, async () => {
  if (questionInfoExpanded.value) {
    await nextTick()
    const textareas = document.querySelectorAll('textarea')
    textareas.forEach(autoResize)
  }
})

// Watch for block info expansion to auto-resize block stem textarea
watch(blockInfoExpanded, async () => {
  if (blockInfoExpanded.value) {
    await nextTick()
    const textareas = document.querySelectorAll('textarea')
    textareas.forEach(autoResize)
  }
})

// Computed
const filteredQuestions = computed(() => {
  if (!searchTerm.value) {
    return questionsByBlock.value
  }

  const term = searchTerm.value.toLowerCase()
  return questionsByBlock.value
    .map(item => {
      if (item.questions) {
        // Block
        const filtered = item.questions.filter(q =>
          q.qstNumber?.toLowerCase().includes(term) ||
          q.qstLabel?.toLowerCase().includes(term)
        )
        return filtered.length > 0 ? { ...item, questions: filtered } : null
      } else {
        // Standalone
        return item.qstNumber?.toLowerCase().includes(term) ||
          item.qstLabel?.toLowerCase().includes(term)
          ? item
          : null
      }
    })
    .filter(Boolean)
})

const sortedResponses = computed(() => {
  if (!selectedQuestion.value?.responses) return []
  return [...selectedQuestion.value.responses].sort((a, b) => a.respValue - b.respValue)
})

const blockQuestionCount = computed(() => {
  if (!selectedQuestion.value?.blockId) return 0
  return questions.value.filter(q => q.blockId === selectedQuestion.value.blockId).length
})

const totalN = computed(() => {
  return Object.values(responseFrequencies.value).reduce((sum, count) => sum + count, 0)
})

const missingN = computed(() => {
  if (!selectedQuestion.value) return 0
  const missVals = [
    selectedQuestion.value.missValue1,
    selectedQuestion.value.missValue2,
    selectedQuestion.value.missValue3
  ].filter(v => v != null)
  
  return missVals.reduce((sum, val) => {
    return sum + (responseFrequencies.value[val] || 0)
  }, 0)
})

const validN = computed(() => totalN.value - missingN.value)

const netFavorability = computed(() => {
  if (!selectedQuestion.value?.responses || validN.value === 0) return null
  
  let positiveN = 0
  let negativeN = 0
  
  selectedQuestion.value.responses.forEach(response => {
    const freq = getFrequency(response.respValue)
    // indexType is a string enum from the API: "Positive", "Negative", "Neutral", "None"
    if (response.indexType === 'Positive') {
      positiveN += freq
    } else if (response.indexType === 'Negative') {
      negativeN += freq
    }
  })
  
  // If no index types are set, don't show net favorability
  if (positiveN === 0 && negativeN === 0) return null
  
  const positivePct = (positiveN / validN.value) * 100
  const negativePct = (negativeN / validN.value) * 100
  
  return Math.round(positivePct - negativePct)
})

// Methods
function autoResize(textarea) {
  if (!textarea) return
  textarea.style.height = 'auto'
  textarea.style.height = textarea.scrollHeight + 'px'
}

function startDrag(e) {
  isDragging.value = true
  const startX = e.clientX
  const startWidth = listWidth.value
  
  const onMouseMove = (moveEvent) => {
    const delta = ((moveEvent.clientX - startX) / window.innerWidth) * 100
    const newWidth = Math.min(Math.max(startWidth + delta, 15), 50) // Min 15%, max 50%
    listWidth.value = newWidth
  }
  
  const onMouseUp = () => {
    isDragging.value = false
    document.removeEventListener('mousemove', onMouseMove)
    document.removeEventListener('mouseup', onMouseUp)
  }
  
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

function toggleBlock(blockId) {
  if (expandedBlocks.value.has(blockId)) {
    expandedBlocks.value.delete(blockId)
  } else {
    expandedBlocks.value.add(blockId)
  }
}

function expandAll() {
  filteredQuestions.value.forEach(item => {
    if (item.questions) {
      expandedBlocks.value.add(item.id)
    }
  })
}

function collapseAll() {
  expandedBlocks.value.clear()
}

function getVariableTypeColor(type) {
  switch (type) {
    case 2: return 'text-blue-500' // Dependent
    case 1: return 'text-red-500'  // Independent
    default: return 'text-gray-500' // None
  }
}

async function selectQuestion(question) {
  await selectQuestionFn(question)
  
  // Auto-expand block if question is in one
  if (question.blockId) {
    expandedBlocks.value.add(question.blockId)
  }
  
  // Auto-resize textareas after question loads
  await nextTick()
  const textareas = document.querySelectorAll('textarea')
  textareas.forEach(autoResize)
}

function getFrequency(respValue) {
  return responseFrequencies.value[respValue.toString()] || responseFrequencies.value[respValue] || 0
}

async function saveQuestionField(questionId, field, value) {
  try {
    await updateQuestion(questionId, { [field]: value })
  } catch (err) {
    console.error('Failed to save question field:', err)
  }
}

async function saveBlockField(blockId, field, value) {
  if (!blockId) return
  
  try {
    await updateBlock(blockId, { [field]: value })
  } catch (err) {
    console.error('Failed to save block field:', err)
  }
}

async function saveResponse(questionId, response) {
  try {
    await updateResponse(questionId, response.id, {
      label: response.label,
      indexType: response.indexType,
      weight: response.weight
    })
  } catch (err) {
    console.error('Failed to save response:', err)
  }
}

function handleResponseFieldBlur(questionId, responseId, field, event) {
  // Find the response in the current question's responses to get all fields
  const question = questions.value.find(q => q.id === questionId)
  if (!question) return
  
  const response = question.responses.find(r => r.id === responseId)
  if (!response) return
  
  // Update the specific field from the event, use response for other fields
  const updates = {
    label: response.label,
    indexType: response.indexType,
    weight: response.weight,
    [field]: field === 'weight' ? (event.target.value ? Number(event.target.value) : null) : event.target.value
  }
  
  updateResponse(questionId, responseId, updates).catch(err => {
    console.error('Failed to save response:', err)
  })
}

async function addNewResponse() {
  // Calculate next value
  const maxValue = sortedResponses.value.length > 0
    ? Math.max(...sortedResponses.value.map(r => r.respValue))
    : 0
  
  try {
    await addResponse(selectedQuestion.value.id, {
      respValue: maxValue + 1,
      label: '',
      indexType: '',
      weight: 1.0
    })
  } catch (err) {
    console.error('Failed to add response:', err)
  }
}

async function confirmDeleteResponse(response) {
  if (confirm(`Delete response "${response.label}" (value ${response.respValue})?`)) {
    try {
      await deleteResponse(selectedQuestion.value.id, response.id)
    } catch (err) {
      console.error('Failed to delete response:', err)
    }
  }
}
</script>
