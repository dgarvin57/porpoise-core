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

    <!-- Main Content -->
    <div v-else class="h-full flex">
      <!-- Master List (Left Panel - 30%) -->
      <div class="w-[30%] border-r border-gray-200 dark:border-gray-700 flex flex-col bg-white dark:bg-gray-900">
        <!-- Search -->
        <div class="p-2 border-b border-gray-200 dark:border-gray-700">
          <input
            v-model="searchTerm"
            type="text"
            placeholder="Search questions..."
            class="w-full px-2 py-1.5 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100 placeholder-gray-500"
          />
        </div>

        <!-- Question List -->
        <div class="flex-1 overflow-y-auto">
          <div v-for="item in filteredQuestions" :key="item.id || item.blockId">
            <!-- Block Group -->
            <div v-if="item.questions" class="mb-1">
              <button
                @click="toggleBlock(item.id)"
                class="w-full text-left px-2 py-1.5 hover:bg-gray-100 dark:hover:bg-gray-800 flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300"
              >
                <span class="text-xs">{{ expandedBlocks.has(item.id) ? '▼' : '▶' }}</span>
                <span class="truncate">{{ item.label }}</span>
                <span class="text-xs text-gray-500">({{ item.questions.length }})</span>
              </button>
              
              <div v-if="expandedBlocks.has(item.id)" class="ml-4 border-l border-gray-200 dark:border-gray-700">
                <button
                  v-for="question in item.questions"
                  :key="question.id"
                  @click="selectQuestion(question)"
                  :class="[
                    'w-full text-left px-2 py-1.5 flex items-center gap-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-900 dark:text-gray-100',
                    selectedQuestion?.id === question.id ? 'bg-blue-50 dark:bg-blue-900/30 border-l-2 border-blue-500' : ''
                  ]"
                >
                  <span :class="getVariableTypeColor(question.variableType)">●</span>
                  <span class="truncate">{{ question.qstNumber }}{{ question.qstLabel ? ': ' + question.qstLabel : '' }}</span>
                </button>
              </div>
            </div>

            <!-- Standalone Question -->
            <button
              v-else
              @click="selectQuestion(item)"
              :class="[
                'w-full text-left px-2 py-1.5 flex items-center gap-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-900 dark:text-gray-100',
                selectedQuestion?.id === item.id ? 'bg-blue-50 dark:bg-blue-900/30 border-l-2 border-blue-500' : ''
              ]"
            >
              <span :class="getVariableTypeColor(item.variableType)">●</span>
              <span class="truncate">{{ item.qstNumber }}{{ item.qstLabel ? ': ' + item.qstLabel : '' }}</span>
            </button>
          </div>
        </div>

        <!-- Total Count -->
        <div class="px-2 py-1.5 border-t border-gray-200 dark:border-gray-700 text-xs text-gray-600 dark:text-gray-500">
          {{ questions.length }} total questions
        </div>
      </div>

      <!-- Detail Panel (Right Panel - 70%) -->
      <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900">
        <div v-if="!selectedQuestion" class="h-full flex items-center justify-center text-gray-500 dark:text-gray-400">
          Select a question to view details
        </div>

        <div v-else class="p-4 space-y-3">
          <!-- Save Indicator -->
          <div v-if="saving" class="fixed top-4 right-4 bg-blue-600 text-white px-3 py-1.5 rounded shadow-lg flex items-center gap-2 text-sm z-50">
            <div class="animate-spin rounded-full h-3 w-3 border-b-2 border-white"></div>
            <span>Saving...</span>
          </div>

          <!-- Question Information -->
          <section class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-3">
            <h3 class="text-base font-semibold mb-2 text-gray-900 dark:text-gray-200">Question Information</h3>
            
            <div class="space-y-2">
              <div class="grid grid-cols-2 gap-2">
                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Question Number</label>
                  <input
                    v-model="selectedQuestion.qstNumber"
                    @blur="saveQuestionField('qstNumber', selectedQuestion.qstNumber)"
                    type="text"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  />
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Data File Column</label>
                  <input
                    v-model.number="selectedQuestion.dataFileCol"
                    disabled
                    type="number"
                    class="w-full px-2 py-1.5 bg-gray-100 dark:bg-gray-900 border border-gray-300 dark:border-gray-600 rounded text-sm text-gray-500"
                  />
                </div>
              </div>

              <div>
                <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Question Label</label>
                <input
                  v-model="selectedQuestion.qstLabel"
                  @blur="saveQuestionField('qstLabel', selectedQuestion.qstLabel)"
                  type="text"
                  class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                />
              </div>

              <div>
                <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Question Stem</label>
                <textarea
                  v-model="selectedQuestion.qstStem"
                  @blur="saveQuestionField('qstStem', selectedQuestion.qstStem)"
                  @input="autoResize($event.target)"
                  ref="qstStemTextarea"
                  rows="2"
                  class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100 resize-none overflow-hidden"
                ></textarea>
              </div>

              <div class="grid grid-cols-2 gap-2">
                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Variable Type</label>
                  <select
                    v-model.number="selectedQuestion.variableType"
                    @change="saveQuestionField('variableType', selectedQuestion.variableType)"
                    class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  >
                    <option :value="0">None</option>
                    <option :value="1">Independent (IV)</option>
                    <option :value="2">Dependent (DV)</option>
                  </select>
                </div>

                <div>
                  <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Data Type</label>
                  <select
                    v-model.number="selectedQuestion.dataType"
                    @change="saveQuestionField('dataType', selectedQuestion.dataType)"
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
                    @blur="saveQuestionField('missValue1', selectedQuestion.missValue1)"
                    type="number"
                    placeholder="Value 1"
                    class="px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  />
                  <input
                    v-model.number="selectedQuestion.missValue2"
                    @blur="saveQuestionField('missValue2', selectedQuestion.missValue2)"
                    type="number"
                    placeholder="Value 2"
                    class="px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  />
                  <input
                    v-model.number="selectedQuestion.missValue3"
                    @blur="saveQuestionField('missValue3', selectedQuestion.missValue3)"
                    type="number"
                    placeholder="Value 3"
                    class="px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                  />
                </div>
              </div>
            </div>
          </section>

          <!-- Block Information (if in block) -->
          <section v-if="selectedQuestion.blockId" class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-3">
            <h3 class="text-base font-semibold mb-2 text-gray-900 dark:text-gray-200">Block Information</h3>
            
            <div class="space-y-2">
              <div>
                <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Block Label</label>
                <input
                  v-model="editingBlockLabel"
                  @blur="saveBlockField('label', editingBlockLabel)"
                  type="text"
                  class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                />
              </div>

              <div>
                <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Block Stem</label>
                <textarea
                  v-model="editingBlockStem"
                  @blur="saveBlockField('stem', editingBlockStem)"
                  rows="2"
                  class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                ></textarea>
                <p class="mt-1 text-xs text-yellow-700 dark:text-yellow-400">
                  ⚠️ Changing this will update all {{ blockQuestionCount }} questions in this block
                </p>
              </div>

              <div>
                <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Block Status</label>
                <select
                  v-model="selectedQuestion.blkQstStatus"
                  @change="saveQuestionField('blkQstStatus', selectedQuestion.blkQstStatus)"
                  class="w-full px-2 py-1.5 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-sm focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                >
                  <option value="First">Block Header</option>
                  <option value="Continuing">Block Member</option>
                </select>
                <p class="mt-1 text-xs text-gray-600 dark:text-gray-500">
                  "Block Header" shows full stem, "Block Member" continues the same block
                </p>
              </div>
            </div>
          </section>

          <!-- Responses Grid -->
          <section class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-3">
            <h3 class="text-base font-semibold mb-2 text-gray-900 dark:text-gray-200">Responses</h3>
            
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-gray-200 dark:border-gray-700">
                    <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-16 text-xs">Value</th>
                    <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium text-xs">Label</th>
                    <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-32 text-xs">Index Type</th>
                    <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-20 text-xs">Weight</th>
                    <th class="text-left py-1.5 px-2 text-gray-700 dark:text-gray-400 font-medium w-16 text-xs">N</th>
                    <th class="w-8"></th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="response in sortedResponses"
                    :key="response.id"
                    class="border-b border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-750"
                  >
                    <td class="py-1.5 px-2 text-gray-600 dark:text-gray-500 text-xs">{{ response.respValue }}</td>
                    <td class="py-1.5 px-2">
                      <input
                        v-model="response.label"
                        @blur="saveResponse(response)"
                        type="text"
                        class="w-full px-2 py-1 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-xs focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                      />
                    </td>
                    <td class="py-1.5 px-2">
                      <select
                        v-model="response.indexType"
                        @change="saveResponse(response)"
                        class="w-full px-2 py-1 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-xs focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                      >
                        <option value="">None</option>
                        <option value="Positive">+ Positive</option>
                        <option value="Negative">- Negative</option>
                        <option value="Neutral">/ Neutral</option>
                      </select>
                    </td>
                    <td class="py-1.5 px-2">
                      <input
                        v-model.number="response.weight"
                        @blur="saveResponse(response)"
                        type="number"
                        step="0.1"
                        class="w-full px-2 py-1 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-xs focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
                      />
                    </td>
                    <td class="py-1.5 px-2 text-gray-700 dark:text-gray-400 text-xs">
                      {{ responseFrequencies[response.respValue] || 0 }}
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
              </div>
              <button
                @click="addNewResponse"
                class="px-2 py-1 bg-blue-600 hover:bg-blue-700 rounded text-white"
              >
                + Add Response
              </button>
            </div>
          </section>
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
const editingBlockLabel = ref('')
const editingBlockStem = ref('')

// Load questions on mount
loadQuestions()

// Watch selected question to update editing fields
watch(selectedQuestion, (newQuestion) => {
  if (newQuestion) {
    editingBlockLabel.value = newQuestion.blkLabel || ''
    editingBlockStem.value = newQuestion.blkStem || ''
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
  
  const onMouseMove = (e) => {
    const delta = ((e.clientX - startX) / window.innerWidth) * 100
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
}

async function saveQuestionField(field, value) {
  try {
    await updateQuestion(selectedQuestion.value.id, { [field]: value })
  } catch (err) {
    console.error('Failed to save question field:', err)
  }
}

async function saveBlockField(field, value) {
  if (!selectedQuestion.value?.blockId) return
  
  try {
    await updateBlock(selectedQuestion.value.blockId, { [field]: value })
  } catch (err) {
    console.error('Failed to save block field:', err)
  }
}

async function saveResponse(response) {
  try {
    await updateResponse(selectedQuestion.value.id, response.id, {
      label: response.label,
      indexType: response.indexType,
      weight: response.weight
    })
  } catch (err) {
    console.error('Failed to save response:', err)
  }
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
