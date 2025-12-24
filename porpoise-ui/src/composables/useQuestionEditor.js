import { ref, computed } from 'vue'
import axios from 'axios'

/**
 * Composable for managing question editing operations
 * Provides state management and API calls for question definition screen
 */
export function useQuestionEditor(surveyId) {
  const questions = ref([])
  const selectedQuestion = ref(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref(null)
  const responseFrequencies = ref({})

  // API base URL
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5050'

  // LocalStorage key for persisting selected question
  const STORAGE_KEY = `porpoise_selected_question_${surveyId}`

  /**
   * Load all questions for the survey with blocks and responses
   */
  async function loadQuestions() {
    loading.value = true
    error.value = null
    try {
      const response = await axios.get(`${apiBaseUrl}/api/surveys/${surveyId}/questions-with-responses`)
      
      // API returns camelCase with proper property names
      questions.value = response.data.map(q => ({
        ...q,
        responses: (q.responses || []).map(r => ({ ...r }))
      }))
      
      // Try to restore previously selected question from localStorage
      const savedQuestionId = localStorage.getItem(STORAGE_KEY)
      if (savedQuestionId && questions.value.length > 0) {
        const savedQuestion = questions.value.find(q => q.id === savedQuestionId)
        if (savedQuestion) {
          selectedQuestion.value = savedQuestion
          await loadResponseFrequencies(savedQuestion.id)
          return
        }
      }
      
      // Auto-select first question if none selected or saved question not found
      if (questions.value.length > 0 && !selectedQuestion.value) {
        selectedQuestion.value = questions.value[0]
        await loadResponseFrequencies(questions.value[0].id)
      }
    } catch (err) {
      error.value = `Failed to load questions: ${err.message}`
      console.error('Error loading questions:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * Select a question and load its response frequencies
   */
  async function selectQuestion(question) {
    selectedQuestion.value = question
    
    // Persist selection to localStorage
    localStorage.setItem(STORAGE_KEY, question.id)
    
    // Load response frequencies for this question
    await loadResponseFrequencies(question.id)
  }

  /**
   * Load frequency counts for a question's responses
   */
  async function loadResponseFrequencies(questionId) {
    try {
      const response = await axios.get(
        `${apiBaseUrl}/api/surveys/${surveyId}/questions/${questionId}/response-frequencies`
      )
      responseFrequencies.value = response.data.frequencies || {}
    } catch (err) {
      console.error('Error loading response frequencies:', err)
      responseFrequencies.value = {}
    }
  }

  /**
   * Update a question's properties
   */
  async function updateQuestion(questionId, updates) {
    saving.value = true
    error.value = null
    try {
      // Find the question to update
      const questionIndex = questions.value.findIndex(q => q.id === questionId)
      if (questionIndex === -1) {
        throw new Error('Question not found')
      }

      const question = questions.value[questionIndex]
      
      // Merge updates but exclude responses array (responses are updated separately)
      const { responses, ...questionWithoutResponses } = question
      const updatedQuestion = { ...questionWithoutResponses, ...updates }

      const response = await axios.put(
        `${apiBaseUrl}/api/surveys/${surveyId}/questions/${questionId}`,
        updatedQuestion
      )

      // Update local state - merge response data with existing responses
      const updatedData = { ...response.data, responses: question.responses }
      questions.value[questionIndex] = updatedData
      if (selectedQuestion.value?.id === questionId) {
        selectedQuestion.value = updatedData
      }

      return response.data
    } catch (err) {
      error.value = `Failed to update question: ${err.message}`
      console.error('Error updating question:', err)
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Update a response's properties
   */
  async function updateResponse(questionId, responseId, updates) {
    saving.value = true
    error.value = null
    try {
      // Find the question and response
      const question = questions.value.find(q => q.id === questionId)
      if (!question) {
        throw new Error('Question not found')
      }

      const responseIndex = question.responses.findIndex(r => r.id === responseId)
      if (responseIndex === -1) {
        throw new Error('Response not found')
      }

      const response = question.responses[responseIndex]
      
      // Only send the fields that the backend UpdateAsync method actually uses
      // See: Porpoise.DataAccess/Repositories/ResponseRepository.cs UpdateAsync
      const updatedResponse = {
        id: responseId,
        respValue: updates.respValue !== undefined ? updates.respValue : response.respValue,
        label: updates.label !== undefined ? updates.label : response.label,
        indexType: updates.indexType !== undefined ? updates.indexType : response.indexType,
        weight: updates.weight !== undefined ? updates.weight : response.weight,
        modifiedDate: new Date().toISOString()
      }

      const result = await axios.put(
        `${apiBaseUrl}/api/surveys/${surveyId}/questions/${questionId}/responses/${responseId}`,
        updatedResponse
      )

      // Update local state
      question.responses[responseIndex] = result.data

      return result.data
    } catch (err) {
      error.value = `Failed to update response: ${err.message}`
      console.error('Error updating response:', err)
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Add a new response to a question
   */
  async function addResponse(questionId, responseData) {
    saving.value = true
    error.value = null
    try {
      const question = questions.value.find(q => q.id === questionId)
      if (!question) {
        throw new Error('Question not found')
      }

      const response = await axios.post(
        `${apiBaseUrl}/api/surveys/${surveyId}/questions/${questionId}/responses`,
        responseData
      )

      // Add to local state
      if (!question.responses) {
        question.responses = []
      }
      question.responses.push(response.data)

      return response.data
    } catch (err) {
      error.value = `Failed to add response: ${err.message}`
      console.error('Error adding response:', err)
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Delete a response from a question
   */
  async function deleteResponse(questionId, responseId) {
    saving.value = true
    error.value = null
    try {
      await axios.delete(
        `${apiBaseUrl}/api/surveys/${surveyId}/questions/${questionId}/responses/${responseId}`
      )

      // Remove from local state
      const question = questions.value.find(q => q.id === questionId)
      if (question) {
        question.responses = question.responses.filter(r => r.id !== responseId)
      }
    } catch (err) {
      error.value = `Failed to delete response: ${err.message}`
      console.error('Error deleting response:', err)
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Update a question block (affects all questions in the block)
   */
  async function updateBlock(blockId, updates) {
    saving.value = true
    error.value = null
    try {
      // Find the block data from first question in that block
      const questionInBlock = questions.value.find(q => q.blockId === blockId)
      if (!questionInBlock) {
        throw new Error('Block not found')
      }

      const blockData = {
        id: blockId,
        surveyId: surveyId,
        label: questionInBlock.blkLabel,
        stem: questionInBlock.blkStem,
        displayOrder: questionInBlock.displayOrder || 0,
        ...updates
      }

      const response = await axios.put(
        `${apiBaseUrl}/api/surveys/${surveyId}/blocks/${blockId}`,
        blockData
      )

      // Update all questions in this block
      questions.value.forEach(q => {
        if (q.blockId === blockId) {
          q.blkLabel = response.data.label
          q.blkStem = response.data.stem
        }
      })

      // Update selected question if it's in this block
      if (selectedQuestion.value?.blockId === blockId) {
        selectedQuestion.value.blkLabel = response.data.label
        selectedQuestion.value.blkStem = response.data.stem
      }

      return response.data
    } catch (err) {
      error.value = `Failed to update block: ${err.message}`
      console.error('Error updating block:', err)
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Group questions by block for display
   * Maintains dataFileCol order with blocks and standalone questions interleaved
   */
  const questionsByBlock = computed(() => {
    // First, sort all questions by dataFileCol to match analytics ordering
    const sortedQuestions = [...questions.value].sort((a, b) => (a.dataFileCol || 0) - (b.dataFileCol || 0))
    
    const result = []
    const blockMap = new Map()
    const processedBlocks = new Set()

    // Process questions in dataFileCol order
    sortedQuestions.forEach(question => {
      if (question.blockId) {
        // This question belongs to a block
        if (!blockMap.has(question.blockId)) {
          // Create new block
          const block = {
            id: question.blockId,
            label: question.blkLabel,
            stem: question.blkStem,
            questions: [],
            minDataFileCol: question.dataFileCol // Track position for ordering
          }
          blockMap.set(question.blockId, block)
        }
        // Add question to block
        blockMap.get(question.blockId).questions.push(question)
      } else {
        // Standalone question - add directly to result with position marker
        result.push({
          ...question,
          isStandalone: true,
          sortOrder: question.dataFileCol
        })
      }
    })

    // Add blocks to result with their position
    blockMap.forEach(block => {
      result.push({
        ...block,
        sortOrder: block.minDataFileCol
      })
    })

    // Sort the final result by position to maintain proper interleaving
    result.sort((a, b) => (a.sortOrder || a.minDataFileCol || 0) - (b.sortOrder || b.minDataFileCol || 0))

    return result
  })

  return {
    questions,
    selectedQuestion,
    loading,
    saving,
    error,
    responseFrequencies,
    questionsByBlock,
    loadQuestions,
    selectQuestion,
    updateQuestion,
    updateResponse,
    addResponse,
    deleteResponse,
    updateBlock,
    loadResponseFrequencies
  }
}
