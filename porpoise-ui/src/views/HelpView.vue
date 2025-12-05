<template>
  <div class="container mx-auto px-6 py-6">
    <div class="max-w-5xl mx-auto">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-6">Help & Support</h1>
      
      <div class="flex gap-6 justify-center">
        <!-- Left Sidebar Navigation -->
        <SidebarNav 
          v-model="activeSection"
          :items="navItems"
        />

        <!-- Vertical Divider -->
        <div class="w-px bg-gray-200 dark:bg-gray-700"></div>

        <!-- Right Content Area -->
        <div class="flex-1 min-w-0">
          <!-- Contact Support Section -->
          <div v-if="activeSection === 'contact'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Contact Support</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Send us a message and we'll respond within 24 hours</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <form @submit.prevent="submitContact">
                <div class="space-y-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Subject</label>
                    <input 
                      v-model="contactForm.subject" 
                      type="text" 
                      required
                      class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="Brief description of your issue"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Category</label>
                    <select 
                      v-model="contactForm.category"
                      class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="technical">Technical Issue</option>
                      <option value="billing">Billing Question</option>
                      <option value="feature">Feature Request</option>
                      <option value="general">General Inquiry</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Message</label>
                    <textarea 
                      v-model="contactForm.message" 
                      rows="8" 
                      required
                      class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="Please provide as much detail as possible..."
                    ></textarea>
                  </div>
                  <div class="flex justify-end pt-2">
                    <button 
                      type="submit" 
                      class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      :disabled="isSubmitting"
                    >
                      {{ isSubmitting ? 'Sending...' : 'Send Message' }}
                    </button>
                  </div>
                </div>
              </form>
            </div>
          </div>

          <!-- Knowledge Base Section -->
          <div v-else-if="activeSection === 'knowledge'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Knowledge Base</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Search articles and guides to find quick answers</p>
            </div>

            <!-- Search -->
            <div class="mb-4">
              <div class="relative">
                <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <input
                  v-model="searchQuery"
                  type="text"
                  placeholder="Search articles..."
                  class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            <!-- Categories -->
            <div class="space-y-3">
              <div v-for="category in knowledgeBase" :key="category.title" class="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden bg-white dark:bg-gray-800">
                <button 
                  @click="toggleCategory(category.title)"
                  class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-700/50 text-left flex items-center justify-between hover:bg-gray-100 dark:hover:bg-gray-700"
                >
                  <div class="flex items-center gap-3">
                    <svg class="w-5 h-5 text-gray-500 dark:text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    <span class="font-medium text-gray-900 dark:text-white">{{ category.title }}</span>
                    <span class="text-sm text-gray-500 dark:text-gray-400">({{ category.articles.length }})</span>
                  </div>
                  <svg 
                    class="w-5 h-5 text-gray-500 transition-transform" 
                    :class="{ 'rotate-180': expandedCategories.includes(category.title) }"
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                <div v-if="expandedCategories.includes(category.title)" class="divide-y divide-gray-200 dark:divide-gray-700">
                  <a 
                    v-for="article in category.articles" 
                    :key="article.title"
                    href="#"
                    class="block px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700/30 group"
                  >
                    <div class="flex items-start justify-between">
                      <div class="flex-1">
                        <div class="text-sm font-medium text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400">
                          {{ article.title }}
                        </div>
                        <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">{{ article.description }}</div>
                      </div>
                      <svg class="w-4 h-4 text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400 flex-shrink-0 ml-2 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                      </svg>
                    </div>
                  </a>
                </div>
              </div>
            </div>
          </div>

          <!-- Video Tutorials Section -->
          <div v-else-if="activeSection === 'videos'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Video Tutorials</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Watch step-by-step guides and tips</p>
            </div>

            <div class="grid grid-cols-1 gap-4">
              <div v-for="video in videos" :key="video.title" class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden hover:border-blue-500 dark:hover:border-blue-500 transition-colors cursor-pointer">
                <div class="flex gap-4 p-4">
                  <div class="w-32 h-20 flex-shrink-0 bg-gray-100 dark:bg-gray-700 rounded flex items-center justify-center">
                    <div class="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center">
                      <svg class="w-5 h-5 text-white ml-0.5" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M8 5v14l11-7z"/>
                      </svg>
                    </div>
                  </div>
                  <div class="flex-1">
                    <h3 class="font-semibold text-gray-900 dark:text-white mb-1">{{ video.title }}</h3>
                    <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">{{ video.description }}</p>
                    <div class="flex items-center gap-3 text-xs text-gray-500 dark:text-gray-400">
                      <span>{{ video.duration }}</span>
                      <span>•</span>
                      <span>{{ video.views }} views</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- FAQ Section -->
          <div v-else-if="activeSection === 'faq'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Frequently Asked Questions</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Quick answers to common questions</p>
            </div>
            
            <div class="space-y-2">
              <div v-for="(faq, index) in faqs" :key="index" class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                <button 
                  @click="toggleFaq(index)"
                  class="w-full px-4 py-3 text-left flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-700/30"
                >
                  <span class="text-sm font-medium text-gray-900 dark:text-white">{{ faq.question }}</span>
                  <svg 
                    class="w-5 h-5 text-gray-500 transition-transform flex-shrink-0 ml-2" 
                    :class="{ 'rotate-180': expandedFaqs.includes(index) }"
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                <div v-if="expandedFaqs.includes(index)" class="px-4 pb-3 text-sm text-gray-600 dark:text-gray-400 border-t border-gray-200 dark:border-gray-700 pt-3">
                  {{ faq.answer }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import SidebarNav from '../components/SidebarNav.vue'

const activeSection = ref('contact')
const isSubmitting = ref(false)
const searchQuery = ref('')
const expandedCategories = ref([])
const expandedFaqs = ref([])

const navItems = [
  { 
    label: 'Contact Support', 
    value: 'contact',
    icon: 'M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z'
  },
  { 
    label: 'Knowledge Base', 
    value: 'knowledge',
    icon: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253'
  },
  { 
    label: 'Video Tutorials', 
    value: 'videos',
    icon: 'M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664zM21 12a9 9 0 11-18 0 9 9 0 0118 0z'
  },
  { 
    label: 'FAQ', 
    value: 'faq',
    icon: 'M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
  }
]

const contactForm = ref({
  subject: '',
  category: 'technical',
  message: ''
})

const knowledgeBase = [
  {
    title: 'Getting Started',
    articles: [
      { title: 'Creating Your First Survey', description: 'Learn how to create and configure a new survey project' },
      { title: 'Importing Existing Data', description: 'Import surveys from other platforms' },
      { title: 'Understanding the Dashboard', description: 'Navigate the main interface and features' },
      { title: 'User Roles and Permissions', description: 'Manage team access and permissions' }
    ]
  },
  {
    title: 'Survey Design',
    articles: [
      { title: 'Question Types and Options', description: 'Explore all available question formats' },
      { title: 'Logic and Branching', description: 'Create conditional survey flows' },
      { title: 'Custom Branding', description: 'Add your logo and customize appearance' },
      { title: 'Multi-language Surveys', description: 'Create surveys in multiple languages' }
    ]
  },
  {
    title: 'Data Analysis',
    articles: [
      { title: 'Generating Reports', description: 'Create and export comprehensive reports' },
      { title: 'Advanced Filtering', description: 'Filter and segment your response data' },
      { title: 'Cross-tabulation', description: 'Analyze relationships between variables' },
      { title: 'Exporting Data', description: 'Export data in various formats' }
    ]
  },
  {
    title: 'Account Management',
    articles: [
      { title: 'Updating Profile Information', description: 'Manage your account details' },
      { title: 'Managing Subscriptions', description: 'Upgrade, downgrade, or cancel your plan' },
      { title: 'Security Settings', description: 'Two-factor authentication and security options' },
      { title: 'Organization Settings', description: 'Configure team and organization preferences' }
    ]
  }
]

const videos = [
  {
    title: 'Getting Started with Pulse Analytics',
    description: 'A complete introduction to the platform and its core features',
    duration: '12:34',
    views: '2.5K'
  },
  {
    title: 'Creating Your First Survey',
    description: 'Step-by-step guide to building and launching a survey',
    duration: '8:45',
    views: '1.8K'
  },
  {
    title: 'Advanced Analytics Features',
    description: 'Deep dive into data analysis and reporting tools',
    duration: '15:20',
    views: '1.2K'
  },
  {
    title: 'Survey Design Best Practices',
    description: 'Tips and tricks for creating effective surveys',
    duration: '10:15',
    views: '950'
  }
]

const faqs = [
  {
    question: 'How do I reset my password?',
    answer: 'Go to Account > Password in the Settings page, or click "Forgot Password" on the login screen. You\'ll receive an email with instructions to reset your password.'
  },
  {
    question: 'What file formats can I import?',
    answer: 'Pulse Analytics supports .porp, .porps, and .porpd files. You can also import CSV files with properly formatted survey data.'
  },
  {
    question: 'How many responses can I collect?',
    answer: 'Response limits depend on your subscription plan. Professional plans include unlimited responses, while starter plans have a monthly limit. Check your account dashboard for current usage.'
  },
  {
    question: 'Can I export my data?',
    answer: 'Yes! You can export your survey data in multiple formats including Excel (.xlsx), CSV, PDF reports, and our native .porp format.'
  },
  {
    question: 'Is my data secure?',
    answer: 'Absolutely. We use industry-standard encryption for data transmission and storage. All data is backed up daily, and we comply with GDPR and other privacy regulations.'
  },
  {
    question: 'How do I add team members?',
    answer: 'Go to Settings > Organization and click "Invite Team Member". Enter their email address and select their role. They\'ll receive an invitation to join your organization.'
  },
  {
    question: 'Can I customize survey branding?',
    answer: 'Yes! Professional and Enterprise plans include custom branding options. Go to Survey Settings to upload your logo and customize colors.'
  },
  {
    question: 'What payment methods do you accept?',
    answer: 'We accept all major credit cards (Visa, MasterCard, American Express) and can arrange invoicing for annual Enterprise subscriptions.'
  }
]

const toggleCategory = (title) => {
  const index = expandedCategories.value.indexOf(title)
  if (index > -1) {
    expandedCategories.value.splice(index, 1)
  } else {
    expandedCategories.value.push(title)
  }
}

const toggleFaq = (index) => {
  const faqIndex = expandedFaqs.value.indexOf(index)
  if (faqIndex > -1) {
    expandedFaqs.value.splice(faqIndex, 1)
  } else {
    expandedFaqs.value.push(index)
  }
}

const submitContact = async () => {
  isSubmitting.value = true
  
  // Simulate API call
  await new Promise(resolve => setTimeout(resolve, 1500))
  
  // Would make actual API call here
  console.log('Contact form submitted:', contactForm.value)
  
  isSubmitting.value = false
  
  // Reset form
  contactForm.value = {
    subject: '',
    category: 'technical',
    message: ''
  }
  
  // Show success message (you could add a toast notification here)
  alert('Thank you! Your message has been sent. We\'ll respond within 24 hours.')
}
</script>
