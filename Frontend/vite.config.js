import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
<<<<<<< HEAD
  plugins: [vue()],
})
=======
  plugins: [
    vue(),
    tailwindcss()
  ],

  server: {
    proxy: {
      "/auth": {
        target: "http://localhost:5179",
        changeOrigin: true
      },

      "/pastes": {
        target: "http://localhost:5179",
        changeOrigin: true
      }
    }
  }
});
>>>>>>> 5fc9dc6 (Update frontend and paste service)
