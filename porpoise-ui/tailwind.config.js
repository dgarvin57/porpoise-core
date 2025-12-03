/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      screens: {
        '3xl': '1450px',
        '4xl': '1900px',
        '5xl': '2500px',
      },
    },
  },
  plugins: [
    function({ addUtilities }) {
      addUtilities({
        '.scrollbar-thin': {
          'scrollbar-width': 'thin',
        },
        '.scrollbar-thin::-webkit-scrollbar': {
          width: '8px',
          height: '8px',
        },
        '.scrollbar-thin::-webkit-scrollbar-track': {
          'border-radius': '4px',
          'background-color': 'rgb(243 244 246)', // gray-100 for light mode
        },
        '.scrollbar-thin::-webkit-scrollbar-thumb': {
          'border-radius': '4px',
          'background-color': 'rgb(209 213 219)', // gray-300 for light mode
        },
        '.scrollbar-thin::-webkit-scrollbar-thumb:hover': {
          'background-color': 'rgb(156 163 175)', // gray-400 on hover
        },
        '.dark .scrollbar-thin::-webkit-scrollbar-track': {
          'background-color': 'rgb(31 41 55)', // gray-800 for dark mode
        },
        '.dark .scrollbar-thin::-webkit-scrollbar-thumb': {
          'background-color': 'rgb(75 85 99)', // gray-600 for dark mode
        },
        '.dark .scrollbar-thin::-webkit-scrollbar-thumb:hover': {
          'background-color': 'rgb(107 114 128)', // gray-500 on hover
        },
      })
    }
  ],
}

