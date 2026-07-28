<template>
  <div>

    <aside
      class="sidebar"
      :class="{ 'sidebar-open': isOpen }"
    >
      <div class="sidebar-header">
        <h2 class="sidebar-title">
          Pastin
        </h2>

        <button
          class="sidebar-close"
          type="button"
          @click="$emit('close-sidebar')"
        >
          ×
        </button>
      </div>

      <nav class="sidebar-menu">
        <router-link
          to="/dashboard"
          class="sidebar-link"
          @click="$emit('close-sidebar')"
        >
          Dashboard
        </router-link>

        <router-link
          to="/paste/new"
          class="sidebar-link"
          @click="$emit('close-sidebar')"
        >
          New paste
        </router-link>

        <router-link
          to="/dashboard"
          class="sidebar-link"
          @click="$emit('close-sidebar')"
        >
          My pastes
        </router-link>
      </nav>
    </aside>
  </div>
</template>

<script>
export default {
  name: "Navbar",

  props: {
    isOpen: {
      type: Boolean,
      default: false
    }
  },

  emits: ["close-sidebar"]
};
</script>
