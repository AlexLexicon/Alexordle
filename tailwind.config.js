/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Alexordle.Client.Blazor.Presentation/*.{razor,html}',
    './Alexordle.Client.Blazor.Presentation/Styles/**/*.{razor,html}',
    './Alexordle.Client.Blazor.Presentation/Views/**/*.{razor,html}',
  ],
  theme: {
    extend: {
      
    },
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
}

