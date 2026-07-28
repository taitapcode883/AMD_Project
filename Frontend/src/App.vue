<template>
  <div
    class="app-layout"
    :class="{ 'sidebar-is-open': sidebarOpen }"
  >
    <Navbar
      v-if="showNavigation"
      :is-open="sidebarOpen"
      @close-sidebar="closeSidebar"
    />

    <main class="app-content">
      <header
        v-if="showNavigation"
        class="page-header"
      >
        <button
          class="menu-button"
          type="button"
          @click="toggleSidebar"
        >
          ☰
        </button>

        <h1 class="page-title">
          PASTE
        </h1>

        <button
          class="header-logout-button"
          type="button"
          @click="logout"
        >
          Logout
        </button>
      </header>

      <router-view />
    </main>
  </div>
</template>

<script>
import Navbar from "./components/Navbar.vue";

export default {
  name: "App",

  components: {
    Navbar
  },

  data() {
    return {
      sidebarOpen: false
    };
  },

  computed: {
    showNavigation() {
      return !["Login", "Register"].includes(this.$route.name);
    }
  },

  methods: {
    toggleSidebar() {
      this.sidebarOpen = !this.sidebarOpen;
    },

    closeSidebar() {
      this.sidebarOpen = false;
    },

    logout() {
      localStorage.removeItem("isLoggedIn");
      localStorage.removeItem("userEmail");

      this.sidebarOpen = false;
      this.$router.push("/login");
    }
  }
};
</script>