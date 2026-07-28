import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [
    vue(),
    tailwindcss()
  ],
  server: {
    proxy: {
      "/pastes": {
        target: "http://localhost:5179",
        changeOrigin: true
      }
    }
  }
});
