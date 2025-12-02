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
  plugins: [],
}
